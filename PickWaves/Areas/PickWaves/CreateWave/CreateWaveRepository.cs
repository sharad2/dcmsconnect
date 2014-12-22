using DcmsMobile.PickWaves.Helpers;
using DcmsMobile.PickWaves.Repository;
using EclipseLibrary.Oracle;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace DcmsMobile.PickWaves.Areas.PickWaves.CreateWave
{
    internal class CreateWaveRepository : PickWaveRepositoryBase
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
        public IEnumerable<Pickslip> GetPickslips(string customerId, string vwhId, IList<Tuple<PickslipDimension, object>> dimensions)
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
                   DEMPS.CUSTOMER_DIST_CENTER_ID    AS CUSTOMER_DIST_CENTER_ID
              FROM <proxy />DEM_PICKSLIP DEMPS
             WHERE DEMPS.PS_STATUS_ID = 1   
               AND DEMPS.CUSTOMER_ID = :CUSTOMER_ID 
               AND DEMPS.VWH_ID = :VWH_ID
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
                CustomerDcId = row.GetString("CUSTOMER_DIST_CENTER_ID")
            }).Parameter("CUSTOMER_ID", customerId)
            .Parameter("VWH_ID", vwhId);

            var attrs = PickWaveHelpers.GetEnumMemberAttributes<PickslipDimension, DataTypeAttribute>();
            var clauses = new List<string>(2);
            foreach (var dim in dimensions)
            {
                if (dim.Item2 == null)
                {
                    throw new ArgumentNullException("dimensions[i].Item2");
                }
                clauses.Add(GetDimensionWhereClause(dim.Item1, dim.Item2));
                if (attrs.ContainsKey(dim.Item1) && attrs[dim.Item1].DataType == DataType.Date)
                {
                    binder.Parameter(dim.Item1.ToString(), Convert.ToDateTime(dim.Item2));
                }
                else
                {
                    // Sharad 16 Dec 2014: Trimming the value because we get leading spaces in priority once in a while
                    binder.Parameter(dim.Item1.ToString(), Convert.ToString(dim.Item2).Trim());
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
                    clause = "(DEMPS.PICKSLIP_IMPORT_DATE &gt;= TRUNC(CAST(:{0} AS DATE)) AND DEMPS.PICKSLIP_IMPORT_DATE &lt; TRUNC(CAST(:{0} AS DATE))  + 1)";
                    break;

                case PickslipDimension.StartDate:
                    clause = "(DEMPS.DELIVERY_DATE &gt;= TRUNC(CAST(:{0} AS DATE)) AND DEMPS.DELIVERY_DATE &lt; TRUNC(CAST(:{0} AS DATE))  + 1)";          break;

                case PickslipDimension.CancelDate:
                    clause = "(DEMPS.CANCEL_DATE &gt;= TRUNC(CAST(:{0} AS DATE)) AND DEMPS.CANCEL_DATE &lt; TRUNC(CAST(:{0} AS DATE))  + 1)";
                    
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
                    clause = "(DEMPS.DC_CANCEL_DATE &gt;= TRUNC(CAST(:{0} AS DATE)) AND DEMPS.DC_CANCEL_DATE &lt; TRUNC(CAST(:{0} AS DATE))  + 1)";
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
        /// For the passed customer and vwh, groups results by col1 and returns a row for each unique value of col1.
        /// Each row contains an array of pickslip counts for each unique value of col2
        /// </summary>
        /// <param name="customerId">Supmmary of pickslip counts and pieces ordered are required for this customer</param>
        /// <param name="vwhId">Consider only those orders of this vwhId</param>
        /// <param param name="col1">First Group by column</param>
        /// <param name="col2">Second Group by column</param>
        /// <returns>Item1 is a list of rows for each unique value of col1. Item2 is number of pickslips per dimension</returns>
        /// <remarks>
        /// If too many rows for the dimension are returned, then null is returned. If no rows are returned for the dimension, and empty collection is returned.
        /// Thus null is different from empty.
        /// </remarks>
        internal CustomerOrderSummary GetOrderSummaryForCustomer(string customerId, string vwhId, PickslipDimension col1, PickslipDimension col2)
        {
            if (string.IsNullOrWhiteSpace(customerId))
            {
                throw new ArgumentNullException("customerId");
            }
            if (string.IsNullOrWhiteSpace(vwhId))
            {
                throw new ArgumentNullException("vwhId");
            }
            if (col1 == PickslipDimension.NotSet)
            {
                throw new ArgumentOutOfRangeException("col1");
            }
            if (col2 == PickslipDimension.NotSet)
            {
                throw new ArgumentOutOfRangeException("col2");
            }
            var dimMap = new Dictionary<PickslipDimension, Tuple<string, Type>>
            {
                {PickslipDimension.Priority, Tuple.Create("LPAD(T.PRIORITY_ID, 10)", typeof(string))},
                {PickslipDimension.CustomerStore, Tuple.Create("T.CUSTOMER_STORE_ID", typeof(string))},
                {PickslipDimension.CustomerDcCancelDate, Tuple.Create("TRUNC(T.DC_CANCEL_DATE)", typeof(DateTime))},
                {PickslipDimension.Label, Tuple.Create("T.PICKSLIP_TYPE", typeof(string))},
                {PickslipDimension.ImportDate, Tuple.Create("TRUNC(T.PICKSLIP_IMPORT_DATE)", typeof(DateTime))},
                {PickslipDimension.StartDate, Tuple.Create("TRUNC(T.DELIVERY_DATE)", typeof(DateTime))},
                {PickslipDimension.CancelDate, Tuple.Create("TRUNC(T.CANCEL_DATE)", typeof(DateTime))},
                {PickslipDimension.CustomerOrderType, Tuple.Create("t.CUSTOMER_ORDER_TYPE", typeof(string))},
                {PickslipDimension.SaleTypeId, Tuple.Create("T.SALES_TYPE_ID", typeof(string))},
                {PickslipDimension.PurchaseOrder, Tuple.Create("T.CUSTOMER_ORDER_ID", typeof(string))},
                {PickslipDimension.CustomerDc, Tuple.Create("T.CUSTOMER_DIST_CENTER_ID", typeof(string))}
            };
            const string QUERY = @"
         WITH Q1 AS
             (SELECT t.PICKSLIP_ID,
                    T.TOTAL_QUANTITY_ORDERED,
                     {0} AS PICKSLIP_DIMENSION,
                     {1} AS DIM_COL,
                     {2}
                FROM <proxy />DEM_PICKSLIP T
               WHERE T.PS_STATUS_ID = 1
                 AND T.CUSTOMER_ID = :CUSTOMER_ID
             AND T.VWH_ID = :VWH_ID)
            SELECT PICKSLIP_DIMENSION, {3}, CAST(DIM_COL_XML AS VARCHAR2(4000)) AS DIM_COL_XML
              FROM Q1 PIVOT XML(COUNT(PICKSLIP_ID) AS PICKSLIP_COUNT,SUM(Q1.TOTAL_QUANTITY_ORDERED) AS ORDER_COUNT FOR DIM_COL IN(ANY))
             ORDER BY PICKSLIP_DIMENSION
        ";


            //var array = new List<string>();
            //foreach (var item in dimMap)
            //{
            //    array.Add(string.Format("COUNT(UNIQUE {0}) OVER() AS {1}", item.Value.Item1, item.Key.ToString()));
            //}
            var query = string.Format(QUERY,
                dimMap[col1].Item1,    //{0}
                dimMap[col2].Item1,    //{1}
                string.Join(", ", dimMap.Select(p => string.Format("COUNT(UNIQUE {0}) OVER() AS {1}", p.Value.Item1, p.Key.ToString()))),    //{2}
                string.Join(", ", dimMap.Select(p => p.Key.ToString()))    //{3}
                );
            /* The value of query will look something like this
WITH Q1 AS
 (SELECT t.PICKSLIP_ID,
         T.TOTAL_QUANTITY_ORDERED,
         TRUNC(T.DC_CANCEL_DATE) AS PICKSLIP_DIMENSION,
         T.CUSTOMER_DIST_CENTER_ID AS DIM_COL,
         COUNT(UNIQUE LPAD(T.PRIORITY_ID, 10)) OVER() AS Priority,
         COUNT(UNIQUE T.CUSTOMER_STORE_ID) OVER() AS CustomerStore,
         COUNT(UNIQUE TRUNC(T.DC_CANCEL_DATE)) OVER() AS CustomerDcCancelDate,
         COUNT(UNIQUE T.PICKSLIP_TYPE) OVER() AS Label,
         COUNT(UNIQUE TRUNC(T.PICKSLIP_IMPORT_DATE)) OVER() AS ImportDate,
         COUNT(UNIQUE TRUNC(T.DELIVERY_DATE)) OVER() AS StartDate,
         COUNT(UNIQUE TRUNC(T.CANCEL_DATE)) OVER() AS CancelDate,
         COUNT(UNIQUE t.CUSTOMER_ORDER_TYPE) OVER() AS CustomerOrderType,
         COUNT(UNIQUE T.SALES_TYPE_ID) OVER() AS SaleTypeId,
         COUNT(UNIQUE T.CUSTOMER_ORDER_ID) OVER() AS PurchaseOrder,
         COUNT(UNIQUE T.CUSTOMER_DIST_CENTER_ID) OVER() AS CustomerDc
    FROM DEM_PICKSLIP T
   WHERE T.PS_STATUS_ID = 1
     AND T.CUSTOMER_ID = :CUSTOMER_ID
     AND T.VWH_ID = :VWH_ID)
SELECT *
  FROM Q1 PIVOT XML(COUNT(PICKSLIP_ID) AS PICKSLIP_COUNT, SUM(Q1.TOTAL_QUANTITY_ORDERED) AS ORDER_COUNT FOR DIM_COL IN(ANY))
 ORDER BY PICKSLIP_DIMENSION
             */

            var result = new CustomerOrderSummary
            {
                AllValues = new Matrix()
            };
            var binder = SqlBinder.Create(row =>
            {
                if (result.CountValuesPerDimension == null)
                {
                    result.CountValuesPerDimension = new Dictionary<PickslipDimension, int>();
                    foreach (var item in dimMap)
                    {
                        result.CountValuesPerDimension.Add(item.Key, row.GetInteger(item.Key.ToString()) ?? 0);
                    }
                }
                return new
            {
                RowValue = dimMap[col1].Item2 == typeof(DateTime) ? (object)row.GetDate("pickslip_dimension") : (object)row.GetString("pickslip_dimension"),
                ColValues = MapOrderSummaryXml(row.GetString("DIM_COL_XML"), dimMap[col2].Item2 == typeof(DateTime))
            };
            });
            binder.Parameter("CUSTOMER_ID", customerId)
                .Parameter("VWH_ID", vwhId)
                ;
            var rows = _db.ExecuteReader(query, binder);

            foreach (var row in rows)
            {
                result.AllValues.AddRow(row.RowValue, row.ColValues);

            }
            return result;
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
        private IDictionary<object, CellValue> MapOrderSummaryXml(string data, bool isColDate)
        {
            var xml = XElement.Parse(data);
            var query = (from item in xml.Elements("item")
                         let column = item.Elements("column")
                         select new
                         {
                             ColElement = column.First(p => p.Attribute("name").Value == "DIM_COL"),
                             PickslipCount = (int)column.First(p => p.Attribute("name").Value == "PICKSLIP_COUNT"),
                             OrderedPieces = column.First(p => p.Attribute("name").Value == "ORDER_COUNT")
                         });
            if (isColDate)
            {
                //return query.ToDictionary(p => (object)(DateTime?)p.ColElement, p => p.PickslipCount);
                return query.ToDictionary(p => (object)(DateTime?)p.ColElement, p => new CellValue
                {
                    OrderedPieces = string.IsNullOrEmpty(p.OrderedPieces.Value) ? 0 : (int)p.OrderedPieces,
                    PickslipCount = p.PickslipCount
                });
            }
            //return query.ToDictionary(p => (object)(string)p.ColElement, p => p.PickslipCount);
            return query.ToDictionary(p => (object)(string)p.ColElement, p => new CellValue
            {
                PickslipCount = p.PickslipCount,
                OrderedPieces = string.IsNullOrEmpty(p.OrderedPieces.Value) ? 0 : (int)p.OrderedPieces,
            });
        }

        /// <summary>
        /// Create bucket
        /// </summary>
        /// <param name="bucket"></param>
        /// <returns></returns>
        public int CreateDefaultWave()
        {
            const string QUERY = @"  
                                    INSERT INTO <proxy />BUCKET BKT
                                        (BKT.BUCKET_ID,
                                         BKT.NAME,
                                         BKT.PRIORITY,
                                         BKT.FREEZE)
                                      VALUES
                                        (<proxy />BUCKET_SEQUENCE.NEXTVAL,
                                         'Automatically Created',
                                         '1',
                                         'Y')
                                      RETURNING BUCKET_ID INTO :BUCKET_ID
              ";
            var binder = SqlBinder.Create();
            //binder.Parameter("PITCH_TYPE", "BOX")
            //      .Parameter("NAME", bucket.BucketName)
            //      .Parameter("PITCH_IA_ID", bucket.PitchAreaId)
            //      .Parameter("PRIORITY", bucket.PriorityId)
            //      .Parameter("PULL_CARTON_AREA", bucket.PullAreaId)
            //      .Parameter("QUICK_PITCH_FLAG", bucket.QuickPitch ? "Y" : null)
            //      .Parameter("PULL_TO_DOCK", bucket.PullingBucket)
            //      ;
            var bucketId = 0;
            binder.OutParameter("BUCKET_ID", val => bucketId = val.Value);
            _db.ExecuteDml(QUERY, binder);
            return bucketId;
        }

        /// <summary>
        /// Add pickslips to bucket
        /// </summary>
        /// <param name="bucketId"></param>
        /// <param name="customerId"></param>
        /// <param name="dimensions"></param>
        /// <param name="vwhId"></param>
        /// <param name="updateBucketName"></param>
        public void AddPickslipsPerDim(int bucketId, string customerId, IList<Tuple<PickslipDimension, object>> dimensions, string vwhId, bool updateBucketName)
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
            var bucket = new PickWave();
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
        internal void AddPickslipsToWave(int bucketId, IList<long> pickslipList)
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
        public IList<VirtualWarehouse> GetVWhListOfCustomer(string customerId)
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

        public PickWave GetPickWave(int bucketId)
        {
            if (bucketId == 0)
            {
                throw new ArgumentNullException("bucketId");
            }

            const string QUERY = @"
                                SELECT COUNT(PS.PICKSLIP_ID)        AS PICKSLIP_COUNT,
                                       MAX(T.SHORT_NAME)            AS PULL_AREA,
                                       MAX(I.SHORT_NAME)            AS PITCH_AREA,
                                       B.BUCKET_ID                  AS BUCKET_ID
                                  FROM <proxy />BUCKET B
                                 LEFT OUTER JOIN <proxy />PS PS
                                    ON PS.BUCKET_ID = B.BUCKET_ID
                                    AND PS.TRANSFER_DATE IS NULL
                                  LEFT OUTER JOIN <proxy />TAB_INVENTORY_AREA T
                                    ON T.INVENTORY_STORAGE_AREA = B.PULL_CARTON_AREA
                                  LEFT OUTER JOIN <proxy />IA I
                                    ON I.IA_ID = B.PITCH_IA_ID
                                 WHERE B.BUCKET_ID = :BUCKET_ID
                                    GROUP BY B.BUCKET_ID";

            var binder = SqlBinder.Create(row => new PickWave
            {
                BucketId = row.GetInteger("BUCKET_ID") ?? 0,
                PullAreaShortName = row.GetString("PULL_AREA"),
                PickslipCount = row.GetInteger("PICKSLIP_COUNT") ?? 0,
                PitchAreaShortName = row.GetString("PITCH_AREA")
            })
                .Parameter("BUCKET_ID", bucketId);
            return _db.ExecuteSingle(QUERY, binder);
        }
    }
}