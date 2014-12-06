using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.Contracts;
using System.Web;
using DcmsMobile.PalletLocating.Models;
using EclipseLibrary.Oracle;

namespace DcmsMobile.PalletLocating.Repository
{
    public class PalletLocatingRepository : IDisposable
    {
        #region Intialization

        const string MODULE_CODE = "PalletLocating";
        const string ACTION_CODE = "PLOC";

        private readonly OracleDatastore _db;

        public OracleDatastore Db
        {
            get
            {
                return _db;
            }
        }

        private int _queryCount;

        public int QueryCount
        {
            get
            {
                return _queryCount;
            }

        }

        /// <summary>
        /// Constructor of class used to create the connection to database.
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="clientInfo"></param>
        /// <param name="trace"></param>
        public PalletLocatingRepository(string userName, string clientInfo, TraceContext trace)
        {
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
        public PalletLocatingRepository(OracleDatastore db)
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
        /// Returns the list of Area.
        /// </summary>
        /// <param name="buildingId">It is required to show all areas in a building</param>
        /// <param name="areaId">It is required to show the info of particular area</param>
        /// <param name="shortName"></param>
        /// <param name="locationId">this is an optional parameter, it will be passed location id was scanned instead of area</param>
        /// <returns></returns>
        /// <remarks>
        /// Sharad 21 Dec 2011: Areas with null building are returned for any building
        /// </remarks>
        public IEnumerable<Area> GetCartonAreas(string areaId, string buildingId, string shortName, string locationId)
        {
            const string QUERY = @"
                SELECT TIA.INVENTORY_STORAGE_AREA  AS INVENTORY_STORAGE_AREA,
                       TIA.WAREHOUSE_LOCATION_ID   AS WAREHOUSE_LOCATION_ID,
                       TIA.SHORT_NAME              AS SHORT_NAME,
                       TIA.DESCRIPTION             AS DESCRIPTION,
                       TIA.REPLENISHMENT_AREA_ID   AS REPLENISHMENT_AREA_ID,
                       TIA2.SHORT_NAME             AS REPLENISHMENT_SHORT_NAME,
                       TIA.LOCATION_NUMBERING_FLAG AS LOCATION_NUMBERING_FLAG,
                       SYSDATE                     AS QUERY_TIME
                  FROM <proxy />TAB_INVENTORY_AREA TIA
                  LEFT OUTER JOIN <proxy />TAB_INVENTORY_AREA TIA2
                    ON TIA.REPLENISHMENT_AREA_ID = TIA2.INVENTORY_STORAGE_AREA
            <if c='$LOCATION_ID'>
                 INNER JOIN <proxy />MASTER_STORAGE_LOCATION MSL
                    ON MSL.STORAGE_AREA = TIA.INVENTORY_STORAGE_AREA
            </if>
             WHERE TIA.STORES_WHAT = 'CTN'
            <if c='not($AREA_ID) and not($SHORT_NAME) and not($LOCATION_ID)'>
               AND TIA.IS_PALLET_REQUIRED IS NOT NULL
               AND tia.location_numbering_flag is not null
           </if>
           <if>AND MSL.LOCATION_ID = :LOCATION_ID</if>
           <if>AND (TIA.WAREHOUSE_LOCATION_ID = :WAREHOUSE_LOCATION_ID OR TIA.WAREHOUSE_LOCATION_ID IS NULL)</if>
           <if>AND TIA.INVENTORY_STORAGE_AREA = :AREA_ID</if>
           <if>AND TIA.SHORT_NAME = :SHORT_NAME</if>
        ";
            var binder = SqlBinder.Create(row => new Area
            {
                AreaId = row.GetString("INVENTORY_STORAGE_AREA"),
                BuildingId = row.GetString("WAREHOUSE_LOCATION_ID"),
                ShortName = row.GetString("SHORT_NAME"),
                Description = row.GetString("DESCRIPTION"),
                ReplenishAreaId = row.GetString("REPLENISHMENT_AREA_ID"),
                ReplenishAreaShortName = row.GetString("REPLENISHMENT_SHORT_NAME"),
                QueryTime = row.GetDate("QUERY_TIME").Value,
                IsNumbered = row.GetString("LOCATION_NUMBERING_FLAG") == "Y"
            });
            binder.Parameter("WAREHOUSE_LOCATION_ID", buildingId)
                .Parameter("AREA_ID", areaId)
                .Parameter("SHORT_NAME", shortName)
                .Parameter("LOCATION_ID", locationId);

            ++_queryCount;
            return _db.ExecuteReader(QUERY, binder);
        }

        /// <summary>
        /// The passed id can be a pallet id or a carton id. In the latter case, the cartonId is used to deduce the pallet.
        /// </summary>
        /// <returns></returns>
        public Pallet GetPallet(string palletId, string cartonId)
        {
            const string QUERY = @"
SELECT SC.PALLET_ID                             AS PALLET_ID,
       COUNT(SC.CARTON_ID)                      AS CARTONS_COUNT,
       COUNT(DISTINCT SC.CARTON_STORAGE_AREA)   AS AREA_COUNT,
       COUNT(DISTINCT SC.LOCATION_ID)           AS LOCATION_COUNT,
       MIN(SC.LOCATION_ID)                      AS LOCATION_ID,
       MIN(SC.CARTON_STORAGE_AREA)              AS CARTON_STORAGE_AREA,
       MIN(SC.QUALITY_CODE)                     AS QUALITY_CODE,
       MIN(SC.VWH_ID)                           AS CTN_VWH_ID,
       COUNT(DISTINCT SC.VWH_ID)                AS CTN_VWH_COUNT,
       COUNT(DISTINCT CTNDET.SKU_ID)            AS SKU_COUNT,
       COUNT(DISTINCT SC.QUALITY_CODE)          AS QUALITY_COUNT,
       MIN(CTNDET.SKU_ID)                       AS SKU_ID,
       MIN(MSKU.STYLE)                          AS STYLE,
       MIN(MSKU.COLOR)                          AS COLOR,
       MIN(MSKU.DIMENSION)                      AS DIMENSION,
       MIN(MSKU.SKU_SIZE)                       AS SKU_SIZE,
       MIN(MSKU.UPC_CODE)                       AS UPC_CODE,
       MIN(TIA.SHORT_NAME)                      AS SHORT_NAME,
       SUM(CTNDET.QUANTITY)                     AS TOTAL_QUANTITY
  FROM <proxy />SRC_CARTON SC
 INNER JOIN <proxy />SRC_CARTON_DETAIL CTNDET
    ON SC.CARTON_ID = CTNDET.CARTON_ID
 INNER JOIN <proxy />MASTER_SKU MSKU
    ON MSKU.SKU_ID = CTNDET.SKU_ID
 INNER JOIN <proxy />TAB_INVENTORY_AREA TIA
    ON TIA.INVENTORY_STORAGE_AREA = SC.CARTON_STORAGE_AREA
WHERE 1 = 1
<if>
     AND SC.PALLET_ID = :PALLET_ID
</if>
<if>
    AND SC.PALLET_ID IN (select ctn.PALLET_ID FROM <proxy />SRC_CARTON ctn WHERE ctn.CARTON_ID = :CARTON_ID)
</if>
   GROUP BY SC.PALLET_ID
        ";
            var binder = SqlBinder.Create(row => new Pallet
            {
                PalletId = row.GetString("PALLET_ID"),
                CartonCount = row.GetInteger("CARTONS_COUNT") ?? 0,
                AreaCount = row.GetInteger("AREA_COUNT") ?? 0,
                LocationCount = row.GetInteger("LOCATION_COUNT") ?? 0,
                CartonVwhId = row.GetString("CTN_VWH_ID"),
                CartonVwhCount = row.GetInteger("CTN_VWH_COUNT") ?? 0,
                LocationId = row.GetString("LOCATION_ID"),
                SkuCount = row.GetInteger("SKU_COUNT") ?? 0,
                CartonQuality = row.GetString("QUALITY_CODE"),
                CartonQualityCount = row.GetInteger("QUALITY_COUNT") ?? 0,
                PalletArea = new Area
                {
                    AreaId = row.GetString("CARTON_STORAGE_AREA"),
                    ShortName = row.GetString("SHORT_NAME")
                },
                PalletSku = row.GetInteger("SKU_ID") == null ? null : new Sku
                {
                    SkuId = row.GetInteger("SKU_ID").Value,
                    Style = row.GetString("STYLE"),
                    Color = row.GetString("COLOR"),
                    Dimension = row.GetString("DIMENSION"),
                    SkuSize = row.GetString("SKU_SIZE"),
                    UpcCode = row.GetString("UPC_CODE"),
                    Quantity = row.GetInteger("TOTAL_QUANTITY") ?? 0
                }
            });
            binder.Parameter("PALLET_ID", palletId)
                .Parameter("CARTON_ID", cartonId);

            ++_queryCount;
            var pallet = _db.ExecuteSingle(QUERY, binder);
            if (pallet != null)
            {
                pallet.QueryTime = DateTime.Now;
            }
            return pallet;
        }

        /// <summary>
        /// Returns locations in passed area where passed SKU can be located
        /// </summary>
        /// <param name="areaId"></param>
        /// <param name="skuId">Prefers locations which have this SKU assigned, followed by locations where this SKU is located, followed by
        /// empty locations. If this is null, only empty unassigned locations are considered.
        /// </param>
        /// <param name="minCartonsToLocate">Only locations which have space for these many cartons are returned</param>
        /// <param name="maxSuggestions"></param>
        /// <param name="assignedVwhId">Prefers locations which having this VWh Id  assigned, only in case of assigned locations</param>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// Unavailable locations are not considered
        /// </para>
        /// <para>
        /// Query assigned_locations: Returns all locations to which the SKU is assigned
        /// </para>
        /// <para>
        /// Query containing_locations: Returns all locations which contain the SKU
        /// </para>
        /// <para>
        /// Query unassigned_empty_locations: Returns all unassigned locations which are empty
        /// </para>
        /// <para>
        /// The final query performs a full outer join to return information about each location. The locations are returned ordered by
        /// 1. Assigned locations having space
        /// 2. Locations having minimum cartons of the passed SKU
        /// 3. Empty unassigned locations
        /// </para>
        /// </remarks>
        public IList<CartonLocation> GetLocationsForSku(string areaId, int? skuId, int minCartonsToLocate, int maxSuggestions, string assignedVwhId)
        {
            const string QUERY = @"
with assigned_locations as
 (select msl.location_id              as location_id,
         msl.assigned_max_cartons as assigned_max_cartons,
         msl.assigned_sku_id AS assigned_sku_id,
         tia.inventory_storage_area AS area_id,
         tia.short_name AS short_name
    from <proxy />master_storage_location msl
inner join <proxy />tab_inventory_area tia on tia.inventory_storage_area = msl.storage_area
<if c='$skuId'>
   where msl.storage_area = :areaId
     and msl.assigned_sku_id = :skuId
     and msl.assigned_vwh_id = :assigned_vwh_id
     and msl.assigned_max_cartons is not null
</if>
<else>
where 1 = 2
</else>
),
containing_locations AS
 (select ctn.location_id AS location_id, MAX(ctn.carton_storage_area) AS area_id, MAX(tia.short_name) AS short_name,
         count(DISTINCT ctn.carton_id) as carton_count
    from <proxy />src_carton ctn
   inner join <proxy />src_carton_detail ctndet
      on ctn.carton_id = ctndet.carton_id
inner join <proxy />tab_inventory_area tia on tia.inventory_storage_area = ctn.carton_storage_area
<if c='$skuId'>
   where ctn.carton_storage_area = :areaId
     and ctn.location_id is not null
     and ctndet.sku_id = :skuId
</if>
<else>
where 1 = 2
</else>
group by ctn.location_id
),

unassigned_empty_locations AS
 (select DISTINCT msl.location_id AS location_id, tia.inventory_storage_area AS area_id, tia.short_name AS short_name
    from <proxy />master_storage_location msl
inner join <proxy />tab_inventory_area tia on tia.inventory_storage_area = msl.storage_area
   where msl.storage_area = :areaId
     and msl.assigned_sku_id is null
    and msl.unavailable_flag is null
  minus
  select ctn.location_id, tia.inventory_storage_area, tia.short_name
    from <proxy />src_carton ctn
inner join <proxy />tab_inventory_area tia on tia.inventory_storage_area = ctn.carton_storage_area
   where ctn.carton_storage_area = :areaId)

select NVL(NVL(al.location_id, cl.location_id), uel.location_id) as location_id,
       NVL(NVL(al.area_id, cl.area_id), uel.area_id) as area_id,
       NVL(NVL(al.short_name, cl.short_name), uel.short_name) as short_name,
       al.assigned_max_cartons AS assigned_max_cartons,
       cl.carton_count AS carton_count,
       al.assigned_sku_id AS assigned_sku_id
  from assigned_locations al
  full outer join containing_locations cl
    on al.location_id = cl.location_id
  full outer join unassigned_empty_locations uel
    on al.location_id = uel.location_id
<if>
where (al.assigned_max_cartons IS NULL OR al.assigned_max_cartons - NVL(cl.carton_count, 0) &gt;= :minCartonsToLocate)
</if>
 order by (al.assigned_max_cartons - NVL(cl.carton_count, 0)) desc nulls last,
          cl.carton_count asc nulls last,
          NVL(NVL(al.location_id, cl.location_id), uel.location_id) asc

        ";
            var binder = SqlBinder.Create(row => new CartonLocation
            {
                LocationId = row.GetString("location_id"),
                MaxCartons = row.GetInteger("assigned_max_cartons"),
                CartonCount = row.GetInteger("carton_count") ?? 0,
                AssignedSku = row.GetInteger("assigned_sku_id") == null ? null : new Sku
                {
                    SkuId = row.GetInteger("assigned_sku_id").Value
                },
                Area = new Area
                {
                    AreaId = row.GetString("area_id"),
                    ShortName = row.GetString("short_name")
                }
            });
            binder.Parameter("areaId", areaId)
                            .Parameter("minCartonsToLocate", minCartonsToLocate)
                            .Parameter("assigned_vwh_id", assignedVwhId)
                            .Parameter("skuId", skuId);
            ++_queryCount;
            var locations = _db.ExecuteReader(QUERY, binder, maxSuggestions);
            return locations;
        }


        /// <summary>
        /// Locates all the cartons on pallet to passed locationId
        /// If the parameter mergeOnPallet is not null then we merge the passed pallet also
        /// </summary>
        /// <param name="locationId"></param>
        /// <param name="palletId"></param>
        /// <param name="areaId"></param>
        /// <param name="mergeOnPallet"></param>
        /// <remarks>
        /// We insert productivity info also using this function.
        /// Action code is PLOC.
        /// Sharad 24 Jan 2012: Set suspense date of each carton to null, because cartons are no longer in suspense
        /// </remarks>
        public void LocatePallet(string locationId, string palletId, string areaId, string mergeOnPallet)
        {
            const string QUERY = @"
DECLARE

  LQUANTITY              <proxy />SRC_CARTON_DETAIL.QUANTITY%TYPE;
  LSOURCE_AREA           <proxy />SRC_CARTON.CARTON_STORAGE_AREA%TYPE;
  LVWH_ID                <proxy />SRC_CARTON.VWH_ID%TYPE;
  LWAREHOUSE_LOCATION_ID <proxy />TAB_INVENTORY_AREA.WAREHOUSE_LOCATION_ID%TYPE;
  LTRAVEL_SEQUENCE       <proxy />MASTER_STORAGE_LOCATION.TRAVEL_SEQUENCE%TYPE;

BEGIN

  SELECT SUM(SD.QUANTITY) AS QUANTITY,
         MAX(S.CARTON_STORAGE_AREA) AS SOURCE_AREA,
         MAX(S.VWH_ID) AS VWH_ID,
         MAX(TIA.WAREHOUSE_LOCATION_ID) AS WAREHOUSE_LOCATION_ID,
         MAX((SELECT MSL.TRAVEL_SEQUENCE
               FROM <proxy />MASTER_STORAGE_LOCATION MSL
              WHERE MSL.LOCATION_ID = :LOCATION_ID)) AS TRAVEL_SEQUENCE
    INTO LQUANTITY,
         LSOURCE_AREA,
         LVWH_ID,
         LWAREHOUSE_LOCATION_ID,
         LTRAVEL_SEQUENCE
    FROM <proxy />SRC_CARTON S
   INNER JOIN <proxy />SRC_CARTON_DETAIL SD
      ON S.CARTON_ID = SD.CARTON_ID
   INNER JOIN <proxy />MASTER_SKU MS
      ON MS.SKU_ID = SD.SKU_ID
    LEFT OUTER JOIN <proxy />TAB_INVENTORY_AREA TIA
      ON TIA.INVENTORY_STORAGE_AREA = S.CARTON_STORAGE_AREA
   WHERE S.PALLET_ID = :PALLET_ID
   GROUP BY S.PALLET_ID;
 
    INSERT INTO <proxy />CARTON_PRODUCTIVITY
      (PRODUCTIVITY_ID,
       MODULE_CODE,
       ACTION_CODE,
       PROCESS_START_DATE,
       PROCESS_END_DATE,
       PALLET_ID,
       CARTON_QUANTITY,
       CARTON_SOURCE_AREA,
       CARTON_DESTINATION_AREA,
       AISLE,
       VWH_ID,
       WAREHOUSE_LOCATION_ID)
    VALUES
      (PRODUCTIVITY_SEQUENCE.NEXTVAL,
       :MODULE_CODE,
       :ACTION_CODE,
       SYSDATE,
       SYSDATE,
       :PALLET_ID,
       LQUANTITY,
       LSOURCE_AREA,
       :AREA_ID,
       LTRAVEL_SEQUENCE,
       LVWH_ID,
       LWAREHOUSE_LOCATION_ID);
 
               UPDATE <proxy />SRC_CARTON SC 
                  SET SC.LOCATION_ID = :LOCATION_ID,
                 <if> SC.pallet_id = :MERGE_ON_PALLET,</if>
                      SC.CARTON_STORAGE_AREA = :AREA_ID,
sc.suspense_date = null
                WHERE SC.PALLET_ID = :PALLET_ID;
END;
            ";
            var binder = new SqlBinder("LocatePallet")
            .Parameter("PALLET_ID", palletId)
            .Parameter("AREA_ID", areaId)
            .Parameter("LOCATION_ID", locationId)
            .Parameter("MERGE_ON_PALLET", mergeOnPallet)
            .Parameter("MODULE_CODE", MODULE_CODE)
            .Parameter("ACTION_CODE", ACTION_CODE);
            ++_queryCount;
            _db.ExecuteNonQuery(QUERY, binder);
        }

        public CartonLocation GetLocation(string locationId)
        {
            const string QUERY = @"
            SELECT MSL.LOCATION_ID                  AS LOCATION_ID,
                   COUNT(DISTINCT SCD.SKU_ID)       AS SKU_COUNT,                   
                   COUNT(SC.CARTON_ID)              AS CARTONS_COUNT,
                   MAX(MSL.ASSIGNED_MAX_CARTONS)    AS ASSIGNED_MAX_CARTONS,
                   MAX(MSL.STORAGE_AREA)            AS STORAGE_AREA,
                   MAX(TIA.SHORT_NAME)              AS SHORT_NAME,
                   MAX(MSL.UNAVAILABLE_FLAG)        AS UNAVAILABLE_FLAG,
                   MAX(MSL.ASSIGNED_SKU_ID)         AS SKU_ID,
                   MAX(MSKU.STYLE)                  AS STYLE_,
                   MAX(MSKU.COLOR)                  AS COLOR_,
                   MAX(MSKU.DIMENSION)              AS DIMENSION_,
                   MAX(MSKU.SKU_SIZE)               AS SKU_SIZE_,
                   MAX(MSKU.UPC_CODE)               AS UPC_CODE_,
                   MAX(MSKU2.SKU_ID)                AS CTN_SKU_ID,
                   MAX(MSL.ASSIGNED_VWH_ID)         AS ASSIGNED_VWH_ID,
                   MAX(MSKU2.STYLE)                 AS CTN_STYLE_,
                   MAX(MSKU2.COLOR)                 AS CTN_COLOR_,
                   MAX(MSKU2.DIMENSION)             AS CTN_DIMENSION_,
                   MAX(MSKU2.SKU_SIZE)              AS CTN_SKU_SIZE_,
                   MAX(MSKU2.UPC_CODE)              AS CTN_UPC_CODE_
              FROM <proxy />MASTER_STORAGE_LOCATION MSL
              LEFT OUTER JOIN <proxy />SRC_CARTON SC
                ON SC.LOCATION_ID = MSL.LOCATION_ID
              LEFT OUTER JOIN <proxy />SRC_CARTON_DETAIL SCD
                ON SCD.CARTON_ID = SC.CARTON_ID
              LEFT OUTER JOIN <proxy />MASTER_SKU    MSKU
                ON MSKU.SKU_ID = MSL.ASSIGNED_SKU_ID
              LEFT OUTER JOIN <proxy />MASTER_SKU MSKU2
                ON MSKU2.SKU_ID = SCD.SKU_ID
              LEFT OUTER JOIN <proxy />TAB_INVENTORY_AREA TIA
                ON MSL.STORAGE_AREA = TIA.INVENTORY_STORAGE_AREA
             WHERE MSL.LOCATION_ID = :LOCATION_ID
             GROUP BY MSL.LOCATION_ID
        ";
            var binder = SqlBinder.Create(row => new CartonLocation
            {
                LocationId = row.GetString("LOCATION_ID"),
                CartonCount = row.GetInteger("CARTONS_COUNT").Value,
                MaxCartons = row.GetInteger("ASSIGNED_MAX_CARTONS"),
                AssignedVWhId = row.GetString("ASSIGNED_VWH_ID"),
                SkuCount = row.GetInteger("SKU_COUNT").Value,
                UnavailableFlag = row.GetString("UNAVAILABLE_FLAG") == "Y",
                AssignedSku = row.GetInteger("SKU_ID") == null ? null : new Sku
                {
                    SkuId = row.GetInteger("SKU_ID").Value,
                    Style = row.GetString("STYLE_"),
                    Color = row.GetString("COLOR_"),
                    Dimension = row.GetString("DIMENSION_"),
                    SkuSize = row.GetString("SKU_SIZE_"),
                    UpcCode = row.GetString("UPC_CODE_")
                },
                CartonSku = row.GetInteger("CTN_SKU_ID") == null ? null : new Sku
                {
                    SkuId = row.GetInteger("CTN_SKU_ID").Value,
                    Style = row.GetString("CTN_STYLE_"),
                    Color = row.GetString("CTN_COLOR_"),
                    Dimension = row.GetString("CTN_DIMENSION_"),
                    SkuSize = row.GetString("CTN_SKU_SIZE_"),
                    UpcCode = row.GetString("CTN_UPC_CODE_")
                },
                Area = new Area
                {
                    AreaId = row.GetString("STORAGE_AREA"),
                    ShortName = row.GetString("SHORT_NAME")
                }
            })
            .Parameter("LOCATION_ID", locationId);
            ++_queryCount;
            return _db.ExecuteSingle(QUERY,binder);
        }

        /// <summary>
        /// Retrieves a list of locations in replenishAreaId from where pallets can be moved to locations in areaId
        /// </summary>
        /// <param name="buildingId"> </param>
        /// <param name="fromAreaId"></param>
        /// <param name="toAreaId"></param>
        /// <param name="maxRows"></param>
        /// <returns></returns>
        /// <remarks>
        /// Pallets containing multiple SKUs will never be suggested
        /// <para>
        /// Query CPK_LOCATIONS: Returns all CPK locations which have space available. Carton count at each location
        /// is compared with assigned_max_cartons to determine space availability.
        /// </para>
        /// <para>
        /// Query CFD_PALLETS: Returns all pallets in CFD along with the SKU content of each pallet. Pallets with multiple SKUs
        /// are not considered
        /// </para>
        /// <para>
        /// Query demand_SKU: Lists SKU in unavailable buckets
        /// </para>
        /// <para>
        /// Sharad 28 Feb 2012: Subtracting CPK inventory from demand
        /// </para>
        /// <para>
        /// DB and Ravneet 17-04-2012: 
        /// 1. Higest priority order sku's will be replenished first. 
        /// 2. We do not replenish freeze buckets now.
        /// 3. Orders which are yet to be put in a bucket are given zero priority.
        /// </para>
        /// </remarks>
        public IEnumerable<ReplenishmentSuggestion> GetReplenishmentSuggestions(string buildingId, string fromAreaId, string toAreaId, int maxRows)
        {
            const string QUERY = @"
WITH CPK_LOCATIONS AS
 (SELECT MSL.LOCATION_ID AS LOCATION_ID,
         MSL.ASSIGNED_SKU_ID AS ASSIGNED_SKU_ID,
         MAX(MSL.ASSIGNED_MAX_CARTONS) AS ASSIGNED_MAX_CARTONS,
         COUNT(DISTINCT CTN.CARTON_ID) AS CARTON_COUNT,
         COUNT(DISTINCT CTN.PALLET_ID) AS PALLET_COUNT,
         MSL.ASSIGNED_VWH_ID AS VWH_ID
    FROM <proxy />MASTER_STORAGE_LOCATION MSL
    LEFT OUTER JOIN <proxy />SRC_CARTON CTN
      ON MSL.LOCATION_ID = CTN.LOCATION_ID
     AND CTN.CARTON_STORAGE_AREA = MSL.STORAGE_AREA
   WHERE MSL.STORAGE_AREA = :AREA_ID
     AND MSL.ASSIGNED_SKU_ID IS NOT NULL
     AND MSL.ASSIGNED_MAX_CARTONS IS NOT NULL
   GROUP BY MSL.LOCATION_ID, MSL.ASSIGNED_SKU_ID, MSL.ASSIGNED_VWH_ID
  HAVING COUNT(DISTINCT CTN.CARTON_ID) &lt;= MAX(MSL.ASSIGNED_MAX_CARTONS)),

CFD_PALLETS AS
 (SELECT MAX(CTNDET.SKU_ID) AS SKU_ID,
         MAX(T.VWH_ID) AS VWH_ID,
         T.PALLET_ID AS PALLET_ID,
         MAX(T.LOCATION_ID) AS LOCATION_ID,
         COUNT(DISTINCT T.CARTON_ID) AS COUNT_CARTONS_ON_PALLET,
         MIN(T.INSERT_DATE) AS INSERT_DATE
    FROM <proxy />SRC_CARTON T
   INNER JOIN <proxy />SRC_CARTON_DETAIL CTNDET
      ON T.CARTON_ID = CTNDET.CARTON_ID
   WHERE T.CARTON_STORAGE_AREA = :REPLENISHMENT_AREA_ID
     AND T.LOCATION_ID IS NOT NULL
   GROUP BY T.PALLET_ID
  HAVING COUNT(DISTINCT CTNDET.SKU_ID) = 1 AND COUNT(DISTINCT T.VWH_ID) = 1 AND COUNT(DISTINCT T.LOCATION_ID) = 1),
  ORDERS_DETAIL AS
 (SELECT MS.SKU_ID,
         DP.VWH_ID AS VWH_ID,
         SUM(DPD.QUANTITY_ORDERED) AS QUANTITY_ORDERED,
         0 AS PRIORITY
    FROM <proxy /> DEM_PICKSLIP DP
   INNER JOIN <proxy /> DEM_PICKSLIP_DETAIL DPD
      ON DPD.PICKSLIP_ID = DP.PICKSLIP_ID
   INNER JOIN <proxy /> MASTER_SKU MS
      ON MS.STYLE = DPD.STYLE
     AND MS.COLOR = DPD.COLOR
     AND MS.DIMENSION = DPD.DIMENSION
     AND MS.SKU_SIZE = DPD.SKU_SIZE
   WHERE DP.PS_STATUS_ID = 1
     AND DP.WAREHOUSE_LOCATION_ID = :BUILDING_ID
   GROUP BY MS.SKU_ID, DP.VWH_ID
  
  UNION ALL
  
  SELECT MS.SKU_ID,
         DP.VWH_ID AS VWH_ID,
         SUM(DPD.PIECES_ORDERED) AS QUANTITY_ORDERED,
         MAX(B.PRIORITY) AS PRIORITY
    FROM <proxy /> BUCKET B
   INNER JOIN <proxy /> PS DP
      ON B.BUCKET_ID = DP.BUCKET_ID
   INNER JOIN <proxy /> PSDET DPD
      ON DPD.PICKSLIP_ID = DP.PICKSLIP_ID
   INNER JOIN <proxy /> MASTER_SKU MS
      ON MS.UPC_CODE = DPD.UPC_CODE
   WHERE DP.WAREHOUSE_LOCATION_ID = :BUILDING_ID
     AND B.STATUS = 'READYFORPULL'
     AND DPD.TRANSFER_DATE IS NULL
     AND B.FREEZE IS NULL
   GROUP BY MS.SKU_ID, DP.VWH_ID),
  
ORDERED_SKU AS
 (SELECT Q1.SKU_ID,
         Q1.VWH_ID,
         SUM(Q1.QUANTITY_ORDERED) AS QUANTITY_ORDERED,
         MAX(Q1.PRIORITY) AS PRIORITY
    FROM ORDERS_DETAIL Q1
   GROUP BY Q1.SKU_ID, Q1.VWH_ID),
RESERVED_SKU AS
 (SELECT MS.SKU_ID,
         B.VWH_ID AS VWH_ID,
         SUM(BD.current_pieces) AS QUANTITY_RESERVED
    FROM <proxy />BOX B
   INNER JOIN <proxy />BOXDET BD
      ON B.PICKSLIP_ID = BD.PICKSLIP_ID
     AND B.UCC128_ID = BD.UCC128_ID
   INNER JOIN <proxy />PS P
      ON P.PICKSLIP_ID = B.PICKSLIP_ID
   INNER JOIN <proxy /> MASTER_SKU MS
      ON BD.UPC_CODE = MS.UPC_CODE
   WHERE P.WAREHOUSE_LOCATION_ID = :BUILDING_ID
     AND P.TRANSFER_DATE IS NULL
     AND B.STOP_PROCESS_DATE IS NULL
     AND BD.STOP_PROCESS_DATE IS NULL
   GROUP BY MS.SKU_ID, B.VWH_ID),
CPK_SKU (SKU_ID, VWH_ID, CPK_PIECES) AS
 (SELECT CTNDET.SKU_ID, CTN.VWH_ID, SUM(CTNDET.QUANTITY)
    FROM <proxy /> SRC_CARTON CTN
   INNER JOIN <proxy /> SRC_CARTON_DETAIL CTNDET
      ON CTN.CARTON_ID = CTNDET.CARTON_ID
   WHERE CTN.CARTON_STORAGE_AREA = 'CPK'
   GROUP BY CTNDET.SKU_ID, CTN.VWH_ID),
DEMAND_SKU AS
 (SELECT OS.SKU_ID,
         OS.VWH_ID,
         OS.PRIORITY,
         OS.QUANTITY_ORDERED,
         RS.QUANTITY_RESERVED,
         OS.QUANTITY_ORDERED - NVL(RS.QUANTITY_RESERVED, 0) -
         NVL(CS.CPK_PIECES, 0) AS DEMAND_PIECES
    FROM ORDERED_SKU OS
    LEFT OUTER JOIN RESERVED_SKU RS
      ON RS.SKU_ID = OS.SKU_ID
     AND RS.VWH_ID = OS.VWH_ID
    LEFT OUTER JOIN CPK_SKU CS
      ON CS.SKU_ID = OS.SKU_ID
     AND CS.VWH_ID = OS.VWH_ID
   WHERE OS.QUANTITY_ORDERED - NVL(RS.QUANTITY_RESERVED, 0) -
         NVL(CS.CPK_PIECES, 0) > 0)
SELECT CPK.LOCATION_ID
       
                   AS CPK_LOCATION_ID,
       CPK.ASSIGNED_MAX_CARTONS    AS ASSIGNED_MAX_CARTONS,
       CPK.CARTON_COUNT            AS CPK_CARTON_COUNT,
       CPK.ASSIGNED_SKU_ID         AS ASSIGNED_SKU_ID,
       CFD.LOCATION_ID             AS CFD_LOCATION_ID,
       CFD.COUNT_CARTONS_ON_PALLET AS CFD_COUNT_CARTONS_ON_PALLET,
       CFD.PALLET_ID               AS PALLET_ID,
       MSKU.STYLE                  AS STYLE,
       MSKU.COLOR                  AS COLOR,
       MSKU.DIMENSION              AS DIMENSION,
       MSKU.SKU_SIZE               AS SKU_SIZE,
       CFD.VWH_ID                  AS VWH_ID,
       DS.DEMAND_PIECES            AS DEMAND_PIECES,
       SYSDATE                     AS QUERY_TIME
  FROM CPK_LOCATIONS CPK
 INNER JOIN CFD_PALLETS CFD
    ON CPK.ASSIGNED_SKU_ID = CFD.SKU_ID
 INNER JOIN  <proxy /> MASTER_SKU MSKU
    ON MSKU.SKU_ID = CPK.ASSIGNED_SKU_ID
  LEFT OUTER JOIN DEMAND_SKU DS
    ON DS.SKU_ID = CPK.ASSIGNED_SKU_ID
   AND DS.VWH_ID = CPK.VWH_ID
 WHERE CPK.ASSIGNED_MAX_CARTONS - CPK.CARTON_COUNT >=
       CFD.COUNT_CARTONS_ON_PALLET
 ORDER BY DS.PRIORITY      DESC NULLS LAST,
          DS.DEMAND_PIECES DESC NULLS LAST,
          CPK.CARTON_COUNT,
          CFD.INSERT_DATE

        ";
            var binder = SqlBinder.Create(row => new ReplenishmentSuggestion
                        {
                            DestinationLocationId = row.GetString("CPK_LOCATION_ID"),
                            CartonCountAtDestinationLocation = row.GetInteger("CPK_CARTON_COUNT") ?? 0,
                            MaxCartonsAtDestinationLocation = row.GetInteger("ASSIGNED_MAX_CARTONS") ?? 0,
                            QueryTime = row.GetDate("QUERY_TIME").Value,
                            PalletLocationId = row.GetString("CFD_LOCATION_ID"),
                            CartonCountOnPallet = row.GetInteger("CFD_count_cartons_on_pallet") ?? 0,

                            PalletIdToPull = row.GetString("PALLET_ID"),
                            SkuToPull = row.GetInteger("ASSIGNED_SKU_ID") == null ? null : new Sku
                            {
                                SkuId = row.GetInteger("ASSIGNED_SKU_ID").Value,
                                Style = row.GetString("style"),
                                Color = row.GetString("color"),
                                Dimension = row.GetString("dimension"),
                                SkuSize = row.GetString("sku_size"),
                            },
                            VwhIdToPull = row.GetString("VWH_ID"),
                            SkuPriority = row.GetInteger("DEMAND_PIECES") ?? 0
                        })
            .Parameter("AREA_ID", toAreaId)
            .Parameter("REPLENISHMENT_AREA_ID", fromAreaId)
            .Parameter("BUILDING_ID", buildingId);
            ++_queryCount;
            return _db.ExecuteReader(QUERY, binder, maxRows);
        }

        public Carton GetCarton(string cartonId)
        {
            const string QUERY = @"
SELECT CTN.CARTON_ID           AS CARTON_ID,
       CTNDET.SKU_ID           AS SKU_ID,
       MSKU.STYLE              AS STYLE_,
       MSKU.COLOR              AS COLOR_,
       MSKU.DIMENSION          AS DIMENSION_,
       MSKU.SKU_SIZE           AS SKU_SIZE_,
       MSKU.UPC_CODE           AS UPC_CODE_,
       CTN.QUALITY_CODE        AS QUALITY_CODE,
       CTNDET.QUANTITY         AS QUANTITY,
       CTN.VWH_ID              AS VWH_ID,
       CTN.CARTON_STORAGE_AREA AS CARTON_STORAGE_AREA,
       TIA.SHORT_NAME          AS SHORT_NAME
  FROM <proxy />SRC_CARTON CTN
  LEFT OUTER JOIN <proxy />SRC_CARTON_DETAIL CTNDET
    ON CTNDET.CARTON_ID = CTN.CARTON_ID
  LEFT OUTER JOIN <proxy />MASTER_SKU MSKU
    ON MSKU.SKU_ID = CTNDET.SKU_ID
  LEFT OUTER JOIN <proxy />TAB_INVENTORY_AREA TIA
    ON TIA.INVENTORY_STORAGE_AREA = CTN.CARTON_STORAGE_AREA
 WHERE CTN.CARTON_ID = :CARTON_ID
";
            var binder = SqlBinder.Create(row => new Carton
            {
                CartonId = row.GetString("CARTON_ID"),
                QualityCode = row.GetString("QUALITY_CODE"),
                Pieces = row.GetInteger("QUANTITY") ?? 0,
                VwhId = row.GetString("VWH_ID"),
                Sku = row.GetInteger("SKU_ID") == null ? null : new Sku
                {
                    SkuId = row.GetInteger("SKU_ID").Value,
                    Style = row.GetString("STYLE_"),
                    Color = row.GetString("COLOR_"),
                    Dimension = row.GetString("DIMENSION_"),
                    SkuSize = row.GetString("SKU_SIZE_"),
                    UpcCode = row.GetString("UPC_CODE_")
                },
                Area = new Area
                {
                    AreaId = row.GetString("CARTON_STORAGE_AREA"),
                    ShortName = row.GetString("SHORT_NAME")
                }
            })
            .Parameter("CARTON_ID", cartonId);
            return _db.ExecuteSingle(QUERY, binder);
        }

        public bool ValidateBuilding(string buildingId)
        {
            const string QUERY = @"
select count(*) as area_count from <proxy />tab_warehouse_location t where t.warehouse_location_id = :warehouse_location_id
";
            var binder = SqlBinder.Create(row => row.GetInteger("area_count"))
            .Parameter("warehouse_location_id", buildingId);
            var n = _db.ExecuteSingle(QUERY, binder);
            return n > 0;
        }

        /// <summary>
        ///  This method is used to show the list of pallets located
        /// in last few hours.
        /// </summary>
        /// <param name="userName">
        /// This parameter will filter the query by user name.
        /// </param>
        /// <param name="insertToDate"></param>
        /// <param name="insertFromDate"></param>
        /// <returns></returns>
        /// <remarks>
        /// Sharad 3 Jan 2012: Display pallets which have actually moved by comparing from/to area/location.
        /// Do not rely on module code.
        /// </remarks>
        public IEnumerable<PalletMovement> GetPalletMovements(string userName, DateTime? insertToDate, DateTime? insertFromDate)
        {
            const string QUERY =
                        @"SELECT SCP.INSERTED_BY AS USER_NAME,
                                   SCP.FROM_PALLET_ID           AS PALLET,
                                   COUNT(SCP.CARTON_ID)         AS NUM_CARTONS,
                                   MAX(TIA1.SHORT_NAME)         AS FROM_AREA,
                                   MAX(TIA2.SHORT_NAME)         AS TO_AREA,
                                   MAX(SCP.FROM_LOCATION_ID)    AS FROM_LOCATION,
                                   MAX(SCP.TO_LOCATION_ID)      AS TO_LOCATION,
                                   SCP.INSERT_DATE              AS INSERT_DATE
                            FROM <proxy />SRC_CARTON_PROCESS_DETAIL SCP
                            LEFT OUTER JOIN <proxy />TAB_INVENTORY_AREA TIA1 ON
                                 TIA1.INVENTORY_STORAGE_AREA = SCP.FROM_CARTON_AREA
                            LEFT OUTER JOIN <proxy />TAB_INVENTORY_AREA TIA2 ON
                                 TIA2.INVENTORY_STORAGE_AREA = SCP.TO_CARTON_AREA
                           WHERE SCP.FROM_PALLET_ID IS NOT NULL and (SCP.FROM_CARTON_AREA != SCP.TO_CARTON_AREA
                                 OR SCP.FROM_LOCATION_ID != SCP.TO_LOCATION_ID)
                        <if>AND SCP.INSERT_DATE &lt;= CAST(:INSERT_TO_DATE+1 AS DATE)</if> 
                        <if>AND SCP.INSERT_DATE &gt;= CAST(:INSERT_FROM_DATE AS DATE)</if>
                        <else>AND SCP.INSERT_DATE &gt;= SYSDATE - 14</else>
                        <if>AND UPPER(SCP.INSERTED_BY) LIKE UPPER('%' || cast(:USER_NAME as varchar2(255)) || '%') </if>
                        GROUP BY  SCP.INSERT_DATE, SCP.INSERTED_BY, SCP.FROM_PALLET_ID
                        ORDER BY SCP.INSERT_DATE DESC, SCP.INSERTED_BY";

            var binder = SqlBinder.Create(row => new PalletMovement
            {
                UserName = row.GetString("USER_NAME"),
                CountCarton = row.GetInteger("NUM_CARTONS").Value,
                FromArea = row.GetString("FROM_AREA"),
                ToArea = row.GetString("TO_AREA"),
                FromLocation = row.GetString("FROM_LOCATION"),
                ToLocation = row.GetString("TO_LOCATION"),
                Pallet = row.GetString("PALLET"),
                InsertDate = row.GetDate("INSERT_DATE"),
            });
            binder.Parameter("USER_NAME", userName)
            .Parameter("INSERT_TO_DATE", insertToDate)
            .Parameter("INSERT_FROM_DATE", insertFromDate);
            // TODO: Allow service to decide max rows
            return _db.ExecuteReader(QUERY, binder, 100);
        }

        /// <summary>
        /// Place the passed carton on passed pallet.
        /// </summary>
        /// <param name="mergeOnPallet">Pallet Id on which the carton will be put</param>
        /// <param name="cartonId">Carton to be palletized</param>
        public void PalletizeCarton(string mergeOnPallet, string cartonId)
        {
            const string QUERY = @"
               UPDATE <proxy />SRC_CARTON SC 
                  SET SC.PALLET_ID = :MERGE_ON_PALLET
                WHERE SC.CARTON_ID = :CARTON_ID
            ";

            var binder = new SqlBinder("PalletizeCarton")
            .Parameter("MERGE_ON_PALLET", mergeOnPallet)
            .Parameter("CARTON_ID", cartonId);
            ++_queryCount;
            _db.ExecuteNonQuery(QUERY, binder);
        }
    }
}



/*
    $Id$ 
    $Revision$
    $URL$
    $Header$
    $Author$
    $Date$
*/