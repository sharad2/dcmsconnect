using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EclipseLibrary.Oracle;

namespace DcmsMobile.Shipping.Repository
{
    // Reviewed by: Deepak Bhatt, Ravneet Kaur, Dinesh : 4 Dec 2012.
    // Reviewed by: Deepak Bhatt, Ravneet Kaur, Binod : 14 Dec 2012.
    /// <summary>
    /// Used by GetorderSummary() to filter the customers retrieved
    /// </summary>
    public enum RoutingStatus
    {
        Notset,
        Unrouted,
        Routing,
        Routed,
        InBol
    }
    public class ShippingRepository : IDisposable
    {
        #region Intialization

        const string MODULE_NAME = "Routing";
        private readonly OracleDatastore _db;

        public OracleDatastore Db
        {
            get
            {
                return _db;
            }
        }

        public ShippingRepository(OracleDatastore db)
        {
            _db = db;
        }
        public ShippingRepository(TraceContext ctx, string connectString, string userName, string clientInfo)
        {
            var db = new OracleDatastore(ctx);
            db.CreateConnection(connectString, userName);

            db.ModuleName = MODULE_NAME;
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

        #region Routing Summary
        /// <summary>
        /// Gets the routing details of all or passed customer.
        /// </summary>
        /// <param name="customerId">Get summary for a specific customer. In this case, a single row is returned.</param>
        /// <param name="filter"> Filtering customers based on the passed routing status. i.e. routed, unrouted etc. For details check the RoutingStatus Class.</param>
        /// <param name="poId">PO to search</param>
        /// <returns></returns>
        /// <remarks>
        /// We  show those orders whose bucket has been made available.
        /// We exclude orders which are either shipped or have been cancelled. 
        /// Query retrieves following attributes 
        /// POS_IN_BOL: The unshipped POS for which BOLs have already been created.
        /// COUNT_UNSHIPPED_BOLS: Total unshipped BOLs.
        /// COUNT_ROUTING_INPROGRESS_POS:  The number of POs for which routing information is awaited from customer. We have already provided ATS date to the customer. 
        /// COUNT_UNROUTED_POS: The the number of POs which are still to be routed. 
        /// PIECES_ROUTED: The number of pieces for which routing information has been received from customer.
        /// </remarks>
        public IList<CustomerOrderSummary> GetOrderSummary(string customerId, RoutingStatus filter)
        {
            const string QUERY = @"                                  
                                    WITH Q1 AS
                                    (SELECT PS.CUSTOMER_ID AS CUSTOMER_ID,
                                            PS.PO_ID AS PO_ID,
                                            MAX(CUST.NAME) AS CUST_NAME,
                                            COUNT(UNIQUE CASE
                                                WHEN EDIPS.EDI_ID IS NULL AND PS.SHIPPING_ID IS NULL THEN
                                                      PS.PICKSLIP_ID END) AS COUNT_UNROUTED_PS,
                                            COUNT(UNIQUE CASE
                                                WHEN EDIPS.EDI_ID IS NOT NULL AND
                                                  (EDIPS.PICKUP_DATE IS NULL AND EDIPS.LOAD_ID IS NULL) AND
                                                  PS.SHIPPING_ID IS NULL THEN
                                                  PS.PICKSLIP_ID END) AS COUNT_RIP_PS,
                                             COUNT(UNIQUE CASE
                                                    WHEN (EDIPS.PICKUP_DATE IS NOT NULL OR EDIPS.LOAD_ID IS NOT NULL) AND
                                                  PS.SHIPPING_ID IS NULL THEN
                                                 PS.PICKSLIP_ID END) AS COUNT_ROUTED_PS,
                                               COUNT(UNIQUE CASE
                                                        WHEN SHIP.ONHOLD_FLAG IS NOT NULL THEN
                                                PS.PICKSLIP_ID END) AS COUNT_INBOL_PS,
                                                SUM(PS.TOTAL_QUANTITY_ORDERED) AS PIECES_ORDERED,
                                                MAX(PO.DC_CANCEL_DATE) AS DC_CANCEL_DATE,
                                                MAX(PO.START_DATE) AS START_DATE,
                                                SUM(PS.TOTAL_DOLLARS_ORDERED) AS TOTAL_DOLLARS_ORDERED,
                                                COUNT(C.CUSTOMER_ID) AS COUNT_EDI_PS
                                                        FROM <proxy />PS PS
                                                       INNER JOIN <proxy />BUCKET B
                                                          ON B.BUCKET_ID = PS.BUCKET_ID
                                                        LEFT OUTER JOIN <proxy />CUST CUST
                                                          ON CUST.CUSTOMER_ID = PS.CUSTOMER_ID
                                                        LEFT OUTER JOIN <proxy />PO PO
                                                          ON PO.CUSTOMER_ID = PS.CUSTOMER_ID
                                                         AND PO.PO_ID = PS.PO_ID
                                                         AND PO.ITERATION = PS.ITERATION
                                                        LEFT OUTER JOIN <proxy />EDI_753_754_PS EDIPS
                                                          ON PS.PICKSLIP_ID = EDIPS.PICKSLIP_ID
                                                        LEFT OUTER JOIN <proxy />SHIP SHIP
                                                          ON PS.SHIPPING_ID = SHIP.SHIPPING_ID
                                                        LEFT OUTER JOIN <proxy />CUSTSPLH C
                                                          ON PS.CUSTOMER_ID = C.CUSTOMER_ID
                                                         AND C.SPLH_ID = '$EDI753'
                                                       WHERE PS.TRANSFER_DATE IS NULL
                                                         AND PS.PICKSLIP_CANCEL_DATE IS NULL
                                                         AND (SHIP.ONHOLD_FLAG IS NOT NULL OR SHIP.SHIP_DATE IS NULL)
                                                         AND B.AVAILABLE_FOR_PITCHING = 'Y'
                                                        <if>and ps.customer_id=:CUSTOMER_ID</if>
                                                       GROUP BY PS.CUSTOMER_ID, PS.PO_ID),
                                                    Q2 AS
                                                     (SELECT Q1.CUSTOMER_ID AS CUSTOMER_ID,
                                                             COUNT(CASE
                                                                     WHEN Q1.COUNT_UNROUTED_PS &gt; 0 THEN
                                                                      Q1.PO_ID
                                                                   END) AS COUNT_UNROUTED_POS,
                                                             COUNT(CASE
                                                                     WHEN Q1.COUNT_UNROUTED_PS = 0 AND Q1.COUNT_RIP_PS &gt; 0 THEN
                                                                      Q1.PO_ID
                                                                   END) AS COUNT_ROUTING_INPROGRESS_POS,
         
                                                             COUNT(CASE
                                                                     WHEN Q1.COUNT_UNROUTED_PS = 0 AND Q1.COUNT_RIP_PS = 0 AND
                                                                          COUNT_ROUTED_PS > 0 THEN
                                                                      Q1.PO_ID
                                                                   END) AS COUNT_ROUTED_POS,
                                                             COUNT(CASE
                                                                     WHEN Q1.COUNT_UNROUTED_PS = 0 AND Q1.COUNT_RIP_PS = 0 AND
                                                                          Q1.COUNT_ROUTED_PS = 0 AND COUNT_INBOL_PS &gt; 0 THEN
                                                                      Q1.PO_ID
                                                                   END) AS POS_IN_BOL,
                                                             COUNT(Q1.PO_ID) AS ORDERED_POS,
                                                             MAX(Q1.CUST_NAME) AS CUST_NAME,
                                                             MAX(Q1.DC_CANCEL_DATE) AS DC_CANCEL_DATE,
                                                             MAX(Q1.START_DATE) AS START_DATE,
                                                             SUM(Q1.TOTAL_DOLLARS_ORDERED) AS TOTAL_DOLLARS_ORDERED,
                                                             SUM(Q1.PIECES_ORDERED) AS PIECES_ORDERED,
                                                             MAX(CASE
                                                                   WHEN Q1.COUNT_EDI_PS &gt; 0 THEN
                                                                    Q1.CUSTOMER_ID
                                                                 END) AS EDI_CUSTOMER
                                                        FROM Q1
                                                       where 1 = 1
                                                       GROUP BY Q1.CUSTOMER_ID),
                                                       Q3 AS 
                                                       (
                                                       SELECT  
                                                       PS.CUSTOMER_ID AS CUSTOMER_ID,
                                                       COUNT(UNIQUE(CASE
                                                                          WHEN SHIP.ONHOLD_FLAG IS NOT NULL
                                                                           THEN PS.SHIPPING_ID
                                                                        END)) AS COUNT_UNSHIPPED_BOLS
                                                                        FROM <proxy />SHIP SHIP
                                                                        LEFT OUTER JOIN <proxy />PS PS
                                                                        ON PS.SHIPPING_ID=SHIP.SHIPPING_ID
                                                                        GROUP BY PS.CUSTOMER_ID),
                                                     Q4 as
                                                     (  
                                                    SELECT Q2.*,Q3.COUNT_UNSHIPPED_BOLS FROM Q2
                                                    LEFT OUTER JOIN Q3 ON
                                                    Q2.CUSTOMER_ID=Q3.CUSTOMER_ID
                                                    )
                                                     select * from q4
                                                     WHERE 1 = 1
                                                                        <if c='$unRoutedOnly'>
                                                                        AND Q4.COUNT_UNROUTED_POS &gt; 0   
                                                                        </if>
                                                                        <if c='$routingOnly'>
                                                                        AND Q4.COUNT_ROUTING_INPROGRESS_POS  &gt; 0
                                                                        </if>
                                                                        <if c='$routedOnly'>
                                                                        AND Q4.COUNT_ROUTED_POS  &gt; 0
                                                                        </if>
                                                                        <if c='$bolOnly'>
                                                                        AND Q4.POS_IN_BOL  &gt; 0
                                                                        </if>
";
            var binder = SqlBinder.Create(row => new CustomerOrderSummary()

            {
                CountRoutingPo = row.GetInteger("COUNT_ROUTING_INPROGRESS_POS"),
                CustomerId = row.GetString("CUSTOMER_ID"),                
                CountUnroutedPo = row.GetInteger("COUNT_UNROUTED_POS"),
                CustomerName = row.GetString("CUST_NAME"),
                CustomerPosCount = row.GetInteger("ORDERED_POS"),
                CountPosInBol = row.GetInteger("POS_IN_BOL"),                
                MaxDcCancelDate = row.GetDate("DC_CANCEL_DATE"),
                PiecesOrdered = row.GetInteger("PIECES_ORDERED"),                
                TotalDollarsOrdered = row.GetDecimal("TOTAL_DOLLARS_ORDERED"),
                CountRoutedPo = row.GetInteger("COUNT_ROUTED_POS"),
                StartDate = row.GetDate("START_DATE"),
                EdiCustomer = row.GetString("EDI_CUSTOMER"),
                TotalUnshippedBols = row.GetInteger("COUNT_UNSHIPPED_BOLS")
               
            })
                .Parameter("CUSTOMER_ID", customerId);            
            binder.ParameterXPath("unRoutedOnly", filter == RoutingStatus.Unrouted);
            binder.ParameterXPath("routingOnly", filter == RoutingStatus.Routing);
            binder.ParameterXPath("routedOnly", filter == RoutingStatus.Routed);
            binder.ParameterXPath("bolOnly", filter == RoutingStatus.InBol);
            return _db.ExecuteReader(QUERY, binder);
        }

        # endregion

        #region Unrouted

        /// <summary>
        /// Gets unrouted orders for the passed customer. 
        /// One row for each PO,iteration and DC is returned.
        /// </summary>
        /// <param name="customerId"></param>  
        /// <param name="buildingId"></param>
        /// <param name="getUnavailableorders">True->Get orders of unavailable buckets as well</param>
        /// <returns></returns>
        /// <Remarks>
        /// We exclude shipped, transferred and cancelled orders.
        /// Deepak and Dinesh:2-5-2013, Added UNION query to show orders which are yet to be added to the bucket. 
        /// </Remarks>
        public IEnumerable<Po> GetUnroutedOrders(string customerId, string buildingId, bool getUnavailableorders)
        {
            const string QUERY1 = @"
                                    WITH Q1 AS
                                   (   
                                             SELECT PS.PICKSLIP_ID,
                                             MAX(PS.PO_ID)                            AS PO_ID,
                                             MAX(ps.bucket_id)                        AS BUCKET_ID,
                                             MAX(CUST.CUSTOMER_ID)                    AS CUSTOMER_ID,
                                             MAX(CUST.NAME)                           AS CUST_NAME,
                                             MAX(PS.ITERATION)                        AS ITERATION,
                                             MAX(PS.TOTAL_QUANTITY_ORDERED)           AS TOTAL_QUANTITY_ORDERED,
                                             MAX(PS.WAREHOUSE_LOCATION_ID)            AS WAREHOUSE_LOCATION_ID,
                                             COUNT(UNIQUE B.UCC128_ID)                AS BOX_COUNT,
                                             SUM(BD.expected_pieces)                  AS EXPECTED_PIECES,
                                             MAX(PS.CUSTOMER_DC_ID)                   AS CUSTOMER_DC_ID,
                                             MAX(PO.START_DATE)                       AS START_DATE,
                                             MIN(PO.DC_CANCEL_DATE)                   AS DCCANCEL_DATE
                                        FROM <proxy />PS PS
                                      INNER JOIN <proxy />BUCKET BK 
                                          ON BK.BUCKET_ID=PS.BUCKET_ID
                                       LEFT OUTER JOIN <proxy />CUST CUST
                                          ON PS.CUSTOMER_ID = CUST.CUSTOMER_ID                                        
                                       LEFT OUTER JOIN <proxy />BOX B
                                          ON B.PICKSLIP_ID = PS.PICKSLIP_ID
                                       LEFT OUTER JOIN <proxy />BOXDET BD
                                          ON B.PICKSLIP_ID = BD.PICKSLIP_ID
                                         AND B.UCC128_ID = BD.UCC128_ID
                                       LEFT OUTER JOIN <proxy />PO PO
                                        ON PO.CUSTOMER_ID = PS.CUSTOMER_ID
                                       AND PO.PO_ID = PS.PO_ID
                                       AND PO.ITERATION = PS.ITERATION
                                       WHERE 1=1
                                         <if>AND PS.CUSTOMER_ID =:CUSTOMER_ID</if>
                                         <if>AND PS.WAREHOUSE_LOCATION_ID =:BUILDING_ID</if>
                                         AND PS.TRANSFER_DATE IS NULL
                                         AND Bk.AVAILABLE_FOR_PITCHING = 'Y'
                                         AND PS.SHIPPING_ID IS NULL
                                         AND PS.PICKSLIP_CANCEL_DATE IS NULL
                                         AND B.STOP_PROCESS_DATE IS NULL 
                                         AND BD.STOP_PROCESS_DATE IS NULL
                                         AND PS.PICKSLIP_ID NOT IN (SELECT EDIPS.PICKSLIP_ID FROM <proxy />EDI_753_754_PS EDIPS)
                                     GROUP BY PS.PICKSLIP_ID                                   
                                        <if c='$getUnavailableorders'>
                                            UNION
                                            SELECT DP.PICKSLIP_ID,
                                                 MAX(DP.CUSTOMER_ORDER_ID)                          AS PO_ID,
                                                 NULL                                               AS BUCKET_ID,
                                                 MAX(CUST.CUSTOMER_ID)                              AS CUSTOMER_ID,
                                                 MAX(CUST.NAME)                                     AS CUST_NAME,
                                                 0                                                  AS ITERATION,
                                                 MAX(DP.TOTAL_QUANTITY_ORDERED)                     AS TOTAL_QUANTITY_ORDERED,
                                                 MAX(DP.WAREHOUSE_LOCATION_ID)                      AS WAREHOUSE_LOCATION_ID,
                                                 NULL                                               AS BOX_COUNT,
                                                 NULL                                               AS EXPECTED_PIECES,
                                                 MAX(DP.CUSTOMER_DIST_CENTER_ID)                    AS CUSTOMER_DC_ID,
                                                 NULL                                               AS START_DATE,
                                                 MIN(DP.DC_CANCEL_DATE)                             AS DCCANCEL_DATE
                                            FROM  <proxy />DEM_PICKSLIP DP
                                            LEFT OUTER JOIN <proxy />CUST CUST
                                              ON DP.CUSTOMER_ID = CUST.CUSTOMER_ID
                                           WHERE DP.PS_STATUS_ID = '1'
                                           <if>AND DP.CUSTOMER_ID =:CUSTOMER_ID</if>
                                           <if>AND DP.WAREHOUSE_LOCATION_ID =:BUILDING_ID</if>
                                              AND DP.Upload_Date IS NULL
                                            GROUP BY DP.PICKSLIP_ID
                                        </if>
                                       )
                                     SELECT Q1.CUSTOMER_ID,
                                           MAX(Q1.CUST_NAME)                                          AS NAME,
                                           MAX(Q1.BUCKET_ID)                                          AS BUCKET_ID,
                                           Q1.PO_ID                                                   AS PO_ID,
                                           Q1.ITERATION                                               AS ITERATION,
                                           Q1.WAREHOUSE_LOCATION_ID                                   AS BUILDING,
                                           SUM(Q1.TOTAL_QUANTITY_ORDERED)                             AS TOTAL_QUANTITY_ORDERED,
                                           SUM(Q1.BOX_COUNT)                                          AS TOTAL_BOXES,
                                           SUM(Q1.EXPECTED_PIECES)                                    AS EXPECTED_PIECES,
                                           MAX(Q1.START_DATE)                                         AS START_DATE,
                                           MIN(Q1.DCCANCEL_DATE)                                      AS MIN_DCCANCEL_DATE,
                                           Q1.CUSTOMER_DC_ID                                          AS CUSTOMER_DC_ID,
                                           COUNT(UNIQUE Q1.ITERATION) OVER(PARTITION BY Q1.CUSTOMER_ID, Q1.PO_ID) AS PO_ITERATION_COUNT,
                                           count(UNIQUE Q1.WAREHOUSE_LOCATION_ID)OVER(PARTITION BY Q1.CUSTOMER_ID, Q1.PO_ID,Q1.ITERATION,Q1.CUSTOMER_DC_ID) AS BUILDING_COUNT
                                    FROM Q1
                                    GROUP BY Q1.CUSTOMER_ID, Q1.PO_ID, Q1.ITERATION,Q1.CUSTOMER_DC_ID,Q1.WAREHOUSE_LOCATION_ID
                                    ORDER BY (MIN(Q1.DCCANCEL_DATE))";
            var binder = SqlBinder.Create(row => new Po()
            {
                CustomerId = row.GetString("CUSTOMER_ID"),
                CustomerName = row.GetString("NAME"),
                PoId = row.GetString("PO_ID"),
                BuildingId = row.GetString("BUILDING"),
                PiecesOrdered = row.GetInteger("TOTAL_QUANTITY_ORDERED"),
                NumberOfBoxes = row.GetInteger("TOTAL_BOXES"),
                StartDate = row.GetDate("START_DATE"),
                MinDcCancelDate = row.GetDate("MIN_DCCANCEL_DATE"),
                PickedPieces = row.GetInteger("EXPECTED_PIECES"),
                Iteration = row.GetInteger("ITERATION").Value,
                BucketId = row.GetInteger("BUCKET_ID"),
                CustomerDcId = row.GetString("CUSTOMER_DC_ID"),
                PoIterationCount = row.GetInteger("PO_ITERATION_COUNT").Value,
                BuidlingCount=row.GetInteger("BUILDING_COUNT")
            }).Parameter("CUSTOMER_ID", customerId)
             .Parameter("BUILDING_ID", buildingId);
            binder.ParameterXPath("getUnavailableorders", getUnavailableorders);
            return _db.ExecuteReader(QUERY1, binder);
        }

        /// <summary>
        /// Create new EDI for the passed customer.
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns>ediId</returns>
        public int CreateEDIforCustomer(string customerId)
        {
            const string QUERY = @"
                    BEGIN
                          :result := <proxy />pkg_edi_2.create_edi(acustomer_id => :aedi_customer);
                    END;";
            int ediId = 0;
            var binder = SqlBinder.Create().
            Parameter("aedi_customer", customerId)
            .OutParameter("result", values => ediId = values ?? 0);
            _db.ExecuteNonQuery(QUERY, binder);
            return ediId;
        }

        /// <summary> 
        /// Adds the POs to the passed EDI.
        /// </summary>
        /// <param name="poList"></param>
        /// <param name="atsDate"></param>
        /// <param name="customerId"></param>
        /// <param name="ediId"> </param>      
        public void AddPoToEdi(string customerId, ICollection<Tuple<string, int, string>> poList, DateTime? atsDate, int ediId)
        {
            const string QUERY = @"                
                            begin
                              -- Call the procedure
                              <proxy />pkg_edi_2.ADD_PO_TO_EDI(  aedi_id =>:aedi_id,
                                                     apo_list => :aedi_po,
                                                     aiteration_list => :aedi_iteration,
                                                     adc_list => :aedi_dc,
                                                     aats_date => :aats_date
                                                    );
                            end;        
                                       ";
            var binder = SqlBinder.Create();
            binder.ParameterAssociativeArray("aedi_po", poList.Select(p => p.Item1).ToArray());
            binder.ParameterAssociativeArray("aedi_iteration", poList.Select(p => (int?)p.Item2).ToArray());
            binder.ParameterAssociativeArray("aedi_dc", poList.Select(p => p.Item3).ToArray());
            binder.Parameter("aats_date", atsDate);
            binder.Parameter("aedi_id", ediId);
            _db.ExecuteNonQuery(QUERY, binder);
        }

        /// <summary>
        /// Gets details of existing ATS dates for passed customer,atsDate. 
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="atsDate"></param>
        /// <param name="ediId"></param>
        /// <returns>AtsDateSummary</returns>
        public IEnumerable<AtsDateSummary> GetExistingAtsDates(string customerId, DateTime? atsDate)
        {
            if (string.IsNullOrWhiteSpace(customerId))
            {
                throw new ArgumentNullException("customerId");
            }

            const string QUERY = @"
                                SELECT EDIPS.ATS_DATE AS ATS_DATE,
                                       MAX(EDIPS.EDI_ID) AS EDI_ID,
                                COUNT(UNIQUE PS.PO_ID) AS PO_ID
                                  FROM <proxy />EDI_753_754_PS EDIPS
                                 INNER JOIN <proxy />PS PS
                                    ON PS.PICKSLIP_ID = EDIPS.PICKSLIP_ID
                                 WHERE PS.CUSTOMER_ID = :CUSTOMER_ID
                   
                            <if>
                                                and EDIPS.ATS_DATE = :ATS_DATE
                            </if>                                                              
                            <else>
                                    and EDIPS.ATS_DATE is not null
                                    AND PS.TRANSFER_DATE IS NULL
                                    AND PS.SHIPPING_ID IS NULL
                           </else>

                                 GROUP BY EDIPS.ATS_DATE   
                        ";
            var binder = SqlBinder.Create(row => new AtsDateSummary
            {
                AtsDate = row.GetDate("ATS_DATE").Value,
                EdiId = row.GetInteger("EDI_ID"),
                PoCount = row.GetInteger("PO_ID") ?? 0
            });
            binder.Parameter("CUSTOMER_ID", customerId);
            binder.Parameter("ATS_DATE", atsDate);         
            return _db.ExecuteReader(QUERY, binder);
        }

        #endregion

        #region Routing

        /// <summary>
        /// Gets either Routable or Routed POs based on the passed routedEdi flag.
        /// </summary>
        /// <param name="customerId">Customer whose POs are needed. Required</param>
        /// <param name="routedEdi">
        /// 
        /// if true we look at all EDIs which have at least one routed PO and return all POs within those EDIs.
        /// if false we look at all EDIs which have at least one unrouted PO and return all POs within those EDIs.
        /// null means all EDIs are looked at. 
        /// </param>
        /// <returns></returns>
        /// <remarks>
        /// Note that EDIs which have some routed and some unrouted POs are always returned regardless of the setting of <paramref name="routedEdi"/>.
        /// </remarks>
        public IEnumerable<RoutablePo> GetRoutablePos(string customerId, bool? routedEdi, DateTime? startDate, DateTime? dcCancelDate,string buildingId)
        {
            if (string.IsNullOrWhiteSpace(customerId))
            {
                throw new ArgumentNullException("customerId");
            }
            const string QUERY = @"
                       with q1 as (
                       SELECT PS.PO_ID                          AS PO_ID,
                       PS.ITERATION                             AS ITERAION,
                       PS.CUSTOMER_ID                           AS CUSTOMER_ID,
                       EDIPS.CUSTOMER_DC_ID                     AS CUSTOMER_DC_ID,
                       MAX(EDIPS.ATS_DATE)                      AS ATS_DATE,
                       Max(bkt.ship_ia_id)                      as DOOR_ID,
                       ---MAX(EDIPS.EDI_ID)                        AS EDI_ID,
                       SYS.STRAGG(UNIQUE(EDIPS.EDI_ID || ',')) AS edi_ID_LIST,
                       SUM(EDIPS.WEIGHT_IN_LB)                  AS WEIGHT,
                       SUM(EDIPS.VOLUME_IN_CUFT)                AS VOLUME,
                       MAX(EDIPS.LOAD_ID)                       AS LOAD_ID,
                       MAX(EDIPS.CARRIER_ID)                    AS CARRIER_ID,
                       MAX(MC.DESCRIPTION)                      AS CARRIER_DESCRIPTION,
                       MAX(EDIPS.ORIGINAL_CARRIER_ID)           AS ORIGINAL_CARRIER_ID,
                       MAX(OMC.DESCRIPTION)                     AS ORIGINAL_CARRIER_DESCRIPTION,
                       MAX(EDIPS.ORIGINAL_CUSTOMER_DC_ID)       AS ORIGINAL_CUSTOMER_DC_ID,
                       SUM(EDIPS.TOTAL_PIECES)                  AS PIECES,
                       SUM(EDIPS.NUMBER_OF_BOXES)               AS BOXES,
                       MAX(EDIPS.PICKUP_DATE)                   AS PICKUP_DATE,
                       MAX(PS.WAREHOUSE_LOCATION_ID)            AS WAREHOUSE_LOCATION_ID,
                       COUNT(UNIQUE NVL(EDIPS.LOAD_ID,'-1'))    AS LOAD_COUNT,
                       COUNT(UNIQUE NVL(EDIPS.CARRIER_ID, '-1')) AS CARRIER_COUNT,
                       COUNT(UNIQUE NVL(bkt.ship_ia_id, '-1')) AS DOOR_ID_COUNT,
                       COUNT(UNIQUE NVL(EDIPS.PICKUP_DATE, SYSDATE)) AS PICKUP_DATE_COUNT,
                       SYS.STRAGG(UNIQUE(EDIPS.CARRIER_ID || '-')) AS CARRIER_ID_LIST,
                       SYS.STRAGG(UNIQUE(EDIPS.PICKUP_DATE || ',')) AS PICKUP_DATE_LIST,
                       SYS.STRAGG(UNIQUE(EDIPS.LOAD_ID || '-')) AS LOAD_ID_LIST,
                       SYS.STRAGG(UNIQUE(bkt.ship_ia_id || '-')) AS DOOR_ID_LIST,
                       MAX(PO.START_DATE) AS START_DATE,
                       MAX(PO.DC_CANCEL_DATE) AS DCCANCEL_DATE,
                       SUM(PS.TOTAL_DOLLARS_ORDERED) AS ORDERED_DOLLARS,
                       MAX(CUST.ASN_FLAG) AS CUST_ASN_FLAG,
                       COUNT(UNIQUE PS.ITERATION) OVER(PARTITION BY PS.CUSTOMER_ID, PS.PO_ID) AS PO_ITERATION_COUNT,
                       SYS.STRAGG(UNIQUE(PS.WAREHOUSE_LOCATION_ID || '-')) AS BUILDING_LIST,
                       COUNT(unique case when max(edips.load_id) is not null or max(edips.pickup_date) is not null then ps.po_id end) OVER(PARTITION BY EDIPS.ATS_DATE) AS EDI_ROUTEDPO_COUNT,
                       COUNT(unique case when max(edips.load_id) is not null or max(edips.pickup_date) is not null then  NULL ELSE ps.po_id end) OVER(PARTITION BY EDIPS.ATS_DATE) AS EDI_UNROUTEDPO_COUNT
                  FROM <proxy />EDI_753_754_PS EDIPS
                  INNER JOIN <proxy />PS PS
                    ON PS.PICKSLIP_ID = EDIPS.PICKSLIP_ID
                  LEFT OUTER JOIN <proxy />PO PO 
                    ON PO.PO_ID=PS.PO_ID
                    AND PO.CUSTOMER_ID=PS.CUSTOMER_ID
                    AND PO.ITERATION=PS.ITERATION
                  INNER JOIN <proxy />EDI_753_754 EDI
                    ON EDI.EDI_ID = EDIPS.EDI_ID
                    LEFT OUTER JOIN <proxy /> BUCKET BKT
                  ON BKT.BUCKET_ID = PS.BUCKET_ID
                  LEFT OUTER JOIN <proxy />MASTER_CARRIER MC
                    ON EDIPS.CARRIER_ID = MC.CARRIER_ID
                  LEFT OUTER JOIN <proxy />MASTER_CARRIER OMC
                    ON EDIPS.ORIGINAL_CARRIER_ID = OMC.CARRIER_ID
                    LEFT OUTER JOIN <proxy />CUST CUST ON
                    CUST.CUSTOMER_ID=PS.CUSTOMER_ID
                   WHERE PS.TRANSFER_DATE IS NULL
                    AND PS.SHIPPING_ID IS NULL
                    AND PS.PICKSLIP_CANCEL_DATE IS NULL
                   AND EDI.CUSTOMER_ID = :CUSTOMER_ID
                        <if>
                            AND PO.START_DATE &gt;=:START_DATE 
                            AND PO.START_DATE &lt;=:START_DATE + 1 
                        </if>
                         <if>
                            AND PO.PO.DC_CANCEL_DATE &gt;=:DCCANCEL_DATE 
                            AND PO.PO.DC_CANCEL_DATE &lt;=:DCCANCEL_DATE + 1 
                        </if>
                        <if>AND PS.WAREHOUSE_LOCATION_ID =:BUILDING_ID</if>
                  GROUP BY PS.PO_ID,PS.CUSTOMER_ID,PS.ITERATION,EDIPS.ATS_DATE, EDIPS.CUSTOMER_DC_ID
                        )
                        select * from q1
                        Where 1=1 
                        <if c='$routed'>
                                                    <if c='$routed = 1'>
                                                                               AND Q1.EDI_ROUTEDPO_COUNT &gt; 0
                                                    </if>
                                                    <else>
                                                                               AND Q1.EDI_UNROUTEDPO_COUNT &gt; 0
                                                    </else>
                                                </if>
                 ORDER BY ATS_DATE ASC";

            var binder = SqlBinder.Create(row => new RoutablePo
            {
                RoutingKey = RoutingKey.Create(row.GetString("CUSTOMER_ID"), row.GetString("PO_ID"), row.GetInteger("ITERAION").Value, row.GetString("CUSTOMER_DC_ID")),
                EdiIdList = (row.GetString("edi_ID_LIST") ?? "").Split(',').Where(p => !string.IsNullOrWhiteSpace(p)).Select(p => int.Parse(p)).ToArray(),
                AtsDate = row.GetDate("ATS_DATE").Value,
                Weight = row.GetDecimal("WEIGHT"),
                Volume = row.GetDecimal("VOLUME"),
                LoadId = row.GetString("LOAD_ID"),
                CarrierId = row.GetString("CARRIER_ID"),
                OriginalCarrierId = row.GetString("ORIGINAL_CARRIER_ID"),
                OriginalCarrierDescription = row.GetString("ORIGINAL_CARRIER_DESCRIPTION"),
                CustomerDcId = row.GetString("CUSTOMER_DC_ID"),
                Pieces = row.GetInteger("PIECES"),
                CountBoxes = row.GetInteger("BOXES"),
                PickUpDate = row.GetDate("PICKUP_DATE"),
                LoadCount = row.GetInteger("LOAD_COUNT"),
                CarrierCount = row.GetInteger("CARRIER_COUNT"),
                PickUpDateCount = row.GetInteger("PICKUP_DATE_COUNT"),
                CarrierDescription = row.GetString("CARRIER_DESCRIPTION"),
                LoadList = row.GetString("LOAD_ID_LIST"),
                CarrierList = row.GetString("CARRIER_ID_LIST"),
                PickupDateList = row.GetString("PICKUP_DATE_LIST"),
                BuildingId = row.GetString("WAREHOUSE_LOCATION_ID"),
                DoorId = row.GetString("DOOR_ID"),
                DoorCount = row.GetInteger("DOOR_ID_COUNT"),
                DoorList = row.GetString("DOOR_ID_LIST"),
                PoIterationCount = row.GetInteger("PO_ITERATION_COUNT").Value,
                StartDate = row.GetDate("START_DATE"),
                DcCancelDate = row.GetDate("DCCANCEL_DATE"),
                OriginalDCId = row.GetString("ORIGINAL_CUSTOMER_DC_ID"),
                TotalDollars = row.GetDecimal("ORDERED_DOLLARS"),
                EdiRoutedPoCount = row.GetInteger("EDI_ROUTEDPO_COUNT"),
                EdiRoutablePoCount = row.GetInteger("EDI_UNROUTEDPO_COUNT"),
                CustAsnFlag=row.GetString("CUST_ASN_FLAG"),
                BuildingList=row.GetString("BUILDING_LIST")
            }).Parameter("CUSTOMER_ID", customerId)
            .Parameter("BUILDING_ID", buildingId);
            if (routedEdi.HasValue)
            {
                binder.Parameter("routed", routedEdi.Value ? 1 : 2);
            }
            else
            {
                binder.ParameterXPath("routed", false);
            }
            binder.Parameter("START_DATE", startDate);
            binder.Parameter("DCCANCEL_DATE", dcCancelDate);
            return _db.ExecuteReader(QUERY, binder);
        }

        /// <summary>
        /// Removes POs from EDI_753_754_PS tables. Resets the order info(carrier,address,DC) back to the original values. 
        /// </summary>
        /// <param name="key"> </param>
        /// <returns></returns>
        internal void UndoRouting(RoutingKey key)
        {
            const string QUERY = @"
                    begin
                      <proxy/> pkg_edi_2.remove_po_from_edi( aiteration => :aiteration,
                                                   acustomer_id => :acustomer_id,
                                                   apo_id => :apo_id,
                                                   adc_id => :adc_id);
                    end;
            ";
            var binder = SqlBinder.Create().Parameter("PO_ID", key.PoId)
                .Parameter("aiteration", key.Iteration)
                .Parameter("apo_id", key.PoId)
                .Parameter("acustomer_id", key.CustomerId)
                .Parameter("adc_id", key.DcId);
            _db.ExecuteNonQuery(QUERY, binder);

        }

        ///<summary>
        /// Update routing information. To update set the appropriate flag in RoutingUpdater class.   
        /// </summary>
        /// <returns>The number of POs affected.</returns>
        internal int UpdateRouting(RoutingUpdater updater)
        {
            if (!updater.UpdateRequired)
            {
                return 0;
            }

            const string QUERY = @"
                                  DECLARE
                                  arouting_rec <proxy />pkg_edi_2.routing_rec;
                                  LSET_FLAG NUMBER := 0
                                                             <if c='$update_carrier'>+ <proxy />pkg_edi_2.PFLAG_CARRIER_ID</if>
                                                             <if c='$update_load'>+ <proxy />pkg_edi_2.PFLAG_LOAD_ID</if>
                                                             <if c='$update_pickupdate'>+ <proxy />pkg_edi_2.PFLAG_PICKUP_DATE</if>
                                                             <if c='$update_dc'>+ <proxy />pkg_edi_2.PFLAG_CUSTOMER_DC_ID</if>
                                ;
                                BEGIN

                                            arouting_rec.CARRIER_ID     := :CARRIER_ID;
                                            arouting_rec.LOAD_ID        := :LOAD_ID;
                                            arouting_rec.PICKUP_DATE    := :PICKUP_DATE;
                                            arouting_rec.CUSTOMER_DC_ID := :CUSTOMER_DC_ID;
                                       :result := <proxy />PKG_EDI_2.SET_ROUTING_FOR_PO(ACUSTOMER_ID =&gt; :CUSTOMER_ID,
                                                                    APO_LIST =&gt; :APO_LIST,
                                                                    AITERATION_LIST =&gt; :AITERATION_LIST,   
                                                                    Arouting_rec =&gt; arouting_rec,                                                                    
                                                                    ARESET_FLAG =&gt; LSET_FLAG,
                                                                    ADC_LIST =&gt; :ADC_LIST);
                                END;                                
                                 ";
            int poCount = 0;
            var binder = SqlBinder.Create();
            binder.ParameterAssociativeArray("APO_LIST", updater.RoutingKeys.Select(p => p.PoId).ToArray());
            binder.ParameterAssociativeArray("AITERATION_LIST", updater.RoutingKeys.Select(p => (int?)p.Iteration).ToArray());
            binder.Parameter("CUSTOMER_ID", updater.RoutingKeys.Select(p => p.CustomerId).First());
            binder.ParameterAssociativeArray("ADC_LIST", updater.RoutingKeys.Select(p => p.DcId).ToArray());
            binder.Parameter("CARRIER_ID", updater.CarrierId);
            binder.Parameter("LOAD_ID", updater.LoadId);
            binder.Parameter("PICKUP_DATE", updater.PickUpDate);
            binder.Parameter("CUSTOMER_DC_ID", updater.CustomerDcId);
            binder.ParameterXPath("update_carrier", updater.UpdateCarrierId);
            binder.ParameterXPath("update_load", updater.UpdateLoadId);
            binder.ParameterXPath("update_dc", updater.UpdateCustomerDcId);
            binder.ParameterXPath("update_pickupdate", updater.UpdatePickupDate);
            binder.OutParameter("result", values => poCount = values ?? 0);
            _db.ExecuteNonQuery(QUERY, binder);
            return poCount;
        }

        /// <summary>
        /// Gets original values of DC and Carrier 
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="poId"></param>
        /// <param name="iteration"></param>
        /// <returns></returns>
        internal Tuple<string,string> GetDefaultRoutingInfo(string customerId,string poId, int iteration,string dcId)
        {
            if (string.IsNullOrWhiteSpace(customerId))
            {
                throw new ArgumentNullException("customerId");
            }
            const string QUERY = @"
                   SELECT max(EDIPS.ORIGINAL_CUSTOMER_DC_ID) AS CUSTOMER_DC, 
                    max(EDIPS.ORIGINAL_CARRIER_ID) AS CARRIER_ID
                    FROM <proxy />EDI_753_754_PS EDIPS
                    INNER JOIN <proxy />PS PS 
                    ON EDIPS.PICKSLIP_ID=PS.PICKSLIP_ID
                    WHERE PS.PO_ID=:PO_ID
                    AND PS.CUSTOMER_ID=:CUSTOMER_ID
                    AND PS.ITERATION=:ITERATION
                    <if>AND PS.CUSTOMER_DC_ID=:DC_ID</if>";
            var binder = SqlBinder.Create(row => Tuple.Create(row.GetString("CUSTOMER_DC"),row.GetString("CARRIER_ID")));
            binder.Parameter("CUSTOMER_ID", customerId)
                .Parameter("PO_ID",poId)
                .Parameter("ITERATION",iteration)
                .Parameter("DC_ID", dcId);
            return _db.ExecuteSingle(QUERY, binder);
        }

        /// <summary>
        /// Get detail of passed DC
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="customerDC"></param>
        /// <returns></returns>
        internal string GetDC(string customerId, string customerDC)
        {
            if (string.IsNullOrWhiteSpace(customerId))
            {
                throw new ArgumentNullException("customerId");
            }
            const string QUERY = @"
                    SELECT 
                        CST.CUSTOMER_DC_ID AS CUSTOMER_DC
                      FROM <proxy/>CUSTDC CST
                     WHERE CST.CUSTOMER_DC_ID = :CUSTOMER_DC_ID
                       AND CST.CUSTOMER_ID = :CUSTOMER_ID";
            var binder = SqlBinder.Create(row => row.GetString("CUSTOMER_DC"));
            binder.Parameter("CUSTOMER_ID", customerId);
            binder.Parameter("CUSTOMER_DC_ID", customerDC);
            return _db.ExecuteSingle(QUERY, binder);
        }

        #endregion

        #region Routed

        /// <summary>
        /// Creates BOL for passed EDI. 
        /// </summary>
        /// <param name="ediId"> </param>
        /// <remarks>
        /// </remarks>
        /// <return>count: Number of bols created.</return>
        public int CreateBol(int ediId)
        {
            const string QUERY = @"
                            DECLARE
                             LBOL_COUNT  NUMBER(3);
                             BEGIN
                                    :LBOL_COUNT := <proxy />PKG_EDI_2.CREATE_BOL_FOR_EDI(AEDI_ID =>:EDI_ID);                    
                                  END;
                                 ";
            int count = 0;
            var binder = SqlBinder.Create().Parameter("EDI_ID", ediId)
            .OutParameter("LBOL_COUNT", val => count = val.Value);
            _db.ExecuteNonQuery(QUERY, binder);
            return count;
        }
        /// <summary>
        /// Get details of PO for the passed Edi
        /// </summary>
        /// <param name="ediId"></param>
        /// <returns></returns>
        public ICollection<EdiPo> EdiSummary(int[] ediIdList)
        {

            const string QUERY = @"
                                    SELECT PS.PO_ID AS PO_ID,
                                           MAX(EDIPS.LOAD_ID) AS LOAD_ID,                                          
                                           MAX(EDIPS.PICKUP_DATE) AS PICKUP_DATE,                                                                                     
                                           COUNT(UNIQUE NVL(EDIPS.LOAD_ID, '-1')) AS LOADCOUNT,
                                           COUNT(UNIQUE NVL(EDIPS.PICKUP_DATE, SYSDATE)) AS PICKUPDATECOUNT
                                      FROM <proxy />EDI_753_754_PS EDIPS
                                    INNER JOIN <proxy />PS PS
                                        ON PS.PICKSLIP_ID = EDIPS.PICKSLIP_ID
                                    WHERE 1=1
                                    <if><a pre='AND EDIPS.EDI_ID IN (' sep=',' post=')'>:EDI_ID_LIST</a></if>
                                    AND PS.SHIPPING_ID IS NULL
                                    GROUP BY PS.PO_ID, PS.ITERATION, PS.CUSTOMER_ID,PS.CUSTOMER_DC_ID

                                  ";
            var binder = SqlBinder.Create(row => new EdiPo
            {
                Po_Id = row.GetString("PO_ID"),
                Load_Id = row.GetString("LOAD_ID"),             
                PickUp_Date = row.GetDate("PICKUP_DATE"),
                LoadCount = row.GetInteger("LOADCOUNT"),               
                PickUpDateCount = row.GetInteger("PICKUPDATECOUNT")               
            });
            binder.ParameterXmlArray("EDI_ID_LIST", ediIdList);
            return _db.ExecuteReader(QUERY, binder);
        }

        #endregion

        #region BOL

        /// <summary>
        /// Gets unshipped BOLs for the passed customer.
        /// </summary>
        /// <param name="customerId"> </param>
        /// <param name="showScheduled">true -> all unshipped bols, false ->show unscheduled only</param>
        /// <returns></returns>
        /// <Remarks>
        /// We consider a BOL as open till ship.onhold_flag is not null.
        /// </Remarks>
        /// <remarks>
        /// </remarks>
        /// 
        public IEnumerable<Bol> GetBols(string customerId, bool showScheduled)
        {
            if (string.IsNullOrEmpty(customerId))
            {
                throw new ArgumentNullException("customerId");
            }

            const string QUERY = @"
                                SELECT S.PARENT_SHIPPING_ID         AS SHIPPING_ID,
                                       MAX(S.SHIP_DATE)             AS SHIP_DATE,
                                       MAX(S.CARRIER_ID)            AS CARRIER_ID,
                                       MAX(MC.DESCRIPTION)          AS CARRIER_DESCRIPTION,
                                       MAX(PS.WAREHOUSE_LOCATION_ID) AS SHIPBUILDING_ID,
                                       MAX(S.DATE_CREATED)          AS BOL_CREATED_ON,
                                       MAX(S.CREATED_BY)            AS BOL_CREATED_BY,
                                       MAX(PO.START_DATE)           AS START_DATE,
                                       MIN(PO.DC_CANCEL_DATE)       AS DC_CANCEL_DATE,
                                       MAX(PO.CANCEL_DATE)          AS CANCEL_DATE,
                                       MAX(PS.CUSTOMER_DC_ID)       AS CUSTOMER_DC_ID,
                                       COUNT(UNIQUE ps.PO_ID)       AS PO_COUNT,
                                       MAX(MA.APPOINTMENT_NUMBER)   AS APPOINTMENT_NUMBER,
                                       MAX(MA.APPOINTMENT_DATE)     AS APPOINTMENT_DATE_TIME,
                                       MAX(S.APPOINTMENT_ID)        AS APPOINTMENT_ID,
                                       MAX(EDIPS.ATS_DATE)          AS ATS_DATE,
                                       MAX(S.EDI_ID)                AS EDI_ID,
                                       SYS.STRAGG(UNIQUE(EDIPS.PICKUP_DATE) || ',') AS PICKUP_DATE_LIST
                                  FROM  <proxy />SHIP S
                                 LEFT OUTER JOIN  <proxy />PS PS
                                    ON S.SHIPPING_ID = PS.SHIPPING_ID
                                 LEFT OUTER JOIN  <proxy />EDI_753_754_PS EDIPS
                                    ON PS.PICKSLIP_ID = EDIPS.PICKSLIP_ID
                                 LEFT OUTER JOIN <proxy />PO PO
                                    ON PS.CUSTOMER_ID = PO.CUSTOMER_ID
                                    AND PS.PO_ID = PO.PO_ID
                                    AND PS.ITERATION = PO.ITERATION
                                LEFT OUTER JOIN <proxy />MASTER_CARRIER MC
                                    ON S.CARRIER_ID = MC.CARRIER_ID
                                LEFT OUTER JOIN <proxy />APPOINTMENT MA
                                    ON MA.APPOINTMENT_ID = S.APPOINTMENT_ID
                                 WHERE PS.TRANSFER_DATE IS NULL
                                 AND   S.Onhold_Flag is not null
                                 AND PS.PICKSLIP_CANCEL_DATE IS NULL
                                 AND S.CUSTOMER_ID = :CUSTOMER_ID
                                    <if c='$showScheduled'>                                   
                                    </if>
                                    <else>
                                    AND S.APPOINTMENT_ID IS NULL
                                    </else>
                                 GROUP BY S.PARENT_SHIPPING_ID
                                 ORDER BY DC_CANCEL_DATE
                            ";
            var binder = SqlBinder.Create(row => new Bol
            {
                ShipDate = row.GetDate("SHIP_DATE"),
                ShippingId = row.GetString("SHIPPING_ID"),
                CarrierId = row.GetString("CARRIER_ID"),
                BolCreatedBy = row.GetString("BOL_CREATED_BY"),
                BolCreatedOn = row.GetDate("BOL_CREATED_ON"),
                StartDate = row.GetDate("START_DATE"),
                DcCancelDate = row.GetDate("DC_CANCEL_DATE"),
                CancelDate = row.GetDate("CANCEL_DATE"),
                CustomerDcId = row.GetString("CUSTOMER_DC_ID"),
                PoCount = row.GetInteger("PO_COUNT"),
                ShipBuilding = row.GetString("SHIPBUILDING_ID"),
                CarrierDescription = row.GetString("CARRIER_DESCRIPTION"),
                AppointmentNumber = row.GetInteger("APPOINTMENT_NUMBER"),
                AppointmentDateTime = row.GetDate("APPOINTMENT_DATE_TIME"),
                AppointmentId = row.GetInteger("APPOINTMENT_ID"),
                AtsDate = row.GetDate("ATS_DATE"),
                EdiId = row.GetInteger("EDI_ID"),
                PickupDateList = row.GetString("PICKUP_DATE_LIST").Split(',').Where(p => !string.IsNullOrEmpty(p)).Select(p => (DateTime?)DateTime.Parse(p))
            }).Parameter("CUSTOMER_ID", customerId);
            binder.ParameterXPath("showScheduled", showScheduled);
            return _db.ExecuteReader(QUERY, binder);
        }

        /// <summary>
        /// Deletes passed BOL from system. 
        /// </summary>
        /// <param name="shippingId"> </param>
        /// <returns>Returns true if bol deleted successfully else false. </returns>
        public bool DeleteBol(string shippingId)
        {
            const string QUERY = @"
                            BEGIN
                           :result :=  <proxy/>pkg_edi_2.DELETE_BOL_2(AParentshipping_id => :ASHIPPING_ID);
                            END;
                            ";
            var binder = SqlBinder.Create()
                .Parameter("ASHIPPING_ID", shippingId);
            string id = string.Empty;
            binder.OutParameter("result", val => id = val);
            _db.ExecuteDml(QUERY, binder);
            return !string.IsNullOrEmpty(id);
        }

        #endregion

        #region Appointment

        /// <summary>
        /// Returns list of appointments
        /// <param name="customerId"></param> 
        /// <param name="endDate"></param>
        /// <param name="startDate"></param>
        /// <param name="maxRows">number of rows returned by function , if  null then 1 row is returned</param>
        /// <param name="scheduled">If Null show all ,1 show scheduled,2 show unscheduled</param>
        /// <param name="appointmentId"></param>
        /// <param name="buildingIdList"></param>
        /// <param name="carrierId"></param>
        /// <param name="shipped">Null-> show unshipped, true-> show shipped as well</param>
        /// </summary> 
        /// <returns></returns>   
        /// <remarks>
        /// Most recent first.
        /// </remarks>
        public IEnumerable<Appointment> GetAppointments(string customerId = null, DateTimeOffset? startDate = null, DateTimeOffset? endDate = null,
            string[] buildingIdList = null, int? appointmentId = null, int? appointmentNumber = null, string carrierId = null, bool? scheduled=null, bool? shipped=null, int? maxRows = null)
        {
            const string QUERY = @"
                               <if c=""$scheduled='NULL' or $scheduled='1'"">
                               SELECT MAX(APMT.BUILDING_ID) AS BUILDING_ID,
                               SYS.STRAGG(UNIQUE(CUST.NAME || '|')) AS CUSTOMER_LIST,
                               MAX(APMT.APPOINTMENT_DATE) AS APPOINTMENT_DATE,
                               COUNT(UNIQUE PS.PO_ID) AS BOL_PO_COUNT,
                               CAST(NULL AS NUMBER) AS nobol_PO_COUNT,
                               MAX(APMT.APPOINTMENT_ID) AS APPOINTMENT_ID,
                               MAX(APMT.APPOINTMENT_NUMBER) AS APPOINTMENT_NUMBER,
                               MAX(APMT.PICKUP_DOOR) AS PICKUP_DOOR,
                               COUNT(UNIQUE SHIP.SHIPPING_ID) AS BOL_COUNT,
                               COUNT(UNIQUE B.UCC128_ID) AS bol_BOX_COUNT,
                               CAST(NULL AS NUMBER) AS nobol_BOX_COUNT,
                               MAX(APMT.CARRIER_ID) AS CARRIER_ID,
                               MAX(MAC.DESCRIPTION) AS CARRIER_NAME,
                               MAX(APMT.REMARKS) AS REMARKS,
                               MAX(APMT.TRUCK_ARRIVAL_DELAY) AS TRUCK_ARRIVAL_TIME,
                               MAX(CUST.CUSTOMER_ID) AS CUSTOMER_ID,
                               MAX(APMT.ORA_ROWSCN) AS ORA_ROWSCN,                               
                               MAX(CASE WHEN (SHIP.TRANSFER_DATE IS NULL OR (SHIP.SHIP_DATE IS NOT NULL AND SHIP.ONHOLD_FLAG IS NOT NULL)) THEN 1  
                               WHEN (PS.TRANSFER_DATE IS NOT NULL OR (SHIP.SHIP_DATE IS NOT NULL AND SHIP.ONHOLD_FLAG IS NULL)) THEN 2 END) AS SHIPPED_STATUS
                          FROM <proxy />APPOINTMENT APMT
                          LEFT OUTER JOIN <proxy />MASTER_CARRIER MAC
                            ON MAC.CARRIER_ID = APMT.CARRIER_ID
                          LEFT OUTER JOIN <proxy />SHIP SHIP
                            ON SHIP.APPOINTMENT_ID = APMT.APPOINTMENT_ID
                          LEFT OUTER JOIN <proxy />PS PS
                            ON PS.SHIPPING_ID = SHIP.SHIPPING_ID
                          LEFT OUTER JOIN <proxy />BOX B
                            ON B.PICKSLIP_ID = PS.PICKSLIP_ID
                          LEFT OUTER JOIN <proxy />CUST CUST
                            ON SHIP.CUSTOMER_ID = CUST.CUSTOMER_ID
                                    WHERE 1=1
                                   <if>AND APMT.APPOINTMENT_DATE &gt;=:START_DATE</if>
                                   <if>AND  APMT.APPOINTMENT_DATE &lt;:END_DATE</if>                                 
                                   <if><a pre='AND APMT.BUILDING_ID IN (' sep=',' post=')'>:BUILDING_ID_LIST</a></if>
                                   <if>
                                    and APMT.APPOINTMENT_ID in 
                                            (select s.APPOINTMENT_ID FROM <proxy />ship s 
                                                where s.customer_id = :customer_id 
                                                  and s.APPOINTMENT_ID is not null)
                                  </if>
                                  <if>AND APMT.APPOINTMENT_ID=:APPOINTMENT_ID</if>
                                  <if>AND APMT.APPOINTMENT_NUMBER=:APPOINTMENT_NUMBER</if>
                                  <if>AND APMT.CARRIER_ID=:CARRIER_ID</if>
                                  <if c=""$shipped='1'""></if>
                                  <else>AND (SHIP.TRANSFER_DATE IS NULL OR (SHIP.SHIP_DATE IS NOT NULL AND SHIP.ONHOLD_FLAG IS NOT NULL))
                                    AND PS.TRANSFER_DATE IS NULL 
                                   AND B.STOP_PROCESS_DATE IS NULL                             
                                   AND PS.PICKSLIP_CANCEL_DATE IS NULL</else>
                            
                                GROUP BY APMT.APPOINTMENT_ID 
                                </if>
                                <if c=""$scheduled='NULL'"" >
                                UNION </if>
                                <if c=""$scheduled='NULL' or $scheduled='2'"">
                                SELECT PS.WAREHOUSE_LOCATION_ID AS BUILDING_ID,
                                       SYS.STRAGG(UNIQUE(CUST.NAME || '|')) AS CUSTOMER_LIST,
                                       CAST(EDIPS.ATS_DATE AS TIMESTAMP(6) WITH TIME ZONE) AS APPOINTMENT_DATE,
                                       COUNT(UNIQUE case when ps.shipping_id is not null then PS.PO_ID end) AS BOL_PO_COUNT,
                                       COUNT(unique case when ps.shipping_id is null then PS.PO_ID end) AS nobol_PO_COUNT,
                                       CAST(NULL AS NUMBER) AS APPOINTMENT_ID,
                                       CAST(NULL AS NUMBER) AS APPOINTMENT_NUMBER,
                                       CAST(NULL AS VARCHAR(2)) AS PICKUP_DOOR,
                                       COUNT(UNIQUE SHIP.SHIPPING_ID) AS BOL_COUNT,
                                       COUNT(unique case when ps.shipping_id is not null then B.UCC128_ID end) AS bol_BOX_COUNT,
                                       COUNT(unique case when ps.shipping_id is null then B.UCC128_ID end) AS nobol_BOX_COUNT,
                                       CAST(NULL AS VARCHAR(2)) AS CARRIER_ID,
                                       CAST(NULL AS VARCHAR(2)) AS CARRIER_NAME,
                                       CAST(NULL AS VARCHAR(2)) AS REMARKS,
                                       CAST(NULL AS INTERVAL DAY(2) TO SECOND(6)) TRUCK_ARRIVAL_TIME,
                                       MAX(PS.CUSTOMER_ID) AS CUSTOMER_ID,
                                       CAST(NULL AS DECIMAL) AS ORA_ROWSCN,
                                       MAX(CASE WHEN PS.TRANSFER_DATE IS NULL THEN 1  
                                       WHEN (PS.TRANSFER_DATE IS NOT NULL OR (SHIP.SHIP_DATE IS NOT NULL AND SHIP.ONHOLD_FLAG IS NULL)) THEN 2 END) AS SHIPPED_STATUS
                                  FROM <proxy />EDI_753_754_PS EDIPS
                                 INNER JOIN <proxy />PS PS
                                    ON PS.PICKSLIP_ID = EDIPS.PICKSLIP_ID
                                  LEFT OUTER JOIN <proxy />BOX B
                                    ON B.PICKSLIP_ID = PS.PICKSLIP_ID
                                  LEFT OUTER JOIN <proxy />SHIP SHIP
                                    ON PS.SHIPPING_ID = SHIP.SHIPPING_ID
                                  LEFT OUTER JOIN <proxy />CUST CUST
                                    ON PS.CUSTOMER_ID = CUST.CUSTOMER_ID
                                WHERE 1=1
                                    <if c=""$shipped='1'""></if>
                                    <else>AND (SHIP.TRANSFER_DATE IS NULL OR (SHIP.SHIP_DATE IS NOT NULL AND SHIP.ONHOLD_FLAG IS NOT NULL))
                                    AND PS.TRANSFER_DATE IS NULL 
                                   AND B.STOP_PROCESS_DATE IS NULL                             
                                   AND PS.PICKSLIP_CANCEL_DATE IS NULL</else>
                                    <if>AND EDIPS.ATS_DATE &gt;=:START_DATE</if>
                                    <if>AND EDIPS.ATS_DATE &lt;:END_DATE</if>  
                                    <if><a pre='AND PS.WAREHOUSE_LOCATION_ID IN (' sep=',' post=')'>:BUILDING_ID_LIST</a></if>
                                    <if c='$APPOINTMENT_ID'>AND 1=2</if>
                                    <if c='$APPOINTMENT_NUMBER'>AND 1=2</if>
                                    <if c='$CARRIER_ID'>AND 1=2</if>
                                    <if> AND PS.CUSTOMER_ID=:CUSTOMER_ID</if>
                                AND B.STOP_PROCESS_DATE IS NULL  
                                AND (PS.SHIPPING_ID IS NULL OR  SHIP.APPOINTMENT_ID IS NULL) 
                                GROUP BY EDIPS.ATS_DATE,PS.WAREHOUSE_LOCATION_ID
                                </if>                            
                                order by APPOINTMENT_DATE desc
                                ";
            var binder = SqlBinder.Create(row => new Appointment
                {
                    AppointmentId = row.GetInteger("APPOINTMENT_ID"),
                    AppointmentNumber = row.GetInteger("APPOINTMENT_NUMBER"),
                    AppointmentTime = row.GetTimeStampTZ("APPOINTMENT_DATE"),
                    BuildingId = row.GetString("BUILDING_ID"),
                    PickUpDoor = row.GetString("PICKUP_DOOR"),
                    CarrierId = row.GetString("CARRIER_ID"),
                    BolCount = row.GetInteger("BOL_COUNT"),
                    Remarks = row.GetString("REMARKS"),
                    ArrivalDelay = row.GetInterval("TRUCK_ARRIVAL_TIME"),
                    CarrierName = row.GetString("CARRIER_NAME"),
                    CustomerNames = row.GetString("CUSTOMER_LIST"),
                    BolBoxCount = row.GetInteger("bol_BOX_COUNT"),
                    NoBolBoxCount = row.GetInteger("nobol_BOX_COUNT"),
                    BolPoCount = row.GetInteger("BOL_PO_COUNT"),
                    NoBolPoCount = row.GetInteger("nobol_PO_COUNT"),
                    RowSequence = row.GetDecimal("ORA_ROWSCN"),
                    CustomerId=row.GetString("CUSTOMER_ID"),
                    IsShipped=row.GetInteger("SHIPPED_STATUS").Value ==1 ? false : true
                }).Parameter("START_DATE", startDate)
                .Parameter("END_DATE", endDate)
                .Parameter("CUSTOMER_ID", customerId)
                .Parameter("APPOINTMENT_ID", appointmentId)
                .Parameter("APPOINTMENT_NUMBER", appointmentNumber);
            if (scheduled.HasValue)
            {
                binder.Parameter("scheduled", scheduled.Value ? 1 : 2);
            }
            else
            {
                binder.Parameter("scheduled", "NULL");
            }
            if (shipped.HasValue)
            {
                binder.Parameter("shipped", 1);
            }
            else
            {
                binder.ParameterXPath("shipped", false);
            }
            binder.Parameter("CARRIER_ID", carrierId);
            binder.ParameterXmlArray("BUILDING_ID_LIST", buildingIdList);           
            return _db.ExecuteReader(QUERY, binder, maxRows.HasValue ? maxRows.Value : 1);
        }

        /// <summary>
        /// Assign appointment to BOLs in list.
        /// </summary>
        /// <param name="shippingIdList"></param>
        /// <param name="appointmentId"></param>
        public void AssignAppointmentToBol(ICollection<string> shippingIdList, int? appointmentId)
        {
            const string QUERY = @"
                            DECLARE
                              ASHIPPINGIDLIST <proxy />PKG_APPOINTMENT.SHIPPINGID_TABLE_T;
                            BEGIN
                              ASHIPPINGIDLIST := <proxy />PKG_APPOINTMENT.SHIPPINGID_TABLE_T(<a sep = ','>:SHIPPINGIDLIST</a>);
                              <proxy />PKG_APPOINTMENT.SET_APPOINTMENT_FOR_BOLS_2(APARENTSHIPPINGLIST =>  ASHIPPINGIDLIST,
                                                              AAPPOINTMENT_ID => :AAPPOINTMENT_ID);
                            END;
                            ";

            var binder = SqlBinder.Create().Parameter("AAPPOINTMENT_ID", appointmentId);
            binder.ParameterXmlArray("SHIPPINGIDLIST", shippingIdList);
            _db.ExecuteNonQuery(QUERY, binder);
        }

        /// <summary>
        /// Create new appointment
        /// </summary>
        /// <param name="app"></param> 
        public void CreateAppointment(Appointment app)
        {
            const string QUERY = @"
                            DECLARE
                              LAPPOINTMENT_ID NUMBER;
                            BEGIN
                              :LAPPOINTMENT_ID := <proxy />PKG_APPOINTMENT.CREATE_APPOINTMENT(AAPPOINTMENT_DATE =&gt; :APPOINTMENT_DATE,
                                                                                          ACARRIER_ID       =&gt; :CARRIER_ID,
                                                                                          ABUILDING         =&gt; :BUILDING_ID,
                                                                                          APICKUPDOOR       =&gt; :PICKUP_DOOR,
                                                                                          AREMARKS          =&gt; :REMARKS);
                            END;";

            var binder = SqlBinder.Create()
               .Parameter("APPOINTMENT_DATE", app.AppointmentTime)
               .Parameter("BUILDING_ID", app.BuildingId)
               .Parameter("PICKUP_DOOR", app.PickUpDoor)
               .Parameter("CARRIER_ID", app.CarrierId)
               .Parameter("REMARKS", app.Remarks);
            int id = 0;
            binder.OutParameter("LAPPOINTMENT_ID", val => id = val.Value);
            _db.ExecuteNonQuery(QUERY, binder);
            app.AppointmentId = id;
            return;
        }

        /// <summary>
        /// Update existing appointment. Does not update the ArrivalDelay.
        /// </summary>
        /// <remarks>
        /// If any error occures in updating the function raises custom error. If no error is raised the update is sucessfull.
        /// </remarks>
        /// <param name="app"></param>
        public void UpdateAppointment(Appointment app)
        {
            const string QUERY = @"
                                    BEGIN
                                                        <proxy />PKG_APPOINTMENT.UPDATE_APPOINTMENT(AAPPOINTMENT_ID    =&gt; :APPOINTMENT_ID,
                                                                  AAPPOINTMENT_DATE =&gt; :APPOINTMENT_DATE,
                                                                   ACARRIER_ID       =&gt; :CARRIER_ID,
                                                                   ABUILDING         =&gt; :BUILDING_ID,
                                                                   APICKUPDOOR       =&gt; :PICKUP_DOOR,
                                                                   AREMARKS          =&gt; :REMARKS,
                                                                   AROW_SEQ          =&gt; :ROW_SEQ);
                                    END;";
            var binder = SqlBinder.Create()
               .Parameter("APPOINTMENT_DATE", app.AppointmentTime)
               .Parameter("BUILDING_ID", app.BuildingId)
               .Parameter("PICKUP_DOOR", app.PickUpDoor)
               .Parameter("CARRIER_ID", app.CarrierId)
               .Parameter("REMARKS", app.Remarks)
               .Parameter("APPOINTMENT_ID", app.AppointmentId)
               .Parameter("ROW_SEQ", app.RowSequence);
            _db.ExecuteNonQuery(QUERY, binder);
        }

        /// <summary>
        /// Delete an appointment.
        /// </summary>
        /// <param name="appointmentId"> </param>
        public void DeleteAppointment(int appointmentId)
        {
            const string QUERY = @"
                                    BEGIN
                                      <proxy />PKG_APPOINTMENT.DELETE_APPOINTMENT(  AAPPOINTMENT_ID =&gt; :APPOINTMENT_ID);
                                    END;";
            var binder = SqlBinder.Create()
                .Parameter("APPOINTMENT_ID", appointmentId);
            _db.ExecuteNonQuery(QUERY, binder);
        }

        /// <summary>
        /// Getting list of Building.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<CodeDescriptionModel> GetBuildingList()
        {
            const string QUERY = @"
            SELECT TWL.WAREHOUSE_LOCATION_ID AS WAREHOUSE_LOCATION_ID,
                    TWL.DESCRIPTION          AS DESCRIPTION
              FROM <proxy />TAB_WAREHOUSE_LOCATION TWL
             ORDER BY WAREHOUSE_LOCATION_ID
            ";
            var binder = SqlBinder.Create(row => new CodeDescriptionModel
                {
                    Code = row.GetString("WAREHOUSE_LOCATION_ID"),
                    Description = row.GetString("DESCRIPTION")
                });
            return _db.ExecuteReader(QUERY, binder);
        }

        /// <summary>
        ///  Retrieves the BOL details of the passed appointments. 
        /// </summary>
        /// <param name="appointmentIdList"></param>
        /// <param name="shipped">Null->Get unshipped BOLs ,true->GEt Shipped also</param>
        /// <returns></returns>
        public IEnumerable<AppointmentBol> GetScheduledAppointmentBols(IEnumerable<int> appointmentIdList ,bool? shipped)
        {
            if (!appointmentIdList.Any())
            {
                throw new ArgumentNullException("appointmentIdList");
            }
            const string QUERY = @"
                                 SELECT  MA.APPOINTMENT_ID AS APPOINTMENT_ID,
                                         MAX(MA.APPOINTMENT_NUMBER) AS APPOINTMENT_NUMBER,
                                         SHIP.PARENT_SHIPPING_ID AS SHIPPING_ID,
                                         MAX(SHIP.CUSTOMER_ID) AS CUSTOMER_ID,
                                         MAX(MC.NAME) AS CUSTOMER_NAME,
                                         COUNT(UNIQUE CASE
                                                 WHEN B.VERIFY_DATE IS NULL AND B.TRUCK_LOAD_DATE IS NULL THEN
                                                  B.UCC128_ID
                                               END) AS UNVERIFY_BOX_COUNT,
                                        COUNT(UNIQUE CASE
                                                    WHEN B.PALLET_ID IS NULL THEN B.UCC128_ID
                                                END) AS UNPALLETIZE_BOX_COUNT,
                                         COUNT(UNIQUE CASE
                                                 WHEN B.VERIFY_DATE IS NOT NULL AND B.TRUCK_LOAD_DATE IS NULL THEN
                                                  B.UCC128_ID
                                               END) AS AT_DOCK_BOX_COUNT,
                                         COUNT(UNIQUE CASE
                                                 WHEN B.TRUCK_LOAD_DATE IS NOT NULL THEN
                                                  B.UCC128_ID
                                               END) AS LOADED_BOX_COUNT,
                                         COUNT(UNIQUE CASE
                                                 WHEN B.TRUCK_LOAD_DATE IS NOT NULL THEN
                                                  B.PALLET_ID
                                               END) AS LOADED_PALLET_COUNT,
                                         COUNT(UNIQUE(B.PALLET_ID)) AS TOTAL_PALLET,
                                         MAX(MA.APPOINTMENT_DATE) AS APPOINTMENT_DATE,
                                         MIN(B.TRUCK_LOAD_DATE) AS TRUCK_LOAD_START,
                                         MAX(B.TRUCK_LOAD_DATE) AS TRUCK_LOAD_END,                                       
                                         CAST(NULL AS NUMBER) AS nobol_PO_COUNT,
                                         COUNT(UNIQUE PS.PO_ID) AS BOL_PO_COUNT
                                    FROM <proxy />SHIP SHIP 
                                    INNER JOIN <proxy />APPOINTMENT MA
                                      ON SHIP.APPOINTMENT_ID = MA.APPOINTMENT_ID
                                    LEFT OUTER JOIN <proxy />PS PS
                                      ON SHIP.SHIPPING_ID = PS.SHIPPING_ID
                                    LEFT OUTER JOIN <proxy />BOX B
                                      ON B.PICKSLIP_ID = PS.PICKSLIP_ID
                                    LEFT OUTER JOIN <proxy />MASTER_CUSTOMER MC
                                      ON MC.CUSTOMER_ID = SHIP.CUSTOMER_ID
                                   WHERE 1=1
                                   <if c=""$shipped='1'"">AND (PS.TRANSFER_DATE IS NULL  OR (PS.TRANSFER_DATE IS NOT NULL OR (SHIP.SHIP_DATE IS NOT NULL AND SHIP.ONHOLD_FLAG IS NULL)))</if>
                                   <else>AND (SHIP.TRANSFER_DATE IS NULL OR (SHIP.SHIP_DATE IS NOT NULL AND SHIP.ONHOLD_FLAG IS NOT NULL))
                                    AND PS.TRANSFER_DATE IS NULL 
                                   AND B.STOP_PROCESS_DATE IS NULL                             
                                   AND PS.PICKSLIP_CANCEL_DATE IS NULL</else>
                            <a pre='AND MA.APPOINTMENT_ID IN (' sep=',' post=')'>:APPOINTMENT_ID_LIST</a>                          
                            GROUP BY SHIP.PARENT_SHIPPING_ID,MA.APPOINTMENT_ID
                            ORDER BY MAX(MA.APPOINTMENT_DATE), MA.APPOINTMENT_ID";
            var binder = SqlBinder.Create(row => new AppointmentBol
            {
                AppointmentNumber = row.GetInteger("APPOINTMENT_NUMBER") ?? 0,
                CustomerId = row.GetString("CUSTOMER_ID"),
                CustomerName = row.GetString("CUSTOMER_NAME"),
                AppointmentId = row.GetInteger("APPOINTMENT_ID").Value,
                AppointmentDateTime = row.GetDate("APPOINTMENT_DATE").Value,
                TotalPalletCount = row.GetInteger("TOTAL_PALLET"),
                UnverifiedBoxCount = row.GetInteger("UNVERIFY_BOX_COUNT"),
                LoadedPalletCount = row.GetInteger("LOADED_PALLET_COUNT"),
                LoadedBoxCount = row.GetInteger("LOADED_BOX_COUNT"),
                AtDockBoxCount = row.GetInteger("AT_DOCK_BOX_COUNT"),
                StartTime = row.GetDate("TRUCK_LOAD_START"),
                EndTime = row.GetDate("TRUCK_LOAD_END"),
                ShippingId = row.GetString("SHIPPING_ID"),
                UnpalletizeBoxCount = row.GetInteger("UNPALLETIZE_BOX_COUNT"),                
                NoBolPoCount = row.GetInteger("NOBOL_PO_COUNT"),
                BolPoCount=row.GetInteger("BOL_PO_COUNT")
            });
            binder.ParameterXmlArray("APPOINTMENT_ID_LIST", appointmentIdList);
            if (shipped.HasValue)
            {
                binder.Parameter("shipped", 1);
            }
            else
            {
                binder.ParameterXPath("shipped",false);
            }
            return _db.ExecuteReader(QUERY, binder, 1000);
        }

        /// <summary>
        /// Update the truck arrival time.
        /// </summary>
        /// <param name="appointmentId"></param>
        /// <param name="arrivalTime"></param>
        public void UpdateArrivalTime(int appointmentId, TimeSpan? arrivalTime)
        {
            const string QUERY = @"
                                   DECLARE
                                    APPOINTMENT_DELAY NUMBER;   
                                    BEGIN
                                      APPOINTMENT_DELAY := <proxy/>PKG_APPOINTMENT.SET_ARRIVAL_DELAY(AAPPOINTMENT_ID =&gt; :APPOINTMENT_ID,
                                                                                   AAPPOINTMENT_DELAY =&gt; :ARRIVAL_INTERVAL);
                                    END;
                                ";
            var binder = SqlBinder.Create()
            .Parameter("ARRIVAL_INTERVAL", arrivalTime)
            .Parameter("APPOINTMENT_ID", appointmentId);
            _db.ExecuteNonQuery(QUERY, binder);
        }

        /// <summary>
        /// Unassign appointment from passed BOLs  
        /// </summary>
        /// <param name="shippingIdList"></param>
        public void UnAssignAppointment(ICollection<string> shippingIdList)
        {
            const string QUERY = @"
                            DECLARE
                              ASHIPPINGIDLIST <proxy />PKG_APPOINTMENT.SHIPPINGID_TABLE_T;
                            BEGIN
                              ASHIPPINGIDLIST := <proxy />PKG_APPOINTMENT.SHIPPINGID_TABLE_T(<a sep = ','>:SHIPPINGIDLIST</a>);
                              <proxy />PKG_APPOINTMENT.UNSET_APPOINTMENT_FROM_BOL_2(APARENTSHIPPINGLIST =>  ASHIPPINGIDLIST);
                            END;
                                ";
            var binder = SqlBinder.Create();
            binder.ParameterXmlArray("SHIPPINGIDLIST", shippingIdList);
            _db.ExecuteNonQuery(QUERY, binder);

        }

        /// <summary>
        /// Gets list of BOls which are part of unscheduled appointments 
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="shipped">Null show unshipped ,true show shipped also</param>
        /// <returns></returns>
        public IEnumerable<AppointmentBol>  GetUnscheduledAppointmentBols(DateTimeOffset start,DateTimeOffset end, bool? shipped)
        {
            const string QUERY = @"SELECT 
                                           max(CUST.NAME)  AS CUSTOMER_NAME,
                                           CAST(EDIPS.ATS_DATE AS TIMESTAMP(6) WITH TIME ZONE) AS APPOINTMENT_DATE,
                                           CAST(NULL AS NUMBER) AS APPOINTMENT_ID,
                                           CAST(NULL AS NUMBER) AS APPOINTMENT_NUMBER,       
                                           PS.CUSTOMER_ID AS CUSTOMER_ID,
                                           PS.WAREHOUSE_LOCATION_ID AS BUILDING_ID,
                                           SHIP.PARENT_SHIPPING_ID as Shipping_id,                                          
                                           COUNT(unique case when ps.shipping_id is null then PS.PO_ID end) AS nobol_PO_COUNT,
                                           COUNT(unique case when ps.shipping_id is not null then PS.PO_ID end) AS bol_PO_COUNT,
                                           COUNT(UNIQUE CASE
                                                    WHEN B.VERIFY_DATE IS NULL AND B.TRUCK_LOAD_DATE IS NULL THEN
                                                     B.UCC128_ID
                                                  END) AS UNVERIFY_BOX_COUNT,
                                            COUNT(UNIQUE CASE
                                                    WHEN B.PALLET_ID IS NULL THEN
                                                     B.UCC128_ID
                                                  END) AS UNPALLETIZE_BOX_COUNT,
                                            COUNT(UNIQUE CASE
                                                    WHEN B.VERIFY_DATE IS NOT NULL AND B.TRUCK_LOAD_DATE IS NULL THEN
                                                     B.UCC128_ID
                                                  END) AS AT_DOCK_BOX_COUNT,
                                            COUNT(UNIQUE CASE
                                                    WHEN B.TRUCK_LOAD_DATE IS NOT NULL THEN
                                                     B.UCC128_ID
                                                  END) AS LOADED_BOX_COUNT,
                                            COUNT(UNIQUE CASE
                                                    WHEN B.TRUCK_LOAD_DATE IS NOT NULL THEN
                                                     B.PALLET_ID
                                                  END) AS LOADED_PALLET_COUNT,
                                            COUNT(UNIQUE(B.PALLET_ID)) AS TOTAL_PALLET,                                                                                      
                                            MIN(B.TRUCK_LOAD_DATE) AS TRUCK_LOAD_START,
                                            MAX(B.TRUCK_LOAD_DATE) AS TRUCK_LOAD_END
                                      FROM  <proxy />EDI_753_754_PS EDIPS
                                    INNER JOIN  <proxy />PS PS
                                        ON PS.PICKSLIP_ID = EDIPS.PICKSLIP_ID
                                      LEFT OUTER JOIN  <proxy />BOX B
                                        ON B.PICKSLIP_ID = PS.PICKSLIP_ID
                                      LEFT OUTER JOIN  <proxy />SHIP SHIP
                                        ON PS.SHIPPING_ID = SHIP.SHIPPING_ID
                                      LEFT OUTER JOIN  <proxy />CUST CUST
                                        ON PS.CUSTOMER_ID = CUST.CUSTOMER_ID
                                    WHERE 1=1
                                    <if c=""$shipped='1'""></if>
                                    <else>AND PS.TRANSFER_DATE IS NULL</else>
                                    and ship.appointment_id is null
                                   <if>AND EDIPS.ATS_DATE &gt;=:START_DATE</if>
                                   <if>AND EDIPS.ATS_DATE &lt;:END_DATE</if>  
                                    group by edips.ats_date,SHIP.PARENT_SHIPPING_ID,ps.warehouse_location_id,PS.CUSTOMER_ID
                                ";
            var binder = SqlBinder.Create(row => new AppointmentBol
            {
                AppointmentNumber = row.GetInteger("APPOINTMENT_NUMBER"),
                CustomerId = row.GetString("CUSTOMER_ID"),
                CustomerName = row.GetString("CUSTOMER_NAME"),
                AppointmentId = row.GetInteger("APPOINTMENT_ID"),
                AppointmentDateTime = row.GetDate("APPOINTMENT_DATE").Value,
                TotalPalletCount = row.GetInteger("TOTAL_PALLET"),
                UnverifiedBoxCount = row.GetInteger("UNVERIFY_BOX_COUNT"),
                LoadedPalletCount = row.GetInteger("LOADED_PALLET_COUNT"),
                LoadedBoxCount = row.GetInteger("LOADED_BOX_COUNT"),
                AtDockBoxCount = row.GetInteger("AT_DOCK_BOX_COUNT"),
                StartTime = row.GetDate("TRUCK_LOAD_START"),
                EndTime = row.GetDate("TRUCK_LOAD_END"),
                ShippingId = row.GetString("SHIPPING_ID"),
                UnpalletizeBoxCount = row.GetInteger("UNPALLETIZE_BOX_COUNT"),
                BuildingId=row.GetString("BUILDING_ID"),               
                NoBolPoCount = row.GetInteger("NOBOL_PO_COUNT"),
                BolPoCount = row.GetInteger("BOL_PO_COUNT")
            });
            binder.Parameter("START_DATE", start);
            binder.Parameter("END_DATE", end);
            if (shipped.HasValue)
            {
                binder.Parameter("shipped", 1);
            }
            else
            {
                binder.ParameterXPath("shipped",false);
            }
            return _db.ExecuteReader(QUERY, binder, 1000);
        }

        #endregion

        #region PoSearch

        /// <summary>
        /// Resturns status of passed PO i.e Unrouted,Routed etc
        /// </summary>
        /// <param name="poPattern"></param>
        /// <returns></returns>
        public IList<PoStatus> GetPoStatus(string poPattern)
        {
                     const string Query = @"
                                SELECT PS.CUSTOMER_ID AS CUSTOMER_ID,
                                       PS.PO_ID AS PO_ID,
                                       PS.CUSTOMER_DC_ID AS DC_ID,
                                       MAX(CUST.NAME) AS CUST_NAME,
                                       MAX(PS.WAREHOUSE_LOCATION_ID) AS BUILDING_ID,
                                       MAX(EDIPS.ATS_DATE) AS ATS_DATE,
                                       COUNT(UNIQUE CASE
                                               WHEN EDIPS.EDI_ID IS NULL AND PS.SHIPPING_ID IS NULL THEN
                                                PS.PO_ID
                                             END) AS COUNT_UNROUTED_PO,
                                       COUNT(UNIQUE CASE
                                               WHEN EDIPS.EDI_ID IS NOT NULL AND
                                                    (EDIPS.PICKUP_DATE IS NULL AND EDIPS.LOAD_ID IS NULL) AND
                                                    PS.SHIPPING_ID IS NULL THEN
                                                PS.PO_ID
                                             END) AS COUNT_RIP_PO,
       
                                       COUNT(UNIQUE CASE
                                               WHEN (EDIPS.PICKUP_DATE IS NOT NULL OR EDIPS.LOAD_ID IS NOT NULL) AND
                                                    PS.SHIPPING_ID IS NULL THEN
                                                PS.PO_ID
                                             END) AS COUNT_ROUTED_PO,
       
                                       MAX(UNIQUE CASE
                                               WHEN SHIP.ONHOLD_FLAG IS NOT NULL THEN
                                                PS.SHIPPING_ID
                                             END) AS SHIPPING_ID,
                                        MAX(PO.DC_CANCEL_DATE) AS DC_CANCEL_DATE         
                                  FROM  <proxy />PS PS
                                 INNER JOIN  <proxy />BUCKET B
                                    ON B.BUCKET_ID = PS.BUCKET_ID
                                 LEFT OUTER JOIN <proxy />PO PO
                                    ON PO.CUSTOMER_ID = PS.CUSTOMER_ID
                                     AND PO.PO_ID = PS.PO_ID
                                     AND PO.ITERATION = PS.ITERATION
                                  LEFT OUTER JOIN  <proxy />CUST CUST
                                    ON CUST.CUSTOMER_ID = PS.CUSTOMER_ID
                                  LEFT OUTER JOIN  <proxy />EDI_753_754_PS EDIPS
                                    ON PS.PICKSLIP_ID = EDIPS.PICKSLIP_ID
                                  LEFT OUTER JOIN <proxy />SHIP SHIP
                                    ON PS.SHIPPING_ID = SHIP.SHIPPING_ID 
                                 WHERE PS.TRANSFER_DATE IS NULL
                                   AND PS.PICKSLIP_CANCEL_DATE IS NULL
                                   AND (SHIP.ONHOLD_FLAG IS NOT NULL OR SHIP.SHIP_DATE IS NULL)
                                   AND B.AVAILABLE_FOR_PITCHING = 'Y'
                                   AND PS.PO_ID like '%' || :PO_ID || '%'
                                 GROUP BY PS.CUSTOMER_ID, PS.PO_ID,PS.CUSTOMER_DC_ID
                                ";

            var binder = SqlBinder.Create(row => new PoStatus
            {
                PoId = row.GetString("PO_ID"),
                CustomerId = row.GetString("CUSTOMER_ID"),
                CustomerName = row.GetString("CUST_NAME"),
                DcId = row.GetString("DC_ID"),
                BuildingId = row.GetString("BUILDING_ID"),
                AtsDate = row.GetDate("ATS_DATE"),
                CountUntoutedPO = row.GetInteger("COUNT_UNROUTED_PO"),
                CountRoutingInprogressPo = row.GetInteger("COUNT_RIP_PO"),
                CountRoutedPO = row.GetInteger("COUNT_ROUTED_PO"),
                ShippingId = row.GetString("SHIPPING_ID"),
                DCCancelDate = row.GetDate("DC_CANCEL_DATE")
            });
            binder.Parameter("PO_ID", poPattern);
            return _db.ExecuteReader(Query, binder);
        }

        #endregion
    }
}