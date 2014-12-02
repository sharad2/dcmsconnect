using DcmsMobile.Inquiry.Helpers;
using EclipseLibrary.Oracle;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.Contracts;
using System.Web;

namespace DcmsMobile.Inquiry.Areas.Inquiry.PickslipEntity
{
    internal class PickslipEntityRepository : IDisposable
    {
        private readonly OracleDatastore _db;
        public PickslipEntityRepository(string userName, string clientInfo)
        {
            _db = new OracleDatastore(HttpContext.Current.Trace);
            _db.CreateConnection(ConfigurationManager.ConnectionStrings["dcms8"].ConnectionString, userName);
            _db.ModuleName = "Inquiry_PickslipEntity";
            _db.ClientInfo = clientInfo;
        }

        public void Dispose()
        {
            _db.Dispose();
        }

        /// <summary>
        /// Function return the Pickslip info.
        /// </summary>
        /// <returns></returns>
        public Pickslip GetActivePickslip(long pickslipId)
        {
            Contract.Assert(_db != null);
            const string QUERY_PICKSLIP_INFO = @"
                SELECT PS.PICKSLIP_ID            AS PICKSLIP_ID,
                MC.CARRIER_ID || '.: ' || MC.DESCRIPTION AS CARRIER_ID,
                       CST.CUSTOMER_ID           AS CUSTOMER_ID,
                       CST.NAME                  AS NAME,
                       PS.PO_ID                  AS PO_ID,                      
                       PS.EXPORT_FLAG            AS EXPORT_FLAG,                       
                       PS.ITERATION              AS ITERATION,                       
                       PS.DATE_CREATED           AS DATE_CREATED,
                       PS.PICKSLIP_IMPORT_DATE   AS PICKSLIP_IMPORT_DATE,
                       PS.PICKSLIP_CANCEL_DATE   AS PICKSLIP_CANCEL_DATE,
                       PS.TRANSFER_DATE          AS TRANSFER_DATE,
                       PS.CUSTOMER_STORE_ID      AS CUSTOMER_STORE_ID,
                       PS.CUSTOMER_DEPARTMENT_ID AS CUSTOMER_DEPARTMENT_ID,                       
                       PS.ERP_ID AS ERP_ID,
                       TRIM(PS.CUSTOMER_DC_ID)   AS CUSTOMER_DC_ID,
                       SHIP.SHIPPING_ID          AS SHIPPING_ID,
                       SHIP.SHIP_DATE            AS SHIP_DATE,
                       SHIP.SHIPPER_NAME         AS SHIPPER_NAME,
                       SHIP.ONHOLD_FLAG         AS ONHOLD_FLAG,
                       PO.DC_CANCEL_DATE         AS DC_CANCEL_DATE,
                       PO.START_DATE             AS START_DATE,
                       PO.CANCEL_DATE            AS CANCEL_DATE,
                       ps.TOTAL_QUANTITY_ORDERED AS TOTAL_QUANTITY_ORDERED,
                       B.BUCKET_ID              AS BUCKET_ID,
                       B.CREATED_BY              AS BUCKET_CREATED_BY,
                       B.DATE_CREATED            AS BUCKET_DATE_CREATED,
                       PS.SHIPPING_ADDRESS.ADDRESS_LINE_1 AS SHIPPING_ADDRESS_LINE_1,
                       PS.SHIPPING_ADDRESS.ADDRESS_LINE_2 AS SHIPPING_ADDRESS_LINE_2,
                       PS.SHIPPING_ADDRESS.ADDRESS_LINE_3 AS SHIPPING_ADDRESS_LINE_3,
                       PS.SHIPPING_ADDRESS.ADDRESS_LINE_4 AS SHIPPING_ADDRESS_LINE_4,
                       PS.SHIPPING_ADDRESS.CITY           AS SHIPPING_ADDRESS_CITY,
                       PS.SHIPPING_ADDRESS.STATE           AS SHIPPING_ADDRESS_STATE,
                       PS.SHIPPING_ADDRESS.ZIP_CODE        AS SHIPPING_ADDRESS_ZIP_CODE,
                       PS.SHIPPING_ADDRESS.COUNTRY_CODE    AS SHIPPING_ADDRESS_COUNTRY_CODE,
                       ps.vendor_number                    AS vendor_number,
                       CST.ASN_FLAG                        AS ASN_FLAG  
                  FROM <proxy />PS PS
                  LEFT OUTER JOIN <proxy />CUST CST
                  ON CST.CUSTOMER_ID = PS.CUSTOMER_ID
                  LEFT OUTER JOIN  <proxy />MASTER_CARRIER MC
                  ON  MC.CARRIER_ID=PS.CARRIER_ID 
                  LEFT OUTER JOIN <proxy />PO PO
                  ON PO.PO_ID = PS.PO_ID
                  AND PO.CUSTOMER_ID = PS.CUSTOMER_ID
                  AND PO.ITERATION = PS.ITERATION
                  LEFT OUTER JOIN <proxy />SHIP SHIP
                  ON SHIP.SHIPPING_ID = PS.SHIPPING_ID
                  LEFT OUTER JOIN <proxy />BUCKET B 
                  ON PS.BUCKET_ID = B.BUCKET_ID 
                  WHERE PS.PICKSLIP_ID = :PICKSLIP_ID
            ";

            var binder = SqlBinder.Create(row => new Pickslip
            {
                PickslipId = row.GetLong("PICKSLIP_ID").Value,
                CarrierId = row.GetString("CARRIER_ID"),
                ShipDate = row.GetDate("ship_date"),
                PickslipCancelDate = row.GetDate("PICKSLIP_CANCEL_DATE"),
                CreateDate = row.GetDate("DATE_CREATED"),
                ImportDate = row.GetDate("PICKSLIP_IMPORT_DATE"),
                TransferDate = row.GetDate("TRANSFER_DATE"),
                CustomerDC = row.GetString("CUSTOMER_DC_ID"),
                CustomerStore = row.GetString("CUSTOMER_STORE_ID"),
                ShippingId = row.GetString("SHIPPING_ID"),
                TotalQuantityOrdered = row.GetInteger("TOTAL_QUANTITY_ORDERED"),
                ExportFlag = !string.IsNullOrWhiteSpace(row.GetString("EXPORT_FLAG")) ? "Yes" : "No",
                AsnFlag=!string.IsNullOrEmpty(row.GetString("ASN_FLAG")),
                BucketId = row.GetInteger("BUCKET_ID"),
                BucketCreatedBy = row.GetString("BUCKET_CREATED_BY"),
                BucketCreatedOn = row.GetDate("BUCKET_DATE_CREATED"),
                ShipperName = row.GetString("SHIPPER_NAME"),
                ShipmentOnHold = row.GetString("ONHOLD_FLAG"),
                CustomerDepartmentId = row.GetString("CUSTOMER_DEPARTMENT_ID"),
                VendorNumber = row.GetString("vendor_number"),
                PoId = row.GetString("PO_ID"),
                Iteration = row.GetInteger("ITERATION").Value,
                CancelDate = row.GetDate("CANCEL_DATE"),
                StartDate = row.GetDate("START_DATE"),
                DcCancelDate = row.GetDate("DC_CANCEL_DATE"),
                CustomerId = row.GetString("CUSTOMER_ID"),
                CustomerName = row.GetString("NAME"),
                ShipAddress = new[] {
                         row.GetString("SHIPPING_ADDRESS_LINE_1"),
                         row.GetString("SHIPPING_ADDRESS_LINE_2"),
                         row.GetString("SHIPPING_ADDRESS_LINE_3"),
                         row.GetString("SHIPPING_ADDRESS_LINE_4")
                     },
                ShipCity = row.GetString("SHIPPING_ADDRESS_CITY"),
                ShipCountry = row.GetString("SHIPPING_ADDRESS_COUNTRY_CODE"),
                ShipState = row.GetString("SHIPPING_ADDRESS_STATE"),
                ShipZipCode = row.GetString("SHIPPING_ADDRESS_ZIP_CODE"),
                ErpId = row.GetString("ERP_ID")
            }).Parameter("PICKSLIP_ID", pickslipId);


            return _db.ExecuteSingle(QUERY_PICKSLIP_INFO, binder);

        }


