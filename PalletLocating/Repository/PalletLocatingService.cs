using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration.Provider;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Caching;
using System.Web;
using System.Web.Routing;
using DcmsMobile.PalletLocating.Models;

namespace DcmsMobile.PalletLocating.Repository
{
    /// <summary>
    /// Defines the state of the proposed location if the proposed pallet is allowed to get located there
    /// </summary>
    [Flags]
    public enum LocationStates
    {
        /// <summary>
        /// The location will be in pristine state
        /// </summary>
        Sucess = 0x0,

        /// <summary>
        /// The location will contain an SKU which is not assigned ther
        /// </summary>
        AssignmentViolation = 0x1,

        /// <summary>
        /// The location will contain more cartons than are permissible
        /// </summary>
        CapacityViolation = 0x2,

        /// <summary>
        /// The location will contain multiple SKUs which may not be desirable
        /// </summary>
        MultipleSkuAtLocation = 0x4,

        /// <summary>
        /// The location is unavailable becoz unavailable_flag is set to 'Y'
        /// </summary>
        UnavailableLocation = 0x8,

        /// <summary>
        /// The location having VWh assignment which is not contained by cartons on pallet,
        /// or pallet having cartons of multiple VWh
        /// </summary>
        VwhAssignmentViolation = 0x10
    }

    /// <summary>
    /// Defines the problems that may happen if the proposed pallet is allowed to get located at the proposed location
    /// </summary>
    public class LocatingResult
    {
        public LocationStates State { get; set; }

        public CartonLocation Location { get; set; }

        public Pallet Pallet { get; set; }
    }


    public class PalletLocatingService : IDisposable
    {

        public int QueryCount
        {
            get
            {
                return _repos.QueryCount;
            }

        }
        /// <summary>
        /// Since area info is needed all the time, we cache it
        /// </summary>
        /// <remarks>
        /// The function <see cref="GetCartonAreas"/> populates the cache. <see cref="GetCartonArea"/> uses the cache.
        /// The cache uses sliding expiration. Stale values are removed from the cache when any stale value is accessed.
        /// </remarks>
        private const string APPKEY_AREAINFO = "PalletLocatingService_AreaInfo";

        /// <summary>
        /// Pallet info is cached for a brief time frame. After the user scans a pallet, pallet info is retrieved and cached.
        /// When the user scans the location, this info is needed again and is retrieved from the cache. When the pallet is located, the
        /// entry in the cache is removed so that the next scan will retrieve updated information. Pallet info contains the queried time stamp.
        /// This time stamp is used to discard values older than a few minutes.
        /// </summary>
        private const string APPKEY_PALLET = "PalletLocatingService_Pallet";

        /// <summary>
        /// Replenishment suggestions once retrived are cached. Whenever a pallet is located, the suggestions are discarded.
        /// </summary>
        private const string APPKEY_REPLENISHMENT_SUGGESTIONS = "PalletLocatingService_Suggestions";

        #region Intialization
        private readonly PalletLocatingRepository _repos;
        private readonly TraceContext _traceContext;

        public PalletLocatingService(RequestContext ctx)
        {
            var clientInfo = string.IsNullOrEmpty(ctx.HttpContext.Request.UserHostName) ?
                             ctx.HttpContext.Request.UserHostAddress : ctx.HttpContext.Request.UserHostName;

            _traceContext = ctx.HttpContext.Trace;
            _repos = new PalletLocatingRepository(ctx.HttpContext.User.Identity.Name, clientInfo, _traceContext);

        }

        public void Dispose()
        {
            _repos.Dispose();
        }

        #endregion


        #region Carton Areas
        private ConcurrentDictionary<string, Area> AreaCache
        {
            get
            {
                var cache = MemoryCache.Default[APPKEY_AREAINFO] as ConcurrentDictionary<string, Area>;
                if (cache == null)
                {
                    cache = new ConcurrentDictionary<string, Area>();
                    // Caching for a relatively short time so that changes to database can become visible soon.
                    MemoryCache.Default.Add(APPKEY_AREAINFO, cache, new CacheItemPolicy
                    {
                        AbsoluteExpiration = DateTime.Now.AddMinutes(15)
                    });
                }
                return cache;
            }
        }

        /// <summary>
        /// Returns potential destination areas for the passed building
        /// </summary>
        /// <param name="buildingId">Pass null to get areas of all buildings</param>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// Returns for all numbered pallet areas within the passed building.
        /// </para>
        /// </remarks>
        public IEnumerable<Area> GetCartonAreas(string buildingId)
        {
            var areas = _repos.GetCartonAreas(null, buildingId, null, null);

            var cache = this.AreaCache;

            // This is a good time to refresh the cache
            foreach (var area in areas)
            {
                Area dummy;
                cache.TryRemove(area.AreaId, out dummy);
                cache.TryAdd(area.AreaId, area);
            }
            return areas;
        }

