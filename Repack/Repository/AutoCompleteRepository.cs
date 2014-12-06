using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.Contracts;
using System.Web;
using DcmsMobile.Repack.Models;
using EclipseLibrary.Oracle;

namespace DcmsMobile.Repack.Repository
{
    public class AutoCompleteRepository : IDisposable
    {

        public void Dispose()
        {
            var dis = _db as IDisposable;
            if (dis != null)
            {
                dis.Dispose();
            }
        }

        #region Intialization

        private readonly OracleDatastore _db;

        public OracleDatastore Db
        {
            get
            {
                return _db;
            }
        }

        /// <summary>
        /// Constructor of class used to create the connection to database.
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="moduleName"></param>
        /// <param name="clientInfo"></param>
        /// <param name="trace"></param>
        public AutoCompleteRepository(string userName, string moduleName, string clientInfo, TraceContext trace)
        {
            Contract.Assert(ConfigurationManager.ConnectionStrings["dcms4"] != null);
            var store = new OracleDatastore(trace);
            store.CreateConnection(ConfigurationManager.ConnectionStrings["dcms4"].ConnectionString,
                userName);
            store.ModuleName = moduleName;
            store.ClientInfo = clientInfo;
            _db = store;
        }

        /// <summary>
        /// For use in unit tests
        /// </summary>
        /// <param name="db"></param>
        public AutoCompleteRepository(OracleDatastore db)
        {
            _db = db;
        }


        #endregion


        /// <summary>
        /// For UPC autocomplete
        /// </summary>
        /// <param name="term"></param>
        /// <returns></returns>
        public IEnumerable<Sku> UpcAutoComplete(string term)
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
                    MS.STANDARD_CASE_QTY AS STANDARD_CASE_QTY,
        <a sep='+'>
        CASE
            WHEN MS.UPC_CODE = CAST(:TERM AS VARCHAR2(255)) THEN 13
            WHEN MS.UPC_CODE LIKE CAST(:TERM AS VARCHAR2(255)) || '%' THEN 3
            WHEN MS.UPC_CODE LIKE '%' || CAST(:TERM AS VARCHAR2(255)) THEN 2
            WHEN MS.UPC_CODE LIKE '%' || CAST(:TERM AS VARCHAR2(255)) || '%' THEN 1
            ELSE 0
        END +
        CASE
            WHEN MS.STYLE = CAST(:TERM AS VARCHAR2(255)) THEN 4
            WHEN MS.STYLE LIKE CAST(:TERM AS VARCHAR2(255)) || '%' THEN 3
            WHEN MS.STYLE LIKE '%' || CAST(:TERM AS VARCHAR2(255))  THEN 2
            WHEN MS.STYLE LIKE '%' || CAST(:TERM AS VARCHAR2(255)) || '%' THEN 1
            ELSE 0
        END +
        CASE
            WHEN MS.COLOR = CAST(:TERM AS VARCHAR2(255)) THEN 3
            WHEN MS.COLOR LIKE CAST(:TERM AS VARCHAR2(255)) || '%' THEN 2
            WHEN MS.COLOR LIKE '%' || CAST(:TERM AS VARCHAR2(255))  THEN 1
            ELSE 0
        END +
        CASE
            WHEN MS.DIMENSION = CAST(:TERM AS VARCHAR2(255)) THEN 3
            WHEN MS.DIMENSION LIKE CAST(:TERM AS VARCHAR2(255)) || '%' THEN 2
            WHEN MS.DIMENSION LIKE '%' || CAST(:TERM AS VARCHAR2(255))  THEN 1
            ELSE 0
        END +
        CASE
            WHEN MS.SKU_SIZE = CAST(:TERM AS VARCHAR2(255)) THEN 3
            WHEN MS.SKU_SIZE LIKE CAST(:TERM AS VARCHAR2(255)) || '%' THEN 2
            WHEN MS.SKU_SIZE LIKE '%' || CAST(:TERM AS VARCHAR2(255))  THEN 1
            ELSE 0
        END
        </a>
        <if c='not($TERM)'>0</if> AS RELEVANCE
        FROM <proxy />MASTER_SKU MS
        WHERE 1=1
        <a pre=' AND ' sep=' OR '>
        (MS.STYLE LIKE '%' || CAST(:TERM AS VARCHAR2(255)) || '%' OR  MS.COLOR LIKE '%' || CAST(:TERM AS VARCHAR2(255)) || '%' OR MS.DIMENSION LIKE '%' || CAST(:TERM AS VARCHAR2(255)) || '%' OR
        MS.SKU_SIZE LIKE '%' || CAST(:TERM AS VARCHAR2(255))  || '%' OR MS.UPC_CODE LIKE '%' || CAST(:TERM AS VARCHAR2(255))  || '%')
        </a>
                    ),
                    RELEVANCE_SKU AS (
        SELECT ALL1.UPC_CODE AS UPC_CODE,
        ALL1.SKU_ID AS SKU_ID,
               ALL1.STYLE AS STYLE,
               ALL1.COLOR AS COLOR,
               ALL1.DIMENSION AS DIMENSION,
               ALL1.SKU_SIZE AS SKU_SIZE,
               ALL1.RELEVANCE AS RELEVANCE,
               ALL1.STANDARD_CASE_QTY AS STANDARD_CASE_QTY,
               ROW_NUMBER() OVER(ORDER BY ALL1.RELEVANCE DESC, ALL1.STYLE, ALL1.COLOR, ALL1.DIMENSION, ALL1.SKU_SIZE) AS ROW_NUMBER
          FROM ALL_SKU ALL1
          WHERE ALL1.INACTIVE_FLAG IS NULL
                    )
        SELECT RS.SKU_ID,RS.STYLE,RS.COLOR,RS.DIMENSION,RS.SKU_SIZE,RS.UPC_CODE, RS.STANDARD_CASE_QTY FROM RELEVANCE_SKU RS
        WHERE RS.ROW_NUMBER &lt; 40
        ORDER BY RS.ROW_NUMBER
        ";
            Contract.Assert(_db != null);
            var binder = SqlBinder.Create(row => new Sku
            {
                SkuId = row.GetInteger("SKU_ID").Value,
                Style = row.GetString("STYLE"),
                Color = row.GetString("COLOR"),
                Dimension = row.GetString("DIMENSION"),
                SkuSize = row.GetString("SKU_SIZE"),
                UpcCode = row.GetString("UPC_CODE"),
                StandardSkuSize = row.GetInteger("STANDARD_CASE_QTY")
            }).Parameter("TERM", term);
            var result = _db.ExecuteReader(QUERY, binder);
            return result;
        }
    }
}



//$Id$