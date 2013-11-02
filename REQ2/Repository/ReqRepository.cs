using System;
using System.Collections.Generic;
using System.Web;
using DcmsMobile.REQ2.Models;
using EclipseLibrary.Oracle;

namespace DcmsMobile.REQ2.Repository
{
    public class ReqRepository : IDisposable
    {
        #region Initialization

        private OracleDatastore _db;

        /// <summary>
        /// For injecting the value through unit tests
        /// </summary>
        /// <param name="db"></param>
        public ReqRepository(OracleDatastore db)
        {
            _db = db;
        }

        /// <summary>
        /// For use in tests
        /// </summary>
        public OracleDatastore Db
        {
            get
            {
                return _db;
            }
        }

        public ReqRepository(TraceContext ctx, string connectString, string userName, string clientInfo, string moduleName)
        {
            var db = new OracleDatastore(ctx);
            db.CreateConnection(connectString, userName);

            db.ModuleName = moduleName;
            db.ClientInfo = clientInfo;
            _db = db;
        }

        public void Dispose()
        {
            if (_db != null)
            {
                _db.Dispose();
            }
        }

        #endregion

        /// <summary>
        /// Getting the list of Quality Codes
        /// </summary>
        /// <returns></returns>
        public IEnumerable<CodeDescriptionModel> GetQualityCodeList()
        {
            const string QUERY =
                @"
               SELECT QC.QUALITY_CODE                       AS QUALITY_CODE,
                       NVL(QC.DESCRIPTION, QC.QUALITY_CODE) AS DESCRIPTION
                 FROM <proxy />TAB_QUALITY_CODE QC
                ORDER BY QC.QUALITY_RANK ASC, QC.QUALITY_CODE
        ";
            var binder = SqlBinder.Create(row => new CodeDescriptionModel
            {
                Code = row.GetString("QUALITY_CODE"),
                Description = row.GetString("DESCRIPTION")
            });
            return _db.ExecuteReader(QUERY, binder);

        }


        /// <summary>
        /// Getting the list of Virtual Warehouse
        /// </summary>
        /// <returns></returns>
        public IEnumerable<CodeDescriptionModel> GetVwhList()
        {
            const string QUERY =
             @"
               SELECT VWH_ID AS VWH_ID, 
                       DESCRIPTION AS DESCRIPTION
                 FROM <proxy />TAB_VIRTUAL_WAREHOUSE
                ORDER BY VWH_ID
        ";
            var binder = SqlBinder.Create(row => new CodeDescriptionModel
            {
                Code = row.GetString("VWH_ID"),
                Description = row.GetString("DESCRIPTION"),
            });
            return _db.ExecuteReader(QUERY, binder);

        }


        /// <summary>
        /// Getting list of those Buildings which have both numbered and unnumbered areas
        /// </summary>
        /// <returns></returns>
        public IEnumerable<CodeDescriptionModel> GetBuildingList()
        {
            const string QUERY = @"
                                WITH Q1 AS 
                                (
                                  SELECT DISTINCT (TWL.WAREHOUSE_LOCATION_ID), TWL.DESCRIPTION
                                    FROM <proxy />TAB_WAREHOUSE_LOCATION TWL
                                    LEFT OUTER JOIN <proxy />TAB_INVENTORY_AREA TIA
                                      ON TIA.WAREHOUSE_LOCATION_ID = TWL.WAREHOUSE_LOCATION_ID
                                   WHERE TIA.LOCATION_NUMBERING_FLAG IS NOT NULL
                                     AND TIA.STORES_WHAT = 'CTN'
                                  INTERSECT
                                  SELECT DISTINCT (TWL.WAREHOUSE_LOCATION_ID), TWL.DESCRIPTION
                                    FROM <proxy />TAB_WAREHOUSE_LOCATION TWL
                                    LEFT OUTER JOIN <proxy />TAB_INVENTORY_AREA TIA
                                      ON TIA.WAREHOUSE_LOCATION_ID = TWL.WAREHOUSE_LOCATION_ID
                                   WHERE TIA.LOCATION_NUMBERING_FLAG IS NULL
                                     AND TIA.UNUSABLE_INVENTORY IS NULL
                                )
                                SELECT * FROM Q1 ORDER BY Q1.WAREHOUSE_LOCATION_ID
            ";
            var binder = SqlBinder.Create(row => new CodeDescriptionModel
            {
                Code = row.GetString("WAREHOUSE_LOCATION_ID"),
                Description = row.GetString("DESCRIPTION"),
            });
            return _db.ExecuteReader(QUERY, binder);
        }


