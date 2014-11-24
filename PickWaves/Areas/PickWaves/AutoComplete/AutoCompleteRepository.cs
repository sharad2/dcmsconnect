using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Web;
using DcmsMobile.PickWaves.Repository.Config;
using EclipseLibrary.Oracle;

namespace DcmsMobile.PickWaves.Repository
{
    internal class AutoCompleteRepository : PickWaveRepositoryBase
    {
        #region Intialization

        public AutoCompleteRepository(TraceContext ctx, string clientInfo)
            : base(ctx, string.Empty, clientInfo)
        {

        }
        #endregion

        /// <summary>
        /// For Customer autocomplete
        /// </summary>
        /// <param name="term"></param>
        /// <returns></returns>
        public IEnumerable<Customer> CustomerAutoComplete(string term)
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
                        AND CUST.INACTIVE_FLAG IS NULL
                        AND ROWNUM &lt; 40
                        ORDER BY CUST.CUSTOMER_ID
                ";
            Contract.Assert(_db != null);
            var binder = SqlBinder.Create(row => new Customer
            {
                CustomerId = row.GetString("CUSTOMER_ID"),
                Name = row.GetString("CUSTOMER_NAME"),
                IsActive = true
            }).Parameter("TERM", term);

            return _db.ExecuteReader(QUERY, binder);

        }

        /// <summary>
        /// To get Style list for Style Auto Complete text box
        /// </summary>
        /// <param name="searchText">
        /// Search term is passed to populate the list
        /// </param>
        /// <param name="style"> </param>
        /// <returns></returns>
        public IEnumerable<Style> StyleAutoComplete(string searchText, string style)
        {
            const string QUERY =@"
                SELECT MS.STYLE AS STYLE, MS.DESCRIPTION AS DESCRIPTION
                  FROM <proxy />MASTER_STYLE MS
                 WHERE 1=1
                        <if c='$style'> 
                            and MS.STYLE =:style
                        </if>
                    <else>
                   AND (UPPER(MS.STYLE) LIKE '%' || UPPER(:SEARCH) || '%')
                    </else>
                   AND ROWNUM &lt; 40
";
            var binder = SqlBinder.Create(row => new Style()
                {
                    StyleId = row.GetString("STYLE"),
                    Description = row.GetString("DESCRIPTION")
                }).Parameter("SEARCH", searchText)
                .Parameter("style",style);
            return _db.ExecuteReader(QUERY,binder);
        }
    }

}