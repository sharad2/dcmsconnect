using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.Contracts;
using System.Web.Routing;
using DcmsMobile.Receiving.Models;
using EclipseLibrary.Oracle;

namespace DcmsMobile.Receiving.Repository
{
    public class ReceivingRepository : IDisposable
    {
        const string MODULE_NAME = "REC";

        private OracleDatastore _db;

        private int _queryCount;

        /// <summary>
        /// For injecting the value through unit tests
        /// </summary>
        /// <param name="db"></param>
        public ReceivingRepository(OracleDatastore db)
        {
            _db = db;
        }

        /// <summary>
        /// For use in tests
        /// </summary>
        // ReSharper disable UnusedMember.Global
        public OracleDatastore Db
        // ReSharper restore UnusedMember.Global
        {
            get
            {
                return _db;
            }
        }
        /// <summary>
        /// Constructor of class used to create the connection to database.
        /// </summary>
        /// <param name="requestContext"></param>
        public ReceivingRepository(RequestContext requestContext)
        {
            var db = new OracleDatastore(requestContext.HttpContext.Trace);
            db.CreateConnection(ConfigurationManager.ConnectionStrings["dcms8"].ConnectionString,
                requestContext.HttpContext.SkipAuthorization ? string.Empty : requestContext.HttpContext.User.Identity.Name);

            // This is a well known module code so that receving reports can reliably access receiving records from src_carton_process table.
            db.ModuleName = MODULE_NAME;
            db.ClientInfo = string.IsNullOrEmpty(requestContext.HttpContext.Request.UserHostName) ? requestContext.HttpContext.Request.UserHostAddress :
                requestContext.HttpContext.Request.UserHostName;
            _db = db;
        }

        public void Dispose()
        {
            var disp = _db as IDisposable;
            if (disp != null)
            {
                disp.Dispose();
            }
            _db = null;
        }

        /// <summary>
        /// Method will be called to accept the Carton against the Pallet passed
        /// </summary>
        /// <param name="palletId"></param>
        /// <param name="cartonId"></param>
        /// <param name="destArea"></param>
        /// <param name="processId"></param>
        public void ReceiveCarton(string palletId, string cartonId, string destArea, int? processId)
        {
            const string QUERY = @"
            BEGIN

             <proxy /> pkg_rec_2.receive_carton_2(
                                       acarton_id => :acarton_id,
                                       apallet_id => :apallet_id,
                                       APROCESS_ID  => :aprocess_id,
                                       acarton_storage_area => :acarton_storage_area);
            END;
           ";
            var binder = SqlBinder.Create()
           .Parameter("acarton_id", cartonId)
                .Parameter("apallet_id", palletId)
                .Parameter("acarton_storage_area", destArea)
                .Parameter("aprocess_id", processId);

            ++_queryCount;
            _db.ExecuteNonQuery(QUERY, binder);
        }

