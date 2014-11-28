using DcmsMobile.Inquiry.Areas.Inquiry.SharedViews;
using DcmsMobile.Inquiry.Helpers;
using EclipseLibrary.Oracle;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.Contracts;
using System.Web;

namespace DcmsMobile.Inquiry.Areas.Inquiry.CartonEntity
{
    internal class CartonEntityRepository : IDisposable
    {
        private readonly OracleDatastore _db;
        public CartonEntityRepository(string userName, string clientInfo)
        {
            _db = new OracleDatastore(HttpContext.Current.Trace);
            _db.CreateConnection(ConfigurationManager.ConnectionStrings["dcms8"].ConnectionString, userName);
            _db.ModuleName = "Inquiry_CartonEntity";
            _db.ClientInfo = clientInfo;
        }

        public void Dispose()
        {
            _db.Dispose();
        }

        internal IList<Tuple<string, string>> GetPrinters()
        {
            return SharedRepository.GetPrinters(_db, PrinterType.LabelPrinter);
        }


        public ActiveCarton GetActiveCarton(string cartonId)
        {
            if (string.IsNullOrWhiteSpace(cartonId))
            {
                throw new ArgumentNullException("cartonId");
            }
            const string QUERY = @"WITH Q1 AS
            (SELECT CTN.CARTON_ID             AS CARTON_ID,
                   CTN.CARTON_STORAGE_AREA   AS CARTON_STORAGE_AREA,
                   TIA.SHORT_NAME            AS SHORT_NAME,
                   TIA.description           AS description, 
                   CTN.LOCATION_ID           AS LOCATION_ID,
                   ctn.quality_code          AS quality_code,
                   TQC.DESCRIPTION           AS QUALITY_DESCRIPTION,
                   CTN.LAST_PULLED_DATE      As LAST_PULLED_DATE,
                   NVL(tia.warehouse_location_id, msl.warehouse_location_id) As warehouse_location_id,
                   ctn.pallet_id             As pallet_id,
                   ctn.price_season_code     As Price_Season_Code,
                   ctn.sewing_plant_code     As sewing_plant_code,
                   ctn.WORK_NEEDED_XML       as WORK_NEEDED_XML,
                   TSP.SEWING_PLANT_NAME     AS SEWING_PLANT_NAME,
                   ctn.vwh_id                As vwh_id,
                   ctn.damage_code           As damage_code,
                    ctn.suspense_date        AS suspense_date,
                   ctn.unmatch_comment_user  As unmatch_comment_user,
                   ctn.unmatch_reason        As unmatch_reason,
                   CTNDET.Quantity           As Quantity,
                   ctndet.sku_id             AS sku_id,
                   msku.style                AS style,
                   msku.color                AS color,
                   msku.dimension            AS dimension,
                   msku.sku_size             AS sku_size,
                   msku.upc_code             AS upc_code,
                   SS.SHIPMENT_ID            AS SHIPMENT_ID,
                   SS.SHIPMENT_DATE          AS SHIPMENT_DATE,
                   B.UCC128_ID               AS UCC128_ID
              FROM <proxy />SRC_CARTON CTN
              LEFT OUTER JOIN <proxy />SRC_CARTON_DETAIL CTNDET
                ON CTN.CARTON_ID = CTNDET.CARTON_ID
              LEFT OUTER JOIN <proxy />MASTER_SKU MSKU
                ON CTNDET.SKU_ID = MSKU.SKU_ID
              LEFT OUTER JOIN <proxy />TAB_SEWINGPLANT TSP
                ON TSP.SEWING_PLANT_CODE = ctn.SEWING_PLANT_CODE
              left outer join <proxy />tab_inventory_area tia
                on tia.inventory_storage_area = ctn.carton_storage_area
              left outer join <proxy />master_storage_location msl
                on msl.storage_area = ctn.carton_storage_area AND msl.location_id = ctn.location_id
                LEFT OUTER JOIN <proxy />src_carton_intransit SS
                    ON ctn.shipment_id = SS.SHIPMENT_ID
                    AND CTN.CARTON_ID = SS.CARTON_ID
                LEFT OUTER JOIN <proxy />BOX B ON
                     CTN.CARTON_ID = B.CARTON_ID
                LEFT OUTER JOIN <proxy />TAB_QUALITY_CODE TQC 
                     ON CTN.QUALITY_CODE = TQC.QUALITY_CODE
             WHERE CTN.CARTON_ID = :cartonId
            ),
            Q2 AS
             (SELECT MS.SKU_ID               AS SKU_ID,
                     I.IA_ID                 AS IA_ID,
                     I.VWH_ID                AS VWH_ID,
                     I.LOCATION_ID           AS LOCATION_ID,
                     I.RESTOCK_AISLE_ID      AS RESTOCK_AISLE,
                     I.WAREHOUSE_LOCATION_ID AS WAREHOUSE_LOCATION_ID,
                     MAX(IA.SHORT_DESCRIPTION)      AS SHORT_NAME
                FROM <proxy />IA IA
               INNER JOIN <proxy />IALOC I
                  ON IA.IA_ID = I.IA_ID
               INNER JOIN <proxy />MASTER_SKU MS
                  ON MS.UPC_CODE = I.ASSIGNED_UPC_CODE
               WHERE MS.SKU_ID IN (SELECT SKU_ID FROM Q1)
                 AND I.VWH_ID IN (SELECT VWH_ID FROM Q1)    
               GROUP BY MS.SKU_ID,
                        I.VWH_ID,
                        I.RESTOCK_AISLE_ID,
                        I.LOCATION_ID,
                        I.IA_ID,
                        I.WAREHOUSE_LOCATION_ID)
            SELECT Q1.CARTON_ID,
                   Q1.CARTON_STORAGE_AREA,
                   Q1.SHORT_NAME,
                   Q1.description, 
                   Q1.LOCATION_ID,
                   Q1.quality_code,
                   q1.QUALITY_DESCRIPTION,
                   Q1.LAST_PULLED_DATE,
                   Q1.warehouse_location_id,
                   Q1.pallet_id,
                   Q1.Price_Season_Code,
                   Q1.sewing_plant_code,
                   Q1.WORK_NEEDED_XML,
                   Q1.SEWING_PLANT_NAME,
                   Q1.vwh_id,
                   Q1.damage_code,
                   Q1.suspense_date,
                   Q1.unmatch_comment_user,
                   Q1.unmatch_reason,
                   Q1.Quantity,
                   Q1.style,
                   Q1.color,
                   Q1.dimension,
                   Q1.sku_size,
                   --Q1.upc_code,
                   Q1.sku_id,
                   Q1.SHIPMENT_ID,
                   Q1.SHIPMENT_DATE,
                   Q1.UCC128_ID,
                   Q2.LOCATION_ID AS ASSIGNED_LOCATION,
                   Q2.RESTOCK_AISLE AS ASSIGNED_RESTOCK_AISLE,
                   Q2.WAREHOUSE_LOCATION_ID AS ASSIGNED_WAREHOUSE_LOCATION,
                  -- Q2.IA_ID AS ASSIGNED_IAID,
                   Q2.SHORT_NAME AS ASSIGNED_IAID_SHORT_NAME
              FROM Q1
              LEFT OUTER JOIN Q2
                ON Q1.SKU_ID = Q2.SKU_ID
               AND Q1.VWH_ID = Q2.VWH_ID
                         ";

            var binder = SqlBinder.Create(row => new ActiveCarton
            {
                CartonId = row.GetString("CARTON_ID"),
                //sku_id = row.GetInteger("sku_id") ?? 0,
                Building = row.GetString("warehouse_location_id"),
                PalletId = row.GetString("pallet_id"),
                CartonAreaId = row.GetString("CARTON_STORAGE_AREA"),
                AreaShortName = row.GetString("SHORT_NAME"),
                AreaDescription = row.GetString("description"),
                //Upc = row.GetString("upc_code"),
                SkuId = row.GetInteger("sku_id"),
                SewingPlantName = row.GetString("SEWING_PLANT_NAME"),
                QualityCode = row.GetString("quality_code"),
                QualityDescription = row.GetString("QUALITY_DESCRIPTION"),
                LastPulledDate = row.GetDate("LAST_PULLED_DATE"),
                Style = row.GetString("style"),
                Color = row.GetString("color"),
                Dimension = row.GetString("dimension"),
                SkuSize = row.GetString("sku_size"),
                Pieces = row.GetInteger("Quantity"),
                SewingPlantCode = row.GetString("sewing_plant_code"),
                LocationId = row.GetString("LOCATION_ID"),
                PriceSeasonCode = row.GetString("Price_Season_Code"),
                SuspenseDate = row.GetDate("suspense_date"),
                UnmatchReason = row.GetString("unmatch_reason"),
                UnmatchComment = row.GetString("unmatch_comment_user"),
                DamageCode = row.GetString("damage_code"),
                VwhId = row.GetString("vwh_id"),
                ShipmentId = row.GetString("SHIPMENT_ID"),
                ShipmentDate = row.GetDate("SHIPMENT_DATE"),
                ReservedUccID = row.GetString("UCC128_ID"),
                IsCartonMarkedForWork = !string.IsNullOrEmpty(row.GetString("WORK_NEEDED_XML")),
                BestRestockAisleId = row.GetString("ASSIGNED_RESTOCK_AISLE"),
                BestRestockBuildingId = row.GetString("ASSIGNED_WAREHOUSE_LOCATION"),
                //AssignedIaId = row.GetString("ASSIGNED_IAID"),
                BestRestockAreaShortName = row.GetString("ASSIGNED_IAID_SHORT_NAME"),
                BestRestockLocationId = row.GetString("ASSIGNED_LOCATION")
            }).Parameter("cartonId", cartonId);
            return _db.ExecuteSingle(QUERY, binder);

        }
        /// <summary>
        /// This Method is Used For Carton Pallet List.
        /// </summary>
        /// <param name="PalletId"></param>
        /// <returns></returns>
        public IList<PalletHeadLine> GetPalletList(int maxRows)
        {
            const string QUERY = @"SELECT SC.PALLET_ID,
                                    COUNT(SC.CARTON_ID) AS TOTAL_CARTON,
                                    MAX(SC.AREA_CHANGE_DATE) AS MAX_AREA_CHANGE_DATE,
                                    MIN(SC.AREA_CHANGE_DATE) AS MIN_AREA_CHANGE_DATE,
                                    COUNT(DISTINCT SC.CARTON_STORAGE_AREA) AS CARTON_AREA_COUNT,
                                    MAX(TIA.SHORT_NAME) AS AREA_SHORT_NAME,
                                   MAX(TIA.WAREHOUSE_LOCATION_ID) AS WAREHOUSE_LOCATION_ID
           FROM <proxy />SRC_CARTON SC
          LEFT OUTER JOIN <proxy />TAB_INVENTORY_AREA TIA
         ON SC.CARTON_STORAGE_AREA = TIA.INVENTORY_STORAGE_AREA
          WHERE SC.PALLET_ID IS NOT NULL
        GROUP BY SC.PALLET_ID
       ORDER BY MIN(SC.AREA_CHANGE_DATE) DESC NULLS LAST";
         var binder = SqlBinder.Create(row => new PalletHeadLine
         {
             PalletId = row.GetString("PALLET_ID"),
             TotalCarton = row.GetInteger("TOTAL_CARTON"),
             MaxAreaChangeDate = row.GetDate("MAX_AREA_CHANGE_DATE"),
              MinAreaChangeDate = row.GetDate("MIN_AREA_CHANGE_DATE"),
             CartonAreaCount = row.GetInteger("CARTON_AREA_COUNT").Value,
             AreaShortName = row.GetString("AREA_SHORT_NAME"),
             WarehouseLocationId = row.GetString("WAREHOUSE_LOCATION_ID")
         });
             return _db.ExecuteReader(QUERY, binder,maxRows);
        }





