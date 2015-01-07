using EclipseLibrary.Oracle;
using System;
using System.Collections.Generic;
using System.Web;

namespace DcmsMobile.PickWaves.Repository.BoxPickPallet
{
    internal class BoxPickPalletRepository : PickWaveRepositoryBase
    {
        #region Intialization

        /// <summary>
        /// Constructor of class used to create the connection to database.
        /// </summary>
        /// <param name="requestContext"> </param>
        public BoxPickPalletRepository(TraceContext ctx, string userName, string clientInfo)
            : base(ctx, userName, clientInfo)
        {
        }
        #endregion

        /// <summary>
        /// Get passed bucket information.
        /// </summary>
        /// <param name="bucketId"></param>
        /// <returns></returns>
        public BoxPickBucket GetBucketDetail(int bucketId)
        {
            var QUERY = @"   SELECT COUNT(B.UCC128_ID) OVER()               AS TOTAL_BOXES,
                                    COUNT(B.PALLET_ID) OVER()               AS BOXES_PALLEIZED,
                                    BUCKET.PITCH_LIMIT                      AS PALLET_LIMIT,
                                    BUCKET.NAME                             AS BUCKET_NAME,
                                    P.CUSTOMER_ID                           AS CUSTOMER_ID,
                                    TIA1.WAREHOUSE_LOCATION_ID              AS PULL_BUILDING_ID,
                                    IA.WAREHOUSE_LOCATION_ID                AS PITCH_BUILDING_ID,
                                    BUCKET.FREEZE                           AS FREEZE
                                FROM <proxy />BUCKET BUCKET
                               INNER JOIN <proxy />PS P
                                  ON P.BUCKET_ID = BUCKET.BUCKET_ID
                                LEFT OUTER JOIN <proxy />BOX B
                                  ON P.PICKSLIP_ID = B.PICKSLIP_ID
                                LEFT OUTER JOIN <proxy />TAB_INVENTORY_AREA TIA1
                                  ON TIA1.INVENTORY_STORAGE_AREA = BUCKET.PULL_CARTON_AREA
                                LEFT OUTER JOIN <proxy />IA IA
                                  ON IA.IA_ID = BUCKET.PITCH_IA_ID
                               WHERE P.BUCKET_ID = :BUCKET_ID
                                 AND P.TRANSFER_DATE IS NULL                                  
                                 AND BUCKET.FREEZE IS NULL
                                 AND B.STOP_PROCESS_DATE IS NULL
                                and bucket.pull_type = 'EXP'
                        ";
            var binder = SqlBinder.Create(row =>
            {
                var bucket = new BoxPickBucket
                     {
                         PalletLimit = row.GetInteger("PALLET_LIMIT") ?? 0,
                         BucketName = row.GetString("BUCKET_NAME"),
                         MaxCustomerId = row.GetString("CUSTOMER_ID"),
                         ExpeditedBoxCount = row.GetInteger("BOXES_PALLEIZED"),
                         CountTotalBox = row.GetInteger("TOTAL_BOXES") ?? 0,
                         IsFrozen = row.GetString("FREEZE") == "Y",
                         PullBuildingId = row.GetString("PULL_BUILDING_ID"),
                         PitchBuildingId = row.GetString("PITCH_BUILDING_ID")
                     };

                return bucket;
            });
            binder.Parameter("BUCKET_ID", bucketId);
            return _db.ExecuteSingle(QUERY, binder);
        }