        /// <summary>
        /// Gives the recent process being worked on.
        /// </summary>
        /// DB: Removed the restriction that data of last 7 days will only be shown. 
        /// Now we show recent 20 rows. If existing process id is passed we show that process.
        /// <returns></returns>
        public IList<ReceivingProcess> GetProcesses(int? processId)
        {
            const string QUERY = @"
                           WITH Q1 AS
                             (SELECT MAX(SCP.PROCESS_ID) AS PROCESS_ID,
                                     MAX(SCP.OPERATOR_NAME) AS OPERATOR_NAME,
                                     MAX(SCP.PRO_NUMBER) AS PRO_NUMBER,
                                     MAX(SCP.PRO_DATE) AS PRO_DATE,
                                     MAX(SCP.PALLET_LIMIT) AS PALLET_LIMIT,
                                     MAX(SCP.CARRIER_ID) AS CARRIER_ID,
                                     MAX(SCP.PRICE_SEASON_CODE) AS PRICE_SEASON_CODE,
                                     MAX(MC.DESCRIPTION) AS DESCRIPTION,
                                     MAX(SCP.EXPECTED_CARTON) AS EXPECTED_CARTON,
                                     MAX(SCP.PROCESS_START_DATE) AS START_DATE,
                                     MAX(SCP.PROCESS_END_DATE) AS RECEIVING_END_DATE,
                                     COUNT(DISTINCT SC.CARTON_ID) +
                                     (SELECT COUNT(*)
                                        FROM <proxy />SRC_OPEN_CARTON
                                       WHERE INSHIPMENT_ID = SCP.PROCESS_ID) AS CARTON_COUNT,
                                     COUNT(DISTINCT SC.PALLET_ID) AS PALLET_COUNT,
                                     MAX(SCP.RECEIVING_AREA_ID) AS RECEIVING_AREA_ID,
                                     MAX(SCP.SPOT_CHECK_AREA_ID) AS SPOT_CHECK_AREA_ID
                                FROM <proxy />SRC_CARTON_PROCESS SCP
                                LEFT OUTER JOIN <proxy />MASTER_CARRIER MC
                                  ON SCP.CARRIER_ID = MC.CARRIER_ID
                                LEFT OUTER JOIN <proxy />TAB_INVENTORY_AREA TIA
                                  ON SCP.AREA_ID_TO_CHECK = TIA.INVENTORY_STORAGE_AREA
                                LEFT OUTER JOIN <proxy />SRC_CARTON SC
                                  ON SC.INSHIPMENT_ID = SCP.PROCESS_ID
                               WHERE SCP.MODULE_CODE = :AMODULE_CODE
                                 AND SCP.RECEIVING_AREA_ID IS NOT NULL  
                                    <if>
                                     AND SCP.PROCESS_ID = :PROCESS_ID
                                    </if>
                               GROUP BY SCP.PROCESS_ID
                               ORDER BY MAX(NVL(SCP.PROCESS_START_DATE, SCP.INSERT_DATE)) DESC NULLS LAST,
                                        SCP.PROCESS_ID)
                            SELECT * FROM Q1 WHERE ROWNUM &lt; 21";

            var binder = SqlBinder.Create(row => new ReceivingProcess()
                {
                    Carrier = new Carrier
                        {
                            CarrierId = row.GetString("CARRIER_ID"),
                            Description = row.GetString("DESCRIPTION")
                        },
                    ProcessId = row.GetInteger("PROCESS_ID").Value,
                    OperatorName = row.GetString("OPERATOR_NAME"),
                    ProNumber = row.GetString("PRO_NUMBER"),
                    ProDate = row.GetDate("PRO_DATE"),
                    PalletLimit = row.GetInteger("PALLET_LIMIT"),
                    PalletCount = row.GetInteger("PALLET_COUNT").Value,
                    CartonCount = row.GetInteger("CARTON_COUNT").Value,
                    ExpectedCartons = row.GetInteger("EXPECTED_CARTON"),
                    PriceSeasonCode = row.GetString("PRICE_SEASON_CODE"),
                    ReceivingAreaId = row.GetString("RECEIVING_AREA_ID"),
                    SpotCheckAreaId = row.GetString("SPOT_CHECK_AREA_ID"),
                    StartDate = row.GetDate("start_date"),
                    ReceivingEndDate = row.GetDate("RECEIVING_END_DATE")
                }).Parameter("AMODULE_CODE", MODULE_NAME)
                .Parameter("PROCESS_ID", processId);
            ++_queryCount;
            return _db.ExecuteReader(QUERY, binder);
        }

