using DcmsMobile.PickWaves.Helpers;
using DcmsMobile.PickWaves.Repository;
using EclipseLibrary.Oracle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace DcmsMobile.PickWaves.Areas.PickWaves.ManageWaves
{

    internal class ManageWavesRepository : PickWaveRepositoryBase
    {
        #region Intialization

        /// <summary>
        /// Constructor of class used to create the connection to database.
        /// </summary>
        /// <param name="trace"></param>
        /// <param name="moduleName"></param>
        /// <param name="clientInfo"></param>
        public ManageWavesRepository(TraceContext trace, string userName, string clientInfo)
            : base(trace, userName, clientInfo)
        {
        }
        #endregion

        public BucketEditable GetEditableBucket(int bucketId)
        {
            const string QUERY = @"
                       SELECT     BKT.BUCKET_ID AS BUCKET_ID,
                                   BKT.NAME AS NAME,
                                   BKT.BUCKET_COMMENT AS BUCKET_COMMENT,
                                   BKT.PITCH_LIMIT AS PITCH_LIMIT,
                                   BKT.PITCH_IA_ID AS PITCH_AREA_ID,
                                  BKT.PULL_CARTON_AREA AS PULL_AREA_ID,
                                   (select max(customer_id) from ps p where p.bucket_id = bkt.bucket_id) AS CUSTOMER_ID,
                                   BKT.FREEZE AS FREEZE,
                                   CASE WHEN BKT.PITCH_TYPE = 'QUICK' THEN 'Y' END AS QUICK_PITCH_FLAG,
                                   BKT.PULL_TYPE AS PULL_TYPE   
                              FROM BUCKET BKT
                             WHERE BKT.BUCKET_ID = :BUCKET_ID";
            var binder = SqlBinder.Create(row =>
            {
                var bucket = new BucketEditable
              {
                  BucketId = row.GetInteger("BUCKET_ID").Value,
                  BucketName = row.GetString("NAME"),
                  BucketComment = row.GetString("BUCKET_COMMENT"),
                  PitchLimit = row.GetInteger("PITCH_LIMIT") ?? 0,
                  IsFrozen = row.GetString("FREEZE") == "Y",
                  CustomerId = row.GetString("CUSTOMER_ID"),
                  PullAreaId = row.GetString("PULL_AREA_ID"),
                  // PullAreaShortName = row.GetString("PULL_AREA_SHORT_NAME"),
                  PitchAreaId = row.GetString("PITCH_AREA_ID"),
                  //  PitchAreaShortName = row.GetString("PITCH_AREA_SHORT_NAME"),
                  QuickPitch = row.GetString("QUICK_PITCH_FLAG") == "Y",
                  RequireBoxExpediting = row.GetString("PULL_TYPE") == "EXP"

              };
                return bucket;
            }
            );
            binder.Parameter("BUCKET_ID", bucketId);
            return _db.ExecuteSingle(QUERY, binder);
        }

        /// <summary>
        /// Get the list of SKUs of passed bucket
        /// </summary>
        /// <param name="bucketId"></param>
        /// <param name="stateFilter"> </param>
        /// <param name="activityFilter"> </param>
        /// <returns></returns>
        public IList<BucketSku> GetBucketSkuList(int bucketId)
        {

            /*
            SELECT ...,
                         count(unique case when b.carton_id is not null then b.ucc128_id end) AS count_pullable_boxes,
                         count(unique case when b.carton_id is null then b.ucc128_id end) AS count_pitchable_boxes
                FROM ...
             */
            const string QUERY = @"
                        WITH ALL_ORDERED_SKU AS
                                 (                             
                                    SELECT PD.SKU_ID               AS SKU_ID,
                                           P.VWH_ID                AS VWH_ID,
                                       --  MAX(B.PITCH_IA_ID)  AS PITCH_AREA,
                                         SUM(PD.PIECES_ORDERED)  AS QUANTITY_ORDERED
                                    FROM <proxy />PS P
                                   INNER JOIN <proxy />PSDET PD
                                      ON P.PICKSLIP_ID = PD.PICKSLIP_ID
                                   WHERE P.BUCKET_ID = :BUCKET_ID
                                     AND P.TRANSFER_DATE IS NULL
                                     AND PD.TRANSFER_DATE IS NULL
group by PD.SKU_ID, P.VWH_ID
                            ),
                            ALL_INVENTORY_SKU(SKU_ID,
                            VWH_ID,
                            BUILDING_ID,
                            INVENTORY_AREA,
                            SHORT_NAME,
                            PIECES_IN_AREA,
location_id,
                            DESCRIPTION,
                            REPLENISH_FROM_AREA_ID
                            ) AS
                                 (SELECT SCD.SKU_ID AS SKU_ID,
                                         SC.VWH_ID AS VWH_ID,
                                         NVL(TIA.WAREHOUSE_LOCATION_ID, MSL.WAREHOUSE_LOCATION_ID) AS BUILDING_ID,
                                         SC.CARTON_STORAGE_AREA AS INVENTORY_AREA,
                                         TIA.SHORT_NAME,
                                         SCD.QUANTITY AS PIECES_IN_AREA,
         sc.location_id,
                                         TIA.DESCRIPTION, 
                                         NULL
                                    FROM <proxy />SRC_CARTON_DETAIL SCD
                                   INNER JOIN <proxy />SRC_CARTON SC
                                      ON SC.CARTON_ID = SCD.CARTON_ID
                                   LEFT OUTER JOIN <proxy />MASTER_STORAGE_LOCATION MSL
                                      ON SC.LOCATION_ID = MSL.LOCATION_ID
                                     AND SC.CARTON_STORAGE_AREA = MSL.STORAGE_AREA
                                   INNER JOIN <proxy />TAB_INVENTORY_AREA TIA
                                      ON SC.CARTON_STORAGE_AREA = TIA.INVENTORY_STORAGE_AREA
                                   WHERE SC.SUSPENSE_DATE IS NULL
                                     AND SC.QUALITY_CODE = '01'
--and (scd.sku_id, sc.vwh_id) in (select sku_id, vwh_id from ALL_ORDERED_SKU)
                                UNION ALL
                                SELECT IC.SKU_ID,
                                       IL.VWH_ID,
                                       IL.WAREHOUSE_LOCATION_ID,
                                       IL.IA_ID,
                                       I.SHORT_NAME,
                                       IC.NUMBER_OF_UNITS,
         il.location_id,
                                       I.SHORT_DESCRIPTION, 
                                       I.DEFAULT_REPREQ_IA_ID
                                  FROM <proxy />IALOC_CONTENT IC
                                 INNER JOIN <proxy />IALOC IL
                                    ON IL.IA_ID = IC.IA_ID
                                   AND IL.LOCATION_ID = IC.LOCATION_ID
                                 INNER JOIN <proxy />IA I
                                    ON I.IA_ID = IL.IA_ID 
--   where (ic.sku_id, il.vwh_id) in (select sku_id, vwh_id from ALL_ORDERED_SKU)                           
                                ),
                            PIVOT_ALL_INVENTORY_SKU(SKU_ID,
                            VWH_ID,
                            XML_COLUMN) AS
                                 (SELECT *
                                    FROM ALL_INVENTORY_SKU PIVOT XML(
MAX(location_id) KEEP(DENSE_RANK FIRST ORDER BY PIECES_IN_AREA DESC) AS best_location_id,
MAX(PIECES_IN_AREA) AS PIECES_AT_BEST_LOCATION,
SUM(PIECES_IN_AREA) AS PIECES_IN_AREA,
MAX(DESCRIPTION) AS AREA_DESCRIPTION,
MAX(SHORT_NAME) AS AREA_SHORT_NAME,
MAX(REPLENISH_FROM_AREA_ID) AS REPLENISH_FROM_AREA_ID
                                    FOR(INVENTORY_AREA, BUILDING_ID) IN(ANY, ANY))),
                            BOX_SKU AS
                                 (SELECT BD.SKU_ID AS SKU_ID,
                                         B.VWH_ID AS VWH_ID,
                                         SUM(CASE
                                               WHEN B.CARTON_ID IS NULL AND b.verify_date is null THEN
                                                BD.CURRENT_PIECES
                                             END) AS UNVRFY_CUR_PCS_PITCH,
                                         SUM(CASE
                                               WHEN B.CARTON_ID IS NULL AND b.verify_date is not null THEN
                                                BD.CURRENT_PIECES
                                             END) AS VRFY_CUR_PCS_PITCH,
                                         SUM(CASE
                                               WHEN B.CARTON_ID IS NULL AND b.verify_date is null THEN
                                                NVL(BD.EXPECTED_PIECES, BD.CURRENT_PIECES)
                                             END) AS UNVRFY_EXP_PCS_PITCH,
                                         SUM(CASE
                                               WHEN B.CARTON_ID IS NOT NULL AND b.verify_date is not null THEN
                                                BD.CURRENT_PIECES
                                             END) AS VRFY_CUR_PCS_PULL,
                                         SUM(CASE
                                               WHEN B.CARTON_ID IS NOT NULL AND b.verify_date is null THEN
                                                NVL(BD.EXPECTED_PIECES, BD.CURRENT_PIECES)
                                             END) AS UNVRFY_EXP_PCS_PULL,
                                          SUM(CASE
                                               WHEN B.CARTON_ID IS NOT NULL AND b.verify_date is null THEN
                                                BD.CURRENT_PIECES
                                             END) AS UNVRFY_CUR_PCS_PULL,                                        
                                         MAX(CASE
                                               WHEN B.CARTON_ID IS NULL THEN
                                                 B.PITCHING_END_DATE
                                               END
                                            ) AS MAX_PITCHING_END_DATE,
                                         MIN(CASE
                                               WHEN B.CARTON_ID IS NULL THEN
                                                 B.PITCHING_END_DATE
                                             END
                                            ) AS MIN_PITCHING_END_DATE,
                                         MAX(CASE
                                               WHEN B.CARTON_ID IS NOT NULL THEN
                                                 B.PITCHING_END_DATE
                                               END
                                            ) AS MAX_PULL_END_DATE,
                                         MIN(CASE
                                               WHEN B.CARTON_ID IS NOT NULL THEN
                                                 B.PITCHING_END_DATE
                                             END
                                            ) AS MIN_PULL_END_DATE,
                         count(unique case when b.carton_id is not null then b.ucc128_id end) AS count_pullable_boxes,
                         count(unique case when b.carton_id is null then b.ucc128_id end) AS count_pitchable_boxes
                                    FROM <proxy />BOX B
                                   INNER JOIN <proxy />BOXDET BD
                                      ON B.PICKSLIP_ID = BD.PICKSLIP_ID
                                     AND B.UCC128_ID = BD.UCC128_ID
                                   INNER JOIN <proxy />PS P
                                      ON P.PICKSLIP_ID = B.PICKSLIP_ID
                                   WHERE p.bucket_id = :BUCKET_ID
                                    and b.stop_process_date is null 
                                    and bd.stop_process_date is null
                                   GROUP BY BD.SKU_ID, B.VWH_ID
                            )
                            SELECT MS.SKU_ID        AS SKU_ID,
                                   MS.STYLE         AS STYLE,
                                   MS.COLOR         AS COLOR,
                                   MS.DIMENSION     AS DIMENSION,
                                   MS.SKU_SIZE      AS SKU_SIZE,
                                   ms.UPC_CODE     AS UPC_CODE,
                                   AOS.VWH_ID       AS VWH_ID,
                                   AOS.QUANTITY_ORDERED             AS QUANTITY_ORDERED,
                                   BOX_SKU.UNVRFY_CUR_PCS_PITCH     AS UNVRFY_CUR_PCS_PITCH,
                                   BOX_SKU.VRFY_CUR_PCS_PITCH       AS VRFY_CUR_PCS_PITCH,
                                   BOX_SKU.UNVRFY_EXP_PCS_PITCH     AS UNVRFY_EXP_PCS_PITCH,
                                   BOX_SKU.VRFY_CUR_PCS_PULL        AS VRFY_CUR_PCS_PULL,
                                   BOX_SKU.UNVRFY_EXP_PCS_PULL      AS UNVRFY_EXP_PCS_PULL,
                                   BOX_SKU.UNVRFY_CUR_PCS_PULL      AS UNVRFY_CUR_PCS_PULL,
                                   BOX_SKU.MAX_PITCHING_END_DATE    AS MAX_PITCHING_END_DATE,
                                   BOX_SKU.MIN_PITCHING_END_DATE    AS MIN_PITCHING_END_DATE,
                                   BOX_SKU.MAX_PULL_END_DATE        AS MAX_PULL_END_DATE,
                                   BOX_SKU.MIN_PULL_END_DATE        AS MIN_PULL_END_DATE,
                                   MS.WEIGHT_PER_DOZEN              AS WEIGHT_PER_DOZEN,
                                   MS.VOLUME_PER_DOZEN              AS VOLUME_PER_DOZEN,
                                   AIS.XML_COLUMN.getstringval()    AS XML_COLUMN,
                                   BOX_SKU.count_pullable_boxes As count_pullable_boxes,
                                   BOX_SKU.count_pitchable_boxes AS count_pitchable_boxes
                              FROM ALL_ORDERED_SKU AOS
                             INNER JOIN <proxy />MASTER_SKU MS
                                ON MS.SKU_ID = AOS.SKU_ID
                              LEFT OUTER JOIN PIVOT_ALL_INVENTORY_SKU AIS
                                ON AIS.SKU_ID = aos.SKU_ID
                               AND AIS.VWH_ID = AOS.VWH_ID
                              LEFT OUTER JOIN BOX_SKU BOX_SKU
                                ON BOX_SKU.SKU_ID = AOS.SKU_ID
                               AND BOX_SKU.VWH_ID = AOS.VWH_ID
";
            var binder = SqlBinder.Create(row =>
                {
                    var bs = new BucketSku
                        {
                            Sku = new Sku
                            {
                                SkuId = row.GetInteger("SKU_ID").Value,
                                Style = row.GetString("STYLE"),
                                Color = row.GetString("COLOR"),
                                Dimension = row.GetString("DIMENSION"),
                                SkuSize = row.GetString("SKU_SIZE"),
                                UpcCode = row.GetString("UPC_CODE"),
                                VwhId = row.GetString("VWH_ID"),
                                WeightPerDozen = row.GetDecimal("WEIGHT_PER_DOZEN"),
                                VolumePerDozen = row.GetDecimal("VOLUME_PER_DOZEN")
                            },
                            QuantityOrdered = row.GetInteger("QUANTITY_ORDERED"),
                            BucketSkuInAreas = MapOrderedSkuXml(row.GetString("XML_COLUMN"))
                        };
                    bs.Activities[BucketActivityType.Pitching].MaxEndDate = row.GetDateTimeOffset("MAX_PITCHING_END_DATE");
                    bs.Activities[BucketActivityType.Pitching].MinEndDate = row.GetDateTimeOffset("MIN_PITCHING_END_DATE");
                    bs.Activities[BucketActivityType.Pitching].Stats[PiecesKind.Current, BoxState.InProgress] = row.GetInteger("UNVRFY_CUR_PCS_PITCH");
                    bs.Activities[BucketActivityType.Pitching].Stats[PiecesKind.Current, BoxState.Completed] = row.GetInteger("VRFY_CUR_PCS_PITCH");
                    bs.Activities[BucketActivityType.Pitching].Stats[PiecesKind.Expected, BoxState.InProgress] = row.GetInteger("UNVRFY_EXP_PCS_PITCH");
                    bs.Activities[BucketActivityType.Pulling].MaxEndDate = row.GetDateTimeOffset("MAX_PULL_END_DATE");
                    bs.Activities[BucketActivityType.Pulling].MinEndDate = row.GetDateTimeOffset("MIN_PULL_END_DATE");
                    bs.Activities[BucketActivityType.Pulling].Stats[PiecesKind.Current, BoxState.InProgress] = row.GetInteger("UNVRFY_CUR_PCS_PULL");
                    bs.Activities[BucketActivityType.Pulling].Stats[PiecesKind.Current, BoxState.Completed] = row.GetInteger("VRFY_CUR_PCS_PULL");
                    bs.Activities[BucketActivityType.Pulling].Stats[PiecesKind.Expected, BoxState.InProgress] = row.GetInteger("UNVRFY_EXP_PCS_PULL");
                    // This contains in progress boxes also
                    bs.Activities[BucketActivityType.Pitching].Stats[BoxState.NotStarted] = row.GetInteger("count_pitchable_boxes");
                    bs.Activities[BucketActivityType.Pulling].Stats[BoxState.NotStarted] = row.GetInteger("count_pullable_boxes");
                    return bs;
                });

            binder.Parameter("BUCKET_ID", bucketId);
            return _db.ExecuteReader(QUERY, binder, 2000);
        }

        private IList<CartonAreaInventory> MapOrderedSkuXml(string xml)
        {
            if (string.IsNullOrWhiteSpace(xml))
            {
                // No inventory
                return new CartonAreaInventory[0];
            }
            var x = XElement.Parse(xml);
            var result = (from item in x.Elements("item")
                          let column = item.Elements("column")
                          select new CartonAreaInventory
                          {
                              InventoryArea = new InventoryArea
                              {
                                  AreaId = (string)column.First(p => p.Attribute("name").Value == "INVENTORY_AREA"),
                                  BuildingId = (string)column.First(p => p.Attribute("name").Value == "BUILDING_ID"),
                                  Description = (string)column.First(p => p.Attribute("name").Value == "AREA_DESCRIPTION"),
                                  ShortName = (string)column.First(p => p.Attribute("name").Value == "AREA_SHORT_NAME"),
                                  ReplenishAreaId = (string)column.First(p => p.Attribute("name").Value == "REPLENISH_FROM_AREA_ID")
                              },
                              InventoryPieces = (int)column.First(p => p.Attribute("name").Value == "PIECES_IN_AREA"),
                              //PiecesInSmallestCarton = (int)column.First(p => p.Attribute("name").Value == "PIECES_IN_SMALLEST_CARTON"),
                              BestLocationId = (string)column.First(p => p.Attribute("name").Value == "BEST_LOCATION_ID"),
                              // PIECES_AT_BEST_LOCATION
                              PiecesAtBestLocation = (int)column.First(p => p.Attribute("name").Value == "PIECES_AT_BEST_LOCATION"),
                          }).ToList();
            return result;
        }

        /// <summary>
        /// Get Pickslip list of passed bucket
        /// </summary>
        /// <param name="bucketId"></param>
        /// <returns></returns>
        public IList<Pickslip> GetBucketPickslips(int bucketId)
        {
            const string QUERY = @"
                                SELECT PS.PICKSLIP_ID                           AS PICKSLIP_ID,
                                       MAX(PS.PO_ID)                            AS PO_ID,
                                       MAX(PS.CUSTOMER_DC_ID)                   AS CUSTOMER_DC_ID,
                                       MAX(PS.CUSTOMER_STORE_ID)                AS CUSTOMER_STORE_ID,
                                       MAX(PS.VWH_ID) AS VWH_ID,
                                       MAX(PS.TOTAL_QUANTITY_ORDERED)           AS ORDERED_PIECES,
MAX(ps.iteration) as iteration,
MAX(ps.customer_id) AS customer_id,
                                       SUM(BD.CURRENT_PIECES)                   AS CURRENT_PIECES,
                                       COUNT(UNIQUE B.UCC128_ID)                AS BOX_COUNT,
                                       COUNT(UNIQUE CASE
                                               WHEN B.STOP_PROCESS_DATE IS NOT NULL AND
                                                    B.STOP_PROCESS_REASON = '$BOXCANCEL' THEN
                                                B.UCC128_ID END)                AS CANCELLED_BOXES,
                                       SUM(CASE
                                             WHEN B.STOP_PROCESS_DATE IS NOT NULL AND
                                                  B.STOP_PROCESS_REASON = '$BOXCANCEL' THEN
                                              BD.EXPECTED_PIECES END)           AS PIECES_IN_CANCELLED_BOXES,
MAX(bkt.freeze) as freeze,
MAX(ps.bucket_id) as bucket_id
                                  FROM <proxy />PS PS
                                 INNER JOIN <proxy />BUCKET BKT
                                    ON PS.BUCKET_ID = BKT.BUCKET_ID
                                 LEFT OUTER JOIN <proxy />BOX B
                                    ON B.PICKSLIP_ID = PS.PICKSLIP_ID
                                 LEFT OUTER JOIN <proxy />BOXDET BD
                                    ON B.PICKSLIP_ID = BD.PICKSLIP_ID
                                   AND B.UCC128_ID = BD.UCC128_ID
                                 WHERE ps.BUCKET_ID = :BUCKET_ID
                                   AND PS.TRANSFER_DATE IS NULL
                                 GROUP BY PS.PICKSLIP_ID";
            var binder = SqlBinder.Create(row => new Pickslip
            {
                PickslipId = row.GetLong("PICKSLIP_ID").Value,
                CustomerDcId = row.GetString("CUSTOMER_DC_ID"),
                PurchaseOrder = row.GetString("PO_ID"),
                VwhId = row.GetString("VWH_ID"),
                CustomerStoreId = row.GetString("CUSTOMER_STORE_ID"),
                OrderedPieces = row.GetInteger("ORDERED_PIECES") ?? 0,
                CurrentPieces = row.GetInteger("CURRENT_PIECES") ?? 0,
                CancelledBoxCount = row.GetInteger("CANCELLED_BOXES") ?? 0,
                PiecesInCancelledBoxes = row.GetInteger("PIECES_IN_CANCELLED_BOXES") ?? 0,
                BoxCount = row.GetInteger("BOX_COUNT") ?? 0,
                Iteration = row.GetInteger("iteration"),
                CustomerId = row.GetString("customer_id"),
                IsFrozenBucket = row.GetString("freeze") == "Y",
                BucketId = row.GetInteger("bucket_id") ?? 0
            });
            binder.Parameter("BUCKET_ID", bucketId);
            return _db.ExecuteReader(QUERY, binder, 2000);
        }

        /// <summary>
        /// Get boxes list of passed bucket
        /// </summary>
        /// <param name="bucketId"></param>
        /// <param name="stateFilter">InProgress filter returns empty boxes. Completed filter includes partially complete as well and therefore returns all non empty boxes</param>
        /// <param name="activityFilter"> </param>
        /// <returns></returns>
        public IList<Box> GetBucketBoxes(int bucketId)
        {
            const string QUERY = @"
                            SELECT B.UCC128_ID                                          AS UCC128_ID,
                                   MAX(B.IA_ID)                                         AS IA_ID,                                   
                                   MAX(B.VERIFY_DATE)                                   AS VERIFY_DATE,                               
                                   MAX(B.CARTON_ID)                                     AS CARTON_ID,
                                   MAX(B.STOP_PROCESS_DATE)                             AS CANCEL_DATE,
                                   MAX(B.PITCHING_END_DATE)                             AS PITCHING_END_DATE,
                                   SUM(BD.CURRENT_PIECES)                               AS CURRENT_PIECES,
                                   SUM(NVL(BD.EXPECTED_PIECES, BD.CURRENT_PIECES))      AS EXPECTED_PIECES,
                                   MAX(B.PICKSLIP_ID)                                   AS PICKSLIP_ID,
                                   MAX(B.CREATED_BY)                                    AS CREATED_BY,
                                   MAX(B.DATE_CREATED)                                  AS DATE_CREATED,
                                   MAX(B.VWH_ID)                                        AS VWH_ID
                              FROM <proxy />BOX B
                              LEFT OUTER JOIN <proxy />BOXDET BD
                                ON B.UCC128_ID = BD.UCC128_ID
                               AND B.PICKSLIP_ID = BD.PICKSLIP_ID
                              LEFT OUTER JOIN <proxy />PS P
                                ON B.PICKSLIP_ID = P.PICKSLIP_ID
                             WHERE P.BUCKET_ID = :BUCKET_ID
                               AND P.TRANSFER_DATE IS NULL
                                    <if c='$Pitching'>AND B.CARTON_ID IS NULL</if>
                                    <if c='$Pulling'>AND B.CARTON_ID IS NOT NULL</if>
                                AND (1 = 2 
                                    <if c='$All'>
                                OR 1 = 1
                                    </if>
                                    <else>
                                        <if c='$Completed'>OR (B.STOP_PROCESS_DATE IS NULL AND ((bd.expected_pieces IS NULL OR bd.expected_pieces &gt;= bd.current_pieces) OR B.VERIFY_DATE IS NOT NULL))</if>
                                        <if c='$InProgress'>OR (B.VERIFY_DATE IS NULL AND B.STOP_PROCESS_DATE IS NULL)</if>
                                        <if c='$Cancelled'>OR B.STOP_PROCESS_DATE IS NOT NULL</if>
                                    </else>
                                )
                             GROUP BY B.UCC128_ID
                             ORDER BY case when MAX(B.STOP_PROCESS_DATE) IS NOT NULL THEN 1 END NULLS FIRST,
                             CASE WHEN MAX(B.VERIFY_DATE) IS NOT NULL THEN 1 END NULLS FIRST,
                             CASE WHEN MAX(B.CARTON_ID) IS NOT NULL THEN 1 END NULLS FIRST,
                             CASE WHEN MAX(B.PITCHING_END_DATE) IS NOT NULL THEN 1 END NULLS FIRST
                           ";


            var binder = SqlBinder.Create(row => new Box
            {
                Ucc128Id = row.GetString("UCC128_ID"),
                AreaId = row.GetString("IA_ID"),
                CartonId = row.GetString("CARTON_ID"),
                VerifyDate = row.GetDateTimeOffset("VERIFY_DATE"),
                CancelDate = row.GetDateTimeOffset("CANCEL_DATE"),
                PitchingEndDate = row.GetDateTimeOffset("PITCHING_END_DATE"),
                ExpectedPieces = row.GetInteger("EXPECTED_PIECES"),
                CurrentPieces = row.GetInteger("CURRENT_PIECES"),
                CreatedDate = row.GetDateTimeOffset("DATE_CREATED").Value,
                CreatedBy = row.GetString("CREATED_BY"),
                PickslipId = row.GetLong("PICKSLIP_ID").Value,
                VWhId = row.GetString("VWH_ID")
            });
            binder.Parameter("BUCKET_ID", bucketId);
            binder.ParameterXPath("All", true);
            binder.TolerateMissingParams = true;
            return _db.ExecuteReader(QUERY, binder, 2000);
        }



        public int UpdatePriority(int bucketId, int delta)
        {
            //  throw new NotImplementedException();      
            const string QUERY = @"
                        UPDATE <proxy />BUCKET BKT
                            SET   BKT.PRIORITY          =  CASE WHEN GREATEST(NVL(BKT.PRIORITY, 0) + :delta, 1) > 99 THEN 99
                                                       ELSE GREATEST(NVL(BKT.PRIORITY, 0) + :delta, 1)
                                                     END
                         WHERE BKT.BUCKET_ID = :BUCKET_ID
                        RETURNING BKT.PRIORITY
                        INTO       :PRIORITY_OUT";
            var binder = SqlBinder.Create();
            binder.Parameter("delta", delta)
                .Parameter("BUCKET_ID", bucketId);
            binder.OutParameter("PRIORITY_OUT", p => delta = p ?? 0);
            int rows = _db.ExecuteDml(QUERY, binder);
            if (rows == 0)
            {
                // Invalid bucket id
                return 0;
            }
            return delta;
        }

        /// <summary> 
        /// Edit bucket property as Pull area , pitch area etc..        
        /// </summary>
        /// <param name="bucket"></param>
        /// <param name="flags"> </param>
        /// <returns>Updated bucket values. If the passed bucket does not exist, returns null.</returns>
        /// <remarks>
        /// Throws an exception if the pick wave is not frozen
        /// </remarks>
        internal BucketEditable UpdateWave(int bucketId, BucketEditable bucket)
        {
            const string QUERY = @"
                        UPDATE <proxy />BUCKET BKT
                            SET  BKT.NAME              = :NAME,           
                                 BKT.PITCH_IA_ID       = :PITCH_IA_ID,    
                                 BKT.BUCKET_COMMENT    = :BUCKET_COMMENT,                    
                                 BKT.PULL_CARTON_AREA  = :PULL_CARTON_AREA,  
                                 BKT.PULL_TYPE      = :PULL_TYPE,      
                                 BKT.PITCH_TYPE  = :PITCH_TYPE,       
                                 BKT.PITCH_LIMIT       = :PITCH_LIMIT,       
                                 BKT.DATE_MODIFIED = SYSDATE
                         WHERE BKT.BUCKET_ID = :BUCKET_ID
                        RETURNING BKT.NAME, 
                                  BKT.PITCH_IA_ID,
                                  BKT.BUCKET_COMMENT,
                                  BKT.PULL_CARTON_AREA,
                                  BKT.PULL_TYPE,
                                  BKT.PITCH_TYPE,
                                  BKT.PITCH_LIMIT,
BKT.FREEZE
                        INTO      :NAME_OUT,
                                  :PITCH_IA_ID_OUT,
                                  :BUCKET_COMMENT_OUT,
                                  :PULL_CARTON_AREA_OUT,
                                  :PULL_TYPE_OUT,
                                  :PITCH_TYPE_OUT,
                                  :PITCH_LIMIT_OUT,
:FREEZE_OUT
";
            var binder = SqlBinder.Create();
            binder.Parameter("NAME", bucket.BucketName)
                  .Parameter("PULL_CARTON_AREA", bucket.PullAreaId)
                  .Parameter("PITCH_IA_ID", bucket.PitchAreaId)
                  .Parameter("BUCKET_ID", bucketId)
                  .Parameter("PULL_TYPE", bucket.RequireBoxExpediting ? "EXP" : null)
                  .Parameter("PITCH_TYPE", bucket.QuickPitch ? "QUICK" : null)
                  .Parameter("PITCH_LIMIT", bucket.PitchLimit)
                  .Parameter("BUCKET_COMMENT", bucket.BucketComment);

            binder.OutParameter("NAME_OUT", p => bucket.BucketName = p)
                .OutParameter("BUCKET_COMMENT_OUT", p => bucket.BucketComment = p)
                .OutParameter("PITCH_LIMIT_OUT", p => bucket.PitchLimit = p ?? 0) //TODO
                .OutParameter("PULL_TYPE_OUT", p => bucket.RequireBoxExpediting = p == "EXP")
                .OutParameter("PITCH_TYPE_OUT", p => bucket.QuickPitch = p == "QUICK")
                .OutParameter("PITCH_IA_ID_OUT", p => bucket.PitchAreaId = p)
                .OutParameter("PULL_CARTON_AREA_OUT", p => bucket.PullAreaId = p)
                .OutParameter("FREEZE_OUT", p => bucket.IsFrozen = p == "Y");


            int rows = _db.ExecuteDml(QUERY, binder);
            if (rows == 0)
            {
                // Invalid bucket id
                throw new ArgumentOutOfRangeException("bucketId", "Could not update pick wave " + bucketId.ToString());
            }

            return bucket;
        }

        /// <summary>
        /// Create boxes of passed bucket
        /// </summary>
        /// <param name="bucketId"></param>
        /// <remarks>
        /// </remarks>
        internal void CreateBoxes(int bucketId)
        {
            const string QUERY = @"
                                DECLARE
                                    LPULL_AREA <proxy />BUCKET.PULL_CARTON_AREA%TYPE;
                                BEGIN
                                    SELECT B.PULL_CARTON_AREA
                                    INTO LPULL_AREA
                                    FROM <proxy />BUCKET B
                                    WHERE B.BUCKET_ID = :ABUCKET_ID;
                                IF LPULL_AREA IS NOT NULL THEN
                                    <proxy />PKG_BUCKET.MAKE_BUCKET_CTNRESV(ABUCKET_ID => :ABUCKET_ID);
                                END IF;
                                <proxy />PKG_BUCKET_PITCHING.CREATE_BOXES_FOR_BUCKET(ABUCKET_ID => :ABUCKET_ID);
                                END;
              ";
            var binder = SqlBinder.Create();
            binder.Parameter("ABUCKET_ID", bucketId);
            _db.ExecuteDml(QUERY, binder);
        }

        /// <summary>
        /// Delete boxes of bucket.
        /// </summary>
        /// <param name="bucketId"></param>
        internal void DeleteBoxes(int bucketId)
        {
            const string QUERY = @"                                
                                BEGIN
                                    <proxy />PKG_BUCKET.DELETE_BUCKET_CTNRESV(:ABUCKET_ID);                                    
                                END;
              ";
            var binder = SqlBinder.Create();
            binder.Parameter("ABUCKET_ID", bucketId);
            _db.ExecuteDml(QUERY, binder);
        }

        /// <summary>
        /// Set bucket freeze or unfreeze.
        /// TODO:If you ask us to freeze a completed bucket, we will do nothing because a completed bucket is already as good as frozen.
        /// </summary>
        /// <param name="bucketId"></param>
        /// <param name="freeze"></param>
        public void SetFreezeStatus(int bucketId, bool freeze)
        {
            const string QUERY = @"
                        UPDATE <proxy />BUCKET BKT
                            SET BKT.FREEZE            = :FREEZE
                         WHERE BKT.BUCKET_ID = :BUCKET_ID ";
            var binder = SqlBinder.Create();
            binder.Parameter("FREEZE", freeze ? "Y" : null)
                  .Parameter("BUCKET_ID", bucketId);
            _db.ExecuteDml(QUERY, binder);
        }

        /// <summary>
        /// Removed passed pickslip from bucket
        /// Sharad Sir and Shiva : Delete Bucket when last pickslip is removed.
        /// </summary>
        /// <param name="pickslips"></param>
        /// <param name="bucketId"></param>
        public void RemovePickslipFromBucket(IList<long> pickslips, int bucketId)
        {
            const string QUERY = @"
                                DECLARE
                                      LPICKSLIP_COUNT NUMBER;
                                    BEGIN
                                      <proxy />PKG_BUCKET.DELETE_PS_CTNRESV(APICKSLIP_ID => :APICKSLIP_ID);
                                      <proxy />PKG_DATA_EXCHANGE.REVERT_PICKSLIP(APICKSLIP_ID => :APICKSLIP_ID);

                                      SELECT COUNT(P.PICKSLIP_ID)
                                        INTO LPICKSLIP_COUNT
                                        FROM <proxy />PS P
                                       WHERE P.BUCKET_ID = :BUCKET_ID
                                         AND P.TRANSFER_DATE IS NULL;

                                      IF LPICKSLIP_COUNT = 0 THEN
                                        DELETE FROM <proxy />BUCKET BKT WHERE BKT.BUCKET_ID = :BUCKET_ID;
                                      END IF;
                                    END;
                                ";

            var binder = SqlBinder.Create(pickslips.Count);
            binder.Parameter("APICKSLIP_ID", pickslips)
               .Parameter("BUCKET_ID", Enumerable.Repeat(bucketId, pickslips.Count));
            _db.ExecuteDml(QUERY, binder);

        }

        /// <summary>
        /// Area list as pull area, pitch area.
        /// TODO : Change query ?? WE USE IA.PICKING_AREA_FLAG.
        /// </summary>
        /// <param name="bucketId"></param>
        /// <returns></returns>
        public IList<BucketArea> GetBucketAreas(int bucketId)
        {
            const string QUERY = @"
                            WITH ORDERED_SKU AS
                                 (SELECT MAX(PD.UPC_CODE) AS UPC_CODE, SKU.SKU_ID, P.VWH_ID
                                    FROM <proxy />PS P
                                   INNER JOIN <proxy />PSDET PD
                                      ON P.PICKSLIP_ID = PD.PICKSLIP_ID
                                   INNER JOIN <proxy />MASTER_SKU SKU
                                      ON SKU.UPC_CODE = PD.UPC_CODE
                                   WHERE P.BUCKET_ID = :BUCKET_ID
                                     AND P.TRANSFER_DATE IS NULL
                                     AND PD.TRANSFER_DATE IS NULL
                                   GROUP BY SKU.SKU_ID, P.VWH_ID),
                                CARTON_AREAS AS
                                 (SELECT CTN.CARTON_STORAGE_AREA, COUNT(UNIQUE OS.SKU_ID) AS COUNT_SKU
                                    FROM <proxy />SRC_CARTON CTN
                                   INNER JOIN <proxy />SRC_CARTON_DETAIL CTNDET
                                      ON CTN.CARTON_ID = CTNDET.CARTON_ID
                                    LEFT OUTER JOIN ORDERED_SKU OS
                                      ON OS.SKU_ID = CTNDET.SKU_ID
                                     AND OS.VWH_ID = CTN.VWH_ID
                                   GROUP BY CTN.CARTON_STORAGE_AREA),
                        PICK_AREAS AS
                            (SELECT IALOC.IA_ID, COUNT(UNIQUE OS.SKU_ID) AS COUNT_SKU
                            FROM <proxy />IALOC IALOC
                            LEFT OUTER JOIN ORDERED_SKU OS
                                ON OS.UPC_CODE = IALOC.ASSIGNED_UPC_CODE
                                AND OS.VWH_ID = IALOC.VWH_ID
                            GROUP BY IALOC.IA_ID)
                        SELECT :PULL_AREA_TYPE AS AREA_TYPE,
                                TIA.INVENTORY_STORAGE_AREA AS INVENTORY_STORAGE_AREA,
                                TIA.DESCRIPTION AS DESCRIPTION,
                                TIA.SHORT_NAME AS SHORT_NAME,
                                TIA.WAREHOUSE_LOCATION_ID AS WAREHOUSE_LOCATION_ID,
                                CA.COUNT_SKU AS COUNT_SKU,
                                (SELECT COUNT(UNIQUE SKU_ID) FROM ORDERED_SKU) AS COUNT_ORDERED_SKU
                            FROM <proxy />TAB_INVENTORY_AREA TIA
                            INNER JOIN CARTON_AREAS CA
                                ON CA.CARTON_STORAGE_AREA = TIA.INVENTORY_STORAGE_AREA                            
                        UNION ALL

                        SELECT :PITCH_AREA_TYPE AS AREA_TYPE,
                               I.IA_ID AS INVENTORY_STORAGE_AREA,
                               I.SHORT_DESCRIPTION AS DESCRIPTION,
                               I.SHORT_NAME AS SHORT_NAME,
                               I.WAREHOUSE_LOCATION_ID AS WAREHOUSE_LOCATION_ID,
                               CA.COUNT_SKU AS COUNT_SKU,
                               (SELECT COUNT(UNIQUE SKU_ID) FROM ORDERED_SKU) AS COUNT_ORDERED_SKU
                          FROM <proxy />IA I
                         INNER JOIN PICK_AREAS CA
                            ON CA.IA_ID = I.IA_ID
                         WHERE I.PICKING_AREA_FLAG = 'Y'";

            var binder = SqlBinder.Create(row => new BucketArea
            {
                AreaId = row.GetString("INVENTORY_STORAGE_AREA"),
                ShortName = row.GetString("SHORT_NAME"),
                Description = row.GetString("DESCRIPTION"),
                BuildingId = row.GetString("WAREHOUSE_LOCATION_ID"),
                AreaType = row.GetEnum<BucketActivityType>("AREA_TYPE"),
                CountSku = row.GetInteger("COUNT_SKU"),
                CountOrderedSku = row.GetInteger("COUNT_ORDERED_SKU")
            });

            binder.Parameter("PITCH_AREA_TYPE", BucketActivityType.Pitching.ToString())
                  .Parameter("PULL_AREA_TYPE", BucketActivityType.Pulling.ToString())
                  .Parameter("BUCKET_ID", bucketId);
            return _db.ExecuteReader(QUERY, binder);
        }

        internal void CancelBoxes(IList<string> boxes)
        {
            const string QUERY = @"
                BEGIN
                <proxy />pkg_pickslip.cancel_box(aucc128_id => :aucc128_id);
                                  END;";
            var binder = SqlBinder.Create(boxes.Count);
            binder.Parameter("aucc128_id", boxes);
            // .Parameter("aucc128_id", ucc128Id);
            _db.ExecuteDml(QUERY, binder);
        }

        public IList<CustomerBucket> GetBucketList(string customerId, string userName)
        {
            //string bucketState = null;
            //switch (state)
            //{
            //    case ProgressStage.Frozen:
            //        bucketState = "Frozen";
            //        break;
            //    case ProgressStage.InProgress:
            //        bucketState = "InProgress";
            //        break;
            //    case ProgressStage.Unknown:
            //        throw new ArgumentOutOfRangeException("state");
            //}
            //            const string QUERY = @"WITH BUCKET_INFO AS
            //                     (SELECT BK.BUCKET_ID,
            //                             BK.NAME,
            //                             BK.BUCKET_COMMENT,
            //                             BK.PRIORITY,
            //                             BK.FREEZE,
            //                             BK.PITCH_LIMIT,
            //                             BK.PULL_TYPE,
            //                             CASE
            //                               WHEN BK.PITCH_TYPE = 'QUICK' THEN
            //                                'Y'
            //                             END AS QUICK_PITCH_FLAG,
            //                             BK.DATE_CREATED,
            //                             BK.CREATED_BY,
            //                             BK.PULL_CARTON_AREA AS PULL_AREA_ID,
            //                             TIA.SHORT_NAME AS PULL_AREA_SHORT_NAME,
            //                             TIA.DESCRIPTION AS PULL_AREA_DESCRIPTION,
            //                             TIA.WAREHOUSE_LOCATION_ID AS BUILDING_PULL_FROM,
            //                             BK.PITCH_IA_ID AS PITCH_IA_ID,
            //         
            //                             IA.SHORT_NAME            AS PITCH_AREA_SHORT_NAME,
            //                             IA.SHORT_DESCRIPTION     AS PITCH_AREA_DESCRIPTION,
            //                             IA.WAREHOUSE_LOCATION_ID AS BUILDING_PITCH_FROM,
            //                             IA.DEFAULT_REPREQ_IA_ID  AS DEFAULT_REPREQ_IA_ID
            //                        FROM BUCKET BK
            //                        LEFT OUTER JOIN TAB_INVENTORY_AREA TIA
            //                          ON TIA.INVENTORY_STORAGE_AREA = BK.PULL_CARTON_AREA
            //                        LEFT OUTER JOIN IA IA
            //                          ON IA.IA_ID = BK.PITCH_IA_ID),
            //                    BUCKET_PICKSLIP_INFO AS
            //                     (SELECT PS.BUCKET_ID,
            //                             COUNT(PS.PICKSLIP_ID) AS PICKSLIP_COUNT,
            //                             COUNT(UNIQUE PS.PO_ID) AS PO_COUNT,
            //                             SUM(PS.TOTAL_QUANTITY_ORDERED) AS ORDERED_PIECES,
            //                             MIN(PO.DC_CANCEL_DATE) AS MIN_DC_CANCEL_DATE,
            //                             MAX(PO.DC_CANCEL_DATE) AS MAX_DC_CANCEL_DATE
            //                        FROM PS PS
            //                       INNER JOIN PO PO
            //                          ON PS.CUSTOMER_ID = PO.CUSTOMER_ID
            //                         AND PS.ITERATION = PO.ITERATION
            //                         AND PS.PO_ID = PO.PO_ID
            //                       WHERE PS.TRANSFER_DATE IS NULL
            //                         AND PS.CUSTOMER_ID = :CUSTOMER_ID
            //  
            //                       GROUP BY PS.BUCKET_ID)
            //                    SELECT Q1.BUCKET_ID             AS BUCKET_ID,
            //                           Q1.NAME                  AS BUCKET_NAME,
            //                           Q1.BUCKET_COMMENT        AS BUCKET_COMMENT,
            //                           Q1.PRIORITY              AS PRIORITY,
            //                           Q1.FREEZE                AS FREEZE,
            //                           Q1.PITCH_LIMIT           AS PITCH_LIMIT,
            //                           Q1.PULL_TYPE             AS PULL_TYPE,
            //                           Q1.QUICK_PITCH_FLAG AS QUICK_PITCH_FLAG,
            //                           Q1.DATE_CREATED AS DATE_CREATED,
            //                           Q1.CREATED_BY AS CREATED_BY,
            //                           Q1.PULL_AREA_ID          AS PULL_AREA_ID,
            //                           Q1.PULL_AREA_SHORT_NAME  AS PULL_AREA_SHORT_NAME,
            //                           Q1.PULL_AREA_DESCRIPTION AS PULL_AREA_DESCRIPTION,
            //                           Q1.BUILDING_PULL_FROM    AS BUILDING_PULL_FROM,
            //                           Q1.PITCH_IA_ID           AS PITCH_IA_ID,
            //       
            //                           Q1.PITCH_AREA_SHORT_NAME  AS PITCH_AREA_SHORT_NAME,
            //                           Q1.PITCH_AREA_DESCRIPTION AS PITCH_AREA_DESCRIPTION,
            //                           Q1.BUILDING_PITCH_FROM    AS BUILDING_PITCH_FROM,
            //                           Q1.DEFAULT_REPREQ_IA_ID AS REPLENISH_AREA_ID,
            //                           Q2.ORDERED_PIECES AS ORDERED_PIECES,
            //                           Q2.PICKSLIP_COUNT AS PICKSLIP_COUNT,
            //                           Q2.PO_COUNT AS PO_COUNT,
            //                           Q2.MIN_DC_CANCEL_DATE AS MIN_DC_CANCEL_DATE,
            //                           Q2.MAX_DC_CANCEL_DATE AS MAX_DC_CANCEL_DATE
            //
            //                      FROM BUCKET_INFO Q1
            //                     INNER JOIN BUCKET_PICKSLIP_INFO Q2
            //                        ON Q2.BUCKET_ID = Q1.BUCKET_ID 
            //   <if>
            //                            WHERE (CASE
            //                                WHEN Q1.FREEZE = 'Y' THEN       :FrozenState                             
            //                                ELSE                            :InProgressState
            //                                END) = :state
            //                            </if>                          
            //                <if>AND Q1.CREATED_BY = :CREATED_BY</if>";

            const string QUERY = @"
with q1 as
 (SELECT row_number() over(partition by bk.bucket_id order by bk.bucket_id) as row_seq_,
         SUM(p.total_quantity_ordered) over(partition by p.bucket_id) as ORDERED_PIECES,
         COUNT(p.pickslip_id) over(partition by p.bucket_id) as PICKSLIP_COUNT,
         COUNT(unique p.po_id) over(partition by p.bucket_id) as PO_COUNT,
         MIN(po.dc_cancel_date) over(partition by p.bucket_id) as MIN_DC_CANCEL_DATE,
         MAX(po.dc_cancel_date) over(partition by p.bucket_id) as MAX_DC_CANCEL_DATE,
         BK.BUCKET_ID as BUCKET_ID,
         BK.NAME as BUCKET_NAME,
         BK.BUCKET_COMMENT as BUCKET_COMMENT,
         BK.PRIORITY as PRIORITY,
         BK.FREEZE as FREEZE,
         BK.PITCH_LIMIT as PITCH_LIMIT,
         BK.PULL_TYPE as PULL_TYPE,
         CASE
           WHEN BK.PITCH_TYPE = 'QUICK' THEN
            'Y'
         END AS QUICK_PITCH_FLAG,
         BK.DATE_CREATED as DATE_CREATED,
         BK.CREATED_BY as CREATED_BY,
         BK.PULL_CARTON_AREA AS PULL_AREA_ID,
         TIA.SHORT_NAME AS PULL_AREA_SHORT_NAME,
         TIA.DESCRIPTION AS PULL_AREA_DESCRIPTION,
         TIA.WAREHOUSE_LOCATION_ID AS BUILDING_PULL_FROM,
         BK.PITCH_IA_ID AS PITCH_IA_ID,
         
         IA.SHORT_NAME            AS PITCH_AREA_SHORT_NAME,
         IA.SHORT_DESCRIPTION     AS PITCH_AREA_DESCRIPTION,
         IA.WAREHOUSE_LOCATION_ID AS BUILDING_PITCH_FROM,
         IA.DEFAULT_REPREQ_IA_ID  AS REPLENISH_AREA_ID
    FROM <proxy/>BUCKET BK
   inner join <proxy/>ps p
      on p.bucket_id = bk.bucket_id
    left outer join <proxy/>po po
      on po.po_id = p.po_id
     and po.customer_id = p.customer_id
     and po.iteration = p.iteration
    LEFT OUTER JOIN <proxy/>TAB_INVENTORY_AREA TIA
      ON TIA.INVENTORY_STORAGE_AREA = BK.PULL_CARTON_AREA
    LEFT OUTER JOIN <proxy/>IA IA
      ON IA.IA_ID = BK.PITCH_IA_ID
   WHERE P.TRANSFER_DATE IS NULL
     AND P.CUSTOMER_ID = :CUSTOMER_ID
                <if>AND Q1.CREATED_BY = :CREATED_BY</if>
)
select * from q1 where row_seq_ = 1

";

            var binder = SqlBinder.Create(row => new CustomerBucket
            {

                BucketId = row.GetInteger("BUCKET_ID").Value,
                BucketName = row.GetString("BUCKET_NAME"),
                BucketComment = row.GetString("BUCKET_COMMENT"),
                PriorityId = row.GetInteger("PRIORITY").Value,
                IsFrozen = row.GetString("FREEZE") == "Y",
                PitchLimit = row.GetInteger("PITCH_LIMIT"),
                RequireBoxExpediting = row.GetString("PULL_TYPE") == "EXP",
                CreatedBy = row.GetString("CREATED_BY"),
                CreationDate = row.GetDate("DATE_CREATED").Value,
                QuickPitch = row.GetString("QUICK_PITCH_FLAG") == "Y",
                OrderedPieces = row.GetInteger("ORDERED_PIECES") ?? 0,
                CountPurchaseOrder = row.GetInteger("PO_COUNT") ?? 0,
                CountPickslips = row.GetInteger("PICKSLIP_COUNT").Value,
                MinDcCancelDate = row.GetDate("MIN_DC_CANCEL_DATE"),
                MaxDcCancelDate = row.GetDate("MAX_DC_CANCEL_DATE"),


                PitchAreaId = row.GetString("PITCH_IA_ID"),
                PitchAreaShortName = row.GetString("PITCH_AREA_SHORT_NAME"),
                PitchAreaDescription = row.GetString("PITCH_AREA_DESCRIPTION"),
                PitchAreaBuildingId = row.GetString("BUILDING_PITCH_FROM"),
                ReplenishAreaId = row.GetString("REPLENISH_AREA_ID"),


                PullAreaId = row.GetString("PULL_AREA_ID"),
                PullAreaShortName = row.GetString("PULL_AREA_SHORT_NAME"),
                PullAreaDescription = row.GetString("PULL_AREA_DESCRIPTION"),
                PullAreaBuildingId = row.GetString("BUILDING_PULL_FROM")

            });

            binder.Parameter("CUSTOMER_ID", customerId)
                  .Parameter("CREATED_BY", userName)
                //.Parameter("state", bucketState)
                  .Parameter("FrozenState", ProgressStage.Frozen.ToString())
                  .Parameter("InProgressState", ProgressStage.InProgress.ToString());
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
