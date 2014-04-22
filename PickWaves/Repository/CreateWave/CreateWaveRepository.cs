using DcmsMobile.PickWaves.Helpers;
using EclipseLibrary.Oracle;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace DcmsMobile.PickWaves.Repository.CreateWave
{
    public class CreateWaveRepository : PickWaveRepositoryBase
    {
        #region Intialization

        public CreateWaveRepository(TraceContext ctx, string userName, string clientInfo)
            : base(ctx, userName, clientInfo)
        {
        }

        #endregion

        /// <summary>
        /// Returns the pickslips of specified customer from dem_pickslip
        /// </summary>
        /// <param name="customerId"> </param>
        /// <param name="dimensions"> </param>
        /// <returns></returns>
        public IEnumerable<Pickslip> GetPickslips(string customerId, IList<Tuple<PickslipDimension, object>> dimensions)
        {
            if (string.IsNullOrEmpty(customerId))
            {
                throw new ArgumentNullException("customerId");
            }

            const string QUERY = @"
            SELECT DEMPS.PICKSLIP_ID                AS PICKSLIP_ID,
                   DEMPS.DC_CANCEL_DATE             AS DC_CANCEL_DATE,
                   DEMPS.CANCEL_DATE                AS CANCEL_DATE,
                   DEMPS.Delivery_Date              AS Delivery_Date,
                   DEMPS.PICKSLIP_IMPORT_DATE       AS PICKSLIP_IMPORT_DATE,
                   DEMPS.CUSTOMER_ORDER_ID          AS CUSTOMER_ORDER_ID,
                   DEMPS.CUSTOMER_ID                AS CUSTOMER_ID,
                   DEMPS.CUSTOMER_STORE_ID          AS CUSTOMER_STORE_ID,
                   DEMPS.VWH_ID                     AS VWH_ID,
                   DEMPS.CUSTOMER_DIST_CENTER_ID    AS CUSTOMER_DIST_CENTER_ID
              FROM <proxy />DEM_PICKSLIP DEMPS
             WHERE DEMPS.PS_STATUS_ID = 1   
               AND DEMPS.CUSTOMER_ID = :CUSTOMER_ID           
               AND {0}
            ORDER BY DEMPS.PRIORITY_ID, DEMPS.PICKSLIP_ID DESC";

            var binder = SqlBinder.Create(row => new Pickslip
            {
                PickslipId = row.GetLong("PICKSLIP_ID").Value,
                DcCancelDate = row.GetDate("DC_CANCEL_DATE"),
                CancelDate = row.GetDate("CANCEL_DATE"),
                StartDate = row.GetDate("Delivery_Date"),
                PickslipImportDate = row.GetDate("PICKSLIP_IMPORT_DATE"),
                PurchaseOrder = row.GetString("CUSTOMER_ORDER_ID"),
                CustomerId = row.GetString("CUSTOMER_ID"),
                CustomerStoreId = row.GetString("CUSTOMER_STORE_ID"),
                VwhId = row.GetString("VWH_ID"),
                CustomerDcId = row.GetString("CUSTOMER_DIST_CENTER_ID")
            }).Parameter("CUSTOMER_ID", customerId);

            var attrs = PickWaveHelpers.GetEnumMemberAttributes<PickslipDimension, DataTypeAttribute>();
            var clauses = new List<string>(2);
            foreach (var dim in dimensions)
            {
                clauses.Add(GetDimensionWhereClause(dim.Item1, dim.Item2));
                if (attrs.ContainsKey(dim.Item1) && attrs[dim.Item1].DataType == DataType.Date)
                {
                    binder.Parameter(dim.Item1.ToString(), Convert.ToDateTime(dim.Item2));
                }
                else
                {
                    binder.Parameter(dim.Item1.ToString(), Convert.ToString(dim.Item2));
                }
            }
            var queryFinal = string.Format(QUERY, string.Join(" AND ", clauses));
            return _db.ExecuteReader(queryFinal, binder, 2000);
        }

        /// <summary>
        /// Returns the where clause corresponding to the passed dimension
        /// </summary>
        /// <param name="dim"></param>
        /// <param name="dimVal"></param>
        /// <returns></returns>
        /// <remarks>
        /// The where clause assumes that the alias of the dem_pickslip table is DEMPS.
        /// </remarks>
        private string GetDimensionWhereClause(PickslipDimension dim, object dimVal)
        {
            if (dimVal == null)
            {
                throw new ArgumentNullException("dimVal");
            }
            string clause;
            switch (dim)
            {
                case PickslipDimension.Priority:
                    clause = "DEMPS.PRIORITY_ID = :{0}";
                    break;

                case PickslipDimension.CustomerStore:
                    clause = "DEMPS.CUSTOMER_STORE_ID = :{0}";
                    break;

                case PickslipDimension.Label:
                    clause = "DEMPS.Pickslip_Type = :{0}";
                    break;

                case PickslipDimension.ImportDate:
                    clause = "DEMPS.PICKSLIP_IMPORT_DATE BETWEEN CAST(:{0} AS DATE) AND CAST(:{0} + 1 AS DATE)";
                    break;

                case PickslipDimension.StartDate:
                    clause = "DEMPS.DELIVERY_DATE BETWEEN CAST(:{0} AS DATE) AND CAST(:{0} + 1 AS DATE)";
                    break;

                case PickslipDimension.CancelDate:
                    clause = "DEMPS.CANCEL_DATE BETWEEN CAST(:{0} AS DATE) AND CAST(:{0} + 1 AS DATE)";
                    break;

                case PickslipDimension.CustomerOrderType:
                    clause = "DEMPS.CUSTOMER_ORDER_TYPE = :{0}";
                    break;

                case PickslipDimension.SaleTypeId:
                    clause = "DEMPS.SALES_TYPE_ID = :{0}";
                    break;

                case PickslipDimension.PurchaseOrder:
                    clause = "DEMPS.CUSTOMER_ORDER_ID = :{0}";
                    break;

                case PickslipDimension.CustomerDcCancelDate:
                    clause = "DEMPS.DC_CANCEL_DATE BETWEEN CAST(:{0} AS DATE) AND CAST(:{0} + 1 AS DATE)";
                    break;

                case PickslipDimension.CustomerDc:
                    clause = "DEMPS.CUSTOMER_DIST_CENTER_ID = :{0}";
                    break;

                default:
                    throw new NotImplementedException();
            }
            clause = string.Format(clause, dim.ToString());
            return clause;
        }

        /// <summary>
        /// The results are returned ordered by the dimension
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="vwhId"> </param>
        /// <param name="dimRow"></param>
        /// <param name="dimCol"> </param>
        /// <returns></returns>
        /// <remarks>
        /// If too many rows for the dimension are returned, then null is returned. If no rows are returned for the dimension, and empty collection is returned.
        /// Thus null is different from empty.
        /// </remarks>
        internal IList<CustomerOrderSummary> GetOrderSummaryForCustomer(string customerId, string vwhId, PickslipDimension dimRow, PickslipDimension dimCol)
        {
            const string QUERY = @"
         WITH Q1 AS
             (SELECT PICKSLIP_ID,
                     <if c='$DIMENSION=""Priority""'> T.PRIORITY_ID </if>
                     <if c='$DIMENSION=""CustomerStore""'> T.CUSTOMER_STORE_ID </if>
                     <if c='$DIMENSION=""CustomerDcCancelDate""'> (T.DC_CANCEL_DATE)</if>
                     <if c='$DIMENSION=""Label""'> T.PICKSLIP_TYPE</if>
                     <if c='$DIMENSION=""ImportDate""'> TRUNC(T.PICKSLIP_IMPORT_DATE)</if>
                     <if c='$DIMENSION=""StartDate""'> TRUNC(T.DELIVERY_DATE)</if>
                     <if c='$DIMENSION=""CancelDate""'> TRUNC(T.CANCEL_DATE)</if> 
                     <if c='$DIMENSION=""CustomerOrderType""'> T.CUSTOMER_ORDER_TYPE</if> 
                     <if c='$DIMENSION=""SaleTypeId""'> T.SALES_TYPE_ID</if>
                     <if c='$DIMENSION=""PurchaseOrder""'> T.CUSTOMER_ORDER_ID</if> 
            <if c='$DIMENSION=""CustomerDc""'> T.CUSTOMER_DIST_CENTER_ID</if> 
               AS PICKSLIP_DIMENSION,
                     <if c='$DIMENSION_COL=""Priority""'> T.PRIORITY_ID </if>
                     <if c='$DIMENSION_COL=""CustomerStore""'> T.CUSTOMER_STORE_ID </if>
                     <if c='$DIMENSION_COL=""CustomerDcCancelDate""'> (T.DC_CANCEL_DATE)</if>
                     <if c='$DIMENSION_COL=""Label""'> T.PICKSLIP_TYPE</if>
                     <if c='$DIMENSION_COL=""ImportDate""'> TRUNC(T.PICKSLIP_IMPORT_DATE)</if>
                     <if c='$DIMENSION_COL=""StartDate""'> TRUNC(T.DELIVERY_DATE)</if>
                     <if c='$DIMENSION_COL=""CancelDate""'> TRUNC(T.CANCEL_DATE)</if> 
                     <if c='$DIMENSION_COL=""CustomerOrderType""'> T.CUSTOMER_ORDER_TYPE</if> 
                     <if c='$DIMENSION_COL=""SaleTypeId""'> T.SALES_TYPE_ID</if>
                     <if c='$DIMENSION_COL=""PurchaseOrder""'> T.CUSTOMER_ORDER_ID</if> 
         <if c='$DIMENSION_COL=""CustomerDc""'> T.CUSTOMER_DIST_CENTER_ID</if> 
              AS DIM_COL,
                     COUNT(UNIQUE T.PRIORITY_ID) OVER()                     AS COUNT_PRIORITY_ID,
                     COUNT(UNIQUE T.CUSTOMER_STORE_ID) OVER()               AS COUNT_CUSTOMER_STORE_ID,
                     COUNT(UNIQUE TRUNC(T.DC_CANCEL_DATE)) OVER()           AS COUNT_DC_CANCEL_DATE,                     
                     COUNT(UNIQUE T.PICKSLIP_TYPE) OVER()                   AS COUNT_PICKSLIP_TYPE,
                     COUNT(UNIQUE TRUNC(T.PICKSLIP_IMPORT_DATE)) OVER()     AS COUNT_IMPORT_DATES,
                     COUNT(UNIQUE TRUNC(T.DELIVERY_DATE)) OVER()            AS COUNT_DELIVERY_DATE,
                     COUNT(UNIQUE TRUNC(T.CANCEL_DATE)) OVER()              AS COUNT_CANCEL_DATE,
                     COUNT(UNIQUE T.CUSTOMER_ORDER_TYPE) OVER()             AS COUNT_CUSTOMER_ORDER_TYPE,
                     COUNT(UNIQUE T.SALES_TYPE_ID) OVER()                   AS COUNT_SALES_TYPE_ID,
                     COUNT(UNIQUE T.CUSTOMER_ORDER_ID) OVER()               AS COUNT_CUSTOMER_ORDER_ID,
                     COUNT(UNIQUE T.CUSTOMER_DIST_CENTER_ID) OVER()         AS COUNT_CUSTOMER_DIST_CENTER_ID
                FROM <proxy />DEM_PICKSLIP T
               WHERE T.PS_STATUS_ID = 1
                 AND T.CUSTOMER_ID = :CUSTOMER_ID
             <if>AND T.VWH_ID = :VWH_ID</if>)
            SELECT *
              FROM Q1 PIVOT XML(COUNT(PICKSLIP_ID) AS PICKSLIP_COUNT FOR DIM_COL IN(ANY))
             ORDER BY PICKSLIP_DIMENSION
        ";
            var dict = PickWaveHelpers.GetEnumMemberAttributes<PickslipDimension, DataTypeAttribute>();
            var isRowDate = dict.ContainsKey(dimRow) && dict[dimRow].DataType == DataType.Date;
            var isColDate = dict.ContainsKey(dimCol) && dict[dimCol].DataType == DataType.Date;

            var binder = SqlBinder.Create(row => new CustomerOrderSummary
            {
                Counts = new Dictionary<PickslipDimension, int>
                    {
                        {PickslipDimension.CancelDate, row.GetInteger("COUNT_CANCEL_DATE") ?? 0},
                        {PickslipDimension.Priority, row.GetInteger("COUNT_PRIORITY_ID") ?? 0},
                        {PickslipDimension.CustomerStore, row.GetInteger("COUNT_CUSTOMER_STORE_ID") ?? 0},
                        {PickslipDimension.CustomerDcCancelDate, row.GetInteger("COUNT_DC_CANCEL_DATE") ?? 0},
                        {PickslipDimension.Label, row.GetInteger("COUNT_PICKSLIP_TYPE")  ?? 0},
                        {PickslipDimension.ImportDate, row.GetInteger("COUNT_IMPORT_DATES")  ?? 0},
                        {PickslipDimension.StartDate, row.GetInteger("COUNT_DELIVERY_DATE")  ?? 0},
                        {PickslipDimension.CustomerOrderType, row.GetInteger("COUNT_CUSTOMER_ORDER_TYPE")  ?? 0},
                        {PickslipDimension.SaleTypeId, row.GetInteger("COUNT_SALES_TYPE_ID")  ?? 0},
                        {PickslipDimension.PurchaseOrder, row.GetInteger("COUNT_CUSTOMER_ORDER_ID")  ?? 0},
                        {PickslipDimension.CustomerDc, row.GetInteger("COUNT_CUSTOMER_DIST_CENTER_ID")  ?? 0}                        
                    },
                DimensionValue = isRowDate ? (object)row.GetDate("pickslip_dimension") : (object)row.GetString("pickslip_dimension"),
                Data = MapOrderSummaryXml(row.GetXml("DIM_COL_XML"), isColDate)
            });
            binder.Parameter("CUSTOMER_ID", customerId)
                .Parameter("VWH_ID", vwhId)
                .Parameter("DIMENSION", dimRow.ToString())
                .Parameter("DIMENSION_COL", dimCol.ToString())
                ;
            return _db.ExecuteReader(QUERY, binder);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xml"></param>
        /// <param name="isColDate"> </param>
        /// <returns></returns>
        /// <remarks>
        /// <code>
        /// <![CDATA[
        /// <PivotSet>
        ///   <item>
        ///     <column name = "CUSTOMER_DIST_CENTER_ID">81234</column>
        ///     <column name = "PICKSLIP_COUNT">647</column>
        ///     <column name = "PO_COUNT">8</column>
        ///   </item>
        ///   <item>
        ///     <column name = "CUSTOMER_DIST_CENTER_ID">82567</column>
        ///     <column name = "PICKSLIP_COUNT">35</column>
        ///     <column name = "PO_COUNT">3</column>
        ///   </item>
        /// </PivotSet>  
        /// ]]>
        /// </code>
        /// </remarks>
        private IDictionary<object, int> MapOrderSummaryXml(XElement xml, bool isColDate)
        {
            var query = (from item in xml.Elements("item")
                        let column = item.Elements("column")
                        select new
                        {
                            ColElement = column.First(p => p.Attribute("name").Value == "DIM_COL"),
                            PickslipCount = (int)column.First(p => p.Attribute("name").Value == "PICKSLIP_COUNT")
                        });
            if (isColDate)
            {
                return query.ToDictionary(p => (object)(DateTime?)p.ColElement, p => p.PickslipCount);
            }
            return query.ToDictionary(p => (object)(string)p.ColElement, p => p.PickslipCount);
        }

        public int CreateWave(PickWaveEditable bucket)
        {
            if (bucket == null)
            {
                throw new ArgumentNullException("bucket");
            }
            const string QUERY = @"  
                                    INSERT INTO <proxy />BUCKET BKT
                                        (BKT.BUCKET_ID,
                                         BKT.PITCH_TYPE,
                                         BKT.NAME,
                                         BKT.PITCH_IA_ID,
                                         BKT.PRIORITY,
                                         BKT.QUICK_PITCH_FLAG,
                                         BKT.PULL_CARTON_AREA,
                                         BKT.FREEZE,
                                         BKT.PULL_TO_DOCK)
                                      VALUES
                                        (<proxy />BUCKET_SEQUENCE.NEXTVAL,
                                         :PITCH_TYPE,
                                         SUBSTR(:NAME, 1, 50),
                                         :PITCH_IA_ID,
                                         :PRIORITY,
                                         :QUICK_PITCH_FLAG,
                                         :PULL_CARTON_AREA,
                                         'Y',
                                         :PULL_TO_DOCK)
                                      RETURNING BUCKET_ID INTO :BUCKET_ID
              ";
            var binder = SqlBinder.Create();
            binder.Parameter("PITCH_TYPE", "BOX")
                  .Parameter("NAME", bucket.BucketName)
                  .Parameter("PITCH_IA_ID", bucket.PitchAreaId)
                  .Parameter("PRIORITY", bucket.PriorityId)
                  .Parameter("PULL_CARTON_AREA", bucket.PullAreaId)
                  .Parameter("QUICK_PITCH_FLAG", bucket.QuickPitch ? "Y" : null)
                  .Parameter("PULL_TO_DOCK", bucket.PullingBucket)
                  ;
            var bucketId = 0;
            binder.OutParameter("BUCKET_ID", val => bucketId = val.Value);
            _db.ExecuteDml(QUERY, binder);
            return bucketId;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bucketId"></param>
        /// <param name="customerId"></param>
        /// <param name="dimensions"></param>
        /// <param name="updateBucketName"></param>
        public void AddPickslipsPerDim(int bucketId, string customerId, IList<Tuple<PickslipDimension, object>> dimensions,string vwhId, bool updateBucketName)
        {
            const string QUERY = @"
                                    DECLARE
                                        LBUCKET_NAME <proxy />BUCKET.NAME%TYPE;
                                        CURSOR PICKSLIP_CURSOR IS
                                        SELECT DEMPS.PICKSLIP_ID AS PICKSLIP_ID, 
                                               SUBSTR(C.NAME,1, 10) || ' ' || SUBSTR(DEMPS.CUSTOMER_ORDER_ID,1,9) || TO_CHAR(DEMPS.DC_CANCEL_DATE, ' MM/DD') AS BUCKET_NAME
                                          FROM <proxy />DEM_PICKSLIP DEMPS
                                          LEFT OUTER JOIN <proxy />MASTER_CUSTOMER C
                                                ON C.CUSTOMER_ID = DEMPS.CUSTOMER_ID
                                         WHERE DEMPS.PS_STATUS_ID = 1
                                           AND DEMPS.CUSTOMER_ID = :CUSTOMER_ID
                                           AND DEMPS.VWH_ID = :VWH_ID
                                           AND {0};
                                      PICKSLIP_COUNT BINARY_INTEGER := 0;
                                    BEGIN                                      
                                      FOR PICKSLIP_REC IN PICKSLIP_CURSOR LOOP
                                        PICKSLIP_COUNT := PICKSLIP_COUNT + 1;
                                        <proxy />PKG_DATA_EXCHANGE.GET_PICKSLIP(PICKSLIP_REC.PICKSLIP_ID, :BUCKET_ID);
                                    LBUCKET_NAME := PICKSLIP_REC.BUCKET_NAME;
                                      END LOOP;
                                      IF PICKSLIP_COUNT = 0 THEN
                                        RAISE_APPLICATION_ERROR(-20000, 'No pickslips were added');
                                      END IF;
                                <if c='$updateBucketName'>
                                  UPDATE <proxy />BUCKET SET NAME = LBUCKET_NAME WHERE BUCKET_ID = :BUCKET_ID;
                                </if>
                                    END;";

            var binder = SqlBinder.Create();
            var bucket = new PickWaveEditable();
            binder.Parameter("BUCKET_ID", bucketId)
                  .Parameter("CUSTOMER_ID", customerId)
                  .Parameter("VWH_ID", vwhId)
                  .ParameterXPath("updateBucketName", updateBucketName)
                  ;

            var attrs = PickWaveHelpers.GetEnumMemberAttributes<PickslipDimension, DataTypeAttribute>();
            var clauses = new List<string>(2);
            foreach (var dim in dimensions)
            {
                clauses.Add(GetDimensionWhereClause(dim.Item1, dim.Item2));
                if (attrs.ContainsKey(dim.Item1) && attrs[dim.Item1].DataType == DataType.Date)
                {
                    binder.Parameter(dim.Item1.ToString(), Convert.ToDateTime(dim.Item2));
                }
                else
                {
                    binder.Parameter(dim.Item1.ToString(), Convert.ToString(dim.Item2));
                }
            }
            var queryFinal = string.Format(QUERY, string.Join(" AND ", clauses));
            _db.ExecuteDml(queryFinal, binder);
        }

        /// <summary>
        /// Add pickslip to passed bucket.
        /// </summary>
        /// <param name="bucketId"></param>
        /// <param name="pickslipList"></param>
        internal void AddPickslipsToWave(int bucketId, IList<int> pickslipList)
        {
            const string QUERY = @"
                            BEGIN                         
                                <proxy />PKG_DATA_EXCHANGE.GET_PICKSLIP(:pickslip_id, :BUCKET_ID);
                            END;";
            var binder = SqlBinder.Create(pickslipList.Count);
            binder.Parameter("pickslip_id", pickslipList);
            binder.Parameter("BUCKET_ID", Enumerable.Repeat(bucketId, pickslipList.Count));
            _db.ExecuteDml(QUERY, binder);
        }

        /// <summary>
        /// Returns those numbered carton areas which contain at least one carton.
        /// Only just imported orders are considered.
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        public IList<CreateWaveArea> GetAreasForCustomer(string customerId)
        {
            const string QUERY = @"
                                WITH ORDERED_SKU AS
                                     (SELECT MAX(PD.UPC_CODE) AS UPC_CODE, SKU.SKU_ID, P.VWH_ID
                                        FROM <proxy />DEM_PICKSLIP P
                                       INNER JOIN <proxy />DEM_PICKSLIP_DETAIL PD
                                          ON P.PICKSLIP_ID = PD.PICKSLIP_ID
                                       INNER JOIN <proxy />MASTER_SKU SKU
                                          ON SKU.UPC_CODE = PD.UPC_CODE
                                       WHERE P.CUSTOMER_ID = :CUSTOMER_ID
                                         AND P.PS_STATUS_ID = 1
                                       GROUP BY SKU.SKU_ID, P.VWH_ID),
                                CARTON_AREAS AS
                                     (SELECT CTN.CARTON_STORAGE_AREA, COUNT(UNIQUE OS.SKU_ID) AS COUNT_SKU
                                        FROM <proxy />SRC_CARTON CTN
                                       INNER JOIN <proxy />SRC_CARTON_DETAIL CTNDET
                                          ON CTN.CARTON_ID = CTNDET.CARTON_ID
                                        LEFT OUTER JOIN ORDERED_SKU OS
                                          ON OS.SKU_ID = CTNDET.SKU_ID
                                         AND OS.VWH_ID = CTN.VWH_ID
                                       WHERE CTN.LOCATION_ID IS NOT NULL
                                       GROUP BY CTN.CARTON_STORAGE_AREA),
                                PICK_AREAS AS
                                 (SELECT IALOC.IA_ID, COUNT(UNIQUE OS.SKU_ID) AS COUNT_SKU
                                    FROM <proxy />IALOC IALOC
                                    LEFT OUTER JOIN ORDERED_SKU OS
                                      ON OS.UPC_CODE = IALOC.ASSIGNED_UPC_CODE
                                     AND OS.VWH_ID = IALOC.VWH_ID  
                                   GROUP BY IALOC.IA_ID)
                            SELECT :PULL_AREA_TYPE                                  AS AREA_TYPE,
                                   TIA.INVENTORY_STORAGE_AREA                       AS INVENTORY_STORAGE_AREA,
                                   TIA.DESCRIPTION                                  AS DESCRIPTION,
                                   TIA.SHORT_NAME                                   AS SHORT_NAME,
                                   TIA.WAREHOUSE_LOCATION_ID                        AS WAREHOUSE_LOCATION_ID,
                                   CA.COUNT_SKU                                     AS COUNT_SKU,
                                   (SELECT COUNT(UNIQUE SKU_ID) FROM ORDERED_SKU)   AS COUNT_ORDERED_SKU
                              FROM <proxy />TAB_INVENTORY_AREA TIA
                             INNER JOIN CARTON_AREAS CA
                                ON CA.CARTON_STORAGE_AREA = TIA.INVENTORY_STORAGE_AREA                            

                            UNION ALL

                            SELECT :PITCH_AREA_TYPE                                 AS AREA_TYPE,
                                   I.IA_ID                                          AS INVENTORY_STORAGE_AREA,
                                   I.SHORT_DESCRIPTION                              AS DESCRIPTION,
                                   I.SHORT_NAME                                     AS SHORT_NAME,
                                   I.WAREHOUSE_LOCATION_ID                          AS WAREHOUSE_LOCATION_ID,
                                   CA.COUNT_SKU                                     AS COUNT_SKU,
                                   (SELECT COUNT(UNIQUE SKU_ID) FROM ORDERED_SKU)   AS COUNT_ORDERED_SKU
                              FROM <proxy />IA I
                             INNER JOIN PICK_AREAS CA
                                ON CA.IA_ID = I.IA_ID
                             WHERE I.PICKING_AREA_FLAG = 'Y'";
            var binder = SqlBinder.Create(row => new CreateWaveArea
            {
                AreaId = row.GetString("INVENTORY_STORAGE_AREA"),
                ShortName = row.GetString("SHORT_NAME"),
                Description = row.GetString("DESCRIPTION"),
                BuildingId = row.GetString("WAREHOUSE_LOCATION_ID"),
                CountSku = row.GetInteger("COUNT_SKU"),
                AreaType = row.GetEnum<BucketActivityType>("AREA_TYPE"),
                CountOrderedSku = row.GetInteger("COUNT_ORDERED_SKU")
            });
            binder.Parameter("CUSTOMER_ID", customerId)
                .Parameter("PITCH_AREA_TYPE", BucketActivityType.Pitching.ToString())
                .Parameter("PULL_AREA_TYPE", BucketActivityType.Pulling.ToString());
            return _db.ExecuteReader(QUERY, binder);
        }

        /// <summary>
        /// Returns the list of VWh ID of passed customer orders
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        public IEnumerable<VirtualWarehouse> GetVWhListOfCustomer(string customerId)
        {
            const string QUERY = @"
            SELECT TVW.VWH_ID           AS VWH_ID, 
                   MAX(TVW.DESCRIPTION) AS DESCRIPTION
              FROM <proxy />TAB_VIRTUAL_WAREHOUSE TVW
             INNER JOIN <proxy />DEM_PICKSLIP DEMPS
                ON DEMPS.VWH_ID = TVW.VWH_ID
             WHERE DEMPS.PS_STATUS_ID = 1
               AND DEMPS.CUSTOMER_ID = :CUSTOMER_ID
             GROUP BY TVW.VWH_ID
             ORDER BY TVW.VWH_ID
            ";
            var binder = SqlBinder.Create(row => new VirtualWarehouse
            {
                VWhId = row.GetString("VWH_ID"),
                Description = row.GetString("DESCRIPTION")
            }).Parameter("CUSTOMER_ID", customerId);
            return _db.ExecuteReader(QUERY, binder);
        }

        public PickWaveEditable GetEditableBucket(int bucketId)
        {
            if (bucketId == 0)
            {
                throw new ArgumentNullException("bucketId");
            }

            const string QUERY = @"
                                SELECT COUNT(PS.PICKSLIP_ID)        AS PICKSLIP_COUNT,
                                       MAX(T.SHORT_NAME)            AS PULL_AREA,
                                       MAX(I.SHORT_NAME)            AS PITCH_AREA
                                  FROM <proxy />BUCKET B
                                 LEFT OUTER JOIN <proxy />PS PS
                                    ON PS.BUCKET_ID = B.BUCKET_ID
                                    AND PS.TRANSFER_DATE IS NULL
                                  LEFT OUTER JOIN <proxy />TAB_INVENTORY_AREA T
                                    ON T.INVENTORY_STORAGE_AREA = B.PULL_CARTON_AREA
                                  LEFT OUTER JOIN <proxy />IA I
                                    ON I.IA_ID = B.PITCH_IA_ID
                                 WHERE B.BUCKET_ID = :BUCKET_ID";

            var binder = SqlBinder.Create(row => new PickWaveEditable
            {
                PullAreaShortName = row.GetString("PULL_AREA"),
                PickslipCount = row.GetInteger("PICKSLIP_COUNT") ?? 0,
                PitchAreaShortName = row.GetString("PITCH_AREA")
            })
                .Parameter("BUCKET_ID", bucketId);
            return _db.ExecuteSingle(QUERY, binder);
        }
    }
}