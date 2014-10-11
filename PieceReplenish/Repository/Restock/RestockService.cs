using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Caching;
using System.Web.Routing;

namespace DcmsMobile.PieceReplenish.Repository.Restock
{
    internal class RestockService : IDisposable
    {
        #region Intialization

        private readonly RestockRepository _repos;

        public RestockService(RequestContext ctx, string userName, string connectString)
        {
            var clientInfo = string.IsNullOrEmpty(ctx.HttpContext.Request.UserHostName) ?
                             ctx.HttpContext.Request.UserHostAddress : ctx.HttpContext.Request.UserHostName;
            _repos = new RestockRepository(userName, clientInfo, ctx.HttpContext.Trace, connectString);
        }

        public void Dispose()
        {
            _repos.Dispose();
        }

        #endregion

        #region Carton Queries
        /// <summary>
        /// Cartons queried more than 5 minutes ago are hidden from this collection
        /// </summary>
        private class CartonCollection : KeyedCollection<string, RestockCarton>
        {
            /// <summary>
            /// We are passing -1 as the dictionaryCreation threshhold to prevent the lookup dictionary from being created.
            /// This is necessary because the key of an expired item changes to null and we do not want looup dictionary related bugs.
            /// </summary>
            public CartonCollection()
                : base(StringComparer.InvariantCultureIgnoreCase, -1)
            {

            }

            protected override string GetKeyForItem(RestockCarton item)
            {
                return item == null ? string.Empty : item.CartonId;
            }

        }

        /// <summary>
        /// After 30 minutes the cache is discarded and a new cache cycle begins
        /// </summary>
        /// <remarks>
        /// When a carton is retrieved, it is added to the cache. When it is restocked, it is removed from the cache
        /// </remarks>
        private CartonCollection CartonCache
        {
            get
            {
                const string KEY = "RestockService_Cartons";
                var cache = MemoryCache.Default[KEY] as CartonCollection;
                if (cache == null)
                {
                    cache = new CartonCollection();
                    MemoryCache.Default.Add(KEY, cache, new CacheItemPolicy
                    {
                        AbsoluteExpiration = DateTime.Now.AddMinutes(30)
                    });
                }
                return cache;
            }
        }

        /// <summary>
        /// Key is sku id.
        /// </summary>
        private ConcurrentDictionary<int, IList<AssignedLocation>> SkuLocationCache
        {
            get
            {
                const string KEY = "RestockService_Sku";
                var cache = MemoryCache.Default[KEY] as ConcurrentDictionary<int, IList<AssignedLocation>>;
                if (cache == null)
                {
                    cache = new ConcurrentDictionary<int, IList<AssignedLocation>>();
                    MemoryCache.Default.Add(KEY, cache, new CacheItemPolicy
                    {
                        AbsoluteExpiration = DateTime.Now.AddMinutes(30)
                    });
                }
                return cache;
            }
        }

        private IList<string> RestockableQualities
        {
            get
            {
                const string KEY = "RestockService_RestockableQuality";
                var cache = MemoryCache.Default[KEY] as IList<string>;
                if (cache == null)
                {
                    cache = _repos.GetOrderQualities();
                    MemoryCache.Default.Add(KEY, cache, new CacheItemPolicy
                    {
                        SlidingExpiration = TimeSpan.FromMinutes(30)
                    });
                }
                return cache;
            }
        }