        public Area GetCartonArea(string buildingId, string shortName)
        {
            var cache = AreaCache;

            // Linear search the dictionary
            var item = cache.FirstOrDefault(p => p.Value.ShortName == shortName &&
                (p.Value.BuildingId == buildingId || string.IsNullOrEmpty(p.Value.BuildingId)));

            Area area;
            if (string.IsNullOrEmpty(item.Key))
            {
                // Area not found in cache
                area = _repos.GetCartonAreas(null, buildingId, shortName, null).FirstOrDefault();
                if (area != null)
                {
                    cache.TryAdd(area.AreaId, area);
                }
            }
            else
            {
                _traceContext.Write(string.Format("Query for area of Building {0} and Short Name {1} avoided due to the cache", buildingId, shortName));
                area = item.Value;
            }
            return area;
        }

        /// <summary>
        /// Returns destination areas info.
        /// </summary>
        /// <param name="areaId"></param>
        /// <returns></returns>
        public Area GetCartonArea(string areaId)
        {
            var cache = AreaCache;

            Area area;
            if (cache.TryGetValue(areaId, out area))
            {
                _traceContext.Write(string.Format("Query for area {0} avoided due to the cache", areaId));
            }
            else
            {
                area = _repos.GetCartonAreas(areaId, null, null, null).FirstOrDefault();
                if (area != null)
                {
                    cache.TryAdd(areaId, area);
                }
            }
            return area;
        }
        #endregion

        /// <summary>
        /// For the passed area, returns info about locations to which the passed SKU has been assigned. Additionally it returns
        /// the space available at each location.
        /// </summary>
        /// <param name="desiredAreaId"></param>
        /// <param name="palletId"></param>
        /// <returns></returns>
        /// <remarks>
        /// For multiple SKU pallets, only empty unassigned locations are suggested
        /// </remarks>
        public IEnumerable<CartonLocation> SuggestLocationsForPallet(string desiredAreaId, string palletId)
        {
            const int MAX_LOCATIONS = 20;

            var area = GetCartonArea(desiredAreaId);
            if (area == null)
            {
                throw new ProviderException("No such area, enter the valid destination area.");
            }
            var pallet = GetPallet(palletId);
            if (pallet == null)
            {
                throw new ProviderException("Pallet does not exist.");
            }
            var countSku = pallet.SkuCount;
            if (countSku == 0)
            {
                throw new ProviderException("Not expecting an empty pallet");
            }

            if (pallet.AreaCount > 1)
            {
                throw new ProviderException(string.Format("Pallet {0} contains {1} cartons of {2} different areas. Therefore can not suggest location.", palletId, pallet.CartonCount, pallet.AreaCount));
            }

            // The area which will be proposed
            string destAreaId;

            // The locations which will be proposed
            IEnumerable<CartonLocation> destLocations;

            // SKU on the pallet. Null if the pallet has multiple SKUs.
            int? skuId = countSku == 1 ? pallet.PalletSku.SkuId : (int?)null;


            if (string.IsNullOrEmpty(area.ReplenishAreaId) || pallet.PalletArea.AreaId == area.ReplenishAreaId)
            {
                // No replenishment area. Pallet will always be located in the passed area
                // This will be the case if the user passes CFD as the destination area.
                destAreaId = area.AreaId;
                destLocations = null;
            }
            else if (countSku > 1)
            {
                // For multi SKU pallets, dest area is always the replenishment area, if it exists.
                // This means that we will never locate multi SKU pallets in CPK.
                // Since Maidenform is SKU sorting pallets, this should never happen.
                Debug.Assert(!string.IsNullOrEmpty(area.ReplenishAreaId));
                destAreaId = area.ReplenishAreaId;
                destLocations = null;
            }
            else
            {
                // Single SKU pallet with replenishment area. Our common case.
                Debug.Assert(skuId.HasValue);
                Debug.Assert(!string.IsNullOrEmpty(area.ReplenishAreaId));

                // First check whether CFD contains this SKU already.
                var replenishLocations = _repos.GetLocationsForSku(area.ReplenishAreaId, skuId, pallet.CartonCount, MAX_LOCATIONS, pallet.CartonVwhId);
                if (replenishLocations.Any(p => p.CartonCount > 0))
                {
                    // SKU exists in replenishment area. We must locate there so that FIFO is honored.
                    destAreaId = area.ReplenishAreaId;
                    destLocations = replenishLocations;
                }
                else
                {
                    // This SKU does not exist in replenishment area. See whether we can locate it in the target area
                    var targetLocations = _repos.GetLocationsForSku(area.AreaId, skuId, pallet.CartonCount, MAX_LOCATIONS, pallet.CartonVwhId);
                    if (targetLocations.Any(p => p.AssignedSku == null || p.AssignedSku.SkuId == skuId))
                    {
                        // Pallet can be located in target area
                        destAreaId = area.AreaId;
                        destLocations = targetLocations;
                    }
                    else
                    {
                        // Pallet must be located in replenishment area
                        destAreaId = area.ReplenishAreaId;
                        destLocations = replenishLocations;
                    }
                }
            }

            if (destLocations == null)
            {
                destLocations = _repos.GetLocationsForSku(destAreaId, skuId, pallet.CartonCount, MAX_LOCATIONS, pallet.CartonVwhId);
            }
            return destLocations;
        }

