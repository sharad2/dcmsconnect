using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Web;
using DcmsMobile.BoxManager.Repository.VasConfiguration;
using EclipseLibrary.Oracle;

namespace DcmsMobile.BoxManager.Repository
{
    public class AutoCompleteRepository
    {
        #region Intialization

        const string MODULE_NAME = "BoxManager";
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
        /// <param name="connectString"> </param>
        /// <param name="userName"></param>
        /// <param name="clientInfo"></param>
        /// <param name="ctx"> </param>
        public AutoCompleteRepository(TraceContext ctx, string connectString, string userName, string clientInfo)
        {
            var db = new OracleDatastore(ctx);
            db.CreateConnection(connectString, userName);

            db.ModuleName = MODULE_NAME;
            db.ClientInfo = clientInfo;
            db.DefaultMaxRows = 10000;      // Allow retrieving up to 10000 rows. Number of cartons can be huge
            _db = db;
        }

        /// <summary>
        /// For use in unit tests
        /// </summary>
        /// <param name="db"></param>
        public AutoCompleteRepository(OracleDatastore db)
        {
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
                CustomerName = row.GetString("CUSTOMER_NAME")
            }).Parameter("TERM", term);

            return _db.ExecuteReader(QUERY, binder);

        }

        public Customer GetCustomer(string customerId)
        {
            const string QUERY =
                        @"SELECT CUST.CUSTOMER_ID AS CUSTOMER_ID, 
                        CUST.NAME AS CUSTOMER_NAME
                        FROM <proxy />CUST CUST
                        WHERE CUST.INACTIVE_FLAG IS NULL
                        <if c='$customerId'> 
                            AND CUST.CUSTOMER_ID =:customerId
                        </if>
            ";
            Contract.Assert(_db != null);
            var binder = SqlBinder.Create(row => new Customer
            {
                CustomerId = row.GetString("CUSTOMER_ID"),
                CustomerName = row.GetString("CUSTOMER_NAME")
            })
            .Parameter("customerId", customerId);
            return _db.ExecuteSingle(QUERY, binder);
        }


        public IEnumerable<Label> GetLabels(string term)
        {
            const string QUERY =
                @"
                SELECT DISTINCT (TSL.LABEL_ID)                    AS LABEL_ID,
                       TSL.LABEL_ID || ' : ' || TSL.DESCRIPTION   AS DESCRIPTION
                  FROM <proxy />TAB_STYLE_LABEL TSL
                 WHERE TSL.INACTIVE_FLAG IS NULL                
                 <if c='$TERM'>
                   AND (UPPER(TSL.LABEL_ID) LIKE '%' || UPPER(:TERM) ||'%' OR UPPER(TSL.DESCRIPTION) LIKE '%' || UPPER(:TERM) ||'%')
                 </if>                        
                   AND ROWNUM &lt; 40
                 ORDER BY TSL.LABEL_ID
                ";
            Contract.Assert(_db != null);
            var binder = SqlBinder.Create(row => new Label
            {
                LabelId = row.GetString("LABEL_ID"),
                Description = row.GetString("DESCRIPTION")
            }).Parameter("TERM", term);

            return _db.ExecuteReader(QUERY, binder);
        }
    }
}