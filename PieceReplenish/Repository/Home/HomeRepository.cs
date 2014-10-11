using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Web;
using EclipseLibrary.Oracle;

namespace DcmsMobile.PieceReplenish.Repository.Home
{
    public class HomeRepository : IDisposable
    {
        #region Intialization

        const string MODULE_NAME = "Replenishment Pulling";

        private OracleDatastore _db;

        public HomeRepository(OracleDatastore db)
        {
            _db = db;
        }

        public OracleDatastore Db
        {
            get
            {
                return _db;
            }
        }

        public HomeRepository(TraceContext ctx, string connectString, string userName, string clientInfo)
        {
            var db = new OracleDatastore(ctx);
            db.CreateConnection(connectString, userName);

            db.ModuleName = MODULE_NAME;
            db.ClientInfo = clientInfo;
            db.DefaultMaxRows = 10000;      // Allow retrieving up to 10000 rows. Number of cartons can be huge
            _db = db;
        }

        public void Dispose()
        {
            _db.Dispose();
        }

        #endregion

        #region Information

        /// <summary>
        /// Returns all inventory areas for which there are cartons to pull
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Area> GetInventoryAreas()
        {
            const string QUERY = @"
SELECT T.Pick_Ia_Id AS Pick_Ia_Id,
MAX(I.SHORT_NAME) AS SHORT_NAME,
       MAX(T.RESTOCK_AREA_ID) AS RESTOCK_AREA_ID,
       MAX(T.Pull_Area_Id) AS Pull_Area_Id,
       T.WAREHOUSE_LOCATION_ID AS WAREHOUSE_LOCATION_ID,
       COUNT(Td.CARTON_ID) AS CARTON_COUNT
  FROM <proxy/>REPLENISH_aisle T
LEFT OUTER JOIN <proxy/>IA I
    ON I.IA_ID = T.PICK_IA_ID
 inner join <proxy/>REPLENISH_aisle_carton td
    on t.replenish_aisle_id = td.replenish_aisle_id
 GROUP BY T.WAREHOUSE_LOCATION_ID, T.Pick_Ia_Id
 ORDER BY T.WAREHOUSE_LOCATION_ID, T.Pick_Ia_Id
            ";
            var binder = SqlBinder.Create(row => new Area
             {
                 AreaId = row.GetString("Pick_Ia_Id"),
                 ShortName = row.GetString("SHORT_NAME"),
                 RestockAreaId = row.GetString("RESTOCK_AREA_ID"),
                 CartonAreaId = row.GetString("Pull_Area_Id"),
                 BuildingId = row.GetString("WAREHOUSE_LOCATION_ID"),
                 PullableCartonCount = row.GetInteger("CARTON_COUNT") ?? 0
             });

            return _db.ExecuteReader(QUERY, binder);
        }

        /// <summary>
        /// Returns carton count on the passed pallet
        /// </summary>
        /// <param name="palletId"></param>
        /// <returns></returns>
        public Pallet GetPallet(string palletId)
        {
            const string QUERY = @"
            SELECT COUNT(CTN.CARTON_ID)             AS CARTON_COUNT,
                   MAX(CTN.CARTON_STORAGE_AREA)     AS MAX_CARTON_STORAGE_AREA,
                   SYS.STRAGG(UNIQUE(CTN.PRICE_SEASON_CODE) || ' ') AS PRICE_SEASON_CODE
                FROM <proxy />SRC_CARTON CTN
                WHERE CTN.PALLET_ID = :PALLET_ID
            ";

            var binder = SqlBinder.Create(row => new Pallet
            {
                CartonCount = row.GetInteger("CARTON_COUNT") ?? 0,
                MaxCartonArea = row.GetString("MAX_CARTON_STORAGE_AREA"),
                PriceSeasonCode = row.GetString("PRICE_SEASON_CODE")
            }).Parameter("PALLET_ID", palletId);

            var pallet = _db.ExecuteSingle(QUERY, binder);
            pallet.PalletId = palletId;
            return pallet;
        }

