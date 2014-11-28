using DcmsMobile.Inquiry.Areas.Inquiry.SharedViews;
using DcmsMobile.Inquiry.Helpers;
using EclipseLibrary.Oracle;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace DcmsMobile.Inquiry.Areas.Inquiry.SkuEntity
{
    internal class SkuEntityRepository : IDisposable
    {
        private readonly OracleDatastore _db;
        public SkuEntityRepository(string userName, string clientInfo)
        {
            _db = new OracleDatastore(HttpContext.Current.Trace);
            _db.CreateConnection(ConfigurationManager.ConnectionStrings["dcms8"].ConnectionString, userName);
            _db.ModuleName = "Inquiry_CartonEntity";
            _db.ClientInfo = clientInfo;
        }

        public void Dispose()
        {
            _db.Dispose();
        }

        /// <summary>
        /// Returns sku info against scanned UPC
        /// </summary>
        /// <param name="upc"></param>
        /// <returns>
        /// </returns>
        public Sku GetSku(int skuId)
        {
            Contract.Assert(_db != null);
            const string QUERY_SKU_DETAIL = @"
              SELECT MSKU.STYLE                   AS STYLE,
                     MSKU.COLOR                   AS COLOR,
                     MSKU.DIMENSION               AS DIMENSION,
                     MSKU.SKU_SIZE                AS SKU_SIZE,
                     MSKU.UPC_CODE                AS UPC_CODE,
                     MSKU.SKU_ID                  AS SKU_ID,
                     MSKU.RETAIL_PRICE            AS RETAIL_PRICE,
                     MSKU.PIECES_PER_PACKAGE      AS PIECES_PER_PACKAGE,
                     MSKU.ADDITIONAL_RETAIL_PRICE AS ADDITIONAL_RETAIL_PRICE,
                     MS.DESCRIPTION               AS DESCRIPTION,
                     MSKU.STANDARD_CASE_QTY       AS STANDARD_CASE_QTY
                FROM <proxy />MASTER_SKU MSKU
                 LEFT OUTER JOIN <proxy />MASTER_STYLE MS
                     ON MSKU.STYLE = MS.STYLE  
               WHERE MSKU.sku_id = :sku_id
 ";
            var binder = SqlBinder.Create(row => new Sku
            {
                SkuId = row.GetInteger("SKU_ID").Value,
                Style = row.GetString("STYLE"),
                Color = row.GetString("COLOR"),
                Dimension = row.GetString("DIMENSION"),
                SkuSize = row.GetString("SKU_SIZE"),
                Upc = row.GetString("UPC_CODE"),
                RetailPrice = row.GetDecimal("RETAIL_PRICE"),
                PiecesPerPackage = row.GetInteger("PIECES_PER_PACKAGE"),
                AdditionalRetailPrice = row.GetString("ADDITIONAL_RETAIL_PRICE"),
                Description = row.GetString("DESCRIPTION"),
                StandardCaseQty = row.GetInteger("STANDARD_CASE_QTY")
            }).Parameter("sku_id", skuId);
            return _db.ExecuteSingle(QUERY_SKU_DETAIL, binder);
        }

        /// <summary>
        /// Retrieve inventory at Area/Vwh level
        /// </summary>
        /// <param name="skuId"></param>
        /// <param name="iaId"></param>
        /// <returns></returns>
        /// <remarks>
        /// 
        /// </remarks>
        public IList<SkuInventoryItem> GetSkuInventoryByArea(int skuId)
        {
            Contract.Assert(_db != null);
            const string QUERY_CARTON_PALLET_DETAIL = @"     
              WITH Q1 AS
                     (SELECT 2 AS AREA_TYPE,
                             MAX(CTN.CARTON_STORAGE_AREA) AS IA_ID,
                             MAX(TIA.SHORT_NAME) AS SHORT_NAME,
                             MAX(TIA.DESCRIPTION) AS DESCRIPTION,
                             MAX(TIA.WAREHOUSE_LOCATION_ID) AS WAREHOUSE_LOCATION_ID,
                             SUM(CTNDET.QUANTITY) AS PIECES,
                             CTN.LOCATION_ID,
                             MAX(CTN.VWH_ID) AS VWH_ID
                        FROM <proxy />SRC_CARTON CTN
                       INNER JOIN <proxy />SRC_CARTON_DETAIL CTNDET
                          ON CTN.CARTON_ID = CTNDET.CARTON_ID
                        LEFT OUTER JOIN <proxy />TAB_INVENTORY_AREA TIA
                          ON CTN.CARTON_STORAGE_AREA = TIA.INVENTORY_STORAGE_AREA
                       WHERE CTNDET.SKU_ID = :SKU_ID
                       GROUP BY CTN.LOCATION_ID
                      HAVING SUM(CTNDET.QUANTITY) != 0
                      UNION ALL
                      SELECT 1 AS AREA_TYPE,
                             MAX(I.IA_ID) AS IA_ID,
                             MAX(IA.SHORT_NAME) AS SHORT_NAME,
                             MAX(IA.DESCRIPTION) AS DESCRIPTION,
                             MAX(IA.WAREHOUSE_LOCATION_ID) AS WAREHOUSE_LOCATION_ID,
                             SUM(IC.NUMBER_OF_UNITS) AS PIECES,
                             IC.LOCATION_ID,
                             MAX(I.VWH_ID) AS VWH_ID
                        FROM <proxy />IALOC I
                       INNER JOIN <proxy />IALOC_CONTENT IC
                          ON I.LOCATION_ID = IC.LOCATION_ID
                       INNER JOIN <proxy />MASTER_SKU MSKU
                          ON MSKU.UPC_CODE = IC.IACONTENT_ID
                        LEFT OUTER JOIN <proxy />IA
                          ON I.IA_ID = IA.IA_ID
                       WHERE I.LOCATION_TYPE = 'RAIL'
                         AND IC.IACONTENT_TYPE_ID = 'SKU'
                         AND MSKU.SKU_ID = :SKU_ID
                       GROUP BY IC.LOCATION_ID
                      HAVING SUM(IC.NUMBER_OF_UNITS) != 0)
                    SELECT MAX(Q1.AREA_TYPE)                                                  AS AREA_TYPE,
                           Q1.IA_ID                                                           AS IA_ID,
                           MAX(Q1.SHORT_NAME)                                                 AS SHORT_NAME,
                           MAX(Q1.DESCRIPTION)                                                AS DESCRIPTION,
                           MAX(Q1.WAREHOUSE_LOCATION_ID)                                      AS WAREHOUSE_LOCATION_ID,
                           SUM(Q1.PIECES)                                                     AS PIECES,
                           MAX(Q1.LOCATION_ID) KEEP(DENSE_RANK FIRST ORDER BY case when q1.location_id is null then NULL ELSE Q1.PIECES END DESC NULLS LAST) AS BEST_LOC,
                           MAX(Q1.PIECES) KEEP(DENSE_RANK FIRST ORDER BY case when q1.location_id is null then NULL ELSE Q1.PIECES END DESC NULLS LAST)      AS PCS_AT_BEST_LOC,
                           COUNT(UNIQUE Q1.LOCATION_ID)                                       AS TOTAL_LOCATIONS,
                           Q1.VWH_ID                                                          AS VWH_ID
                      FROM Q1
                     GROUP BY Q1.IA_ID, Q1.VWH_ID

             ";
            var binder = SqlBinder.Create(row => new SkuInventoryItem
            {
                IaId = row.GetString("IA_ID"),
                ShortName = row.GetString("SHORT_NAME"),
                AreaDescription = row.GetString("DESCRIPTION"),
                Building = row.GetString("WAREHOUSE_LOCATION_ID"),
                Pieces = row.GetInteger("PIECES"),
                LocationId = row.GetString("BEST_LOC"),
                PiecesAtLocation = row.GetInteger("PCS_AT_BEST_LOC").Value,
                LocationCount = row.GetInteger("TOTAL_LOCATIONS").Value,
                VwhId = row.GetString("VWH_ID"),
                IsCartonArea = row.GetInteger("AREA_TYPE") == 2

            }).Parameter("SKU_ID", skuId);
            return _db.ExecuteReader(QUERY_CARTON_PALLET_DETAIL, binder);

        }


        public IList<CustomerSkuLabel> GetPrivateLabelBarCodesOfSku(int skuId)
        {
            Contract.Assert(_db != null);
            const string QUERY = @"
                SELECT M.CUSTOMER_ID      AS CUSTOMER_ID,
                       C.NAME             AS NAME,
                       NVL(M.SCANNED_BAR_CODE, m.customer_sku_id) AS SCANNED_BAR_CODE
                  FROM <proxy />MASTER_CUSTOMER_SKU M
                  LEFT OUTER JOIN <proxy />CUST C
                    ON  M.CUSTOMER_ID = C.CUSTOMER_ID
                 WHERE M.SKU_ID = :SKU_ID
                   AND M.INACTIVE_FLAG IS NULL
                ";
            var binder = SqlBinder.Create(row => new CustomerSkuLabel
            {
                CustomerId = row.GetString("CUSTOMER_ID"),
                CustomerName = row.GetString("NAME"),
                //InsertedBy = row.GetString("INSERTED_BY"),
                //ModifiedBy = row.GetString("MODIFIED_BY"),
                ScannedBarCode = row.GetString("SCANNED_BAR_CODE")
                //InsetDate = row.GetDate("INSERT_DATE"),
                //ModifiedDate = row.GetDate("MODIFIED_DATE")

            }).Parameter("SKU_ID", skuId);

            return _db.ExecuteReader(QUERY, binder);
        }

        /// <summary>
        /// This function will return orders summary of Customer for last 180 days.Summary is grouped on the basis of import date and we wont show more than 100 rows.
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        public IList<PoHeadline> GetRecentOrders(int skuId, int maxRows)
        {
            return SharedRepository.GetRecentOrders(_db, null, skuId, maxRows);
        }

        /// <summary>
        /// Retrieve Scanned style information
        /// </summary>
        /// <param name="style"></param>
        /// <returns></returns>
        public Style GetStyleInfo(string style)
        {
            Contract.Assert(_db != null);
            const string QUERY = @"
with q1 AS
 (SELECT MS.STYLE       AS STYLE,
         MS.DESCRIPTION AS DESCRIPTION,
         msc.color,
         MS.LABEL_ID    AS LABEL_ID,
         MSC.COUNTRY_ID AS COUNTRY_ID,
         tc.name        as country_name
    FROM <proxy/>MASTER_STYLE MS
    LEFT OUTER JOIN <proxy/>MASTER_STYLE_COLOR MSC
      ON MS.STYLE = MSC.STYLE
    LEFT OUTER JOIN <proxy/>TAB_COUNTRY TC
      ON MSC.COUNTRY_ID = TC.COUNTRY_ID
   WHERE MS.STYLE = :style
     )
select STYLE, DESCRIPTION, LABEL_ID, cast(country_id_xml as varchar2(4000)) as country_id_xml
  from q1 pivot XML(count(distinct q1.color) as count_colors, max(q1.country_name) as country_name for country_id in(ANY))
            ";

            var binder = SqlBinder.Create(row => new Style
            {
                StyleId = row.GetString("STYLE"),
                Description = row.GetString("DESCRIPTION"),
                LabelId = row.GetString("LABEL_ID"),
                CountryOfOrigins = MapCountryXml(row.GetString("country_id_xml"))
            }).Parameter("STYLE", style);

            var result = _db.ExecuteSingle(QUERY, binder);

            return result;

        }

        private static IEnumerable<CountryOfOrigin> MapCountryXml(string data)
        {
            var xml = XElement.Parse(data);
            var query = from item in xml.Elements("item")
                        let columns = item.Elements("column")
                        select new CountryOfOrigin
                        {
                            CountColors = (int)columns.First(q => q.Attribute("name").Value == "COUNT_COLORS"),
                            CountryId = (string)columns.First(q => q.Attribute("name").Value == "COUNTRY_ID"),
                            CountryName = (string)columns.First(q => q.Attribute("name").Value == "COUNTRY_NAME")
                        };

            return new List<CountryOfOrigin>(query);
        }

        /// <summary>
        /// Retrieve Scanned style label information
        /// </summary>
        /// <param name="labelId"></param>
        /// <returns></returns>
        public Tuple<string, string> GetLabelInfo(string labelId)
        {
            Contract.Assert(_db != null);
            const string QUERY_LABEL = @"
        SELECT TSL.LABEL_ID         AS LABEL_ID,
               TSL.DESCRIPTION      AS DESCRIPTION
          FROM <proxy />TAB_STYLE_LABEL TSL
         WHERE TSL.LABEL_ID = :LABEL_ID
        ";
            var binder = SqlBinder.Create(row => Tuple.Create(row.GetString("LABEL_ID"), row.GetString("DESCRIPTION")))
                .Parameter("LABEL_ID", labelId);
            //{
            //    Code = row.GetString("LABEL_ID"),
            //    Description = row.GetString("DESCRIPTION")
            //}).Parameter("LABEL_ID", labelId);
            return _db.ExecuteSingle(QUERY_LABEL, binder);
        }

        /// <summary>
        /// For UPC autocomplete
        /// </summary>
        /// <param name="term"></param>
        /// <returns></returns>
        public IList<SkuAutoComplete> SkuAutoComplete(string term)
        {
            const string QUERY =
                @"
        WITH ALL_SKU AS
            (SELECT 'U' AS BAR_CODE_TYPE, MS.UPC_CODE AS UPC_CODE,
        MS.SKU_ID AS SKU_ID,
                    MS.STYLE AS STYLE,
                    MS.COLOR AS COLOR,
                    MS.DIMENSION AS DIMENSION,
                    MS.SKU_SIZE AS SKU_SIZE,
                    MS.INACTIVE_FLAG AS INACTIVE_FLAG,
        <a sep='+'>
        CASE
            WHEN MS.UPC_CODE = :TERM THEN 100
            ELSE 0
        END +
        CASE
            WHEN MS.STYLE = :TERM THEN 10
            WHEN MS.STYLE LIKE :TERM || '%' THEN 1
            ELSE 0
        END +
        CASE
            WHEN MS.COLOR = :TERM THEN 10
WHEN MS.COLOR LIKE :TERM || '%' THEN 1
            ELSE 0
        END +
        CASE
            WHEN MS.DIMENSION = :TERM THEN 10
WHEN MS.DIMENSION LIKE :TERM || '%' THEN 1
            ELSE 0
        END +
        CASE
            WHEN MS.SKU_SIZE = :TERM THEN 10
WHEN MS.SKU_SIZE LIKE :TERM || '%' THEN 1
            ELSE 0
        END
        </a>
        <if c='not($TERM)'>0</if> AS RELEVANCE
        FROM <proxy />MASTER_SKU MS
        WHERE 1=1
        <a pre=' AND ' sep=' OR '>
        (MS.STYLE LIKE '%' || :TERM || '%' OR  MS.COLOR LIKE '%' || :TERM || '%' OR MS.DIMENSION LIKE '%' || :TERM || '%' OR
        MS.SKU_SIZE LIKE '%' || :TERM  || '%' OR MS.UPC_CODE LIKE '%' || :TERM || '%')
        </a>
                    ),
                    RELEVANCE_SKU AS (
        SELECT ALL1.UPC_CODE AS UPC_CODE,
 max(all1.relevance) over() as max_relevance,
        ALL1.SKU_ID AS SKU_ID,
               ALL1.STYLE AS STYLE,
               ALL1.COLOR AS COLOR,
               ALL1.DIMENSION AS DIMENSION,
               ALL1.SKU_SIZE AS SKU_SIZE,
               ALL1.RELEVANCE AS RELEVANCE
          FROM ALL_SKU ALL1
ORDER BY ALL1.RELEVANCE DESC, ALL1.STYLE, ALL1.COLOR, ALL1.DIMENSION, ALL1.SKU_SIZE
                    )
        SELECT RS.SKU_ID, RS.STYLE, RS.COLOR, RS.DIMENSION, RS.SKU_SIZE, RS.UPC_CODE FROM RELEVANCE_SKU RS
        WHERE rownum &lt; 20 and relevance = max_relevance
        ";
            Contract.Assert(_db != null);
            var binder = SqlBinder.Create(row => new SkuAutoComplete
            {
                SkuId = row.GetInteger("SKU_ID") ?? 0,
                Style = row.GetString("STYLE"),
                Color = row.GetString("COLOR"),
                Dimension = row.GetString("DIMENSION"),
                SkuSize = row.GetString("SKU_SIZE"),
                Upc = row.GetString("UPC_CODE")
            });
            binder.ParameterXmlArray("TERM", term.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()));
            return _db.ExecuteReader(QUERY, binder);
        }

        /// <summary>
        /// List of Sku returned.
        /// </summary>
        /// <returns></returns>
        public IList<SkuHeadline> GetSkuList()
        {
            Contract.Assert(_db != null);
                            const string QUERY = @"
                       SELECT MAX(M.SKU_ID) AS SKU_ID,
                       MAX(M.STYLE) AS STYLE,
                       MAX(M.COLOR) AS COLOR,
                       MAX(M.DIMENSION) AS DIMENSION,
                       MAX(M.SKU_SIZE) AS SKU_SIZE,
                       M.UPC_CODE AS UPC_CODE,
                       MAX(PS.PICKSLIP_IMPORT_DATE) AS PICKSLIP_IMPORT_DATE
                  FROM <proxy/>PS PS
                 INNER JOIN <proxy/>PSDET PD
                    ON PS.PICKSLIP_ID = PD.PICKSLIP_ID
                 INNER JOIN <proxy/>MASTER_SKU M
                    ON M.UPC_CODE = PD.UPC_CODE
                 GROUP BY M.UPC_CODE
                 ORDER BY MAX(PS.PICKSLIP_IMPORT_DATE) DESC NULLS LAST
 ";

            var binder = SqlBinder.Create(row => new SkuHeadline
            {
                SkuId = row.GetInteger("SKU_ID").Value,
                Style = row.GetString("STYLE"),
                Color = row.GetString("COLOR"),
                Dimension = row.GetString("DIMENSION"),
                SkuSize = row.GetString("SKU_SIZE"),
                Upc = row.GetString("UPC_CODE"),
                PickslipOrderDate = row.GetDate("PICKSLIP_IMPORT_DATE")
                
            });

            return _db.ExecuteReader(QUERY, binder,200);
        }

    }
}