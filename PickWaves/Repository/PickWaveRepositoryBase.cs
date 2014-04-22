using DcmsMobile.PickWaves.Helpers;
using EclipseLibrary.Oracle;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Web;

namespace DcmsMobile.PickWaves.Repository
{
    public abstract class PickWaveRepositoryBase : IDisposable
    {
        #region Intialization

        protected OracleDatastore _db;

        //protected const string MODULE_CODE = "PickWaveManager";

        protected PickWaveRepositoryBase(TraceContext ctx, string userName, string clientInfo)
        {
            var db = new OracleDatastore(ctx);
            var connectString = ConfigurationManager.ConnectionStrings["dcms8"].ConnectionString;
            db.CreateConnection(connectString, userName);

            db.ModuleName = "PickWaveManager";
            db.ClientInfo = clientInfo;
            _db = db;
        }

        public void Dispose()
        {
            _db.Dispose();
        }

        #endregion

        public DbTransaction BeginTransaction()
        {
            return _db.BeginTransaction();
        }

        /// <summary>
        /// Returns information about a specific customer.
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        public Customer GetCustomer(string customerId)
        {
            const string QUERY = @"
                                    SELECT CUST.CUSTOMER_ID AS CUSTOMER_ID,
                                           CUST.NAME AS NAME,
                                           CUST.INACTIVE_FLAG as INACTIVE_FLAG
                                     FROM <proxy />MASTER_CUSTOMER CUST
                                    WHERE CUST.CUSTOMER_ID = :SEARCH
                                    ";
            var binder = SqlBinder.Create(row => new Customer
            {
                CustomerId = row.GetString("CUSTOMER_ID"),
                Name = row.GetString("NAME"),
                IsActive = row.GetString("INACTIVE_FLAG") != "Y"
            });
            binder.Parameter("SEARCH", customerId);
            return _db.ExecuteSingle(QUERY, binder);
        }