        /// <summary>
        /// create new process for the receiving.
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public int InsertProcess(ReceivingProcess info)
        {
            const string QUERY = @"
                         BEGIN
                            INSERT INTO <proxy />SRC_CARTON_PROCESS
                                  (PROCESS_ID,
                                   MODULE_CODE,
                                   PRO_NUMBER,
                                   CARRIER_ID,
                                   PRO_DATE,
                                   EXPECTED_CARTON,
                                   PALLET_LIMIT,
                                   RECEIVING_AREA_ID,
                                   SPOT_CHECK_AREA_ID,
                                   PRICE_SEASON_CODE )
                                VALUES
                                  (Process_Sequence.Nextval,
                                   :amodule_code,
                                   :aprono,
                                   :acarrier,
                                   :aprodate,
                                   :expectedcarton,
                                   :pallet_limit,
                                   :receivingArea,
                                   :spotCheckArea,
                                   :priceSeasonCode
                                  )
                        RETURNING PROCESS_ID INTO :process_id;
                         END;
                ";

            int processId = 0;
            var binder = SqlBinder.Create().Parameter("aprono", info.ProNumber)
                .Parameter("acarrier", info.Carrier.CarrierId)
                .Parameter("aprodate", info.ProDate)
                .Parameter("amodule_code", MODULE_NAME)
                .Parameter("expectedcarton", info.ExpectedCartons)
                .Parameter("pallet_limit", info.PalletLimit)
                .Parameter("receivingArea", info.ReceivingAreaId)
                .Parameter("spotCheckArea", info.SpotCheckAreaId)
                .Parameter("priceSeasonCode", info.PriceSeasonCode)
                .OutParameter("process_id", val => processId = val.Value);
            _db.ExecuteNonQuery(QUERY, binder);

            return processId;
        }

        /// <summary>                                                          
        /// Update the info for the passed process                             
        /// </summary>
        /// <param name="info"></param>

        public void UpdateProcess(ReceivingProcess info)
        {
            const string QUERY = @"           
                    UPDATE <proxy />SRC_CARTON_PROCESS SCP
                    SET 
                        SCP.PRO_NUMBER= :PRO_NUMBER,
                        SCP.EXPECTED_CARTON = :EXPECTED_CARTON,
                        SCP.PRO_DATE= :PRO_DATE,
                        SCP.PALLET_LIMIT = :PALLET_LIMIT,
                        SCP.CARRIER_ID= :CARRIER_ID,
                        SCP.PRICE_SEASON_CODE = :PRICE_SEASON_CODE,
                        SCP.RECEIVING_AREA_ID = :RECEIVING_AREA_ID,
                        SCP.SPOT_CHECK_AREA_ID = :SPOT_CHECK_AREA_ID
                    WHERE SCP.PROCESS_ID= :PROCESS_ID            
                    ";
            var binder = SqlBinder.Create()
           .Parameter("PRO_NUMBER", info.ProNumber)
           .Parameter("PROCESS_ID", info.ProcessId)
           .Parameter("EXPECTED_CARTON", info.ExpectedCartons)
           .Parameter("PALLET_LIMIT", info.PalletLimit)
           .Parameter("PRO_DATE", info.ProDate)
           .Parameter("CARRIER_ID", info.Carrier.CarrierId)
           .Parameter("PRICE_SEASON_CODE", info.PriceSeasonCode)
           .Parameter("RECEIVING_AREA_ID", info.ReceivingAreaId)
           .Parameter("SPOT_CHECK_AREA_ID", info.SpotCheckAreaId);
            ++_queryCount;
            _db.ExecuteNonQuery(QUERY, binder);
        }

        /// <summary>
        /// function to remove carton from pallet
        /// </summary>
        /// <param name="cartonId"></param>
        public string RemoveFromPallet(string cartonId)
        {
            string palletId = string.Empty;
            const string QUERY = @"
             DECLARE
              LPALLET <proxy />SRC_CARTON.PALLET_ID%TYPE;
            BEGIN
              SELECT SC.PALLET_ID 
                INTO LPALLET
                FROM <proxy />SRC_CARTON SC
               WHERE SC.CARTON_ID = :acarton_id;

              UPDATE <proxy />SRC_CARTON SC SET SC.PALLET_ID = NULL WHERE SC.CARTON_ID = :acarton_id;
            :pallet := LPALLET;
            END;

";
            var binder = SqlBinder.Create()
             .Parameter("acarton_id", cartonId)
                 .OutParameter("pallet", val => palletId = val);
            ++_queryCount;
            _db.ExecuteNonQuery(QUERY, binder);
            return palletId;
        }

        /// <summary>
        /// Gets printer for printing carton ticket.
        /// </summary>
        public IList<Tuple<string, string>> GetPrinters()
        {
            const string QUERY = @"
                        select
                           tabprinter.name AS name,
                           tabprinter.description AS description
                        from <proxy />tab_printer tabprinter 
                        where 1 = 1
                        AND upper(printer_type) = upper('zebra')
                        order by lower(name)";
            var binder = SqlBinder.Create(row => Tuple.Create(row.GetString("NAME"), row.GetString("DESCRIPTION")));
            ++_queryCount;
            return _db.ExecuteReader(QUERY, binder);
        }