        /// <summary>
        /// Getting list of sewing plant
        /// </summary>
        /// <returns></returns>
        public IEnumerable<CodeDescriptionModel> GetSewingPlants()
        {
            const string QUERY =
            @"
                SELECT SEWING_PLANT_CODE AS SEWING_PLANT_CODE,
                       SEWING_PLANT_NAME AS DESCRIPTION
                  FROM <proxy />TAB_SEWINGPLANT
                 ORDER BY SEWING_PLANT_NAME
            ";
            var binder = SqlBinder.Create(row => new CodeDescriptionModel
            {
                Code = row.GetString("SEWING_PLANT_CODE"),
                Description = row.GetString("DESCRIPTION"),
            });
            return _db.ExecuteReader(QUERY, binder);

        }


        /// <summary>
        /// Creates a new request and returns the request id.
        /// 25-1-2012:Insert IS_CONVERSION_REQUEST colomn value in table when request is for conversion.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public string CreateRequest(Request model)
        {
            //TODO: remove hardwirings of Module Code
            const string QUERY = @"
                        declare
                        Lresv_rec <proxy />pkg_ctnresv_2.resv_rec_type;
                    begin
                      Lresv_rec.ctn_resv_id := :resv_id;
                      Lresv_rec.source_area := :source_area;
                      Lresv_rec.destination_area := :destination_area;
                      Lresv_rec.vwh_id := :source_vwh_id;
                      Lresv_rec.conversion_vwh_id := :conversion_vwh_id;
                      Lresv_rec.priority := :priority;
                      Lresv_rec.quality_code := :quality_code;
                      Lresv_rec.target_quality := :target_quality;
                      Lresv_rec.module_code := 'REQ3';
                      Lresv_rec.warehouse_location_id := :warehouse_location_id;
                      Lresv_rec.price_season_code := :price_season_code;
                      Lresv_rec.sewing_plant_code := :sewing_plant_code;
                      Lresv_rec.receive_date := :receive_date;
                      Lresv_rec.is_conversion_request := :is_conversion_request;
                      Lresv_rec.remarks := :remarks;
                      :ctn_resv_id := <proxy />pkg_ctnresv_2.create_resv_id(aresv_rec =&gt; Lresv_rec);
                    end;";
            var binder = SqlBinder.Create()
                .Parameter("source_area", model.SourceAreaId)
                .Parameter("destination_area", model.DestinationArea)
                .Parameter("source_vwh_id", model.SourceVwhId)
                .Parameter("conversion_vwh_id", model.TargetVwhId)
                .Parameter("priority", model.Priority)
                .Parameter("quality_code", model.SourceQuality)
                .Parameter("target_quality", model.TargetQuality)
                .Parameter("warehouse_location_id", model.BuildingId)
                .Parameter("price_season_code", model.PriceSeasonCode)
                .Parameter("sewing_plant_code", model.SewingPlantCode)
                .Parameter("remarks", model.Remarks)
                .Parameter("receive_date", model.CartonReceivedDate)
                .Parameter("resv_id", model.CtnResvId)
                .Parameter("is_conversion_request", model.IsConversionRequest ? "Y" : "")
                .OutParameter("ctn_resv_id", val => model.CtnResvId = val)
            ;
            _db.ExecuteNonQuery(QUERY, binder);

            return model.CtnResvId;
        }

