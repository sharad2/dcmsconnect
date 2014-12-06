using DcmsMobile.Inquiry.Helpers;
using EclipseLibrary.Oracle;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.Contracts;
using System.Web;

namespace DcmsMobile.Inquiry.Areas.Inquiry.CartonAreaEntity
{
    internal class CartonAreaEntityRepository:IDisposable
    {
        private readonly OracleDatastore _db;
        public CartonAreaEntityRepository(string userName, string clientInfo)
        {
            _db = new OracleDatastore(HttpContext.Current.Trace);
            _db.CreateConnection(ConfigurationManager.ConnectionStrings["dcms8"].ConnectionString, userName);
            _db.ModuleName = "Inquiry_CartonAreaEntity";
            _db.ClientInfo = clientInfo;
        }

        public void Dispose()
        {
            _db.Dispose();
        }


        /// <summary>
        /// FOR CATRON LOCATION INFORMATION
        /// </summary>
        /// <param name="locationId"></param>
        /// <returns></returns>
        public CartonLocation GetCartonLocationInfo(string locationId)
        {
            Contract.Assert(_db != null);
            const string QUERY = @"
                            Select Msl.Location_Id        As Location_Id,
                                Mskuass.Style             As Assigned_Style,
                                Mskuass.Color             As Assigned_Color,
                                Mskuass.Dimension         As Assigned_Dimension,
                                Mskuass.Sku_Size          As Assigned_Sku_Size,
                                Msl.Storage_Area          As Storage_Area,
                                Msl.Warehouse_Location_Id As Warehouse_Location_Id,
                                Msl.Assigned_Sku_Id       As Assigned_Sku_Id,
                                tia.short_name            as short_name,
                                Msl.Assigned_Max_Cartons  As Assigned_Max_Cartons
                      From <proxy />Master_Storage_Location Msl
                      Left Outer Join <proxy />Master_Sku Mskuass
                        On Mskuass.Sku_Id = Msl.Assigned_Sku_Id
                    left outer join <proxy />tab_inventory_area tia
                        on msl.storage_area = tia.inventory_storage_area
                     Where Msl.Location_Id = :Location_Id
                    ";
            var binder = SqlBinder.Create(row => new CartonLocation
            {
                LocationId = row.GetString("Location_Id"),
                WhId = row.GetString("Warehouse_Location_Id"),
                Capacity = row.GetInteger("Assigned_Max_Cartons"),
                Area = row.GetString("Storage_Area"),
                ShortName = row.GetString("short_name"),
                AssignedSku = new SkuBase
                {
                    Style = row.GetString("Assigned_Style"),
                    Color = row.GetString("Assigned_Color"),
                    Dimension = row.GetString("Assigned_Dimension"),
                    SkuSize = row.GetString("Assigned_Sku_Size"),
                    SkuId = row.GetInteger("Assigned_Sku_Id") ?? 0
                }
            }).Parameter("Location_Id", locationId);
            return _db.ExecuteSingle(QUERY, binder);
        }



        /// <summary>
        ///PROVIDE INFORMATION ABOUT PALLETS IN LOCATION
        /// </summary>
        /// <param name="locationId"></param>
        /// <returns></returns>
//        [Obsolete]
//        public IList<CartonAreaInventory> GetCartonsOfLocationOnPallet(string locationId)
//        {
//            const string QUERY = @" SELECT COUNT(S.CARTON_ID) AS CARTON_COUNT,
//                                           S.PALLET_ID AS PALLET_ID,
//                                           SUM(SD.QUANTITY) AS PIECES,
//                                           COUNT(DISTINCT SD.SKU_ID) AS SKU_COUNT
//                                      FROM <proxy />SRC_CARTON S
//                                     INNER JOIN <proxy />SRC_CARTON_DETAIL SD
//                                        ON S.CARTON_ID = SD.CARTON_ID
//                                     WHERE S.LOCATION_ID = :LOCATION_ID
//                                       AND S.PALLET_ID IS NOT NULL
//                                     GROUP BY S.PALLET_ID";
//            var binder = SqlBinder.Create(row => new CartonAreaInventory
//            {
//                CartonCount = row.GetInteger("CARTON_COUNT"),
//                PalletId = row.GetString("PALLET_ID"),
//                SKUQuantity = row.GetInteger("PIECES"),
//                DistinctSKUs = row.GetInteger("SKU_COUNT")
//            }).Parameter("LOCATION_ID", locationId);

//            return _db.ExecuteReader(QUERY, binder);
//        }

