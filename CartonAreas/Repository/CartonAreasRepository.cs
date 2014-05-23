using EclipseLibrary.Oracle;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.Contracts;
using System.Web;

namespace DcmsMobile.CartonAreas.Repository
{
    public class CartonAreasRepository : IDisposable
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
        public CartonAreasRepository(string userName, string moduleName, string clientInfo, TraceContext trace)
        {
            Contract.Assert(ConfigurationManager.ConnectionStrings["dcms4"] != null);
            var store = new OracleDatastore(trace);
            store.CreateConnection(ConfigurationManager.ConnectionStrings["dcms4"].ConnectionString,
                userName);
            store.ModuleName = moduleName;
            store.ClientInfo = clientInfo;
            _db = store;
        }

        /// <summary>
        /// For use in unit tests
        /// </summary>
        /// <param name="db"></param>
        public CartonAreasRepository(OracleDatastore db)
        {
            _db = db;
        }

        public void Dispose()
        {
            _db.Dispose();
        }

        #endregion

        internal IList<Building> GetBuildings(string buildingId)
        {
            const string QUERY = @"
                                    WITH LOCATION_COUNTS AS
                                     (SELECT TIA.WAREHOUSE_LOCATION_ID AS WAREHOUSE_LOCATION_ID,
                                             COUNT(MSL.LOCATION_ID) AS COUNT_LOCATIONS,
                                             COUNT(UNIQUE TIA.INVENTORY_STORAGE_AREA) AS COUNT_AREAS,
                                             COUNT(UNIQUE MSL.STORAGE_AREA) AS COUNT_NUMBERED_AREAS
                                        FROM <proxy />TAB_INVENTORY_AREA TIA
                                        LEFT OUTER JOIN <proxy />MASTER_STORAGE_LOCATION MSL
                                          ON TIA.INVENTORY_STORAGE_AREA = MSL.STORAGE_AREA
                                       WHERE TIA.WAREHOUSE_LOCATION_ID IS NOT NULL
                                       GROUP BY TIA.WAREHOUSE_LOCATION_ID)
                                    SELECT T.WAREHOUSE_LOCATION_ID  AS WAREHOUSE_LOCATION_ID,
                                           T.DESCRIPTION            AS DESCRIPTION,
                                           T.INSERT_DATE            AS INSERT_DATE,
                                           T.INSERTED_BY            AS INSERTED_BY,
                                           T.RECEIVING_PALLET_LIMIT AS RECEIVING_PALLET_LIMIT,
                                           T.ADDRESS_1              AS ADDRESS_1,
                                           T.ADDRESS_2              AS ADDRESS_2,
                                           T.ADDRESS_3              AS ADDRESS_3,
                                           T.ADDRESS_4              AS ADDRESS_4,
                                           T.CITY                   AS CITY,
                                           T.STATE                  AS STATE,
                                           T.ZIP_CODE               AS ZIP_CODE,
                                           T.COUNTRY_CODE           AS COUNTRY_CODE,
                                           LC.COUNT_AREAS           AS COUNT_AREAS,
                                           LC.COUNT_NUMBERED_AREAS  AS COUNT_NUMBERED_AREAS,
                                           LC.COUNT_LOCATIONS       AS COUNT_LOCATIONS
                                      FROM <proxy />TAB_WAREHOUSE_LOCATION T
                                      LEFT OUTER JOIN LOCATION_COUNTS LC
                                        ON LC.WAREHOUSE_LOCATION_ID = T.WAREHOUSE_LOCATION_ID
                                      WHERE 1 = 1
                                        <if>AND T.WAREHOUSE_LOCATION_ID = :WAREHOUSE_LOCATION_ID</if>
                                ";
            var binder = SqlBinder.Create(row => new Building
            {
                BuildingId = row.GetString("warehouse_location_id"),
                Description = row.GetString("description"),
                InsertDate = row.GetDate("insert_date"),
                InsertedBy = row.GetString("inserted_by"),
                ReceivingPalletLimit = row.GetInteger("receiving_pallet_limit"),
                Address1 = row.GetString("address_1"),
                Address2 = row.GetString("address_2"),
                Address3 = row.GetString("address_3"),
                Address4 = row.GetString("address_4"),
                City = row.GetString("City"),
                State = row.GetString("State"),
                ZipCode = row.GetString("zip_code"),
                CountryCode = row.GetString("country_code"),
                CountArea = row.GetInteger("count_areas"),
                CountNumberedArea = row.GetInteger("count_numbered_areas"),
                CountLocation = row.GetInteger("count_locations")
            });
            binder.Parameter("WAREHOUSE_LOCATION_ID", buildingId);
            return _db.ExecuteReader(QUERY, binder);
        }