        public void PrintCarton(string cartonId, string printer)
        {
            const string QUERY = @"
                    begin
                      <proxy />pkg_jf_src_2.pkg_jf_src_ctn_tkt(acarton_id => :acarton_id,
                                                      aprinter_name => :aprinter_name);
                    end;
                    ";
            var binder = SqlBinder.Create()
                .Parameter("acarton_id", cartonId)
                .Parameter("aprinter_name", printer);
            ++_queryCount;
            _db.ExecuteNonQuery(QUERY, binder);
        }

        /// <summary>
        /// Query new pallet sequence.
        /// </summary>
        /// <returns></returns>
        public int GetPalletSequence()
        {
            const string QUERY = @"SELECT <proxy />PALLET_SEQUENCE.NEXTVAL  AS PALLET_SEQUENCE FROM dual";

            var binder = SqlBinder.Create(row => row.GetInteger("PALLET_SEQUENCE").Value);
            ++_queryCount;
            return _db.ExecuteSingle(QUERY, binder);
        }


        /// <summary>
        /// count location for passed sku
        /// </summary>
        /// <param name="areaId"></param>
        /// <param name="skuId"></param>
        /// <returns></returns>
        public int GetAssignedLocationCount(string areaId, int skuId)
        {
            const string QUERY = @"
                SELECT COUNT(*) AS COUNT
                  FROM <proxy/>MASTER_STORAGE_LOCATION MSL
                 WHERE MSL.STORAGE_AREA = :areaId
                   AND MSL.ASSIGNED_SKU_ID = :skuId
                ";
            var binder = SqlBinder.Create(row => row.GetInteger("COUNT").Value)
                .Parameter("areaId", areaId)
                .Parameter("skuId", skuId);
            ++_queryCount;
            return _db.ExecuteSingle(QUERY, binder);
        }

        /// <summary>
        /// Get received carton count
        /// </summary>
        /// <param name="processId"></param>
        /// <returns>
        /// </returns>
        public int GetCartonsOfProcess(int? processId)
        {
            const string QUERY = @"
                                    SELECT COUNT(CARTON_ID) AS CARTON_COUNT
                      FROM <proxy />SRC_CARTON
                    WHERE INSHIPMENT_ID = :inshipmentId
                       AND PALLET_ID IS NOT NULL
                ";
            var binder = SqlBinder.Create(row => row.GetInteger("CARTON_COUNT").Value).Parameter("inshipmentId", processId);
            ++_queryCount;
            return _db.ExecuteSingle(QUERY, binder);
        }

        public IEnumerable<CartonArea> GetCartonAreas()
        {
            const string QUERY =
                                    @"
                    SELECT T.INVENTORY_STORAGE_AREA  AS INVENTORY_STORAGE_AREA,
                           T.WAREHOUSE_LOCATION_ID   AS BUILDING_ID,
                           T.SHORT_NAME              AS SHORT_NAME,
                           T.LOCATION_NUMBERING_FLAG AS IS_NUMBERED_AREA,
                           T.IS_RECEIVING_AREA AS IS_RECEIVING_AREA,
                           T.IS_spotcheck_area as IS_SPOTCHECK_AREA,
                           t.description AS description
                      FROM <proxy />TAB_INVENTORY_AREA T
                     WHERE T.STORES_WHAT = 'CTN'
                            ";

            var binder = SqlBinder.Create(row => new CartonArea()
                {
                    BuildingId = row.GetString("BUILDING_ID"),
                    ShortName = row.GetString("SHORT_NAME"),
                    AreaId = row.GetString("INVENTORY_STORAGE_AREA"),
                    Description = row.GetString("description"),
                    IsNumberedArea = row.GetString("IS_NUMBERED_AREA") == "Y",
                    IsReceivingArea = row.GetString("IS_RECEIVING_AREA") == "Y",
                    IsSpotCheckArea = row.GetString("IS_SPOTCHECK_AREA") == "Y"
                });
            ++_queryCount;
            return _db.ExecuteReader(QUERY, binder);
        }