        /// <summary>
        /// This method is used to get the list of all processes  of a carton.
        /// </summary>
        /// <param name="cartonId"></param>
        /// <returns></returns>
        public IList<CartonProcess> GetCartonHistory(string cartonId)
        {
            const string QUERY = @" SELECT PRODET.MODULE_CODE,
                            PRODET.FROM_PALLET_ID,
                            PRODET.TO_PALLET_ID,
                            PRODET.FROM_CARTON_AREA,                            
                            PRODET.INSERTED_BY,
                            PRODET.TO_CARTON_AREA,
                            PRODET.INSERT_DATE,
                            PRODET.OLD_SUSPENSE_DATE,
                            PRODET.NEW_SUSPENSE_DATE,
                            PRODET.FROM_LOCATION_ID,
                            PRODET.TO_LOCATION_ID,
                            PRODET.APPLICATION_ACTION,
                            PRODET.OLD_CARTON_QTY,
                            PRODET.NEW_CARTON_QTY
         FROM <proxy />SRC_CARTON_PROCESS_DETAIL PRODET            
         WHERE PRODET.CARTON_ID = :CARTON_ID
         ORDER BY PRODET.INSERT_DATE DESC";
            var binder = SqlBinder.Create(row => new CartonProcess
            {
                
                ModuleCode = row.GetString("MODULE_CODE"),
                InsertedBy = row.GetString("INSERTED_BY"),
                FromPalletId = row.GetString("FROM_PALLET_ID"),
                ToPalletId = row.GetString("TO_PALLET_ID"),
                FromCartonArea = row.GetString("FROM_CARTON_AREA"),
                ToCartonArea = row.GetString("TO_CARTON_AREA"),
                InsertDate = row.GetDate("INSERT_DATE"),
                OldSuspenseDate = row.GetDate("OLD_SUSPENSE_DATE"),
                NewSuspenseDate = row.GetDate("NEW_SUSPENSE_DATE"),
                FromLocation = row.GetString("FROM_LOCATION_ID"),
                ToLocation = row.GetString("TO_LOCATION_ID"),
                ActionPerformed = row.GetString("APPLICATION_ACTION"),
                OldCartonQuantity = row.GetInteger("OLD_CARTON_QTY"),
                NewCartonQuantity = row.GetInteger("NEW_CARTON_QTY")

            }).Parameter("CARTON_ID", cartonId);

            return _db.ExecuteReader(QUERY, binder);
        }