        /// <summary>
        /// Pallets are sorted by print date descending
        /// </summary>
        /// <param name="bucketId"></param>
        /// <param name="palletId"></param>
        /// <returns></returns>
        public IEnumerable<Pallet> GetPallets(int? bucketId, string palletId)
        {
            var QUERY = @"
                SELECT B.PALLET_ID                  AS PALLET_ID,
                       COUNT(DISTINCT B.UCC128_ID)  AS TOTAL_BOXES,
                       MAX(B.LAST_UCC_PRINT_DATE)   AS PRINT_DATE,
                       COUNT(B.IA_ID)               AS PICKED_BOXES,
                       MAX(B.IA_CHANGE_DATE)        AS IA_CHANGE_DATE,
                       MAX(PS.BUCKET_ID)            AS BUCKET_ID
                  FROM <proxy />BOX B
                 INNER JOIN <proxy />PS PS
                    ON B.PICKSLIP_ID = PS.PICKSLIP_ID
                 WHERE 1 = 1
               <if>
                   AND PS.BUCKET_ID = :BUCKET_ID
                   AND PS.TRANSFER_DATE IS NULL
                   AND B.STOP_PROCESS_DATE IS NULL
                   AND B.PALLET_ID IS NOT NULL
               </if>
               <if>AND B.PALLET_ID = :PALLET_ID</if>
                 GROUP BY B.PALLET_ID
                 ORDER BY MAX(B.LAST_UCC_PRINT_DATE) DESC                           
             ";
            var binder = SqlBinder.Create(row => new Pallet
            {
                PalletId = row.GetString("PALLET_ID"),
                BucketId = row.GetInteger("BUCKET_ID"),
                TotalBoxesOnPallet = row.GetInteger("TOTAL_BOXES") ?? 0,
                PrintDate = row.GetDate("PRINT_DATE"),
                PickedBoxes = row.GetInteger("PICKED_BOXES"),
                IaChangeDate = row.GetDate("IA_CHANGE_DATE")
            }).Parameter("BUCKET_ID", bucketId)
                .Parameter("PALLET_ID", palletId);

            return _db.ExecuteReader(QUERY, binder);
        }

        /// <summary>
        /// Create a new pallet.
        /// </summary>
        /// <param name="bucketId"></param>
        /// <param name="palletId"></param>
        /// <param name="palletLimit"></param>
        /// <returns>Returns the number of boxes/new boxes put on the pallet</returns>
        public int CreatePallet(int bucketId, string palletId, int palletLimit)
        {
            var count = 0;
            var QUERY = @"
               BEGIN  
                    :RESULT := <proxy />PKG_BOXEXPEDITE.CREATE_PALLET_2(ABUCKET_ID    => :ABUCKET_ID,
                                             APALLET_ID    => :APALLET_ID,
                                             APALLET_LIMIT => :APALLET_LIMIT);
               END;
            ";
            var binder = SqlBinder.Create();
            binder.Parameter("ABUCKET_ID", bucketId);
            binder.Parameter("APALLET_ID", palletId);
            binder.Parameter("APALLET_LIMIT", palletLimit);
            binder.OutParameter("RESULT", val => count = val ?? 0);
            _db.ExecuteNonQuery(QUERY, binder);
            return count;
        }

        /// <summary>
        /// Remove unPickedBoxes from pallet.
        /// </summary>
        /// <param name="palletId"></param>
        public void RemoveUnPickedBoxesFromPallet(string palletId)
        {
            var QUERY = @"
                BEGIN
                  <proxy />PKG_BOXEXPEDITE.REMOVE_FROM_PALLET(APALLET_ID => :APALLET_ID);
                END;
            ";
            var binder = SqlBinder.Create();
            binder.Parameter("APALLET_ID", palletId);
            _db.ExecuteNonQuery(QUERY, binder);
        }

//        /// <summary>
//        /// Returns the highest priority bucket which has most boxes for expediting
//        /// </summary>
//        /// <returns>The best bucket id</returns>
//        [Obsolete]
//        public int? GetBucketToExpedite()
//        {
//            const string QUERY = @"
//                    SELECT BUCKET.BUCKET_ID
//                      FROM <proxy />BUCKET BUCKET
//                     INNER JOIN <proxy />PS P
//                        ON P.BUCKET_ID = BUCKET.BUCKET_ID
//                     INNER JOIN <proxy />BOX B
//                        ON P.PICKSLIP_ID = B.PICKSLIP_ID
//                     WHERE B.IA_ID IS NULL
//                       AND B.STOP_PROCESS_DATE IS NULL
//                       AND B.PALLET_ID IS NULL
//                       AND BUCKET.PICK_MODE = 'ADREPPWSS'
//                       AND  bucket.pull_type = 'EXP'
//                       AND P.TRANSFER_DATE IS NULL
//                       AND BUCKET.FREEZE IS NULL
//                     GROUP BY BUCKET.BUCKET_ID
//                     ORDER BY MAX(BUCKET.PRIORITY) DESC, COUNT(P.PICKSLIP_ID) DESC
//                ";
//            var binder = SqlBinder.Create(row => row.GetInteger("BUCKET_ID"));
//            return _db.ExecuteSingle(QUERY, binder);
//        }
    }
}