        #region Pallet
        private ConcurrentDictionary<string, Pallet> PalletCache
        {
            get
            {
                var cache = MemoryCache.Default[APPKEY_PALLET] as ConcurrentDictionary<string, Pallet>;
                if (cache == null)
                {
                    cache = new ConcurrentDictionary<string, Pallet>();
                    // We discard pallets individually, so the cache time is sliding
                    MemoryCache.Default.Add(APPKEY_PALLET, cache, new CacheItemPolicy
                    {
                        SlidingExpiration = TimeSpan.FromMinutes(20)
                    });
                }
                return cache;
            }
        }

        /// <summary>
        /// This call cannot take advantage of the cached Pallet info
        /// </summary>
        /// <param name="cartonId"></param>
        /// <returns></returns>
        public Pallet GetPalletFromCartonId(string cartonId)
        {
            var pallet = _repos.GetPallet(null, cartonId);
            if (pallet != null)
            {
                // Update the cache with the latest value
                PalletCache.AddOrUpdate(pallet.PalletId, pallet, (key, oldValue) => pallet);
            }
            return pallet;
        }

        /// <summary>
        /// Returns the info of Pallet including All skus and Header info.
        /// </summary>
        /// <param name="palletId"></param>
        /// <returns></returns>
        public Pallet GetPallet(string palletId)
        {
            var cache = PalletCache;

            Pallet pallet;
            var queryNeeded = true;     // Assume we need to query
            if (cache.TryGetValue(palletId, out pallet))
            {
                // Make sure that the value found in the cache is not too stale
                if (DateTime.Now - pallet.QueryTime > TimeSpan.FromMinutes(10))
                {
                    // If pallet info is more than 10 min old, we use this opportunity to remove all stale values
                    var keysToRemove = cache.Where(p => DateTime.Now - p.Value.QueryTime > TimeSpan.FromMinutes(10)).Select(p => p.Key).ToArray();
                    foreach (var key in keysToRemove)
                    {
                        cache.TryRemove(key, out pallet);
                    }
                }
                else
                {
                    queryNeeded = false;
                }
            }
            if (queryNeeded)
            {
                pallet = _repos.GetPallet(palletId, null);
                if (pallet != null)
                {
                    cache.TryAdd(palletId, pallet);
                }
            }
            else
            {
                _traceContext.Write(string.Format("Query for Pallet {0} avoided due to the cache", palletId));
            }
            return pallet;
        }

        /// <summary>
        /// This service required LocationId and PalletId only to locate the pallet.
        /// The areaId is self extracted from the locationId
        /// </summary>
        /// <param name="locationId">Where the pallet will be located</param>
        /// <param name="palletId">Pallet which will be merged</param>
        /// <param name="areaId"></param>
        /// <param name="mergeOnPallet">Pallet on which second pallet will be merged</param>
        public void LocateandMergePallet(string locationId, string palletId, string areaId, string mergeOnPallet)
        {
            // First remove pallet from cache so that the next get pallet retrieves updated info
            Pallet pallet;
            PalletCache.TryRemove(palletId, out pallet);
            _repos.LocatePallet(locationId, palletId, areaId, mergeOnPallet);

            if (!string.IsNullOrEmpty(areaId))
            {
                // Since we have located a pallet in this area, the suggestions for this area are no longer valid
                IEnumerable<ReplenishmentSuggestion> suggestions;
                ReplenishmentSuggestionCache.TryRemove(areaId, out suggestions);
            }
        }
        #endregion

        public Carton GetCarton(string cartonId)
        {
            return _repos.GetCarton(cartonId);
        }