        /// <summary>
        ///  This method gets Buckets information.
        /// </summary>
        /// <param name="bucketId">To get information about passed bucket.If BucketId is passed, every other parameter is ignored</param>
        /// <param name="customerId">This is optional parameter, need to see only customer specific buckets</param>
        /// <param name="state">Passed to get bucket with specific status.</param>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// 30 Mar 2012 Sharad: All buckets of the passed status are displayed. Earlier we were showing only those buckets for which
        /// boxes needed to be created.
        /// </para>
        /// <para>
        /// 30 Aug 2012 Sharad: As a defensive check, NULL pick modes are treated as PITCHING
        /// </para>
        /// <para>
        /// 25 Feb 2013: If expected pieces are null, we presume that they are same as current pieces. This avoids the anomaly of the box containing more than expected pieces.
        /// </para>
        /// <para>
        /// 14 Mar 2013: Pick area is returned as NULL for ADRE and ADREPPWSS buckets
        /// </para>
        /// </remarks>
        public IEnumerable<Bucket> GetBuckets(int? bucketId, string customerId, ProgressStage? state)
        {
            if (string.IsNullOrWhiteSpace(customerId) && bucketId == null)
            {
                throw new ArgumentException("All parameters cannot be null to avoid retrieving too many buckets");
            }
            string bucketState = null;
            switch (state)
            {
                case ProgressStage.Frozen:
                    bucketState = "Frozen";
                    break;
                case ProgressStage.InProgress:
                    bucketState = "InProgress";
                    break;
                case ProgressStage.Completed:
                    bucketState = "Completed";
                    break;
                case ProgressStage.Unknown:
                    throw new ArgumentOutOfRangeException("state");
            }
            var QUERY = @"
           WITH TOTAL_ORDERED_PIECES AS
                    (
                    SELECT BKT.BUCKET_ID                            AS BUCKET_ID,
                           MIN(BKT.NAME)                            AS NAME,
                           MAX(BKT.DATE_CREATED)                    AS DATE_CREATED,
                           MAX(BKT.CREATED_BY)                      AS CREATED_BY,
                           MAX(BKT.PITCH_IA_ID)                     AS PITCH_IA_ID,
                           MAX(BKT.PITCH_LIMIT)                     AS PITCH_LIMIT,
                           MAX(IA.SHORT_NAME)                       AS PITCH_AREA_SHORT_NAME,
                           MAX(IA.SHORT_DESCRIPTION)                AS PITCH_AREA_DESCRIPTION,
                           MAX(IA.WAREHOUSE_LOCATION_ID)            AS BUILDING_PITCH_FROM,
                           SUM(NVL(PS.TOTAL_QUANTITY_ORDERED,0))    AS ORDERED_PIECES,
                           MIN(PO.DC_CANCEL_DATE)                   AS MIN_DC_CANCEL_DATE,
                           MAX(PO.DC_CANCEL_DATE)                   AS MAX_DC_CANCEL_DATE,
                           COUNT(DISTINCT PS.PO_ID)                 AS PO_COUNT,
                           MIN(PS.PO_ID)                            AS MIN_PO,
                           MAX(PS.PO_ID)                            AS MAX_PO,
                           MAX(PS.CUSTOMER_ID)                      AS CUSTOMER_ID,
                           MAX(CUST.NAME)                           AS CUSTOMER_NAME,
                           COUNT(DISTINCT PS.PICKSLIP_ID)           AS PICKSLIP_COUNT,
                           MAX(BKT.FREEZE)                          AS FREEZE,
                           MAX(BKT.QUICK_PITCH_FLAG)                AS QUICK_PITCH_FLAG,
                           MAX(BKT.PULL_CARTON_AREA)                AS PULL_AREA_ID,
                           MAX(TIA.SHORT_NAME)                      AS PULL_AREA_SHORT_NAME,
                           MAX(TIA.DESCRIPTION)                     AS PULL_AREA_DESCRIPTION,
                           MAX(TIA.WAREHOUSE_LOCATION_ID)           AS BUILDING_PULL_FROM,                           
                           MIN(BKT.PRIORITY)                        AS PRIORITY,
                           MAX(BKT.PULL_TO_DOCK)                    AS PULL_TO_DOCK,
                           MAX(BKT.BUCKET_COMMENT)                  AS BUCKET_COMMENT,
                           MAX(IA.DEFAULT_REPREQ_IA_ID)             AS DEFAULT_REPREQ_IA_ID,
                           COUNT(UNIQUE PSD.SKU_ID)                 AS COUNT_TOTAL_SKU,
                           COUNT(UNIQUE IL.ASSIGNED_UPC_CODE)       AS COUNT_ASSIGNED_SKU
                        FROM <proxy />BUCKET BKT
                       INNER JOIN <proxy />PS PS
                          ON PS.BUCKET_ID = BKT.BUCKET_ID
                        INNER JOIN <proxy />PSDET PSD
                          ON PSD.PICKSLIP_ID = PS.PICKSLIP_ID
                       INNER JOIN <proxy />MASTER_CUSTOMER CUST
                          ON CUST.CUSTOMER_ID = PS.CUSTOMER_ID
                        LEFT OUTER JOIN <proxy />PO PO
                          ON PS.CUSTOMER_ID = PO.CUSTOMER_ID
                         AND PS.PO_ID = PO.PO_ID
                         AND PS.ITERATION = PO.ITERATION
                        LEFT OUTER JOIN <proxy />TAB_INVENTORY_AREA TIA
                          ON TIA.INVENTORY_STORAGE_AREA = BKT.PULL_CARTON_AREA
                        LEFT OUTER JOIN <proxy />IA IA
                          ON IA.IA_ID = BKT.PITCH_IA_ID
                        LEFT OUTER JOIN <proxy />IALOC IL
                            ON IL.ASSIGNED_UPC_CODE = PSD.UPC_CODE
                            AND IL.IA_ID = BKT.PITCH_IA_ID
                            AND IL.VWH_ID = PS.VWH_ID
                       WHERE PS.TRANSFER_DATE IS NULL
                            AND PSD.TRANSFER_DATE IS NULL
                        <if>
                        AND BKT.BUCKET_ID = :BUCKET_ID
                        </if>
                        <else>
                        <if>AND PS.CUSTOMER_ID = :CUSTOMER_ID</if>
                        </else>
                           GROUP BY BKT.BUCKET_ID),
                    TOTAL_PICKED_PIECES AS
                        (
                        SELECT BKT.BUCKET_ID AS BUCKET_ID,                               
                               SUM(CASE
                                     WHEN BOX.CARTON_ID IS NULL AND
                                          BOX.STOP_PROCESS_DATE IS NOT NULL THEN
                                      NVL(BD.EXPECTED_PIECES, BD.CURRENT_PIECES)
                                   END) AS CAN_EXP_PCS_PITCH,
                               SUM(CASE
                                     WHEN BOX.CARTON_ID IS NULL AND
                                          BOX.STOP_PROCESS_DATE IS NOT NULL THEN
                                      BD.CURRENT_PIECES
                                   END) AS CAN_CUR_PCS_PITCH,
                               SUM(CASE
                                     WHEN BOX.CARTON_ID IS NULL AND BOX.VERIFY_DATE IS NOT NULL AND BOX.STOP_PROCESS_DATE IS NULL THEN
                                      NVL(BD.EXPECTED_PIECES, BD.CURRENT_PIECES)
                                   END) AS VRFY_EXP_PCS_PITCH,
                               SUM(CASE
                                     WHEN BOX.CARTON_ID IS NULL AND BOX.VERIFY_DATE IS NOT NULL AND BOX.STOP_PROCESS_DATE IS NULL THEN
                                      BD.CURRENT_PIECES
                                   END) AS VRFY_CUR_PCS_PITCH,
                               SUM(CASE
                                     WHEN BOX.CARTON_ID IS NULL AND BOX.VERIFY_DATE IS NULL AND BOX.STOP_PROCESS_DATE IS NULL THEN
                                      NVL(BD.EXPECTED_PIECES, BD.CURRENT_PIECES)
                                   END) AS UNVRFY_EXP_PCS_PITCH,
                               SUM(CASE
                                     WHEN BOX.CARTON_ID IS NULL AND BOX.VERIFY_DATE IS NULL AND BOX.STOP_PROCESS_DATE IS NULL THEN
                                      BD.CURRENT_PIECES
                                   END) AS UNVRFY_CUR_PCS_PITCH,
                               COUNT(UNIQUE CASE
                                       WHEN BOX.CARTON_ID IS NULL AND BOX.VERIFY_DATE IS NULL AND BOX.STOP_PROCESS_DATE IS NULL AND BOX.IA_ID IS NOT NULL THEN
                                        BOX.UCC128_ID
                                     END) AS INPROGRESS_BOXES_PITCH,
                               COUNT(UNIQUE CASE
                                       WHEN BOX.CARTON_ID IS NULL AND BOX.VERIFY_DATE IS NOT NULL AND BOX.STOP_PROCESS_DATE IS NULL THEN
                                        BOX.UCC128_ID
                                     END) AS VALIDATED_BOXES_PITCH,
                               COUNT(UNIQUE CASE
                                       WHEN BOX.CARTON_ID IS NULL AND BOX.STOP_PROCESS_DATE IS NOT NULL THEN
                                        BOX.UCC128_ID
                                     END) AS CANCELLED_BOXES_PITCH,
                               SUM(CASE
                                     WHEN BOX.CARTON_ID IS NOT NULL AND
                                          BOX.STOP_PROCESS_DATE IS NOT NULL THEN
                                      NVL(BD.EXPECTED_PIECES, BD.CURRENT_PIECES)
                                   END) AS CAN_EXP_PCS_PULL,
                               SUM(CASE
                                     WHEN BOX.CARTON_ID IS NOT NULL AND
                                          BOX.STOP_PROCESS_DATE IS NOT NULL THEN
                                      BD.CURRENT_PIECES
                                   END) AS CAN_CUR_PCS_PULL,
                               SUM(CASE
                                     WHEN BOX.CARTON_ID IS NOT NULL AND BOX.VERIFY_DATE IS NOT NULL AND BOX.STOP_PROCESS_DATE IS NULL THEN
                                      NVL(BD.EXPECTED_PIECES, BD.CURRENT_PIECES)
                                   END) AS VRFY_EXP_PCS_PULL,
                               SUM(CASE
                                     WHEN BOX.CARTON_ID IS NOT NULL AND BOX.VERIFY_DATE IS NOT NULL AND BOX.STOP_PROCESS_DATE IS NULL THEN
                                      BD.CURRENT_PIECES
                                   END) AS VRFY_CUR_PCS_PULL,
                               SUM(CASE
                                     WHEN BOX.CARTON_ID IS NOT NULL AND BOX.VERIFY_DATE IS NULL AND BOX.STOP_PROCESS_DATE IS NULL THEN
                                      NVL(BD.EXPECTED_PIECES, BD.CURRENT_PIECES)
                                   END) AS UNVRFY_EXP_PCS_PULL,
                               SUM(CASE
                                     WHEN BOX.CARTON_ID IS NOT NULL AND BOX.VERIFY_DATE IS NULL AND BOX.STOP_PROCESS_DATE IS NULL THEN
                                      BD.CURRENT_PIECES
                                   END) AS UNVRFY_CUR_PCS_PULL,
                               COUNT(UNIQUE CASE
                                       WHEN BOX.CARTON_ID IS NOT NULL AND BOX.VERIFY_DATE IS NULL AND BOX.STOP_PROCESS_DATE IS NULL AND BOX.IA_ID IS NOT NULL THEN
                                        BOX.UCC128_ID
                                     END) AS INPROGRESS_BOXES_PULL,
                               COUNT(UNIQUE CASE
                                       WHEN BOX.CARTON_ID IS NOT NULL AND BOX.VERIFY_DATE IS NOT NULL AND BOX.STOP_PROCESS_DATE IS NULL THEN
                                        BOX.UCC128_ID
                                     END) AS VALIDATED_BOXES_PULL,
                               COUNT(UNIQUE CASE
                                       WHEN BOX.CARTON_ID IS NOT NULL AND BOX.STOP_PROCESS_DATE IS NOT NULL THEN
                                        BOX.UCC128_ID
                                     END) AS CANCELLED_BOXES_PULL,
                            COUNT(UNIQUE CASE
                                       WHEN BOX.CARTON_ID IS NOT NULL  AND BOX.VERIFY_DATE IS NULL AND BOX.STOP_PROCESS_DATE IS NULL AND BOX.IA_ID IS NULL THEN
                                        BOX.UCC128_ID
                                     END) AS NONPHYSICAL_BOXES_PULL,
                            COUNT(UNIQUE CASE
                                       WHEN BOX.CARTON_ID IS NULL AND BOX.VERIFY_DATE IS NULL AND BOX.STOP_PROCESS_DATE IS NULL AND BOX.IA_ID IS NULL THEN
                                        BOX.UCC128_ID
                                     END) AS NONPHYSICAL_BOXES_PITCH,
                               MAX(CASE
                                     WHEN BOX.CARTON_ID IS NULL THEN
                                      BOX.PITCHING_END_DATE
                                   END) AS MAX_PITCHING_END_DATE,
                               MIN(CASE
                                     WHEN BOX.CARTON_ID IS NULL THEN
                                      BOX.PITCHING_END_DATE
                                   END) AS MIN_PITCHING_END_DATE,
                               MAX(CASE
                                     WHEN BOX.CARTON_ID IS NOT NULL THEN
                                      BOX.PITCHING_END_DATE
                                   END) AS MAX_PULLING_END_DATE,
                               MIN(CASE
                                     WHEN BOX.CARTON_ID IS NOT NULL THEN
                                      BOX.PITCHING_END_DATE
                                   END) AS MIN_PULLING_END_DATE                              
                            FROM <proxy />PS PS
                           INNER JOIN <proxy />BUCKET BKT
                              ON PS.BUCKET_ID = BKT.BUCKET_ID
                           INNER JOIN <proxy />BOX BOX
                              ON PS.PICKSLIP_ID = BOX.PICKSLIP_ID
                           INNER JOIN <proxy />BOXDET BD
                              ON BOX.PICKSLIP_ID = BD.PICKSLIP_ID
                             AND BOX.UCC128_ID = BD.UCC128_ID
                           WHERE PS.TRANSFER_DATE IS NULL                         
                            <if>
                                AND BKT.BUCKET_ID = :BUCKET_ID
                            </if>
                            <else>
                                <if>AND PS.CUSTOMER_ID = :CUSTOMER_ID</if>
                            </else>
                           GROUP BY BKT.BUCKET_ID)
                    SELECT OP.BUCKET_ID               AS BUCKET_ID,
                           OP.NAME                    AS NAME,
                           OP.DATE_CREATED            AS DATE_CREATED,
                           OP.CREATED_BY              AS CREATED_BY,
                           OP.CUSTOMER_ID             AS CUSTOMER_ID,
                           OP.CUSTOMER_NAME           AS CUSTOMER_NAME,
                           OP.PO_COUNT                AS PO_COUNT,
                           OP.MIN_PO                  AS MIN_PO,
                           OP.MAX_PO                  AS MAX_PO,
                           OP.PICKSLIP_COUNT          AS PICKSLIP_COUNT,
                           OP.ORDERED_PIECES          AS ORDERED_PIECES,
                           PP.CAN_EXP_PCS_PITCH       AS CAN_EXP_PCS_PITCH,
                           PP.CAN_CUR_PCS_PITCH       AS CAN_CUR_PCS_PITCH,
                           PP.VRFY_EXP_PCS_PITCH      AS VRFY_EXP_PCS_PITCH,
                           PP.VRFY_CUR_PCS_PITCH      AS VRFY_CUR_PCS_PITCH,
                           PP.UNVRFY_EXP_PCS_PITCH    AS UNVRFY_EXP_PCS_PITCH,
                           PP.UNVRFY_CUR_PCS_PITCH    AS UNVRFY_CUR_PCS_PITCH,
                           PP.INPROGRESS_BOXES_PITCH  AS INPROGRESS_BOXES_PITCH,
                           PP.VALIDATED_BOXES_PITCH   AS VALIDATED_BOXES_PITCH,
                           PP.CANCELLED_BOXES_PITCH   AS CANCELLED_BOXES_PITCH,                           
                           PP.CAN_EXP_PCS_PULL        AS CAN_EXP_PCS_PULL,
                           PP.CAN_CUR_PCS_PULL        AS CAN_CUR_PCS_PULL,
                           PP.VRFY_EXP_PCS_PULL       AS VRFY_EXP_PCS_PULL,
                           PP.VRFY_CUR_PCS_PULL       AS VRFY_CUR_PCS_PULL,
                           PP.UNVRFY_EXP_PCS_PULL     AS UNVRFY_EXP_PCS_PULL,
                           PP.UNVRFY_CUR_PCS_PULL     AS UNVRFY_CUR_PCS_PULL,
                           PP.INPROGRESS_BOXES_PULL   AS INPROGRESS_BOXES_PULL,
                           PP.VALIDATED_BOXES_PULL    AS VALIDATED_BOXES_PULL,
                           PP.CANCELLED_BOXES_PULL    AS CANCELLED_BOXES_PULL,
                           PP.NONPHYSICAL_BOXES_PULL  AS NONPHYSICAL_BOXES_PULL,
                           PP.NONPHYSICAL_BOXES_PITCH AS NONPHYSICAL_BOXES_PITCH,
                           OP.MIN_DC_CANCEL_DATE      AS MIN_DC_CANCEL_DATE,
                           OP.MAX_DC_CANCEL_DATE      AS MAX_DC_CANCEL_DATE,
                           OP.FREEZE                  AS FREEZE,
                           OP.QUICK_PITCH_FLAG        AS QUICK_PITCH_FLAG,
                           OP.PULL_AREA_ID            AS PULL_AREA_ID,
                           OP.PULL_AREA_SHORT_NAME    AS PULL_AREA_SHORT_NAME,
                           OP.PULL_AREA_DESCRIPTION   AS PULL_AREA_DESCRIPTION,
                           OP.BUILDING_PULL_FROM      AS BUILDING_PULL_FROM,
                           OP.PRIORITY                AS PRIORITY,
                           OP.PITCH_IA_ID             AS PITCH_IA_ID,
                           OP.PITCH_AREA_SHORT_NAME   AS PITCH_AREA_SHORT_NAME,
                           OP.PITCH_AREA_DESCRIPTION  AS PITCH_AREA_DESCRIPTION,
                           OP.BUILDING_PITCH_FROM     AS BUILDING_PITCH_FROM, 
                           OP.default_repreq_ia_id    AS REPLENISH_AREA_ID,
                           OP.PULL_TO_DOCK            AS PULL_TO_DOCK,
                           OP.BUCKET_COMMENT          AS BUCKET_COMMENT,                                                                              
                           PP.MAX_PITCHING_END_DATE   AS MAX_PITCHING_END_DATE,
                           PP.MIN_PITCHING_END_DATE   AS MIN_PITCHING_END_DATE,
                           pp.MAX_PULLING_END_DATE    AS MAX_PULLING_END_DATE,
                           pp.MIN_PULLING_END_DATE    AS MIN_PULLING_END_DATE,
                           OP.PITCH_LIMIT             AS PITCH_LIMIT,
                           OP.COUNT_TOTAL_SKU         AS COUNT_TOTAL_SKU,
                           OP.COUNT_ASSIGNED_SKU      AS COUNT_ASSIGNED_SKU                    
                      FROM TOTAL_ORDERED_PIECES OP
                      LEFT OUTER JOIN TOTAL_PICKED_PIECES PP
                        ON OP.BUCKET_ID = PP.BUCKET_ID
                        <if>
                            WHERE (CASE
                                WHEN OP.FREEZE = 'Y' OR PP.BUCKET_ID IS NULL THEN       :FrozenState
                                WHEN NVL(PP.INPROGRESS_BOXES_PULL,0) + NVL(PP.INPROGRESS_BOXES_PITCH,0)  + NVL(PP.NONPHYSICAL_BOXES_PULL,0) + NVL(PP.NONPHYSICAL_BOXES_PITCH,0) = 0 THEN :CompletedState 
                                ELSE                            :InProgressState
                                END) = :state
                            </if>";
            var binder = SqlBinder.Create(row =>
                {
                    var bucket = new Bucket
                        {
                            BucketId = row.GetInteger("BUCKET_ID").Value,
                            BucketComment = row.GetString("BUCKET_COMMENT"),
                            CreatedBy = row.GetString("CREATED_BY"),
                            CreationDate = row.GetDate("DATE_CREATED").Value,
                            BucketName = row.GetString("NAME"),
                            OrderedPieces = row.GetInteger("ORDERED_PIECES").Value,
                            CountPurchaseOrder = row.GetInteger("PO_COUNT") ?? 0,
                            MinPoId = row.GetString("MIN_PO"),
                            MaxPoId = row.GetString("MAX_PO"),
                            MaxCustomerId = row.GetString("CUSTOMER_ID"),
                            MaxCustomerName = row.GetString("CUSTOMER_NAME"),
                            CountPickslips = row.GetInteger("PICKSLIP_COUNT").Value,
                            PriorityId = row.GetInteger("PRIORITY").Value,
                            MinDcCancelDate = row.GetDate("MIN_DC_CANCEL_DATE"),
                            MaxDcCancelDate = row.GetDate("MAX_DC_CANCEL_DATE"),
                            IsFrozen = row.GetString("FREEZE") == "Y",
                            PullingBucket = row.GetString("PULL_TO_DOCK"),
                            QuickPitch = row.GetString("QUICK_PITCH_FLAG") == "Y",
                            PitchLimit = row.GetInteger("PITCH_LIMIT"),
                            CountAssignedSku = row.GetInteger("COUNT_ASSIGNED_SKU") ?? 0,
                            CountTotalSku = row.GetInteger("COUNT_TOTAL_SKU") ?? 0
                        };
                    var activity = bucket.Activities[BucketActivityType.Pulling];
                    activity.Area = new InventoryArea
                        {
                            AreaId = row.GetString("PULL_AREA_ID"),
                            ShortName = row.GetString("PULL_AREA_SHORT_NAME"),
                            Description = row.GetString("PULL_AREA_DESCRIPTION"),
                            BuildingId = row.GetString("BUILDING_PULL_FROM")
                        };
                    activity.Stats[BoxState.Cancelled, PiecesKind.Expected] = row.GetInteger("CAN_EXP_PCS_PULL");
                    activity.Stats[BoxState.Cancelled, PiecesKind.Current] = row.GetInteger("CAN_CUR_PCS_PULL");
                    activity.Stats[BoxState.Completed, PiecesKind.Expected] = row.GetInteger("VRFY_EXP_PCS_PULL");
                    activity.Stats[BoxState.Completed, PiecesKind.Current] = row.GetInteger("VRFY_CUR_PCS_PULL");
                    activity.Stats[BoxState.InProgress, PiecesKind.Expected] = row.GetInteger("UNVRFY_EXP_PCS_PULL");
                    activity.Stats[BoxState.InProgress, PiecesKind.Current] = row.GetInteger("UNVRFY_CUR_PCS_PULL");

                    activity.Stats[BoxState.InProgress] = row.GetInteger("INPROGRESS_BOXES_PULL");
                    activity.Stats[BoxState.Completed] = row.GetInteger("VALIDATED_BOXES_PULL");
                    activity.Stats[BoxState.Cancelled] = row.GetInteger("CANCELLED_BOXES_PULL");
                    activity.Stats[BoxState.NotStarted] = row.GetInteger("NONPHYSICAL_BOXES_PULL");


                    activity.MaxEndDate = row.GetDateTimeOffset("MAX_PULLING_END_DATE");
                    activity.MinEndDate = row.GetDateTimeOffset("MIN_PULLING_END_DATE");

                    activity = bucket.Activities[BucketActivityType.Pitching];
                    activity.Area = new InventoryArea
                               {
                                   AreaId = row.GetString("PITCH_IA_ID"),
                                   ShortName = row.GetString("PITCH_AREA_SHORT_NAME"),
                                   Description = row.GetString("PITCH_AREA_DESCRIPTION"),
                                   BuildingId = row.GetString("BUILDING_PITCH_FROM"),
                                   ReplenishAreaId = row.GetString("REPLENISH_AREA_ID")
                               };
                    activity.Stats[BoxState.Cancelled, PiecesKind.Expected] = row.GetInteger("CAN_EXP_PCS_PITCH");
                    activity.Stats[BoxState.Cancelled, PiecesKind.Current] = row.GetInteger("CAN_CUR_PCS_PITCH");
                    activity.Stats[BoxState.Completed, PiecesKind.Expected] = row.GetInteger("VRFY_EXP_PCS_PITCH");
                    activity.Stats[BoxState.Completed, PiecesKind.Current] = row.GetInteger("VRFY_CUR_PCS_PITCH");
                    activity.Stats[BoxState.InProgress, PiecesKind.Expected] = row.GetInteger("UNVRFY_EXP_PCS_PITCH");
                    activity.Stats[BoxState.InProgress, PiecesKind.Current] = row.GetInteger("UNVRFY_CUR_PCS_PITCH");

                    // Count of unverified boxes
                    activity.Stats[BoxState.InProgress] = row.GetInteger("INPROGRESS_BOXES_PITCH");
                    activity.Stats[BoxState.Completed] = row.GetInteger("VALIDATED_BOXES_PITCH");
                    activity.Stats[BoxState.Cancelled] = row.GetInteger("CANCELLED_BOXES_PITCH");
                    activity.Stats[BoxState.NotStarted] = row.GetInteger("NONPHYSICAL_BOXES_PITCH");

                    activity.MaxEndDate = row.GetDateTimeOffset("MAX_PITCHING_END_DATE");
                    activity.MinEndDate = row.GetDateTimeOffset("MIN_PITCHING_END_DATE");
                    return bucket;
                });

            binder.Parameter("CUSTOMER_ID", customerId)
                  .Parameter("BUCKET_ID", bucketId)
                  .Parameter("state", bucketState)
                  .Parameter("FrozenState", ProgressStage.Frozen.ToString())
                  .Parameter("InProgressState", ProgressStage.InProgress.ToString())
                  .Parameter("CompletedState", ProgressStage.Completed.ToString());
            return _db.ExecuteReader(QUERY, binder, 2000);
        }
    }
}