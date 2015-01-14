using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Runtime.Caching;

namespace DcmsMobile.PickWaves.Repository
{
    /// <summary>
    /// Provides services common to all controllers
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class PickWaveServiceBase<T> : IDisposable where T : PickWaveRepositoryBase
    {
        #region Intialization
        protected T _repos;

        public void Dispose()
        {
            _repos.Dispose();
        }

        #endregion

        private const string APPKEY_CUSTOMER_INFO = "PickWavesService_CustomerInfoCacheKey";

        public string GetCustomerName(string customerId)
        {
            if (string.IsNullOrWhiteSpace(customerId))
            {
                throw new ArgumentNullException("customerId");
            }
            var dictCache = MemoryCache.Default[APPKEY_CUSTOMER_INFO] as IDictionary<string, string>;
            if (dictCache == null)
            {
                dictCache = new Dictionary<string, string>();
                MemoryCache.Default.Add(APPKEY_CUSTOMER_INFO, dictCache, DateTime.Now.AddHours(2));
            }
            string customer;
            if (!dictCache.TryGetValue(customerId, out customer))
            {
                customer = _repos.GetCustomerName(customerId);
                dictCache.Add(customerId, customer);
            }
            return customer;
        }


        /// <summary>
        /// </summary>
        /// <param name="bucketId"></param>
        /// <returns></returns>
        internal BucketWithActivities GetBucket(int bucketId)
        {
            return _repos.GetBucket(bucketId);
        }

        internal DbTransaction BeginTransaction()
        {
            return _repos.BeginTransaction();
        }
    }
}