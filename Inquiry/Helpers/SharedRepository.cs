using DcmsMobile.Inquiry.Areas.Inquiry.BoxEntity;
using EclipseLibrary.Oracle;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace DcmsMobile.Inquiry.Helpers
{
    internal enum PrinterType
    {
        LabelPrinter,
        LaserPrinter
    }

    /// <summary>
    /// Contains queries needed by multiple repositories.
    /// </summary>
    internal static class SharedRepository
    {

        /// <summary>
        /// Item1 is name. Item2 is description
        /// </summary>
        /// <returns></returns>
        public static IList<Tuple<string, string>> GetPrinters(OracleDatastore db, PrinterType printerType)
        {
            Contract.Assert(db != null);
            const string QUERY = @"    
    SELECT NAME AS NAME, DESCRIPTION AS DESCRIPTION
      FROM <proxy />TAB_PRINTER
     WHERE PRINTER_TYPE = :PRINTER_TYPE
     ORDER BY NAME ASC
              ";

            var binder = SqlBinder.Create(row => Tuple.Create(row.GetString("NAME"), row.GetString("DESCRIPTION")))
                .Parameter("PRINTER_TYPE", printerType == PrinterType.LabelPrinter ? "ZEBRA" : "LASER");
            return db.ExecuteReader(QUERY, binder);
        }

        /// <summary>
        /// This function will return orders summary of Customer for last 180 days.Summary is grouped on the basis of import date and we wont show more than 100 rows.
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        public static IList<PoHeadline> GetRecentOrders(OracleDatastore db, string customerId, int? skuId, int maxRows)
        {
            Contract.Assert(db != null);
            //if (string.IsNullOrWhiteSpace(customerId) && skuId == null)
            //{
            //    throw new ArgumentNullException("customerId,skuId", "At least one of customerId or skuId must be passed");
            //}
            const string QUERY = @"               
                            WITH PSDATA AS
                             (
                              -- PK PICKSLIP ID
                              SELECT PS.PICKSLIP_ID            AS PICKSLIP_ID,
                                      PS.TOTAL_QUANTITY_ORDERED AS PIECES_ORDERED,
                                      PO.DC_CANCEL_DATE         AS DC_CANCEL_DATE,
                                      PO.START_DATE             AS START_DATE,
                                      PS.PICKSLIP_IMPORT_DATE   AS IMPORT_DATE,
                                      PS.CUSTOMER_ID AS CUSTOMER_ID,
                                      CUST.NAME                 AS CUSTOMER_NAME,
                                      PO.PO_ID                  AS PO_ID,
                                    --  PS.WAREHOUSE_LOCATION_ID  AS WAREHOUSE_LOCATION_ID,
                                    --  PS.VWH_ID                 AS VWH_ID,
                                      PO.ITERATION              AS ITERATION
                                FROM <proxy />PS PS
                                    INNER JOIN <proxy />CUST CUST
                                 ON PS.CUSTOMER_ID = CUST.CUSTOMER_ID    
                               INNER JOIN <proxy />PO PO
                                  ON PS.CUSTOMER_ID = PO.CUSTOMER_ID
                                 AND PS.PO_ID = PO.PO_ID
                                 AND PS.ITERATION = PO.ITERATION
                               WHERE PS.TRANSFER_DATE IS NULL
                                 AND PS.PICKSLIP_CANCEL_DATE IS NULL
<if>
                                 AND PS.CUSTOMER_ID = :CUSTOMER_ID
</if>
<if>
and ps.pickslip_id in
         (select pd.pickslip_id
            from psdet pd
           where pd.sku_id = :sku_id AND PD.TRANSFER_DATE IS NULL)
</if>
                              UNION ALL
                              SELECT PS1.PICKSLIP_ID            AS PICKSLIP_ID,
                                      PS1.TOTAL_QUANTITY_ORDERED AS PIECES_ORDERED,
                                      PS1.DC_CANCEL_DATE         AS DC_CANCEL_DATE,
                                      PS1.DELIVERY_DATE          AS START_DATE,
                                      PS1.PICKSLIP_IMPORT_DATE   AS IMPORT_DATE,
                                      PS1.CUSTOMER_ID AS CUSTOMER_ID,
                                      CT.NAME                    AS CUSTOMER_NAME,
                                      PS1.CUSTOMER_ORDER_ID      AS PO_ID,
                                     --  PS1.WAREHOUSE_LOCATION_ID  AS WAREHOUSE_LOCATION_ID,
                                     -- PS1.VWH_ID                 AS VWH_ID,
                                      0                          AS ITERATION   
                                FROM <proxy />DEM_PICKSLIP PS1
                                 INNER JOIN <proxy />MASTER_CUSTOMER CT
                                      ON PS1.CUSTOMER_ID = CT.CUSTOMER_ID
                               WHERE  PS1.PS_STATUS_ID = 1
<if>
AND PS1.CUSTOMER_ID = :CUSTOMER_ID
</if>
<if>
   AND ps1.pickslip_id in (select pd.pickslip_id
                         from <proxy />dem_pickslip_detail pd
                        inner join <proxy />master_sku s
                           on s.upc_code = pd.upc_code
                        where s.sku_id = :sku_id)
</if>
),
                            BOX_PICKSLIPS AS
                             (
                              -- PK PICKSLIP_ID  
                              SELECT B.PICKSLIP_ID,                                     
                                      SUM(BD.CURRENT_PIECES) AS CURRENT_PIECES
                                     -- COUNT(UNIQUE B.UCC128_ID) AS PICKSLIP_BOX_COUNT
                                FROM <proxy />BOX B
                               INNER JOIN <proxy />BOXDET BD
                                  ON B.UCC128_ID = BD.UCC128_ID
                                 AND B.PICKSLIP_ID = BD.PICKSLIP_ID
                               INNER JOIN <proxy />PS PS
                                  ON PS.PICKSLIP_ID = B.PICKSLIP_ID
                               WHERE BD.STOP_PROCESS_DATE IS NULL
                                 AND B.STOP_PROCESS_DATE IS NULL
<if>
                                 AND B.PICKSLIP_ID IN (SELECT PICKSLIP_ID
                                                         FROM <proxy />PS
                                                        WHERE PS.TRANSFER_DATE IS NULL
                                                          AND PS.CUSTOMER_ID = :CUSTOMER_ID)
</if>
<if>
     AND B.PICKSLIP_ID IN (select pd.pickslip_id
                             from <proxy />psdet pd
                            where pd.sku_id = :sku_id)
</if>
                               GROUP BY B.PICKSLIP_ID),
                            ALL_PO AS
                             (
                              -- PK PO, VWH, BUILDING  
                              SELECT ROW_NUMBER() OVER(ORDER BY MAX(PSDATA.IMPORT_DATE) DESC) AS IMPORT_SEQUENCE,
                                      SUM(PSDATA.PIECES_ORDERED) AS ORDERED_PIECES,
                                      PSDATA.CUSTOMER_ID AS CUSTOMER_ID,
                                      MAX(PSDATA.CUSTOMER_NAME) AS CUSTOMER_NAME,
                                      PSDATA.PO_ID AS PO_ID,
                                      MAX(PSDATA.DC_CANCEL_DATE) AS DC_CANCEL_DATE,
                                      MAX(PSDATA.START_DATE) AS START_DATE,
                                      MAX(PSDATA.IMPORT_DATE) AS IMPORT_DATE,
                                     -- PSDATA.WAREHOUSE_LOCATION_ID AS WAREHOUSE_LOCATION_ID,
                                     -- PSDATA.VWH_ID AS VWH_ID,
                                      COUNT(DISTINCT PSDATA.PICKSLIP_ID) AS NO_OF_PICKSLIPS,
                                      COUNT(*) OVER() AS TOTAL_PO,                                     
                                      SUM(BP.CURRENT_PIECES) AS CURRENT_PIECES,
                                     -- SUM(BP.PICKSLIP_BOX_COUNT) AS PO_BOX_COUNT,
                                      PSDATA.ITERATION   as ITERATION
                                FROM PSDATA PSDATA
                                LEFT OUTER JOIN BOX_PICKSLIPS BP
                                  ON PSDATA.PICKSLIP_ID = BP.PICKSLIP_ID
                               GROUP BY  PSDATA.ITERATION,PSDATA.CUSTOMER_ID,PSDATA.PO_ID)
                                        --PSDATA.WAREHOUSE_LOCATION_ID,,
                                            --PSDATA.VWH_ID
                                          
                            SELECT AC.CUSTOMER_ID AS CUSTOMER_ID,
                                   AC.CUSTOMER_NAME         AS CUSTOMER_NAME,
                                   AC.PO_ID                 AS PO_ID,
                                   AC.DC_CANCEL_DATE        AS DC_CANCEL_DATE,
                                   AC.START_DATE            AS START_DATE,
                                   AC.IMPORT_DATE           AS IMPORT_DATE,
                                   AC.ORDERED_PIECES        AS ORDERED_PIECES,                                   
                                   AC.CURRENT_PIECES        AS PIECES_IN_BOXES,                                   
                                   AC.NO_OF_PICKSLIPS       AS NO_OF_PICKSLIPS,
                                   --AC.WAREHOUSE_LOCATION_ID AS WAREHOUSE_LOCATION_ID,
                                   --AC.VWH_ID                AS VWH_ID,
                                   AC.TOTAL_PO              AS TOTAL_PO,
                                   AC.ITERATION             AS ITERATION
                                   --AC.PO_BOX_COUNT          AS BOX_COUNT
                              FROM ALL_PO AC
                             WHERE AC.IMPORT_SEQUENCE &lt; :maxRows
                             ORDER BY AC.IMPORT_SEQUENCE

            ";

            var binder = SqlBinder.Create(row => new PoHeadline
            {
                CustomerId = row.GetString("CUSTOMER_ID"),
                CustomerName=row.GetString("CUSTOMER_NAME"),
                PO = row.GetString("PO_ID"),
                DcCancelDate = row.GetDate("DC_CANCEL_DATE"),
                StartDate = row.GetDate("START_DATE"),
                ImportDate = row.GetDate("IMPORT_DATE"),
                PiecesOrdered = row.GetInteger("ORDERED_PIECES"),
                PiecesInBox = row.GetInteger("PIECES_IN_BOXES"),
                TotalPickslip = row.GetInteger("NO_OF_PICKSLIPS") ?? 0,
                //BuildingId = row.GetString("WAREHOUSE_LOCATION_ID"),
                //VWhId = row.GetString("VWH_ID"),
                TotalPO = row.GetInteger("TOTAL_PO") ?? 0,
                Iteration = row.GetInteger("ITERATION") ?? 0,
                //BoxCount = row.GetInteger("BOX_COUNT")
            }).Parameter("CUSTOMER_ID", customerId)
            .Parameter("sku_id", skuId).Parameter("maxRows", maxRows);

            return db.ExecuteReader(QUERY, binder, maxRows);
        }

        /// <summary>
        /// If no filter is passed, then active boxes are returned, most recently touched first
        /// </summary>
        /// <param name="db"></param>
        /// <param name="pickslipId"></param>
        /// <param name="palletId">Cancelled boxes are not returned a pallet</param>
        /// <param name="maxRows"></param>
        /// <returns></returns>
        public static IList<BoxHeadline> GetBoxes(OracleDatastore db, long? pickslipId, string palletId,int maxRows)
        {
            //if (pickslipId == null && string.IsNullOrWhiteSpace(palletId))
            //{
            //    throw new ArgumentNullException("Both pickslipId and palletId cannot be null");
            //}
            Contract.Assert(db != null);
            const string QUERY_BOX_DETAIL = @"
                        SELECT BOX.UCC128_ID                 AS UCC128_ID,
                               COUNT(BOX.UCC128_ID) OVER() AS total_boxes,
                               MAX(BOX.PICKSLIP_ID)          AS PICKSLIP_ID,
                               MAX(BOX.IA_ID)                AS IA_ID,
                               MAX(IA.WAREHOUSE_LOCATION_ID) AS WAREHOUSE_LOCATION_ID,
                                MAX(IA.SHORT_NAME)           AS SHORT_NAME,
                               MAX(BOX.PALLET_ID)            AS PALLET_ID,
                               MAX(PS.CUSTOMER_ID)           AS CUSTOMER_ID,
                               MAX(BOX.STOP_PROCESS_DATE)    AS STOP_PROCESS_DATE,
                               MAX(BOX.STOP_PROCESS_REASON)    AS STOP_PROCESS_REASON,
                               MAX(CUST.NAME)                AS NAME,
                               MAX(BOX.PITCHING_END_DATE)    AS PITCHING_END_DATE,
                               MAX(BOX.VERIFY_DATE)          AS VERIFY_DATE,
                               Max(BOX.TRUCK_LOAD_DATE)      AS TRUCK_LOAD_DATE,
                               SUM(NVL(BD.EXPECTED_PIECES, BD.CURRENT_PIECES))       AS EXPECTED_PIECES,
                               SUM(BD.CURRENT_PIECES)        AS CURRENT_PIECES,
                               MAX(PS.BUCKET_ID)            AS BUCKET_ID,
                               MAX(BOX.CARTON_ID)            AS CARTON_ID,
                               MAX(BOX.LAST_UCC_PRINT_DATE)  AS LAST_UCC_PRINTED_DATE,
                               MAX(BOX.LAST_CCL_PRINT_DATE)  AS LAST_CCL_PRINT_DATE,
                               MIN(BD.LAST_PITCHED_BY)                      AS MIN_PITCHED_BY
                         FROM <proxy />BOX BOX
                         INNER JOIN <proxy />PS PS
                            ON BOX.PICKSLIP_ID = PS.PICKSLIP_ID
                         INNER JOIN <proxy />BOXDET BD
                            ON BD.UCC128_ID = BOX.UCC128_ID
                            AND BD.PICKSLIP_ID = BOX.PICKSLIP_ID
                          LEFT OUTER JOIN <proxy />CUST CUST
                            ON CUST.CUSTOMER_ID = PS.CUSTOMER_ID  
                          LEFT OUTER JOIN <proxy />IA IA
                            ON BOX.IA_ID = IA.IA_ID                       
                        WHERE 
                        <if>
                        box.PICKSLIP_ID = :PICKSLIP_ID
                       </if>
                    <else>
                        <if>
                        box.PALLET_ID = :PALLET_ID AND box.stop_process_date IS NULL
                        </if>
                        <else>
                        box.stop_process_date IS NULL
                        and bd.stop_process_date is null 
                        and ps.transfer_date is null  
                        </else>
                     </else>
                        group by box.ucc128_id
                        ORDER BY  MAX(PS.CUSTOMER_ID),MAX(BOX.PICKSLIP_ID),BOX.UCC128_ID,greatest(MAX(box.truck_load_date),
                                   MAX(box.last_ccl_print_date),
                                   MAX(box.pitching_end_date),
                                   MAX(box.verify_date)) desc nulls last
                        ";
            var binder = SqlBinder.Create(row => new BoxHeadline
            {
                Ucc128Id = row.GetString("UCC128_ID"),
                PalletId = row.GetString("PALLET_ID"),
                CartonId = row.GetString("CARTON_ID"),
                IaId = row.GetString("IA_ID"),
                ShortName = row.GetString("SHORT_NAME"),
                VerificationDate = row.GetDate("VERIFY_DATE"),
                PitchingEndDate = row.GetDate("PITCHING_END_DATE"),
                TruckLoadDate = row.GetDate("TRUCK_LOAD_DATE"),
                ExpectedPieces = row.GetInteger("EXPECTED_PIECES"),
                CurrentPieces = row.GetInteger("CURRENT_PIECES"),
                LastCclPrintedDate = row.GetDate("LAST_CCL_PRINT_DATE"),
                LastUccPrintedDate = row.GetDate("LAST_UCC_PRINTED_DATE"),
                MinPickerName = row.GetString("MIN_PITCHED_BY"),
                StopProcessDate = row.GetDate("STOP_PROCESS_DATE"),
                StopProcessReason = row.GetString("STOP_PROCESS_REASON"),
                PickslipId = row.GetLong("PICKSLIP_ID").Value,
                BucketId = row.GetInteger("BUCKET_ID") ?? 0,
                CustomerId = row.GetString("CUSTOMER_ID"),
                CustomerName = row.GetString("NAME"),
                Building = row.GetString("WAREHOUSE_LOCATION_ID"),
                TotalBoxes = row.GetInteger("total_boxes") ?? 0
            }).Parameter("PICKSLIP_ID", pickslipId)
                .Parameter("PALLET_ID", palletId);            
            return db.ExecuteReader(QUERY_BOX_DETAIL, binder, maxRows);
        }

    }
}