using System.Collections.Generic;

namespace DcmsMobile.DcmsLite.Repository.Receive
{
    public class ReceiveService : DcmsLiteServiceBase<ReceiveRepository>
    {

        internal IEnumerable<InventoryArea> GetRestockAreaList(string buildingId)
        {
            return _repos.GetRestockAreaList(buildingId);
        }

        public IEnumerable<AsnSummary> GetAsnListToReceive(string buildingId)
        {
            return _repos.GetAsnListToReceive(buildingId);
        }

        internal int ReceiveCartonsOfAsn(string intransitId, string shipmentId, string restockArea)
        {
            var receivedCount = 0;
            using (var trans = _repos.BeginTransaction())
            {
                receivedCount = _repos.ReceiveCartonsOfAsn(intransitId, shipmentId, restockArea);
                _repos.RestockCartons(restockArea);
                trans.Commit();
            }
            return receivedCount;
        }
    }
}