        /// <summary>
        /// Update an existing request.
        /// </summary>
        public void UpdateRequest(Request updatedRequest)
        {
            const string QUERY = @"
                    DECLARE
                      -- Non-scalar parameters require additional processing 
                      ANEW_RESV_REC <proxy />PKG_CTNRESV_2.RESV_REC_TYPE;
                    BEGIN
                      ANEW_RESV_REC.CTN_RESV_ID           := :RESV_ID;
                      ANEW_RESV_REC.SOURCE_AREA           := :SOURCE_AREA;
                      ANEW_RESV_REC.DESTINATION_AREA      := :DESTINATION_AREA;
                      ANEW_RESV_REC.VWH_ID                := :aSOURCE_VWH_ID;
                      ANEW_RESV_REC.CONVERSION_VWH_ID     := :CONVERSION_VWH_ID;
                      ANEW_RESV_REC.PRIORITY              := :PRIORITY;
                      ANEW_RESV_REC.QUALITY_CODE          := :QUALITY_CODE;
                      ANEW_RESV_REC.TARGET_QUALITY        := :TARGET_QUALITY;
                      ANEW_RESV_REC.WAREHOUSE_LOCATION_ID := :WAREHOUSE_LOCATION_ID;
                      ANEW_RESV_REC.PRICE_SEASON_CODE     := :PRICE_SEASON_CODE;
                      ANEW_RESV_REC.SEWING_PLANT_CODE     := :SEWING_PLANT_CODE;
                      ANEW_RESV_REC.RECEIVE_DATE          := :RECEIVE_DATE;
                      ANEW_RESV_REC.IS_CONVERSION_REQUEST := :IS_CONVERSION_REQUEST;
                      ANEW_RESV_REC.REMARKS               := :REMARKS;

                      -- Call the procedure
                      <proxy />PKG_CTNRESV_2.UPDATE_RESV_ID(ANEW_RESV_REC =&gt; ANEW_RESV_REC,
                                                            AROW_SEQ   =&gt; :AROW_SEQ);
                    END;";
            var binder = SqlBinder.Create()
                .Parameter("source_area", updatedRequest.SourceAreaId)
                .Parameter("destination_area", updatedRequest.DestinationArea)
                .Parameter("asource_vwh_id", updatedRequest.SourceVwhId)
                .Parameter("conversion_vwh_id", updatedRequest.TargetVwhId)
                .Parameter("priority", updatedRequest.Priority)
                .Parameter("quality_code", updatedRequest.SourceQuality)
                .Parameter("target_quality", updatedRequest.TargetQuality)
                .Parameter("warehouse_location_id", updatedRequest.BuildingId)
                .Parameter("price_season_code", updatedRequest.PriceSeasonCode)
                .Parameter("sewing_plant_code", updatedRequest.SewingPlantCode)
                .Parameter("remarks", updatedRequest.Remarks)
                .Parameter("receive_date", updatedRequest.CartonReceivedDate)
                .Parameter("resv_id", updatedRequest.CtnResvId)
                .Parameter("is_conversion_request", updatedRequest.IsConversionRequest ? "Y" : "")
                .Parameter("AROW_SEQ", updatedRequest.RowSequence)
                ;
            _db.ExecuteNonQuery(QUERY, binder);
        }

        /// <summary>
        /// Delete the existing Request 
        /// </summary>
        /// <param name="ctnresvId"></param>
        public void DeleteCartonRequest(string ctnresvId)
        {
            const string QUERY =
                            @"
                            begin
                                <proxy />pkg_ctnresv_2.delete_ctnresv(actn_resv_id =&gt; :ctn_resv_id);
                            end;
                        ";
            var binder = SqlBinder.Create().Parameter("ctn_resv_id", ctnresvId);
            _db.ExecuteNonQuery(QUERY, binder);
        }

        /// <summary>
        /// Getting the list of price season codes
        /// </summary>
        /// <returns></returns>
        public IEnumerable<CodeDescriptionModel> GetPriceSeasonCodes()
        {
            const string QUERY =
                 @"
               SELECT Price_Season_code AS Price_Season_code, 
                       DESCRIPTION AS DESCRIPTION
                 FROM <proxy />TAB_Price_Season ORDER BY Price_Season_code
        ";
            var binder = SqlBinder.Create(row => new CodeDescriptionModel
            {
                Code = row.GetString("Price_Season_code"),
                Description = row.GetString("DESCRIPTION"),
            });
            return _db.ExecuteReader(QUERY, binder);
        }

