using DcmsMobile.Inquiry.Areas.Inquiry.SharedViews;
using DcmsMobile.Inquiry.Helpers;
using EclipseLibrary.Oracle;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.Contracts;
using System.Web;

namespace DcmsMobile.Inquiry.Areas.Inquiry.CustomerEntity
{
    internal class CustomerEntityRepository : IDisposable
    {
        private readonly OracleDatastore _db;
        public CustomerEntityRepository(string userName, string clientInfo)
        {
            _db = new OracleDatastore(HttpContext.Current.Trace);
            _db.CreateConnection(ConfigurationManager.ConnectionStrings["dcms8"].ConnectionString, userName);
            _db.ModuleName = "Inquiry_CustomerEntity";
            _db.ClientInfo = clientInfo;
        }

        public void Dispose()
        {
            _db.Dispose();
        }

        /// <summary>
        /// return the customer info against scanned customer id.
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        public Customer GetCustomerInfo(string customerId)
        {
            Contract.Assert(_db != null);
            const string QUERY_CUSTOMER_DETAIL = @"
                            with PIVOT_CUST_SPLH(CUSTOMER_ID,
                            EDI753,
                            AMS,
                            SCO) AS
                             (SELECT *
                                FROM (SELECT CUSTOMER_ID, SPLH_ID, SPLH_VALUE FROM <proxy />CUSTSPLH) PIVOT(MAX(SPLH_VALUE) FOR SPLH_ID IN('$EDI753',
                                                                                                                                  '$AMS',
                                                                                                                                  '$SCO'))
                               where CUSTOMER_ID = :CUSTOMER_ID),
                            pivot_cust_doc(
                            NUMBER_OF_MPS,
                            MPS_SHORT_NAME,
                            NUMBER_OF_PSPB,
                            PSPB_SHORT_NAME,
                            NUMBER_OF_UCC,
                            UCC_SHORT_NAME,
                            NUMBER_OF_CCL,
                            CCL_SHORT_NAME,
                            NUMBER_OF_SHLBL,
                            SHLBL_SHORT_NAME
                            ) AS
                             (

                            SELECT *
                              FROM (SELECT d.DOCUMENT_ID,
                             d.short_description as short_description,
                                           case
                                             when cd.document_id is not null then
                                              cd.NUMBER_OF_COPIES
                                             else
                                              d.default_no_of_copies
                                           end as NUMBER_OF_COPIES
                                      FROM <proxy />doc d
                                      left outer join <proxy/>CUSTDOC cd
                                        on cd.document_id = d.document_id
                                       and cd.customer_id = :CUSTOMER_ID
                                       and cd.number_of_copies is not null) PIVOT(MAX(NUMBER_OF_COPIES),  MAX(short_description) as x FOR DOCUMENT_ID IN('$MPS',
                                                                                                                                                        '$PSPB',
                                                                                                                                                        '$UCC',
                                                                                                                                                        '$CCL',
                                                                                                                                                        '$SHLBL'))
                               ),
                CUST_VAS_CONFIGURATION AS
                    (SELECT MCV.CUSTOMER_ID AS CUSTOMER_ID,
                         MAX(TV.DESCRIPTION) AS DESCRIPTION,
                         MAX(LISTAGG(MCV.VAS_ID || ': ' || TV.DESCRIPTION, ', ') WITHIN
                        GROUP(ORDER BY MCV.VAS_ID)) OVER(PARTITION BY MCV.CUSTOMER_ID) AS CUST_VAS
                       FROM <proxy />MASTER_CUSTOMER_VAS MCV
                            INNER JOIN <proxy />TAB_VAS TV
                             ON TV.VAS_CODE = MCV.VAS_ID
                         GROUP BY MCV.CUSTOMER_ID)
                            SELECT CST.CUSTOMER_ID        AS CUSTOMER_ID,
                                   CST.NAME               AS NAME,
                                   CST.CATEGORY           AS CATEGORY,
                                   CTYPE.description      AS ACCOUNT_TYPE,
                                   CST.CARRIER_ID         AS CARRIER_ID,
                                   M.DESCRIPTION          AS CARRIER_DESCRIPTION,
                                   CST.ASN_FLAG           AS ASN_FLAG,
                                   CST.DEFAULT_PICK_MODE  AS DEFAULT_PICK_MODE,
                                   CST.MIN_PIECES_PER_BOX AS MIN_PIECES_PER_BOX,
                                   CST.MAX_PIECES_PER_BOX AS MAX_PIECES_PER_BOX,
                                   PCS.EDI753             AS EDI753,
                                   PCS.AMS                AS AMS,
                                   PCS.SCO                AS SCO,
                                   pcd.NUMBER_OF_MPS      as NUMBER_OF_MPS,
                            pcd.MPS_SHORT_NAME as MPS_SHORT_NAME,
                            pcd.PSPB_SHORT_NAME as PSPB_SHORT_NAME,
                            pcd.UCC_SHORT_NAME as UCC_SHORT_NAME,
                            pcd.CCL_SHORT_NAME as CCL_SHORT_NAME,
                            pcd.SHLBL_SHORT_NAME as SHLBL_SHORT_NAME,
                                   pcd.NUMBER_OF_PSPB     as NUMBER_OF_PSPB,
                                   pcd.NUMBER_OF_UCC      as NUMBER_OF_UCC,
                                   pcd.NUMBER_OF_CCL      as NUMBER_OF_CCL,
                                   pcd.NUMBER_OF_SHLBL    as NUMBER_OF_SHLBL,
                                    CVC.CUST_VAS           AS CUST_VAS              
                              FROM <proxy />CUST CST
                            LEFT OUTER JOIN CUST_VAS_CONFIGURATION CVC
                                ON CST.CUSTOMER_ID = CVC.CUSTOMER_ID
                             left outer join custtype CTYPE
                                ON CST.ACCOUNT_TYPE = CTYPE.customer_type
                             left outer JOIN PIVOT_CUST_SPLH PCS
                                ON CST.CUSTOMER_ID = PCS.CUSTOMER_ID
                             LEFT OUTER JOIN MASTER_CARRIER M
                                ON CST.CARRIER_ID = M.CARRIER_ID
                            cross join  pivot_cust_doc pcd
                             WHERE CST.CUSTOMER_ID = :CUSTOMER_ID
            ";
            var binder = SqlBinder.Create(row => new Customer
            {
                CustomerId = row.GetString("CUSTOMER_ID"),
                CustomerName = row.GetString("NAME"),
                Category = row.GetString("CATEGORY"),
                CustomerTypeDescription = row.GetString("ACCOUNT_TYPE"),
                CarrierId = row.GetString("CARRIER_ID"),
                CarrierDescription = row.GetString("CARRIER_DESCRIPTION"),
                DefaultPickMode = row.GetString("DEFAULT_PICK_MODE"),
                MinPiecesPerBox = row.GetInteger("MIN_PIECES_PER_BOX"),
                MaxPiecesPerBox = row.GetInteger("MAX_PIECES_PER_BOX"),
                AmsFlag = !string.IsNullOrEmpty(row.GetString("AMS")),
                EdiFlag = !string.IsNullOrEmpty(row.GetString("EDI753")),
                ScoFlag = !string.IsNullOrEmpty(row.GetString("SCO")),
                Asn_flag = !string.IsNullOrEmpty(row.GetString("ASN_FLAG")),
                NumberOfMps = row.GetInteger("NUMBER_OF_MPS"),
                MpsShortName = row.GetString("MPS_SHORT_NAME"),
                NumberOfPspb = row.GetInteger("NUMBER_OF_PSPB"),
                PspbShortName = row.GetString("PSPB_SHORT_NAME"),
                NumberOfUcc = row.GetInteger("NUMBER_OF_UCC"),
                UccShortName = row.GetString("UCC_SHORT_NAME"),
                NumberOfCcl = row.GetInteger("NUMBER_OF_CCL"),
                CclShortName = row.GetString("CCL_SHORT_NAME"),
                NumberOfShlbl = row.GetInteger("NUMBER_OF_SHLBL"),
                ShlblShortName = row.GetString("SHLBL_SHORT_NAME"),
                CustVas = row.GetString("CUST_VAS")

            }).Parameter("CUSTOMER_ID", customerId);

            return _db.ExecuteSingle(QUERY_CUSTOMER_DETAIL, binder);
        }

