using System;
using System.Collections.Generic;
using System.Web;
using DcmsMobile.PickWaves.Helpers;
using EclipseLibrary.Oracle;

namespace DcmsMobile.PickWaves.Repository.Home
{

    public enum SearchTextType
    {
        Unknown,
        BucketId,
        CustomerId
    }

    public class HomeRepository : PickWaveRepositoryBase
    {

        #region Intialization

        public HomeRepository(TraceContext ctx, string userName, string clientInfo)
            : base(ctx, userName, clientInfo)
        {
        }

        #endregion

        /// <summary>
        /// Counts all buckets whose status is not VALIDATED. Checking buckets are excluded since we do not want to manage them.
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        /// <remarks>
        /// Sharad 13 Aug 2012: Excluding buckets for which all pickslips have been transferred
        /// Sharad 09 Jan 2013: READYFORMPC is included in READYFORPULL
        /// Shiva 29 apr 2013: Removed dependency of Status. and State of buckets are decides as
        /// Created => No any boxes created.
        /// InProgress => Picking or pulling are not completed yet.
        /// Completed => Picking or pulling are completed yet.
        /// </remarks>
        public IList<BucketSummary> GetBucketSummary(string customerId)
        {
            const string QUERY = @"
                                WITH BUCKET_INFO AS
                                 (
                                  -- PK: pickslip_id
                                  SELECT  B.BUCKET_ID                               AS BUCKET_ID,
                                          B.PRIORITY                                AS PRIORITY,
                                          P.PICKSLIP_ID                             AS PICKSLIP_ID,
                                          P.CUSTOMER_ID                             AS CUSTOMER_ID,
                                          B.FREEZE                                  AS FREEZE,
                                          P.TOTAL_QUANTITY_ORDERED                  AS TOTAL_QUANTITY_ORDERED,
                                          SUM(P.TOTAL_QUANTITY_ORDERED) OVER(PARTITION BY B.BUCKET_ID) AS TOTAL_QUANTITY_ORDERED_IN_BKT,
                                          CUST.NAME                                 AS CUSTOMER_NAME,
                                          CUST.INACTIVE_FLAG                        AS INACTIVE_FLAG,
                                          B.PULL_CARTON_AREA                        AS PULL_CARTON_AREA,
                                          TIA.SHORT_NAME                            AS PULL_AREA_SHORT_NAME,
                                          B.PITCH_IA_ID                             AS PITCH_IA_ID,
                                          IA.SHORT_NAME                             AS PITCH_IA_SHORT_NAME,
                                          TIA.WAREHOUSE_LOCATION_ID                 AS PULL_BUILDING_ID,
                                          IA.WAREHOUSE_LOCATION_ID                  AS PITCH_BUILDING_ID,
                                          TWL_PULL.DESCRIPTION                      AS PULL_BUILDING_NAME,
                                          TWL_PITCH.DESCRIPTION                     AS PITCH_BUILDING_NAME,
                                          PO.DC_CANCEL_DATE                         AS DC_CANCEL_DATE
                                    FROM <proxy />BUCKET B
                                   INNER JOIN <proxy />PS P
                                      ON P.BUCKET_ID = B.BUCKET_ID
                                    INNER JOIN <proxy />PO PO
                                        ON PO.PO_ID = P.PO_ID
                                         AND PO.ITERATION = P.ITERATION
                                         AND PO.CUSTOMER_ID = P.CUSTOMER_ID
                                   INNER JOIN <proxy />CUST CUST
                                      ON P.CUSTOMER_ID = CUST.CUSTOMER_ID
                                    LEFT OUTER JOIN <proxy />TAB_INVENTORY_AREA TIA
                                      ON TIA.INVENTORY_STORAGE_AREA = B.PULL_CARTON_AREA
                                    LEFT OUTER JOIN <proxy />IA IA
                                      ON IA.IA_ID = B.PITCH_IA_ID
                                    LEFT OUTER JOIN <proxy />TAB_WAREHOUSE_LOCATION TWL_PULL
                                      ON TWL_PULL.WAREHOUSE_LOCATION_ID = TIA.WAREHOUSE_LOCATION_ID
                                    LEFT OUTER JOIN <proxy />TAB_WAREHOUSE_LOCATION TWL_PITCH
                                      ON TWL_PITCH.WAREHOUSE_LOCATION_ID = IA.WAREHOUSE_LOCATION_ID
                                   WHERE P.TRANSFER_DATE IS NULL
                                    AND B.CREATED_BY_MODULE = :MODULE_CODE
                                    <if> AND P.CUSTOMER_ID = :CUSTOMER_ID </if>
                                ),
                                PICKED_PIECES AS
                                 (
                                  -- PK: pickslip_id for row_num = 1
                                  SELECT ROW_NUMBER() OVER(PARTITION BY P.PICKSLIP_ID ORDER BY BD.BOXDET_ID) AS ROW_NUM,
                                          P.PICKSLIP_ID AS PICKSLIP_ID,
                                          SUM(CASE
                                                WHEN B.STOP_PROCESS_DATE IS NULL THEN
                                                 BD.CURRENT_PIECES
                                              END) OVER(PARTITION BY P.PICKSLIP_ID) AS CURRENT_PIECES,
                                          SUM(CASE
                                                WHEN B.STOP_PROCESS_DATE IS NOT NULL THEN
                                                 NVL(BD.EXPECTED_PIECES, BD.CURRENT_PIECES)
                                              END) OVER(PARTITION BY P.PICKSLIP_ID) AS CANCELLED_PIECES,
                                          SUM(NVL(BD.EXPECTED_PIECES, BD.CURRENT_PIECES)) OVER(PARTITION BY P.BUCKET_ID) AS EXPECTED_PIECES_IN_BKT,
                                          SUM(NVL(BD.EXPECTED_PIECES, BD.CURRENT_PIECES)) OVER(PARTITION BY P.PICKSLIP_ID) AS EXPECTED_PIECES,                                         
                                          COUNT(UNIQUE B.UCC128_ID) OVER(PARTITION BY P.BUCKET_ID) AS COUNT_BOXES_IN_BUCKET,                                
                                          SUM(CASE
                                                WHEN B.STOP_PROCESS_DATE IS NULL AND B.VERIFY_DATE IS NULL AND
                                                     B.IA_ID IS NOT NULL THEN
                                                 B.UCC128_ID
                                              END) OVER(PARTITION BY P.BUCKET_ID) AS INPROGRESS_BOXES_IN_BKT,
                                          SUM(CASE
                                                WHEN B.STOP_PROCESS_DATE IS NULL AND B.VERIFY_DATE IS NULL AND
                                                     B.IA_ID IS NULL THEN
                                                 B.UCC128_ID
                                              END) OVER(PARTITION BY P.BUCKET_ID) AS NONPHYSICAL_BOXES_IN_BKT,
                                          MAX(B.PITCHING_END_DATE) OVER(PARTITION BY P.BUCKET_ID) AS MAX_PITCHING_END_DATE,
                                          MIN(B.PITCHING_END_DATE) OVER(PARTITION BY P.BUCKET_ID) AS MIN_PITCHING_END_DATE
                                    FROM <proxy />PS P
                                   INNER JOIN <proxy />BUCKET BKT
                                      ON P.BUCKET_ID = BKT.BUCKET_ID
                                   INNER JOIN <proxy />BOX B
                                      ON B.PICKSLIP_ID = P.PICKSLIP_ID
                                   INNER JOIN <proxy />BOXDET BD
                                      ON B.PICKSLIP_ID = BD.PICKSLIP_ID
                                     AND B.UCC128_ID = BD.UCC128_ID
                                   WHERE P.TRANSFER_DATE IS NULL
                                    <if> AND P.CUSTOMER_ID = :CUSTOMER_ID </if>
                                     AND BKT.CREATED_BY_MODULE = :MODULE_CODE)
                                SELECT BI.CUSTOMER_ID                               AS CUSTOMER_ID,
                                       MAX(BI.CUSTOMER_NAME)                        AS CUSTOMER_NAME,                                       
                                       COUNT(UNIQUE BI.BUCKET_ID)                   AS BUCKET_COUNT,                                       
                                       MAX(BI.PRIORITY)                             AS MAX_PRIORITY,
                                       SUM(PP.CURRENT_PIECES)                       AS CURRENT_PIECES,
                                       SUM(BI.TOTAL_QUANTITY_ORDERED)               AS TOTAL_QUANTITY_ORDERED,                                       
                                       SUM(PP.EXPECTED_PIECES)                      AS EXPECTED_PIECES,
                                       SUM(PP.CANCELLED_PIECES)                     AS CANCELLED_PIECES,                                     
                                       MAX(BI.INACTIVE_FLAG)                        AS INACTIVE_FLAG,
                                       MAX(PP.MAX_PITCHING_END_DATE)                AS MAX_PITCHING_END_DATE,
                                       MIN(PP.MIN_PITCHING_END_DATE)                AS MIN_PITCHING_END_DATE,
                                       MAX(BI.PULL_CARTON_AREA) KEEP(DENSE_RANK LAST ORDER BY BI.PULL_CARTON_AREA NULLS FIRST)              AS MAX_PULL_AREA,
                                       MAX(BI.PULL_AREA_SHORT_NAME) KEEP(DENSE_RANK LAST ORDER BY BI.PULL_CARTON_AREA NULLS FIRST)          AS MAX_PULL_AREA_SHORT_NAME,
                                       MIN(BI.PULL_CARTON_AREA) KEEP(DENSE_RANK FIRST ORDER BY BI.PULL_CARTON_AREA NULLS LAST)              AS MIN_PULL_AREA,
                                       MIN(BI.PULL_AREA_SHORT_NAME) KEEP(DENSE_RANK FIRST ORDER BY BI.PULL_CARTON_AREA NULLS LAST)          AS MIN_PULL_AREA_SHORT_NAME,
                                       MAX(BI.PULL_BUILDING_ID) KEEP(DENSE_RANK LAST ORDER BY BI.PULL_CARTON_AREA NULLS FIRST)              AS MAX_PULL_BUILDING_ID,
                                       MAX(BI.PULL_BUILDING_NAME) KEEP(DENSE_RANK LAST ORDER BY BI.PULL_CARTON_AREA NULLS FIRST)            AS MAX_PULL_BUILDING_NAME,
                                       MIN(BI.PULL_BUILDING_ID) KEEP(DENSE_RANK FIRST ORDER BY BI.PULL_CARTON_AREA NULLS LAST)              AS MIN_PULL_BUILDING_ID,
                                       MIN(BI.PULL_BUILDING_NAME) KEEP(DENSE_RANK FIRST ORDER BY BI.PULL_CARTON_AREA NULLS LAST)            AS MIN_PULL_BUILDING_NAME,
                                       MAX(BI.PITCH_IA_ID) KEEP(DENSE_RANK LAST ORDER BY BI.PITCH_IA_ID NULLS FIRST)                        AS MAX_PITCH_AREA,                                       
                                       MAX(BI.PITCH_IA_SHORT_NAME) KEEP(DENSE_RANK LAST ORDER BY BI.PITCH_IA_ID NULLS FIRST)                AS MAX_PITCH_IA_SHORT_NAME,
                                       MAX(BI.PITCH_BUILDING_ID) KEEP(DENSE_RANK LAST ORDER BY BI.PITCH_IA_ID NULLS FIRST)                  AS MAX_PITCH_BUILDING_ID,
                                       MAX(BI.PITCH_BUILDING_NAME) KEEP(DENSE_RANK LAST ORDER BY BI.PITCH_IA_ID NULLS FIRST)                AS MAX_PITCH_BUILDING_NAME,
                                       MIN(BI.PITCH_IA_ID) KEEP(DENSE_RANK FIRST ORDER BY BI.PITCH_IA_ID NULLS LAST)                        AS MIN_PITCH_AREA,                                       
                                       MIN(BI.PITCH_IA_SHORT_NAME) KEEP(DENSE_RANK FIRST ORDER BY BI.PITCH_IA_ID NULLS LAST)                AS MIN_PITCH_IA_SHORT_NAME,
                                       MIN(BI.PITCH_BUILDING_ID) KEEP(DENSE_RANK FIRST ORDER BY BI.PITCH_IA_ID NULLS LAST)                  AS MIN_PITCH_BUILDING_ID,
                                       MIN(BI.PITCH_BUILDING_NAME) KEEP(DENSE_RANK FIRST ORDER BY BI.PITCH_IA_ID NULLS LAST)                AS MIN_PITCH_BUILDING_NAME,
                                       COUNT(UNIQUE BI.PULL_CARTON_AREA)            AS PULL_AREA_COUNT,                                       
                                       COUNT(UNIQUE BI.PITCH_IA_ID)                 AS PITCH_AREA_COUNT,                                      
                                       CASE
                                         WHEN BI.FREEZE = 'Y' THEN                                                                  :FrozenState
                                         WHEN NVL(PP.INPROGRESS_BOXES_IN_BKT, 0) + NVL(PP.NONPHYSICAL_BOXES_IN_BKT, 0) = 0 THEN   :COMPLETEDSTATE
                                         ELSE                                                                                       :InProgressState
                                       END                                                                                                  AS BUCKET_STATUS,
                                    MAX(BI.DC_CANCEL_DATE)                                                                                  AS MAX_DC_CANCEL_DATE,
                                    MIN(BI.DC_CANCEL_DATE)                                                                                  AS MIN_DC_CANCEL_DATE
                                  FROM BUCKET_INFO BI
                                  LEFT OUTER JOIN PICKED_PIECES PP
                                    ON PP.PICKSLIP_ID = BI.PICKSLIP_ID
                                   AND PP.ROW_NUM = 1                                  
                                WHERE    1 = 1   
                                <if> AND BI.CUSTOMER_ID = :CUSTOMER_ID </if>
                                GROUP BY BI.CUSTOMER_ID,
                                        CASE
                                            WHEN BI.FREEZE = 'Y' THEN                                                                   :FrozenState
                                            WHEN NVL(PP.INPROGRESS_BOXES_IN_BKT, 0) + NVL(PP.NONPHYSICAL_BOXES_IN_BKT, 0) = 0 THEN      :CompletedState
                                            ELSE                                                                                        :InProgressState
                                        END";
            var binder = SqlBinder.Create(row => new BucketSummary
                   {
                       BucketCount = row.GetInteger("BUCKET_COUNT").Value,
                       Customer = new Customer
                       {
                           CustomerId = row.GetString("CUSTOMER_ID"),
                           Name = row.GetString("CUSTOMER_NAME"),
                           IsActive = row.GetString("INACTIVE_FLAG") != "Y"
                       },
                       MaxDcCancelDate = row.GetDate("MAX_DC_CANCEL_DATE").Value,
                       MinDcCancelDate = row.GetDate("MIN_DC_CANCEL_DATE").Value,
                       MaxPriorityId = row.GetInteger("MAX_PRIORITY") ?? 0,
                       OrderedPieces = row.GetInteger("TOTAL_QUANTITY_ORDERED") ?? 0,
                       CurrentPieces = row.GetInteger("CURRENT_PIECES") ?? 0,
                       ExpectedPieces = row.GetInteger("EXPECTED_PIECES") ?? 0,
                       CancelledPieces = row.GetInteger("CANCELLED_PIECES") ?? 0,
                       PitchAreaCount = row.GetInteger("PITCH_AREA_COUNT") ?? 0,
                       PullAreaCount = row.GetInteger("PULL_AREA_COUNT") ?? 0,
                       BucketState = row.GetEnum<ProgressStage>("BUCKET_STATUS"),
                       MaxPitchingEndDate = row.GetDateTimeOffset("MAX_PITCHING_END_DATE"),
                       MinPitchingEndDate = row.GetDateTimeOffset("MIN_PITCHING_END_DATE"),
                       MaxPitchArea = new InventoryArea
                       {
                           AreaId = row.GetString("MAX_PITCH_AREA"),
                           ShortName = row.GetString("MAX_PITCH_IA_SHORT_NAME"),
                           BuildingId = row.GetString("MAX_PITCH_BUILDING_ID"),
                           BuildingName = row.GetString("MAX_PITCH_BUILDING_NAME")
                       },
                       MinPitchArea = new InventoryArea
                       {
                           AreaId = row.GetString("MIN_PITCH_AREA"),
                           ShortName = row.GetString("MIN_PITCH_IA_SHORT_NAME"),
                           BuildingId = row.GetString("MIN_PITCH_BUILDING_ID"),
                           BuildingName = row.GetString("MIN_PITCH_BUILDING_NAME")
                       },
                       MaxPullArea = new InventoryArea
                       {
                           AreaId = row.GetString("MAX_PULL_AREA"),
                           ShortName = row.GetString("MAX_PULL_AREA_SHORT_NAME"),
                           BuildingId = row.GetString("MAX_PULL_BUILDING_ID"),
                           BuildingName = row.GetString("MAX_PULL_BUILDING_NAME")
                       },
                       MinPullArea = new InventoryArea
                       {
                           AreaId = row.GetString("MIN_PULL_AREA"),
                           ShortName = row.GetString("MIN_PULL_AREA_SHORT_NAME"),
                           BuildingId = row.GetString("MIN_PULL_BUILDING_ID"),
                           BuildingName = row.GetString("MIN_PULL_BUILDING_NAME")
                       }
                   });
            binder.Parameter("CUSTOMER_ID", customerId)
                  .Parameter("FrozenState", ProgressStage.Frozen.ToString())
                  .Parameter("InProgressState", ProgressStage.InProgress.ToString())
                  .Parameter("CompletedState", ProgressStage.Completed.ToString())
                  .Parameter("MODULE_CODE", MODULE_CODE);
            return _db.ExecuteReader(QUERY, binder);
        }

