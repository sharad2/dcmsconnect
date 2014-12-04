using System;
using System.Collections.Concurrent;
using System.Runtime.Caching;

namespace DcmsMobile.DcmsLite.Repository.Validation
{
    public class ValidationService : DcmsLiteServiceBase<ValidationRepository>
    {
        private const string APPKEY_AREAINFO = "DcmsLiteService_AreaInfo";
        private const string APPKEY_POSTVERIFICATIONAREA = "$POSTVERIFICATIONAREA";
        private const string APPKEY_BADVERIFICATIONAREA = "$BADVERIFICATIONAREA";

        /// <summary>
        /// Cache to store Post verification and Bad verification areas name
        /// </summary>
        private ConcurrentDictionary<string, string> AreaCache
        {
            get
            {
                var cache = MemoryCache.Default[APPKEY_AREAINFO] as ConcurrentDictionary<string, string>;
                if (cache == null)
                {
                    cache = new ConcurrentDictionary<string, string>();
                    // Caching for an hour
                    MemoryCache.Default.Add(APPKEY_AREAINFO, cache, new CacheItemPolicy
                    {
                        AbsoluteExpiration = DateTime.Now.AddMinutes(60)
                    });
                }
                return cache;
            }
        }

        /// <summary>
        /// Returns the area where validated boxes will be send after successful validation
        /// </summary>
        internal string GetPostVerificationArea()
        {
            string areaId;
            this.AreaCache.TryGetValue(APPKEY_POSTVERIFICATIONAREA, out areaId);
            if (string.IsNullOrWhiteSpace(areaId))
            {
                areaId = _repos.GetPostVerificationArea();
                AreaCache.TryAdd(APPKEY_POSTVERIFICATIONAREA, areaId);
            }
            return areaId;
        }

        /// <summary>
        /// Returns the area where rejected boxes will be send after validation failed
        /// </summary>
        internal string GetBadVerificationArea()
        {
            string areaId;
            this.AreaCache.TryGetValue(APPKEY_BADVERIFICATIONAREA, out areaId);
            if (string.IsNullOrWhiteSpace(areaId))
            {
                areaId = _repos.GetBadVerificationArea();
                AreaCache.TryAdd(APPKEY_BADVERIFICATIONAREA, areaId);
            }
            return areaId;
        }

        /// <summary>
        /// Mark scanned box as validated.
        /// </summary>
        /// <param name="uccId"></param>
        /// <param name="postVerificationAreaId"> </param>
        /// <param name="badVerificationAreaId"> </param>
        /// <returns></returns>
        internal Tuple<ValidationStatus, string> ValidateBox(string uccId, string postVerificationAreaId, string badVerificationAreaId)
        {
           return _repos.ValidateBox(uccId, postVerificationAreaId, badVerificationAreaId);
        }
    }
}