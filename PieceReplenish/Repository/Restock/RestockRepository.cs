using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Web;
using EclipseLibrary.Oracle;

namespace DcmsMobile.PieceReplenish.Repository.Restock
{
    internal class RestockRepository : IDisposable
    {
        #region Intialization

        private readonly OracleDatastore _db;

        public OracleDatastore Db
        {
            get { return _db; }
        }
        /// <summary>
        /// we need this variable while capturing productivity
        /// </summary>
        const string Module_Name = "Restock";

        /// <summary>
        /// we need this variable while capturing productivity
        /// </summary>
        string _userName = "";

        /// <summary>
        /// Constructor of class used to create the connection to database.
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="clientInfo"></param>
        /// <param name="trace"></param>
        /// <param name="connectString"> </param>
        public RestockRepository(string userName, string clientInfo, TraceContext trace, string connectString)
        {
            var store = new OracleDatastore(trace);
            store.CreateConnection(connectString, userName);
            store.ClientInfo = clientInfo;
            _db = store;
            _userName = userName;
        }

        /// <summary>
        /// For use in unit tests
        /// </summary>
        /// <param name="db"></param>
        public RestockRepository(OracleDatastore db)
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

        public DbTransaction BeginTransaction()
        {
            return _db.BeginTransaction();
        }

        /// <summary>
        /// Gets details of  passed carton 
        /// </summary>
        /// <param name="cartonId">string</param>
        /// <returns>Retuns RestockCarton object</returns>
        public RestockCarton GetCartonDetails(string cartonId)
        {

            const string QUERY = @"
                            SELECT 0 AS CARTON_TYPE,
                                   SC.CARTON_ID AS CARTON_ID,
MAX(sc.pallet_id) AS pallet_id,
                                   MAX(SC.QUALITY_CODE) AS QUALITY_CODE,
                                   MAX(SC.VWH_ID) AS VWH_ID,
                                   MAX(MSKU.SKU_ID) AS SKU_ID,
                                   MAX(MSKU.STYLE) AS STYLE,
                                   MAX(MSKU.COLOR) AS COLOR,
                                   MAX(MSKU.DIMENSION) AS DIMENSION,
                                   MAX(MSKU.SKU_SIZE) AS SKU_SIZE,
                                   MAX(MSKU.UPC_CODE) AS UPC_CODE,
                                   MAX(MS.LABEL_ID) AS LABEL_ID,
                                   MAX(SCD.QUANTITY) AS QUANTITY,
                                   COUNT(DISTINCT SCD.SKU_ID) AS NUMBER_OF_SKU,
                                   MAX(SC.WORK_NEEDED_XML) AS WORK_NEEDED_XML,
                                   MAX(SC.CARTON_STORAGE_AREA) AS CARTON_STORAGE_AREA,
                                   MAX(MSKU.PIECES_PER_PACKAGE) AS PIECES_PER_PACKAGE,
                                   MAX(MSKU.RETAIL_PRICE) AS RETAIL_PRICE
                              FROM <proxy />SRC_CARTON SC
                              LEFT OUTER JOIN <proxy />SRC_CARTON_DETAIL SCD
                                ON SCD.CARTON_ID = SC.CARTON_ID
                              LEFT OUTER JOIN <proxy />MASTER_SKU MSKU
                                ON SCD.SKU_ID = MSKU.SKU_ID
                              LEFT OUTER JOIN <proxy />MASTER_STYLE MS
                                ON MS.STYLE = MSKU.STYLE
                             WHERE SC.CARTON_ID = :CARTON_ID
                             GROUP BY SC.CARTON_ID
                UNION ALL
                        SELECT 1,
                               SC.CARTON_ID AS CARTON_ID,
NULL,
                               SC.QUALITY_CODE AS QUALITY_CODE,
                               SC.VWH_ID AS VWH_ID,
                               SC.SKU_ID AS SKU_ID,
                               NULL AS STYLE_,
                               NULL AS COLOR_,
                               NULL AS DIMENSION_,
                               NULL AS SKU_SIZE_,
                               SC.UPC_CODE AS UPC_CODE_,
                               NULL AS LABEL_ID_,
                               SC.TOTAL_CARTON_QUANTITY AS QUANTITY,
                               1 AS NUMBER_OF_SKU,
                               NULL AS WORK_NEEDED_XML,
                               NULL AS CARTON_STORAGE_AREA,
                               NULL AS PIECES_PER_PACKAGE_,
                               NULL AS RETAIL_PRICE_
                          FROM <proxy />SRC_OPEN_CARTON SC
                         WHERE SC.CARTON_ID = :CARTON_ID
                  ORDER BY 1";

            var binder = SqlBinder.Create(row => new RestockCarton
                                    {
                                        CartonId = row.GetString("CARTON_ID"),
                                        CartonType = row.GetInteger("carton_type") ?? 0,
                                        QualityCode = row.GetString("QUALITY_CODE"),
                                        VwhId = row.GetString("VWH_ID"),
                                        PiecesInCarton = row.GetInteger("QUANTITY") ?? 0,
                                        SkuCount = row.GetInteger("NUMBER_OF_SKU"),
                                        CartonStorageArea = row.GetString("CARTON_STORAGE_AREA"),
                                        IsWorkNeeded = !string.IsNullOrEmpty(row.GetString("WORK_NEEDED_XML")),
                                        SkuId = row.GetInteger("SKU_ID"),
                                        Style = row.GetString("STYLE"),
                                        Color = row.GetString("COLOR"),
                                        Dimension = row.GetString("DIMENSION"),
                                        SkuSize = row.GetString("SKU_SIZE"),
                                        UpcCode = row.GetString("UPC_CODE"),
                                        LabelId = row.GetString("LABEL_ID"),
                                        PiecesPerPackage = row.GetInteger("PIECES_PER_PACKAGE") ?? 0,
                                        RetailPrice = row.GetDecimal("RETAIL_PRICE"),
                                        PalletId = row.GetString("pallet_id")
                                    });
            binder.Parameter("CARTON_ID", cartonId);
            return _db.ExecuteSingle(QUERY, binder);
        }


