using DcmsMobile.PickWaves.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DcmsMobile.PickWaves.Areas.PickWaves.Home
{
    internal class HomeService : IDisposable
    {
        #region Intialization
        private readonly HomeRepository _repos;

        /// <summary>
        /// For unit tests. 
        /// </summary>
        public HomeService(HomeRepository repos)
        {
            _repos = repos;
        }

        public HomeService(TraceContext ctx, string userName, string clientInfo)
        {
            _repos = new HomeRepository(ctx, userName, clientInfo);
        }

        public void Dispose()
        {
            _repos.Dispose();
        }
        #endregion

        /// <summary>
        /// Get buckets of passed customer or all customers
        /// </summary>
        /// <param name="filterType"> </param>
        /// <param name="filterText"> </param>
        /// <returns></returns>
        /// <remarks>
        /// Routing information is returned only when customer id is passed
        /// </remarks>
        public IList<BucketSummary> GetBucketSummary(SearchTextType filterType, string filterText)
        {
            switch (filterType)
            {
                case SearchTextType.Unknown:
                    return _repos.GetBucketSummary(null, null);

                case SearchTextType.BucketId:
                    // We don't expect to show a summary for a single bucket
                    throw new NotSupportedException();

                case SearchTextType.CustomerId:
                    return _repos.GetBucketSummary(filterText, null);

                case SearchTextType.UserName:
                    return _repos.GetBucketSummary(null, filterText);

                default:
                    throw new NotImplementedException();
            }
        }

        public SearchTextType ParseSearchText(string searchText)
        {
            return _repos.ParseSearchText(searchText);
        }

        public IEnumerable<ImportedOrderSummary> GetImportedOrderSummary(SearchTextType filterType, string filterText)
        {
            switch (filterType)
            {
                case SearchTextType.Unknown:
                    return _repos.GetImportedOrderSummary(null);

                case SearchTextType.BucketId:
                    // We don't expect to show a summary for a single bucket
                    throw new NotSupportedException();

                case SearchTextType.CustomerId:
                    return _repos.GetImportedOrderSummary(filterText);

                case SearchTextType.UserName:
                   return Enumerable.Empty<ImportedOrderSummary>();

                default:
                    throw new NotImplementedException();
            }
        }

        public IList<BucketBase> GetRecentCreatedBucket(int maxRows)
        {
            return _repos.GetRecentCreatedBucket(maxRows);
        }

        public IList<BucketBase> GetBucketToExpedite(int maxRows)
        {
           return _repos.GetExpeditableBuckets(maxRows);
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
