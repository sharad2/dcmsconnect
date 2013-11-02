using DcmsMobile.REQ2.Models;
using EclipseLibrary.Oracle;
using System;
using System.Collections.Generic;
using System.Web;

namespace DcmsMobile.REQ2.Repository.Pull
{
    public class PullRepository : IDisposable
    {
        #region Initialization

        private OracleDatastore _db;

        /// <summary>
        /// For injecting the value through unit tests
        /// </summary>
        /// <param name="db"></param>
        public PullRepository(OracleDatastore db)
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

        public PullRepository(TraceContext ctx, string connectString, string userName, string clientInfo, string moduleName)
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
        ///  Brings the areas where pulling is required. Also fetches best request per area.  
        ///  
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Area> GetPullAreaSuggestions()
        {
            const string QUERY = @"
                                SELECT 
                                     MAX(SOURCE_TIA.SHORT_NAME) AS SOURCE_SHORT_NAME,
                                     MAX(DESTINATION_TIA.SHORT_NAME) AS DEST_SHORT_NAME,
                                     C.DESTINATION_AREA AS DESTINATION_AREA,
                                      C.SOURCE_AREA AS SOURCE_AREA,
                                     <proxy/>PKG_PUL_3.GET_BEST_REQUEST(C.SOURCE_AREA, C.DESTINATION_AREA) AS BEST_REQUEST ,  
                                     COUNT(CPC.CARTON_ID) AS CARTON_COUNT,
                                     MAX(SOURCE_TIA.WAREHOUSE_LOCATION_ID) AS SOURCE_BUILDING_ID,
                                     MAX(DESTINATION_TIA.WAREHOUSE_LOCATION_ID) AS DEST_BUILDING_ID
                                     
                                  FROM <proxy/>CTNRESV C
                                 INNER JOIN <proxy/>CTNRESV_DETAIL CD
                                    ON C.CTN_RESV_ID = CD.CTN_RESV_ID
                                 INNER JOIN <proxy/>CTNRESV_PULL_CARTON CPC
                                    ON CPC.REQ_PROCESS_ID = CD.REQ_PROCESS_ID
                                 LEFT OUTER JOIN TAB_INVENTORY_AREA SOURCE_TIA
                                    ON SOURCE_TIA.INVENTORY_STORAGE_AREA = C.SOURCE_AREA
                                 LEFT OUTER JOIN TAB_INVENTORY_AREA DESTINATION_TIA
                                    ON DESTINATION_TIA.INVENTORY_STORAGE_AREA = C.DESTINATION_AREA
                                 WHERE CPC.IS_PULLED IS NULL
                                 GROUP BY C.SOURCE_AREA, C.DESTINATION_AREA
                                 ORDER BY MAX(C.PRIORITY) DESC";
            var binder = SqlBinder.Create(row => new Area
                                        {
                                           
                                            DestinationAreaId = row.GetString("DESTINATION_AREA"),
                                            SourceAreaId = row.GetString("SOURCE_AREA"),
                                            TopRequestId = row.GetString("BEST_REQUEST"),
                                            DestinationBuildingId = row.GetString("DEST_BUILDING_ID"),
                                            SourceBuildingId = row.GetString("SOURCE_BUILDING_ID"),
                                            PullableCartonCount = row.GetInteger("CARTON_COUNT") ?? 0,                                            
                                            SourceAreaShortName = row.GetString("SOURCE_SHORT_NAME"),
                                            DestinationAreaShortName = row.GetString("DEST_SHORT_NAME")
                                        });
            return _db.ExecuteReader(QUERY, binder);
        }