        /// <summary>
        /// Returns the list of Cartons based on passed parameters
        /// </summary>
        /// <param name="palletId">Returns cartons on pallet</param>
        /// <param name="processId">Returns cartons of this process. We do not return cartons which exist on a pallet contain cartons of multiple areas.</param>
        /// <param name="buddyCartonId">Returns cartons which are on the same pallet as this carton</param>
        /// <returns></returns>

        public IList<ReceivedCarton> GetReceivedCartons2(string palletId, int? processId, string buddyCartonId)
        {
            const string QUERY = @"
                    WITH q1 AS
                     (SELECT 
                             CTN.PALLET_ID AS PALLET_ID,
                             CTN.CARTON_ID AS CARTON_ID,
                             CTN.INSERT_DATE AS INSERT_DATE,
                             CTN.CARTON_STORAGE_AREA AS CARTON_STORAGE_AREA,
                             CTN.VWH_ID AS VWH_ID,
                             CTNDET.SKU_ID AS SKU_ID,
                             MSKU.STYLE AS STYLE_,
                             MSKU.COLOR AS COLOR_,
                             MSKU.DIMENSION AS DIMENSION_,
                             MSKU.SKU_SIZE AS SKU_SIZE_,
                             MSKU.RETAIL_PRICE AS SKU_PRICE
                        FROM <proxy />SRC_CARTON CTN
                       LEFT OUTER JOIN <proxy />SRC_CARTON_DETAIL CTNDET
                          ON CTN.CARTON_ID = CTNDET.CARTON_ID
                       LEFT OUTER JOIN <proxy />MASTER_SKU MSKU
                          ON CTNDET.SKU_ID = MSKU.SKU_ID
                      LEFT OUTER JOIN <proxy />MASTER_STYLE M
                          ON M.STYLE = MSKU.STYLE
                       WHERE 1 = 1
                    <if>
                    AND CTN.PALLET_ID = :pallet_id
                    </if>
                    <if>
                    AND CTN.PALLET_ID IN
                             (SELECT DISTINCT S.PALLET_ID
                                FROM <proxy />SRC_CARTON S
                               WHERE S.inshipment_id = :PROCESS_ID
                                 )
                    </if>
                    <if>
                    AND CTN.carton_id = :carton_id
                    </if>
                    order by CTN.insert_date desc
                    )

                    select q1.* from q1 where 
                     rownum &lt; 500
                    ";
            Contract.Assert(_db != null);
            var binder = SqlBinder.Create(row => new ReceivedCarton()
                {
                    CartonId = row.GetString("CARTON_ID"),
                    ReceivedDate = row.GetDate("INSERT_DATE"),
                    PalletId = row.GetString("PALLET_ID"),
                    DestinationArea = row.GetString("CARTON_STORAGE_AREA"),
                    VwhId = row.GetString("VWH_ID"),
                    Sku = row.GetInteger("SKU_ID") == null ? null : new Sku
                        {
                            SkuId = row.GetInteger("SKU_ID").Value,
                            Style = row.GetString("STYLE_"),
                            Color = row.GetString("COLOR_"),
                            Dimension = row.GetString("DIMENSION_"),
                            SkuSize = row.GetString("SKU_SIZE_"),
                            SkuPrice = row.GetDecimal("SKU_PRICE")
                        }
                }).Parameter("pallet_id", palletId)
                .Parameter("PROCESS_ID", processId)
                .Parameter("carton_id", buddyCartonId);
            ++_queryCount;
            return _db.ExecuteReader(QUERY, binder);
        }

        /// <summary>
        /// Returns the information of intransit carton.
        /// </summary>
        /// <param name="scan"></param>
        /// <returns></returns>

