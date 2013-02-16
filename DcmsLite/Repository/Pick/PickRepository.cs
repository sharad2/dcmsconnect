using EclipseLibrary.Oracle;
using System.Collections.Generic;

namespace DcmsMobile.DcmsLite.Repository.Pick
{
    public class PickRepository : DcmsLiteRepositoryBase
    {
        public string PrintBucket(int bucketId, int batchSize, string printerId)
        {
            const string QUERY = @"
                    DECLARE
                    LCount BINARY_INTEGER;
                    BEGIN
                    :PALLET_ID := <proxy />pkg_bucket_lite.CREATE_BATCH(ABUCKET_ID =&gt; :BUCKET_ID, ABATCH_SIZE =&gt; :NUMBER_OF_BOXES);
                    IF :PALLET_ID IS NOT NULL THEN
                      LCount := <proxy />pkg_bucket_lite.PRINT_BATCH(ABATCH_ID =&gt; :PALLET_ID, APRINTER_NAME =&gt; :PRINTER_NAME);
                      LCount := <proxy />pkg_bucket_lite.PITCH_BATCH(ABATCH_ID =&gt; :PALLET_ID);
                    END IF;
                    END;
                ";
            string palletId = string.Empty;
            var binder = SqlBinder.Create()
                .Parameter("BUCKET_ID", bucketId)
                .Parameter("NUMBER_OF_BOXES", batchSize)
                .Parameter("PRINTER_NAME", printerId)
                .OutParameter("PALLET_ID", val => palletId = val)
                ;

            _db.ExecuteNonQuery(QUERY, binder);
            return palletId;
        }

        /// <summary>
        /// If bucketId is not passed, only available buckets are returned.
        /// </summary>
        /// <param name="buildingId"></param>
        /// <param name="bucketId"></param>
        /// <param name="maxRows"></param>
        /// <returns></returns>
        /// <remarks>We ensure that 0 rows are returned for non qualifying buckets</remarks>
        public IEnumerable<Bucket> GetBucketSummary(string buildingId, int? bucketId, string customerId, int? maxRows)
        {
            const string QUERY = @"
WITH Q1 AS
 (SELECT MAX(BKT.BUCKET_ID)             AS BUCKET_ID,
         MAX(BKT.NAME)                  AS NAME,
         MAX(P.VWH_ID)                  AS VWH_ID,
         MAX(P.CUSTOMER_DC_ID)          AS CUSTOMER_DC_ID,
         MAX(CUST.NAME)                 AS CUSTOMER_NAME,
         MAX(P.CUSTOMER_ID)             AS CUSTOMER_ID,
         MAX(BKT.DATE_CREATED)          AS DATE_CREATED,
         MIN(B.LAST_UCC_PRINT_DATE)     AS MIN_PRINT_DATE,
         MAX(B.LAST_UCC_PRINT_DATE)     AS MAX_PRINT_DATE,
         COUNT(DISTINCT P.PICKSLIP_ID)  AS PICKSLIP_COUNT,
         MAX(P.PO_ID)                   AS PO_ID,
         COUNT(DISTINCT B.UCC128_ID)    AS BOX_COUNT,
         MAX(BKT.FREEZE)                AS FREEZE,
         COUNT(DISTINCT CASE
                 WHEN B.LAST_UCC_PRINT_DATE IS NOT NULL THEN
                  B.UCC128_ID
               END)                     AS NUMBER_OF_UCC_PRINTED,
         COUNT(DISTINCT CASE
                 WHEN B.PALLET_ID IS NULL THEN
                  B.UCC128_ID
               END)                     AS BOXES_NOT_IN_BATCH,
         MIN(B.LAST_UCC_PRINTED_BY)     AS LAST_PRINTED_BY_MIN,
        COUNT(UNIQUE B.LAST_UCC_PRINTED_BY)     AS COUNT_PRINTED_BY
    FROM <proxy />BUCKET BKT
   INNER JOIN <proxy />PS P
      ON BKT.BUCKET_ID = P.BUCKET_ID
    LEFT OUTER JOIN <proxy />BOX B
      ON P.PICKSLIP_ID = B.PICKSLIP_ID
   INNER JOIN <proxy />CUST CUST
      ON CUST.CUSTOMER_ID = p.CUSTOMER_ID
   WHERE P.TRANSFER_DATE IS NULL
     AND B.STOP_PROCESS_DATE IS NULL
 <if>AND P.WAREHOUSE_LOCATION_ID = :WAREHOUSE_LOCATION_ID</if>
 <if>AND BKT.BUCKET_ID = :BUCKET_ID</if>
<else>
AND BKT.STATUS IS NOT NULL
</else>
<if>AND P.CUSTOMER_ID = :CUSTOMER_ID</if>
<if c='$BUCKET_ID'>
having  MAX(BKT.BUCKET_ID) is not null
</if>
<else>
   GROUP BY BKT.BUCKET_ID,
            P.CUSTOMER_ID,
            P.PO_ID,
            P.VWH_ID,
            P.CUSTOMER_DC_ID
</else>
   ORDER BY MAX(BKT.DATE_CREATED) DESC)
SELECT * FROM Q1
<if>
WHERE ROWNUM &lt; :max_rows
</if>
            ";

            var binder = SqlBinder.Create(row => new Bucket
            {
                BucketId = row.GetInteger("BUCKET_ID").Value,
                BucketName = row.GetString("NAME"),
                LastPrintedBy = row.GetString("LAST_PRINTED_BY_MIN"),
                CreationDate = row.GetDate("Date_Created").Value,
                TotalPickslips = row.GetInteger("PICKSLIP_COUNT").Value,
                TotalBoxes = row.GetInteger("BOX_COUNT").Value,
                BoxesNotInBatch = row.GetInteger("BOXES_NOT_IN_BATCH").Value,
                CountPrintedBoxes = row.GetInteger("NUMBER_OF_UCC_PRINTED").Value,
                MinPrintedOn = row.GetDate("min_print_date"),
                MaxPrintedOn = row.GetDate("max_print_date"),
                CustomerName = row.GetString("CUSTOMER_NAME"),
                CustomerId = row.GetString("CUSTOMER_ID"),
                VwhId = row.GetString("VWH_ID"),
                PoId = row.GetString("po_id"),
                IsFrozen = row.GetString("FREEZE") == "Y",
                CustomerDcId = row.GetString("customer_dc_id"),
                CountPrintedBy = row.GetInteger("COUNT_PRINTED_BY")
            })
            .Parameter("WAREHOUSE_LOCATION_ID", buildingId)
            .Parameter("BUCKET_ID", bucketId)
            .Parameter("CUSTOMER_ID", customerId)
            .Parameter("max_rows", maxRows);

            return _db.ExecuteReader(QUERY, binder);
        }