        /// <summary>
        /// Returns the list of cartons suggested to the puller along with count of previously suggested cartons
        /// </summary>
        /// <param name="buildingId"></param>
        /// <param name="pickAreaId"></param>
        /// <param name="restockAreaId"></param>
        /// <param name="palletId"></param>
        /// <param name="maxRows"></param>
        /// <param name="oldSuggestionCount"></param>
        /// <returns></returns>
        public IEnumerable<PullableCarton> GetCartonSuggestions(string palletId,string requestId)
        {
            //TODO: Remove hardwiring of amaxrows
            //TODO: Backend should return a list of cartons instead of count. 
            const string QUERY = @"
                                    DECLARE
                                      LCountSuggestions NUMBER(4);
                                    BEGIN
                                    LCountSuggestions := <proxy/>pkg_pul_3.create_pull_pallet(apallet_id => :apallet_id,
                                          arequest => :arequest,
                                          amaxcartons => :amaxcartons);

                                      OPEN :REF_CURSOR FOR
                                    SELECT T.CARTON_ID        AS CARTON_ID,
                                           T.LOCATION_ID      AS LOCATION_ID,
                                           CTNDET.SKU_ID      AS SKU_ID,
                                           MS.STYLE           AS STYLE,
                                           MS.COLOR           AS COLOR,
                                           MS.DIMENSION       AS DIMENSION,
                                           MS.SKU_SIZE        AS SKU_SIZE
                                      FROM <proxy/>TEMP_PULL_CARTON T
                                     INNER JOIN <proxy/>SRC_CARTON CTN
                                        ON CTN.CARTON_ID = T.CARTON_ID
                                     INNER JOIN <proxy/>SRC_CARTON_DETAIL CTNDET
                                        ON CTNDET.CARTON_ID = T.CARTON_ID
                                     INNER JOIN <proxy/>MASTER_SKU MS
                                        ON CTNDET.SKU_ID = MS.SKU_ID
                                     WHERE CTNDET.QUANTITY IS NOT NULL
                                       AND T.PALLET_ID = :APALLET_ID
                                     ORDER BY T.PULL_PATH_ORDER;
                                    END;
                                    ";
            var binder = SqlBinder.Create(row =>
            {
                return new PullableCarton
                 {
                     CartonId = row.GetString("carton_id"),
                     LocationId = row.GetString("location_id"),
                     SkuInCarton = new Sku
                      {
                          SkuId = row.GetInteger("sku_id").Value,
                          Style = row.GetString("style"),
                          Color = row.GetString("color"),
                          Dimension = row.GetString("dimension"),
                          SkuSize = row.GetString("sku_size"),
                      }
                 };
            });
            binder.Parameter("apallet_id", palletId)
                .Parameter("amaxcartons", 5) //TODO: Remove this hardwiring.
                .Parameter("arequest", requestId)
                .OutRefCursorParameter("REF_CURSOR");
            return _db.ExecuteReader(QUERY, binder);
        }


        /// <summary>
        /// Pulls the passed carton.
        /// </summary>
        /// <param name="areaId"></param>
        /// <param name="palletId"></param>
        /// <param name="cartonId"></param>
        /// TODO: Remove Destination area and use request instead. 
        public void PullCartonForRequest (string palletId, string cartonId, string requestId)
        {
            const string QUERY = @"
                                    BEGIN 
                                     pkg_pul_3.pull_carton_for_request(acarton_id => :acarton_id,
                                                                       apallet_id => :apallet_id,
                                                                       arequest_id => :arequest_id);
                                    END;
                                    ";
            var binder = SqlBinder.Create();
            binder.Parameter("apallet_id", palletId)
                .Parameter("acarton_id", cartonId)
                .Parameter("arequest_id",requestId);
            _db.ExecuteDml(QUERY, binder);
        }

        public PullRequest GetRequestInfo(string requestId)
        {
            const string QUERY = @"SELECT SOURCE_TIA.SHORT_NAME      AS SOURCE_AREA,
                                       DESTINATION_TIA.SHORT_NAME AS DESTINATION_AREA,
                                       c.INSERTED_BY            AS REQUESTED_BY
                                  FROM CTNRESV C
                                  LEFT OUTER JOIN TAB_INVENTORY_AREA SOURCE_TIA
                                    ON SOURCE_TIA.INVENTORY_STORAGE_AREA = C.SOURCE_AREA
                                  LEFT OUTER JOIN TAB_INVENTORY_AREA DESTINATION_TIA
                                    ON DESTINATION_TIA.INVENTORY_STORAGE_AREA = C.DESTINATION_AREA
                                WHERE C.CTN_RESV_ID = :requestId ";

            var binder = SqlBinder.Create(row => new PullRequest
            {
                DestShortName = row.GetString("DESTINATION_AREA"),
                SourceShortName = row.GetString("SOURCE_AREA"),
                RequestedBy = row.GetString("REQUESTED_BY")
            }).Parameter("requestId", requestId);
           return _db.ExecuteSingle(QUERY, binder);

        }

        /// <summary>
        /// Gives you the best request for passed source,destination area and 
        /// </summary>
        /// <param name="sourceareaId"></param>
        /// <param name="destareaId"></param>
        /// <returns></returns>
        public string GetBestRequest(string sourceareaId, string destareaId)
        {
            const string QUERY = @"
                                    begin
                                    
                                         :request := pkg_pul_3.get_best_request(asource_area => :asource_area,
                                        adestination_area => :adestination_area);
                                    end;
                                    ";
            string bestRequest = string.Empty;
            var binder = SqlBinder.Create();
            binder.Parameter("adestination_area", destareaId)
                .Parameter("asource_area", sourceareaId)
                .OutParameter("request",val => bestRequest = val);
            _db.ExecuteDml(QUERY, binder);
            return bestRequest;
        }
  
    }
}