        /// <summary>
        /// This method gets the carton area info and number of locations in it.
        /// </summary>
        /// <param name="areaId"></param>
        /// <returns></returns>
        public IEnumerable<CartonArea> GetCartonAreas(string areaId, string buildingId)
        {
            const string QUERY = @"
                SELECT MAX(TIA.INVENTORY_STORAGE_AREA)       AS INVENTORY_STORAGE_AREA,
                       TIA.SHORT_NAME                        AS SHORT_NAME,
                       MAX(TIA.DESCRIPTION)                  AS DESCRIPTION,
                       MAX(TIA.LOCATION_NUMBERING_FLAG)      AS LOCATION_NUMBERING_FLAG,                             
                       MAX(TIA.IS_PALLET_REQUIRED)           AS IS_PALLET_REQUIRED,
                       MAX(TIA.UNUSABLE_INVENTORY)           AS UNUSABLE_INVENTORY,
                       TIA.WAREHOUSE_LOCATION_ID             AS WAREHOUSE_LOCATION_ID,
                       COUNT(DISTINCT MSL.LOCATION_ID)       AS LOCATION_COUNT,
<if c='$AREA_ID'>
       count(distinct case
               when ctn.location_id is null then
                msl.location_id
             end) as empty_locations,
       count(distinct case
               when ctn.location_id is not null then
                msl.location_id
             end) as nonempty_locations,
       count(distinct case
               when msl.assigned_sku_id is not null then
                msl.location_id
             end) as assigned_locations,
       count(distinct case
               when msl.assigned_sku_id is not null and ctn.location_id is null then
                msl.location_id
             end) as empty_assigned_locations,
       count(distinct case
               when msl.assigned_sku_id is not null and
                    ctn.location_id is not null then
                msl.location_id
             end) as nonempty_assigned_locations,
       count(distinct case
               when msl.assigned_sku_id is null and ctn.location_id is null then
                msl.location_id
             end) as empty_unassigned_locations,
       count(distinct case
               when msl.assigned_sku_id is null and ctn.location_id is not null then
                msl.location_id
             end) as nonempty_unassigned_locations,
       count(distinct case
               when msl.assigned_sku_id is null then
                msl.location_id
             end) as total_unassigned_locations
</if>
<else>
       CAST(NULL AS NUMBER) as empty_locations,
       CAST(NULL AS NUMBER) as nonempty_locations,
       CAST(NULL AS NUMBER) as assigned_locations,
       CAST(NULL AS NUMBER) as empty_assigned_locations,
       CAST(NULL AS NUMBER) as nonempty_assigned_locations,
       CAST(NULL AS NUMBER) as empty_unassigned_locations,
       CAST(NULL AS NUMBER) as nonempty_unassigned_locations,
       CAST(NULL AS NUMBER) as total_unassigned_locations
</else>
                  FROM <proxy />TAB_INVENTORY_AREA TIA
                 LEFT OUTER JOIN <proxy />MASTER_STORAGE_LOCATION MSL
                    ON MSL.STORAGE_AREA = TIA.INVENTORY_STORAGE_AREA
<if c='$AREA_ID'>
  left outer join <proxy />src_carton ctn
    on ctn.location_id = msl.location_id
   and ctn.carton_storage_area = msl.storage_area
</if>
                 WHERE TIA.STORES_WHAT = 'CTN'
               <if>AND TIA.INVENTORY_STORAGE_AREA = :AREA_ID</if>
              <if> AND TIA.WAREHOUSE_LOCATION_ID=:WAREHOUSE_LOCATION_ID</if>
                 GROUP BY TIA.WAREHOUSE_LOCATION_ID, tia.short_name
                 ORDER BY TIA.WAREHOUSE_LOCATION_ID, tia.short_name
        ";
            var binder = SqlBinder.Create(row => new CartonArea
            {
                AreaId = row.GetString("INVENTORY_STORAGE_AREA"),
                Description = row.GetString("DESCRIPTION"),
                LocationNumberingFlag = row.GetString("LOCATION_NUMBERING_FLAG") == "Y",
                IsPalletRequired = row.GetString("IS_PALLET_REQUIRED") == "Y",
                UnusableInventory = row.GetString("UNUSABLE_INVENTORY") == "Y",
                BuildingId = row.GetString("WAREHOUSE_LOCATION_ID"),
                TotalLocations = row.GetInteger("LOCATION_COUNT") ?? 0,
                ShortName = row.GetString("SHORT_NAME"),
                CountEmptyLocations = row.GetInteger("empty_locations"),
                CountNonemptyLocations = row.GetInteger("nonempty_locations"),
                CountAssignedLocations = row.GetInteger("assigned_locations"),
                CountEmptyUnassignedLocations = row.GetInteger("empty_unassigned_locations"),
                CountUnassignedLocations = row.GetInteger("total_unassigned_locations"),
                CountEmptyAssignedLocations = row.GetInteger("empty_assigned_locations"),
                CountNonemptyAssignedLocations = row.GetInteger("nonempty_assigned_locations"),
                CountNonemptyUnassignedLocations = row.GetInteger("nonempty_unassigned_locations")
            });
            binder.Parameter("AREA_ID", areaId).Parameter("WAREHOUSE_LOCATION_ID", buildingId);
            return _db.ExecuteReader(QUERY, binder);
        }