        public IEnumerable<AisleSku> GetPullableSkus(string buildingId, string pickAreaId, string cartonAreaId, string restockAreaId)
        {
            const string QUERY = @"
SELECT T.SKU_ID AS SKU_ID,
       MAX(MSKU.STYLE) AS STYLE,
       MAX(MSKU.COLOR) AS COLOR,
       MAX(MSKU.DIMENSION) AS DIMENSION,
       MAX(MSKU.SKU_SIZE) AS SKU_SIZE,
       MAX(MSKU.UPC_CODE) AS UPC_CODE,
       MAX(T.VWH_ID) AS VWH_ID,
       MAX(T.WAVE_PRIORITY) AS WAVE_PRIORITY,
       MAX(T.PIECES_IN_RESTOCK) AS PIECES_AVAILABLE_IN_RST,
       SUM(Td.Pieces_In_Carton) AS PIECES_IN_CARTON,
       MAX(T.Wave_Count) AS BUCKET_COUNT,       
       T.RESTOCK_AISLE_ID AS RESTOCK_AISLE_ID,
       MAX(T.Sku_Priority) AS REPLENISHMENT_PRIORITY,
       MAX(t.pieces_capacity) AS ASSIGNED_UPC_MAX_PIECES,
       MAX(t.PIECES_AT_LOCATIONS) AS PIECES_AT_LOCATIONS,
       COUNT(Td.CARTON_ID) AS CARTONs_to_pull,
       MAX(T.CARTONS_IN_RESTOCK) AS CARTONS_IN_RESTOCK,
       MAX(T.PIECES_TO_PICK) AS PIECES_TO_PICK
  FROM <proxy/>REPLENISH_aisle T
 INNER JOIN <proxy/>MASTER_SKU MSKU
    ON MSKU.SKU_ID = T.SKU_ID
 inner join <proxy/>REPLENISH_aisle_carton td
    on t.replenish_aisle_id = td.replenish_aisle_id
 WHERE T.WAREHOUSE_LOCATION_ID = :WAREHOUSE_LOCATION_ID
   AND T.Pull_Area_Id = :REPLENISH_IA_ID
   AND T.RESTOCK_AREA_ID = :RESTOCK_IA_ID
   AND T.PICK_IA_ID = :DEST_AREA_ID
 GROUP BY T.SKU_ID, T.RESTOCK_AISLE_ID
";

            var binder = SqlBinder.Create(row => new AisleSku
            {
                SkuId = row.GetInteger("SKU_ID").Value,
                Style = row.GetString("STYLE"),
                Color = row.GetString("COLOR"),
                Dimension = row.GetString("DIMENSION"),
                SkuSize = row.GetString("SKU_SIZE"),
                UpcCode = row.GetString("UPC_CODE"),
                VwhId = row.GetString("VWH_ID"),
                WavePriority = row.GetInteger("WAVE_PRIORITY"),
                PiecesAwaitingRestock = row.GetInteger("PIECES_AVAILABLE_IN_RST"),
                PiecesInPullableCarton = row.GetInteger("PIECES_IN_CARTON").Value,
                WaveCount = row.GetInteger("BUCKET_COUNT"),
                CartonsToPull = row.GetInteger("CARTONs_to_pull").Value,
                CartonsInRestock = row.GetInteger("CARTONS_IN_RESTOCK"),
                RestockAisleId = row.GetString("RESTOCK_AISLE_ID"),
                SkuReplenishmentPriority = row.GetInteger("REPLENISHMENT_PRIORITY"),
                Capacity = row.GetInteger("ASSIGNED_UPC_MAX_PIECES").Value,
                PiecesAtPickLocations = row.GetInteger("PIECES_AT_LOCATIONS"),
                PiecesToPick = row.GetInteger("PIECES_TO_PICK")
            });
            binder.Parameter("DEST_AREA_ID", pickAreaId)
                  .Parameter("RESTOCK_IA_ID", restockAreaId)
                  .Parameter("REPLENISH_IA_ID", cartonAreaId)
                  .Parameter("WAREHOUSE_LOCATION_ID", buildingId);

            return _db.ExecuteReader(QUERY, binder, 500);
        }

        #endregion

        #region SKU Priority