        /// <summary>
        /// Validation done for palletizing an orphan carton. 
        /// DB: Think if we can avoid some of these these validations??
        /// Shouldn't we allow a carton to be placed on a pallet even if it is from other area?
        /// </summary>
        /// <param name="cartonId"></param>
        /// <param name="pallet"></param>
        /// <returns></returns>
        public Pallet PalletizeCarton(string cartonId, Pallet pallet)
        {
            if (pallet.SkuCount > 1)
            {
                throw new ProviderException(string.Format("Pallet {0} contains {1} different SKUs, Can't Palletize carton {2}", pallet.PalletId, pallet.SkuCount, cartonId));
            }
            if (pallet.AreaCount > 1)
            {
                throw new ProviderException(string.Format("Pallet {0} contains cartons of {1} different Areas, Can't Palletize carton {2}", pallet.PalletId, pallet.AreaCount, cartonId));
            }
            if (pallet.CartonVwhCount > 1)
            {
                throw new ProviderException(string.Format("Pallet {0} contains cartons of {1} different VWh, Can't Palletize carton {2}", pallet.PalletId, pallet.CartonVwhCount, cartonId));
            }
            if (pallet.CartonQualityCount > 1)
            {
                throw new ProviderException(string.Format("Pallet {0} contains cartons of {1} different Qualities, Can't Palletize carton {2}", pallet.PalletId, pallet.CartonQualityCount, cartonId));
            }
            var carton = GetCarton(cartonId);
            if (carton == null || carton.Sku == null)
            {
                throw new ProviderException(string.Format("Invalid carton {0} passed.", cartonId));
            }
            if (pallet.PalletSku != null && pallet.PalletSku.SkuId != carton.Sku.SkuId)
            {
                throw new ProviderException(string.Format("Pallet {0} contains SKU {1} but carton having SKU {2}, Can't Palletize carton {3}", pallet.PalletId, pallet.PalletSku, carton.Sku, carton.CartonId));
            }
            if (pallet.CartonVwhId != carton.VwhId)
            {
                throw new ProviderException(string.Format("Pallet {0} contains SKU of VWh {1} but carton having SKU of VWh {2}, Can't Palletize carton {3}", pallet.PalletId, pallet.CartonVwhId, carton.VwhId, carton.CartonId));
            }
            if (pallet.PalletArea.AreaId != carton.Area.AreaId)
            {
                throw new ProviderException(string.Format("Cartons on the pallet {0} belongs to area {1} but carton {2} belong to area {3}, Can't Palletize carton", pallet.PalletId, pallet.PalletArea.ShortName, carton.CartonId, carton.Area.ShortName));
            }
            if (pallet.CartonQuality != carton.QualityCode)
            {
                throw new ProviderException(string.Format("Cartons on the pallet {0} having quality {1} but carton {2} having qality {3}, Can't Palletize carton", pallet.PalletId, pallet.CartonQuality, carton.CartonId, carton.QualityCode));
            }
            _repos.PalletizeCarton(pallet.PalletId, cartonId);
            return _repos.GetPallet(pallet.PalletId, null);
        }

        /// <summary>
        /// Defines what will happen if the passed pallet is located at the passed location
        /// </summary>
        /// <param name="palletId"></param>
        /// <param name="locationId"></param>
        /// <returns></returns>
        /// <remarks>
        /// If the pallet contains multiple SKUs, the state can never be success. It will be one or more of MultipleSkuAtLocation and AssignmentViolation.
        /// Additionally, other states are possible too.
        /// </remarks>
        /// <remarks>
        /// Location having assigned VWh which should not be violated, 
        /// if location is Empty or unassigned then allow the pallet which having cartons from multiple VWh
        /// </remarks>
        public LocatingResult QualifyLocationForPallet(string palletId, string locationId)
        {
            var result = new LocatingResult();
            result.Pallet = this.GetPallet(palletId);
            if (result.Pallet == null)
            {
                throw new ProviderException(string.Format("Pallet {0} does not exist", palletId));
            }

            Debug.Assert(result.Pallet.PalletSku != null, "Not expecting empty pallets");

            result.Location = _repos.GetLocation(locationId);
            if (result.Location == null)
            {
                throw new ProviderException(string.Format("Location {0} does not exist", locationId));
            }

            // Now both pallet and location are valid


            // Assigned location. The assigned SKU must match the pallet SKU
            if (result.Location.AssignedSku != null && (result.Pallet.SkuCount > 1 || result.Location.AssignedSku.SkuId != result.Pallet.PalletSku.SkuId))
            {
                result.State |= LocationStates.AssignmentViolation;
            }

            if (result.Pallet.SkuCount > 1 || (result.Location.CartonSku != null && result.Location.SkuCount == 1 && result.Location.CartonSku.SkuId != result.Pallet.PalletSku.SkuId))
            {
                result.State |= LocationStates.MultipleSkuAtLocation;
            }
            if (result.Location.MaxCartons != null && result.Location.CartonCount + result.Pallet.CartonCount > result.Location.MaxCartons)
            {
                result.State |= LocationStates.CapacityViolation;
            }

            if (result.Location.UnavailableFlag)
            {
                result.State |= LocationStates.UnavailableLocation;
            }
            //location having assigned VWh which should not be violated, 
            //if location is Empty or unassigned then allow the pallet which having cartons from multiple VWh
            if (!string.IsNullOrEmpty(result.Location.AssignedVWhId) && result.Location.AssignedVWhId != result.Pallet.CartonVwhId)
            {
                result.State |= LocationStates.VwhAssignmentViolation;
            }
            return result;
        }

