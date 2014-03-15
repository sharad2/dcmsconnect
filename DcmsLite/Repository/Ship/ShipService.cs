using DcmsMobile.DcmsLite.Repository;
using DcmsMobile.DcmsLite.Repository.Ship;
using System.Collections.Generic;

namespace DcmsMobile.DcmsLite.Areas.DcmsLite.Controllers
{
    public class ShipService : DcmsLiteServiceBase<ShipRepository>
    {

        internal IList<PO> GetPoList(string customerId)
        {
            return _repos.GetPoList(customerId);
        }
    }
}
