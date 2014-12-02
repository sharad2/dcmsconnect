using DcmsMobile.Inquiry.Areas.Inquiry.SharedViews;
using DcmsMobile.Inquiry.Helpers;
using EclipseLibrary.Oracle;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Web;

namespace DcmsMobile.Inquiry.Areas.Inquiry.BoxEntity
{
    internal class BoxEntityRepository : IDisposable
    {
        private readonly OracleDatastore _db;
        public BoxEntityRepository(string userName, string clientInfo)
        {
            _db = new OracleDatastore(HttpContext.Current.Trace);
            _db.CreateConnection(ConfigurationManager.ConnectionStrings["dcms8"].ConnectionString, userName);
            _db.ModuleName = "Inquiry_BoxEntity";
            _db.ClientInfo = clientInfo;
        }

        public void Dispose()
        {
            _db.Dispose();
        }

        public Box GetBoxOfUcc(string uccId)
        {
            if (string.IsNullOrWhiteSpace(uccId))
            {
                throw new ArgumentNullException("uccId");
            }
            Contract.Assert(_db != null);
            const string QUERY_BOX_DETAIL = @"
                        SELECT BOX.UCC128_ID                 AS UCC128_ID,
                               BOX.PICKSLIP_ID          AS PICKSLIP_ID,
                               --PS.PO_ID                 AS PO_ID,
                               BOX.IA_ID                AS IA_ID,
                               IA.SHORT_NAME            AS SHORT_NAME,
                               IA.WAREHOUSE_LOCATION_ID AS WAREHOUSE_LOCATION_ID,
                               IA.SHORT_DESCRIPTION     AS IA_SHORT_DESCRIPTION,
                               BOX.PALLET_ID            AS PALLET_ID,
                               BOX.VWH_ID               AS VWH_ID,
                               BOX.PRO_NUMBER           AS PRO_NUMBER,
                               PS.CUSTOMER_ID           AS CUSTOMER_ID,
                               BOX.STOP_PROCESS_DATE    AS STOP_PROCESS_DATE,
                               BOX.STOP_PROCESS_REASON    AS STOP_PROCESS_REASON,
                               CUST.NAME                AS NAME,
                               --PS.WAREHOUSE_LOCATION_ID AS WAREHOUSE_LOCATION_ID,
                               BOX.PITCHING_END_DATE    AS PITCHING_END_DATE,
                               BOX.VERIFY_DATE          AS VERIFY_DATE,
                               BOX.QC_DATE              AS QC_DATE,
                               PS.IS_RFIDTAGS_REQUIRED  AS IS_RFIDTAGS_REQUIRED,
                               --SUM(NVL(BD.EXPECTED_PIECES, BD.CURRENT_PIECES       AS EXPECTED_PIECES,
                               --SUM(BD.CURRENT_PIECES        AS CURRENT_PIECES,
                               BOX.SUSPENSE_DATE        AS SUSPENSE_DATE,
                               BOX.REJECTION_CODE       AS REJECTION_CODE, 
                               --COUNT(DISTINCT BD.UPC_CODE   AS SKU_IN_BOX,
                               PS.BUCKET_ID            AS BUCKET_ID,
                               BOX.CARTON_ID            AS CARTON_ID,
                               BOX.LAST_CCL_PRINTED_BY  AS LAST_CCL_PRINTED_BY,
                               BOX.LAST_UCC_PRINTED_BY  AS LAST_UCC_PRINTED_BY,
                               BOX.LAST_UCC_PRINT_DATE  AS LAST_UCC_PRINTED_DATE,
                               BOX.LAST_CCL_PRINT_DATE  AS LAST_CCL_PRINT_DATE,
                               PS.CUSTOMER_DC_ID        AS CUSTOMER_DC_ID,
                               PS.CUSTOMER_STORE_ID     AS CUSTOMER_STORE_ID,
                               PS.SHIPPING_ADDRESS.ADDRESS_LINE_1      AS ADDRESS_LINE_1,
                               PS.SHIPPING_ADDRESS.ADDRESS_LINE_2      AS ADDRESS_LINE_2,
                               PS.SHIPPING_ADDRESS.ADDRESS_LINE_3      AS ADDRESS_LINE_3,
                               PS.SHIPPING_ADDRESS.ADDRESS_LINE_4      AS ADDRESS_LINE_4,
                               PS.SHIPPING_ADDRESS.CITY                AS CITY,
                               PS.SHIPPING_ADDRESS.STATE               AS STATE,
                               c.document_id as catalog_document_id
                         FROM <proxy />BOX BOX
                         INNER JOIN <proxy />PS PS
                            ON BOX.PICKSLIP_ID = PS.PICKSLIP_ID
                          LEFT OUTER JOIN <proxy />CUST CUST
                            ON CUST.CUSTOMER_ID = PS.CUSTOMER_ID        
                          left outer join  <proxy />CUSTDOC C 
                            on     PS.CUSTOMER_ID = c.customer_id       
                            AND C.DOCUMENT_ID = '$CL'      
                          LEFT OUTER JOIN <proxy />IA IA
                            ON BOX.IA_ID = IA.IA_ID
                        WHERE  box.ucc128_id = :UCC_ID
                        ";

            var binder = SqlBinder.Create(row => new Box
            {
                Ucc128Id = row.GetString("UCC128_ID"),
                PalletId = row.GetString("PALLET_ID"),
                CartonId = row.GetString("CARTON_ID"),
                IaId = row.GetString("IA_ID"),
                RfidTagsRequired = row.GetString("IS_RFIDTAGS_REQUIRED"),
                QcDate = row.GetDate("QC_DATE"),
                VerificationDate = row.GetDate("VERIFY_DATE"),
                PitchingEndDate = row.GetDate("PITCHING_END_DATE"),
                VwhId = row.GetString("VWH_ID"),
                ProNo = row.GetString("PRO_NUMBER"),
                LastCclPrintedBy = row.GetString("LAST_CCL_PRINTED_BY"),
                LastUccPrintedBy = row.GetString("LAST_UCC_PRINTED_BY"),
                LastCclPrintedDate = row.GetDate("LAST_CCL_PRINT_DATE"),
                LastUccPrintedDate = row.GetDate("LAST_UCC_PRINTED_DATE"),
                RejectionCode = row.GetString("REJECTION_CODE"),
                SuspenseDate = row.GetDate("SUSPENSE_DATE"),
                StopProcessDate = row.GetDate("STOP_PROCESS_DATE"),
                StopProcessReason = row.GetString("STOP_PROCESS_REASON"),
                PickslipId = row.GetLong("PICKSLIP_ID").Value,
                BucketId = row.GetInteger("BUCKET_ID") ?? 0,
                CustomerDC = row.GetString("CUSTOMER_DC_ID"),
                CustomerStore = row.GetString("CUSTOMER_STORE_ID"),
                CustomerId = row.GetString("CUSTOMER_ID"),
                CustomerName = row.GetString("NAME"),
                ShortName = row.GetString("SHORT_NAME"),
                IaShortDescription = row.GetString("IA_SHORT_DESCRIPTION"),
                Building = row.GetString("WAREHOUSE_LOCATION_ID"),
                ToAddress = new[]{
                                row.GetString("ADDRESS_LINE_1"),
                                row.GetString("ADDRESS_LINE_2"),
                                row.GetString("ADDRESS_LINE_3"),
                                row.GetString("ADDRESS_LINE_4")                            
                            },
                ToCity = row.GetString("CITY"),
                ToState = row.GetString("STATE"),
                CatalogDocumentId = row.GetString("catalog_document_id")
            }).Parameter("UCC_ID", uccId);
            return _db.ExecuteSingle(QUERY_BOX_DETAIL, binder);
        }

        public IList<Epc> GetBoxEpc(string uccId)
        {
            Contract.Assert(_db != null);
            const string QUERY_BOX_SKU_EPC = @"
                SELECT bd.SKU_ID AS SKU_ID, 
                        BDEPC.EPC AS EPC
                  FROM <proxy />BOXDET BD
                  LEFT OUTER JOIN <proxy />BOXDET_EPC BDEPC
                    ON BDEPC.BOXDET_ID = BD.BOXDET_ID
                 WHERE BD.UCC128_ID = :aucc_id
                    AND BDEPC.EPC IS NOT NULL
            ";
            var binder = SqlBinder.Create(row => new Epc
            {
                EpcCode = row.GetString("EPC"),
                SkuId = row.GetInteger("SKU_ID").Value
            }).Parameter("aucc_id", uccId);
            return _db.ExecuteReader(QUERY_BOX_SKU_EPC, binder);
        }


        /// <summary>
        /// Function getting the Box Sku Details with pieces count.
        /// </summary>
        /// <param name="uccId"></param>
        /// <param name="epc"></param>
        /// <returns></returns>
        public IList<BoxSku> GetBoxSku(string uccId, string palletId)
        {
            if (string.IsNullOrWhiteSpace(uccId) && string.IsNullOrWhiteSpace(palletId))
            {
                throw new ArgumentNullException("Both uccId and palletId cannot be null");
            }
            if (!string.IsNullOrWhiteSpace(uccId) && !string.IsNullOrWhiteSpace(palletId))
            {
                throw new ArgumentOutOfRangeException("Exactly one of uccId and palletId must be specified");
            }
            Contract.Assert(_db != null);
            const string QUERY_BOX_SKU_DETAIL = @"
            SELECT MAX(MSKU.STYLE)         AS STYLE,
                   MAX(MSKU.COLOR)         AS COLOR,
                   MAX(MSKU.DIMENSION)     AS DIMENSION,
                   MAX(MSKU.SKU_SIZE)      AS SKU_SIZE,
                   
                   MSKU.SKU_ID             AS SKU_ID,
                   MAX(B.VWH_ID)           as VWH_ID, 
                   MAX(BD.EXTENDED_PRICE)  AS EXTENDED_PRICE,
                   SUM(BD.CURRENT_PIECES)  AS CURRENT_PIECES,
                   SUM(BD.EXPECTED_PIECES) AS EXPECTED_PIECES,
                   MIN(BD.LAST_PITCHED_BY)  AS MIN_PITCHER
              FROM <proxy />BOX B
             INNER JOIN <proxy />BOXDET BD
                ON  B.UCC128_ID = BD.UCC128_ID
               AND  B.PICKSLIP_ID = BD.PICKSLIP_ID
             INNER JOIN <proxy />MASTER_SKU MSKU
                ON MSKU.SKU_ID = BD.SKU_ID
             WHERE 
<if>BD.UCC128_ID = :UCC128_ID</if>
<if>B.PALLET_ID = :PALLET_ID</if>
             GROUP BY MSKU.SKU_ID
             ORDER BY MAX(MSKU.STYLE),
                      MAX(MSKU.COLOR),
                      MAX(MSKU.DIMENSION),
                      MAX(MSKU.SKU_SIZE)
            ";

            var binder = SqlBinder.Create(row => new BoxSku
            {
                CurrentPieces = row.GetInteger("CURRENT_PIECES"),
                ExpectedPieces = row.GetInteger("EXPECTED_PIECES"),
                ExtendedPrice = row.GetDecimal("EXTENDED_PRICE"),
                Style = row.GetString("STYLE"),
                Color = row.GetString("COLOR"),
                Dimension = row.GetString("DIMENSION"),
                SkuSize = row.GetString("SKU_SIZE"),
                //Upc = row.GetString("UPC_CODE"),
                SkuId = row.GetInteger("SKU_ID") ?? 0,
                MinPicker = row.GetString("MIN_PITCHER"),
                VwhId = row.GetString("VWH_ID")
            }).Parameter("UCC128_ID", uccId)
                .Parameter("PALLET_ID", palletId);
            return _db.ExecuteReader(QUERY_BOX_SKU_DETAIL, binder);
        }

        public IList<BoxVas> GetVasOnBox(string uccId)
        {
            Contract.Assert(_db != null);
            const string QUERY = @"
                        WITH Q1 AS
                             (SELECT VASREQ.DESCRIPTION AS INCOMPLETE_VAS, VASREQ.VAS_CODE AS VASCODE
                                FROM <proxy />BOX B
                                LEFT OUTER JOIN <proxy />PS_VAS PVAS
                                  ON B.PICKSLIP_ID = PVAS.PICKSLIP_ID
                                LEFT OUTER JOIN <proxy />TAB_VAS VASREQ
                                  ON PVAS.VAS_ID = VASREQ.VAS_CODE
                               WHERE B.UCC128_ID = :UCC128_ID),
                            Q2 AS
                             (SELECT VASCOM.DESCRIPTION AS COMPLETE_VAS, VASCOM.VAS_CODE AS VASCODE
                                FROM <proxy />BOX B
                                LEFT OUTER JOIN <proxy />BOX_VAS BP
                                  ON B.UCC128_ID = BP.UCC128_ID
                                LEFT OUTER JOIN <proxy />TAB_VAS VASCOM
                                  ON BP.BOX_PROCESS_CODE = VASCOM.VAS_CODE
                               WHERE B.UCC128_ID = :UCC128_ID)
                            SELECT 
q1.vascode as vas_required,
q2.vascode as vas_completed,
NVL(Q1.INCOMPLETE_VAS, Q2.COMPLETE_VAS) as VAS_DESCRIPTION
                              FROM Q1
                              FULL OUTER JOIN Q2
                                ON Q1.VASCODE = Q2.VASCODE
where Q1.VASCODE IS NOT NULL OR Q2.VASCODE IS NOT NULL
            ";

            var binder = SqlBinder.Create(row => new BoxVas
                {
                    VasDescription = row.GetString("VAS_DESCRIPTION"),
                    IsComplete = !string.IsNullOrWhiteSpace(row.GetString("vas_completed")),
                    IsRequired = !string.IsNullOrWhiteSpace(row.GetString("vas_required")),
                }).Parameter("UCC128_ID", uccId);

            return _db.ExecuteReader(QUERY, binder);
        }
        //        [Obsolete]
        //        public Box GetVasOnBox(string uccId)
        //        {
        //            Contract.Assert(_db != null);
        //            const string QUERY = @"
        //            SELECT LISTAGG(VASREQ.DESCRIPTION, ',') WITHIN GROUP(ORDER BY VASREQ.DESCRIPTION) AS INCOMPLETE_VAS,
        //                   LISTAGG(VASCOM.DESCRIPTION, ',') WITHIN GROUP(ORDER BY VASCOM.DESCRIPTION) AS COMPLETE_VAS
        //              FROM <proxy />BOX B
        //              LEFT OUTER JOIN <proxy />PS_VAS PVAS
        //                ON B.PICKSLIP_ID = PVAS.PICKSLIP_ID
        //              LEFT OUTER JOIN <proxy />TAB_VAS VASREQ
        //                ON PVAS.VAS_ID = VASREQ.VAS_CODE
        //              LEFT OUTER JOIN <proxy />BOX_VAS BP
        //                ON B.UCC128_ID = BP.UCC128_ID
        //              LEFT OUTER JOIN <proxy />TAB_VAS VASCOM
        //                ON BP.BOX_PROCESS_CODE = VASCOM.VAS_CODE
        //                WHERE B.UCC128_ID = :UCC128_ID
        //            ";
        //            var binder = SqlBinder.Create(row => new Box
        //            {
        //                ListOfCompleteVas = string.Join(", ", row.GetString("Complete_VAS").Split(',').Distinct()),
        //                ListOfIncompleteVas = string.Join(", ", row.GetString("Incomplete_VAS").Split(',').Distinct())
        //            }).Parameter("UCC128_ID", uccId);

        //            return _db.ExecuteSingle(QUERY, binder);
        //        }
        /// <summary>
        /// This Method IS use For boxpalletlist
        /// </summary>
        /// <param name="uccId"></param>
        /// <returns></returns>
        public IList<BoxPalletheadLine> GetBoxPalletList( int maxrows)
        {
            const string QUERY = @"WITH PS_ORDER AS
 (SELECT PS.PICKSLIP_ID          AS PICKSLIP_ID,
         PS.CUSTOMER_ID          AS CUSTOMER_ID,
         MC.NAME                 AS CUSTOMER_NAME,
         PS.PICKSLIP_IMPORT_DATE AS PICKSLIP_IMPORT_DATE
    FROM PS PS
   INNER JOIN MASTER_CUSTOMER MC
      ON MC.CUSTOMER_ID = PS.CUSTOMER_ID)
SELECT B.PALLET_ID AS PALLET_ID,
       COUNT(B.UCC128_ID) AS BOX_COUNT,
       MAX(PRD.CUSTOMER_ID) AS CUSTOMER_ID,
       MAX(PRD.CUSTOMER_NAME) AS CUSTOMER_NAME,
       MIN(IA.SHORT_NAME) AS SHORT_NAME,
       COUNT(DISTINCT B.IA_ID) AS BOX_AREA_COUNT,
       MAX(IA.WAREHOUSE_LOCATION_ID) AS WAREHOUSE_LOCATION_ID
  FROM BOX B
  LEFT OUTER JOIN PS_ORDER PRD
    ON PRD.PICKSLIP_ID = B.PICKSLIP_ID
  LEFT OUTER JOIN IA
    ON IA.IA_ID = B.IA_ID
 WHERE B.PALLET_ID IS NOT NULL
 AND B.STOP_PROCESS_DATE IS NULL
 GROUP BY B.PALLET_ID
 ORDER BY COUNT(B.UCC128_ID) DESC NULLS LAST";
            var binder = SqlBinder.Create(row => new BoxPalletheadLine
                {

                    PalletId=row.GetString("PALLET_ID"),
                    BoxCount = row.GetInteger("BOX_COUNT"),
                    CustomerId = row.GetString("CUSTOMER_ID"),
                    CustomerName = row.GetString("CUSTOMER_NAME"),
                    AreaShortName = row.GetString("SHORT_NAME"),
                    BoxAreaCount = row.GetInteger("BOX_AREA_COUNT"),
                    WarehouseLocationId= row.GetString("WAREHOUSE_LOCATION_ID")
                });
            return _db.ExecuteReader(QUERY, binder, maxrows);
        }
        public IList<BoxAudit> GetBoxProcesssHistory(string uccId)
        {
            Contract.Assert(_db != null);
            const string QUERY = @"
                    SELECT B.ACTION_PERFORMED AS ACTION_PERFORMED,
                           IA.SHORT_NAME      AS TO_IA_ID,
                           B.MODULE_CODE      AS MODULE_CODE,
                           B.DATE_CREATED     AS DATE_CREATED,
                           B.CREATED_BY       AS CREATED_BY,
                           B.REJECTION_CODE   AS REJECTION_CODE,
                           i.short_name       AS FROM_AREA,
                           B.FROM_PALLET      AS FROM_PALLET,
                           B.TO_PALLET        AS TO_PALLET,
                           B.FROM_LOCATION    AS FROM_LOCATION,
                           B.TO_LOCATION      AS TO_LOCATION
        
                      FROM <proxy />BOX_AUDIT B
                        LEFT OUTER JOIN <proxy />IA I
                          ON B.FROM_AREA = I.IA_ID
                        LEFT OUTER JOIN <proxy />IA IA
                          ON B.IA_ID = IA.IA_ID
                     WHERE B.UCC128_ID = :UCC128_ID
                     ORDER BY B.DATE_CREATED DESC
                ";

            var binder = SqlBinder.Create(row => new BoxAudit
            {
                ActionPerformed = row.GetString("ACTION_PERFORMED"),
                FromIaId = row.GetString("FROM_AREA"),
                ToIaId = row.GetString("TO_IA_ID"),
                ModuleCode = row.GetString("MODULE_CODE"),
                DateCreated = row.GetDate("DATE_CREATED"),
                CreatedBy = row.GetString("CREATED_BY"),
                RejectionCode = row.GetString("REJECTION_CODE"),
                FromPallet = row.GetString("FROM_PALLET"),
                ToPallet = row.GetString("TO_PALLET"),
                FromLocation = row.GetString("FROM_LOCATION"),
                ToLocation = row.GetString("TO_LOCATION")
            }).Parameter("UCC128_ID", uccId);

            return _db.ExecuteReader(QUERY, binder);
        }

        public int PrintCatalog(string ucc128Id, string printerId)
        {

            var count = 0;
            const string QUERY = @"                
                                    BEGIN
                                     :count := <proxy />FNC_PRINT_CATALOG_LABEL(AUCC128_ID =&gt; :ucc128Id, APRINTER_NAME =&gt; :printerId);
                                    END;
                ";
            var binder = SqlBinder.Create()
                .Parameter("ucc128Id", ucc128Id)
                .Parameter("printerId", printerId)
                .OutParameter("count", val => count = val ?? 0);
            _db.ExecuteNonQuery(QUERY, binder);
            return count;
        }

        public void PrintCCL(string ucc128Id, string printerId)
        {

            var count = 0;
            const string QUERY = @"                
                                    BEGIN
                                     :count := <proxy />PKG_PRINT_CCL.WRITE_CCL_TO_FILE(AUCC128_ID =&gt; :ucc128Id, APRINTER_NAME =&gt; :printerId);
                                    END;
                ";
            var binder = SqlBinder.Create()
                .Parameter("ucc128Id", ucc128Id)
            .Parameter("printerId", printerId)
            .OutParameter("count", val => count = val ?? 0);
            _db.ExecuteNonQuery(QUERY, binder);
        }

        public void PrintUCC(string ucc128Id, string printerId)
        {
            var count = 0;
            const string QUERY = @"
                BEGIN
                  :count := <proxy />PKG_PRINT_UCC.WRITE_UCC_TO_FILE(AUCC128_ID =&gt; :ucc128Id, APRINTER_NAME =&gt; :printerId );
                END;
            ";
            var binder = SqlBinder.Create()
            .Parameter("ucc128Id", ucc128Id)
            .Parameter("printerId", printerId)
            .OutParameter("count", val => count = val ?? 0);
            _db.ExecuteNonQuery(QUERY, binder);
        }


        /// <summary>
        /// This function cancel the passed BOX.
        /// </summary>
        /// <param name="id">takes ucc128Id</param>
        public void CancelBox(string ucc128Id)
        {
            const string QUERY = @"
                BEGIN
                <proxy />pkg_pickslip.cancel_box(aucc128_id => :aucc128_id);
                                  END;";
            var binder = SqlBinder.Create().Parameter("aucc128_id", ucc128Id);
            _db.ExecuteNonQuery(QUERY, binder);
        }

        #region BoxPallet
        /// <summary>
        /// This function returns pallets history from box productivity table
        /// </summary>
        /// <param name="palletId"></param>
        /// <returns></returns>
        public IList<BoxPalletHistory> GetBoxPalletHistory(string palletId)
        {
            Contract.Assert(_db != null);
            const string QUERY = @"
                  SELECT B.OPERATION_START_DATE AS OPERATION_START_DATE,
                          B.OPERATOR            AS OPERATOR,
                          B.OUTCOME              AS OUTCOME,
                          B.MODULE_CODE          AS MODULE_CODE,
                          B.OPERATION_CODE       AS OPERATION_CODE
                     FROM <proxy />BOX_PRODUCTIVITY B
                    WHERE B.TO_PALLET = :PALLET_ID
                       OR B.FROM_PALLET = :PALLET_ID
                order by B.OPERATION_START_DATE desc
                ";
            var binder = SqlBinder.Create(row => new BoxPalletHistory
            {
                OperationStartDate = row.GetDate("OPERATION_START_DATE").Value,
                Operator = row.GetString("OPERATOR"),
                OutCome = row.GetString("OUTCOME"),
                ModuleCode = row.GetString("MODULE_CODE"),
                Operation = row.GetString("OPERATION_CODE")
            }).Parameter("PALLET_ID", palletId);

            return _db.ExecuteReader(QUERY, binder);
        }

        public IList<BoxHeadline> GetBoxesOfPallet(string palletId, int maxRows)
        {
            if (string.IsNullOrWhiteSpace(palletId))
            {
                throw new ArgumentNullException("palletId");
            }
            return SharedRepository.GetBoxes(_db, null, palletId, maxRows);
        }

        /// <summary>
        /// Getting Box info against the scanned UCC
        /// </summary>
        /// <param name="binder"></param>
        /// <returns></returns>
        [Obsolete]
        public IList<BoxHeadline> GetBoxesOfPickslip(long pickslipId, int maxRows)
        {
            Contract.Assert(_db != null);
            return SharedRepository.GetBoxes(_db, pickslipId, null, maxRows);
        }

        /// <summary>
        /// Function getting the Box pallet inventory details.
        /// </summary>
        /// <param name="palletId"></param>
        /// <returns></returns>
        //        [Obsolete]
        //        public IList<SkuInventoryItem> GetBoxPalletSku(string palletId)
        //        {
        //            Contract.Assert(_db != null);
        //            const string QUERY_PALLET_INVENTORY_DETAIL = @"
        //         SELECT SUM(BD.CURRENT_PIECES) AS CURRENT_PIECES,
        //                MAX(MSKU.STYLE)        AS STYLE,
        //                MAX(MSKU.COLOR)        AS COLOR,
        //                MAX(MSKU.DIMENSION)    AS DIMENSION,
        //                MAX(MSKU.SKU_SIZE)     AS SKU_SIZE,
        //                MAX(MSKU.UPC_CODE)     AS UPC_CODE,
        //                MSKU.SKU_ID            AS SKU_ID,
        //                B.VWH_ID               AS VWH_ID
        //              FROM <proxy />BOXDET BD
        //             INNER JOIN <proxy />BOX B
        //                ON BD.UCC128_ID = B.UCC128_ID
        //             INNER JOIN <proxy />MASTER_SKU MSKU
        //                ON MSKU.SKU_ID = BD.SKU_ID
        //             WHERE B.PALLET_ID = :PALLET_ID
        //             GROUP BY MSKU.SKU_ID, B.VWH_ID
        //            ";
        //            var binder = SqlBinder.Create(row => new SkuInventoryItem
        //            {
        //                Style = row.GetString("STYLE"),
        //                Color = row.GetString("COLOR"),
        //                Dimension = row.GetString("DIMENSION"),
        //                SkuSize = row.GetString("SKU_SIZE"),
        //                Upc = row.GetString("UPC_CODE"),
        //                VwhId = row.GetString("VWH_ID"),
        //                Pieces = row.GetInteger("CURRENT_PIECES"),
        //                SkuId = row.GetInteger("SKU_ID").Value
        //            }).Parameter("PALLET_ID", palletId);
        //            return _db.ExecuteReader(QUERY_PALLET_INVENTORY_DETAIL, binder);
        //        }

        /// <summary>
        /// This function will print boxes of pallet followed by pallet summary.
        /// This function will allow us to optionally print printed or non printed boxes and pallet summary as per users choice.
        /// </summary>
        /// <param name="palletId"></param>
        /// <param name="printerId"></param>
        /// <param name="numberOfCopies"></param>
        /// <param name="printAllBoxes"></param>
        /// <param name="printPalletHeader"></param>
        public void PrintBoxesOfPallet(string palletId, string printerId, int numberOfCopies, bool printAllBoxes, bool printPalletHeader, bool printBoxesOfPallet)
        {
            const string QUERY = @"
            DECLARE
  LOCAL_LOB CLOB;

  CURSOR BOX_CUR IS
    SELECT BOX.UCC128_ID
      FROM <proxy />BOX
     WHERE BOX.PALLET_ID = :PALLET_ID
       AND BOX.SUSPENSE_DATE IS NULL
  <if c='$PrintNonPrintedBoxes=""1""'> AND BOX.LAST_CCL_PRINT_DATE IS NULL AND BOX.LAST_UCC_PRINT_DATE IS NULL</if>
  ORDER BY LPAD((BOX.BOX_ID), 4, 0) DESC;                                  

BEGIN  
  DBMS_LOB.CREATETEMPORARY(LOCAL_LOB, TRUE, 2);
<if c='$PrintBoxesOfPallet=""1""'>
  FOR BOX_REC IN BOX_CUR LOOP
     <proxy />PKG_PRINT_UCC.WRITE_UCC_TO_CLOB(ACLOB         =&gt; LOCAL_LOB,
                                    AUCC128_ID    =&gt; BOX_REC.UCC128_ID,
                                    APRINTER_NAME =&gt; :APRINTER_NAME,
                                    ACOPIES       =&gt; :ACOPIES,
                                    AOPTIONS      =&gt; NULL);  
  
    <proxy />PKG_PRINT_CCL.WRITE_CCL_TO_CLOB(ACLOB         =&gt; LOCAL_LOB,
                                    AUCC128_ID    =&gt; BOX_REC.UCC128_ID,
                                    APRINTER_NAME =&gt; :APRINTER_NAME,
                                    ACOPIES       =&gt; :ACOPIES,
                                    AOPTIONS      =&gt; NULL);
  
  END LOOP;
</if>
 <if c='$PrintPalletHeader=""1""'> 
   <proxy />PKG_BOXEXPEDITE.PRINT_PALLET_HEADER(ACLOB         =&gt; LOCAL_LOB,
                                      APALLET_ID    =&gt; :PALLET_ID,
                                      APRINTER_NAME =&gt; :APRINTER_NAME,
                                      AHEADERONLY =&gt; :AHEADERONLY);
</if>
  FINISH_LOB(ACLOB =&gt; LOCAL_LOB);
END;
                    ";

            var binder = SqlBinder.Create().Parameter("PALLET_ID", palletId)
                .Parameter("APRINTER_NAME", printerId)
            .Parameter("ACOPIES", numberOfCopies)
            .Parameter("PrintNonPrintedBoxes", printAllBoxes == false ? "1" : "0")
            .Parameter("PrintPalletHeader", printPalletHeader == true ? "1" : "0")
            .Parameter("PrintBoxesOfPallet", printBoxesOfPallet == true ? "1" : "0")
            .Parameter("AHEADERONLY", printBoxesOfPallet == false ? "Y" : "");

            _db.ExecuteNonQuery(QUERY, binder);

        }



        #endregion


        internal IList<Tuple<string, string>> GetPrinters()
        {
            return SharedRepository.GetPrinters(_db, PrinterType.LabelPrinter);
        }



        internal IList<BoxHeadline> GetRecentPitchedBoxList(int maxRows)
        {
            return SharedRepository.GetBoxes(_db, null, string.Empty, maxRows);
        }
    }
}