        internal SearchTextType ParseSearchText(string searchText)
        {
            const string QUERY = @"
<if>
SELECT 1 FROM <proxy />BUCKET BKT
 INNER JOIN <proxy />PS P ON P.BUCKET_ID = BKT.BUCKET_ID
 WHERE BKT.BUCKET_ID = :int_value
   AND BKT.CREATED_BY_MODULE = :MODULE_CODE
   AND P.TRANSFER_DATE IS NULL
UNION
</if>
SELECT 2 FROM <proxy />CUST WHERE CUSTOMER_ID = :string_value
ORDER BY 1
";
            var binder = SqlBinder.Create(row => row.GetInteger(0) ?? 0);
            int intValue;
            if (int.TryParse(searchText, out intValue))
            {
                binder.Parameter("int_value", intValue);
            }
            else
            {
                binder.Parameter("int_value", string.Empty);
            }

            binder.Parameter("string_value", searchText)
                .Parameter("MODULE_CODE", MODULE_CODE);
            var result = _db.ExecuteSingle(QUERY, binder);

            SearchTextType ret;
            switch (result)
            {
                case 0:
                    ret = SearchTextType.Unknown;
                    break;

                case 1:
                    ret = SearchTextType.BucketId;
                    break;

                case 2:
                    ret = SearchTextType.CustomerId;
                    break;

                default:
                    throw new NotImplementedException("Not expected");

            }
            return ret;
        }