        /// <summary>
        /// Gets top 20 recently created requests.
        /// 25-1-2012: Showing IS_CONVERSION_REQUEST column value.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Request> GetRequests(string ctnResvId, int maxRows)
        {
            const string QUERY = @"
                      WITH CARTON_RESERVATIONS AS
                             (SELECT T.CTN_RESV_ID      AS CTN_RESV_ID,
                                     ROW_NUMBER() OVER(PARTITION BY T.CTN_RESV_ID ORDER BY T.CTN_RESV_ID) AS SEQUENCE_WITHIN_RESV_ID,
                                     DENSE_RANK() OVER(ORDER BY T.INSERT_DATE DESC, T.PRIORITY DESC) AS RANK_BY_CREATION,
                                     T.PRIORITY         AS PRIORITY,
                                     T.TARGET_QUALITY   AS TARGET_QUALITY,
                                     T.SOURCE_AREA      AS SOURCE_AREA,
                                     T.DESTINATION_AREA AS DESTINATION_AREA,
                                     T.VWH_ID           AS VWH_ID,
                                     T.CONVERSION_VWH_ID AS CONVERSION_VWH_ID,
                                     T.SEWING_PLANT_CODE AS SEWING_PLANT_CODE,
                                     T.QUALITY_CODE AS QUALITY_CODE,
                                     T.PRICE_SEASON_CODE AS PRICE_SEASON_CODE,
                                     T.RECEIVE_DATE AS RECEIVE_DATE,
                                     T.WAREHOUSE_LOCATION_ID AS WAREHOUSE_LOCATION_ID,
                                     SUM(CD.QUANTITY_REQUESTED) OVER(PARTITION BY T.CTN_RESV_ID) AS QUANTITY_REQUESTED,
                                     T.INSERT_DATE AS INSERT_DATE,
                                     T.REMARKS AS REMARKS,
                                     T.INSERTED_BY AS INSERTED_BY,
                                     T.IS_CONVERSION_REQUEST AS IS_CONVERSION_REQUEST,
                                     COUNT(UNIQUE MS.SKU_ID) OVER(PARTITION BY T.CTN_RESV_ID) AS REQUESTED_SKU_COUNT,
                                     T.ASSIGN_DATE AS ASSIGN_DATE,
                                     T.ORA_ROWSCN AS ROW_SEQUENCE
                                FROM <proxy />CTNRESV T
                               LEFT OUTER JOIN <proxy />CTNRESV_DETAIL CD
                                  ON T.CTN_RESV_ID = CD.CTN_RESV_ID
                                LEFT OUTER JOIN <proxy />MASTER_SKU MS
                                  ON MS.SKU_ID = CD.SKU_ID
                               WHERE T.MODULE_CODE = 'REQ3'
                              <if>AND T.CTN_RESV_ID = :CTN_RESV_ID</if>
                              ),

                            ASSIGNED_CARTON_SUMMARY AS
                             (SELECT C.CTN_RESV_ID AS CTN_RESV_ID,
                                     COUNT(CASE
                                             WHEN CPC.IS_PULLED = 'Y' THEN
                                              CPC.CARTON_ID
                                           END) AS PULLED_CARTONS,
                                     COUNT(CASE
                                             WHEN SC.WORK_NEEDED_XML IS NOT NULL THEN
                                              SC.CARTON_ID
                                           END) AS REWORK_CARTON_COUNT,
                                     SUM(NVL(CPC.QUANTITY, 0)) AS TOTAL_PIECES_IN_CARTONS,
                                     COUNT(DISTINCT(CPC.CARTON_ID)) AS CARTON_COUNT
                                FROM <proxy />CTNRESV C
                               INNER JOIN <proxy />CTNRESV_DETAIL CD
                                  ON C.CTN_RESV_ID = CD.CTN_RESV_ID
                               INNER JOIN <proxy />CTNRESV_PULL_CARTON CPC
                                  ON CPC.REQ_PROCESS_ID = CD.REQ_PROCESS_ID
                               INNER JOIN <proxy />SRC_CARTON SC
                                  ON SC.CARTON_ID = CPC.CARTON_ID
                               GROUP BY C.CTN_RESV_ID)
                            SELECT REQ.CTN_RESV_ID             AS CTN_RESV_ID,
                                   REQ.SOURCE_AREA             AS SOURCE_AREA,
                                   TIA.SHORT_NAME              AS SOURCE_AREA_SHORT_NAME,
                                   REQ.DESTINATION_AREA        AS DESTINATION_AREA,
                                   TIA2.SHORT_NAME             AS DESTINATION_AREA_SHORT_NAME,
                                   REQ.VWH_ID                  AS VWH_ID,
                                   REQ.CONVERSION_VWH_ID       AS CONVERSION_VWH_ID,
                                   REQ.SEWING_PLANT_CODE       AS SEWING_PLANT_CODE,
                                   REQ.QUALITY_CODE            AS QUALITY_CODE,
                                   REQ.PRICE_SEASON_CODE       AS PRICE_SEASON_CODE,
                                   REQ.RECEIVE_DATE            AS RECEIVE_DATE,
                                   REQ.WAREHOUSE_LOCATION_ID   AS WAREHOUSE_LOCATION_ID,
                                   REQ.QUANTITY_REQUESTED      AS QUANTITY_REQUESTED,
                                   REQ.INSERT_DATE             AS INSERT_DATE,
                                   REQ.REMARKS                 AS REMARKS,
                                   REQ.INSERTED_BY             AS INSERTED_BY,
                                   REQ.PRIORITY                AS PRIORITY,
                                   REQ.TARGET_QUALITY          AS TARGET_QUALITY,
                                   CTN.CARTON_COUNT            AS CARTON_COUNT,
                                   REQ.IS_CONVERSION_REQUEST   AS IS_CONVERSION_REQUEST,
                                   CTN.TOTAL_PIECES_IN_CARTONS AS TOTAL_PIECES_IN_CARTONS,
                                   REQ.REQUESTED_SKU_COUNT     AS REQUESTED_SKU_COUNT,
                                   CTN.PULLED_CARTONS          AS PULLED_CARTONS,
                                   REQ.ASSIGN_DATE             AS ASSIGN_DATE,
                                   REQ.ROW_SEQUENCE            AS ROW_SEQUENCE,     
                                   CTN.REWORK_CARTON_COUNT     AS REWORK_CARTON_COUNT
                              FROM CARTON_RESERVATIONS REQ
                              LEFT OUTER JOIN ASSIGNED_CARTON_SUMMARY CTN
                                ON CTN.CTN_RESV_ID = REQ.CTN_RESV_ID
                              LEFT OUTER JOIN <proxy />TAB_INVENTORY_AREA TIA
                                ON TIA.INVENTORY_STORAGE_AREA = REQ.SOURCE_AREA
                              LEFT OUTER JOIN <proxy />TAB_INVENTORY_AREA TIA2
                                ON TIA2.INVENTORY_STORAGE_AREA = REQ.DESTINATION_AREA
                             WHERE REQ.SEQUENCE_WITHIN_RESV_ID = 1
                             AND REQ.RANK_BY_CREATION &lt;= :MAX_ROWS
                              ORDER BY REQ.RANK_BY_CREATION
            ";
            var binder = SqlBinder.Create(row => new Request
            {
                CtnResvId = row.GetString("CTN_RESV_ID"),
                SourceAreaId = row.GetString("SOURCE_AREA"),
                SourceAreaShortName = row.GetString("SOURCE_AREA_SHORT_NAME"),
                BuildingId = row.GetString("WAREHOUSE_LOCATION_ID"),
                SourceVwhId = row.GetString("VWH_ID"),
                Priority = row.GetString("PRIORITY"),
                DateCreated = row.GetDate("INSERT_DATE"),
                Remarks = row.GetString("REMARKS"),
                RequestedBy = row.GetString("INSERTED_BY"),
                DestinationArea = row.GetString("DESTINATION_AREA"),
                DestinationAreaShortName = row.GetString("DESTINATION_AREA_SHORT_NAME"),
                QuantityRequested = row.GetInteger("QUANTITY_REQUESTED") ?? 0,
                TargetVwhId = row.GetString("CONVERSION_VWH_ID"),
                SewingPlantCode = row.GetString("SEWING_PLANT_CODE"),
                SourceQuality = row.GetString("QUALITY_CODE"),
                PriceSeasonCode = row.GetString("PRICE_SEASON_CODE"),
                CartonReceivedDate = row.GetDate("RECEIVE_DATE"),
                AssignedCartonCount = row.GetInteger("CARTON_COUNT") ?? 0,
                AssignedPieces = row.GetInteger("TOTAL_PIECES_IN_CARTONS") ?? 0,
                TargetQuality = row.GetString("TARGET_QUALITY"),
                IsConversionRequest = row.GetString("IS_CONVERSION_REQUEST") == "Y",
                RequestedSkuCount = row.GetInteger("REQUESTED_SKU_COUNT"),
                PulledCartons = row.GetInteger("PULLED_CARTONS"),
                AssignDate = row.GetDate("ASSIGN_DATE"),
                RowSequence = row.GetDecimal("ROW_SEQUENCE"),
                ReworkCartonCount = row.GetInteger("REWORK_CARTON_COUNT")
            }).Parameter("CTN_RESV_ID", ctnResvId)
            .Parameter("max_rows", maxRows);
            return _db.ExecuteReader(QUERY, binder);
        }