        /// <summary>
        /// The priority of the passed SKU is increased for 30 minutes.
        /// </summary>
        /// <param name="buildingId"></param>
        /// <param name="areaId"></param>
        /// <param name="skuId"> </param>
        /// <param name="userName"> </param>
        /// <returns>
        /// Returns the expiry time till when Sku Priority will be high.
        /// If sku is not assigned at any location null will be returned. 
        /// </returns>
        public DateTime? IncreaseSkuPriority(string buildingId, string areaId, int skuId, string userName)
        {
            //TODO: Get Priority
            const string QUERY = @"
                DECLARE
                    LPRIORITY   NUMBER := <proxy />PKG_REPLENISH.P_HIGH_PRIORITY;
                    TYPE EXPIRY_LIST_T IS TABLE OF DATE;
                    EXPIRY_LIST EXPIRY_LIST_T;
                    LRESULT <proxy />PKG_REPLENISH.PRIORITY_INFO_REC;
                BEGIN
                    LRESULT := <proxy />PKG_REPLENISH.SET_SKU_PRIORITY(ABUILDING_ID =&gt; :WAREHOUSE_LOCATION_ID,
                                                                 AAREA_ID =&gt; :IA_ID,
                                                                 ASKU_ID =&gt; :SKU_ID,
                                                                 APRIORITY=&gt; LPRIORITY,
                                                                 AUSER_NAME =&gt; :USER_NAME);
                :EXPIRY_TIME := LRESULT.EXPIRY_TIME;
                END;
            ";

            var binder = SqlBinder.Create();

            DateTime? expiryTime = null;
            binder.Parameter("SKU_ID", skuId)
                  .Parameter("IA_ID", areaId)
                  .Parameter("WAREHOUSE_LOCATION_ID", buildingId)
                  .Parameter("USER_NAME", userName);
            binder.OutParameter("EXPIRY_TIME", p => expiryTime = p);

            _db.ExecuteNonQuery(QUERY, binder);
            return expiryTime;
        }

        /// <summary>
        /// The priority of the SKU is reverted to normal
        /// </summary>
        /// <param name="buildingId"></param>
        /// <param name="areaId"></param>
        /// <param name="skuId"> </param>
        /// <returns>True if SKU priority was decreased</returns>
        /// <remarks>
        /// This will also revert the priority set by CreateMPC, if any.
        /// </remarks>
        public bool DecreaseSkuPriority(string buildingId, string areaId, int skuId, string userName)
        {
            var rows = -1;
            const string QUERY = @"
                DECLARE
                 LPRIORITY   NUMBER := <proxy />PKG_REPLENISH.P_NORMAL_PRIORITY;
                 LRESULT <proxy />PKG_REPLENISH.PRIORITY_INFO_REC;
                BEGIN
                  LRESULT := <proxy />PKG_REPLENISH.SET_SKU_PRIORITY(ABUILDING_ID =&gt; :WAREHOUSE_LOCATION_ID,
                                                                 AAREA_ID =&gt; :IA_ID,
                                                                 APRIORITY=&gt; LPRIORITY,
                                                                 ASKU_ID =&gt; :SKU_ID,
                                                                 AUSER_NAME =&gt; :USER_NAME);
               :RESULT := LRESULT.ROWS_UPDATED;
                END;
             ";
            var binder = SqlBinder.Create();

            binder.Parameter("SKU_ID", skuId)
                  .Parameter("IA_ID", areaId)
                  .Parameter("WAREHOUSE_LOCATION_ID", buildingId)
                  .Parameter("USER_NAME", userName);
            binder.OutParameter("RESULT", p => rows = p ?? 0);
            _db.ExecuteNonQuery(QUERY, binder);
            return rows > 0;
        }

        #endregion

        #region Refresh Job

        /// <summary>
        ///  Returns the info about job which refresh the list of pullable cartons
        /// </summary>
        /// <returns></returns>
        public JobRefresh GetRefreshInfo()
        {
            const string QUERY = @"
                DECLARE
                  LRESULT <proxy />PKG_REPLENISH.REFRESH_INFO_REC;
                BEGIN
	              LRESULT          := <proxy />PKG_REPLENISH.GET_REFRESH_STATUS;
                  :STATE           := LRESULT.STATUS;
                  :LAST_START_DATE := LRESULT.LAST_START_DATE;
                  :NEXT_RUN_DATE   := LRESULT.NEXT_RUN_DATE;  
                END;
            ";
            var binder = SqlBinder.Create();
            var job = new JobRefresh();
            binder.OutParameter("LAST_START_DATE", val => job.LastRefreshedTime = val)
                .OutParameter("NEXT_RUN_DATE", val => job.NextRunDate = val)
                .OutParameter("STATE", val => job.Status = val);
            _db.ExecuteNonQuery(QUERY, binder);
            if (job.Status == "RUNNING")
            {
                job.IsRefreshingNow = true;
            }
            return job;
        }