        /// <summary>
        /// This function will return orders summary of Customer for last 180 days.Summary is grouped on the basis of import date and we wont show more than 100 rows.
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        public IList<PoHeadline> GetRecentOrders(string customerId, int maxRows)
        {
            return SharedRepository.GetRecentOrders(_db, customerId, null, maxRows);
        }


        public IList<Tuple<string, string>> CustomerAutoComplete(string term)
        {
            const string QUERY =
                @"
                SELECT CUST.CUSTOMER_ID AS CUSTOMER_ID, 
                       CUST.NAME AS CUSTOMER_NAME
                FROM <proxy />CUST CUST
                WHERE 1 = 1
                 <if c='$TERM'>
                        AND (UPPER(CUST.CUSTOMER_ID) LIKE '%' || UPPER(:TERM) ||'%' 
                            OR UPPER(CUST.NAME) LIKE '%' || UPPER(:TERM) ||'%')
                 </if>                      
                        AND ROWNUM &lt; 40 and SUBSTR(UPPER(CUST.CUSTOMER_ID), 1, 1) != '$'
                        ORDER BY CUST.CUSTOMER_ID
                ";
            Contract.Assert(_db != null);
            var binder = SqlBinder.Create(row => Tuple.Create(row.GetString("CUSTOMER_ID"), row.GetString("CUSTOMER_NAME")))
                .Parameter("TERM", term);

            return _db.ExecuteReader(QUERY, binder);

        }