        /// <summary>
        /// Returns the all SKUs of a request
        /// </summary>
        /// <param name="ctnresvId"></param>
        /// <returns></returns>
        public IEnumerable<RequestSku> GetRequestSkus(string ctnresvId)
        {
            const string QUERY = @"SELECT max(MSKU.STYLE) AS STYLE,
                                       MAX(MSKU.COLOR) AS COLOR,
                                       MAX(MSKU.DIMENSION) AS DIMENSION,
                                       MAX(MSKU.SKU_SIZE) AS SKU_SIZE,
                                       REQDET.SKU_ID AS SKU_ID,
                                       MAX(MSKUCONV.STYLE) AS CON_STYLE_,
                                       MAX(MSKUCONV.COLOR) AS CON_COLOR_,
                                       MAX(MSKUCONV.DIMENSION) AS CON_DIMENSION_,
                                       MAX(MSKUCONV.SKU_SIZE) AS CON_SKU_SIZE_,
                                       MAX(MSKUCONV.SKU_ID) AS CON_SKU_ID,
                                       MAX(REQDET.QUANTITY_REQUESTED) AS QUANTITY_REQUESTED,
                                       COUNT(DISTINCT CPC.CARTON_ID) AS TOTAL_CARTONS,
                                       COUNT(CASE
                                               WHEN CPC.IS_PULLED = 'Y' THEN
                                                CPC.CARTON_ID
                                               ELSE
                                                NULL
                                             END) AS PULLED_CARTONS,
                                       SUM(CPC.QUANTITY) AS NUM_PIECES
                                  FROM <proxy />CTNRESV_DETAIL REQDET
                                 INNER JOIN <proxy />CTNRESV C
                                    ON C.CTN_RESV_ID = REQDET.CTN_RESV_ID
                                  LEFT OUTER JOIN <proxy />MASTER_SKU MSKU
                                    ON MSKU.SKU_ID = REQDET.SKU_ID
                                  LEFT OUTER JOIN <proxy />MASTER_SKU MSKUCONV
                                    ON MSKUCONV.SKU_ID = REQDET.TARGET_SKU_ID
                                  LEFT OUTER JOIN <proxy />CTNRESV_PULL_CARTON CPC
                                    ON CPC.REQ_PROCESS_ID = REQDET.REQ_PROCESS_ID
                                 WHERE C.CTN_RESV_ID = :ctnresv_id
                                 GROUP BY C.CTN_RESV_ID, C.MODULE_CODE, REQDET.SKU_ID
                         ORDER BY MAX(REQDET.INSERT_DATE) DESC

        ";
            var binder = SqlBinder.Create(row => new RequestSku
            {
                Pieces = row.GetInteger("QUANTITY_REQUESTED") ?? 0,
                PulledCartons = row.GetInteger("PULLED_CARTONS"),
                TotalCartons = row.GetInteger("TOTAL_CARTONS"),
                AssignedPieces = row.GetInteger("NUM_PIECES"),
                SourceSku = new Sku
                {
                    Style = row.GetString("STYLE"),
                    Color = row.GetString("COLOR"),
                    Dimension = row.GetString("DIMENSION"),
                    SkuSize = row.GetString("SKU_SIZE"),
                    SkuId = row.GetInteger("SKU_ID") ?? 0,
                },

                TargetSku = row.GetInteger("CON_SKU_ID") != null ? new Sku
                {
                    Style = row.GetString("CON_STYLE_"),
                    Color = row.GetString("CON_COLOR_"),
                    Dimension = row.GetString("CON_DIMENSION_"),
                    SkuSize = row.GetString("CON_SKU_SIZE_"),
                    SkuId = row.GetInteger("CON_SKU_ID") ?? 0
                } : null
            }).Parameter("ctnresv_id", ctnresvId);
            var result = _db.ExecuteReader(QUERY, binder);
            return result;
        }


