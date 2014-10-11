using System;
using System.Collections.Generic;
using System.Web;
using EclipseLibrary.Oracle;

namespace DcmsMobile.PieceReplenish.Repository.Diagnostic
{
    public class DiagnosticRepository
    {
        #region Intialization

        private readonly OracleDatastore _db;

        public OracleDatastore Db
        {
            get { return _db; }
        }
        
        /// <summary>
        /// Constructor of class used to create the connection to database.
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="clientInfo"></param>
        /// <param name="trace"></param>
        /// <param name="connectString"> </param>
        public DiagnosticRepository(string userName, string clientInfo, TraceContext trace, string connectString)
        {
            var store = new OracleDatastore(trace);
            store.CreateConnection(connectString, userName);
            store.ClientInfo = clientInfo;
            store.ModuleName = "PieceReplenish";
            _db = store;
        }

        /// <summary>
        /// For use in unit tests
        /// </summary>
        /// <param name="db"></param>
        public DiagnosticRepository(OracleDatastore db)
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

        internal IEnumerable<SkuRequirement> GetSkuRequirements(int? skuId)
        {
            const string QUERY = @"
            SELECT MS.SKU_ID                AS SKU_ID,
                   MS.STYLE                 AS STYLE,
                   MS.COLOR                 AS COLOR,
                   MS.DIMENSION             AS DIMENSION,
                   MS.SKU_SIZE              AS SKU_SIZE,
                   MS.UPC_CODE              AS UPC_CODE,
                   L.VWH_ID                 AS VWH_ID,
                   NVL(I.WAREHOUSE_LOCATION_ID, L.WAREHOUSE_LOCATION_ID) AS WAREHOUSE_LOCATION_ID,
                   L.IA_ID                  AS PICK_IA_ID,
                   I.SHORT_NAME             AS SHORT_NAME,
                   L.RESTOCK_IA_ID          AS RESTOCK_AREA_ID,
                   I.Default_Repreq_Ia_Id   AS CARTON_AREA_ID,
                   L.LOCATION_ID            AS LOCATION_ID,
                   L.RESTOCK_AISLE_ID       AS RESTOCK_AISLE_ID,
                   C.NUMBER_OF_UNITS        AS PIECES_AT_LOCATIONS,
                   L.ASSIGNED_UPC_MAX_PIECES AS PIECES_CAPACITY,
                   L.ASSIGNED_UPC_MAX_PIECES - NVL(C.NUMBER_OF_UNITS, 0) AS PIECES_REQUIRED,
                   L.LOCATION_TYPE          AS LOCATION_TYPE,
                   L.FREEZE_FLAG            AS FREEZE_FLAG
              FROM <proxy/>MASTER_SKU MS
              LEFT OUTER JOIN <proxy/>IALOC L
                ON MS.UPC_CODE = L.ASSIGNED_UPC_CODE AND L.ASSIGNED_UPC_CODE IS NOT NULL      
               AND L.CAN_ASSIGN_SKU = 1
              LEFT OUTER JOIN <proxy/>IALOC_CONTENT C
                ON L.LOCATION_ID = C.LOCATION_ID
               AND L.IA_ID = C.IA_ID
               AND C.IACONTENT_ID = L.ASSIGNED_UPC_CODE
               AND C.IACONTENT_TYPE_ID = 'SKU'
              LEFT OUTER JOIN <proxy/>IA I
                ON I.IA_ID = L.IA_ID
             WHERE  MS.SKU_ID = :SKU_ID
            ";
            var binder = SqlBinder.Create(row => new SkuRequirement
            {
                RestockAisleId = row.GetString("RESTOCK_AISLE_ID"),
                LocationId = row.GetString("LOCATION_ID"),
                BuildingId = row.GetString("WAREHOUSE_LOCATION_ID"),
                PickAreaId = row.GetString("PICK_IA_ID"),
                ShortName = row.GetString("SHORT_NAME"),
                RestockAreaId = row.GetString("RESTOCK_AREA_ID"),
                ReplenishAreaId = row.GetString("CARTON_AREA_ID"),
                LocationType = row.GetString("LOCATION_TYPE"),
                IsFrozen = row.GetString("FREEZE_FLAG") == "Y",
                LocationCapacity = row.GetInteger("PIECES_CAPACITY"),
                PiecesAtLocation = row.GetInteger("PIECES_AT_LOCATIONS") ?? 0,
                PiecesRequiredAtLocation = row.GetInteger("PIECES_REQUIRED") ?? 0,
                VwhId = row.GetString("VWH_ID"),
                Sku = row.GetInteger("SKU_ID") == null ? null : new Sku
                {
                    SkuId = row.GetInteger("SKU_ID").Value,
                    Style = row.GetString("STYLE"),
                    Color = row.GetString("COLOR"),
                    Dimension = row.GetString("DIMENSION"),
                    SkuSize = row.GetString("SKU_SIZE"),
                    UpcCode = row.GetString("UPC_CODE")
                }
            })
            .Parameter("SKU_ID", skuId);

            return _db.ExecuteReader(QUERY, binder);
        }

