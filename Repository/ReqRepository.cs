using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.Contracts;
using System.Web;
using DcmsMobile.REQ2.Models;
using EclipseLibrary.Oracle;
using EclipseLibrary.Oracle.Helpers;

namespace DcmsMobile.REQ2.Repository
{


    public class ReqRepository : IDisposable
    {
        #region Intialization

        private readonly OracleDatastore _db;

        public OracleDatastore Db
        {
            get
            {
                return _db;
            }
        }

        /// <summary>
        /// Constructor of class used to create the connection to database.
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="moduleName"></param>
        /// <param name="clientInfo"></param>
        /// <param name="trace"></param>
        public ReqRepository(string userName, string moduleName, string clientInfo, TraceContext trace)
        {

            const string MODULE_CODE = "REQ2";
            Contract.Assert(ConfigurationManager.ConnectionStrings["dcms8"] != null);
            var store = new OracleDatastore(trace);
            store.CreateConnection(ConfigurationManager.ConnectionStrings["dcms8"].ConnectionString,
                userName);
            store.ModuleName = MODULE_CODE;
            store.ClientInfo = clientInfo;
            _db = store;
        }

        /// <summary>
        /// For use in unit tests
        /// </summary>
        /// <param name="db"></param>
        public ReqRepository(OracleDatastore db)
        {
            _db = db;
        }

        public void Dispose()
        {
            var dis = _db as IDisposable;
            if (dis != null)
            {
                dis.Dispose();
            }
        }

        #endregion


