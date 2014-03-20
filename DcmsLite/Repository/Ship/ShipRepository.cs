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
                                     AND Bk.AVAILABLE_FOR_PITCHING = 'Y'
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

        public string CreateBol(string customerId, ICollection<Tuple<string, int, string>> poList)
        {
            const string QUERY = @"
                                DECLARE
                                  LPARENT_BOL_ID SHIP.PARENT_SHIPPING_ID%TYPE;
                                  LBOL_ID        SHIP.SHIPPING_ID%TYPE; 
                                  TYPE PICKSLIP_TABLE_T IS TABLE OF PS.PICKSLIP_ID%TYPE;
                                  PICKSLIP_LIST PICKSLIP_TABLE_T;

                                BEGIN
                                  LPARENT_BOL_ID := GET_BOL_ID(NULL);
                                  LBOL_ID        := GET_BOL_ID(:PICKSLIP_PREFIX);

                                  SELECT PS.PICKSLIP_ID BULK COLLECT
                                    INTO PICKSLIP_LIST
                                    FROM PS PS
                                   WHERE PS.BUCKET_ID > 0
                                     AND PS.REPORTING_STATUS IS NOT NULL
                                     AND PS.TRANSFER_DATE IS NULL
                                     AND PS.SHIPPING_ID IS NULL
                                     AND PS.CUSTOMER_ID = :CUSTOMER_ID
                                     AND PS.PO_ID = :PO_ID
                                     AND PS.ITERATION = :ITERATION
                                     AND PS.CUSTOMER_DC_ID = :CUSTOMER_DC_ID;

                                  INSERT INTO SHIP SHP
                                    (SHP.PARENT_SHIPPING_ID,
                                     SHP.SHIPPING_ID,
                                     SHP.SHIPPING_TYPE,
                                     SHP.CUSTOMER_ID,
                                     SHP.CUSTOMER_DC_ID,
                                     SHP.DOOR_IA_ID, --7562016,3,00890
                                     SHP.PREPAID_CODE,
                                     SHP.SHIP_DATE,
                                     SHP.ARRIVAL_DATE,
                                     SHP.ONHOLD_FLAG,
                                     SHP.SHIPPER_NAME)
                                  VALUES
                                    (LPARENT_BOL_ID,
                                     LBOL_ID,
                                     :SHIPPING_TYPE,
                                     :CUSTOMER_ID,
                                     :CUSTOMER_DC_ID,
                                     :DOOR_IA_ID,
                                     :PREPAID_CODE,
                                     SYSDATE,
                                     SYSDATE + 5,
                                     :ONHOLD_FLAG,
                                     USER);

                                  FORALL I IN PICKSLIP_LIST.FIRST .. PICKSLIP_LIST.LAST
                                    UPDATE DCMS8.PS PS
                                       SET PS.SHIPPING_ID = LBOL_ID
                                     WHERE PS.PICKSLIP_ID = PICKSLIP_LIST(I);

                                  :LBOL_ID := LBOL_ID;
                                END;";
            string bolId = null;
            var binder = SqlBinder.Create();
            binder.ParameterAssociativeArray("PO_ID", poList.Select(p => p.Item1).ToArray());
            binder.ParameterAssociativeArray("ITERATION", poList.Select(p => (int?)p.Item2).ToArray());
            binder.ParameterAssociativeArray("CUSTOMER_DC_ID", poList.Select(p => p.Item3).ToArray());
            binder.Parameter("SHIPPING_TYPE", "BOL")
                .Parameter("CUSTOMER_ID", customerId)
                .Parameter("DOOR_IA_ID", "ADR")
                .Parameter("PREPAID_CODE", "CC")
                .Parameter("ONHOLD_FLAG", "Y")
                .OutParameter("LBOL_ID", val => bolId = val);
            _db.ExecuteNonQuery(QUERY, binder);
            return bolId;
        }
    }
}