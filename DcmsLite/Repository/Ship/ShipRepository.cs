using EclipseLibrary.Oracle;
using System.Collections.Generic;
namespace DcmsMobile.DcmsLite.Repository.Ship
{
    public class ShipRepository : DcmsLiteRepositoryBase
    {
        internal IList<PO> GetPoList()
        {
            const string QUERY = @"WITH Q1 AS
 (SELECT PS.PICKSLIP_ID,
         MAX(PS.PO_ID) AS PO_ID,
         MAX(ps.bucket_id) AS BUCKET_ID,
         MAX(CUST.CUSTOMER_ID) AS CUSTOMER_ID,
         MAX(CUST.NAME) AS CUST_NAME,
         MAX(PS.TOTAL_QUANTITY_ORDERED) AS TOTAL_QUANTITY_ORDERED,
         MAX(PS.WAREHOUSE_LOCATION_ID) AS WAREHOUSE_LOCATION_ID,
         COUNT(UNIQUE B.UCC128_ID) AS BOX_COUNT,
         SUM(BD.expected_pieces) AS EXPECTED_PIECES,
         MAX(PS.CUSTOMER_DC_ID) AS CUSTOMER_DC_ID,
         MAX(PO.START_DATE) AS START_DATE,
         MIN(PO.DC_CANCEL_DATE) AS DCCANCEL_DATE
    FROM PS PS
   INNER JOIN BUCKET BK
      ON BK.BUCKET_ID = PS.BUCKET_ID
    LEFT OUTER JOIN CUST CUST
      ON PS.CUSTOMER_ID = CUST.CUSTOMER_ID
    LEFT OUTER JOIN BOX B
      ON B.PICKSLIP_ID = PS.PICKSLIP_ID
    LEFT OUTER JOIN BOXDET BD
      ON B.PICKSLIP_ID = BD.PICKSLIP_ID
     AND B.UCC128_ID = BD.UCC128_ID
    LEFT OUTER JOIN PO PO
      ON PO.CUSTOMER_ID = PS.CUSTOMER_ID
     AND PO.PO_ID = PS.PO_ID
     AND PO.ITERATION = PS.ITERATION
   WHERE 1 = 1
     AND PS.CUSTOMER_ID = '11160'
        
     AND PS.TRANSFER_DATE IS NULL
     AND Bk.AVAILABLE_FOR_PITCHING = 'Y'
     AND PS.SHIPPING_ID IS NULL
     AND PS.PICKSLIP_CANCEL_DATE IS NULL
     AND B.STOP_PROCESS_DATE IS NULL
     AND BD.STOP_PROCESS_DATE IS NULL
     AND PS.PICKSLIP_ID NOT IN
         (SELECT EDIPS.PICKSLIP_ID FROM EDI_753_754_PS EDIPS)
   GROUP BY PS.PICKSLIP_ID
  
  )
SELECT Q1.CUSTOMER_ID,
       MAX(Q1.CUST_NAME) AS NAME,
       MAX(Q1.BUCKET_ID) AS BUCKET_ID,
       Q1.PO_ID AS PO_ID,
       Q1.WAREHOUSE_LOCATION_ID AS BUILDING,
       SUM(Q1.TOTAL_QUANTITY_ORDERED) AS TOTAL_QUANTITY_ORDERED,
       SUM(Q1.BOX_COUNT) AS TOTAL_BOXES,
       SUM(Q1.EXPECTED_PIECES) AS EXPECTED_PIECES,
       MAX(Q1.START_DATE) AS START_DATE,
       MIN(Q1.DCCANCEL_DATE) AS MIN_DCCANCEL_DATE,
       Q1.CUSTOMER_DC_ID AS CUSTOMER_DC_ID
  FROM Q1
 GROUP BY Q1.CUSTOMER_ID,
          Q1.PO_ID,
          Q1.CUSTOMER_DC_ID,
          Q1.WAREHOUSE_LOCATION_ID
 ORDER BY (MIN(Q1.DCCANCEL_DATE))";

            var binder = SqlBinder.Create(row => new PO
            {
                CustomerId = row.GetString("CUSTOMER_ID"),
                CustomerName = row.GetString("NAME"),
                PoId = row.GetString("PO_ID"),
                BuildingId = row.GetString("BUILDING"),
                PiecesOrdered = row.GetInteger("TOTAL_QUANTITY_ORDERED"),
                NumberOfBoxes = row.GetInteger("TOTAL_BOXES"),
                StartDate = row.GetDate("START_DATE"),
                MinDcCancelDate = row.GetDate("MIN_DCCANCEL_DATE"),
                PickedPieces = row.GetInteger("EXPECTED_PIECES"),
                BucketId = row.GetInteger("BUCKET_ID"),
                CustomerDcId = row.GetString("CUSTOMER_DC_ID")
               
               
                
            });
            return _db.ExecuteReader(QUERY, binder);
        }
    }
}