        /// <summary>
        /// This function gets the location details in a specified area,
        /// and also give all information of any one location for assignment.
        /// </summary>
        /// <param name="filters"></param>
        /// <returns></returns>
        /// <remarks>
        /// SS 28/12/2011: Pallet count was wrong. Changed COUNT(SC.PALLET_ID) to COUNT(DISTINCT SC.PALLET_ID)
        /// </remarks>
        public IEnumerable<Location> GetLocations(LocationFilter filters)
        {
            const string QUERY = @"
                SELECT MSL.LOCATION_ID                AS LOCATION_ID,
                       MAX(MSL.ASSIGNED_MAX_CARTONS)  AS ASSIGNED_MAX_CARTONS,
                       MAX(MSL.Assigned_Vwh_Id)       AS ASSIGNED_VWH_ID,
                       COUNT(DISTINCT SC.PALLET_ID)   AS NUMBER_OF_PALLET,
                       COUNT(SC.CARTON_ID)            AS NUMBER_OF_CARTONS,
                       COUNT(DISTINCT(SC.VWH_ID))     AS CARTON_VWH_COUNT,
                       MAX(SC.VWH_ID)                 AS CARTON_VWH_ID,
                       SUM(SCD.QUANTITY)              AS TOTAL_PIECES,
                       COUNT(DISTINCT(SCD.SKU_ID))    AS CARTON_SKU_COUNT,
                       MAX(SCD.SKU_ID)                AS CARTON_SKU_ID,
                       MAX(MSKU.SKU_ID)               AS SKU_ID,
                       MAX(MSKU.STYLE)                AS STYLE_,
                       MAX(MSKU.COLOR)                AS COLOR_,
                       MAX(MSKU.DIMENSION)            AS DIMENSION_,
                       MAX(MSKU.SKU_SIZE)             AS SKU_SIZE_,
                       MAX(MSKU.UPC_CODE)             AS UPC_CODE_,
                       MAX(MSKU2.SKU_ID)              AS CTN_SKU_ID_,
                       MAX(MSKU2.STYLE)               AS CTN_STYLE_,
                       MAX(MSKU2.COLOR)               AS CTN_COLOR_,
                       MAX(MSKU2.DIMENSION)           AS CTN_DIMENSION_,
                       MAX(MSKU2.SKU_SIZE)            AS CTN_SKU_SIZE_,
                       MAX(MSKU2.UPC_CODE)            AS CTN_UPC_CODE_                       
                  FROM <proxy/>MASTER_STORAGE_LOCATION MSL
                  LEFT OUTER JOIN <proxy/>SRC_CARTON SC
                    ON SC.LOCATION_ID = MSL.LOCATION_ID
                   AND SC.CARTON_STORAGE_AREA = MSL.STORAGE_AREA
                  LEFT OUTER JOIN <proxy/>SRC_CARTON_DETAIL SCD
                    ON SCD.CARTON_ID = SC.CARTON_ID
                  LEFT OUTER JOIN <proxy/>MASTER_SKU MSKU
                    ON MSKU.SKU_ID = MSL.ASSIGNED_SKU_ID
                LEFT OUTER JOIN <proxy/>MASTER_SKU MSKU2
                    ON MSKU2.SKU_ID = SCD.SKU_ID
                 WHERE 1 = 1
               <if>AND MSL.STORAGE_AREA = :AREA_ID</if>
               <if>AND MSL.LOCATION_ID = :LOCATION_ID</if>                
               <if>AND MSL.ASSIGNED_SKU_ID = :SKU_ID</if>
               <if c=""$EMPTY_LOCATION_FLAG = 'true' "">
                   AND NOT EXISTS
                    (SELECT 1 FROM <proxy/>SRC_CARTON SRC WHERE SRC.LOCATION_ID = MSL.LOCATION_ID 
                      <if>AND SRC.LOCATION_ID = :LOCATION_ID</if>  )
               </if>
               <if c=""$EMPTY_LOCATION_FLAG = 'false' "">
                   AND MSL.LOCATION_ID IN
                    (SELECT LOCATION_ID FROM <proxy/>SRC_CARTON WHERE CARTON_STORAGE_AREA = :AREA_ID)
               </if>
               <if c=""$ASSIGNED_FLAG ='true' "">
                  AND MSL.ASSIGNED_SKU_ID IS NOT NULL
               </if>
               <if c=""$ASSIGNED_FLAG ='false' "">
                AND MSL.ASSIGNED_SKU_ID IS NULL
               </if>                              
              GROUP BY MSL.TRAVEL_SEQUENCE, MSL.LOCATION_ID
              ORDER BY MSL.TRAVEL_SEQUENCE, MSL.LOCATION_ID           
          ";
            var binder = SqlBinder.Create(row => new Location()
                {
                    LocationId = row.GetString("LOCATION_ID"),
                    PalletCount = row.GetInteger("NUMBER_OF_PALLET").Value,
                    TotalPieces = row.GetInteger("TOTAL_PIECES"),
                    MaxAssignedCarton = row.GetInteger("ASSIGNED_MAX_CARTONS"),
                    CartonCount = row.GetInteger("NUMBER_OF_CARTONS").Value,
                    AssignedVwhId = row.GetString("ASSIGNED_VWH_ID"),
                    CartonVwhCount = row.GetInteger("CARTON_VWH_COUNT").Value,
                    CartonVwhId = row.GetString("CARTON_VWH_ID"),
                    CartonSkuCount = row.GetInteger("CARTON_SKU_COUNT").Value,
                    CartonSku = row.GetInteger("CARTON_SKU_ID") == null ? null : new Sku
                        {
                            Style = row.GetString("CTN_STYLE_"),
                            Color = row.GetString("CTN_COLOR_"),
                            Dimension = row.GetString("CTN_DIMENSION_"),
                            SkuSize = row.GetString("CTN_SKU_SIZE_"),
                            SkuId = row.GetInteger("CTN_SKU_ID_").Value,
                            UpcCode = row.GetString("CTN_UPC_CODE_")
                        },
                    AssignedSku = row.GetInteger("SKU_ID") == null ? null : new Sku
                        {
                            Style = row.GetString("STYLE_"),
                            Color = row.GetString("COLOR_"),
                            Dimension = row.GetString("DIMENSION_"),
                            SkuSize = row.GetString("SKU_SIZE_"),
                            SkuId = row.GetInteger("SKU_ID").Value,
                            UpcCode = row.GetString("UPC_CODE_")
                        }
                }).Parameter("AREA_ID", filters.CartonAreaId)
                .Parameter("LOCATION_ID", filters.LocationId)
               .Parameter("EMPTY_LOCATION_FLAG", filters.EmptyLocations.HasValue ? filters.EmptyLocations.ToString().ToLower() : "")
               .Parameter("SKU_ID", filters.SkuId);
            if (filters.SkuId == null)
            {
                binder.Parameter("ASSIGNED_FLAG", filters.AssignedLocations.HasValue ? filters.AssignedLocations.ToString().ToLower() : "");
            }
            else
            {
                // Ignore the ASSIGNED_FLAG
                binder.Parameter("ASSIGNED_FLAG", "");
            }
            return _db.ExecuteReader(QUERY, binder, 100);
        }