        /// <summary>
        /// Refreshes the list of pullable cartons
        /// </summary>
        public void RefreshPullableCartons()
        {
            const string QUERY = @"
                BEGIN
                  <proxy />PKG_REPLENISH.ASYNC_REFRESH_PULLABLE_CARTONS;
                END;
               ";
            var binder = SqlBinder.Create();
            _db.ExecuteNonQuery(QUERY, binder);
        }

        #endregion

        public DbTransaction BeginTransaction()
        {
            return _db.BeginTransaction();
        }

        #region Suggestions

        /// <summary>
        /// Reeturns the list of cartons suggested to the puller along with count of previously suggested cartons
        /// </summary>
        /// <param name="buildingId"></param>
        /// <param name="pickAreaId"></param>
        /// <param name="restockAreaId"></param>
        /// <param name="palletId"></param>
        /// <param name="maxRows"></param>
        /// <param name="oldSuggestionCount"></param>
        /// <returns></returns>
        public IEnumerable<PullableCarton> GetCartonSuggestions(string buildingId, string pickAreaId, string restockAreaId, string palletId, int maxRows, out int oldSuggestionCount)
        {
            const string QUERY = @"
declare
  LCountSuggestions NUMBER(4);
begin
  LCountSuggestions := <proxy/>pkg_replenish.suggest_cartons(abuilding_id => :abuilding_id,
                                           apickarea_id => :apickarea_id,
                                           arestockarea_id => :arestockarea_id,
                                           apallet_id => :apallet_id,
                                           amaxrows => :amaxrows);

  OPEN :REF_CURSOR FOR
select t.carton_id as carton_id,
       t.location_id as location_id,
       t.restock_aisle_id AS restock_aisle_id,
       ctndet.sku_id as sku_id,       
       ms.style as style,
       ms.color as color,
       ms.dimension as dimension,
       ms.sku_size as sku_size,      
       t.sku_priority as sku_priority,
       LCountSuggestions as countSuggestions
  from <proxy/>temp_pull_carton t
 inner join <proxy/>src_carton ctn
    on ctn.carton_id = t.carton_id
 inner join <proxy/>src_carton_detail ctndet
    on ctndet.carton_id = t.carton_id
 inner join <proxy/>master_sku ms
    on ctndet.sku_id = ms.sku_id
 where ctndet.quantity is not null 
<if>and t.pallet_id = :apallet_id</if>
order by t.pull_path_order;
  
end;
";
            int count = 0;
            var binder = SqlBinder.Create(row =>
                {
                    count = row.GetInteger("countSuggestions").Value;
                    return new PullableCarton
                    {
                        CartonId = row.GetString("carton_id"),
                        LocationId = row.GetString("location_id"),
                        SkuPriority = row.GetInteger("sku_priority"),
                        RestockAisleId = row.GetString("restock_aisle_id"),
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


            binder.Parameter("abuilding_id", buildingId)
                .Parameter("apickarea_id", pickAreaId)
                .Parameter("arestockarea_id", restockAreaId)
                .Parameter("apallet_id", palletId)
                .Parameter("amaxrows", maxRows)
                .OutRefCursorParameter("REF_CURSOR")
                ;
            var cartons = _db.ExecuteReader(QUERY, binder);
            oldSuggestionCount = count;
            return cartons;
        }

        /// <summary>        
        /// Puller just don't want to pull this carton. Remove it from list of suggestions.
        /// </summary>
        /// <param name="cartonId"></param>
        public void RemoveCartonSuggestion(string cartonId)
        {
            const string QUERY = @"
                BEGIN
                  <proxy />PKG_REPLENISH.REMOVE_CARTON_SUGGESTIONS(ACARTON_ID => :ACARTON_ID);
                END;
             ";
            var binder = SqlBinder.Create();
            binder.Parameter("ACARTON_ID", cartonId);
            _db.ExecuteNonQuery(QUERY, binder);
        }

        /// <summary>
        /// Removes the all reserved cartons to pull against passed pallet.
        /// So that those can be assigned to others.
        /// </summary>
        /// <param name="pullerName"></param>
        /// <param name="palletId"></param>
        internal int DiscardPalletSuggestion(string pullerName, string palletId)
        {
            const string QUERY = @"
            DELETE <proxy />TEMP_PULL_CARTON 
             WHERE PALLET_ID = :PALLET_ID
               AND OPERATOR_NAME = :OPERATOR_NAME
            ";

            var binder = SqlBinder.Create();
            binder.Parameter("PALLET_ID", palletId)
                  .Parameter("OPERATOR_NAME", pullerName);

            return _db.ExecuteDml(QUERY, binder);
        }

        //        /// <summary>
        //        /// Records the carton which is currently suggeted to the puller
        //        /// </summary>
        //        /// <param name="buildingId"> </param>
        //        /// <param name="restockAisleId"> </param>
        //        /// <param name="cartonId"> </param>
        //        /// <param name="userName"></param>
        //        internal void RecordCartonSuggestion(string buildingId, string restockAisleId, string cartonId, string userName)
        //        {
        //            const string QUERY = @"
        //            BEGIN  
        //              <proxy />PKG_REPLENISH.RECORD_CARTON_SUGGESTION(ABUILDING_ID     =&gt; :ABUILDING_ID,
        //                                                              ARESTOCKAISLE_ID =&gt; :ARESTOCKAISLE_ID,                                                              
        //                                                              ACARTON_ID       =&gt; :ACARTON_ID,
        //                                                              AUSER_NAME       =&gt; :AUSER_NAME);
        //            END;
        //            ";

        //            var binder = SqlBinder.Create();
        //            binder.Parameter("AUSER_NAME", userName)
        //                  .Parameter("ARESTOCKAISLE_ID", restockAisleId)
        //                  .Parameter("ABUILDING_ID", buildingId)
        //                  .Parameter("ACARTON_ID", cartonId);
        //            _db.ExecuteNonQuery(QUERY, binder);
        //        }

        /// <summary>
        /// Returns the information about pullers who are currently pulling 
        /// </summary>
        /// <param name="buildingId"></param>
        /// <returns></returns>
        internal IEnumerable<PullerActivity> GetPullerActivity(string buildingId)
        {
            const string QUERY = @"
                SELECT T.OPERATOR_NAME AS OPERATOR_NAME,
                       T.PALLET_ID AS PALLET_ID,
                       COUNT(T.PALLET_ID) AS CARTON_COUNT,
                       MAX(T.BUILDING_ID) AS BUILDING_ID,
                       MIN(T.ASSIGN_DATE) AS MIN_ASSIGN_DATE,
                       T.RESTOCK_AISLE_ID AS RESTOCK_AISLE_ID,
                       LISTAGG(T.PULL_MODULE_CODE, ',') WITHIN GROUP(ORDER BY T.RESTOCK_AISLE_ID) AS PULL_MODULE_CODE,
                       LISTAGG(CTNDET.SKU_ID, ',') WITHIN GROUP(ORDER BY CTNDET.SKU_ID) AS LIST_SKU_ID,
                       LISTAGG(MSKU.STYLE, ',') WITHIN GROUP(ORDER BY MSKU.STYLE) AS LIST_STYLE
                  FROM <proxy />TEMP_PULL_CARTON T
                 INNER JOIN <proxy />SRC_CARTON CTN
                    ON CTN.CARTON_ID = T.CARTON_ID
                 INNER JOIN <proxy />SRC_CARTON_DETAIL CTNDET
                    ON CTNDET.CARTON_ID = T.CARTON_ID
                 INNER JOIN <proxy />MASTER_SKU MSKU
                    ON MSKU.SKU_ID = CTNDET.SKU_ID
                 WHERE T.PULL_MODULE_CODE = 'PUL' OR T.PALLET_ID IS NOT NULL
                 <if>AND T.BUILDING_ID = :BUILDING_ID </if>
                 GROUP BY T.OPERATOR_NAME, T.PALLET_ID, T.RESTOCK_AISLE_ID
            ";

            var binder = SqlBinder.Create(row => new PullerActivity
            {
                PullerName = row.GetString("OPERATOR_NAME"),
                PalletId = row.GetString("PALLET_ID"),
                CartonCount = row.GetInteger("CARTON_COUNT"),
                MinAssignDate = row.GetDate("MIN_ASSIGN_DATE"),
                BuildingId = row.GetString("BUILDING_ID"),
                ListSkuId = row.GetString("LIST_SKU_ID").Split(',').Distinct().Select(p => int.Parse(p)),
                RestockAisleId = row.GetString("RESTOCK_AISLE_ID"),
                IsUsingReplenishmentModule = row.GetString("PULL_MODULE_CODE").Split(',').Distinct().Contains(MODULE_NAME),
                Styles = string.Join(", ", row.GetString("LIST_STYLE").Split(',').Distinct())
            });

            binder.Parameter("building_id", buildingId);
            return _db.ExecuteReader(QUERY, binder);
        }

        #endregion

        #region Pulling

        /// <summary>
        /// Pull the passed carton
        /// </summary>
        /// <param name="cartonId">The carton to pull</param>
        /// <param name="palletId">The pallet to place the carton on</param>
        /// <param name="areaId">The new area of the carton</param>
        /// <param name="restockAisleId">Aisle for which carton is being pulled</param>
        /// <param name="countSuggestions">Number of suggestions still available for this pallet</param>
        /// <returns>
        /// Returns true if carton is successfully pulled
        /// </returns>
        /// <remarks>
        /// If the aisle <paramref name="restockAisleId"/> can accommodate the passed carton <paramref name="cartonId"/>, then
        /// system pulls the carton, else throws an exception
        /// </remarks>
        public bool TryPullCartonForAisle(string cartonId, string palletId, string areaId, string restockAisleId, out int countSuggestions)
        {

            const string QUERY = @"

                Declare
                RESULT number;
                BEGIN
               :RESULT:= <proxy />PKG_REPLENISH.TRY_PULL_CARTON_FOR_AISLE(ACARTON_ID =&gt; :ACARTON_ID,
                                                               APALLET_ID =&gt; :APALLET_ID,
                                                               AAREA_ID =&gt; :ADESTINATION_AREA,
                                                               ARESTOCK_AISLE_ID =&gt; :ARESTOCK_AISLE_ID);
select count(*)
INTO :count_suggestions
  from <proxy />temp_pull_carton t
 where t.pallet_id = :apallet_id;
END;
            ";

            var totalSuggestions = 0;
            var result = 0;
            var binder = SqlBinder.Create();
            binder.Parameter("ACARTON_ID", cartonId)
                  .Parameter("APALLET_ID", palletId)
                  .Parameter("ARESTOCK_AISLE_ID", restockAisleId)
                  .Parameter("ADESTINATION_AREA", areaId)
                  .OutParameter("count_suggestions", p => totalSuggestions = p ?? 0)
                  .OutParameter("RESULT", p => result = p ?? 0)
            ;
            _db.ExecuteNonQuery(QUERY, binder);
            countSuggestions = totalSuggestions;
            return result > 0;
        }

        /// <summary>
        /// The passed carton is moved to suspense
        /// </summary>
        /// <param name="cartonId"></param>
        /// <param name="locationId"></param>
        /// <returns></returns>
        /// <remarks>
        /// If system finds that passed carton is not on passed location then only carton will be marked in suspense.
        /// </remarks>
        internal bool MarkCartonInSuspense(string cartonId, string locationId)
        {
            var result = -1;
            const string QUERY = @"
                BEGIN
                 :RESULT := <proxy />PKG_REPLENISH.MARK_CARTON_IN_SUSPENSE(ACARTON_ID =&gt; :ACARTON_ID,
                                                                           ALOCATION_ID =&gt; :ALOCATION_ID);
                END;
            ";
            var binder = SqlBinder.Create();

            binder.Parameter("ACARTON_ID", cartonId)
                  .Parameter("ALOCATION_ID", locationId)
                  .OutParameter("RESULT", p => result = p ?? 0);
            _db.ExecuteNonQuery(QUERY, binder);
            return result > 0;
        }

        #endregion

    }
}


/*
    $Id: PieceReplenishRepository.cs 17726 2012-07-26 08:19:26Z bkumar $
    $Revision: 17726 $
    $URL: svn://vcs/net4/Projects/Mvc/DcmsMobile.Pull/trunk/Pull/Repository/PieceReplenishRepository.cs $
    $Header: svn://vcs/net4/Projects/Mvc/DcmsMobile.Pull/trunk/Pull/Repository/PieceReplenishRepository.cs 17726 2012-07-26 08:19:26Z bkumar $
    $Author: bkumar $
    $Date: 2012-07-26 13:49:26 +0530 (Thu, 26 Jul 2012) $
*/