        //This function is for carton printing
        public void PrintCarton(string cartonId, string printerId)
        {
            Contract.Assert(_db != null);
            const string QUERY = @"begin
               <proxy />pkg_jf_src_2.pkg_jf_src_ctn_tkt(acarton_id => :acarton_id,
                                              aprinter_name => :aprinter_name);
                        end;           
            ";
            var binder = SqlBinder.Create()
            .Parameter("acarton_id", cartonId)
            .Parameter("aprinter_name", printerId);
            _db.ExecuteNonQuery(QUERY, binder);

        }


        public OpenCarton GetOpenCarton(string id)
        {
            Contract.Assert(_db != null);
            const string QUERY = @"SELECT OPENCTN.CARTON_ID                AS CARTON_ID,
                                          OPENCTN.LAST_CARTON_STORAGE_AREA AS LAST_CARTON_STORAGE_AREA,
                                          TIA.SHORT_NAME                   AS CARTON_AREA_SHORT_NAME,
                                          OPENCTN.LAST_LOCATION_ID         AS LAST_LOCATION_ID,
                                          OPENCTN.QUALITY_CODE             AS QUALITY_CODE,
                                          TQC.DESCRIPTION                  AS QUALITY_DESCRIPTION,
                                          OPENCTN.LAST_PULLED_DATE         AS LAST_PULLED_DATE,
                                          OPENCTN.LAST_PALLET_ID           AS LAST_PALLET_ID,
                                          OPENCTN.PRICE_SEASON_CODE        AS PRICE_SEASON_CODE,
                                          OPENCTN.VWH_ID                   AS VWH_ID,
                                          OPENCTN.DAMAGE_CODE              AS DAMAGE_CODE,
                                          OPENCTN.SEWING_PLANT_CODE        AS SEWING_PLANT_CODE,
                                          TSP.SEWING_PLANT_NAME            AS SEWING_PLANT_NAME,
                                          OPENCTN.ORIGINAL_CARTON_QUANTITY AS ORIGINAL_CARTON_QUANTITY,
                                          MSKU.SKU_ID                      AS SKU_ID,
                                          --MSKU.UPC_CODE                    AS UPC_CODE,
                                          MSKU.STYLE                       AS STYLE,
                                          MSKU.COLOR                       AS COLOR,
                                          MSKU.DIMENSION                   AS DIMENSION,
                                          MSKU.SKU_SIZE                    AS SKU_SIZE,
                                          SS.SHIPMENT_ID                   AS SHIPMENT_ID,
                                          SS.SHIPMENT_DATE                 AS SHIPMENT_DATE,
                                          B.UCC128_ID                      AS UCC128_ID
                                     FROM <proxy />SRC_OPEN_CARTON OPENCTN
                                     LEFT OUTER JOIN <proxy />MASTER_SKU MSKU
                                       ON OPENCTN.UPC_CODE = MSKU.UPC_CODE
                                     LEFT OUTER JOIN <proxy />TAB_SEWINGPLANT TSP
                                       ON OPENCTN.SEWING_PLANT_CODE = TSP.SEWING_PLANT_CODE
                                     LEFT OUTER JOIN <proxy />SRC_CARTON_INTRANSIT SS
                                       ON OPENCTN.SHIPMENT_ID = SS.SHIPMENT_ID
                                       AND OPENCTN.CARTON_ID = SS.CARTON_ID
                                     LEFT OUTER JOIN <proxy />BOX B
                                       ON OPENCTN.CARTON_ID = B.CARTON_ID
                                     LEFT OUTER JOIN <proxy />TAB_INVENTORY_AREA TIA
                                       ON OPENCTN.LAST_CARTON_STORAGE_AREA = TIA.INVENTORY_STORAGE_AREA
                                    LEFT OUTER JOIN <proxy />TAB_QUALITY_CODE TQC 
                                       ON OPENCTN.QUALITY_CODE = TQC.QUALITY_CODE
                                    WHERE OPENCTN.CARTON_ID = :CARTON_ID
            ";

            var binder = SqlBinder.Create(row => new OpenCarton
            {
                CartonId = row.GetString("CARTON_ID"),
                CartonStorageArea = row.GetString("LAST_CARTON_STORAGE_AREA"),
                ShortName = row.GetString("CARTON_AREA_SHORT_NAME"),
                LocationId = row.GetString("LAST_LOCATION_ID"),
                LastPulledDate = row.GetDate("LAST_PULLED_DATE"),
                PalletId = row.GetString("LAST_PALLET_ID"),
                PriceSeasonCode = row.GetString("PRICE_SEASON_CODE"),
                DamageCode = row.GetString("DAMAGE_CODE"),
                SewingPlantCode = row.GetString("SEWING_PLANT_CODE"),
                SewingPlantName = row.GetString("SEWING_PLANT_NAME"),
                ReservedUccID = row.GetString("UCC128_ID"),
                QualityCode = row.GetString("QUALITY_CODE"),
                QualityDescription = row.GetString("QUALITY_DESCRIPTION"),
                VwhId = row.GetString("VWH_ID"),
                Pieces = row.GetInteger("ORIGINAL_CARTON_QUANTITY"),
                //Upc = row.GetString("UPC_CODE"),
                SkuId = row.GetInteger("SKU_ID"),
                Style = row.GetString("STYLE"),
                Color = row.GetString("COLOR"),
                Dimension = row.GetString("DIMENSION"),
                SkuSize = row.GetString("SKU_SIZE"),
                ShipmentId = row.GetString("SHIPMENT_ID"),
                ShipmentDate = row.GetDate("SHIPMENT_DATE")

            }).Parameter("CARTON_ID", id);

            return _db.ExecuteSingle(QUERY, binder);

        }

