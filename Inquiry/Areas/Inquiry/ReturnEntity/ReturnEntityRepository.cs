using EclipseLibrary.Oracle;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Web;

namespace DcmsMobile.Inquiry.Areas.Inquiry.ReturnEntity
{
    internal class ReturnEntityRepository : IDisposable
    {
        private readonly OracleDatastore _db;
        public ReturnEntityRepository(string userName, string clientInfo)
        {
            _db = new OracleDatastore(HttpContext.Current.Trace);
            _db.CreateConnection(ConfigurationManager.ConnectionStrings["dcms8"].ConnectionString, userName);
            _db.ModuleName = "Inquiry_ReturnEntity";
            _db.ClientInfo = clientInfo;
        }

        public void Dispose()
        {
            _db.Dispose();
        }

        /// <summary>
        /// Results are sorted by Customer Name, ReceiveDate desc
        /// </summary>
        /// <param name="returnNumber"></param>
        /// <param name="maxRows"></param>
        /// <returns></returns>
        public IList<ReturnReceiptDetail> GetReturnInfo(string returnNumber, int maxRows)
        {
            const string QUERY = @"
                         SELECT RET.RECEIPT_NUMBER    AS RECEIPT_NUMBER,
                                 RET.RECEIVED_DATE     AS RECEIVED_DATE,                                                               
                                 CUST.CUSTOMER_ID      AS CUSTOMER_ID,
                                 CUST.NAME             AS CUSTOMER_NAME,
                                 RET.CUSTOMER_STORE_ID AS CUSTOMER_STORE_ID,
                                 TR.REASON_CODE        AS REASON_CODE,
                                 TR.DESCRIPTION        AS REASON_CODE_DESCRIPTION,
                                 RET.IS_COMPLETE_RETURN AS IS_COMPLETE_RETURN,
                                 RET.ACTIVITY_ID        AS ACTIVITY_ID,
                                 RET.NO_OF_CARTONS      AS NO_OF_CARTONS,
                                 RET.EXPECTED_PIECES    AS EXPECTED_PIECES,
                                 COUNT(*) OVER()        AS TOTAL_RECEIPT
                            FROM <proxy />DEM_RETURNS RET
                            LEFT OUTER JOIN <proxy />CUST CUST
                              ON RET.CUSTOMER_ID = CUST.CUSTOMER_ID
                            LEFT OUTER JOIN <proxy />TAB_REASON TR
                              ON RET.REASON_CODE = TR.REASON_CODE
                           WHERE RET.RETURNS_AUTHORIZATION_NUMBER = :RETURNS_AUTHORIZATION_NUMBER
                           ORDER BY CUST.NAME, RET.RECEIVED_DATE DESC
            ";

            var binder = SqlBinder.Create(row => new ReturnReceiptDetail
            {
                ReceiptNumber = row.GetString("RECEIPT_NUMBER"),
                ReceivedDate = row.GetDate("RECEIVED_DATE").Value,
                ReasonCode = row.GetString("REASON_CODE"),
                ReasonDescription = row.GetString("REASON_CODE_DESCRIPTION"),
                IsCompleteReceipt = !string.IsNullOrWhiteSpace(row.GetString("IS_COMPLETE_RETURN")),
                ActivityId = row.GetInteger("ACTIVITY_ID"),
                TotalReceipts = row.GetInteger("TOTAL_RECEIPT").Value,
                CustomerId = row.GetString("CUSTOMER_ID"),
                CustomerName = row.GetString("CUSTOMER_NAME"),
                CustomerStoreId = row.GetString("CUSTOMER_STORE_ID"),
                NoOfCartons = row.GetInteger("NO_OF_CARTONS"),
                ExpectedPieces = row.GetInteger("EXPECTED_PIECES")
            }).Parameter("RETURNS_AUTHORIZATION_NUMBER", returnNumber);

            return _db.ExecuteReader(QUERY, binder, maxRows);
        }