        public IEnumerable<CartonList> GetCartonList(string ctnresvId)
        {
            const string QUERY =
                              @"SELECT SRC.CARTON_ID          AS CARTON_ID,
                                       CTNRESV.CTN_RESV_ID    AS CTN_RESV_ID,
                                       SC.QUALITY_CODE        AS QUALITY_CODE,
                                       SC.VWH_ID              AS VWH_ID,
                                       SRC.QUANTITY           AS QUANTITY,
                                       SC.WORK_NEEDED_XML     AS REWORK_NEEDED,
                                       SC.CARTON_STORAGE_AREA AS INVENTORY_STORAGE_AREA,
                                       TIA.SHORT_NAME         AS SHORT_NAME,
                                       TIA.DESCRIPTION        AS AREA_DESCRIPTION
                                  FROM <proxy />CTNRESV_PULL_CARTON SRC
                                  LEFT OUTER JOIN <proxy />SRC_CARTON SC
                                    ON SRC.CARTON_ID = SC.CARTON_ID
                                 INNER JOIN <proxy />TAB_INVENTORY_AREA TIA
                                    ON TIA.INVENTORY_STORAGE_AREA = SC.CARTON_STORAGE_AREA
                                  LEFT OUTER JOIN <proxy />CTNRESV_DETAIL REQDET
                                    ON SRC.REQ_PROCESS_ID = REQDET.REQ_PROCESS_ID
                                  LEFT OUTER JOIN <proxy />CTNRESV CTNRESV
                                    ON REQDET.CTN_RESV_ID = CTNRESV.CTN_RESV_ID
                                 WHERE CTNRESV.CTN_RESV_ID = :CTN_RESV_ID

  ";
            var binder = SqlBinder.Create(row => new CartonList
            {
                CartonId = row.GetString("CARTON_ID"),
                AreaShortName = row.GetString("SHORT_NAME"),
                StoregeArea = row.GetString("INVENTORY_STORAGE_AREA"),
                AreaDescription = row.GetString("AREA_DESCRIPTION"),
                QuilityCode = row.GetString("QUALITY_CODE"),
                VwhId = row.GetString("VWH_ID"),
                CtnresvId = row.GetString("CTN_RESV_ID"),
                Quantity = row.GetInteger("QUANTITY") ?? 0,
                ReworkNeeded = row.GetString("REWORK_NEEDED")
            }).Parameter("CTN_RESV_ID", ctnresvId);
            return _db.ExecuteReader(QUERY, binder);
        }