        /// <summary>
        /// This method will update the capacity and Assigned SKU of passed location
        /// </summary>
        /// <param name="locationId">Contains required properties to update the location</param>
        /// <param name="skuId"></param>
        /// <param name="maxCartons"></param>
        /// <param name="vwhId"></param>
        public int AssignSkuToLocation(string locationId, int? skuId, int? maxCartons, string vwhId)
        {
            const string QUERY = @"
              UPDATE <proxy />MASTER_STORAGE_LOCATION MSL
                 SET MSL.ASSIGNED_SKU_ID      = :SKU_ID,
                     MSL.ASSIGNED_MAX_CARTONS = :CARTON_COUNT,
                     MSL.UNAVAILABLE_FLAG     = :UNAVAILABLE,
                     MSL.ASSIGNED_VWH_ID      = :ASSIGNED_VWH_ID
               WHERE MSL.LOCATION_ID = :LOCATION_ID
            ";
            var binder = SqlBinder.Create()
                .Parameter("LOCATION_ID", locationId)
                .Parameter("SKU_ID", skuId)
                .Parameter("UNAVAILABLE", skuId.HasValue || maxCartons != null || !string.IsNullOrEmpty(vwhId) ? "" : "Y")
                .Parameter("CARTON_COUNT", maxCartons)
                .Parameter("ASSIGNED_VWH_ID", vwhId);
            return _db.ExecuteDml(QUERY, binder);

        }