        /// <summary>
        /// Returns all carton areas
        /// 25-1-2012: Removing conversion area.Now conversion can be done in any area.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<CartonArea> GetCartonAreas(string areaId)
        {
            const string QUERY =
                @"
                SELECT TIA.INVENTORY_STORAGE_AREA   AS INVENTORY_STORAGE_AREA,
                       TIA.DESCRIPTION              AS DESCRIPTION,
                       TIA.SHORT_NAME               AS SHORT_NAME,
                       TIA.STORES_WHAT              AS  STORES_WHAT,
                       TIA.LOCATION_NUMBERING_FLAG  AS LOCATION_NUMBERING_FLAG,
                       TIA.WAREHOUSE_LOCATION_ID    AS WAREHOUSE_LOCATION_ID,
                       TIA.UNUSABLE_INVENTORY       AS UNUSABLE_INVENTORY
                  FROM <proxy />TAB_INVENTORY_AREA TIA
               <if>
                WHERE TIA.INVENTORY_STORAGE_AREA  = :INVENTORY_STORAGE_AREA
                </if>
                 ORDER BY TIA.INVENTORY_STORAGE_AREA
        ";
            var binder = SqlBinder.Create(row => new CartonArea
            {
                AreaId = row.GetString("INVENTORY_STORAGE_AREA"),
                Description = row.GetString("DESCRIPTION"),
                ShortName = row.GetString("SHORT_NAME"),
                BuildingId = row.GetString("WAREHOUSE_LOCATION_ID"),
                LocationNumberingFlag = row.GetString("LOCATION_NUMBERING_FLAG") == "Y",
                UnusableInventory = row.GetString("UNUSABLE_INVENTORY") == "Y",
                IsCartonArea = row.GetString("STORES_WHAT") == "CTN"
            }).Parameter("INVENTORY_STORAGE_AREA", areaId);
            return _db.ExecuteReader(QUERY, binder);

        }

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
        /// Get sale types
        /// </summary>
        /// <returns></returns>
        public IEnumerable<CodeDescriptionModel> GetSaleTypeList()
        {
            const string QUERY =
                @"
               SELECT T.SALES_TYPE_ID    AS SALES_TYPE_ID,
                      T.DESCRIPTION     AS DESCRIPTION
                FROM <proxy />TAB_SALE_TYPE T
                WHERE T.INACTIVE_FLAG IS NULL ORDER BY SALES_TYPE_ID
        ";
            var binder = SqlBinder.Create(row => new CodeDescriptionModel
            {
                Code = row.GetString("SALES_TYPE_ID"),
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
        /// Getting list of Building.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<CodeDescriptionModel> GetBuildingList()
        {
            const string QUERY = @"
            SELECT TWL.WAREHOUSE_LOCATION_ID AS WAREHOUSE_LOCATION_ID,
                    TWL.DESCRIPTION          AS DESCRIPTION
              FROM <proxy />TAB_WAREHOUSE_LOCATION TWL
             ORDER BY WAREHOUSE_LOCATION_ID
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
        /// Getting Building of passed area
        /// </summary>
        /// <returns></returns>
        public string GetBuildingofArea(string areaId)
        {
            const string QUERY =
        @"SELECT TIA.Warehouse_Location_Id FROM <proxy />TAB_INVENTORY_AREA TIA
          WHERE TIA.INVENTORY_STORAGE_AREA = :areaId
        ";
            var binder = SqlBinder.Create(row => row.GetString("Warehouse_Location_Id")).Parameter("areaId", areaId);
            return _db.ExecuteSingle(QUERY, binder);
        }

        /// <summary>
        /// This method is use for find ctnresvId
        /// </summary>
        /// <param name="reqId"></param>
        /// <returns></returns>
//        public string GetCtnRevId(string reqId)
//        {
//            const string QUERY =
//           @"  select c.ctn_resv_id
//             from  <proxy />ctnresv c where c.ctn_resv_id=:dcms4_req_id";
//            var binder = SqlBinder.Create(row => row.GetString("ctn_resv_id")).Parameter("dcms4_req_id", reqId);
//            return _db.ExecuteSingle(QUERY, binder);
//            return reqId;
//        }

        /// <summary>
        /// Creates a new request and returns the request id.
        /// 25-1-2012:Insert IS_CONVERSION_REQUEST colomn value in table when request is for conversion.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public void CreateCartonRequest(RequestModel model)
        {
            //TODO: remove hardwirings of Module Code
            const string QUERY = @"
                        declare
                        Lresv_rec <proxy />pkg_ctnresv.resv_rec_type;
                    begin
                      Lresv_rec.ctn_resv_id := :resv_id;
                      Lresv_rec.source_area := :source_area;
                      Lresv_rec.destination_area := :destination_area;
                      Lresv_rec.pieces_constraint := :pieces_constraint;
                      Lresv_rec.vwh_id := :source_vwh_id;
                      Lresv_rec.conversion_vwh_id := :conversion_vwh_id;
                      Lresv_rec.priority := :priority;
                      Lresv_rec.quality_code := :quality_code;
                      Lresv_rec.target_quality := :target_quality;
                      Lresv_rec.module_code := 'REQ2';
                      Lresv_rec.warehouse_location_id := :warehouse_location_id;
                      Lresv_rec.packaging_preference := :packaging_preference;
                      Lresv_rec.sale_type_id := :sale_type_id;
                      Lresv_rec.price_season_code := :price_season_code;
                      Lresv_rec.sewing_plant_code := :sewing_plant_code;
                      Lresv_rec.receive_date := :receive_date;
                      Lresv_rec.is_conversion_request := :is_conversion_request;
                      Lresv_rec.remarks := :remarks;
                      :ctn_resv_id := <proxy />pkg_ctnresv.create_resv_id(aresv_rec =&gt; Lresv_rec);
                    end;";
            var binder = SqlBinder.Create()
                .Parameter("source_area", model.SourceAreaId)
                .Parameter("destination_area", model.DestinationArea)
                .Parameter("pieces_constraint", model.AllowOverPulling)
                .Parameter("source_vwh_id", model.SourceVwhId)
                .Parameter("conversion_vwh_id", model.TargetVwhId)
                .Parameter("priority", model.Priority)
                .Parameter("quality_code", model.SourceQuality)
                .Parameter("target_quality", model.TargetQuality)
                .Parameter("warehouse_location_id", model.BuildingId)
                .Parameter("packaging_preference", model.PackagingPreferance)
                .Parameter("sale_type_id", model.SaleTypeId)
                .Parameter("price_season_code", model.PriceSeasonCode)
                .Parameter("sewing_plant_code", model.SewingPlantCode)
                .Parameter("remarks", model.Remarks)
                .Parameter("receive_date", model.CartonReceivedDate)
                .Parameter("resv_id", model.CtnResvId)
                .Parameter("is_conversion_request", model.IsConversionRequest ? "Y" : "")
                .OutParameter("ctn_resv_id", val => model.CtnResvId = val)
            ;

            _db.ExecuteNonQuery(QUERY, binder);

            return;
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
                                <proxy />pkg_ctnresv.delete_ctnresv(actn_resv_id =&gt; :ctn_resv_id);
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
        public IEnumerable<RequestModel> GetRequests(string ctnResvId, int maxRows)
        {
            const string QUERY = @"
            WITH REQUESTS AS
             (SELECT T.CTN_RESV_ID                                 AS CTN_RESV_ID,
                     ROW_NUMBER() OVER(ORDER BY MAX(T.INSERT_DATE) DESC, MAX(T.PRIORITY) DESC, T.CTN_RESV_ID) AS ROW_SEQUENCE,
                     MAX(T.PRIORITY)                               AS PRIORITY,
                     MAX(T.TARGET_QUALITY)                         AS TARGET_QUALITY,
                     MAX(T.SOURCE_AREA)                            AS SOURCE_AREA,
                     MAX(T.DESTINATION_AREA)                       AS DESTINATION_AREA,
                     MAX(T.VWH_ID)                                 AS VWH_ID,
                     MAX(T.CONVERSION_VWH_ID)                      AS CONVERSION_VWH_ID,
                     MAX(T.SEWING_PLANT_CODE)                      AS SEWING_PLANT_CODE,
                     MAX(T.QUALITY_CODE)                           AS QUALITY_CODE,
                     MAX(T.ASSIGNED_FLAG)                          AS ASSIGNED_FLAG,
                     MAX(T.PIECES_CONSTRAINT)                      AS OVERPULLING,
                     MAX(T.PACKAGING_PREFERENCE)                   AS PACKAGING_PREFERENCE,
                     MAX(T.PRICE_SEASON_CODE)                      AS PRICE_SEASON_CODE,
                     MAX(T.SALE_TYPE_ID)                           AS SALE_TYPE_ID,
                     MAX(T.RECEIVE_DATE)                           AS RECEIVE_DATE,
                     MAX(T.WAREHOUSE_LOCATION_ID)                  AS WAREHOUSE_LOCATION_ID,
                     SUM(REQDET.QUANTITY_REQUESTED)                AS QUANTITY_REQUESTED,
                     MAX(T.INSERT_DATE)                            AS INSERT_DATE,
                     MAX(T.REMARKS)                                AS REMARKS,
                     MAX(T.INSERTED_BY)                            AS INSERTED_BY,
                     MAX(T.IS_CONVERSION_REQUEST)                  AS IS_CONVERSION_REQUEST,
                     MAX(REQDET.REQ_PROCESS_ID)             AS REQ_PROCESS_ID
                FROM <proxy />CTNRESV T
                LEFT OUTER JOIN <proxy />SRC_REQ_DETAIL REQDET
                  ON T.CTN_RESV_ID = REQDET.CTN_RESV_ID
               <if>
             Where T.CTN_RESV_ID = :CTN_RESV_ID
               </if>
               GROUP BY T.CTN_RESV_ID),
            CARTONS AS
             (SELECT COUNT(*)                AS CARTON_COUNT,
                     CTNDET.REQ_PROCESS_ID   AS REQ_PROCESS_ID,
                     SUM(CTNDET.QUANTITY)    AS TOTAL_PIECES
                FROM <proxy />SRC_CARTON_DETAIL CTNDET
               WHERE CTNDET.REQ_MODULE_CODE = 'REQ2'
               GROUP BY CTNDET.REQ_PROCESS_ID)
            SELECT REQ.CTN_RESV_ID           AS CTN_RESV_ID,
                   REQ.SOURCE_AREA           AS SOURCE_AREA,
                   TIA.SHORT_NAME            AS SOURCE_AREA_SHORT_NAME,
                   REQ.DESTINATION_AREA      AS DESTINATION_AREA,
                   TIA2.SHORT_NAME           AS DESTINATION_AREA_SHORT_NAME,
                   REQ.VWH_ID                AS VWH_ID,
                   REQ.CONVERSION_VWH_ID     AS CONVERSION_VWH_ID,
                   REQ.SEWING_PLANT_CODE     AS SEWING_PLANT_CODE,
                   REQ.QUALITY_CODE          AS QUALITY_CODE,
                   REQ.ASSIGNED_FLAG         AS ASSIGNED_FLAG,
                   REQ.OVERPULLING           AS OVERPULLING,
                   REQ.PACKAGING_PREFERENCE  AS PACKAGING_PREFERENCE,
                   REQ.PRICE_SEASON_CODE     AS PRICE_SEASON_CODE,
                   REQ.SALE_TYPE_ID          AS SALE_TYPE_ID,
                   REQ.RECEIVE_DATE          AS RECEIVE_DATE,
                   REQ.WAREHOUSE_LOCATION_ID AS WAREHOUSE_LOCATION_ID,
                   REQ.QUANTITY_REQUESTED    AS QUANTITY_REQUESTED,
                   REQ.INSERT_DATE           AS INSERT_DATE,
                   REQ.REMARKS               AS REMARKS,
                   REQ.INSERTED_BY           AS INSERTED_BY,
                   REQ.PRIORITY              AS PRIORITY,
                   REQ.TARGET_QUALITY        AS TARGET_QUALITY,
                   CTN.CARTON_COUNT          AS CARTON_COUNT,
                   REQ.IS_CONVERSION_REQUEST AS IS_CONVERSION_REQUEST,
                   CTN.TOTAL_PIECES          AS TOTAL_PIECES
              FROM REQUESTS REQ
              LEFT OUTER JOIN CARTONS CTN
                ON CTN.REQ_PROCESS_ID = REQ.REQ_PROCESS_ID
              LEFT OUTER JOIN <proxy />TAB_INVENTORY_AREA TIA
                ON TIA.INVENTORY_STORAGE_AREA = REQ.SOURCE_AREA
              LEFT OUTER JOIN <proxy />TAB_INVENTORY_AREA TIA2
                ON TIA2.INVENTORY_STORAGE_AREA = REQ.DESTINATION_AREA
             WHERE REQ.ROW_SEQUENCE &lt;= :max_rows
             ORDER BY REQ.ROW_SEQUENCE
            ";
            //var binder = new SqlBinder<RequestModel>("GetRecentRequests");
            var binder = SqlBinder.Create(row => new RequestModel
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
                AssignedFlag = row.GetString("ASSIGNED_FLAG") == "Y",
                TargetVwhId = row.GetString("CONVERSION_VWH_ID"),
                SewingPlantCode = row.GetString("SEWING_PLANT_CODE"),
                SourceQuality = row.GetString("QUALITY_CODE"),
                AllowOverPulling = row.GetString("OVERPULLING"),
                PackagingPreferance = row.GetString("PACKAGING_PREFERENCE"),
                PriceSeasonCode = row.GetString("PRICE_SEASON_CODE"),
                CartonReceivedDate = row.GetDate("RECEIVE_DATE"),
                SaleTypeId = row.GetString("SALE_TYPE_ID"),
                AssignedCartonCount = row.GetInteger("CARTON_COUNT") ?? 0,
                AssignedPieces = row.GetInteger("total_pieces") ?? 0,
                //ReqId = row.GetInteger("DCMS4_REQ_ID"),
                TargetQuality = row.GetString("TARGET_QUALITY"),
                IsConversionRequest = row.GetString("IS_CONVERSION_REQUEST") == "Y",
            }).Parameter("CTN_RESV_ID", ctnResvId)
            .Parameter("max_rows", maxRows);
            return _db.ExecuteReader(QUERY, binder);
        }

        /// <summary>
        /// Returns the all SKUs of a request
        /// </summary>
        /// <param name="ctnresvId"></param>
        /// <returns></returns>
        public IEnumerable<RequestSkuModel> GetRequestSkus(string ctnresvId)
        {
            const string QUERY = @"
            SELECT 
                   MAX(MSKU.STYLE)                    AS STYLE,
                   MAX(MSKU.COLOR)                    AS COLOR,
                   MAX(MSKU.DIMENSION)                AS DIMENSION,
                   MAX(MSKU.SKU_SIZE)                 AS SKU_SIZE,
                   MAX(MSKU.SKU_ID)                     AS SKU_ID,
                   MAX(MSKUCONV.STYLE)         AS CON_STYLE_,
                   MAX(MSKUCONV.COLOR)         AS CON_COLOR_,
                   MAX(MSKUCONV.DIMENSION)     AS CON_DIMENSION_,
                   MAX(MSKUCONV.SKU_SIZE)      AS CON_SKU_SIZE_,
                   MAX(MSKUCONV.SKU_ID)                 AS CON_SKU_ID,
                   MAX(REQDET.QUANTITY_REQUESTED)       AS QUANTITY_REQUESTED
              FROM <proxy />CTNRESV C
              INNER JOIN <proxy />SRC_REQ_DETAIL REQDET
                ON C.ctn_resv_id = REQDET.ctn_resv_id
              LEFT OUTER JOIN <proxy />MASTER_SKU MSKU
                ON MSKU.SKU_ID =  reqdet.sku_id
              LEFT OUTER JOIN <proxy />MASTER_SKU MSKUCONV
                ON MSKUCONV.sku_id = reqdet.conversion_sku_id
             WHERE C.CTN_RESV_ID = :ctnresv_id
            GROUP BY REQDET.REQ_PROCESS_ID, REQDET.REQ_LINE_NUMBER
        ";
            var binder = SqlBinder.Create(row => new RequestSkuModel
            {
                Pieces = row.GetInteger("QUANTITY_REQUESTED") ?? 0,
                SourceSku = new SkuModel
                {
                    Style = row.GetString("STYLE"),
                    Color = row.GetString("COLOR"),
                    Dimension = row.GetString("DIMENSION"),
                    SkuSize = row.GetString("SKU_SIZE"),
                    SkuId = row.GetInteger("SKU_ID") ?? 0,
                },

                TargetSku = row.GetInteger("CON_SKU_ID") != null ? new SkuModel
                {
                    Style = row.GetString("CON_STYLE_"),
                    Color = row.GetString("CON_COLOR_"),
                    Dimension = row.GetString("CON_DIMENSION_"),
                    SkuSize = row.GetString("CON_SKU_SIZE_"),
                    SkuId = row.GetInteger("CON_SKU_ID") ?? 0
                } : null
            }).Parameter("ctnresv_id", ctnresvId);
            //var binder = new SqlBinder<RequestSkuModel>("GetRequestInfo");
            //binder.Parameter("ctnresv_id", ctnresvId);
            //binder.CreateMapper(QUERY, config =>
            //    config.CreateMap<RequestSkuModel>()
            //            .MapField("QUANTITY_REQUESTED", dest => dest.Pieces)
            //            .ForMember(dest => dest.SourceSku, opt => opt.MapFrom(src => src.GetValue<int?>("SKU_ID").HasValue ? new SkuModel
            //            {
            //                Color = src.GetValue<string>("COLOR"),
            //                Dimension = src.GetValue<string>("DIMENSION"),
            //                SkuSize = src.GetValue<string>("SKU_SIZE"),
            //                Style = src.GetValue<string>("STYLE"),
            //                SkuId = src.GetValue<int>("SKU_ID")
            //            } : null))
            //            .ForMember(dest => dest.TargetSku, opt => opt.MapFrom(src => src.GetValue<int?>("CON_SKU_ID").HasValue ? new SkuModel
            //            {
            //                Color = src.GetValue<string>("CON_COLOR"),
            //                Dimension = src.GetValue<string>("CON_DIMENSION"),
            //                SkuSize = src.GetValue<string>("CON_SKU_SIZE"),
            //                Style = src.GetValue<string>("CON_STYLE"),
            //                SkuId = src.GetValue<int>("CON_SKU_ID")
            //            } : null))
            //            );
            var result = _db.ExecuteReader(QUERY, binder);
            return result;
        }

        /// <summary>
        /// Getting the list of cartons assigned to passed reservation id 
        /// </summary>
        /// <param name="ctnresvId"></param>
        /// <returns></returns>
        public IEnumerable<AssignedCarton> GetAssignedCartons(string ctnresvId)
        {
            const string QUERY =
                @"
                 SELECT msku.style AS style,
                        msku.color AS color,
                        msku.dimension AS dimension,
                        msku.sku_size AS sku_size,
                        msku.sku_id AS sku_id,
                        count(DISTINCT ctndet.carton_id) AS num_cartons,
                        count(DISTINCT DECODE(ctn.carton_storage_area,
                                    c.source_area,
                                    NULL,
                                    ctndet.carton_id)) AS pulled_cartons,
                        SUM(ctndet.quantity) AS num_pieces,
                        SUM(DECODE(ctn.carton_storage_area,
                                    c.source_area,
                                    0,
                                    ctndet.quantity)) AS pulled_pieces
                    FROM <proxy />ctnresv c
                    inner join <proxy />src_req_detail srd on 
                    srd.ctn_resv_id = c.ctn_resv_id
                    INNER JOIN <proxy />src_carton_detail ctndet 
                            ON srd.req_process_id = ctndet.req_process_id
                            and srd.req_module_code = ctndet.req_module_code
                            and srd.req_line_number = ctndet.req_line_number
                    INNER JOIN <proxy />src_carton ctn 
                            ON ctndet.carton_id = ctn.carton_id 
                    INNER JOIN <proxy />master_sku msku
                            ON ctndet.sku_id = msku.sku_id
                    WHERE c.ctn_resv_id = :ctnresv_id
                      AND c.module_code = 'REQ2'
                    GROUP BY msku.style, msku.color, msku.dimension, msku.sku_size, msku.sku_id
                    ORDER BY msku.style, msku.color, msku.dimension, msku.sku_size
            ";
            var binder = SqlBinder.Create(row => new AssignedCarton
            {
                PulledCartons = row.GetInteger("pulled_cartons").Value,
                PulledPieces = row.GetInteger("pulled_pieces").Value,
                TotalCartons = row.GetInteger("num_cartons").Value,
                TotalPieces = row.GetInteger("num_pieces").Value,
                Sku = new SkuModel
                {
                    Style = row.GetString("STYLE"),
                    Color = row.GetString("COLOR"),
                    Dimension = row.GetString("DIMENSION"),
                    SkuSize = row.GetString("SKU_SIZE"),
                    SkuId = row.GetInteger("SKU_ID").Value
                }
            }).Parameter("ctnresv_id", ctnresvId);
            var result = _db.ExecuteReader(QUERY, binder);
            return result;
        }

        public IEnumerable<CartonList> GetCartonList(string ctnresvId, int maxRows)
        {
            const string QUERY =
 @"SELECT SRC.CARTON_ID AS ACARTON_ID,
       MAX(SRC.PALLET_ID) AS PALLET_ID,
       MAX(CTNRESV.CTN_RESV_ID) AS CTN_RESV_ID,
       MAX(reqdet.req_process_id) As req_process_id,
       MAX(SRC.CARTON_STORAGE_AREA) AS CARTON_STORAGE_AREA,
       MAX(IA.DESCRIPTION) AS AREA_DESCRIPTION,
       MAX(SRC.QUALITY_CODE) AS QUALITY_CODE,
       MAX(SRC.VWH_ID) AS VWH_ID,
       SUM(SRCD.QUANTITY) AS QUANTITY
    FROM <proxy />SRC_CARTON SRC
    LEFT OUTER JOIN <proxy />SRC_CARTON_DETAIL SRCD
     ON SRCD.CARTON_ID = SRC.CARTON_ID
    LEFT OUTER JOIN <proxy />TAB_INVENTORY_AREA IA
     ON IA.INVENTORY_STORAGE_AREA = SRC.CARTON_STORAGE_AREA
    LEFT OUTER JOIN <proxy />TAB_SEWINGPLANT TSP
     ON TSP.SEWING_PLANT_CODE = SRC.SEWING_PLANT_CODE
    LEFT OUTER JOIN <proxy />SRC_REQ_DETAIL REQDET
     ON SRCD.REQ_PROCESS_ID = REQDET.REQ_PROCESS_ID
     AND SRCD.REQ_LINE_NUMBER = REQDET.REQ_LINE_NUMBER
     AND SRCD.REQ_MODULE_CODE = REQDET.REQ_MODULE_CODE
    LEFT OUTER JOIN <proxy />CTNRESV CTNRESV
     ON REQDET.ctn_resv_id = CTNRESV.ctn_resv_id
     WHERE 1 = 1
     AND CTNRESV.CTN_RESV_ID =:CTN_RESV_ID
     AND ROWNUM &lt;= :max_rows
  GROUP BY SRC.CARTON_ID
 ORDER BY carton_storage_area
  ";
            var binder = SqlBinder.Create(row => new CartonList
            {
                CartonId = row.GetString("ACARTON_ID"),
                PalletId = row.GetString("PALLET_ID"),
                StoregeArea = row.GetString("CARTON_STORAGE_AREA"),
                AreaDescription = row.GetString("AREA_DESCRIPTION"),
                QuilityCode = row.GetString("QUALITY_CODE"),
                VwhId = row.GetString("VWH_ID"),
                ReqId = row.GetInteger("req_process_id") ?? 0,
                CtnresvId = row.GetString("CTN_RESV_ID"),
                Quantity = row.GetInteger("QUANTITY") ?? 0,
            }).Parameter("CTN_RESV_ID", ctnresvId).Parameter("max_rows", maxRows);
            return _db.ExecuteReader(QUERY, binder);
        }

        /// <summary>
        /// Adds SKU to passed request
        /// </summary>
        /// <param name="requestSkuModel"></param>
        /// <param name="ctnresvId"></param>
        public void AddSkutoRequest(string ctnresvId, int skuId, int pieces, int? conversionSkuId)
        {
            // Remove hardwirings
            // DB: TODO Why destination location is needed ???

            const string QUERY = @"
               begin
                          <proxy />pkg_ctnresv.add_sku(actn_resv_id =&gt; :ctn_resv_id,
                                          asku_id =&gt; :sku_id,
                                          apieces_per_package =&gt; 1,
                                          arequired_pieces =&gt; :quantity_requested,
                                          atarget_sku_id =&gt; :conversion_sku_id);
                    end;";
            var binder = SqlBinder.Create().Parameter("ctn_resv_id", ctnresvId)
                  .Parameter("sku_id", skuId)
                  .Parameter("quantity_requested", pieces)
                  .Parameter("conversion_sku_id", conversionSkuId);
            _db.ExecuteNonQuery(QUERY, binder);
        }

        /// <summary>
        /// Delete an SKU from a request
        /// </summary>
        /// <param name="skuId"></param>
        /// <param name="ctnresvId"></param>
        public void DeleteSkuFromRequest(int skuId, string ctnresvId)
        {
            const string QUERY = @"
               begin
                <proxy />pkg_ctnresv.delete_sku(actn_resv_id => :ctn_resv_id,
                                            asku_id => :sku_id);
              end;";

            var binder = SqlBinder.Create()
                .Parameter("ctn_resv_id", ctnresvId)
                .Parameter("sku_id", skuId);
            _db.ExecuteNonQuery(QUERY, binder);
        }

        /// <summary>
        /// This method Assigns carton to the request
        /// </summary>
        public void AssignCartons(string ctnresvId)
        {
            const string QUERY = @"
                BEGIN
                    <proxy />pkg_ctnresv.assign(actn_resv_id =&gt; :ctn_resv_id,
                                        acustomer_id =&gt; NULL);
                END;";
            var binder = SqlBinder.Create().Parameter("ctn_resv_id", ctnresvId);
            _db.ExecuteNonQuery(QUERY, binder);
        }

        /// <summary>
        /// This method is used to unassigned the cartons from a request
        /// </summary>
        /// <param name="ctnresvId"></param>
        public void UnAssignCartons(string ctnresvId)
        {
            const string QUERY =
                            @"
                            BEGIN
                                <proxy />pkg_ctnresv.unassign(actn_resv_id =&gt; :ctn_resv_id);
                            END;
                        ";
            var binder = SqlBinder.Create().Parameter("ctn_resv_id", ctnresvId);
            _db.ExecuteNonQuery(QUERY, binder);
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
        public SkuModel GetSku(string style, string color, string dimension, string skuSize)
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

            var binder = SqlBinder.Create(row => new SkuModel
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

    }
}

//$Id$