        /// <summary>
        /// Get Imported Order for a specific customer
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        public IEnumerable<ImportedOrderSummary> GetImportedOrderSummary(string customerId)
        {
            const string QUERY = @"
                                SELECT DEMPS.CUSTOMER_ID                    AS CUSTOMER_ID,
                                       MAX(CUST.NAME)                       AS NAME,
                                       COUNT(*)                             AS PICKSLIP_COUNT,
                                       SUM(DEMPS.TOTAL_QUANTITY_ORDERED)    AS TOTAL_QUANTITY_ORDERED,
                                       SUM(DEMPS.TOTAL_DOLLARS_ORDERED)     AS TOTAL_DOLLARS_ORDERED,
                                       MIN(DC_CANCEL_DATE)                  AS MIN_DC_CANCEL_DATE,
                                       MAX(DC_CANCEL_DATE)                  AS MAX_DC_CANCEL_DATE,
                                       MIN(DEMPS.PICKSLIP_IMPORT_DATE)      AS MIN_PICKSLIP_IMPORT_DATE,
                                       MAX(DEMPS.PICKSLIP_IMPORT_DATE)      AS MAX_PICKSLIP_IMPORT_DATE,
                                       MAX(case when cust.customer_id is null then 'Y' else cust.inactive_flag end) as inactive_flag
                                  FROM <proxy />DEM_PICKSLIP DEMPS
                                  LEFT OUTER JOIN <proxy />CUST
                                    ON CUST.CUSTOMER_ID = DEMPS.CUSTOMER_ID
                                 WHERE DEMPS.PS_STATUS_ID = 1
                                <if>AND DEMPS.CUSTOMER_ID = :customer_id</if>
                                 GROUP BY DEMPS.CUSTOMER_ID
                                 ORDER BY TOTAL_DOLLARS_ORDERED DESC
                               ";

            var binder = SqlBinder.Create(row => new ImportedOrderSummary
            {
                Customer = new Customer
                {
                    CustomerId = row.GetString("CUSTOMER_ID"),
                    Name = row.GetString("NAME"),
                    IsActive = row.GetString("inactive_flag") != "Y"
                },
                PickslipCount = row.GetInteger("PICKSLIP_COUNT").Value,
                PiecesOrdered = row.GetInteger("TOTAL_QUANTITY_ORDERED") ?? 0,
                DollarsOrdered = row.GetDecimal("TOTAL_DOLLARS_ORDERED") ?? 0,
                MinDcCancelDate = row.GetDate("MIN_DC_CANCEL_DATE").Value,
                MaxDcCancelDate = row.GetDate("MAX_DC_CANCEL_DATE").Value,
                MinPickslipImportDate = row.GetDate("MIN_PICKSLIP_IMPORT_DATE").Value,
                MaxPickslipImportDate = row.GetDate("MAX_PICKSLIP_IMPORT_DATE").Value
            });
            binder.Parameter("customer_id", customerId);
            return _db.ExecuteReader(QUERY, binder);
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