        /// <summary>
        ///PROVIDE INFORMATION ABOUT CARTONS NOT ON PALLETS IN LOCATION
        /// </summary>
        /// <param name="locationId"></param>
        /// <returns></returns>
        public IList<CartonAtLocation> GetCartonsAtLocation(string locationId)
        {
            const string QUERY = @"
                    SELECT S.CARTON_ID AS CARTON_ID,
                           MAX(S.PALLET_ID) AS PALLET_ID,
                         SUM(SD.QUANTITY) AS PIECES      
                    FROM <proxy />SRC_CARTON S
                   INNER JOIN <proxy />SRC_CARTON_DETAIL SD
                      ON S.CARTON_ID = SD.CARTON_ID
                   WHERE S.LOCATION_ID = :LOCATION_ID                     
                   GROUP BY S.CARTON_ID
                ";
            var binder = SqlBinder.Create(row => new CartonAtLocation
            {
                CartonId = row.GetString("CARTON_ID"),
                SKUQuantity = row.GetInteger("PIECES"),
                //DistinctSKUs = row.GetInteger("SKU_COUNT"),
                PalletId = row.GetString("PALLET_ID")
            }).Parameter("LOCATION_ID", locationId);

            return _db.ExecuteReader(QUERY, binder, 1000);
        }

        /// <summary>
        /// FOR CATRON AREA INFORMATION
        /// </summary>
        /// <param name="cartonArea"></param>
        /// <returns></returns>
        public CartonArea GetCartonAreaInfo(string cartonArea)
        {
            Contract.Assert(_db != null);


            const string QUERY_CARTON_AREA_DETAIL = @"
WITH location_stats AS
 (SELECT COUNT(*) AS TOTAL_LOCATIONS,
         COUNT(MSL.ASSIGNED_SKU_ID) AS ASSIGNED_LOCATIONS,
         msl.STORAGE_AREA AS STORAGE_AREA
    FROM <proxy />MASTER_STORAGE_LOCATION MSL
   WHERE MSL.STORAGE_AREA = :INVENTORY_STORAGE_AREA
   group by msl.STORAGE_AREA),

content_stats AS
 (SELECT T.CARTON_STORAGE_AREA AS CARTON_STORAGE_AREA,
         COUNT(DISTINCT T.LOCATION_ID) AS NON_EMPTY_LOCATION
    FROM <proxy />SRC_CARTON T
   WHERE T.CARTON_STORAGE_AREA = :INVENTORY_STORAGE_AREA
   group by t.CARTON_STORAGE_AREA),
area_info AS
 (SELECT TIA.INVENTORY_STORAGE_AREA  AS CARTON_STORAGE_AREA,
         TIA.SHORT_NAME              AS SHORT_NAME,
         TIA.DESCRIPTION             AS DESCRIPTION,
         TIA.LOCATION_NUMBERING_FLAG AS LOCATION_NUMBERING_FLAG,
         TIA.IS_PALLET_REQUIRED      AS PALLET_REQUIRED,
         TIA.UNUSABLE_INVENTORY      AS UNUSABLE_INVENTORY,
         TIA.OVERDRAFT_ALLOWED       AS OVERDRAFT_ALLOWED,
         TIA.IS_REPACK_AREA          AS IS_REPACK_AREA,
         TIA.WAREHOUSE_LOCATION_ID   AS WAREHOUSE_LOCATION_ID
    FROM <proxy />TAB_INVENTORY_AREA TIA
   WHERE TIA.INVENTORY_STORAGE_AREA = :INVENTORY_STORAGE_AREA)

select   ai.CARTON_STORAGE_AREA      AS CARTON_STORAGE_AREA,
         ai.SHORT_NAME               AS SHORT_NAME,
         ai.DESCRIPTION              AS DESCRIPTION,
         ai.LOCATION_NUMBERING_FLAG  As LOCATION_NUMBERING_FLAG,
         ai.PALLET_REQUIRED          As PALLET_REQUIRED,
         --ai.UNUSABLE_INVENTORY       AS UNUSABLE_INVENTORY,
         ai.OVERDRAFT_ALLOWED        As OVERDRAFT_ALLOWED,
         ai.IS_REPACK_AREA           As IS_REPACK_AREA,
         ai.WAREHOUSE_LOCATION_ID    AS WAREHOUSE_LOCATION_ID, 
         ls.TOTAL_LOCATIONS          AS TOTAL_LOCATIONS,   
         ls.ASSIGNED_LOCATIONS       AS ASSIGNED_LOCATIONS,
         cs.NON_EMPTY_LOCATION       AS NON_EMPTY_LOCATION 
  from area_info ai
  left outer join location_stats ls
    on ls.STORAGE_AREA = ai.CARTON_STORAGE_AREA
  left outer join content_stats cs
    on cs.CARTON_STORAGE_AREA = ai.CARTON_STORAGE_AREA
           ";
            var binder = SqlBinder.Create(row => new CartonArea
            {
                CartonStorageArea = row.GetString("CARTON_STORAGE_AREA"),
                Description = row.GetString("DESCRIPTION"),
                ShortName = row.GetString("SHORT_NAME"),
                LocationNumberingFlag = !string.IsNullOrEmpty(row.GetString("LOCATION_NUMBERING_FLAG")),
                IsPalletRequired = !string.IsNullOrEmpty(row.GetString("PALLET_REQUIRED")),
                //ShipableInventory = string.IsNullOrEmpty(row.GetString("UNUSABLE_INVENTORY")),
                OverdraftAllowed = !string.IsNullOrEmpty(row.GetString("OVERDRAFT_ALLOWED")),
                IsRepackArea = !string.IsNullOrEmpty(row.GetString("IS_REPACK_AREA")),
                WhID = row.GetString("WAREHOUSE_LOCATION_ID"),
                TotalLocations = row.GetInteger("TOTAL_LOCATIONS"),
                AssignedLocations = row.GetInteger("ASSIGNED_LOCATIONS"),
                NonEmptyLocations = row.GetInteger("NON_EMPTY_LOCATION")
            }).Parameter("INVENTORY_STORAGE_AREA", cartonArea);
            return _db.ExecuteSingle(QUERY_CARTON_AREA_DETAIL, binder);
        }

