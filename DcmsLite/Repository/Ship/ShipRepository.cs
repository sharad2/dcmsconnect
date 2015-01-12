using EclipseLibrary.Oracle;
using System;
using System.Collections.Generic;
using System.Linq;
namespace DcmsMobile.DcmsLite.Repository.Ship
{
    public class ShipRepository : DcmsLiteRepositoryBase
    {
        internal IList<PO> GetPoList(string customerId)
        {
            const string QUERY = @"WITH Q1 AS
                                    (SELECT PS.PICKSLIP_ID,
                                         MAX(PS.PO_ID)                              AS PO_ID,
                                         MAX(PS.ITERATION)                          AS ITERATION,
                                         MAX(ps.bucket_id)                          AS BUCKET_ID,
                                         MAX(CUST.CUSTOMER_ID)                      AS CUSTOMER_ID,
                                         MAX(CUST.NAME)                             AS CUST_NAME,
                                         MAX(PS.TOTAL_QUANTITY_ORDERED)             AS TOTAL_QUANTITY_ORDERED,
                                         MAX(PS.WAREHOUSE_LOCATION_ID)              AS WAREHOUSE_LOCATION_ID,
                                         COUNT(UNIQUE B.UCC128_ID)                  AS BOX_COUNT,
                                         SUM(BD.expected_pieces)                    AS EXPECTED_PIECES,
                                         MAX(PS.CUSTOMER_DC_ID)                     AS CUSTOMER_DC_ID,
                                         MAX(PO.START_DATE)                         AS START_DATE,
                                         MIN(PO.DC_CANCEL_DATE)                     AS DCCANCEL_DATE
                                    FROM <proxy/>PS PS
                                   INNER JOIN <proxy/>BUCKET BK
                                      ON BK.BUCKET_ID = PS.BUCKET_ID
                                    LEFT OUTER JOIN <proxy/>CUST CUST
                                      ON PS.CUSTOMER_ID = CUST.CUSTOMER_ID
                                    LEFT OUTER JOIN <proxy/>BOX B
                                      ON B.PICKSLIP_ID = PS.PICKSLIP_ID
                                    LEFT OUTER JOIN <proxy/>BOXDET BD
                                      ON B.PICKSLIP_ID = BD.PICKSLIP_ID
                                     AND B.UCC128_ID = BD.UCC128_ID
                                    LEFT OUTER JOIN <proxy/>PO PO
                                      ON PO.CUSTOMER_ID = PS.CUSTOMER_ID
                                     AND PO.PO_ID = PS.PO_ID
                                     AND PO.ITERATION = PS.ITERATION
                                   WHERE 1 = 1
                                     AND PS.CUSTOMER_ID = :CUSTOMER_ID
                                     AND PS.TRANSFER_DATE IS NULL
                                     AND Bk.FREEZE IS NULL
                                     AND PS.SHIPPING_ID IS NULL
                                     AND PS.PICKSLIP_CANCEL_DATE IS NULL
                                     AND B.STOP_PROCESS_DATE IS NULL
                                     AND BD.STOP_PROCESS_DATE IS NULL
                                     AND PS.PICKSLIP_ID NOT IN
                                         (SELECT EDIPS.PICKSLIP_ID FROM <proxy/>EDI_753_754_PS EDIPS)
                                   GROUP BY PS.PICKSLIP_ID
                                  )
                                SELECT Q1.CUSTOMER_ID                       AS CUSTOMER_ID,
                                       MAX(Q1.CUST_NAME)                    AS NAME,
                                       MAX(Q1.BUCKET_ID)                    AS BUCKET_ID,
                                       Q1.PO_ID                             AS PO_ID,
                                       Q1.ITERATION                         AS ITERATION,
                                       Q1.WAREHOUSE_LOCATION_ID             AS BUILDING,
                                       SUM(Q1.TOTAL_QUANTITY_ORDERED)       AS TOTAL_QUANTITY_ORDERED,
                                       SUM(Q1.BOX_COUNT)                    AS TOTAL_BOXES,
                                       SUM(Q1.EXPECTED_PIECES)              AS EXPECTED_PIECES,
                                       MAX(Q1.START_DATE)                   AS START_DATE,
                                       MIN(Q1.DCCANCEL_DATE)                AS MIN_DCCANCEL_DATE,
                                       Q1.CUSTOMER_DC_ID                    AS CUSTOMER_DC_ID
                                  FROM Q1
                                 GROUP BY Q1.CUSTOMER_ID,
                                          Q1.PO_ID,
                                          Q1.CUSTOMER_DC_ID,
                                          Q1.ITERATION,
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
                CustomerDcId = row.GetString("CUSTOMER_DC_ID"),
                Iteration = row.GetInteger("ITERATION") ?? 0
            }).Parameter("CUSTOMER_ID", customerId);
            return _db.ExecuteReader(QUERY, binder, 10);     //TODO: Remove limit after applying suitable group by.
        }

        public int CreateBol(string customerId, ICollection<Tuple<string, int, string>> poList)
        {
            const string QUERY = @"
                                DECLARE
                                      LEDI_ID         EDI_753_754.EDI_ID%TYPE;                                     
                                      LBOL_COUNT      NUMBER(3);

                                    BEGIN
                                      LEDI_ID := PKG_EDI_2.CREATE_EDI(ACUSTOMER_ID => :CUSTOMER_ID);

                                      PKG_EDI_2.ADD_PO_TO_EDI(AEDI_ID         => LEDI_ID,
                                                              APO_LIST        => :PO_LIST,
                                                              AITERATION_LIST => :ITERATION_LIST,
                                                              ADC_LIST        => :DC_LIST,
                                                              AATS_DATE       => SYSDATE);

                                      :LBOL_COUNT := PKG_EDI_2.CREATE_BOL_FOR_EDI(AEDI_ID => LEDI_ID);

                                    END;";
            int bolCount = 0;
            var binder = SqlBinder.Create();
            binder.ParameterAssociativeArray("PO_LIST", poList.Select(p => p.Item1).ToArray());
            binder.ParameterAssociativeArray("ITERATION_LIST", poList.Select(p => (int?)p.Item2).ToArray());
            binder.ParameterAssociativeArray("DC_LIST", poList.Select(p => p.Item3).ToArray());
            binder.Parameter("CUSTOMER_ID", customerId);
            binder.OutParameter("LBOL_COUNT", val => bolCount = val.Value);
            _db.ExecuteNonQuery(QUERY, binder);
            return bolCount;
        }
    }
}