        /// <summary>
        /// Adds SKU to passed request
        /// </summary>
        /// <param name="ctnresvId"></param>
        /// <param name="skuId"></param>
        /// <param name="pieces">If the SKU already exists in the request, the existing pieces are replaced by the passed pieces</param>
        /// <param name="conversionSkuId"></param>
        public int AddSkutoRequest(string ctnresvId, int skuId, int pieces, int? conversionSkuId)
        {
            const string QUERY = @"
               begin
                       :result := <proxy />pkg_ctnresv_2.MERGE_SKU(actn_resv_id =&gt; :ctn_resv_id,
                                          asku_id =&gt; :sku_id,
                                          arequired_pieces =&gt; :quantity_requested,
                                          atarget_sku_id =&gt; :conversion_sku_id);
                    end;";
            int count = 0;
            var binder = SqlBinder.Create().Parameter("ctn_resv_id", ctnresvId)
                  .Parameter("sku_id", skuId)
                  .Parameter("quantity_requested", pieces)
                  .Parameter("conversion_sku_id", conversionSkuId);
            binder.OutParameter("result", val => count = val ?? 0);
            _db.ExecuteNonQuery(QUERY, binder);
            return count;
        }

        /// <summary>
        /// Delete an SKU from a request. Also delete any cartons assigned to this request which contain this SKU. Do not delete pulled cartons.
        /// </summary>
        /// <param name="skuId"></param>
        /// <param name="ctnresvId"></param>
        public void DeleteSkuFromRequest(int skuId, string ctnresvId)
        {
            const string QUERY = @"
    DECLARE
           Lresult number;
        BEGIN
           Lresult := <proxy /> PKG_CTNRESV_2.MERGE_SKU(ACTN_RESV_ID        =&gt; :ACTN_RESV_ID,
                                                        ASKU_ID             =&gt; :ASKU_ID,
                                                        AREQUIRED_PIECES    =&gt; 0,
                                                        ATARGET_SKU_ID      =&gt; NULL);
            END;";

            var binder = SqlBinder.Create()
                .Parameter("ACTN_RESV_ID", ctnresvId)
                .Parameter("ASKU_ID", skuId);
            _db.ExecuteNonQuery(QUERY, binder);
        }

        /// <summary>
        /// This method Assigns carton to the request
        /// </summary>
        public int AssignCartons(string ctnresvId)
        {
            const string QUERY = @"
                BEGIN
                  :RESULT :=  <proxy />pkg_ctnresv_2.assign(actn_resv_id =&gt; :ctn_resv_id);
                END;";
            int pieces = 0;
            var binder = SqlBinder.Create().Parameter("ctn_resv_id", ctnresvId);
            binder.OutParameter("result", val => pieces = val ?? 0);
            _db.ExecuteNonQuery(QUERY, binder);
            return pieces;
        }

