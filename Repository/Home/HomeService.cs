using System;
using System.Collections.Generic;
using System.Web;

namespace DcmsMobile.PieceReplenish.Repository.Home
{
    public class HomeService : IDisposable
    {
        #region Intialization

        private readonly HomeRepository _repos;
        private readonly string _userName;
        public HomeService(HomeRepository repos)
        {
            _repos = repos;
        }

        public HomeService(TraceContext ctx, string connectString, string userName, string clientInfo)
        {
            _userName = userName;
#if DEBUG
            if (userName.StartsWith("_"))
            {
                // This is a dummy user. Don't let the repository know about this
                userName = "";
            }
#endif
            _repos = new HomeRepository(ctx, connectString, userName, clientInfo);
        }

        public void Dispose()
        {
            _repos.Dispose();
        }

        #endregion

        private Pallet _pallet;

        /// <summary>
        /// Get areas/building list for which there are cartons to pull
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Area> GetAreaList()
        {
            return _repos.GetInventoryAreas();
        }

        /// <summary>
        /// Cache the result locally to optimize multiple calls within same invocation
        /// </summary>
        /// <param name="palletId"></param>
        /// <returns></returns>
        public Pallet GetPallet(string palletId)
        {
            if (_pallet == null || _pallet.PalletId != palletId)
            {
                _pallet = _repos.GetPallet(palletId);
            }
            return _pallet;
        }

        /// <summary>
        /// Returns all pullable SKUs
        /// </summary>
        /// <param name="buildingId"></param>
        /// <param name="destAreaId"></param>
        /// <param name="cartonAreaId"> </param>
        /// <param name="restockAreaId"> </param>
        /// <returns></returns>
        public IEnumerable<AisleSku> GetSkusToPull(string buildingId, string destAreaId, string cartonAreaId, string restockAreaId)
        {
            var skus = _repos.GetPullableSkus(buildingId, destAreaId, cartonAreaId, restockAreaId);
            return skus;
        }

        /// <summary>
        /// Returns a list of cartons which should be pulled
        /// </summary>
        /// <param name="buildingId"></param>
        /// <param name="pickAreaId"></param>
        /// <param name="restockAreaId"></param>
        /// <param name="palletId"></param>
        /// <param name="suggestCartonCount">
        /// Returns the count of cartons which were suggested previously for this pallet
        /// </param>
        /// <returns></returns>
        public IEnumerable<PullableCarton> GetCartonSuggestions(string buildingId, string pickAreaId, string restockAreaId, string palletId, int maxRows, out int suggestCartonCount)
        {
            return _repos.GetCartonSuggestions(buildingId, pickAreaId, restockAreaId, palletId, maxRows, out suggestCartonCount);
        }

        public IEnumerable<PullableCarton> GetProposedCartonSuggestions(string buildingId, string pickAreaId, string cartonAreaId, string restockAreaId, int maxRows)
        {
            int dummy;
            using (var trans = _repos.BeginTransaction())
            {
                return _repos.GetCartonSuggestions(buildingId, pickAreaId, restockAreaId, null, maxRows, out dummy);
                // No commit which means that these suggestions will not be saved
            }
        }

        /// <summary>
        /// Returns a list of cartons which have been suggested to someone.
        /// </summary>
        /// <param name="buildingId"></param>
        /// <returns></returns>
        public IEnumerable<PullerActivity> GetCartonsBeingPulled(string buildingId)
        {
            return _repos.GetPullerActivity(buildingId);
        }

        /// <summary>
        /// Marks a carton in suspense, if location id of passed carton <paramref name="cartonId"/> is <paramref name="locationId"/> in system
        /// </summary>
        /// <param name="cartonId"></param>
        /// <param name="locationId"></param>
        /// <returns>
        /// returns true if successfully marked carton in suspense, else false.
        /// </returns>
        internal bool MarkCartonInSuspense(string cartonId, string locationId)
        {
            return _repos.MarkCartonInSuspense(cartonId, locationId);
        }

        /// <summary>
        /// Pulls the cartons for passed area/aisle
        /// </summary>
        /// <param name="cartonId"></param>
        /// <param name="palletId"></param>
        /// <param name="restockAreaId"></param>
        /// <param name="restockAisleId"></param>
        /// <param name="countSuggestions">Number of suggestions remaining for the pallet</param>
        /// <returns>Returns true if carton is successfully pulled</returns>
        public bool TryPullCarton(string cartonId, string palletId, string restockAreaId, string restockAisleId, out int countSuggestions)
        {
            //int count_suggestions;
            var result = _repos.TryPullCartonForAisle(cartonId, palletId, restockAreaId, restockAisleId, out countSuggestions);
            _pallet = null;     // Force requery of the pallet
            //countSuggestions = count_suggestions;
            return result;
        }

        /// <summary>        
        /// Puller just don't want to pull this carton. Remove it from list of suggestions.
        /// </summary>
        /// <param name="cartonId"></param>
        internal void RemoveCartonSuggestion(string cartonId)
        {
            _repos.RemoveCartonSuggestion(cartonId);
        }

        /// <summary>
        /// The priority of the passed SKU is increased.
        /// </summary>
        /// <param name="buildingId"></param>
        /// <param name="areaId"></param>
        /// <param name="skuId"></param>
        /// <returns>
        /// If successfully updated then returns expiration time when till this priority will be set to high, else null.
        /// </returns>
        /// <remarks>
        /// If successfully updated then discards the old data of pullable cartons
        /// </remarks>
        public DateTime? IncreaseSkuPriority(string buildingId, string areaId, int skuId)
        {
            var expiryTime = _repos.IncreaseSkuPriority(buildingId, areaId, skuId, _userName);
            return expiryTime;
        }

        /// <summary>
        /// The priority of the passed SKU is set to normal.
        /// </summary>
        /// <param name="buildingId"></param>
        /// <param name="areaId"></param>
        /// <param name="skuId"></param>
        /// <returns>
        /// Returns true if successfully updated.
        /// </returns>
        /// <remarks>
        /// If successfully updated then discards the old data of pullable cartons
        /// </remarks>
        public bool DecreaseSkuPriority(string buildingId, string areaId, int skuId)
        {
            var b = _repos.DecreaseSkuPriority(buildingId, areaId, skuId, _userName);
            return b;
        }

        /// <summary>
        /// Get the info about refresh time of the pullable cartons list
        /// </summary>
        /// <returns></returns>
        public JobRefresh GetRefreshInfo()
        {
            return _repos.GetRefreshInfo();
        }

        /// <summary>
        /// Refreshes the old data of pullable cartons
        /// </summary>
        public void RefreshPullableCartons()
        {
            _repos.RefreshPullableCartons();
        }

        public int DiscardPalletSuggestion(string pullerName, string palletId)
        {
            return _repos.DiscardPalletSuggestion(pullerName, palletId);
        }
    }
}



/*
    $Id: PieceReplenishService.cs 17726 2012-07-26 08:19:26Z bkumar $
    $Revision: 17726 $
    $URL: svn://vcs/net4/Projects/Mvc/DcmsMobile.Pull/trunk/Pull/Repository/PieceReplenishService.cs $
    $Header: svn://vcs/net4/Projects/Mvc/DcmsMobile.Pull/trunk/Pull/Repository/PieceReplenishService.cs 17726 2012-07-26 08:19:26Z bkumar $
    $Author: bkumar $
    $Date: 2012-07-26 13:49:26 +0530 (Thu, 26 Jul 2012) $
*/