        #region Replenishment Suggestions
        /// <summary>
        /// Will never return NULL
        /// </summary>
        private ConcurrentDictionary<string, IEnumerable<ReplenishmentSuggestion>> ReplenishmentSuggestionCache
        {
            get
            {
                var cache = MemoryCache.Default[APPKEY_REPLENISHMENT_SUGGESTIONS] as ConcurrentDictionary<string, IEnumerable<ReplenishmentSuggestion>>;
                if (cache == null)
                {
                    cache = new ConcurrentDictionary<string, IEnumerable<ReplenishmentSuggestion>>();
                    // We discard pallets indivvidually, so the cache time is sliding
                    MemoryCache.Default.Add(APPKEY_REPLENISHMENT_SUGGESTIONS, cache, new CacheItemPolicy
                    {
                        AbsoluteExpiration = DateTime.Now.AddMinutes(15)
                    });
                }
                return cache;
            }
        }

        /// <summary>
        /// If there is no replenishment area, there can be no suggestions
        /// </summary>
        /// <param name="buildingId"> </param>
        /// <param name="areaId"></param>
        /// <param name="replenishAreaId"></param>
        /// <param name="maxSuggestions">Maximum suggestios to retrieve</param>
        /// <param name="useCache">Whether it is OK to display the suggestions from the cache</param>
        /// <returns></returns>
        public IEnumerable<ReplenishmentSuggestion> GetReplenishmentSuggestions(string buildingId, string areaId, string replenishAreaId,
            int maxSuggestions, bool useCache)
        {
            if (string.IsNullOrEmpty(replenishAreaId))
            {
                return Enumerable.Empty<ReplenishmentSuggestion>();
            }

            // Sanity check. We will always display at least 10 suggestions
            maxSuggestions = Math.Max(10, maxSuggestions);

            IEnumerable<ReplenishmentSuggestion> suggestions;
            if (!useCache || !ReplenishmentSuggestionCache.TryGetValue(areaId, out suggestions))
            {
                suggestions = _repos.GetReplenishmentSuggestions(buildingId, replenishAreaId, areaId, maxSuggestions);
                //Trying to expire the cache for getting updated suggestion
                MemoryCache.Default.Remove(APPKEY_REPLENISHMENT_SUGGESTIONS);
                ReplenishmentSuggestionCache.TryAdd(areaId, suggestions);
            }
            return suggestions;
        }
        #endregion

        public bool ValidateBuilding(string buildingId)
        {
            return _repos.ValidateBuilding(buildingId);
        }

        /// <summary>
        ///  Give the info of Pallets located in last few hours.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<PalletMovement> GetPalletMovements(string userName, DateTime? insertToDate, DateTime? insertFromDate)
        {
            return _repos.GetPalletMovements(userName, insertToDate, insertFromDate);
        }

        /// <summary>
        /// Method is used to deduce the area from the passed location id
        /// </summary>
        /// <param name="locationId"></param>
        /// <returns>
        /// returns the Area model with value of all properties i.e. area id, short name, ReplenishmentAreaId, ReplenishAreaShortName, 
        /// IsAreaNumbered and BuildingId
        /// </returns>
        public Area GetAreaFromLocationId(string locationId)
        {
            if (string.IsNullOrEmpty(locationId))
            {
                throw new ProviderException("Location ID is required to deduce the area.");
            }
            return _repos.GetCartonAreas(null, null, null, locationId).FirstOrDefault();
        }

    }
}


/*
    $Id$ 
    $Revision$
    $URL$
    $Header$
    $Author$
    $Date$
*/