        /// <summary>
        /// Gets location suggestions for passed UPC ,VWH and Building.
        /// If location passed it gets details of the passed location.
        /// </summary>
        /// <param name="upcCode">string</param>
        /// <param name="vwhId">string</param>
        /// <returns>List of locations if no location passed else returns passed locations detail</returns>
        public IList<AssignedLocation> GetAssignedLocations(int skuId)
        {
            const string QUERY = @"
                SELECT I.LOCATION_ID             AS LOCATION_ID,
                       I.RESTOCK_AISLE_ID        AS RESTOCK_AISLE_ID,
                       I.IA_ID                   AS IA_ID,
                       I.ASSIGNED_UPC_MAX_PIECES  AS RAILCAPACITY, 
                       I.VWH_ID as VWH_ID,
                       I.WAREHOUSE_LOCATION_ID   AS WAREHOUSE_LOCATION_ID,
                       IC.NUMBER_OF_UNITS        AS PIECES_AT_LOCATION
                  FROM <proxy />IALOC I
                  inner join <proxy />master_sku ms 
                    on ms.upc_code = i.assigned_upc_code
                  LEFT OUTER JOIN <proxy />IALOC_CONTENT IC
                    ON I.LOCATION_ID = IC.LOCATION_ID
                   AND I.ASSIGNED_UPC_CODE = IC.IACONTENT_ID
                 WHERE ms.SKU_ID = :SKU_ID
                   AND I.CAN_ASSIGN_SKU = 1
                ";
            var binder = SqlBinder.Create(row => new AssignedLocation
                                    {
                                        LocationId = row.GetString("LOCATION_ID"),
                                        RestockAisleId = row.GetString("RESTOCK_AISLE_ID"),
                                        PiecesAtLocation = row.GetInteger("PIECES_AT_LOCATION") ?? 0,
                                        RailCapacity = row.GetInteger("RAILCAPACITY"),
                                        IaId = row.GetString("IA_ID"),
                                        BuildingId = row.GetString("WAREHOUSE_LOCATION_ID"),
                                        AssignedVwhId = row.GetString("VWH_ID")
                                    });
            binder.Parameter("SKU_ID", skuId);
            return _db.ExecuteReader(QUERY, binder);
        }


        /// <summary>
        /// Gets order quality from tab_quality_code table
        /// </summary>
        /// <returns>list of qualities</returns>
        public IList<string> GetOrderQualities()
        {
            const string QUERY = @"
                SELECT TQ.QUALITY_CODE AS QUALITY_CODE
                    FROM <proxy />TAB_QUALITY_CODE TQ
                  WHERE TQ.ORDER_QUALITY IS NOT NULL";

            var binder = SqlBinder.Create(row => row.GetString("QUALITY_CODE"));
            return _db.ExecuteReader(QUERY, binder);
        }

        /// <summary>
        /// This function restocks passed carton on the passed location 
        /// </summary>
        /// <param name="carton">RestockCarton entity</param>
        /// <param name="locationId"></param>
        public void RestockCarton(RestockCarton carton, string locationId)
        {
            if (carton == null)
            {
                throw new ArgumentNullException("carton");
            }
            if (carton.SkuId == null)
            {
                throw new ArgumentNullException("carton.SkuId");
            }

            const string QUERY = @"
                    declare
                        LRelatedTransactionId NUMBER(10);
                    begin
                      <proxy />pkg_resv.add_to_ialoc2(alocation_id =&gt; :alocation,
                                             asku_id =&gt; :asku_id,
                                             apieces =&gt; :apieces);
                                                  
                       LRelatedTransactionId := <proxy />pkg_inv_3.openctn(acarton_id =&gt; :acarton_id,
                                                   adestination_area =&gt; :adestination_area,
                                                   arelated_transaction_id =&gt; NULL);
                    end;    
            ";

            var binder = SqlBinder.Create();
            binder.Parameter("alocation", locationId)
                    .Parameter("asku_id", carton.SkuId)
                    .Parameter("apieces", carton.PiecesInCarton)
                    .Parameter("acarton_id", carton.CartonId)
                    .Parameter("adestination_area", "SHL");

            _db.ExecuteNonQuery(QUERY, binder);
        }