        /// <summary>
        /// This function will provide information about intransit cartons. 
        ///  <param name="carton_id"></param>
        /// </summary>
        public IntransitCarton GetIntransitCartonInfo(string carton_id)
        {
            Contract.Assert(_db != null);
            var QUERY = @"
                SELECT TS.SEWING_PLANT_NAME                                          as SEWING_PLANT_NAME,
                       TS.SEWING_PLANT_CODE                                          AS SEWING_PLANT_CODE,
                       S.SHIPMENT_ID                                                 AS SHIPMENT_ID,
                       S.SHIPMENT_DATE                                               AS SHIPMENT_DATE,
                       S.CARTON_ID                                                   AS CARTON_ID,
                       S.STYLE                                                       AS STYLE,
                       S.COLOR                                                       AS COLOR,
                       S.DIMENSION                                                   AS DIMENSION,
                       S.SKU_SIZE                                                    AS SKU_SIZE,
                       S.QUANTITY                                                    AS QUANTITY,
                       S.PRICE_SEASON_CODE                                           AS PRICE_SEASON_CODE,
                       S.VWH_ID                                                      AS VWH_ID,
                       S.INTRANSIT_ID                                                AS INTRANSIT_ID,
                       --M.UPC_CODE                                                  AS UPC_CODE,
                       M.SKU_ID                                                      AS SKU_ID,
                       S.SOURCE_ORDER_PREFIX                                         AS SOURCE_ORDER_PREFIX, 
                       S.SOURCE_ORDER_ID                                             AS SOURCE_ORDER_ID, 
                       S.SOURCE_ORDER_LINE_NUMBER                                    AS SOURCE_ORDER_LINE_NUMBER
                  FROM <proxy />SRC_CARTON_INTRANSIT S
                  LEFT OUTER JOIN <proxy />TAB_SEWINGPLANT TS
                    ON TS.SEWING_PLANT_CODE = S.SEWING_PLANT_CODE
                  LEFT OUTER JOIN <proxy />MASTER_SKU M
                    ON S.STYLE = M.STYLE
                    AND S.COLOR = M.COLOR
                    AND S.DIMENSION = M.DIMENSION 
                    AND S.SKU_SIZE = M.SKU_SIZE
                  
                 WHERE S.CARTON_ID = :CARTON_ID
                ";
            var binder = SqlBinder.Create(row => new IntransitCarton
            {
                PriceSeasonCode = row.GetString("PRICE_SEASON_CODE"),
                CartonId = row.GetString("CARTON_ID"),
                SewingPlantName = row.GetString("SEWING_PLANT_NAME"),
                SewingPlantCode = row.GetString("SEWING_PLANT_CODE"),
                SourceOrderedID = row.GetInteger("SOURCE_ORDER_ID"),
                SourceOrderPrefix = row.GetString("SOURCE_ORDER_PREFIX"),
                SourceOrderLineNumber = row.GetInteger("SOURCE_ORDER_LINE_NUMBER"),
                ShipmentId = row.GetString("SHIPMENT_ID"),
                ShipmentDate = row.GetDate("SHIPMENT_DATE"),
                IntransitId = row.GetInteger("INTRANSIT_ID"),
                Style = row.GetString("STYLE"),
                Color = row.GetString("COLOR"),
                Dimension = row.GetString("DIMENSION"),
                SkuSize = row.GetString("SKU_SIZE"),
                VwhId = row.GetString("VWH_ID"),
                Pieces = row.GetInteger("QUANTITY"),
                //Upc = row.GetString("UPC_CODE"),
                SkuId = row.GetInteger("SKU_ID")
            }).Parameter("CARTON_ID", carton_id);
            return _db.ExecuteSingle(QUERY, binder);
        }

