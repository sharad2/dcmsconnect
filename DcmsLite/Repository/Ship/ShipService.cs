using DcmsMobile.DcmsLite.Repository;
using DcmsMobile.DcmsLite.Repository.Ship;
using System;
using System.Collections.Generic;

namespace DcmsMobile.DcmsLite.Areas.DcmsLite.Controllers
{
    public class ShipService : DcmsLiteServiceBase<ShipRepository>
    {

        internal IList<PO> GetPoList(string customerId)
        {
            return _repos.GetPoList(customerId);
        }

        public string CreateBol(string customerId, ICollection<Tuple<string, int, string>> poList)
        {
            return _repos.CreateBol(customerId, poList);
        }
    }
}