        internal IEnumerable<Carton> GetCartonsOfSku(int? skuId, string restockAreaId, string cartonAreaId)
        {
            const string QUERY = @"
WITH ALL_CARTONS AS (
SELECT ROW_NUMBER() OVER(PARTITION BY NVL(IA.WAREHOUSE_LOCATION_ID, MSL.WAREHOUSE_LOCATION_ID), CTN.CARTON_STORAGE_AREA ORDER BY NVL(CTN.AREA_CHANGE_DATE,CTN.INSERT_DATE)) AS ROW_NUM,
                    CTN.CARTON_ID            AS CARTON_ID,
                    CTN.CARTON_STORAGE_AREA  AS CARTON_STORAGE_AREA,
                    MS.SKU_ID                AS SKU_ID,
                    MS.STYLE                 AS STYLE,
                    MS.COLOR                 AS COLOR,
                    MS.DIMENSION             AS DIMENSION,
                    MS.SKU_SIZE              AS SKU_SIZE,
                    MS.UPC_CODE              AS UPC_CODE,
                    CTN.VWH_ID               AS VWH_ID,
                    CTNDET.QUANTITY          AS QUANTITY,
                    CTN.DAMAGE_CODE          AS DAMAGE_CODE,
                    CTN.SUSPENSE_DATE        AS SUSPENSE_DATE,
                    CTN.WORK_NEEDED_XML      AS WORK_NEEDED_XML,
                    CTN.QUALITY_CODE         AS QUALITY_CODE,
                    CTN.LOCATION_ID          AS LOCATION_ID,
                    NVL(IA.WAREHOUSE_LOCATION_ID, MSL.WAREHOUSE_LOCATION_ID) AS WAREHOUSE_LOCATION_ID,
                    (CASE
                        WHEN CTN.QUALITY_CODE IN
                             (SELECT QUALITY_CODE
                                FROM <proxy/>TAB_QUALITY_CODE
                               WHERE ORDER_QUALITY = 'Y') THEN
                            'Y'
                       ELSE
                           NULL
                       END)                 AS IS_BEST_QUALITY_CARTON
                FROM <proxy/>SRC_CARTON CTN
               INNER JOIN <proxy/>SRC_CARTON_DETAIL CTNDET
                  ON CTN.CARTON_ID = CTNDET.CARTON_ID
                LEFT OUTER JOIN <proxy/>MASTER_SKU MS
                  ON MS.SKU_ID = CTNDET.SKU_ID
                LEFT OUTER JOIN <proxy/>MASTER_STORAGE_LOCATION MSL
                  ON MSL.LOCATION_ID = CTN.LOCATION_ID
                LEFT OUTER JOIN <proxy/>IA IA
                  ON IA.IA_ID = CTN.CARTON_STORAGE_AREA
               WHERE CTN.CARTON_STORAGE_AREA IN (:RESTOCK_AREA_ID,:PULL_AREA_ID)                 
                 AND CTNDET.SKU_ID = :SKU_ID
                 AND CTN.WORK_NEEDED_XML IS NULL
            ORDER BY ROW_NUM
)
            SELECT AC.CARTON_ID                AS CARTON_ID,
                    AC.CARTON_STORAGE_AREA     AS CARTON_STORAGE_AREA,
                    AC.SKU_ID                  AS SKU_ID,
                    AC.STYLE                   AS STYLE,
                    AC.COLOR                   AS COLOR,
                    AC.DIMENSION               AS DIMENSION,
                    AC.SKU_SIZE                AS SKU_SIZE,
                    AC.UPC_CODE                AS UPC_CODE,
                    AC.VWH_ID                  AS VWH_ID,
                    AC.QUANTITY                AS QUANTITY,
                    AC.DAMAGE_CODE             AS DAMAGE_CODE,
                    AC.SUSPENSE_DATE           AS SUSPENSE_DATE,
                    AC.WORK_NEEDED_XML         AS WORK_NEEDED_XML,
                    AC.QUALITY_CODE            AS QUALITY_CODE,
                    AC.LOCATION_ID             AS LOCATION_ID,
                    AC.WAREHOUSE_LOCATION_ID   AS WAREHOUSE_LOCATION_ID,
                    AC.IS_BEST_QUALITY_CARTON  AS IS_BEST_QUALITY_CARTON 
            FROM ALL_CARTONS AC
            WHERE AC.ROW_NUM &lt; 201
            ";
            var binder = SqlBinder.Create(row => new Carton
            {
                CartonId = row.GetString("CARTON_ID"),
                AreaId = row.GetString("CARTON_STORAGE_AREA"),
                BuildingId = row.GetString("WAREHOUSE_LOCATION_ID"),
                Quantity = row.GetInteger("QUANTITY") ?? 0,
                QualityCode = row.GetString("QUALITY_CODE"),
                IsCartonDamage = !string.IsNullOrWhiteSpace(row.GetString("DAMAGE_CODE")),
                IsCartonInSuspense = row.GetDate("SUSPENSE_DATE") != null,
                IsWorkNeeded = !string.IsNullOrWhiteSpace(row.GetString("WORK_NEEDED_XML")),
                IsBestQalityCarton = row.GetString("IS_BEST_QUALITY_CARTON") == "Y",
                LocationId = row.GetString("LOCATION_ID"),
                VwhId = row.GetString("VWH_ID"),
                SkuInCarton = row.GetInteger("SKU_ID") == null ? null : new Sku
                {
                    SkuId = row.GetInteger("SKU_ID").Value,
                    Style = row.GetString("STYLE"),
                    Color = row.GetString("COLOR"),
                    Dimension = row.GetString("DIMENSION"),
                    SkuSize = row.GetString("SKU_SIZE"),
                    UpcCode = row.GetString("UPC_CODE")
                }
            })
            .Parameter("PULL_AREA_ID", cartonAreaId)
            .Parameter("RESTOCK_AREA_ID", restockAreaId)
            .Parameter("SKU_ID", skuId);

            return _db.ExecuteReader(QUERY, binder);
        }

    }
}