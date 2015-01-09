using DcmsMobile.PickWaves.Helpers;
using DcmsMobile.PickWaves.Repository;
using EclipseLibrary.Oracle;
using System;
using System.Collections.Generic;
using System.Web;

namespace DcmsMobile.PickWaves.Areas.PickWaves.Home
{

    public enum SearchTextType
    {
        Unknown,
        BucketId,
        CustomerId,
        UserName
    }

    internal class HomeRepository : PickWaveRepositoryBase
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
        /// <param name="userName"></param>
        /// <returns></returns>
        /// <remarks>
        /// Sharad 13 Aug 2012: Excluding buckets for which all pickslips have been transferred
        /// Sharad 09 Jan 2013: READYFORMPC is included in READYFORPULL
        /// Shiva 29 apr 2013: Removed dependency of Status. and State of buckets are decides as
        /// Created => No any boxes created.
        /// InProgress => Picking or pulling are not completed yet.
        /// Completed => Picking or pulling are completed yet.
        /// </remarks>
        public IList<BucketSummary> GetBucketSummary(string customerId, string userName)
        {
            const string QUERY = @"
                                    WITH BUCKET_INFO AS
                                                 (SELECT B.BUCKET_ID AS BUCKET_ID,
                                                         P.CUSTOMER_ID AS CUSTOMER_ID,
                                                         MAX(B.PRIORITY) AS PRIORITY,
                                                         MAX(B.FREEZE) AS FREEZE,
                                                         SUM(P.TOTAL_QUANTITY_ORDERED) AS TOTAL_QUANTITY_ORDERED,
                                                         MAX(CUST.NAME) AS CUSTOMER_NAME,
                                                         MAX(CUST.INACTIVE_FLAG) AS INACTIVE_FLAG,
                                                         MIN(PO.DC_CANCEL_DATE) AS DC_CANCEL_DATE,
                                                         MAX(B.CREATED_BY) AS CREATED_BY
                                                    FROM <proxy />BUCKET B
                                                   INNER JOIN <proxy />PS P
                                                      ON P.BUCKET_ID = B.BUCKET_ID
                                                     AND P.TRANSFER_DATE IS NULL
                                                   INNER JOIN <proxy />PO PO
                                                      ON PO.PO_ID = P.PO_ID
                                                     AND PO.ITERATION = P.ITERATION
                                                     AND PO.CUSTOMER_ID = P.CUSTOMER_ID
                                                   INNER JOIN <proxy />MASTER_CUSTOMER CUST
                                                      ON P.CUSTOMER_ID = CUST.CUSTOMER_ID
                                                   GROUP BY B.BUCKET_ID, P.CUSTOMER_ID),
                                                PICKED_PIECES AS
                                                 (SELECT P.BUCKET_ID,
                                                         P.CUSTOMER_ID,
                                                         SUM(BD.CURRENT_PIECES) AS CURRENT_PIECES,
                                                         SUM(NVL(BD.EXPECTED_PIECES, BD.CURRENT_PIECES)) AS EXPECTED_PIECES,
                                                         COUNT(UNIQUE B.UCC128_ID) AS COUNT_BOXES,
                                                         COUNT(UNIQUE CASE
                                                                 WHEN B.VERIFY_DATE IS NOT NULL THEN
                                                                  B.UCC128_ID
                                                               END) AS COUNT_VALIDATED_BOXES
                                                    FROM <proxy />PS P
                                                   INNER JOIN <proxy />BOX B
                                                      ON B.PICKSLIP_ID = P.PICKSLIP_ID
                                                     AND B.STOP_PROCESS_DATE IS NULL
                                                   INNER JOIN <proxy />BOXDET BD
                                                      ON B.PICKSLIP_ID = BD.PICKSLIP_ID
                                                     AND B.UCC128_ID = BD.UCC128_ID
                                                     AND BD.STOP_PROCESS_DATE IS NULL
                                                   WHERE P.TRANSFER_DATE IS NULL  
                                                   GROUP BY P.BUCKET_ID, P.CUSTOMER_ID  
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
                                                 WHEN BI.FREEZE = 'Y'  THEN
                                                  :FROZENSTATE
                                                 WHEN NVL(pp.COUNT_VALIDATED_BOXES, 0) = NVL(pp.COUNT_BOXES, 0) THEN
                                                  :COMPLETEDSTATE
                                                 ELSE
                                                  :INPROGRESSSTATE
                                               END AS BUCKET_STATUS
                                          FROM BUCKET_INFO BI
                                          LEFT OUTER JOIN PICKED_PIECES PP
                                            ON PP.BUCKET_ID = BI.BUCKET_ID
                                           AND PP.CUSTOMER_ID = BI.CUSTOMER_ID
                                         WHERE 1 = 1
                                           <if>AND BI.CUSTOMER_ID = :CUSTOMER_ID</if>
                                           <if>AND BI.CREATED_BY = :CREATED_BY</if>
                                        GROUP BY BI.CUSTOMER_ID,
                                                CASE
                                                WHEN BI.FREEZE = 'Y'  THEN
                                                    :FROZENSTATE
                                                WHEN NVL(pp.COUNT_VALIDATED_BOXES, 0) = NVL(pp.COUNT_BOXES, 0) THEN
                                                    :COMPLETEDSTATE
                                                ELSE
                                                    :INPROGRESSSTATE
                                                END
                                    ";
            var binder = SqlBinder.Create(row => new BucketSummary
                   {
                       BucketCount = row.GetInteger("BUCKET_COUNT").Value,

                       CustomerId = row.GetString("CUSTOMER_ID"),
                       CustomerName = row.GetString("CUSTOMER_NAME"),
                       IsActiveCustomer = row.GetString("INACTIVE_FLAG") != "Y",

                       MaxDcCancelDate = row.GetDate("MAX_DC_CANCEL_DATE").Value,
                       MinDcCancelDate = row.GetDate("MIN_DC_CANCEL_DATE").Value,
                       MaxPriorityId = row.GetInteger("MAX_PRIORITY") ?? 0,
                       OrderedPieces = row.GetInteger("TOTAL_QUANTITY_ORDERED") ?? 0,
                       CurrentPieces = row.GetInteger("CURRENT_PIECES") ?? 0,
                       ExpectedPieces = row.GetInteger("EXPECTED_PIECES") ?? 0,
                       BucketState = row.GetEnum<ProgressStage>("BUCKET_STATUS")
                   });
            binder.Parameter("CUSTOMER_ID", customerId)
                  .Parameter("CREATED_BY", userName)
                  .Parameter("FROZENSTATE", ProgressStage.Frozen.ToString())
                  .Parameter("INPROGRESSSTATE", ProgressStage.InProgress.ToString())
                  .Parameter("COMPLETEDSTATE", ProgressStage.Completed.ToString());
            return _db.ExecuteReader(QUERY, binder);
        }

        internal SearchTextType ParseSearchText(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                throw new ArgumentNullException("searchText");
            }
            const string QUERY = @"
                        <if>
                        SELECT 1 FROM <proxy />BUCKET BKT
                            INNER JOIN <proxy />PS P ON P.BUCKET_ID = BKT.BUCKET_ID
                            WHERE BKT.BUCKET_ID = :int_value
                            AND P.TRANSFER_DATE IS NULL
                        UNION ALL
                        </if>
                        SELECT 2 FROM <proxy />MASTER_CUSTOMER WHERE CUSTOMER_ID = :string_value
                        UNION ALL
                        select 3 from all_users where username = :string_value
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

            binder.Parameter("string_value", searchText.ToUpper());
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
                case 3:
                    ret = SearchTextType.UserName;
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
        public IList<ImportedOrderSummary> GetImportedOrderSummary(string customerId)
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
                                       MAX(case when cust.customer_id is null then 'Y' else cust.inactive_flag end) as inactive_flag,
                                       MAX(CUST.INTERNATIONAL_FLAG)         AS INTERNATIONAL_FLAG
                                  FROM <proxy />DEM_PICKSLIP DEMPS
                                  LEFT OUTER JOIN <proxy />MASTER_CUSTOMER CUST
                                    ON CUST.CUSTOMER_ID = DEMPS.CUSTOMER_ID
                                 WHERE DEMPS.PS_STATUS_ID = 1
                                <if>AND DEMPS.CUSTOMER_ID = :customer_id</if>
                                 GROUP BY DEMPS.CUSTOMER_ID
                                 ORDER BY TOTAL_DOLLARS_ORDERED DESC
                               ";

            var binder = SqlBinder.Create(row => new ImportedOrderSummary
            {

                CustomerId = row.GetString("CUSTOMER_ID"),
                CustonerName = row.GetString("NAME"),
                IsActiveCustomer = row.GetString("inactive_flag") != "Y",

                PickslipCount = row.GetInteger("PICKSLIP_COUNT").Value,
                PiecesOrdered = row.GetInteger("TOTAL_QUANTITY_ORDERED") ?? 0,
                DollarsOrdered = row.GetDecimal("TOTAL_DOLLARS_ORDERED") ?? 0,
                MinDcCancelDate = row.GetDate("MIN_DC_CANCEL_DATE").Value,
                MaxDcCancelDate = row.GetDate("MAX_DC_CANCEL_DATE").Value,
                MinPickslipImportDate = row.GetDate("MIN_PICKSLIP_IMPORT_DATE").Value,
                MaxPickslipImportDate = row.GetDate("MAX_PICKSLIP_IMPORT_DATE").Value,
                InternationalFlag = row.GetString("INTERNATIONAL_FLAG") == "Y"
            });
            binder.Parameter("customer_id", customerId);
            return _db.ExecuteReader(QUERY, binder);
        }



        public IList<BucketBase> GetRecentCreatedBucket(int maxRows)
        {
            const string QUERY = @"SELECT BK.BUCKET_ID AS BUCKET_ID,
                                    MIN(BK.NAME) AS BUCKET_NAME,
                                   MIN(BK.DATE_CREATED) AS DATE_CREATED,
                                   MIN(BK.CREATED_BY) AS CREATED_BY,
                                   MIN(BK.date_modified) as date_modified                                
                              FROM BUCKET BK
                             INNER JOIN PS PS
                                ON BK.BUCKET_ID = PS.BUCKET_ID
                             GROUP BY BK.BUCKET_ID
                             ORDER BY MIN(BK.DATE_CREATED) DESC 
                                NULLS LAST";

            var binder = SqlBinder.Create(row => new BucketBase
            {
                BucketId = row.GetInteger("BUCKET_ID").Value,
                CreatedBy = row.GetString("CREATED_BY"),
                CreationDate = row.GetDate("DATE_CREATED"),
                ModifyDate = row.GetDate("date_modified"),
                BucketName = row.GetString("BUCKET_NAME")

            });
            return _db.ExecuteReader(QUERY, binder, maxRows);
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