        /// <summary>
        /// FOR CATRON AREA INVENTORY INFORMATION
        /// </summary>
        /// <param name="cartonArea"></param>
        /// <returns></returns>
        public IList<CartonAreaInventory> GetCartonAreaInventory(string cartonArea)
        {
            Contract.Assert(_db != null);
            const string QUERY_CARTON_AREA_INVENTORY_DETAIL = @"
 SELECT COUNT(CTN.CARTON_ID)          AS CARTON_COUNT,
        COUNT(DISTINCT CTNDET.SKU_ID) AS DISTINCT_SKU,
        MS.LABEL_ID                   AS LABEL_ID,
        SUM(CTNDET.QUANTITY)          AS QUANTITY
   FROM <proxy />SRC_CARTON CTN
   LEFT OUTER JOIN <proxy />SRC_CARTON_DETAIL CTNDET
     ON CTN.CARTON_ID = CTNDET.CARTON_ID
   LEFT OUTER JOIN <proxy />Master_Sku msku
     ON ctndet.sku_id = msku.sku_id    
   LEFT OUTER JOIN <proxy />MASTER_STYLE MS
     ON msku.style = MS.STYLE
  WHERE CTN.CARTON_STORAGE_AREA = :CARTON_STORAGE_AREA
  GROUP BY CTN.CARTON_STORAGE_AREA, MS.LABEL_ID
HAVING SUM(CTNDET.QUANTITY) &gt; 0
  ORDER BY SUM(CTNDET.QUANTITY) DESC, MS.LABEL_ID
";
            var binder = SqlBinder.Create(row => new CartonAreaInventory
            {
                CartonCount = row.GetInteger("CARTON_COUNT"),
                DistinctSKUs = row.GetInteger("DISTINCT_SKU"),
                LabelId = row.GetString("LABEL_ID"),
                SKUQuantity = row.GetInteger("QUANTITY")

            }).Parameter("CARTON_STORAGE_AREA", cartonArea);
            return _db.ExecuteReader(QUERY_CARTON_AREA_INVENTORY_DETAIL, binder);
        }