        public IEnumerable<Printer> GetPrinterList(string buildingId)
        {
            const string QUERY = @"
                SELECT TP.NAME                  AS NAME,
                       TP.NAME || ' - ' || TP.DESCRIPTION   AS DESCRIPTION,
                       TP.PRINTER_TYPE          AS PRINTER_TYPE,
                       TP.WAREHOUSE_LOCATION_ID AS WAREHOUSE_LOCATION_ID
                  FROM <proxy />TAB_PRINTER TP
                 WHERE TP.WAREHOUSE_LOCATION_ID = :WAREHOUSE_LOCATION_ID
                   AND TP.PRINTER_TYPE ='ZEBRA'
            ";

            var binder = SqlBinder.Create(row => new Printer
            {
                PrinterName = row.GetString("NAME"),
                Description = row.GetString("DESCRIPTION"),
                PrinterType = row.GetString("PRINTER_TYPE"),
                BuildingId = row.GetString("WAREHOUSE_LOCATION_ID")
            }).Parameter("WAREHOUSE_LOCATION_ID", buildingId);

            return _db.ExecuteReader(QUERY, binder);
        }

        internal IList<Box> GetBoxesOfBatch(string batchId)
        {
            const string QUERY = @"
               SELECT B.UCC128_ID           AS UCC128_ID,
                      B.BOX_ID              AS BOX_ID,
                      B.LAST_UCC_PRINT_DATE AS LAST_UCC_PRINT_DATE
                 FROM <proxy />BOX B
                WHERE B.PALLET_ID = :BATCH_ID
                ORDER BY LPAD(BOX_ID, 4, 0)
            ";

            var binder = SqlBinder.Create(row => new Box
            {
                UccId = row.GetString("UCC128_ID"),
                BoxId = row.GetString("BOX_ID"),
                LastUccPrintDate = row.GetDate("LAST_UCC_PRINT_DATE")
            }).Parameter("BATCH_ID", batchId);

            return _db.ExecuteReader(QUERY, binder);
        }

