using System.Collections.Generic;
using System.Web;

namespace DcmsMobile.PieceReplenish.Repository.Diagnostic
{
    public class DiagnosticService
    {
        #region Intialization

        private readonly DiagnosticRepository _repos;
        public DiagnosticService(DiagnosticRepository repos)
        {
            _repos = repos;
        }

        public DiagnosticService(string userName, string clientInfo, TraceContext ctx, string connectString)
        {
            _repos = new DiagnosticRepository(userName, clientInfo, ctx, connectString);
        }

        public void Dispose()
        {
            _repos.Dispose();
        }

        #endregion

        internal IEnumerable<SkuRequirement> GetSkuRequirements(int? skuId)
        {
            return _repos.GetSkuRequirements(skuId);
        }

        internal IEnumerable<Carton> GetCartonsOfSku(int? skuId, string restockAreaId, string cartonAreaId)
        {
            return _repos.GetCartonsOfSku(skuId, restockAreaId, cartonAreaId);
        }
    }
}