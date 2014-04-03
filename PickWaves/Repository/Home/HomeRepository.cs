using DcmsMobile.PickWaves.Helpers;
using EclipseLibrary.Oracle;
using System;
using System.Collections.Generic;
using System.Web;

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

            /*
            WITH BUCKET_INFO AS
             (SELECT B.BUCKET_ID AS BUCKET_ID,
                     P.CUSTOMER_ID AS CUSTOMER_ID,
                     MAX(B.PRIORITY) AS PRIORITY,
                     MAX(B.FREEZE) AS FREEZE,
                     SUM(P.TOTAL_QUANTITY_ORDERED) AS TOTAL_QUANTITY_ORDERED,
                     MAX(CUST.NAME) AS CUSTOMER_NAME,
                     MAX(CUST.INACTIVE_FLAG) AS INACTIVE_FLAG,
                     MIN(PO.DC_CANCEL_DATE) AS DC_CANCEL_DATE
                FROM BUCKET B
               INNER JOIN PS P
                  ON P.BUCKET_ID = B.BUCKET_ID
                 and P.TRANSFER_DATE IS NULL
               INNER JOIN PO PO
                  ON PO.PO_ID = P.PO_ID
                 AND PO.ITERATION = P.ITERATION
                 AND PO.CUSTOMER_ID = P.CUSTOMER_ID
               INNER JOIN CUST CUST
                  ON P.CUSTOMER_ID = CUST.CUSTOMER_ID
               group by B.BUCKET_ID, P.CUSTOMER_ID),
            PICKED_PIECES AS
             (SELECT p.bucket_id,
                     p.customer_id,
                     SUM(BD.CURRENT_PIECES) AS CURRENT_PIECES,
                     SUM(NVL(BD.EXPECTED_PIECES, BD.CURRENT_PIECES)) AS EXPECTED_PIECES,
                     count(unique B.UCC128_ID) AS count_boxes,
                     count(unique CASE
                             WHEN B.VERIFY_DATE IS not NULL THEN
                              B.UCC128_ID
                           END) AS count_validated_boxes
                FROM PS P
               INNER JOIN BOX B
                  ON B.PICKSLIP_ID = P.PICKSLIP_ID
                 and b.stop_process_date is null
               INNER JOIN BOXDET BD
                  ON B.PICKSLIP_ID = BD.PICKSLIP_ID
                 AND B.UCC128_ID = BD.UCC128_ID
                 and bd.stop_process_date is null
               WHERE P.TRANSFER_DATE IS NULL
  
               group by p.bucket_id, p.customer_id
  
              )
            SELECT BI.CUSTOMER_ID AS CUSTOMER_ID,
                   MAX(BI.CUSTOMER_NAME) AS CUSTOMER_NAME,
                   COUNT(UNIQUE BI.BUCKET_ID) AS BUCKET_COUNT,
                   MAX(BI.PRIORITY) AS MAX_PRIORITY,
                   SUM(PP.CURRENT_PIECES) AS CURRENT_PIECES,
                   SUM(BI.TOTAL_QUANTITY_ORDERED) AS TOTAL_QUANTITY_ORDERED,
                   SUM(PP.EXPECTED_PIECES) AS EXPECTED_PIECES,
                   MAX(BI.INACTIVE_FLAG) AS INACTIVE_FLAG,
                   MAX(BI.DC_CANCEL_DATE) AS MAX_DC_CANCEL_DATE,
                   MIN(BI.DC_CANCEL_DATE) AS MIN_DC_CANCEL_DATE,
                   CASE
                     WHEN BI.FREEZE = 'Y' THEN
                      ':FrozenState'
                     WHEN count_validated_boxes = count_boxes THEN
                      ':COMPLETEDSTATE'
                     ELSE
                      ':InProgressState'
                   END
              FROM BUCKET_INFO BI
              LEFT OUTER JOIN PICKED_PIECES PP
                ON PP.bucket_id = BI.bucket_id
               and pp.customer_id = bi.customer_id
             GROUP BY BI.CUSTOMER_ID,
                      CASE
                        WHEN BI.FREEZE = 'Y' THEN
                         ':FrozenState'
                        WHEN count_validated_boxes = count_boxes THEN
                         ':COMPLETEDSTATE'
                        ELSE
                         ':InProgressState'
                      END

             */
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
                                          CUST.NAME                                 AS CUSTOMER_NAME,
                                          CUST.INACTIVE_FLAG                        AS INACTIVE_FLAG,
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
                                   WHERE P.TRANSFER_DATE IS NULL
                                    <if> AND P.CUSTOMER_ID = :CUSTOMER_ID </if>
                                ),
                                PICKED_PIECES AS
                                 (
                                  -- PK: pickslip_id for row_num = 1
                                  SELECT ROW_NUMBER() OVER(PARTITION BY P.PICKSLIP_ID ORDER BY BD.BOXDET_ID) AS ROW_NUM,
                                          P.PICKSLIP_ID AS PICKSLIP_ID,
                                          SUM(BD.CURRENT_PIECES) OVER(PARTITION BY P.PICKSLIP_ID) AS CURRENT_PIECES,                                         
                                          SUM(NVL(BD.EXPECTED_PIECES, BD.CURRENT_PIECES)) OVER(PARTITION BY P.PICKSLIP_ID) AS EXPECTED_PIECES,                                                                        
                                          count(unique CASE
                                                WHEN B.VERIFY_DATE IS NULL AND
                                                     B.IA_ID IS NOT NULL THEN
                                                 B.UCC128_ID
                                              END) OVER(PARTITION BY P.BUCKET_ID) AS INPROGRESS_BOXES_IN_BKT,
                                          count(unique CASE
                                                WHEN B.VERIFY_DATE IS NULL AND
                                                     B.IA_ID IS NULL THEN
                                                 B.UCC128_ID
                                              END) OVER(PARTITION BY P.BUCKET_ID) AS NONPHYSICAL_BOXES_IN_BKT
                                    FROM <proxy />PS P
                                   INNER JOIN <proxy />BOX B
                                      ON B.PICKSLIP_ID = P.PICKSLIP_ID
                                   INNER JOIN <proxy />BOXDET BD
                                      ON B.PICKSLIP_ID = BD.PICKSLIP_ID
                                     AND B.UCC128_ID = BD.UCC128_ID
                                   WHERE P.TRANSFER_DATE IS NULL
                                    <if> AND P.CUSTOMER_ID = :CUSTOMER_ID </if>
                                     and b.stop_process_date is null
                                     and bd.stop_process_date is null)
                                SELECT BI.CUSTOMER_ID                               AS CUSTOMER_ID,
                                       MAX(BI.CUSTOMER_NAME)                        AS CUSTOMER_NAME,                                       
                                       COUNT(UNIQUE BI.BUCKET_ID)                   AS BUCKET_COUNT,                                       
                                       MAX(BI.PRIORITY)                             AS MAX_PRIORITY,
                                       SUM(PP.CURRENT_PIECES)                       AS CURRENT_PIECES,
                                       SUM(BI.TOTAL_QUANTITY_ORDERED)               AS TOTAL_QUANTITY_ORDERED,                                       
                                       SUM(PP.EXPECTED_PIECES)                      AS EXPECTED_PIECES,                                                                            
                                       MAX(BI.INACTIVE_FLAG)                        AS INACTIVE_FLAG,                                    
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
                       //PitchAreaCount = row.GetInteger("PITCH_AREA_COUNT") ?? 0,
                      // PullAreaCount = row.GetInteger("PULL_AREA_COUNT") ?? 0,
                       BucketState = row.GetEnum<ProgressStage>("BUCKET_STATUS"),
                       //MaxPitchArea = new InventoryArea
                       //{
                       //    AreaId = row.GetString("MAX_PITCH_AREA"),
                       //    ShortName = row.GetString("MAX_PITCH_IA_SHORT_NAME"),
                       //    BuildingId = row.GetString("MAX_PITCH_BUILDING_ID"),
                       //    BuildingName = row.GetString("MAX_PITCH_BUILDING_NAME")
                       //},
                       //MinPitchArea = new InventoryArea
                       //{
                       //    AreaId = row.GetString("MIN_PITCH_AREA"),
                       //    ShortName = row.GetString("MIN_PITCH_IA_SHORT_NAME"),
                       //    BuildingId = row.GetString("MIN_PITCH_BUILDING_ID"),
                       //    BuildingName = row.GetString("MIN_PITCH_BUILDING_NAME")
                       //},
                       //MaxPullArea = new InventoryArea
                       //{
                       //    AreaId = row.GetString("MAX_PULL_AREA"),
                       //    ShortName = row.GetString("MAX_PULL_AREA_SHORT_NAME"),
                       //    BuildingId = row.GetString("MAX_PULL_BUILDING_ID"),
                       //    BuildingName = row.GetString("MAX_PULL_BUILDING_NAME")
                       //},
                       //MinPullArea = new InventoryArea
                       //{
                       //    AreaId = row.GetString("MIN_PULL_AREA"),
                       //    ShortName = row.GetString("MIN_PULL_AREA_SHORT_NAME"),
                       //    BuildingId = row.GetString("MIN_PULL_BUILDING_ID"),
                       //    BuildingName = row.GetString("MIN_PULL_BUILDING_NAME")
                       //}
                   });
            binder.Parameter("CUSTOMER_ID", customerId)
                  .Parameter("FrozenState", ProgressStage.Frozen.ToString())
                  .Parameter("InProgressState", ProgressStage.InProgress.ToString())
                  .Parameter("CompletedState", ProgressStage.Completed.ToString());
            return _db.ExecuteReader(QUERY, binder);
        }

        internal SearchTextType ParseSearchText(string searchText)
        {
            const string QUERY = @"
<if>
SELECT 1 FROM <proxy />BUCKET BKT
 INNER JOIN <proxy />PS P ON P.BUCKET_ID = BKT.BUCKET_ID
 WHERE BKT.BUCKET_ID = :int_value
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

            binder.Parameter("string_value", searchText);
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