        /// <summary>
        /// Puts carton in suspense.
        /// </summary>
        /// <param name="cartonId"></param>
        public void SuspenseCarton(string cartonId)
        {
            const string QUERY = @"
            UPDATE <proxy />SRC_CARTON SET SUSPENSE_DATE = SYSDATE WHERE CARTON_ID = :CARTON_ID
            ";

            var binder = SqlBinder.Create();
            binder.Parameter("CARTON_ID", cartonId);

            _db.ExecuteNonQuery(QUERY, binder);
        }

        /// <summary>
        /// Captures carton_productivity.
        /// </summary>
        /// <remarks>
        /// Productivity of a carton must be captured in the talbel carton_prodcutivity, as we do not
        /// want to change the behavior of Restock Prodctivity report we are inserting it in box_productivity.
        /// </remarks>
        public void CaptureProductivity(RestockCarton carton, bool isRestockCarton)
        {
            const string QUERY = @"
                    INSERT INTO <proxy />box_productivity
                    (
                        OPERATION_CODE,
                        MODULE_CODE,
                        OPERATOR,
                        OPERATION_START_DATE,
                        OPERATION_END_DATE,
                        OUTCOME,
                        UCC128_ID,
                        UPC_CODE,
                        LABEL_ID,
                        TO_LOCATION_ID,
                        FROM_INVENTORY_AREA,
                        TO_INVENTORY_AREA,
                        NUM_OF_PIECES,
                        NUM_OF_UNITS,
                        WAREHOUSE_LOCATION_ID,
                        VWH_ID,
FROM_PALLET)
                    VALUES
                    (
                        '$RST',
                        :MODULE,
                        NVL(:OPERATOR,user),
                        SYSDATE,
                        SYSDATE,
                        :OUTCOME,
                        :CARTON_ID,
                        :UPC_CODE,
                        :LABEL_ID,
                        :LOCATION_ID,
                        :CARTON_SOURCE_AREA,
                        :CARTON_DESTINATION_AREA,
                        :PIECES,
                        :NO_OF_UNITS,
                        :WAREHOUSE_LOCATION_ID,
                        :VWH_ID,
:PALLET_ID)
            ";
            var binder = SqlBinder.Create();
            binder.Parameter("CARTON_ID", carton.CartonId)
                    .Parameter("UPC_CODE", carton.UpcCode)
                    .Parameter("OPERATOR", _userName)
                    .Parameter("MODULE", Module_Name)
                    .Parameter("LOCATION_ID", carton.RestockAtLocation)
                    .Parameter("CARTON_SOURCE_AREA", carton.CartonStorageArea)
                    .Parameter("CARTON_DESTINATION_AREA", isRestockCarton ? carton.PickAreaId : carton.CartonStorageArea)
                    .Parameter("PIECES", carton.PiecesInCarton)
                    .Parameter("NO_OF_UNITS", carton.NumberOfUnits)
                    .Parameter("WAREHOUSE_LOCATION_ID", carton.BuildingId)
                    .Parameter("OUTCOME", isRestockCarton ? "RESTOCKED" : "CARTON IN SUSPENSE")
                    .Parameter("VWH_ID", carton.VwhId)
                    .Parameter("LABEL_ID", carton.LabelId)
                    .Parameter("PALLET_ID", carton.PalletId);
            _db.ExecuteNonQuery(QUERY, binder);
        }

        /// <summary>
        /// Used to validated the UPC code
        /// </summary>
        /// <param name="barCode"></param>
        /// <returns></returns>
        public int? GetSkuIdFromBarCode(string barCode)
        {
            const string QUERY = @"
            SELECT 
                   MS.SKU_ID    AS SKU_ID
              FROM <proxy />MASTER_SKU MS
             WHERE MS.UPC_CODE = :UPCCODE
               AND MS.INACTIVE_FLAG IS NULL
            UNION ALL
            SELECT MS.SKU_ID
              FROM <proxy />MASTER_CUSTOMER_SKU MCS
             INNER JOIN <proxy />MASTER_SKU MS
                ON  MS.SKU_ID = MCS.SKU_ID
             WHERE MS.INACTIVE_FLAG IS NULL
               AND NVL(MCS.SCANNED_BAR_CODE, MCS.CUSTOMER_SKU_ID) = :UPCCODE";

            var binder = SqlBinder.Create(row => row.GetInteger("SKU_ID"));
            binder.Parameter("UPCCODE", barCode);
            return _db.ExecuteSingle(QUERY, binder);
        }
    }
}