        /// <summary>
        /// This function gets the VwhId 
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
                Description = row.GetString("DESCRIPTION")
            });
            return _db.ExecuteReader(QUERY, binder);

        }

        /// <summary>
        /// This method is used to update area information.
        /// </summary>
        /// <param name="model"></param>
        /// <remarks>
        /// Updates the passed model with updated properties
        /// </remarks>
        public void UpdateArea(CartonArea model)
        {
            const string QUERY = @"
                    UPDATE <proxy />TAB_INVENTORY_AREA TIA
                       SET TIA.LOCATION_NUMBERING_FLAG = :LOCATION_NUMBERING_FLAG,
                           TIA.IS_PALLET_REQUIRED      = :IS_PALLET_REQUIRED,
                           TIA.DESCRIPTION             = :DESCRIPTION,
                           TIA.UNUSABLE_INVENTORY      = :UNUSABLE_INVENTORY
                           WHERE TIA.INVENTORY_STORAGE_AREA =:INVENTORY_STORAGE_AREA
                RETURNING tia.short_name INTO :short_name

";
            var binder = SqlBinder.Create()
            .Parameter("INVENTORY_STORAGE_AREA", model.AreaId)
            .Parameter("LOCATION_NUMBERING_FLAG", model.LocationNumberingFlag ? "Y" : string.Empty)
            .Parameter("IS_PALLET_REQUIRED", model.IsPalletRequired ? "Y" : string.Empty)
            .Parameter("UNUSABLE_INVENTORY", model.UnusableInventory ? "Y" : string.Empty)
            .Parameter("DESCRIPTION", model.Description)
            .OutParameter("short_name", p => model.ShortName = p);
            _db.ExecuteNonQuery(QUERY, binder);
        }


        public void UpdatePalletLimit(string buildingId, int? palletLimit)
        {
            const string QUERY = @"
             UPDATE <proxy />TAB_WAREHOUSE_LOCATION T
                   SET T.RECEIVING_PALLET_LIMIT = :RECEIVING_PALLET_LIMIT
                 WHERE T.WAREHOUSE_LOCATION_ID = :WAREHOUSE_LOCATION_ID
            ";
            var binder = SqlBinder.Create()
                .Parameter("WAREHOUSE_LOCATION_ID", buildingId)
                .Parameter("RECEIVING_PALLET_LIMIT", palletLimit);
            _db.ExecuteDml(QUERY, binder);

        }

        public void UpdateAddress(UpdateAddressOfBuilding building)
        {
            if (string.IsNullOrWhiteSpace(building.BuildingId))
            {
                throw new ArgumentNullException("BuildingId");
            }
            const string QUERY = @"
                    UPDATE TAB_WAREHOUSE_LOCATION T
                           SET T.ADDRESS_1    = :ADDRESS_1,
                               T.ADDRESS_2    = :ADDRESS_2,
                               T.ADDRESS_3    = :ADDRESS_3,
                               T.ADDRESS_4    = :ADDRESS_4,
                               T.CITY         = :CITY,
                               T.STATE        = :STATE,
                               T.ZIP_CODE     = :ZIP_CODE,
                               T.COUNTRY_CODE = :COUNTRY_CODE
                         WHERE T.WAREHOUSE_LOCATION_ID = :WAREHOUSE_LOCATION_ID
            ";
            var binder = SqlBinder.Create()
                .Parameter("ADDRESS_1", building.Address1)
                .Parameter("ADDRESS_2", building.Address2)
                .Parameter("ADDRESS_3", building.Address3)
                .Parameter("ADDRESS_4", building.Address4)
                .Parameter("CITY", building.City)
                .Parameter("STATE", building.State)
                .Parameter("ZIP_CODE", building.ZipCode)
                .Parameter("COUNTRY_CODE", building.CountryCode)
                .Parameter("WAREHOUSE_LOCATION_ID", building.BuildingId);
            _db.ExecuteNonQuery(QUERY, binder);
        }


        public void AddBuilding(Building building)
        {
            const string QUERY = @" INSERT INTO TAB_WAREHOUSE_LOCATION T
                                  (T.WAREHOUSE_LOCATION_ID,
                                   T.ADDRESS_1,
                                   T.ADDRESS_2,
                                   T.ADDRESS_3,
                                   T.ADDRESS_4,
                                   T.CITY,
                                   T.STATE,
                                   T.ZIP_CODE,
                                   T.RICHTER_WAREHOUSE_ID_O,
                                   T.RECEIVING_PALLET_LIMIT)
                                VALUES
                                  (:WAREHOUSE_LOCATION_ID,
                                   :ADDRESS_1,
                                   :ADDRESS_2,
                                   :ADDRESS_3,
                                   :ADDRESS_4,
                                   :CITY,
                                   :STATE,
                                   :ZIP_CODE,
                                   :RICHTER_WAREHOUSE_ID_O,
                                   :RECEIVING_PALLET_LIMIT)

                             ";
            var binder = SqlBinder.Create()
                .Parameter("WAREHOUSE_LOCATION_ID", building.BuildingId)
                .Parameter("ADDRESS_1", building.Address1)
                .Parameter("ADDRESS_2", building.Address2)
                .Parameter("ADDRESS_3", building.Address3)
                .Parameter("ADDRESS_4", building.Address4)
                .Parameter("CITY", building.City)
                .Parameter("STATE", building.State)
                .Parameter("ZIP_CODE", building.ZipCode)
                .Parameter("RICHTER_WAREHOUSE_ID_O", building.RictherWarehouseId)
                .Parameter("RECEIVING_PALLET_LIMIT", building.ReceivingPalletLimit);
            _db.ExecuteNonQuery(QUERY, binder);
        }

    }
}
//$Id$