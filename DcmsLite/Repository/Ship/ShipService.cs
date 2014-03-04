using DcmsMobile.DcmsLite.Repository;
using DcmsMobile.DcmsLite.Repository.Ship;
using System.Collections.Generic;

namespace DcmsMobile.DcmsLite.Areas.DcmsLite.Controllers
{
    public class ShipService : DcmsLiteServiceBase<ShipRepository>
    {

        internal IEnumerable<PO> GetPoList()
        {
            return _repos.GetPoList();
        }
    }
}