        public IntransitCarton GetIntransitCarton2(string scan)
        {
            const string QUERY = @"
                        with q1 AS
                         (    Select   SCI.VWH_ID AS VWH_ID,
                                 MS.UPC_CODE AS UPC_CODE,
                                 MS.STYLE AS STYLE_,
                                 MS.COLOR AS COLOR_,
                                 MS.DIMENSION AS DIMENSION_,
                                 MS.SKU_SIZE AS SKU_SIZE_,
                                 SCI.CARTON_ID AS CARTON_ID,
                                 SCI.IS_SHIPMENT_CLOSED,
                                 MS.SKU_ID AS SKU_ID,
                                 mss.spotcheck_percent AS spotcheck_percent,
                                 sci.received_date AS received_date,
                                 mss.spotcheck_flag as IsSpotCheck_Enabled
                            FROM <proxy />SRC_CARTON_INTRANSIT SCI
                            LEFT OUTER JOIN <proxy />MASTER_SKU MS
                              ON MS.STYLE = SCI.STYLE
                             AND MS.COLOR = SCI.COLOR
                             AND MS.DIMENSION = SCI.DIMENSION
                             AND MS.SKU_SIZE = SCI.SKU_SIZE
                        LEFT OUTER JOIN <proxy />MASTER_STYLE M
                              ON M.STYLE = MS.STYLE
                          LEFT OUTER JOIN <proxy />MASTER_SEWINGPLANT_STYLE MSS
                            ON (SCI.STYLE = MSS.STYLE OR MSS.STYLE = '.')
                           AND (SCI.SEWING_PLANT_CODE = MSS.SEWING_PLANT_CODE OR
                               MSS.SEWING_PLANT_CODE = '.')
                           AND (SCI.COLOR = MSS.COLOR OR
                               MSS.COLOR = '.')
                           WHERE SCI.CARTON_ID = :carton_id)
                        select * from q1
                 ";

            var binder = SqlBinder.Create(row => new IntransitCarton()
                {
                    Sku = row.GetInteger("SKU_ID") == null ? null : new Sku
                    {
                        SkuId = row.GetInteger("SKU_ID").Value,
                        Style = row.GetString("STYLE_"),
                        Color = row.GetString("COLOR_"),
                        Dimension = row.GetString("DIMENSION_"),
                        SkuSize = row.GetString("SKU_SIZE_"),
                    },
                    CartonId = row.GetString("CARTON_ID"),
                    VwhId = row.GetString("VWH_ID"),
                    UpcCode = row.GetString("UPC_CODE"),
                    SpotCheckPercent = row.GetDecimal("spotcheck_percent"),
                    ReceivedDate = row.GetDate("received_date"),
                    IsShipmentClosed = row.GetString("IS_SHIPMENT_CLOSED") == "Y",
                    IsSpotCheckEnabled = row.GetString("isspotcheck_enabled") == "Y"
                }).Parameter("carton_id", scan)
                .OutRefCursorParameter("result");
            ++_queryCount;
            return _db.ExecuteSingle(QUERY, binder);
        }

        /// <summary>
        /// Get the list of PriceSeasonCode.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<CodeDescription> GetPriceSeasonCode()
        {
            const string QUERY =
                            @"SELECT TPS.PRICE_SEASON_CODE, TPS.DESCRIPTION
                              FROM <proxy />TAB_PRICE_SEASON TPS
                             ORDER BY TPS.PRICE_SEASON_CODE";
            var binder = SqlBinder.Create(row => new CodeDescription()
                {
                    Code = row.GetString("PRICE_SEASON_CODE"),
                    Description = row.GetString("DESCRIPTION")
                });
            return _db.ExecuteReader(QUERY, binder);
        }

        public int QueryCount
        {
            get { return _queryCount; }
        }

        /// <summary>
        /// Gives the area where this SKU is required.
        /// </summary>
        /// <param name="cartonId"></param>
        /// <returns></returns>
        public string GetCartonDestination(string cartonId)
        {

            var QUERY = @"
                    begin
                      :result := <proxy />PKG_REC_2.GETCARTONDESTINATION(acarton_id => :carton_id);
                    end;
                    ";
            string areaId = string.Empty;
            var binder = SqlBinder.Create().Parameter("carton_id", cartonId)
           .OutParameter("result", val => { areaId = val; });
            _db.ExecuteNonQuery(QUERY, binder);
            return areaId;
        }