        /// <summary>
        /// This method is used to unassigned the cartons from a request
        /// </summary>
        /// <param name="ctnresvId"></param>
        public int UnAssignCartons(string ctnresvId)
        {
            const string QUERY =
                            @"
                            BEGIN
                                :result := <proxy />pkg_ctnresv_2.unassign(actn_resv_id =&gt; :ctn_resv_id);
                            END;
                        ";
            int cartons = 0;
            var binder = SqlBinder.Create().Parameter("ctn_resv_id", ctnresvId);
            binder.OutParameter("result", val => cartons = val ?? 0);
            _db.ExecuteNonQuery(QUERY, binder);
            return cartons;
        }

        /// <summary>
        /// This method returns all value in SKuModel.
        /// </summary>
        /// <param name="style"></param>
        /// <param name="color"></param>
        /// <param name="dimension"></param>
        /// <param name="skuSize"></param>
        /// <returns>
        /// Return style,color,dimension,SKuSize and SKuId.
        /// </returns>
        public Sku GetSku(string style, string color, string dimension, string skuSize)
        {
            const string QUERY =
                             @" SELECT MSKU.SKU_ID      AS SKU_ID,
                                       MSKU.STYLE       AS STYLE,
                                       MSKU.COLOR       AS COLOR,
                                       MSKU.DIMENSION   AS DIMENSION,
                                       MSKU.SKU_SIZE    AS SKU_SIZE
                                 FROM <proxy />MASTER_SKU MSKU
                                      WHERE MSKU.STYLE = :STYLE
                                        AND MSKU.COLOR = :COLOR
                                        AND MSKU.DIMENSION = :DIMENSION
                                        AND MSKU.SKU_SIZE = :SKU_SIZE";

            var binder = SqlBinder.Create(row => new Sku
            {
                Style = row.GetString("STYLE"),
                Color = row.GetString("COLOR"),
                Dimension = row.GetString("DIMENSION"),
                SkuSize = row.GetString("SKU_SIZE"),
                SkuId = row.GetInteger("SKU_ID").Value
            }).Parameter("STYLE", style)
            .Parameter("COLOR", color)
            .Parameter("DIMENSION", dimension)
            .Parameter("SKU_SIZE", skuSize);
            return _db.ExecuteSingle(QUERY, binder);
        }

        /// <summary>
        /// Returns all carton areas
        /// 25-1-2012: Removing conversion area.Now conversion can be done in any area.
        /// </summary>
        /// <returns></returns>
        internal IEnumerable<CartonArea> GetCartonAreas(string buildingId)
        {
            const string QUERY =
                @"
                    SELECT TIA.INVENTORY_STORAGE_AREA AS INVENTORY_STORAGE_AREA,
                       MAX(TIA.DESCRIPTION)           AS DESCRIPTION,
                       MAX(TIA.SHORT_NAME)            AS SHORT_NAME,
                       MAX(TIA.LOCATION_NUMBERING_FLAG) AS LOCATION_NUMBERING_FLAG,
                       MAX(TIA.WAREHOUSE_LOCATION_ID) AS WAREHOUSE_LOCATION_ID,
                       COUNT(UNIQUE CTN.CARTON_ID) AS CARTON_COUNT,
                       MAX(TIA.IS_CONVERSION_AREA)    AS REWORK_AREA
                  FROM <proxy />SRC_CARTON CTN
                 INNER JOIN <proxy />TAB_INVENTORY_AREA TIA
                    ON TIA.INVENTORY_STORAGE_AREA = CTN.CARTON_STORAGE_AREA
                         WHERE TIA.WAREHOUSE_LOCATION_ID  = :WAREHOUSE_LOCATION_ID
                         AND TIA.STORES_WHAT = 'CTN'
                AND TIA.UNUSABLE_INVENTORY IS NULL
                    GROUP BY TIA.INVENTORY_STORAGE_AREA
                 ORDER BY TIA.INVENTORY_STORAGE_AREA
        ";
            var binder = SqlBinder.Create(row => new CartonArea
            {
                AreaId = row.GetString("INVENTORY_STORAGE_AREA"),
                Description = row.GetString("DESCRIPTION"),
                ShortName = row.GetString("SHORT_NAME"),
                BuildingId = row.GetString("WAREHOUSE_LOCATION_ID"),
                LocationNumberingFlag = row.GetString("LOCATION_NUMBERING_FLAG") == "Y",
                IsReworkArea = row.GetString("REWORK_AREA") == "Y",
                CartonCount = row.GetInteger("CARTON_COUNT").Value
            }).Parameter("WAREHOUSE_LOCATION_ID", buildingId);
            return _db.ExecuteReader(QUERY, binder);

        }
    }
}

//$Id$