        /// <summary>
        /// Pass any one of the parameters to get the requisite list of batches
        /// </summary>
        /// <param name="bucketId">Returns information about all batches of this bucket</param>
        /// <param name="batchId">Returns information about this batch</param>
        /// <param name="uccId">Returns a single batch to which this ucc belongs</param>
        /// <returns></returns>
        internal IList<PrintBatch> GetBatches(int? bucketId, string batchId, string uccId)
        {
            const string QUERY = @"
             SELECT B.PALLET_ID                 AS BATCH_ID,
                    MAX(P.BUCKET_ID)            AS BUCKET_ID,                    
                    COUNT(B.UCC128_ID)          AS BOX_COUNT,
                    MAX(B.LAST_UCC_PRINT_DATE)  AS LAST_PRINT_DATE,
                    RTRIM(REGEXP_REPLACE((listagg(b.last_ucc_printed_by, ',') WITHIN
                         GROUP(ORDER BY b.last_ucc_print_date)), '([^,]*)(,\1)+($|,)','\1\3'),',') AS LAST_PRINTED_BY,
            COUNT(DISTINCT CASE
                    WHEN B.LAST_UCC_PRINT_DATE IS NULL THEN
                  B.UCC128_ID
               END) AS UNPRINTED_BOXES
              FROM <proxy />BOX B
             INNER JOIN <proxy />PS P
                ON P.PICKSLIP_ID = B.PICKSLIP_ID
             INNER JOIN <proxy />BUCKET BKT
                ON BKT.BUCKET_ID = P.BUCKET_ID             
             WHERE P.TRANSFER_DATE IS NULL              
               AND B.STOP_PROCESS_DATE IS NULL               
              AND B.PALLET_ID IS NOT NULL
               <if>AND BKT.BUCKET_ID = :BUCKET_ID</if>
               <if>AND B.PALLET_ID = :PALLET_ID</if>
               <if>AND B.UCC128_ID = :UCC_ID</if>
             GROUP BY B.PALLET_ID
             ORDER BY MAX(B.LAST_UCC_PRINT_DATE) DESC NULLS LAST
            ";

            var binder = SqlBinder.Create(row => new PrintBatch
            {
                BatchId = row.GetString("BATCH_ID"),
                BucketId = row.GetInteger("BUCKET_ID"),
                LastUccPrintDate = row.GetDate("LAST_PRINT_DATE"),
                PrintedBy = row.GetString("LAST_PRINTED_BY"),
                CountBoxes = row.GetInteger("BOX_COUNT").Value,
                CountUnprintedBoxes = row.GetInteger("UNPRINTED_BOXES").Value
            })
            .Parameter("BUCKET_ID", bucketId)
            .Parameter("PALLET_ID", batchId)
            .Parameter("UCC_ID", uccId);
            return _db.ExecuteReader(QUERY, binder);
        }


        /// <summary>
        /// Reprints labels for the boxes of <paramref name="batchId"/> as well try to pitch all the box.
        /// </summary>
        /// <param name="batchId"></param>
        /// <param name="printer"></param>
        internal void ReprintBatch(string batchId, string printer)
        {
            const string QUERY = @"
                DECLARE
                LCount BINARY_INTEGER;
                BEGIN
                  LCount := <proxy />pkg_bucket_lite.PRINT_BATCH(ABATCH_ID =&gt; :PALLET_ID, APRINTER_NAME =&gt; :PRINTER_NAME);
                  LCount := <proxy />pkg_bucket_lite.PITCH_BATCH(ABATCH_ID =&gt; :PALLET_ID);
                END;
                ";
            var binder = SqlBinder.Create()
                .Parameter("PALLET_ID", batchId)
                .Parameter("PRINTER_NAME", printer);
            _db.ExecuteNonQuery(QUERY, binder);
        }

        internal void PrintBoxLabel(string uccId, string printer)
        {
            const string QUERY = @"
              BEGIN             
              <proxy />PKG_BUCKET_LITE.PRINT_BOX(AUCC128_ID   =&gt; :AUCC128_ID,
                                                APRINTER_NAME =&gt; :APRINTER_NAME);
            END;  ";
            var binder = SqlBinder.Create()
                .Parameter("AUCC128_ID", uccId)
                .Parameter("APRINTER_NAME", printer);
            _db.ExecuteNonQuery(QUERY, binder);
        }
    }
}