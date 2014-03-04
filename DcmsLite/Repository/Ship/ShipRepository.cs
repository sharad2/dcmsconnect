using EclipseLibrary.Oracle;
using System.Collections.Generic;
namespace DcmsMobile.DcmsLite.Repository.Ship
{
    public class ShipRepository : DcmsLiteRepositoryBase
    {
        internal IEnumerable<PO> GetPoList()
        {
             const string QUERY = @"Select p.po_id,p.customer_id, p.bucket_id from ps p where p.transfer_date is null
            and rownum &lt; 10";

            var binder = SqlBinder.Create(row => new PO
            {
                PoId = row.GetString("PO_ID"),
                CustomerId = row.GetString("CUSTOMER_ID"),
                BucketId = row.GetInteger("BUCKET_ID").Value
            });
            return _db.ExecuteReader(QUERY, binder);
        }
    }
}