        public IList<CustomerHeadline> GetCustomerList()
        {
            Contract.Assert(_db != null);
            const string QUERY = @"   
WITH CUSTOMER_ORDER AS
 (SELECT P.CUSTOMER_ID          AS CUSTOMER_ID,
         P.ITERATION            AS ITERATION,
         P.PO_ID                AS PO_ID,
         P.PICKSLIP_IMPORT_DATE AS PICKSLIP_IMPORT_DATE
    FROM <proxy/> PS P
   WHERE P.TRANSFER_DATE IS NULL
  
  UNION ALL
  SELECT DP.CUSTOMER_ID          AS CUSTOMER_ID,
         1                       AS ITERATION,
         DP.CUSTOMER_ORDER_ID    AS PO_ID,
         DP.PICKSLIP_IMPORT_DATE AS PICKSLIP_IMPORT_DATE
    FROM <proxy/> DEM_PICKSLIP DP
   WHERE DP.PS_STATUS_ID = 1),
GROUP_CUSTOMER_PO AS
 (SELECT CO.CUSTOMER_ID AS CUSTOMER_ID,
         MAX(M.NAME) AS CUSTOMER_NAME,
         CO.ITERATION,
         MAX(CT.DESCRIPTION) AS CUSTOMER_TYPE_DESCRIPTION,
         MAX(CO.PICKSLIP_IMPORT_DATE) AS PICKSLIP_IMPORT_DATE,
         COUNT(CO.PO_ID) AS PO_ID
    FROM CUSTOMER_ORDER CO
    LEFT OUTER JOIN <proxy/> MASTER_CUSTOMER M
      ON M.CUSTOMER_ID = CO.CUSTOMER_ID
    LEFT OUTER JOIN <proxy/> CUSTTYPE CT
      ON CT.CUSTOMER_TYPE = M.CUSTOMER_TYPE
   GROUP BY CO.CUSTOMER_ID, CO.ITERATION, CO.PO_ID)
SELECT MAX(GC.CUSTOMER_ID) AS CUSTOMER_ID,
       MAX(GC.CUSTOMER_NAME) AS CUSTOMER_NAME,
       MAX(GC.PICKSLIP_IMPORT_DATE) AS PICKSLIP_IMPORT_DATE,
       MAX(GC.CUSTOMER_TYPE_DESCRIPTION) AS CUSTOMER_TYPE_DESCRIPTION,
       MAX(COUNT(GC.PO_ID)) OVER(PARTITION BY GC.CUSTOMER_ID) AS PO_COUNT
  FROM GROUP_CUSTOMER_PO GC
 GROUP BY GC.CUSTOMER_ID
 ORDER BY MAX(GC.PICKSLIP_IMPORT_DATE) DESC NULLS LAST

                                    ";
            var binder = SqlBinder.Create(row => new CustomerHeadline
            {
                CustomerId = row.GetString("CUSTOMER_ID"),
                CustomerName = row.GetString("CUSTOMER_NAME"),
                CustomerTypeDescription = row.GetString("CUSTOMER_TYPE_DESCRIPTION"),
                PickslipImportDate = row.GetDate("PICKSLIP_IMPORT_DATE"),
                PoCount = row.GetInteger("PO_COUNT")
            });
            return _db.ExecuteReader(QUERY, binder, 200);
        }
    }
}