        /// <summary>
        /// Retreives Scanned Carton details
        /// </summary>
        /// <param name="cartonId">string</param>
        /// <returns>RestockCarton model</returns>
        public RestockCarton GetCartonDetails(string cartonId, bool ignoreCache)
        {
            RestockCarton carton;
            var isInCache = CartonCache.Contains(cartonId);
            if (!ignoreCache && isInCache)
            {
                carton = CartonCache[cartonId];
            }
            else
            {
                carton = _repos.GetCartonDetails(cartonId);
                if (carton != null)
                {
                    if (isInCache)
                    {
                        CartonCache.Remove(carton.CartonId);
                    }
                    CartonCache.Add(carton);
                }
            }
            if (carton == null)
            {
                return null;
            }

            IList<AssignedLocation> assignedLocations;
            if (carton.SkuId.HasValue)
            {
                if (!SkuLocationCache.TryGetValue(carton.SkuId.Value, out assignedLocations))
                {
                    assignedLocations = _repos.GetAssignedLocations(carton.SkuId.Value);

                    SkuLocationCache.TryAdd(carton.SkuId.Value, assignedLocations);

                }
            }
            else
            {
                assignedLocations = new AssignedLocation[0];
            }

            carton.AssignedLocations = assignedLocations.Where(p => p.AssignedVwhId == carton.VwhId).OrderByDescending(p => p.SpaceAvailable).ToList();

            if (carton.AssignedLocations.Count == 0 && carton.SkuId.HasValue)
            {
                SkuLocationCache.TryRemove(carton.SkuId.Value, out assignedLocations);
            }
            carton.RestockableQualities = RestockableQualities;
            return carton;
        }

        /// <summary>
        /// Restocks carton
        /// </summary>
        /// <param name="carton">RestockCarton entity</param>
        /// <param name="locationId">string</param>
        public void RestockCarton(RestockCarton carton, string locationId)
        {
            if (carton == null || string.IsNullOrWhiteSpace(carton.CartonId))
            {
                throw new ArgumentNullException("carton");
            }

            if (string.IsNullOrWhiteSpace(locationId))
            {
                throw new ArgumentNullException("locationId");
            }
            using (var trans = _repos.BeginTransaction())
            {
                _repos.RestockCarton(carton, locationId);
                _repos.CaptureProductivity(carton, true);
                trans.Commit();
            }
            CartonCache.Remove(carton.CartonId);

            // Remove assigned locations of SKU from the cache so that we can see updated pieces at location
            if (carton.SkuId.HasValue)
            {
                // Safety check
                IList<AssignedLocation> dummy;
                SkuLocationCache.TryRemove(carton.SkuId.Value, out dummy);
            }
        }

        /// <summary>
        /// Puts carton in suspense
        /// </summary>
        /// <param name="carton">String</param>
        public void SuspenseCarton(RestockCarton carton)
        {
            if (carton == null || string.IsNullOrWhiteSpace(carton.CartonId))
            {
                throw new ArgumentNullException("carton");
            }
            using (var trans = _repos.BeginTransaction())
            {
                _repos.SuspenseCarton(carton.CartonId);
                _repos.CaptureProductivity(carton, false);
                trans.Commit();
            }
            CartonCache.Remove(carton.CartonId);
        }
        #endregion

        /// <summary>
        /// This cache maps a bar code to SKU id. This this information is almost static, we are using sliding expiration.
        /// </summary>
        private ConcurrentDictionary<string, int> BarCodeSkuIdCache
        {
            get
            {
                const string KEY = "RestockService_BarCode";
                var cache = MemoryCache.Default[KEY] as ConcurrentDictionary<string, int>;
                if (cache == null)
                {
                    cache = new ConcurrentDictionary<string, int>();
                    MemoryCache.Default.Add(KEY, cache, new CacheItemPolicy
                    {
                        SlidingExpiration = TimeSpan.FromMinutes(30)
                    });
                }
                return cache;
            }
        }

        /// <summary>
        /// Returns null if the bar code is  invalid
        /// </summary>
        /// <param name="barCode"></param>
        /// <returns></returns>
        internal int? GetSkuId(string barCode)
        {
            int skuId;
            if (!BarCodeSkuIdCache.TryGetValue(barCode, out skuId))
            {
                var id = _repos.GetSkuIdFromBarCode(barCode);
                if (id == null)
                {
                    return null;
                }
                BarCodeSkuIdCache.TryAdd(barCode, id.Value);
                skuId = id.Value;
            }
            return skuId;
        }

    }
}