        public void PutCartonOnPallet(string palletId, string cartonId)
        {
            const string QUERY =
                @"UPDATE <proxy />SRC_CARTON SC SET SC.PALLET_ID = :pallet_id WHERE SC.CARTON_ID = :carton_id";
            var binder = SqlBinder.Create()
           .Parameter("carton_id", cartonId)
           .Parameter("pallet_id", palletId);
            ++_queryCount;
            _db.ExecuteNonQuery(QUERY, binder);
        }
        /// <summary>
        /// Gets cartons of passed process which are received but not on any pallet.
        /// </summary>
        /// <param name="processId"></param>
        /// <returns>CartonList</returns>
        public IEnumerable<ReceivedCarton> GetUnpalletizedCartons(int? processId)
        {
            const string QUERY =
                            @"
                            SELECT S.CARTON_ID           AS CARTON_ID,
                                   TIA.SHORT_NAME        AS SHORT_NAME,
                                   S.VWH_ID              AS VWH_ID
                          FROM <proxy/>SRC_CARTON S
                          LEFT OUTER JOIN <proxy/>TAB_INVENTORY_AREA TIA
                            ON TIA.INVENTORY_STORAGE_AREA = S.CARTON_STORAGE_AREA
                             WHERE S.INSHIPMENT_ID = :INSHIPMENT_ID
                               AND S.PALLET_ID IS NULL";
            var binder = SqlBinder.Create(row => new ReceivedCarton()
                {
                    CartonId = row.GetString("CARTON_ID"),
                    DestinationArea = row.GetString("SHORT_NAME"),
                    VwhId = row.GetString("VWH_ID")
                }).Parameter("INSHIPMENT_ID", processId);
            return _db.ExecuteReader(QUERY, binder);
        }

        /// <summary>
        /// Get shipment list which is not close.
        /// TODO: Improve this query.  
        /// Shows list of Shipment, PO. 
        /// </summary>
        /// <returns></returns>
        public IList<ShipmentList> GetShipmentList()
        {

            const string QUERY = @"
  SELECT SCI.SHIPMENT_ID AS SHIPMENT_ID,
         SCI.SOURCE_ORDER_ID AS SOURCE_ORDER_ID,
         MAX(SCI.INTRANSIT_TYPE) AS INTRANSIT_TYPE,
         sum(NVL(CASE
               WHEN nvl(SCI.ORIGINAL_SHIPMENT_ID,sci.shipment_id) = SCI.SHIPMENT_ID AND SCI.RECEIVED_DATE IS NULL THEN
                SCI.QUANTITY
             END,
             0)) AS PIECES_NOT_RECEIVED,
         sum(NVL(CASE
               WHEN nvl(SCI.ORIGINAL_SHIPMENT_ID,sci.shipment_id) = SCI.SHIPMENT_ID AND SCI.RECEIVED_DATE IS NOT NULL THEN
                SCI.QUANTITY
             END,
             0)) AS PIECES_RECEIVED,
         COUNT(UNIQUE CASE
                 WHEN nvl(SCI.ORIGINAL_SHIPMENT_ID,sci.shipment_id) = SCI.SHIPMENT_ID AND SCI.RECEIVED_DATE IS NULL THEN
                  SCI.CARTON_ID
               END) AS CARTON_NOT_RECEIVED,
         COUNT(UNIQUE CASE
                 WHEN nvl(SCI.ORIGINAL_SHIPMENT_ID,sci.shipment_id) = SCI.SHIPMENT_ID AND SCI.RECEIVED_DATE IS NOT NULL THEN
                  SCI.CARTON_ID
               END) AS CARTON_RECEIVED,
         max(SCI.RECEIVED_DATE) AS RECEIVED_DATE,
         MAX(SCI.ERP_ID) AS ERP_TYPE,
         MAX(S.INSHIPMENT_ID) AS INSHIPMENT_ID,
         MAX(SCI.SHIPMENT_DATE) AS SHIPMENT_DATE
    FROM <proxy />SRC_CARTON_INTRANSIT SCI
    LEFT OUTER JOIN <proxy />SRC_CARTON S
      ON SCI.CARTON_ID = S.CARTON_ID
   WHERE SCI.IS_SHIPMENT_CLOSED IS NULL
     AND SCI.PARTITION_UPLOAD_DATE = TO_DATE(1, 'J')    
   GROUP BY SCI.SHIPMENT_ID, SCI.SOURCE_ORDER_ID          
    ORDER BY MAX(sci.RECEIVED_DATE) DESC NULLS LAST, MAX(s.INSHIPMENT_ID) DESC
            ";


            var binder = SqlBinder.Create(row => new ShipmentList()
            {
                ShipmentId = row.GetString("SHIPMENT_ID"),
                PoNumber = row.GetLong("SOURCE_ORDER_ID"),
                IntransitType = row.GetString("INTRANSIT_TYPE"),
                ExpectedQuantity = ((row.GetInteger("PIECES_NOT_RECEIVED") ?? 0) + (row.GetInteger("PIECES_RECEIVED") ?? 0)),
                ReceivedQuantity = row.GetInteger("PIECES_RECEIVED") ?? 0,
                ErpType = row.GetString("ERP_TYPE"),
                MaxReceiveDate = row.GetDateTimeOffset("RECEIVED_DATE"),
                CartonCount = ((row.GetInteger("CARTON_NOT_RECEIVED") ?? 0) + (row.GetInteger("CARTON_RECEIVED") ?? 0)),
                CartonReceived = row.GetInteger("CARTON_RECEIVED") ?? 0,
                ProcessNumber = row.GetLong("INSHIPMENT_ID"),
                ShipmentDate = row.GetDate("SHIPMENT_DATE").Value

            });
            return _db.ExecuteReader(QUERY, binder, 200);
        }

