using EclipseLibrary.Oracle;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Web;

namespace DcmsMobile.Inquiry.Areas.Inquiry.IntransitEntity
{
    [Flags]
    internal enum ShipmentFilters
    {
        NoFilter = 0,

        OpenShipments = 0x1,

        ClosedShipments = 0x2,

        BuildingTransferShipments = 0x4,

        VendorShipments = 0x8,

        VarianceOnlyShipments = 0x10
    }
    /// <summary>
    /// Shipment Sku filters
    /// </summary>
    [Flags]
    internal enum ShipmentSkuFilters
    {
        NoFilter = 0,

        VendorShipments = 0x1,

        BuildingTransferShipments = 0x2,

        AllSku = 0x4,

        VarianceSku = 0x4

    }



    internal class IntransityEntityRepository : IDisposable
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

        public IntransityEntityRepository(OracleDatastore db)
        {
            _db = db;
        }
        public IntransityEntityRepository(TraceContext ctx, string connectString, string userName, string clientInfo)
        {
            var db = new OracleDatastore(ctx);
            db.CreateConnection(connectString, userName);

            db.ModuleName = "Inquiry";
            db.ClientInfo = clientInfo;
            db.DefaultMaxRows = 10000;      // Allow retrieving up to 10000 rows. Number of cartons can be huge
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
        /// This function will provide all Shipments.
        /// </summary>
        /// <param name="maxCloseDate"></param>
        /// <param name="minCloseDate"></param>
        /// <param name="statusFilter">Pass the set of flags which control which filers will be applied</param>
        /// <returns></returns>
        public IList<IntransitShipment> GetInboundShipmentSummary(DateTime? minCloseDate, DateTime? maxCloseDate, ShipmentFilters statusFilter, string sewingPlant, int nRowsToShow)
        {
            Contract.Assert(_db != null);
            var QUERY = @"
                         WITH EXPECTED_SHIPMENT AS
                     (SELECT S.ORIGINAL_SHIPMENT_ID AS SHIPMENT_ID,   
                             MAX(S.IS_SHIPMENT_CLOSED) AS IS_SHIPMENT_CLOSED,                             
                             SUM(S.QUANTITY) AS EXPECTED_QUANTITY,
                             COUNT(S.CARTON_ID) AS EXPECTED_CTNS,
         MIN(CASE
               WHEN S.ORIGINAL_SHIPMENT_ID != S.SHIPMENT_ID AND
                    S.RECEIVED_DATE IS NOT NULL THEN
                S.SHIPMENT_ID
             END) AS MIN_OTHER_SHIPMENT_ID,
         MAX(CASE
               WHEN S.ORIGINAL_SHIPMENT_ID != S.SHIPMENT_ID AND
                    S.RECEIVED_DATE IS NOT NULL THEN
                S.SHIPMENT_ID
             END) AS MAX_OTHER_SHIPMENT_ID,
         COUNT(UNIQUE CASE
                 WHEN S.ORIGINAL_SHIPMENT_ID != S.SHIPMENT_ID AND
                      S.RECEIVED_DATE IS NOT NULL THEN
                  S.SHIPMENT_ID
               END) AS COUNT_OTHER_SHIPMENTS,
         COUNT(CASE
                 WHEN S.RECEIVED_DATE IS NOT NULL AND
                      S.ORIGINAL_SHIPMENT_ID != S.SHIPMENT_ID THEN
                  S.CARTON_ID
               END) AS COUNT_OTHER_RECEIVED_CTN,
         SUM(CASE
               WHEN S.RECEIVED_DATE IS NOT NULL AND
                    S.ORIGINAL_SHIPMENT_ID != S.SHIPMENT_ID THEN
                S.QUANTITY
             END) AS COUNT_OTHER_RECEIVED_PIECES,
                              SUM(case
                                   when s.received_date is null then
                                    S.QUANTITY
                                 end) AS NOT_RECEIVED_QTY,
                             COUNT(case
                                     when s.received_date is null then
                                      S.CARTON_ID
                                   end) AS NOT_RECEIVED_CTNS,
                             MAX(S.SEWING_PLANT_CODE) AS SEWING_PLANT_CODE,
                             MAX(TS.SEWING_PLANT_NAME) AS SEWING_PLANT_NAME,
                             MAX(S.SHIPMENT_DATE) AS SHIPMENT_DATE,
                             MAX(S.INTRANSIT_TYPE) AS INTRANSIT_TYPE                        
                        FROM <proxy />SRC_CARTON_INTRANSIT S
                        LEFT OUTER JOIN <proxy />TAB_SEWINGPLANT TS
                          ON TS.SEWING_PLANT_CODE = S.SEWING_PLANT_CODE
                       WHERE S.ORIGINAL_SHIPMENT_ID IS NOT NULL
                       GROUP BY S.ORIGINAL_SHIPMENT_ID),
                    ACTUAL_SHIPMENT AS
                     (SELECT S.SHIPMENT_ID,
                            min(case when s.original_shipment_id != s.shipment_id and s.received_date is not null then s.original_shipment_id end) as min_buddy_shipment_id,
                            max(case when s.original_shipment_id != s.shipment_id and s.received_date is not null then s.original_shipment_id end) as max_buddy_shipment_id,
                     count ( unique case when s.original_shipment_id != s.shipment_id and s.received_date is not null then s.original_shipment_id end) as count_buddy_shipments,
                             MAX(S.IS_SHIPMENT_CLOSED) AS IS_SHIPMENT_CLOSED,
                               SUM(case
                                   when s.received_date is not null then
                                    S.QUANTITY
                                 end) AS RECEIVED_QTY,
                             COUNT(case
                                     when s.received_date is not null then
                                      S.CARTON_ID
                                   end) AS RECEIVED_CTN,
                              COUNT(case
                                     when s.received_date is not null and
                                          s.original_shipment_id != s.shipment_id then
                                      S.CARTON_ID
                                   end) AS buddy_RECEIVED_CTN,
                              sum(case
                                     when s.received_date is not null and
                                          s.original_shipment_id != s.shipment_id then
                                      S.QUANTITY
                                   end) AS buddy_RECEIVED_pieces,
                             min(s.received_date) as min_received_date,
                             max(s.received_date) as max_received_date,
                             MAX(S.upload_date) as MAX_UPLOAD_DATE,
                             MIN(S.upload_date) as MIN_UPLOAD_DATE
                        FROM <proxy />SRC_CARTON_INTRANSIT S
                       WHERE S.ORIGINAL_SHIPMENT_ID IS NOT NULL
                       GROUP BY S.SHIPMENT_ID)
                    SELECT NVL(ES.SHIPMENT_ID,ASH.SHIPMENT_ID) as SHIPMENT_ID,
                           COUNT(*) OVER() AS TOTAL_SHIPMENTS,
                           ash.min_buddy_shipment_id,
                           ash.max_buddy_shipment_id,
                           es.MIN_OTHER_SHIPMENT_ID,
                           es.Max_OTHER_SHIPMENT_ID,
                           es.COUNT_OTHER_SHIPMENTS,
                            es.COUNT_OTHER_RECEIVED_CTN,
                            es.COUNT_OTHER_RECEIVED_PIECES,
                            ash.count_buddy_shipments,
                           ES.EXPECTED_QUANTITY,
                           ES.EXPECTED_CTNS,
                           ASH.RECEIVED_QTY,
                           ASH.RECEIVED_CTN,
                           es.NOT_RECEIVED_CTNS,
                           es.NOT_RECEIVED_QTY,
                           ash.buddy_RECEIVED_CTN,
                           ash.buddy_RECEIVED_pieces,
                           ES.SEWING_PLANT_CODE,
                           ES.SEWING_PLANT_NAME,
                           ash.IS_SHIPMENT_CLOSED,
                           ES.SHIPMENT_DATE,
                           ash.min_received_date,
                           ash.max_received_date,
                           ASH.MIN_UPLOAD_DATE,
                           ASH.MAX_UPLOAD_DATE,                           
                           ES.INTRANSIT_TYPE
                      FROM EXPECTED_SHIPMENT ES
                     full outer JOIN ACTUAL_SHIPMENT ASH
                        ON ES.SHIPMENT_ID = ASH.SHIPMENT_ID
                    where 1=1
                      <if>AND ASH.MIN_UPLOAD_DATE &gt;= :MIN_UPLOAD_DATE </if>
                      <if>AND ASH.MIN_UPLOAD_DATE &lt;= :MAX_UPLOAD_DATE</if>
                      <if>AND ES.SEWING_PLANT_CODE = :SEWING_PLANT_CODE</if>
                      <if c='$closed_filter'>and  (ash.IS_SHIPMENT_CLOSED is not NULL)</if>
                      <if c='$open_filter'>and  ash.IS_SHIPMENT_CLOSED is NULL</if>
                      <if c='$transfer_filter'> and ES.intransit_type in ('TR','ZEL')</if>
                      <if c='$vendor_filter'> and (ES.intransit_type is null or ES.intransit_type = 'IT')</if>
                      <if c= '$variance_filter'> and (ash.count_buddy_shipments &gt; 0 or ES.EXPECTED_CTNS != ASH.RECEIVED_CTN) </if>
                     order by ASH.MAX_UPLOAD_DATE desc nulls last";

            var binder = SqlBinder.Create(row => new IntransitShipment
            {
                ShipmentId = row.GetString("SHIPMENT_ID"),
                TotalShipmentCount = row.GetInteger("TOTAL_SHIPMENTS"),
                MinBuddyShipmentId = row.GetString("min_buddy_shipment_id"),
                MaxBuddyShipmentId = row.GetString("max_buddy_shipment_id"),
                MinOtherShipmentId = row.GetString("MIN_OTHER_SHIPMENT_ID"),
                MaxOtherShipmentId = row.GetString("MAX_OTHER_SHIPMENT_ID"),
                CountBuddyShipmentId = row.GetInteger("count_buddy_shipments"),
                CountOtherShipmentId = row.GetInteger("COUNT_OTHER_SHIPMENTS"),
                CountOtherReceivedCarton = row.GetInteger("COUNT_OTHER_RECEIVED_CTN"),
                CountOtherReceivedPieces = row.GetInteger("COUNT_OTHER_RECEIVED_PIECES"),
                SewingPlantCode = row.GetString("SEWING_PLANT_CODE"),
                SewingPlantName = row.GetString("SEWING_PLANT_NAME"),
                ShipmentDate = row.GetDate("SHIPMENT_DATE"),
                MinUploadDate = row.GetDate("MIN_UPLOAD_DATE"),
                MaxUploadDate = row.GetDate("MAX_UPLOAD_DATE"),
                MinReceiveDate = row.GetDate("MIN_RECEIVED_DATE"),
                MaxReceiveDate = row.GetDate("MAX_RECEIVED_DATE"),
                ExpectedPieces = row.GetInteger("EXPECTED_QUANTITY"),
                ExpectedCartonCount = row.GetInteger("EXPECTED_CTNS"),
                ReceivedPieces = row.GetInteger("RECEIVED_QTY"),
                ReceivedCartonCount = row.GetInteger("RECEIVED_CTN"),
                UnReceivedCartonCount = row.GetInteger("NOT_RECEIVED_CTNS"),
                UnReceivedPieces = row.GetInteger("NOT_RECEIVED_QTY"),
                BuddyCartonCount = row.GetInteger("BUDDY_RECEIVED_CTN"),
                BuddyReceivedPieces = row.GetInteger("BUDDY_RECEIVED_PIECES"),
                IntransitType = row.GetString("INTRANSIT_TYPE"),
                IsShipmentClosed = row.GetString("IS_SHIPMENT_CLOSED") == "Y" ? true : false
            })
            .Parameter("MIN_UPLOAD_DATE", minCloseDate)
            .Parameter("MAX_UPLOAD_DATE", maxCloseDate)
            .Parameter("SEWING_PLANT_CODE", sewingPlant);
            binder.ParameterXPath("closed_filter", statusFilter.HasFlag(ShipmentFilters.ClosedShipments));
            binder.ParameterXPath("open_filter", statusFilter.HasFlag(ShipmentFilters.OpenShipments));
            binder.ParameterXPath("transfer_filter", statusFilter.HasFlag(ShipmentFilters.BuildingTransferShipments));
            binder.ParameterXPath("vendor_filter", statusFilter.HasFlag(ShipmentFilters.VendorShipments));
            binder.ParameterXPath("variance_filter", statusFilter.HasFlag(ShipmentFilters.VarianceOnlyShipments));
            return _db.ExecuteReader(QUERY, binder, nRowsToShow);
        }

        /// <summary>
        /// Brings list of shipments per SKU.
        /// </summary>
        /// <returns></returns>
        public IList<IntransitShipmentSkuSummary> GetInboundShipmentSkuDetail(ShipmentSkuFilters statusFilter, DateTime? minCloseDate, DateTime? maxCloseDate, string sewingPlant, int nRowsToShow)
        {
            Contract.Assert(_db != null);
            const string QUERY = @"
WITH CLOSED_SHIPMENTS AS
 (SELECT S.SHIPMENT_ID,
         S.STYLE,
         S.COLOR,
         S.DIMENSION,
         S.SKU_SIZE,
         S.VWH_ID AS VWH_ID,
         max(s.is_shipment_closed) AS  IS_SHIPMENT_CLOSED,
           MAX(s.sewing_plant_code) AS sewing_plant_code,
         MAX(TS.SEWING_PLANT_NAME) AS SEWING_PLANT_NAME,
         MAX(S.UPLOAD_DATE) AS MAX_UPLOAD_DATE,
         MIN(S.UPLOAD_DATE) AS MIN_UPLOAD_DATE,
         MIN(CASE
               WHEN S.ORIGINAL_SHIPMENT_ID != S.SHIPMENT_ID AND
                    S.RECEIVED_DATE IS NOT NULL THEN
                S.ORIGINAL_SHIPMENT_ID
             END) AS MIN_BUDDY_SHIPMENT_ID,
         MAX(CASE
               WHEN S.ORIGINAL_SHIPMENT_ID != S.SHIPMENT_ID AND
                    S.RECEIVED_DATE IS NOT NULL THEN
                S.ORIGINAL_SHIPMENT_ID
             END) AS MAX_BUDDY_SHIPMENT_ID,
         COUNT(UNIQUE CASE
                 WHEN S.ORIGINAL_SHIPMENT_ID != S.SHIPMENT_ID AND
                      S.RECEIVED_DATE IS NOT NULL THEN
                  S.ORIGINAL_SHIPMENT_ID
               END) AS COUNT_BUDDY_SHIPMENTS,
         SUM(CASE
               WHEN S.RECEIVED_DATE IS NOT NULL AND
                    S.ORIGINAL_SHIPMENT_ID = S.SHIPMENT_ID THEN
                S.QUANTITY
             END) AS RECEIVED_QUANTITY_MINE,
         COUNT(CASE
                 WHEN S.RECEIVED_DATE IS NOT NULL AND
                      S.ORIGINAL_SHIPMENT_ID = S.SHIPMENT_ID THEN
                  S.CARTON_ID
               END) AS RECEIVED_CTN_MINE,
         
         SUM(CASE
               WHEN S.RECEIVED_DATE IS NOT NULL AND
                    S.ORIGINAL_SHIPMENT_ID != S.SHIPMENT_ID THEN
                S.QUANTITY
             END) AS RECEIVED_QTY_OF_BUDDIES,
         COUNT(CASE
                 WHEN S.RECEIVED_DATE IS NOT NULL AND
                      S.ORIGINAL_SHIPMENT_ID != S.SHIPMENT_ID THEN
                  S.CARTON_ID
               END) AS RECEIVED_CTN_OF_BUDDIES,
         MAX(S.SHIPMENT_DATE) AS SHIPMENT_DATE
        
    FROM <proxy />SRC_CARTON_INTRANSIT S
    LEFT OUTER JOIN <proxy />TAB_SEWINGPLANT TS
    ON TS.SEWING_PLANT_CODE = S.SEWING_PLANT_CODE
   WHERE S.ORIGINAL_SHIPMENT_ID IS NOT NULL
    
   GROUP BY S.SHIPMENT_ID,
            S.STYLE,
            S.COLOR,
            S.DIMENSION,
            S.SKU_SIZE,
            S.VWH_ID),
EXPECTED_SHIPMENTS AS
 (SELECT S.ORIGINAL_SHIPMENT_ID AS SHIPMENT_ID,
         S.STYLE,
         S.COLOR,
         S.DIMENSION,
         S.SKU_SIZE,
         S.VWH_ID AS VWH_ID,
  max(S.Is_Shipment_Closed) AS Is_Shipment_Closed,
 MAX(S.INTRANSIT_TYPE) AS INTRANSIT_TYPE,
         MIN(CASE
               WHEN S.ORIGINAL_SHIPMENT_ID != S.SHIPMENT_ID AND
                    S.RECEIVED_DATE IS NOT NULL THEN
                S.SHIPMENT_ID
             END) AS MIN_OTHER_SHIPMENT_ID,
         MAX(CASE
               WHEN S.ORIGINAL_SHIPMENT_ID != S.SHIPMENT_ID AND
                    S.RECEIVED_DATE IS NOT NULL THEN
                S.SHIPMENT_ID
             END) AS MAX_OTHER_SHIPMENT_ID,
         COUNT(UNIQUE CASE
                 WHEN S.ORIGINAL_SHIPMENT_ID != S.SHIPMENT_ID AND
                      S.RECEIVED_DATE IS NOT NULL THEN
                  S.SHIPMENT_ID
               END) AS COUNT_OTHER_SHIPMENTS,
         SUM(S.QUANTITY) AS EXPECTED_QUANTITY,
         COUNT(S.CARTON_ID) AS EXPECTED_CTNS,
         COUNT(CASE
                 WHEN S.RECEIVED_DATE IS NOT NULL AND
                      S.ORIGINAL_SHIPMENT_ID != S.SHIPMENT_ID THEN
                  S.CARTON_ID
               END) AS RECEIVED_CTN_BY_BUDDIES,
         SUM(CASE
               WHEN S.RECEIVED_DATE IS NOT NULL AND
                    S.ORIGINAL_SHIPMENT_ID != S.SHIPMENT_ID THEN
                S.QUANTITY
             END) AS RECEIVED_QTY_BY_BUDDIES
    FROM <proxy /> SRC_CARTON_INTRANSIT S
   WHERE S.ORIGINAL_SHIPMENT_ID IS NOT NULL
   GROUP BY S.ORIGINAL_SHIPMENT_ID,
            S.STYLE,
            S.COLOR,
            S.DIMENSION,
            S.SKU_SIZE,
            S.VWH_ID)

SELECT NVL(ASH.SHIPMENT_ID, ES.SHIPMENT_ID) AS SHIPMENT_ID,
       ES.MIN_OTHER_SHIPMENT_ID AS MIN_OTHER_SHIPMENT_ID,
       ES.MAX_OTHER_SHIPMENT_ID AS MAX_OTHER_SHIPMENT_ID,
       --COUNT(UNIQUE NVL(ASH.SHIPMENT_ID, ES.SHIPMENT_ID)) OVER() AS total_shipment_count,
       ES.COUNT_OTHER_SHIPMENTS,
       ASH.MIN_BUDDY_SHIPMENT_ID AS MIN_BUDDY_SHIPMENT_ID,
       ASH.MAX_BUDDY_SHIPMENT_ID AS MAX_BUDDY_SHIPMENT_ID,
       ASH.COUNT_BUDDY_SHIPMENTS,
      ash.sewing_plant_code,
      ash.SEWING_PLANT_NAME,
       NVL(ASH.STYLE, ES.STYLE) AS STYLE,
       NVL(ASH.COLOR, ES.COLOR) AS COLOR,
       NVL(ASH.DIMENSION, ES.DIMENSION) AS DIMENSION,
       NVL(ASH.SKU_SIZE, ES.SKU_SIZE) AS SKU_SIZE,
       NVL(ASH.VWH_ID, ES.VWH_ID) AS VWH_ID,
       ES.EXPECTED_QUANTITY AS EXPECTED_QUANTITY,
       ES.EXPECTED_CTNS AS EXPECTED_CTNS,
       ASH.RECEIVED_QUANTITY_MINE AS RECEIVED_QUANTITY_MINE,
       ASH.RECEIVED_CTN_MINE,
       ES.RECEIVED_CTN_BY_BUDDIES,
       ASH.RECEIVED_CTN_OF_BUDDIES,
       ES.RECEIVED_QTY_BY_BUDDIES,
       ASH.RECEIVED_QTY_OF_BUDDIES,
       ASH.SHIPMENT_DATE AS SHIPMENT_DATE,
       ASH.MAX_UPLOAD_DATE AS MAX_UPLOAD_DATE,
       ES.INTRANSIT_TYPE AS INTRANSIT_TYPE
  FROM CLOSED_SHIPMENTS ASH
  FULL OUTER JOIN EXPECTED_SHIPMENTS ES
    ON ES.SHIPMENT_ID = ASH.SHIPMENT_ID
   AND ES.STYLE = ASH.STYLE
   AND ES.COLOR = ASH.COLOR
   AND ES.DIMENSION = ASH.DIMENSION
   AND ES.SKU_SIZE = ASH.SKU_SIZE
   AND ES.VWH_ID = ASH.VWH_ID
where (ash.IS_SHIPMENT_CLOSED IS NOT NULL)
   <if>AND ASH.MIN_UPLOAD_DATE &gt;= :MIN_UPLOAD_DATE </if>
   <if>AND ASH.MIN_UPLOAD_DATE &lt;= :MAX_UPLOAD_DATE</if>
   <if>AND ASH.SEWING_PLANT_CODE = :SEWING_PLANT_CODE</if>
   <if c='$transfer_filter'> and ES.intransit_type in ('TR','ZEL')</if>
   <if c='$vendor_filter'> and (ES.intransit_type is null or ES.intransit_type = 'IT')</if>                    
   <if c= '$variance_filter'> and (ash.count_buddy_shipments &gt; 0 or count_other_shipments &gt; 0  or (es.EXPECTED_CTNS - ash.RECEIVED_CTN_MINE &gt; 0 )) </if>
 order by ASH.max_upload_date desc nulls last";

            var binder = SqlBinder.Create(row => new IntransitShipmentSkuSummary
            {
                ShipmentId = row.GetString("SHIPMENT_ID"),
                MinOtherShipmentId = row.GetString("min_other_shipment_id"),
                MaxOtherShipmentId = row.GetString("max_other_shipment_id"),
               // TotalShipmentCount = row.GetInteger("total_shipment_count"),
                CountOtherShipments = row.GetInteger("count_other_shipments"),
                MinBuddyShipmentId = row.GetString("min_buddy_shipment_id"),
                MaxBuddyShipmentId = row.GetString("max_buddy_shipment_id"),
                CountBuddyShipments = row.GetInteger("count_buddy_shipments"),
                ShipmentDate = row.GetDate("SHIPMENT_DATE"),
                SewingPlantCode = row.GetString("sewing_plant_code"),
                SewingPlantName = row.GetString("sewing_plant_name"),
                UploadDate = row.GetDate("MAX_UPLOAD_DATE"),
                Style = row.GetString("STYLE"),
                Color = row.GetString("COLOR"),
                Dimension = row.GetString("DIMENSION"),
                SkuSize = row.GetString("SKU_SIZE"),
                VwhId = row.GetString("VWH_ID"),
                ExpectedPieces = row.GetInteger("EXPECTED_QUANTITY"),
                ExpectedCartonCount = row.GetInteger("EXPECTED_CTNS"),
                ReceivedPiecesMine = row.GetInteger("RECEIVED_QUANTITY_MINE"),
                ReceivedCartonsMine = row.GetInteger("RECEIVED_CTN_MINE"),
                IntransitType = row.GetString("INTRANSIT_TYPE"),
                ReceivedCtnByBuddies = row.GetInteger("RECEIVED_CTN_BY_BUDDIES"),
                ReceivedCtnOfBuddies = row.GetInteger("RECEIVED_CTN_OF_BUDDIES"),
                ReceivedPiecesByBuddies = row.GetInteger("RECEIVED_QTY_BY_BUDDIES"),
                ReceivedPiecesOfBuddies = row.GetInteger("RECEIVED_QTY_OF_BUDDIES")

            })
            .Parameter("MIN_UPLOAD_DATE", minCloseDate)
            .Parameter("MAX_UPLOAD_DATE", maxCloseDate)
            .Parameter("SEWING_PLANT_CODE", sewingPlant);
            binder.ParameterXPath("transfer_filter", statusFilter.HasFlag(ShipmentSkuFilters.BuildingTransferShipments));
            binder.ParameterXPath("vendor_filter", statusFilter.HasFlag(ShipmentSkuFilters.VendorShipments));
            binder.ParameterXPath("variance_filter", statusFilter.HasFlag(ShipmentSkuFilters.VarianceSku));
            return _db.ExecuteReader(QUERY, binder, nRowsToShow);
        }

        internal IList<SewingPlant> GetSewingPlantList()
        {
            Contract.Assert(_db != null);

            const string QUERY = @"
                                    SELECT SCT.SEWING_PLANT_CODE AS SEWING_PLANT_CODE,
                                           MAX(SP.SEWING_PLANT_NAME) AS SEWING_PLANT_NAME
                                              FROM <proxy />SRC_CARTON_INTRANSIT SCT 
                                             LEFT OUTER JOIN <proxy />TAB_SEWINGPLANT SP
                                                ON SCT.SEWING_PLANT_CODE = SP.SEWING_PLANT_CODE
                                   
                                                    WHERE SCT.ORIGINAL_SHIPMENT_ID IS NOT NULL
                                                     GROUP BY SCT.SEWING_PLANT_CODE
                                                        
ORDER BY SCT.SEWING_PLANT_CODE
                                    ";

            var binder = SqlBinder.Create(row => new SewingPlant()
            {
                SewingPlantCode = row.GetString("SEWING_PLANT_CODE"),
                PlantName = row.GetString("SEWING_PLANT_NAME")
            });
            return _db.ExecuteReader(QUERY, binder);
        }

        public IList<IntransitShipmentSku> GetInboundShipmentInfo(string shipmentId)
        {
            Contract.Assert(_db != null);
            const string QUERY = @"
                   WITH EXPECTED_SHIPMENT AS
 (SELECT S.ORIGINAL_SHIPMENT_ID AS SHIPMENT_ID,
         S.STYLE,
         S.COLOR,
         S.DIMENSION,
         S.SKU_SIZE,
         S.VWH_ID AS VWH_ID,
         MAX(S.ERP_ID) AS ERP_ID,
         MAX(S.INSERT_DATE) AS CREATED_ON,
         SUM(S.QUANTITY) AS EXPECTED_QUANTITY,
         SUM(CASE
               WHEN S.RECEIVED_DATE IS NULL OR
                    S.SHIPMENT_ID != S.ORIGINAL_SHIPMENT_ID THEN
                S.QUANTITY
             END) AS UNDER_RECEIVED_QTY,
         SUM(CASE
             WHEN S.RECEIVED_DATE IS NOT NULL AND
                  S.SHIPMENT_ID != S.ORIGINAL_SHIPMENT_ID THEN
              S.QUANTITY
           END) AS QTY_REC_IN_OTHER_SHIPMENT,
         COUNT(S.CARTON_ID) AS EXPECTED_CTNS,
         COUNT(CASE
                 WHEN S.RECEIVED_DATE IS NULL OR
                      S.ORIGINAL_SHIPMENT_ID != S.SHIPMENT_ID THEN
                  S.CARTON_ID
               END) AS UNDER_RECEIVED_CTN,
         COUNT(CASE
               WHEN S.RECEIVED_DATE IS NOT NULL AND
                    S.ORIGINAL_SHIPMENT_ID != S.SHIPMENT_ID THEN
                S.CARTON_ID
             END) AS CTN_REC_IN_OTHER_SHIPMENT,
         MAX(S.SEWING_PLANT_CODE) AS SEWING_PLANT_CODE,
         MAX(TS.SEWING_PLANT_NAME) AS SEWING_PLANT_NAME,
         MAX(S.SHIPMENT_DATE) AS SHIPMENT_DATE,
         MAX(S.INTRANSIT_ID) AS INTRANSIT_ID,
         MAX(S.INTRANSIT_TYPE) AS INTRANSIT_TYPE,
         MIN(CASE WHEN S.ORIGINAL_SHIPMENT_ID != S.SHIPMENT_ID THEN S.SHIPMENT_ID END) AS MIN_MERGED_TO_BUDDY_SHIPMENT,
         MAX(CASE WHEN S.ORIGINAL_SHIPMENT_ID != S.SHIPMENT_ID THEN S.SHIPMENT_ID END) AS MAX_MERGED_TO_BUDDY_SHIPMENT,
         COUNT ( UNIQUE CASE WHEN S.ORIGINAL_SHIPMENT_ID != S.SHIPMENT_ID THEN S.SHIPMENT_ID END) AS COUNT_MERGED_TO_BUDDY
    FROM <proxy />SRC_CARTON_INTRANSIT S
    LEFT OUTER JOIN <proxy />TAB_SEWINGPLANT TS
      ON TS.SEWING_PLANT_CODE = S.SEWING_PLANT_CODE
   WHERE S.ORIGINAL_SHIPMENT_ID IS NOT NULL
   
   GROUP BY S.ORIGINAL_SHIPMENT_ID,
            S.STYLE,
            S.COLOR,
            S.DIMENSION,
            S.SKU_SIZE,
            S.VWH_ID),
ACTUAL_SHIPMENT AS
 (SELECT S.SHIPMENT_ID,
         S.STYLE,
         S.COLOR,
         S.DIMENSION,
         S.SKU_SIZE,
         S.VWH_ID AS VWH_ID,
         MAX(S.ERP_ID) AS ERP_ID,
         MAX(S.IS_SHIPMENT_CLOSED) AS IS_SHIPMENT_CLOSED,
         SUM(CASE
               WHEN S.RECEIVED_DATE IS NOT NULL THEN
                S.QUANTITY
             END) AS RECEIVED_QTY,
         COUNT(CASE
                 WHEN S.RECEIVED_DATE IS NOT NULL THEN
                  S.CARTON_ID
               END) AS RECEIVED_CTN,
         COUNT(CASE
                 WHEN S.RECEIVED_DATE IS NOT NULL AND
                      S.ORIGINAL_SHIPMENT_ID != S.SHIPMENT_ID THEN
                  S.CARTON_ID
               END) AS OVER_RECEIVED_CTN,
        SUM(CASE
               WHEN S.RECEIVED_DATE IS NOT NULL AND
                    S.ORIGINAL_SHIPMENT_ID != S.SHIPMENT_ID THEN
                S.QUANTITY
             END) AS OVER_RECEIVED_PIECES,
         MIN(S.RECEIVED_DATE) AS MIN_RECEIVED_DATE,
         MAX(S.RECEIVED_DATE) AS MAX_RECEIVED_DATE,
       MIN(CASE WHEN S.ORIGINAL_SHIPMENT_ID != S.SHIPMENT_ID THEN S.ORIGINAL_SHIPMENT_ID END) AS MIN_MERGED_IN_BUDDY_SHIPMENT,
       MAX(CASE WHEN S.ORIGINAL_SHIPMENT_ID != S.SHIPMENT_ID THEN S.ORIGINAL_SHIPMENT_ID END) AS MAX_MERGED_IN_BUDDY_SHIPMENT,
       COUNT ( UNIQUE CASE WHEN S.ORIGINAL_SHIPMENT_ID != S.SHIPMENT_ID THEN S.ORIGINAL_SHIPMENT_ID END) AS COUNT_MERGED_IN_BUDDY
    FROM <proxy />SRC_CARTON_INTRANSIT S
where S.ORIGINAL_SHIPMENT_ID IS NOT NULL
  
   GROUP BY S.SHIPMENT_ID,
            S.STYLE,
            S.COLOR,
            S.DIMENSION,
            S.SKU_SIZE,
            S.VWH_ID)
SELECT --COALESCE(ES.SHIPMENT_ID, ASH.SHIPMENT_ID) as SHIPMENT_ID,
 ES.MIN_MERGED_TO_BUDDY_SHIPMENT as MIN_MERGED_TO_BUDDY_SHIPMENT,
 ES.MAX_MERGED_TO_BUDDY_SHIPMENT as MAX_MERGED_TO_BUDDY_SHIPMENT,
 ES.COUNT_MERGED_TO_BUDDY as COUNT_MERGED_TO_BUDDY,
 ASH.MIN_MERGED_IN_BUDDY_SHIPMENT as MIN_MERGED_IN_BUDDY_SHIPMENT,
 ASH.MAX_MERGED_IN_BUDDY_SHIPMENT as MAX_MERGED_IN_BUDDY_SHIPMENT,
 ASH.COUNT_MERGED_IN_BUDDY as COUNT_MERGED_IN_BUDDY,
 COALESCE(ES.STYLE, ASH.STYLE) AS STYLE,
 COALESCE(ES.COLOR, ASH.COLOR) AS COLOR,
 COALESCE(ES.DIMENSION, ASH.DIMENSION) AS DIMENSION,
 COALESCE(ES.SKU_SIZE, ASH.SKU_SIZE) AS SKU_SIZE,
 COALESCE(ES.VWH_ID, ASH.VWH_ID) AS VWH_ID,
 ES.EXPECTED_QUANTITY AS EXPECTED_QUANTITY,
 ES.EXPECTED_CTNS AS EXPECTED_CTNS,
 ASH.RECEIVED_QTY AS RECEIVED_QTY,
 ASH.RECEIVED_CTN AS RECEIVED_CTN,
 ES.UNDER_RECEIVED_QTY AS UNDER_RECEIVED_QTY,
 ES.UNDER_RECEIVED_CTN as COUNT_UNRECEVIED_CARTON,
 ASH.OVER_RECEIVED_PIECES AS OVER_RECEIVED_QTY,
 ASH.OVER_RECEIVED_CTN AS COUNT_OVERRECEVIED_CARTON,
 ES.QTY_REC_IN_OTHER_SHIPMENT AS QTY_REC_IN_OTHER_SHIPMENT,
 ES.CTN_REC_IN_OTHER_SHIPMENT AS CTN_REC_IN_OTHER_SHIPMENT,
 ES.SEWING_PLANT_CODE AS SEWING_PLANT_CODE,
 ES.SEWING_PLANT_NAME AS SEWING_PLANT_NAME,
 ASH.IS_SHIPMENT_CLOSED AS IS_SHIPMENT_CLOSED,
 ES.SHIPMENT_DATE AS SHIPMENT_DATE,
 ASH.MIN_RECEIVED_DATE AS MIN_RECEIVED_DATE,
 ASH.MAX_RECEIVED_DATE AS MAX_RECEIVED_DATE,
 ES.INTRANSIT_ID AS INTRANSIT_ID,
 ES.INTRANSIT_TYPE AS INTRANSIT_TYPE,
 ES.CREATED_ON AS CREATED_ON,
 COALESCE(ES.ERP_ID, ASH.ERP_ID) AS ERP_ID
  FROM EXPECTED_SHIPMENT ES
  FULL OUTER JOIN ACTUAL_SHIPMENT ASH
    ON es.shipment_id = ash.shipment_id 
    AND ES.STYLE = ASH.STYLE
   AND ES.COLOR = ASH.COLOR
   AND ES.DIMENSION = ASH.DIMENSION
   AND ES.SKU_SIZE = ASH.SKU_SIZE
  and es.vwh_id=ash.vwh_id
where NVL(es.shipment_id,ash.shipment_id) = :SHIPMENT_ID

            ";

            var binder = SqlBinder.Create(row => new IntransitShipmentSku
            {

                SewingPlantCode = row.GetString("SEWING_PLANT_CODE"),
                SewingPlantName = row.GetString("SEWING_PLANT_NAME"),
                ShipmentDate = row.GetDate("SHIPMENT_DATE"),
                CreatedOn = row.GetDate("CREATED_ON"),
                MinReceiveDate = row.GetDate("MIN_RECEIVED_DATE"),
                MaxReceiveDate = row.GetDate("MAX_RECEIVED_DATE"),
                IntransitId = row.GetInteger("INTRANSIT_ID"),
                Style = row.GetString("STYLE"),
                Color = row.GetString("COLOR"),
                Dimension = row.GetString("DIMENSION"),
                SkuSize = row.GetString("SKU_SIZE"),
                Vwh = row.GetString("VWH_ID"),
                ExpectedPieces = row.GetInteger("EXPECTED_QUANTITY"),
                ExpectedCartonCount = row.GetInteger("EXPECTED_CTNS"),
                ReceivedPieces = row.GetInteger("RECEIVED_QTY"),
                ReceivedCartonCount = row.GetInteger("RECEIVED_CTN"),
                IsShipmentClosed = row.GetString("IS_SHIPMENT_CLOSED"),
                IntransitType = row.GetString("INTRANSIT_TYPE"),
                ErpId = row.GetString("ERP_ID"),
                UnderReceviedCartonCount = row.GetInteger("COUNT_UNRECEVIED_CARTON"),
                OverReceviedCartonCount = row.GetInteger("COUNT_OVERRECEVIED_CARTON"),
                UnderReceviedPieces = row.GetInteger("UNDER_RECEIVED_QTY"),
                OverReceviedPieces = row.GetInteger("OVER_RECEIVED_QTY"),
                MinMergedToBuddyShipment = row.GetString("MIN_MERGED_TO_BUDDY_SHIPMENT"),
                MaxMergedToBuddyShipment = row.GetString("MAX_MERGED_TO_BUDDY_SHIPMENT"),
                MinMergedInBuddyShipment = row.GetString("MIN_MERGED_IN_BUDDY_SHIPMENT"),
                MaxMergedInBuddyShipment = row.GetString("MAX_MERGED_IN_BUDDY_SHIPMENT"),
                CountMergedToBuddyShipment = row.GetInteger("COUNT_MERGED_TO_BUDDY") ?? 0,
                CountMergedInBuddyShipment = row.GetInteger("COUNT_MERGED_IN_BUDDY") ?? 0,
                PcsReceivedInOtherShipment = row.GetInteger("QTY_REC_IN_OTHER_SHIPMENT"),
                CtnsReceivedInOtherShipment = row.GetInteger("CTN_REC_IN_OTHER_SHIPMENT")

            }).Parameter("SHIPMENT_ID", shipmentId);

            return _db.ExecuteReader(QUERY, binder);
        }


    }
}