using System;
using System.Collections.Generic;
using System.Web;
using System.Linq;

namespace DcmsMobile.REQ2.Repository.Pull
{
    public class PullService : IDisposable
    {
        #region Intialization

        private readonly PullRepository _repos;

        /// <summary>
        /// For unit tests. 
        /// </summary>
        public PullService(PullRepository repos)
        {
            _repos = repos;
        }

        /// <summary>
        /// Used to store destination area of intransit cartons until they are received
        /// </summary>      
        public PullService(TraceContext ctx, string connectString, string userName, string clientInfo, string moduleName)
        {
            _repos = new PullRepository(ctx, connectString, userName, clientInfo, moduleName);
        }

        public void Dispose()
        {
            _repos.Dispose();
        }

        #endregion
        /// <summary>
        /// Gets the areas where pulling can be done. also fetches best request per area.
        /// </summary>
        /// <returns></returns>
        internal  IEnumerable<Area> GetAreaSuggestions()
        {
            return _repos.GetPullAreaSuggestions();
        }

        internal string GetBestRequest(string sourceAreaId ,string destAreaId)
        {
            return _repos.GetBestRequest(sourceAreaId, destAreaId);
        }

        /// <summary>
        /// Suggests cartons for the pased area/request.  Called after each carton scan.
        /// </summary>
        /// <param name="areaId"></param>
        /// <param name="palletId"></param>
        /// <param name="requestId"></param>
        /// <returns></returns>
        internal IEnumerable<PullableCarton> GetCartonSuggestions(string palletId, string requestId)
        {
            return _repos.GetCartonSuggestions(palletId,requestId);
        }

        /// <summary>
        /// Pulls the passed carton. 
        /// </summary>
        /// <param name="areaId"></param>
        /// <param name="palletId"></param>
        /// <param name="cartonId"></param>
        internal void PullCarton(string palletId, string cartonId, string requestId)
        {
            _repos.PullCartonForRequest(palletId, cartonId, requestId);
        }

        /// <summary>
        /// Called after pallet scan and then after each carton scan. TODO: So we should cache it.
        /// </summary>
        /// <param name="requestId"></param>
        /// <returns></returns>
        internal PullRequest  GetRequest(string requestId)
        {
           return  _repos.GetRequestInfo(requestId);
        }
    }
}