        /// <summary>
        /// Close passed shipment.
        /// </summary>
        /// <param name="shipmentId"></param>
        public void CloseShipment(string shipmentId, long? poId)
        {
            const string QUERY = @"
                    begin
                    -- Call the procedure
                  <proxy />pkg_rec_2.close_shipment(ashipment_id => :ashipment_id,
                                           apo_id => :apo_id);
                            end;
                ";
            var binder = SqlBinder.Create()
             .Parameter("ashipment_id", shipmentId)
             .Parameter("apo_id", poId);

            _db.ExecuteNonQuery(QUERY, binder);
        }

        /// <summary>
        /// This function will reopen the close shipment
        /// </summary>
        /// <param name="shipmentId"></param>
        /// <param name="poId"></param>
        public bool ReOpenShipment(string shipmentId, long? poId)
        {
            var rowCount = 0;                      
            const string QUERY = @"
           begin
              -- Call the function
              :result := <proxy />pkg_rec_2.reopen_shipment(ashipment_id => :ashipment_id,
                                                   apo_id => :apo_id);
            end;
           ";
            var binder = SqlBinder.Create()
         .Parameter("ashipment_id", shipmentId)
         .Parameter("apo_id", poId).OutParameter("result", val => rowCount = val ?? 0);

            _db.ExecuteNonQuery(QUERY, binder);

            return rowCount > 0;
        }

        internal bool AcceptCloseShipmentCtn(string cartonId, int processId)
        {

            int rowCount = 0;
            const string QUERY = @"
           begin
              :result := <proxy />pkg_rec_2.accept_close_shipment_carton(acarton_id => :acarton_id,
                                                    aprocess_id => :aprocess_id);
            end;
           ";
            var binder = SqlBinder.Create()
         .Parameter("acarton_id", cartonId)
         .Parameter("aprocess_id", processId).OutParameter("result", val => rowCount = val ?? 0);
            _db.ExecuteNonQuery(QUERY, binder);
            return rowCount > 0;
        }

        /// <summary>
        /// Validate carrier
        /// </summary>
        /// <param name="carrierId"></param>
        /// <returns></returns>
        public Carrier GetCarrier(string carrierId)
        {
            const string QUERY = @"
                        SELECT mc.carrier_id as carrier_id, 
                                mc.description as description
                            FROM <proxy />v_carrier mc
                        where  mc.carrier_id =:carrierId                        
                        ";
            var binder = SqlBinder.Create(row => new Carrier()
            {
                CarrierId = row.GetString("carrier_id"),
                Description = row.GetString("description")
            })
               .Parameter("carrierId", carrierId);
            return _db.ExecuteSingle(QUERY, binder);

        }
    }
}

//$Id$