        public void PrintPackingSlip(long pickslipId, bool printMasterPackingslip, bool printPackingSlip, bool printAllPackingslip, string printerid, int numberOfCopies = 1)
        {
            const string QUERY = @"
DECLARE

  CURSOR BOX_CUR IS
    SELECT BOX.UCC128_ID
      FROM <proxy />BOX
     WHERE BOX.Pickslip_Id = :apickslip_id
    <if c='$PrintAllPackingslip=""1""'> AND BOX.UCC128_ID = :UCC128_ID </if>
     ORDER BY LPAD((BOX.BOX_ID), 4, 0) DESC;

  Lresult number;
BEGIN
    <if c='$PrintPackingSlip=""1""'>
  FOR BOX_REC IN BOX_CUR LOOP
    Lresult := <proxy />pkg_print_pack.write_pspb_to_file(aucc128_id    =&gt; BOX_REC.UCC128_ID,
                                                 aprinter_name =&gt; :aprinter_name,
                                                 acopies =&gt; :acopies,
                                                 aoptions      =&gt; null);
  
  END LOOP;
</if>
 <if c='$PrintMasterPackingslip=""1""'>
  Lresult :=<proxy />pkg_print_pack.write_mps_to_file(apickslip_id =&gt; :apickslip_id,
                                              aprinter_name =&gt; :aprinter_name,
                                              acopies =&gt; :acopies,
                                              aoptions =&gt; null);
</if>

END;
            ";

            var binder = SqlBinder.Create().Parameter("apickslip_id", pickslipId)
                .Parameter("PrintAllPackingslip", printAllPackingslip == false ? "1" : "0")
                .Parameter("PrintPackingSlip", printPackingSlip == true ? "1" : "0")
                .Parameter("aprinter_name", printerid)
                .Parameter("acopies", numberOfCopies)
                .Parameter("PrintMasterPackingslip", printMasterPackingslip == true ? "1" : "0");

            _db.ExecuteNonQuery(QUERY, binder);


        }

        /// <summary>
        /// returns the inventory of the passed pickslip
        /// </summary>
        /// <param name="pickslipId"></param>
        /// <returns></returns>
        public IList<PickslipSku> GetPickslipSku(long pickslipId)
        {
            Contract.Assert(_db != null);
            const string QUERY_PICKSLIP_INVENTORY = @"
                  SELECT 
                         PSD.SKU_ID         AS SKU_ID,
                         MSKU.STYLE         AS STYLE,
                         MSKU.COLOR         AS COLOR,
                         MSKU.DIMENSION     AS DIMENSION,
                         MSKU.SKU_SIZE      AS SKU_SIZE,
                         PS.VWH_ID          AS VWH_ID,
                         PSD.PIECES_ORDERED AS PIECES_ORDERED,
                         PSD.QUALITY_CODE   AS QUALITY_CODE,
                        (MSKU.RETAIL_PRICE * PSD.PIECES_ORDERED) AS RETAIL_PRICE,
                        PSD.MIN_PIECES_PER_BOX AS MIN_PIECES_PER_BOX,
                        PSD.MAX_PIECES_PER_BOX AS MAX_PIECES_PER_BOX,
                        PSD.PIECES_PER_PACKAGE AS PIECES_PER_PACKAGE
                    FROM <proxy />PSDET PSD
                   INNER JOIN <proxy />MASTER_SKU MSKU
                      ON MSKU.UPC_CODE = PSD.UPC_CODE
                   INNER JOIN <proxy />PS PS 
                   ON PS.PICKSLIP_ID=PSD.PICKSLIP_ID
                   WHERE PSD.PICKSLIP_ID = :PICKSLIP_ID
                   ORDER BY STYLE, COLOR, DIMENSION, SKU_SIZE
            ";

            var binder = SqlBinder.Create(row => new PickslipSku
            {
                Style = row.GetString("STYLE"),
                Color = row.GetString("COLOR"),
                Dimension = row.GetString("DIMENSION"),
                SkuSize = row.GetString("SKU_SIZE"),
                SkuId = row.GetInteger("SKU_ID").Value,
                Pieces = row.GetInteger("PIECES_ORDERED"),
                QualityCode = row.GetString("QUALITY_CODE"),
                VwhId = row.GetString("VWH_ID"),
                RetailPrice = row.GetDecimal("RETAIL_PRICE"),
                PiecesPerPackage = row.GetInteger("PIECES_PER_PACKAGE").Value,
                MaxPiecesPerBox = row.GetInteger("MAX_PIECES_PER_BOX"),
                MinPiecesPerBox = row.GetInteger("MIN_PIECES_PER_BOX")
            }).Parameter("PICKSLIP_ID", pickslipId);
            return _db.ExecuteReader(QUERY_PICKSLIP_INVENTORY, binder);

        }

        public IList<Tuple<string, string>> GetPackingSlipPrinters()
        {
            Contract.Assert(_db != null);
            const string QUERY = @"    
    SELECT NAME AS NAME, DESCRIPTION AS DESCRIPTION
      FROM <proxy />TAB_PRINTER
     WHERE UPPER(PRINTER_TYPE) IN
           (SELECT UPPER(PRINTER_TYPE)
              FROM <proxy />DOC
             WHERE DOCUMENT_ID IN ('$MPS', '$PSPB'))
     ORDER BY NAME ASC
              ";

            var binder = SqlBinder.Create(row => Tuple.Create(row.GetString("NAME"), row.GetString("DESCRIPTION")));
            return _db.ExecuteReader(QUERY, binder);
        }