        public IEnumerable<ReturnReceiptDetail> GetReturnReceiptInfo(string receiptNumber)
        {
            const string QUERY = @"
                SELECT RET.RETURNS_AUTHORIZATION_NUMBER as RETURNS_AUTHORIZATION_NUMBER,
                       RET.RECEIPT_NUMBER as RECEIPT_NUMBER,
                       CUST.CUSTOMER_ID   as CUSTOMER_ID,
                       CUST.NAME          as CUSTOMER_NAME,
                       RET.RECEIVED_DATE  as RECEIVED_DATE,
                       MC.CARRIER_ID      as  CARRIER_ID,
                       MC.DESCRIPTION     as  CARRIER_DESCRIPTION,       
                       RET.DM_NUMBER      as DM_NUMBER,
                       RET.INSERTED_BY    as INSERTED_BY,
                       RET.VWH_ID         as VWH_ID,
                       RET.ACTIVITY_ID    as ACTIVITY_ID,
                       RET.CUSTOMER_STORE_ID as CUSTOMER_STORE_ID,
                       RET.DM_DATE        as DM_DATE,
                       TR.REASON_CODE     as REASON_CODE,
                       TR.DESCRIPTION     as REASON_CODE_DESCRIPTION,
                       RETDET.BAR_CODE    as BAR_CODE ,
                       RETDET.STYLE       as STYLE,
                       RETDET.COLOR       as COLOR,
                       RETDET.DIMENSION   as DIMENSION,
                       RETDET.SKU_SIZE    as SKU_SIZE,
                       RETDET.QUANTITY    AS QUANTITY,
                       MSKU.RETAIL_PRICE  AS RETAIL_PRICE,
                       MSKU.SKU_ID        AS SKU_ID,
                       RET.INSERT_DATE    AS INSERT_DATE,                       
                       RET.MODIFIED_DATE  AS MODIFIED_DATE,
                       RET.MODIFIED_BY    AS MODIFIED_BY,
                       RET.EXPECTED_PIECES AS EXPECTED_PIECES
                  FROM <proxy />DEM_RETURNS RET
                  LEFT OUTER JOIN <proxy />DEM_RETURNS_DETAIL RETDET
                    ON RETDET.RECEIPT_NUMBER = RET.RECEIPT_NUMBER
                  LEFT OUTER JOIN <proxy />CUST CUST
                    ON RET.CUSTOMER_ID = CUST.CUSTOMER_ID
                  LEFT OUTER JOIN <proxy />MASTER_CARRIER MC
                    ON RET.CARRIER_ID = MC.CARRIER_ID
                  LEFT OUTER JOIN <proxy />TAB_REASON TR
                    ON RET.REASON_CODE = TR.REASON_CODE
                 LEFT OUTER JOIN <proxy />MASTER_SKU MSKU
                    ON RETDET.BAR_CODE = MSKU.UPC_CODE
                 WHERE RET.RECEIPT_NUMBER = :RECEIPT_NUMBER
            ";

            var binder = SqlBinder.Create(row => new ReturnReceiptDetail
            {
                ReceiptNumber = row.GetString("RECEIPT_NUMBER"),
                CustomerId = row.GetString("CUSTOMER_ID"),
                CustomerName = row.GetString("CUSTOMER_NAME"),
                ReceivedDate = row.GetDate("RECEIVED_DATE").Value,
                CarrierId = row.GetString("CARRIER_ID"),
                CarrierDescription = row.GetString("CARRIER_DESCRIPTION"),
                DMNumber = row.GetString("DM_NUMBER"),
                InsertedBy = row.GetString("INSERTED_BY"),
                VwhId = row.GetString("VWH_ID"),
                ActivityId = row.GetInteger("ACTIVITY_ID"),
                SkuId = row.GetInteger("SKU_ID"),
                Upc = row.GetString("BAR_CODE"),
                Style = row.GetString("STYLE"),
                Color = row.GetString("COLOR"),
                Dimension = row.GetString("DIMENSION"),
                SkuSize = row.GetString("SKU_SIZE"),
                CustomerStoreId = row.GetString("CUSTOMER_STORE_ID"),
                ReasonCode = row.GetString("REASON_CODE"),
                ReasonDescription = row.GetString("REASON_CODE_DESCRIPTION"),
                DmDate = row.GetDate("DM_DATE"),
                Quantity = row.GetInteger("QUANTITY"),
                RetailPrice = row.GetDecimal("RETAIL_PRICE"),
                ReturnNumber = row.GetString("RETURNS_AUTHORIZATION_NUMBER"),
                InsertDate = row.GetDate("INSERT_DATE").Value,
                ModifiedDate = row.GetDate("MODIFIED_DATE").Value,
                ModifiedBy = row.GetString("MODIFIED_BY"),
                ExpectedPieces = row.GetInteger("EXPECTED_PIECES")
            }).Parameter("RECEIPT_NUMBER", receiptNumber);

            return _db.ExecuteReader(QUERY, binder);
        }

        public IList<ReturnReceiptHeadline> GetReturnList()
        {
            const string QUERY = @"SELECT
                                      DR.RETURNS_AUTHORIZATION_NUMBER AS RETURNS_AUTHORIZATION_NUMBER,
                                      COUNT(DR.RECEIPT_NUMBER) AS TOTAL_RECEIPT_NUMBER,
                                      SUM(DR.NO_OF_CARTONS) AS TOTAL_CARTONS,
                                      SUM(DR.EXPECTED_PIECES) AS TOTAL_EXPECTED_PIECES,
                                      MIN(DR.RECEIVED_DATE) AS RECEIVED_DATE,
                                      COUNT(UNIQUE DR.CUSTOMER_ID) AS CUSTOMER_COUNT,
                                      MAX(MC.CUSTOMER_ID) AS CUSTOMER_ID,
                                      MAX(MC.NAME) AS CUSTOMER_NAME
                                  FROM DEM_RETURNS DR
                                  LEFT OUTER JOIN MASTER_CUSTOMER MC
                                    ON MC.CUSTOMER_ID = DR.CUSTOMER_ID
                                 GROUP BY DR.RETURNS_AUTHORIZATION_NUMBER
                                 ORDER BY MIN(DR.RECEIVED_DATE) DESC
                                ";
            var binder = SqlBinder.Create(row => new ReturnReceiptHeadline
            {
                ReturnNumber = row.GetString("RETURNS_AUTHORIZATION_NUMBER"),
                CustomerId = row.GetString("CUSTOMER_ID"),
                CustomerName = row.GetString("CUSTOMER_NAME"),
                TotalReceipts = row.GetInteger("TOTAL_RECEIPT_NUMBER").Value,
                NoOfCartons = row.GetInteger("TOTAL_CARTONS"),
                ExpectedPieces = row.GetInteger("TOTAL_EXPECTED_PIECES"),
                ReceivedDate = row.GetDate("RECEIVED_DATE").Value,
                CustomerCount = row.GetInteger("CUSTOMER_COUNT")
            });
            return _db.ExecuteReader(QUERY, binder,200);
        }

    }
}