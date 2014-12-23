using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Web;
using EclipseLibrary.Oracle;

namespace DcmsMobile.Shipping.Repository
{
    public class AutoCompleteRepository: IDisposable
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

        const string MODULE_NAME = "Shipping";
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

 
        #endregion

        #region CustomerAutoComplete

        /// <summary>
        /// For Customer autocomplete
        /// </summary>
        /// <param name="term"></param>
        /// <returns></returns>
        public IEnumerable<Customer> CustomerAutoComplete(string searchText)
        {
            const string QUERY =
                @"
                SELECT CUST.CUSTOMER_ID AS CUSTOMER_ID, 
                       CUST.NAME AS CUSTOMER_NAME
                FROM <proxy />CUST CUST
                WHERE 1 = 1
                 <if c='$customer'>
                        AND (UPPER(CUST.CUSTOMER_ID) LIKE '%' || UPPER(:customer) ||'%' 
                            OR UPPER(CUST.NAME) LIKE '%' || UPPER(:customer) ||'%')
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
                }).Parameter("customer", searchText);
           
            return _db.ExecuteReader(QUERY,binder);
           
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
            });
            binder.Parameter("customerId", customerId);
            return _db.ExecuteSingle(QUERY, binder);
        }

        #endregion

        #region CarrierAutoComplete
        /// <summary>
        /// To get Carrier list for Carrier Auto Complete text box
        /// </summary>
        /// <param name="searchText">
        /// Search term is passed to populate the list
        /// </param>
        /// <param name="carrierId"> </param>
        /// <returns></returns>
        public IEnumerable<Carrier> GetCarriers(string searchText, string carrierId)
        {
            const string QUERY = @"
                        SELECT mc.carrier_id as carrier_id, 
                                mc.description as description
                            FROM <proxy />v_carrier mc
                        where 1 =1 
                        <if c='$carrierId'> 
                            and mc.carrier_id =:carrierId
                        </if>
                        <else>
                        and (UPPER(mc.carrier_id) LIKE '%' || UPPER(:carrier) ||'%' 
                            OR UPPER(mc.description) LIKE '%' || UPPER(:carrier) ||'%')
                        </else>

                        --AND mc.inactive_flag IS NULL
                        AND ROWNUM &lt; 40
                        ORDER BY mc.carrier_id
                        ";
            var binder = SqlBinder.Create(row => new Carrier()
            {
                CarrierId = row.GetString("carrier_id"),
                Description = row.GetString("description")
            })
                .Parameter("carrier", searchText)
                .Parameter("carrierId", carrierId);
            return _db.ExecuteReader(QUERY, binder);

        }
        #endregion

    }
}



//$Id$