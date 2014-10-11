using DcmsMobile.CartonManager.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Caching;
using System.Web;
using System.Web.Routing;

namespace DcmsMobile.CartonManager.Repository.Locating
{

    public enum ScanType
    {
        LocationId,
        CartonId
    }
    /// <summary>
    /// Sounds to confirm scans. Used by home controller.
    /// </summary>
    public enum Sound
    {
        /// <summary>
        /// Error Sound when something catastrophic happens
        /// </summary>
        Error = 'E',

        /// <summary>
        /// When something unusual happens
        /// </summary>
        Warning = 'W',

        /// <summary>
        /// Nothing happened.
        /// </summary>
        None = '\0'
    }
    public class LocatingService : IDisposable
    {

        private readonly LocatingRepository _repos;
        private readonly TraceContext _trace;

        public LocatingService(RequestContext ctx)
        {
            string module = "Locating";
            var clientInfo = string.IsNullOrEmpty(ctx.HttpContext.Request.UserHostName) ?
                             ctx.HttpContext.Request.UserHostAddress : ctx.HttpContext.Request.UserHostName;

            _repos = new LocatingRepository(ctx.HttpContext.User.Identity.Name, module, clientInfo, ctx.HttpContext.Trace);
            _trace = ctx.HttpContext.Trace;
        }

        public void Dispose()
        {
            _repos.Dispose();
        }

        private ConcurrentDictionary<string, HashSet<Carton>> CachedPallets
        {
            get
            {
                const string CACHE_PALLETS = "Locating_Pallets";
                var pallets = MemoryCache.Default[CACHE_PALLETS] as ConcurrentDictionary<string, HashSet<Carton>>;
                if (pallets == null)
                {
                    pallets = new ConcurrentDictionary<string, HashSet<Carton>>();
                    MemoryCache.Default.Add(CACHE_PALLETS, pallets, DateTimeOffset.UtcNow + TimeSpan.FromMinutes(30));
                }
                return pallets;
            }
        }

        /// <summary>
        /// Get cartons of pallet
        /// </summary>
        /// <param name="palletId"></param>
        /// <returns></returns>
        public IList<Carton> GetCartonsOfPallet(string palletId)
        {
            if (string.IsNullOrWhiteSpace(palletId))
            {
                throw new ArgumentNullException("palletId");
            }
            var pallets = CachedPallets;
            HashSet<Carton> cartons;

            if (!pallets.TryGetValue(palletId, out cartons))
            {
                cartons = new HashSet<Carton>(_repos.GetCartonsOfPallet(palletId, null));
                pallets.TryAdd(palletId, cartons);
            }
            else
            {
                _trace.Write(string.Format("Cache Hit. Carton count of Pallet {0} is {1}", palletId, cartons.Count));
            }
            return cartons.ToArray();
        }

        /// <summary>
        /// Get needed information of passed cartons.
        /// </summary>
        /// <param name="cartonList"></param>
        /// <returns></returns>
        public IEnumerable<Carton> GetCartonsDetails(IList<string> cartonList)
        {
            if (cartonList == null)
            {
                throw new ArgumentNullException("cartonId");
            }
            return _repos.GetCartonsOfPallet(null, cartonList);
        }

        /// <summary>
        /// Returns ScanType. 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        internal ScanType GetScanType(string id)
        {
            // Is this a carton on any cached pallet ?           
            if (CachedPallets.Any(p => p.Value.Any(q => q.CartonId.Contains(id))))
            {
                return ScanType.CartonId;
            }
            // Is this a location ?
            if (GetLocation(id) != null)
            {
                return ScanType.LocationId;
            }

            // Assume carton
            return ScanType.CartonId;
        }

        private const string CACHE_LOCATIONS = "Locating_Locations";

        private class LocationCollection : KeyedCollection<string, Location>
        {
            protected override string GetKeyForItem(Location item)
            {
                return item.LocationId;
            }
        }

        /// <summary>
        /// Maintains a cache of location static properties
        /// </summary>
        /// <param name="locationId"></param>
        /// <returns></returns>
        internal Location GetLocation(string locationId)
        {
            var locations = MemoryCache.Default[CACHE_LOCATIONS] as LocationCollection;
            if (locations == null)
            {
                locations = new LocationCollection();
                // Cache for 30 minutes
                MemoryCache.Default.Add(CACHE_LOCATIONS, locations, DateTimeOffset.UtcNow + TimeSpan.FromMinutes(30));
            }
            Location loc;
            lock (locations)
            {
                if (locations.Contains(locationId))
                {
                    // Cache hit. Saves a query
                    loc = locations[locationId];
                    _trace.Write(string.Format("Cache Hit. Location {0} found in cache", locationId));
                }
                else
                {
                    loc = _repos.GetLocation(locationId);
                    if (loc != null)
                    {
                        locations.Add(loc);
                    }
                }
            }
            return loc;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cartonList"></param>
        /// <param name="destBuildingId"></param>
        /// <param name="destAreaId"></param>
        /// <param name="destLocationId"></param>
        /// <param name="locationTravelSequence"></param>
        /// <param name="sourcePalletId"></param>
        /// <returns>List of successfully located cartons</returns>
        public IList<string> LocateCartons(IEnumerable<string> cartonList, string destBuildingId, string destAreaId, string destLocationId, int? locationTravelSequence, string sourcePalletId)
        {
            var list = cartonList.Where(cartonId => DoLocateCarton(cartonId, destBuildingId, destAreaId, destLocationId, locationTravelSequence, sourcePalletId))
                .ToList();
            return list;
        }

        /// <summary>
        /// Locates the passed carton to the passed destination area. 
        /// Removes the carton from the cached pallet. 
        /// Increments the number of cartons at location
        /// </summary>
        /// <param name="cartonId"></param>
        /// <param name="destBuildingId"></param>
        /// <param name="destAreaId"></param>
        /// <param name="destLocationId"></param>
        /// <param name="locationTravelSequence"></param>
        /// <param name="sourcePalletId"></param>
        public bool DoLocateCarton(string cartonId, string destBuildingId, string destAreaId, string destLocationId, int? locationTravelSequence, string sourcePalletId)
        {
            var result = _repos.LocateCarton(cartonId, destBuildingId, destAreaId, destLocationId, locationTravelSequence, sourcePalletId);
            if (result.Item2)
            {
                //Returns false if invalid carton passed.
                return false;
            }

            var locations = MemoryCache.Default[CACHE_LOCATIONS] as LocationCollection;
            if (locations != null && locations.Contains(destLocationId))
            {
                // Increment the number of cartons at location
                lock (locations)
                {
                    if (result.Item1 != destLocationId)
                    {
                        locations[destLocationId].CountCartons += 1;
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(sourcePalletId))
            {
                var pallets = CachedPallets;

                HashSet<Carton> cartons;
                if (pallets.TryGetValue(sourcePalletId, out cartons))
                {
                    // Carton is no longer on the pallet
                    lock (cartons)
                    {
                        cartons.RemoveWhere(p => p.CartonId == cartonId);
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="palletId"></param>
        /// <returns></returns>       
        public void MarkCartonInSuspense(string palletId)
        {
            if (string.IsNullOrWhiteSpace(palletId))
            {
                throw new ArgumentNullException("palletId");
            }
            _repos.MarkCartonInSuspense(palletId);
        }
    }
}