        /// <summary>
        /// All non-empty areas are returned
        /// </summary>
        /// <returns></returns>
        public IList<CartonAreaHeadline> GetCartonAreaList()
        {
            Contract.Assert(_db != null);
            const string QUERY = @"
                                WITH Q1 AS
                 (SELECT T.INVENTORY_STORAGE_AREA AS AREA,
                         MAX(T.SHORT_NAME) AS AREA_SHORT_NAME,
                         MAX(T.DESCRIPTION) AS DESCRIPTION,
                         COUNT(CTN.CARTON_ID) AS CARTON_COUNT,
                         COUNT(UNIQUE CTN.LOCATION_ID) AS USED_LOCATION_COUNT,
                         COUNT(DISTINCT CTNDET.SKU_ID) AS DISTINCT_SKU,
                         SUM(CTNDET.QUANTITY) AS QUANTITY,
                         MAX(TWL.WAREHOUSE_LOCATION_ID) AS WAREHOUSE_LOCATION_ID,
                         MAX(TWL.DESCRIPTION) AS WAREHOUSE_DESCRIPTION
                    FROM <proxy />TAB_INVENTORY_AREA T
                    LEFT OUTER JOIN <proxy />TAB_WAREHOUSE_LOCATION TWL
                      ON T.WAREHOUSE_LOCATION_ID = TWL.WAREHOUSE_LOCATION_ID  
                    LEFT OUTER JOIN <proxy />SRC_CARTON CTN
                      ON T.INVENTORY_STORAGE_AREA = CTN.CARTON_STORAGE_AREA
                    LEFT OUTER JOIN <proxy />SRC_CARTON_DETAIL CTNDET
                      ON CTN.CARTON_ID = CTNDET.CARTON_ID
                    LEFT OUTER JOIN <proxy />MASTER_SKU MSKU
                      ON CTNDET.SKU_ID = MSKU.SKU_ID
                   WHERE T.STORES_WHAT = 'CTN'
                   GROUP BY T.INVENTORY_STORAGE_AREA
having SUM(CTNDET.QUANTITY) &gt; 0
),
                Q2 AS
                 (SELECT COUNT(UNIQUE MS.LOCATION_ID) AS TOTAL_LOCATIONS,
                         MAX(MS.STORAGE_AREA) AS AREA
                    FROM <proxy />MASTER_STORAGE_LOCATION MS
                   GROUP BY MS.STORAGE_AREA)
                SELECT Q1.AREA,
                       Q1.AREA_SHORT_NAME, 
                       Q1.DESCRIPTION,
                       Q1.CARTON_COUNT,
                       Q1.USED_LOCATION_COUNT,
                       Q1.DISTINCT_SKU,
                       Q1.QUANTITY,
                       Q2.TOTAL_LOCATIONS,
                       Q1.WAREHOUSE_LOCATION_ID,
                       Q1.WAREHOUSE_DESCRIPTION
                  FROM Q1
                  LEFT OUTER JOIN Q2
                    ON Q1.AREA = Q2.AREA
                 ORDER BY Q1.AREA
            ";

            var binder = SqlBinder.Create(row => new CartonAreaHeadline
                {
                    CartonCount = row.GetInteger("CARTON_COUNT"),
                    Area = row.GetString("AREA"),
                    AreaShortName = row.GetString("AREA_SHORT_NAME"),
                    Description = row.GetString("DESCRIPTION"),
                    DistinctSKUs = row.GetInteger("DISTINCT_SKU"),
                    Quantity = row.GetInteger("QUANTITY"),
                    TotalLocations = row.GetInteger("TOTAL_LOCATIONS") ?? 0,
                    UsedLocations = row.GetInteger("USED_LOCATION_COUNT") ?? 0,
                    Building = row.GetString("WAREHOUSE_LOCATION_ID"),
                    BuildingDescription = row.GetString("WAREHOUSE_DESCRIPTION")
                });

            return _db.ExecuteReader(QUERY, binder);
        }


        public IList<CartonLocationHeadline> GetCartonLocationList()
        {
            Contract.Assert(_db != null);
            const string QUERY = @"
                                   WITH LOC AS
                         (SELECT M.LOCATION_ID, M.WAREHOUSE_LOCATION_ID, M.STORAGE_AREA
                            FROM MASTER_STORAGE_LOCATION M),
                        FILLED_LOC AS
                         (SELECT S.LOCATION_ID,
                                 S.CARTON_STORAGE_AREA,
                                 COUNT(S.CARTON_ID) AS CARTON_COUNT
                            FROM SRC_CARTON S
                           WHERE S.LOCATION_ID IS NOT NULL
                           GROUP BY S.LOCATION_ID, S.CARTON_STORAGE_AREA)
                        SELECT LOC.LOCATION_ID,
                               LOC.WAREHOUSE_LOCATION_ID,
                               LOC.STORAGE_AREA,
                               FLOC.CARTON_COUNT,
                               TIA.SHORT_NAME
                          FROM LOC LOC
                          LEFT OUTER JOIN TAB_INVENTORY_AREA TIA
                            ON LOC.STORAGE_AREA = TIA.INVENTORY_STORAGE_AREA
                          LEFT OUTER JOIN FILLED_LOC FLOC
                            ON LOC.LOCATION_ID = FLOC.LOCATION_ID
                           AND FLOC.CARTON_STORAGE_AREA = LOC.STORAGE_AREA
                         ORDER BY LOC.STORAGE_AREA, CARTON_COUNT DESC NULLS LAST, LOC.LOCATION_ID";
            var binder = SqlBinder.Create(row => new CartonLocationHeadline
            {
                LocationId = row.GetString("Location_Id"),
                WhId = row.GetString("Warehouse_Location_Id"),
                Capacity = row.GetInteger("CARTON_COUNT"),
                Area = row.GetString("Storage_Area"),
                ShortName = row.GetString("short_name")
            });
            return _db.ExecuteReader(QUERY, binder,200);
        }

    }
}