        public IList<PickslipHeadline> GetInOrderBucketPickslipsOfPo(string poId, string customerId)
        {
            Contract.Assert(_db != null);
            const string QUERY_PICKSLIP_INFO = @"
             SELECT PS.PICKSLIP_ID               AS PICKSLIP_ID,
                    PS.EXPORT_FLAG               AS EXPORT_FLAG,
                    PS.PICKSLIP_CANCEL_DATE      AS PICKSLIP_CANCEL_DATE,
                    PS.PICKSLIP_IMPORT_DATE      AS PICKSLIP_IMPORT_DATE,
                    PS.TOTAL_QUANTITY_ORDERED    AS TOTAL_QUANTITY_ORDERED,
                    PS.UPLOAD_DATE               AS UPLOAD_DATE,
                    PS.CANCEL_DATE               AS CANCEL_DATE,
                    PS.DC_CANCEL_DATE            AS DC_CANCEL_DATE,
                    PS.DELIVERY_DATE             AS DELIVERY_DATE,
                    CUST.NAME                    AS NAME,
                    CUST.CUSTOMER_ID              AS CUSTOMER_ID,
                    PS.CUSTOMER_ORDER_ID          AS PO
                FROM <proxy />DEM_PICKSLIP PS
                LEFT OUTER JOIN <proxy />CUST CUST
                ON CUST.CUSTOMER_ID = PS.CUSTOMER_ID
                WHERE
                PS.PS_STATUS_ID = 1
                AND PS.CUSTOMER_ORDER_ID = :CUSTOMER_ORDER_ID AND PS.CUSTOMER_ID = :CUSTOMER_ID   
        ";
            var binder = SqlBinder.Create(row => new PickslipHeadline
            {

                PickslipId = row.GetLong("PICKSLIP_ID").Value,
                PickslipCancelDate = row.GetDate("PICKSLIP_CANCEL_DATE"),
                ImportDate = row.GetDate("PICKSLIP_IMPORT_DATE"),
                TotalQuantityOrdered = row.GetInteger("TOTAL_QUANTITY_ORDERED") ?? 0,
                ExportFlag = !string.IsNullOrWhiteSpace(row.GetString("EXPORT_FLAG")) ? "Yes" : "No",
                TransferDate = row.GetDate("UPLOAD_DATE"),
                CustomerId = row.GetString("CUSTOMER_ID"),
                CustomerName = row.GetString("NAME"),
                CancelDate = row.GetDate("CANCEL_DATE"),
                DcCancelDate = row.GetDate("DC_CANCEL_DATE"),
                StartDate = row.GetDate("DELIVERY_DATE"),
                PoId = row.GetString("PO"),         
            }).Parameter("CUSTOMER_ORDER_ID", poId)
                .Parameter("CUSTOMER_ID", customerId);

            return _db.ExecuteReader(QUERY_PICKSLIP_INFO, binder);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="poId">Item1 is customer id, Item 2 is po id</param>
        /// <param name="customerId"></param>
        /// <param name="po"></param>
        /// <param name="pickslipId"></param>
        /// <returns></returns>
        public Pickslip GetInOrderBucketPickslip(long pickslipId)
        {
            Contract.Assert(_db != null);
            const string QUERY_PICKSLIP_INFO = @"
             SELECT PS.PICKSLIP_ID               AS PICKSLIP_ID,
                    PS.EXPORT_FLAG               AS EXPORT_FLAG,
                    PS.ASN_FLAG                  AS ASN_FLAG,
                    PS.PICKSLIP_CANCEL_DATE      AS PICKSLIP_CANCEL_DATE,
                    PS.PICKSLIP_IMPORT_DATE      AS PICKSLIP_IMPORT_DATE,
                    PS.TOTAL_QUANTITY_ORDERED    AS TOTAL_QUANTITY_ORDERED,
                    PS.UPLOAD_DATE               AS UPLOAD_DATE,
                    PS.CUSTOMER_STORE_ID         AS CUSTOMER_STORE_ID,
                    PS.CUSTOMER_DIST_CENTER_ID   AS CUSTOMER_DIST_CENTER_ID,
                    PS.CANCEL_DATE               AS CANCEL_DATE,
                    PS.DC_CANCEL_DATE            AS DC_CANCEL_DATE,
                    PS.DELIVERY_DATE             AS DELIVERY_DATE,
                    CUST.NAME                    AS NAME,
                   CUST.CUSTOMER_ID              AS CUSTOMER_ID,
                   PS.CUSTOMER_ORDER_ID          AS PO,
                   PS.CUSTOMER_DEPARTMENT_ID     AS DEPARTMENT,
                   PS.VENDOR_NUMBER              AS VENDOR_NUMBER,
                   MA.ADDRESS_1                  AS SHIPPING_ADDRESS_1,
                   MA.ADDRESS_2                  AS SHIPPING_ADDRESS_2,
                   MA.ADDRESS_3                  AS SHIPPING_ADDRESS_3,
                   MA.ADDRESS_4                  AS SHIPPING_ADDRESS_4,
                   MA.CITY                       AS SHIPPING_ADDRESS_CITY,
                   MA.STATE                      AS SHIPPING_ADDRESS_STATE,
                   MA.ZIP_CODE                   AS SHIPPING_ADDRESS_ZIP_CODE,
                   MA.COUNTRY_CODE               AS SHIPPING_ADDRESS_COUNTRY_CODE,                   
                   PS.ERP_ID                     AS ERP_ID
                FROM <proxy />DEM_PICKSLIP PS
                LEFT OUTER JOIN <proxy />CUST CUST
                ON CUST.CUSTOMER_ID = PS.CUSTOMER_ID
                LEFT OUTER JOIN <PROXY />MASTER_ADDRESS MA 
                ON PS.SHIPPING_ADDRESS_ID = MA.ADDRESS_ID
                WHERE
PS.PS_STATUS_ID = 1
AND PS.PICKSLIP_ID = :pickslip_id
        ";
            var binder = SqlBinder.Create(row => new Pickslip
            {

                PickslipId = row.GetLong("PICKSLIP_ID").Value,
                PickslipCancelDate = row.GetDate("PICKSLIP_CANCEL_DATE"),
                ImportDate = row.GetDate("PICKSLIP_IMPORT_DATE"),
                TotalQuantityOrdered = row.GetInteger("TOTAL_QUANTITY_ORDERED") ?? 0,
                CustomerStore = row.GetString("CUSTOMER_STORE_ID"),
                CustomerDC = row.GetString("CUSTOMER_DIST_CENTER_ID"),
                ExportFlag = row.GetString("EXPORT_FLAG"),
                AsnFlag = !string.IsNullOrEmpty(row.GetString("ASN_FLAG")),
                TransferDate = row.GetDate("UPLOAD_DATE"),
                CustomerDepartmentId = row.GetString("DEPARTMENT"),
                VendorNumber = row.GetString("VENDOR_NUMBER"),
                ErpId = row.GetString("ERP_ID"),
                CustomerId = row.GetString("CUSTOMER_ID"),
                CustomerName = row.GetString("NAME"),
                CancelDate = row.GetDate("CANCEL_DATE"),
                DcCancelDate = row.GetDate("DC_CANCEL_DATE"),
                StartDate = row.GetDate("DELIVERY_DATE"),
                PoId = row.GetString("PO"),
                ShipAddress = new[] {
                         row.GetString("SHIPPING_ADDRESS_1"),
                         row.GetString("SHIPPING_ADDRESS_2"),
                         row.GetString("SHIPPING_ADDRESS_3"),
                         row.GetString("SHIPPING_ADDRESS_4")
                     },
                ShipCity = row.GetString("SHIPPING_ADDRESS_CITY"),
                ShipCountry = row.GetString("SHIPPING_ADDRESS_COUNTRY_CODE"),
                ShipState = row.GetString("SHIPPING_ADDRESS_STATE"),
                ShipZipCode = row.GetString("SHIPPING_ADDRESS_ZIP_CODE")
            }).Parameter("pickslip_id", pickslipId);

            return _db.ExecuteSingle(QUERY_PICKSLIP_INFO, binder);
        }

        public IList<PickslipSku> GetSkuOfTransferPickslip(long pickslipId)
        {
            Contract.Assert(_db != null);
            const string QUERY_TRANSFER_PICKSLIP_DETAIL = @"
            SELECT 
                   MAX(MSKU.SKU_ID)            AS SKU_ID,
                   PS.VWH_ID                   AS VWH_ID,
                   DEMPS.STYLE                 AS STYLE,
                   DEMPS.COLOR                 AS COLOR,
                   DEMPS.DIMENSION             AS DIMENSION,
                   DEMPS.SKU_SIZE              AS SKU_SIZE,
                   DEMPS.QUALITY_CODE          AS QUALITY_CODE,
                   SUM(DEMPS.QUANTITY_ORDERED) AS QUANTITY_ORDERED,
                   MAX(DEMPS.MIN_PIECES_PER_BOX) AS MIN_PIECES_PER_BOX,
                   MAX(DEMPS.MAX_PIECES_PER_BOX) AS MAX_PIECES_PER_BOX,
                   MAX(DEMPS.PIECES_PER_PACKAGE) AS PIECES_PER_PACKAGE
              FROM <proxy />DEM_PICKSLIP_DETAIL DEMPS
              LEFT OUTER JOIN <proxy />MASTER_SKU MSKU
                ON MSKU.STYLE = DEMPS.STYLE
               AND MSKU.COLOR = DEMPS.COLOR
               AND MSKU.DIMENSION = DEMPS.DIMENSION
               AND DEMPS.SKU_SIZE = MSKU.SKU_SIZE             
             INNER JOIN <proxy />DEM_PICKSLIP PS
                ON PS.PICKSLIP_ID = DEMPS.PICKSLIP_ID
             WHERE DEMPS.PICKSLIP_ID = :PICKSLIP_ID
             GROUP BY DEMPS.STYLE,
                      DEMPS.COLOR,
                      DEMPS.DIMENSION,
                      DEMPS.SKU_SIZE,
                      DEMPS.QUALITY_CODE,
                      PS.VWH_ID
             ORDER BY DEMPS.STYLE, DEMPS.COLOR, DEMPS.DIMENSION, DEMPS.SKU_SIZE
            ";
            var binder = SqlBinder.Create(row => new PickslipSku
            {
                Style = row.GetString("STYLE"),
                Color = row.GetString("COLOR"),
                Dimension = row.GetString("DIMENSION"),
                SkuSize = row.GetString("SKU_SIZE"),
                SkuId = row.GetInteger("SKU_ID") ?? 0,
                //Upc = row.GetString("UPC_CODE"),
                Pieces = row.GetInteger("QUANTITY_ORDERED"),
                QualityCode = row.GetString("QUALITY_CODE"),
                VwhId = row.GetString("VWH_ID"),
                PiecesPerPackage = row.GetInteger("PIECES_PER_PACKAGE").Value,
                MaxPiecesPerBox = row.GetInteger("MAX_PIECES_PER_BOX"),
                MinPiecesPerBox = row.GetInteger("MIN_PIECES_PER_BOX")
            }).Parameter("PICKSLIP_ID", pickslipId);

            return _db.ExecuteReader(QUERY_TRANSFER_PICKSLIP_DETAIL, binder);
        }

        /// <summary>
        /// Function return the PO info.
        /// </summary>
        /// <param name="poId"></param>
        /// <param name="customerId"></param>
        /// <param name="iteration"></param>
        /// <returns></returns>
        public PurchaseOrder GetPo(string poId, string customerId, int iteration)
        {
            Contract.Assert(_db != null);
            const string QUERY_PO_INFO = @"
                SELECT PO.PO_ID               AS PO_ID,
                       PO.CUSTOMER_ID         AS CUSTOMER_ID,
                       PO.ITERATION           AS ITERATION,  
                       MAX(CUST.NAME)         AS NAME,
                       MAX(PO.ORDER_DATE)     AS ORDER_DATE,
                       MIN(PO.START_DATE)     AS START_DATE,
                       MIN(PO.CANCEL_DATE)    AS CANCEL_DATE,
                       MIN(PO.DC_CANCEL_DATE) AS DC_CANCEL_DATE,                       
                       COUNT(DISTINCT CASE
                               WHEN B.LAST_UCC_PRINT_DATE IS NOT NULL THEN
                                B.UCC128_ID
                             END) AS NUMBER_OF_UCC_PRINTED,
                       COUNT(DISTINCT CASE
                               WHEN B.LAST_CCL_PRINT_DATE IS NOT NULL THEN
                                B.UCC128_ID
                             END) AS NUMBER_OF_CCL_PRINTED,
                       COUNT(DISTINCT B.UCC128_ID) AS TOTAL_BOXES,
COUNT(UNIQUE PO.ITERATION) OVER() AS COUNT_ITERATIONS
                  FROM <proxy />PO PO
                  LEFT OUTER JOIN <proxy />PS PS
                    ON PS.PO_ID = PO.PO_ID
                   AND PS.CUSTOMER_ID = PO.CUSTOMER_ID
                   AND PS.ITERATION = PO.ITERATION
                  LEFT OUTER JOIN <proxy />BOX B 
                   ON PS.PICKSLIP_ID = B.PICKSLIP_ID
                 INNER JOIN <proxy />CUST CUST
                    ON CUST.CUSTOMER_ID = PO.CUSTOMER_ID
                 WHERE PO.PO_ID = :PO_ID
                   AND PO.CUSTOMER_ID = :CUSTOMER_ID
                   --AND PO.ITERATION = :ITERATION
                 GROUP BY PO.CUSTOMER_ID, PO.PO_ID, PO.ITERATION
ORDER BY case when po.iteration = :ITERATION THEN :ITERATION END NULLS LAST
        ";
            var binder = SqlBinder.Create(row => new PurchaseOrder
            {
                PoId = row.GetString("PO_ID"),
                //PSCount = row.GetInteger("PS_COUNT"),
                OrderDate = row.GetDate("ORDER_DATE"),
                StartDate = row.GetDate("START_DATE"),
                CancelDate = row.GetDate("CANCEL_DATE"),
                DcCancelDate = row.GetDate("DC_CANCEL_DATE"),

                CustomerId = row.GetString("CUSTOMER_ID"),
                CustomerName = row.GetString("NAME"),

                Iteration = row.GetInteger("ITERATION") ?? 0,
                CountOfCclPrinted = row.GetInteger("NUMBER_OF_CCL_PRINTED") ?? 0,
                CountOfUccPrinted = row.GetInteger("NUMBER_OF_UCC_PRINTED") ?? 0,
                TotalBoxes = row.GetInteger("TOTAL_BOXES") ?? 0,
                CountIterations = row.GetInteger("COUNT_ITERATIONS") ?? 0
            }).Parameter("PO_ID", poId)
            .Parameter("CUSTOMER_ID", customerId)
            .Parameter("ITERATION", iteration);
            return _db.ExecuteSingle(QUERY_PO_INFO, binder);

        }

        /// <summary>
        /// All filter parameters are optional. Qualifying pickslips are sorted by touch date descending and then top maxRows pickslips are returned.
        /// </summary>
        /// <param name="poId"></param>
        /// <param name="customerId"></param>
        /// <param name="iteration"></param>
        /// <param name="maxRows"></param>
        /// <returns></returns>
        public IList<PickslipHeadline> GetPickslips(string poId, string customerId, int? iteration, int maxRows)
        {
            Contract.Assert(_db != null);
            const string QUERY = @"
             SELECT                 PS.PICKSLIP_ID                          AS PICKSLIP_ID,
                                    MAX(PS.PICKSLIP_CANCEL_DATE)            AS PICKSLIP_CANCEL_DATE,
                                    MAX(PS.PICKSLIP_IMPORT_DATE)            AS PICKSLIP_IMPORT_DATE,
                                    MAX(PS.EXPORT_FLAG)                     AS EXPORT_FLAG,
                                    MAX(PS.TRANSFER_DATE)                   AS TRANSFER_DATE,
                                    MAX(SHIP.SHIPPING_ID)                   AS SHIPPING_ID,
                                    MAX(SHIP.SHIP_DATE)                     AS SHIP_DATE,
                                    MAX(SHIP.SHIPPER_NAME)                  AS SHIPPER_NAME,
                                    MAX(PS.TOTAL_QUANTITY_ORDERED)          AS TOTAL_QUANTITY_ORDERED,
                                    SUM(CASE
                                        WHEN B.STOP_PROCESS_DATE IS NOT NULL OR B.VERIFY_DATE IS NOT NULL THEN
                                            BD.EXPECTED_PIECES
                                        END)                                AS EXPECTED_PIECES,
                                    SUM(CASE 
                                        WHEN B.STOP_PROCESS_REASON != '$BOXCANCEL' OR
                                            B.STOP_PROCESS_REASON IS NULL  THEN
                                            BD.CURRENT_PIECES 
                                        END)                                AS CURRENT_PIECES,
                                    MAX(SHIP.ONHOLD_FLAG)                   AS ONHOLD_FLAG,
                                    max(b.verify_date)                      as verify_date
                             FROM <proxy />PS PS                
                               LEFT OUTER JOIN <proxy />BOX B ON 
                               PS.PICKSLIP_ID = B.PICKSLIP_ID
                               AND B.STOP_PROCESS_DATE IS NULL                                
                               LEFT OUTER JOIN <proxy />BOXDET BD ON 
                               B.UCC128_ID = BD.UCC128_ID
                               AND B.PICKSLIP_ID = BD.PICKSLIP_ID
                               AND BD.STOP_PROCESS_DATE IS NULL
                              LEFT OUTER JOIN <proxy />SHIP SHIP
                                ON SHIP.SHIPPING_ID = PS.SHIPPING_ID
                                AND SHIP.PARTITION_UPLOAD_DATE = TO_DATE(1,'J')
                            WHERE 
                                <if>
                                    PS.PO_ID = :PO_ID
                                    AND PS.CUSTOMER_ID = :CUSTOMER_ID
                                    AND PS.ITERATION = :ITERATION                   
                                </if>
                                <else>
                                    PS.TRANSFER_DATE IS NULL
                                </else>
                          GROUP BY PS.PICKSLIP_ID
                          ORDER BY GREATEST(MAX(B.TRUCK_LOAD_DATE),
                                            MAX(B.LAST_CCL_PRINT_DATE),
                                            MAX(B.PITCHING_END_DATE),
                                            MAX(B.VERIFY_DATE)) DESC NULLS LAST
            ";

            var binder = SqlBinder.Create(row => new PickslipHeadline
            {
                PickslipId = row.GetLong("PICKSLIP_ID").Value,
                ShipDate = row.GetDate("ship_date"),
                PickslipCancelDate = row.GetDate("PICKSLIP_CANCEL_DATE"),
                TransferDate = row.GetDate("TRANSFER_DATE"),
                ShippingId = row.GetString("SHIPPING_ID"),
                TotalQuantityOrdered = row.GetInteger("TOTAL_QUANTITY_ORDERED"),
                ExportFlag = row.GetString("EXPORT_FLAG"),
                ShipperName = row.GetString("SHIPPER_NAME"),
                ExpectedPieces = row.GetInteger("EXPECTED_PIECES"),
                CurrentPieces = row.GetInteger("CURRENT_PIECES"),
                ImportDate = row.GetDate("PICKSLIP_IMPORT_DATE"),
                ShipmentOnHold = row.GetString("ONHOLD_FLAG"),
                ValidationDate = row.GetDate("verify_date")
            }).Parameter("PO_ID", poId)
            .Parameter("CUSTOMER_ID", customerId)
            .Parameter("ITERATION", iteration);
            return _db.ExecuteReader(QUERY, binder, maxRows);

        }


//        public Wave GetWaveInfo(int waveId)
//        {
//            Contract.Assert(_db != null);
//            const string QUERY_WAVE_INFO = @"
//               SELECT BKT.BUCKET_ID AS BUCKET_ID,
//       MAX(BKT.NAME) AS NAME,
//       MAX(BKT.BUCKET_COMMENT) AS BUCKET_COMMENT,
//       MAX(IA.SHORT_DESCRIPTION) AS AREA_DESCRIPTION,
//       MAX(BKT.PITCH_LIMIT) AS PITCH_LIMIT,
//       MAX(BKT.PITCH_TYPE) AS PITCH_TYPE,
//       MAX(BKT.DATE_CREATED) AS DATE_CREATED,
//       MAX(BKT.CREATED_BY) AS CREATED_BY,
//       MAX(BKT.FREEZE) AS FREEZE,
//       MAX(BKT.AVAILABLE_FOR_PITCHING) AS AVAILABLE_FOR_PITCHING,
//       MAX(BKT.STATUS) AS BUCKET_STATUS,
//       MAX(BKT.PULL_TO_DOCK) AS PULL_TO_DOCK,
//       MAX(BKT.PICK_MODE) AS PICK_MODE,
//       MAX(C.NAME) AS CUSTOMER_NAME,
//       MAX(TWL.DESCRIPTION) AS BUILDING,
//       MAX(P.EXPORT_FLAG) AS EXPORT_FLAG,
//       COUNT(DISTINCT BOX.UCC128_ID) AS TOTAL_BOXES,
//       COUNT(DISTINCT CASE
//               WHEN (BOX.IA_ID) IS NULL THEN
//                (BOX.UCC128_ID)
//             END) AS NON_PHYSICAL_BOX_COUNT,
//       COUNT(DISTINCT CASE
//               WHEN BOX.IA_ID = 'RED' THEN
//                BOX.UCC128_ID
//             END) AS RED_BOX_COUNT,
//       COUNT(DISTINCT CASE
//               WHEN BOX.CARTON_ID IS NOT NULL THEN
//                BOX.UCC128_ID
//             END) AS PULLABLE_BOX_COUNT,
//       COUNT(DISTINCT CASE
//               WHEN BOX.IA_ID IS NULL AND BOX.CARTON_ID IS NOT NULL THEN
//                BOX.CARTON_ID
//             END) AS ABOUT_TO_PULL_BOXES,
//       SUM(CASE
//             WHEN BOX.IA_ID IS NULL THEN
//              BOXDET.EXPECTED_PIECES
//           END) AS UNPROCESSED_PIECES,
//       COUNT(DISTINCT P.PICKSLIP_ID) AS PICKSLIP_COUNT,
//       COUNT(DISTINCT CASE
//               WHEN BOX.IA_ID IS NULL AND BOX.CARTON_ID IS NULL AND
//                    BKT.PICK_MODE = 'PITCHING' THEN
//                BOX.UCC128_ID
//             END) AS PITCHABLE_BOXES,
//       COUNT(DISTINCT CASE
//               WHEN BOXDET.EXPECTED_PIECES = BOXDET.CURRENT_PIECES AND
//                    BOX.PITCHING_END_DATE IS NOT NULL THEN
//                BOX.UCC128_ID
//             END) AS PITCHED_BOXES,
//       COUNT(DISTINCT CASE
//               WHEN BOXDET.CURRENT_PIECES IS NOT NULL AND
//                    BKT.PICK_MODE = 'CHECKING' THEN
//                BOX.UCC128_ID
//             END) AS CHECKED_BOXES
//  FROM <proxy />BUCKET BKT
//  LEFT OUTER JOIN <proxy />CUST C
//    ON C.CUSTOMER_ID = BKT.CUSTOMER_ID
//  LEFT OUTER JOIN <proxy />IA IA
//    ON IA.IA_ID = BKT.PITCH_IA_ID
//  LEFT OUTER JOIN <proxy />PS P
//    ON BKT.BUCKET_ID = P.BUCKET_ID
//  LEFT OUTER JOIN <proxy />BOX BOX
//    ON BOX.PICKSLIP_ID = P.PICKSLIP_ID
//  LEFT OUTER JOIN <proxy />BOXDET BOXDET
//    ON BOXDET.UCC128_ID = BOX.UCC128_ID
//   AND BOXDET.PICKSLIP_ID = BOX.PICKSLIP_ID
//  LEFT OUTER JOIN <proxy />TAB_WAREHOUSE_LOCATION TWL
//    ON P.WAREHOUSE_LOCATION_ID = TWL.WAREHOUSE_LOCATION_ID
// WHERE BKT.BUCKET_ID = :BUCKET_ID
// GROUP BY BKT.BUCKET_ID
//            ";
//            var binder = SqlBinder.Create(row => new Wave
//            {
//                BucketId = row.GetInteger("BUCKET_ID").Value,
//                Name = row.GetString("NAME"),
//                Comment = row.GetString("BUCKET_COMMENT"),
//                PitchArea = row.GetString("AREA_DESCRIPTION"),
//                PitchLimit = row.GetInteger("PITCH_LIMIT").Value,
//                PitchType = row.GetString("PITCH_TYPE"),
//                DateCreated = row.GetDate("DATE_CREATED").Value,
//                CreatedBy = row.GetString("CREATED_BY"),
//                Freeze = !string.IsNullOrEmpty(row.GetString("FREEZE")),
//                AvailableForPitching = !string.IsNullOrEmpty(row.GetString("AVAILABLE_FOR_PITCHING")),
//                Status = row.GetString("BUCKET_STATUS"),
//                CustomerName = row.GetString("CUSTOMER_NAME"),
//                PullToDock = row.GetString("PULL_TO_DOCK"),
//                PickMode = row.GetString("PICK_MODE"),
//                Building = row.GetString("BUILDING"),
//                TotalBoxes = row.GetInteger("TOTAL_BOXES"),
//                NonPhysicalBoxCount = row.GetInteger("NON_PHYSICAL_BOX_COUNT"),
//                RedBoxCount = row.GetInteger("RED_BOX_COUNT"),
//                PullableBoxCount = row.GetInteger("PULLABLE_BOX_COUNT"),
//                AboutToPullBoxCount = row.GetInteger("ABOUT_TO_PULL_BOXES"),
//                UnprocessedPieces = row.GetInteger("UNPROCESSED_PIECES"),
//                PickslipCount = row.GetInteger("PICKSLIP_COUNT"),
//                PitchableBoxes = row.GetInteger("PITCHABLE_BOXES"),
//                PitchedBoxes = row.GetInteger("PITCHED_BOXES"),
//                CheckedBoxes = row.GetInteger("CHECKED_BOXES"),
//                ExportFlag = !string.IsNullOrEmpty(row.GetString("EXPORT_FLAG"))
//            }).Parameter("BUCKET_ID", waveId);

//            return _db.ExecuteSingle(QUERY_WAVE_INFO, binder);
//        }

   public Wave GetWaveInfo(int waveId)
        {
            Contract.Assert(_db != null);
            const string QUERY_WAVE_INFO = @"WITH BUCKET_PS_INFO AS
 (SELECT BKT.BUCKET_ID,
         MAX(BKT.NAME) AS BUCKET_NAME,
         MAX(BKT.CUSTOMER_ID) AS CUSTOMER_ID,
         MAX(C.NAME) AS CUSTOMER_NAME,
         MAX(PS.WAREHOUSE_LOCATION_ID) AS BUILDING_pitch,
         MAX(IA.SHORT_NAME) AS Pitch_Area,
         MAX(IA.SHORT_DESCRIPTION) AS Pitch_AREA_DESCRIPTION,
         MAX(TIA.WAREHOUSE_LOCATION_ID) AS BUILDING_PULL_FROM,
         MAX(TIA.SHORT_NAME) AS PULL_AREA,
         MAX(TIA.DESCRIPTION) AS PULL_AREA_DESCRIPTION,
         MAX(BKT.DATE_CREATED) AS DATE_CREATED,
         MAX(BKT.STATUS) AS BUCKET_STATUS,
         MAX(BKT.CREATED_BY) AS CREATED_BY,
         MAX(BKT.PULL_TO_DOCK) AS PULL_TO_DOCK,  
         MAX(BKT.FREEZE) AS FREEZE,
         MAX(BKT.PICK_MODE) AS PICK_MODE,
         SUM(PD.PIECES_ORDERED) AS TOTAL_QUANTITY_ORDERED,
         MIN(PO.DC_CANCEL_DATE) AS MIN_DC_CANCEL_DATE,
         MIN(BKT.PRIORITY) AS PRIORITY,
         COUNT(UNIQUE PS.PICKSLIP_ID) AS COUNT_OF_PICKSLIPS,
         COUNT(UNIQUE PO.PO_ID) AS COUNT_OF_POS,
         MAX(PS.EXPORT_FLAG) AS EXPORT_FLAG,
         COUNT(UNIQUE PD.SKU_ID) AS COUNT_TOTAL_SKU,
         COUNT(UNIQUE (SELECT MAX(IL.ASSIGNED_UPC_CODE)
                  FROM IALOC IL
                 WHERE IL.ASSIGNED_UPC_CODE = PD.UPC_CODE
                   AND IL.VWH_ID = PS.VWH_ID
                   AND IL.IA_ID = BKT.PITCH_IA_ID)) AS COUNT_ASSIGNED_SKU
    FROM BUCKET BKT
   LEFT OUTER JOIN PS
      ON PS.BUCKET_ID = BKT.BUCKET_ID
   LEFT OUTER JOIN PSDET PD
      ON PS.PICKSLIP_ID = PD.PICKSLIP_ID
    LEFT OUTER JOIN IA
      ON BKT.PITCH_IA_ID = IA.IA_ID
    LEFT OUTER JOIN TAB_INVENTORY_AREA TIA
      ON TIA.INVENTORY_STORAGE_AREA = BKT.PULL_CARTON_AREA
    LEFT OUTER JOIN PO
      ON PS.CUSTOMER_ID = PO.CUSTOMER_ID
     AND PS.PO_ID = PO.PO_ID
     AND PS.ITERATION = PO.ITERATION
    LEFT OUTER JOIN CUST C
      ON C.CUSTOMER_ID = BKT.CUSTOMER_ID
   WHERE BKT.BUCKET_ID = :BUCKET_ID
      --IA.PICKING_AREA_FLAG = 'Y' AND      
      GROUP BY BKT.BUCKET_ID),
BOX_INFO AS
 (SELECT PS.BUCKET_ID,
         COUNT(DISTINCT B.UCC128_ID) AS TOTAL_BOXES,
        CASE
         WHEN Count(UNIQUE B.UCC128_ID) > 0 THEN
          'Y'
         ELSE
          ''
       END AS AVAILABLE_FOR_PITCHING,
         SUM(BD.CURRENT_PIECES) AS PICKED_PIECES,
         COUNT(UNIQUE CASE
                 WHEN B.VERIFY_DATE IS NOT NULL THEN
                  B.UCC128_ID
               END) AS VERIFIED_BOXES,
         COUNT(DISTINCT(CASE
                          WHEN B.IA_ID_COPY = 'NOA' THEN
                           B.UCC128_ID
                        END)) AS COUNT_OF_UNAVAILABLE_BOX
    FROM PS
    LEFT OUTER JOIN BOX B
      ON B.PICKSLIP_ID = PS.PICKSLIP_ID
    LEFT OUTER JOIN BOXDET BD
      ON B.UCC128_ID = BD.UCC128_ID
     AND B.PICKSLIP_ID = BD.PICKSLIP_ID
   WHERE PS.BUCKET_ID = :BUCKET_ID
   GROUP BY PS.BUCKET_ID)
SELECT BPI.BUCKET_ID,
       BPI.BUCKET_NAME,
       BPI.CUSTOMER_ID,
       BPI.CUSTOMER_NAME,
       BPI.BUILDING_pitch,
       BPI.Pitch_Area,
       BPI.Pitch_AREA_DESCRIPTION,
       BPI.BUILDING_PULL_FROM,
       BPI.PULL_AREA,
       BPI.PULL_AREA_DESCRIPTION,
       BPI.DATE_CREATED,
       BPI.CREATED_BY,
       BPI.PULL_TO_DOCK,  
       BPI.FREEZE,
       BPI.PICK_MODE,
       BPI.TOTAL_QUANTITY_ORDERED,
       BPI.MIN_DC_CANCEL_DATE,
       BPI.PRIORITY,
       BPI.COUNT_OF_PICKSLIPS,
       BPI.COUNT_OF_POS,
       BPI.EXPORT_FLAG,
       BPI.COUNT_TOTAL_SKU, 
       BPI.COUNT_ASSIGNED_SKU, 
       BI.TOTAL_BOXES,
       BI.VERIFIED_BOXES,
       BI.PICKED_PIECES,
       BI.COUNT_OF_UNAVAILABLE_BOX,
       BI.AVAILABLE_FOR_PITCHING,
       BPI.BUCKET_STATUS
  FROM BUCKET_PS_INFO BPI
  LEFT OUTER JOIN BOX_INFO BI
    ON BPI.BUCKET_ID = BI.BUCKET_ID
";

            var binder = SqlBinder.Create(row => new Wave
            {
                BucketId = row.GetInteger("BUCKET_ID").Value,
                BucketName = row.GetString("BUCKET_NAME"),
                CustomerId = row.GetString("CUSTOMER_ID"),
                CustomerName = row.GetString("CUSTOMER_NAME"),
                PitchBuilding = row.GetString("BUILDING_pitch"),
                PitchArea = row.GetString("Pitch_Area"),
                PitchAreaDescription = row.GetString("Pitch_AREA_DESCRIPTION"),
                BuildingPullFrom = row.GetString("BUILDING_PULL_FROM"),
                PullArea = row.GetString("PULL_AREA"),
                PullAreaDescription = row.GetString("PULL_AREA_DESCRIPTION"),
                DateCreated = row.GetDate("DATE_CREATED").Value,
                CreatedBy = row.GetString("CREATED_BY"),
                PullToDock = row.GetString("PULL_TO_DOCK"),
                Freeze = !string.IsNullOrEmpty(row.GetString("FREEZE")),
                PickMode = row.GetString("PICK_MODE"),
                TotalQuantityOrdered = row.GetInteger("TOTAL_QUANTITY_ORDERED")??0,
                DcCancelDate = row.GetDate("MIN_DC_CANCEL_DATE"),
                Priority=row.GetInteger("PRIORITY"),           
                AvailableForPitching = !string.IsNullOrEmpty(row.GetString("AVAILABLE_FOR_PITCHING")),
                PickslipCount = row.GetInteger("COUNT_OF_PICKSLIPS") ,                  
                PoCount = row.GetInteger("COUNT_OF_POS"),
                TotalSkuCount = row.GetInteger("COUNT_TOTAL_SKU")??0,
                AssignedSkuCount = row.GetInteger("COUNT_ASSIGNED_SKU")??0, 
                ExportFlag = !string.IsNullOrEmpty(row.GetString("EXPORT_FLAG")),                         
                TotalBoxes = row.GetInteger("TOTAL_BOXES")??0,
                VerifiedBoxes = row.GetInteger("VERIFIED_BOXES")??0,
                PickedPieces = row.GetInteger("PICKED_PIECES")??0,
                UnavailableBoxCount = row.GetInteger("COUNT_OF_UNAVAILABLE_BOX"),
                BucketStatus=row.GetString("BUCKET_STATUS")
               
            }).Parameter("BUCKET_ID", waveId);

            return _db.ExecuteSingle(QUERY_WAVE_INFO, binder);
        }



        
        internal IList<BoxHeadline> GetBoxes(long pickslipId, int maxRows)
        {
            return SharedRepository.GetBoxes(_db, pickslipId, null, maxRows);
        }

        public IList<WaveHeadline> GetWaveList(int maxRows)
        {
            const string QUERY = @"
                     WITH BUCKET_INFO AS
 (SELECT BKT.BUCKET_ID             AS BUCKET_ID,
         BKT.NAME                  AS BUCKET_DESCRIPTION,
         BKT.DATE_CREATED          AS DATE_CREATED,
         BKT.FREEZE                AS IS_BUCKET_FROZEN,
         BKT.STATUS                AS BUCKET_STATUS,
         BKT.PICK_MODE             AS BUCKET_PICKMODE,
         BKT.PRIORITY              AS BUCKET_PRIORITY,
         IA.SHORT_NAME             AS PITCH_AREA,
         IA.SHORT_DESCRIPTION      AS PITCH_AREA_DESCRIPTION,
         TIA.WAREHOUSE_LOCATION_ID AS BUILDING_PULL_FROM,
         TIA.SHORT_NAME            AS PULL_AREA,
         TIA.DESCRIPTION           AS PULL_AREA_DESCRIPTION
    FROM BUCKET BKT
    LEFT OUTER JOIN IA
      ON BKT.PITCH_IA_ID = IA.IA_ID
    LEFT OUTER JOIN TAB_INVENTORY_AREA TIA
      ON BKT.PULL_CARTON_AREA = TIA.INVENTORY_STORAGE_AREA),
ORDER_INFO AS
 (SELECT PS.BUCKET_ID AS BUCKET_ID,
         COUNT(PS.PICKSLIP_ID) AS PICKSLIP_COUNT,
         COUNT(DISTINCT PS.PO_ID) AS PO_COUNT,
         MAX(PS.CUSTOMER_ID) AS CUSTOMER_ID,
         MAX(C.NAME) AS CUSTOMER_NAME,
         MAX(PS.WAREHOUSE_LOCATION_ID) AS BUILIDING_PITCHED
    FROM PS
    LEFT OUTER JOIN CUST C
      ON C.CUSTOMER_ID = PS.CUSTOMER_ID
   WHERE PS.TRANSFER_DATE IS NULL
   GROUP BY PS.BUCKET_ID)
SELECT BI.BUCKET_ID              AS BUCKET_ID,
       BI.BUCKET_DESCRIPTION     AS BUCKET_NAME,
       BI.DATE_CREATED           AS DATE_CREATED,
       BI.IS_BUCKET_FROZEN       AS IS_BUCKET_FROZEN,
       BI.BUCKET_STATUS          AS BUCKET_STATUS,
       BI.BUCKET_PRIORITY        AS PRIORITY,
       OI.CUSTOMER_ID            AS CUSTOMER_ID,
       OI.CUSTOMER_NAME          AS CUSTOMER_NAME,
       OI.PICKSLIP_COUNT         AS PICKSLIP_COUNT,
       OI.PO_COUNT               AS PO_COUNT,
       OI.BUILIDING_PITCHED      AS BUILIDING_PITCHED,
       BI.PITCH_AREA             AS PITCH_AREA,
       BI.PITCH_AREA_DESCRIPTION AS PITCH_AREA_DESCRIPTION,
       BI.BUILDING_PULL_FROM     AS BUILDING_PULL_FROM,
       BI.PULL_AREA              AS PULL_AREA,
       BI.PULL_AREA_DESCRIPTION  AS PULL_AREA_DESCRIPTION
  FROM BUCKET_INFO BI
 INNER JOIN ORDER_INFO OI
    ON OI.BUCKET_ID = BI.BUCKET_ID
 ORDER BY BUCKET_PRIORITY DESC, DATE_CREATED
";

            var binder = SqlBinder.Create(row => new WaveHeadline
            {
                BucketId = row.GetInteger("BUCKET_ID").Value,
                BucketName = row.GetString("BUCKET_NAME"),
                DateCreated = row.GetDate("DATE_CREATED").Value,
                Freeze = !string.IsNullOrEmpty(row.GetString("IS_BUCKET_FROZEN")),
                BucketStatus = row.GetString("BUCKET_STATUS"),
                Priority = row.GetInteger("PRIORITY"),
                CustomerId = row.GetString("CUSTOMER_ID"),
                CustomerName = row.GetString("CUSTOMER_NAME"),
                PickslipCount = row.GetInteger("PICKSLIP_COUNT"),
                PoCount = row.GetInteger("PO_COUNT"),
                PitchBuilding = row.GetString("BUILIDING_PITCHED"),
                PitchArea = row.GetString("PITCH_AREA"),
                PitchAreaDescription = row.GetString("PITCH_AREA_DESCRIPTION"),
                BuildingPullFrom = row.GetString("BUILDING_PULL_FROM"),
                PullArea = row.GetString("PULL_AREA"),
                PullAreaDescription = row.GetString("PULL_AREA_DESCRIPTION")
            });
            return _db.ExecuteReader(QUERY, binder, maxRows);

        }
        
        public IList<PoHeadline> GetRecentOrders(int maxRows)
        {
            return SharedRepository.GetRecentOrders(_db, null, null, maxRows);
        }


    }
}