        public IList<CartonHeadline> GetCartonsOfPallet(string palletId, int maxRows = 1000)
        {
            //if (string.IsNullOrWhiteSpace(palletId))
            //{
            //    throw new ArgumentNullException("palletId");
            //}
            const string QUERY = @"WITH Q1 AS
                    (SELECT CTN.CARTON_ID            AS CARTON_ID,
                           CTN.CARTON_STORAGE_AREA   AS CARTON_STORAGE_AREA,
                           TIA.SHORT_NAME            AS SHORT_NAME,
                           CTN.LOCATION_ID           AS LOCATION_ID,
                           ctn.quality_code          AS quality_code,
                           CTN.LAST_PULLED_DATE      As LAST_PULLED_DATE,
                           tia.warehouse_location_id As warehouse_location_id,
                           max(ctn.area_change_date) over()      AS max_area_change_date,
                           min(ctn.area_change_date) over()      AS min_area_change_date,
                           ctn.vwh_id                As vwh_id,
                           ctn.suspense_date         AS suspense_date,
                           CTNDET.Quantity           As Quantity,
                           ctndet.REQ_PROCESS_ID     as REQ_PROCESS_ID, 
                           ctndet.sku_id             AS sku_id,
                           msku.style                AS style,
                           msku.color                AS color,
                           msku.dimension            AS dimension,
                           msku.sku_size             AS sku_size,
                           tqc.ORDER_QUALITY         as ORDER_QUALITY,
                           TQC.DESCRIPTION           AS QUALITY_DESCRIPTION                  
                      FROM <proxy />SRC_CARTON CTN
                      LEFT OUTER JOIN <proxy />SRC_CARTON_DETAIL CTNDET
                        ON CTN.CARTON_ID = CTNDET.CARTON_ID
                      LEFT OUTER JOIN <proxy />MASTER_SKU MSKU
                        ON CTNDET.SKU_ID = MSKU.SKU_ID
                      left outer join <proxy />tab_inventory_area tia
                        on tia.inventory_storage_area = ctn.carton_storage_area
                       left outer join <proxy />tab_quality_code tqc
                        on ctn.quality_code = tqc.quality_code
                     WHERE
                        <if>
                        CTN.pallet_id = :pallet_id
                        </if>
                        <else>
                        CTN.SUSPENSE_DATE is null
                        and CTNDET.sku_id is not null
                        </else>    
                    ),
                    Q2 AS
                     (
  --PK SKU, vwh for sku_seq = 1
  SELECT row_number() over(partition by sku_id, i.vwh_id order by i.location_id, sku_id) as sku_vwh_seq,
          MS.SKU_ID AS SKU_ID,
          I.IA_ID AS IA_ID,
          I.VWH_ID AS VWH_ID,
          I.LOCATION_ID AS LOCATION_ID,
          I.RESTOCK_AISLE_ID AS RESTOCK_AISLE,
          I.WAREHOUSE_LOCATION_ID AS WAREHOUSE_LOCATION_ID,
          IA.SHORT_DESCRIPTION AS SHORT_NAME
    FROM <proxy />IA IA
   INNER JOIN <proxy />IALOC I
      ON IA.IA_ID = I.IA_ID
   INNER JOIN <proxy />MASTER_SKU MS
      ON MS.UPC_CODE = I.ASSIGNED_UPC_CODE

)
SELECT Q1.CARTON_ID,
       Q1.CARTON_STORAGE_AREA,
       Q1.SHORT_NAME,
       Q1.LOCATION_ID,
       Q1.quality_code,
       Q1.suspense_date,
       Q1.ORDER_QUALITY,
       Q1.min_area_change_date,
       Q1.max_area_change_date,
       Q1.QUALITY_DESCRIPTION,
       Q1.LAST_PULLED_DATE,
       Q1.warehouse_location_id,
       Q1.vwh_id,
       Q1.Quantity,
       Q1.REQ_PROCESS_ID,
       Q1.sku_id,
       Q1.style,
       Q1.color,
       Q1.dimension,
       Q1.sku_size,
       Q2.LOCATION_ID           AS ASSIGNED_LOCATION,
       Q2.RESTOCK_AISLE         AS ASSIGNED_RESTOCK_AISLE,
       Q2.WAREHOUSE_LOCATION_ID AS ASSIGNED_WAREHOUSE_LOCATION,
       Q2.IA_ID                 AS ASSIGNED_IAID,
       Q2.SHORT_NAME            AS ASSIGNED_IAID_SHORT_NAME
  FROM Q1
  LEFT OUTER JOIN Q2
    ON Q1.SKU_ID = Q2.SKU_ID
   AND Q1.VWH_ID = Q2.VWH_ID
   and q2.sku_vwh_seq = 1
                                 ";
            var binder = SqlBinder.Create(row => new CartonHeadline
            {
                AreaId = row.GetString("CARTON_STORAGE_AREA"),
                AreaShortName = row.GetString("SHORT_NAME"),
                BestRestockAisleId = row.GetString("ASSIGNED_RESTOCK_AISLE"),
                BestRestockAreaId = row.GetString("ASSIGNED_IAID"),
                BestRestockAreaShortName = row.GetString("ASSIGNED_IAID_SHORT_NAME"),
                BestRestockBuildingId = row.GetString("ASSIGNED_WAREHOUSE_LOCATION"),
                //BestRestockLocationId = row.GetString("ASSIGNED_LOCATION"),
                BestSKUAssignedLocationId = row.GetString("ASSIGNED_LOCATION"),
                BuildingId = row.GetString("warehouse_location_id"),
                CartonId = row.GetString("CARTON_ID"),
                Color = row.GetString("color"),
                Dimension = row.GetString("dimension"),
                LastPulledDate = row.GetDate("LAST_PULLED_DATE"),
                LocationId = row.GetString("LOCATION_ID"),
                Pieces = row.GetInteger("Quantity"),
                QualityCode = row.GetString("quality_code"),
                SuspenseDate = row.GetDate("suspense_date"),
                MaxAreaChangeDate = row.GetDate("max_area_change_date"),
                MinAreaChangeDate = row.GetDate("min_area_change_date"),
                IsShippableQuality = !string.IsNullOrEmpty(row.GetString("ORDER_QUALITY")),
                QualityDescription = row.GetString("QUALITY_DESCRIPTION"),
                ReqProcessId = row.GetInteger("REQ_PROCESS_ID"),
                SkuSize = row.GetString("sku_size"),
                Style = row.GetString("style"),
                SkuId = row.GetInteger("sku_id"),
                VwhId = row.GetString("vwh_id"),
            }).Parameter("pallet_id", palletId);

            return _db.ExecuteReader(QUERY, binder, maxRows);
        }

    }
}