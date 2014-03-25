using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Security.Authentication;
using System.Web;
using EclipseLibrary.Oracle.Helpers;
using Oracle.DataAccess.Client;
using System.Text.RegularExpressions;

namespace EclipseLibrary.Oracle
{
    /// <summary>
    /// Use this class to conveniently execute queries against Oracle database
    /// </summary>
    /// <remarks>
    /// <para>
    /// You must explicitly create the connection using <see cref="CreateConnection"/>.
    /// Proxy authentication is supported.
    /// Query results are returned as strongly typed classes by ExecuteSingle() and ExecuteReader().
    /// </para>
    /// <para>
    /// Retrieving only the first column. In this situation, you can create a binder for a scalar type and not specify any mapping.
    /// </para>
    /// <para>
    /// Sharad 7 Jun 2011: New properties <see cref="ClientInfo"/> and <see cref="ModuleName"/> added.
    /// </para>
    /// <para>
    /// Sharad 12 Aug 2011: Added support for transactions <see cref="BeginTransaction"/>.
    /// </para>
    /// <para>
    /// Sharad 20 Aug: Adding query info to all OracleExceptions. The custom page of DcmsMobile is capable of printing this extra info.
    /// </para>
    /// </remarks>
    /// <example>
    /// <para>
    /// The following is an example of executing a typical query in a repository function.
    /// </para>
    /// <code>
    /// <![CDATA[
    ///internal IList<Box> GetArchivePickslipBoxes(int pickslipId)
    ///{
    ///    const string QUERY_ArchivePickslipBoxes = @"
    ///        SELECT MAX(DEM_BOX.UCC128_ID) AS UCC128_ID,
    ///            COUNT(*) AS SKU_IN_BOX,
    ///            NULL AS AREA,
    ///            SUM(DEM_PSDET.QUANTITY_ORDERED) AS CURRENT_PIECES,
    ///            SUM(DEM_PSDET.QUANTITY_ORDERED) AS EXPECTED_PIECES
    ///    FROM DEM_PICKSLIP_DETAIL_H DEM_PSDET
    ///    INNER JOIN DEM_BOX_H DEM_BOX
    ///        ON DEM_PSDET.PICKSLIP_ID = DEM_BOX.CHECKING_ID
    ///    WHERE DEM_PSDET.PICKSLIP_ID = :PICKSLIP_ID
    ///    GROUP BY DEM_PSDET.STYLE,
    ///            DEM_PSDET.COLOR,
    ///            DEM_PSDET.DIMENSION,
    ///            DEM_PSDET.SKU_SIZE
    ///        ";
    ///        
    ///    var binder = new SqlBinder<Box>("GetArchivePickslipBoxes");
    ///    binder.CreateMapper(QUERY_ArchivePickslipBoxes, config =>
    ///    {
    ///        config.CreateMap<Box>()
    ///            .MapField("UCC128_ID", dest => dest.Ucc128Id)
    ///            .MapField("AREA", dest => dest.Area)
    ///            .MapField("EXPECTED_PIECES", dest => dest.ExpectedPieces)
    ///            .MapField("CURRENT_PIECES", dest => dest.CurrentPieces)
    ///            .MapField("SKU_IN_BOX", dest => dest.CountSku)
    ///            ;
    ///    });
    ///    binder.Parameter("PICKSLIP_ID", pickslipId);
    ///    var result = _db.ExecuteReader(binder);
    ///    return result;
    ///}
    /// ]]>
    /// </code>
    /// </example>
    public sealed class OracleDatastore : IDisposable
    {
        #region Construction and Destruction

        private readonly TraceContext _traceContext;

        private readonly OracleConnection _conn;

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// </remarks>
        public OracleDatastore(TraceContext traceContext)
        {
            _traceContext = traceContext;
            this.DefaultMaxRows = 1000;         // Never retrieve more than 1000 rows

            _conn = new OracleConnection();

            _conn.StateChange += Connection_StateChange;
        }

        public int DefaultMaxRows { get; set; }

        /// <summary>
        /// Disposes the connection
        /// </summary>
        public void Dispose()
        {
            _conn.Dispose();
        }
        #endregion

        #region Connection

        /// <summary>
        /// For use by the ProxyTagResolver
        /// </summary>
        private OracleConnectionStringBuilder _builder;

        /// <summary>
        /// The value which will replace <proxy/> tag in queries, e.g. dcms8.
        /// </summary>
        private string _proxyTagValue;

        /// <summary>
        /// Creates a connection on behalf of the passed <paramref name="userId"/>.
        /// </summary>
        /// <param name="connectString"></param>
        /// <param name="userId"></param>
        /// <remarks>
        /// <para>
        /// If you are not using proxy authentication, then the <paramref name="connectString"/> is used as is and <paramref name="userId"/>
        /// is ignored.
        /// </para>
        /// <para>
        /// The normal case is that the passed <paramref name="connectString"/> contains the <c>ProxyUserId</c> and
        /// <c>ProxyPassword</c> attributes. The passed userId is simply set as the <c>UserId</c> attribute of the connect string.
        /// </para>
        /// <para>
        /// If the user is not authenticated, <paramref name="userId"/> will be null. In this case the passed <paramref name="connectString"/> is modified
        /// to make the ProxyUserId as the real user id. This connection will therefore not use a proxy.
        /// </para>
        /// </remarks>
        public void CreateConnection(string connectString, string userId)
        {
            _builder = new OracleConnectionStringBuilder(connectString);
            if (string.IsNullOrEmpty(_builder.ProxyUserId))
            {
                // Proxy not being used. Nothing to do. Ignore userId. Use connect string as is.
            }
            else if (string.IsNullOrEmpty(userId) || string.Compare(_builder.ProxyUserId, userId, true) == 0)
            {
                // Anonymous user wants to execute a query
                // Special case: If userId is same as proxy user, Create a direct connection for the passed user.
                // This prevents the unreasonable error "dcms4 not allowed to connect on behalf of dcms4".
                // Treat the proxy as the real user and remove the proxy attributes.
                _builder.UserID = _builder.ProxyUserId;
                _builder.Password = _builder.ProxyPassword;
                _builder.ProxyUserId = string.Empty;
                _builder.ProxyPassword = string.Empty;
            }
            else
            {
                // Proxy is being used.
                _builder.UserID = userId;
            }
            //if (_conn != null)
            //{
            //    _conn.Dispose();
            //}
            if (_builder.PersistSecurityInfo)
            {
                if (_traceContext != null)
                {
                    // Is null during unit tests
                    _traceContext.Warn("Connection String",
                                       "The connection string specifies PersistSecurityInfo=true. This setting is not recommended and is not necessary.");
                }
                _builder.PersistSecurityInfo = false;
            }

            if (_conn.State != ConnectionState.Closed)
            {
                _conn.Close();
            }
            _conn.ConnectionString = _builder.ConnectionString; // factory.CreateConnection();
            //_proxyUserId = (_builder.ProxyUserId ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(_builder.ProxyUserId))
            {
                _proxyTagValue = null;
            }
            else
            {
                _proxyTagValue = _builder.ProxyUserId.Trim() + ".";
            }
        }

        /// <summary>
        /// Called whenever the connection is opened. Set the module and client info
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Connection_StateChange(object sender, StateChangeEventArgs e)
        {
            if (e.CurrentState == ConnectionState.Open)
            {
                var conn = (OracleConnection)sender;
                conn.ModuleName = this.ModuleName;
                conn.ClientInfo = this.ClientInfo;
                if (_traceContext != null)
                {
                    var msg = string.Format("Connection opened at {0:F}. ModuleName: {1}; ClientInfo: {2}", DateTimeOffset.Now, this.ModuleName, this.ClientInfo);
                    _traceContext.Write("Connection Opened", msg);
                    //Encrypt the password with '****'
                    _traceContext.Write("Connection String", Regex.Replace(conn.ConnectionString, @"(\bPASSWORD=\S*;\b)", "PASSWORD=****"));
                }
            }
        }

        /// <summary>
        /// The connection to be used for executing queries
        /// </summary>
        /// <remarks>
        /// <para>
        /// The connection can be created using the <see cref="CreateConnection"/> method.
        /// </para>
        /// </remarks>
        public OracleConnection Connection
        {
            get
            {
                // This may be closed but it will not be null
                if (string.IsNullOrWhiteSpace(_conn.ConnectionString))
                {
                    // To maintain compatibility with EclipseLibrary.WebForms, we return null if CreateConnection() has not been called.
                    return null;
                }
                return _conn;
            }
        }

        private string _moduleName;

        /// <summary>
        /// Set via DBMS_APPLICATION_INFO. Max 48 characters. If it is too long, we chop off the initial characters.
        /// </summary>
        /// <remarks>
        /// You should set this property immediately after creating the connection. It will be passed along with each query which will
        /// be executed against this connection.
        /// </remarks>
        public string ModuleName
        {
            get
            {
                return _moduleName;
            }
            set
            {
                if (_moduleName != value)
                {
                    _moduleName = value.Substring(Math.Max(value.Length - 48, 0));
                }
            }
        }

        private string _clientInfo;
        /// <summary>
        /// Set via DBMS_APPLICATION_INFO.
        /// </summary>
        /// <remarks>
        /// You should set this property immediately after creating the connection. It will be passed along with each query which will
        /// be executed against this connection.
        /// Max 64 characters
        /// </remarks>
        public string ClientInfo
        {
            get
            {
                return _clientInfo;
            }
            set
            {
                if (_clientInfo != value)
                {
                    _clientInfo = value.Substring(Math.Max(value.Length - 64, 0));
                }
            }
        }

        #endregion

        #region Transactions
        /// <summary>
        /// This is the only function needed to create a transaction
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// The commit and rollback methods are available on the returned transaction object.
        /// If you forget to commit, none of your changes will be saved. This actually is helpful behavior because we do not have to worry about
        /// rolling back in case of an exception.
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// using (var trans = _db.BeginTransaction()) {
        ///   foreach (var dispos in dispositions)
        ///   {
        ///       _db.ExecuteNonQuery(...);
        ///   }
        ///   trans.Commit();
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public DbTransaction BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            //Contract.Assert(_conn != null, "Connection must be created");
            if (_conn.State == ConnectionState.Closed)
            {
                _conn.Open();
            }
            return _conn.BeginTransaction();
        }
        #endregion

        #region Public Sql Execution Functions

        /// <summary>
        /// This overload executed Array DML
        /// </summary>
        /// <param name="xml"></param>
        /// <param name="binder"></param>
        /// <returns></returns>
        public int ExecuteDml(string xml, SqlBinderDmlArray binder)
        {
            if (binder == null)
            {
                throw new ArgumentNullException("binder");
            }

            //Contract.Ensures(this.Connection != null, "The connection should be created before executing a query");

            OracleCommand cmd = null;
            try
            {
                cmd = this.CreateCommand(xml, binder.GetParameter, binder.ActionName);
                //PrepareConnection(binder.ActionName);
                cmd.ArrayBindCount = binder.ArrayBindCount;

                var rowsAffected = cmd.ExecuteNonQuery();
                binder.OnQueryExecuted(rowsAffected, cmd.Parameters);
                return rowsAffected;
            }
            catch (OracleException ex)
            {
                if (_traceContext != null)
                {
                    this._traceContext.Warn("Exception", "", ex);
                }
                if (ex.Number == 1866)
                {
                    // The datetime class in invalid. This is an Oracle internal error, wich in my opinion is an oracle bug.
                    // It is raised when the DML query is returning a date column and no rows are affected.
                    // This code hides this oracle bug
                    binder.OnQueryExecuted(0, cmd.Parameters);
                    return 0;
                }
                throw new OracleDataStoreException(ex, cmd);
            }
            finally
            {
                if (cmd != null)
                {
                    foreach (var parameter in cmd.Parameters.OfType<IDisposable>())
                    {
                        parameter.Dispose();
                    }
                    cmd.Dispose();
                }
                QueryLogging.TraceQueryEnd(_traceContext);
            }
        }

        public void ExecuteNonQuery(string xml, SqlBinder binder)
        {
            ExecuteDml(xml, binder);
            return;
        }

        /// <summary>
        /// Executes the passed query using the passed parameters
        /// </summary>
        /// <param name="xml">The query to execute</param>
        /// <param name="binder">Parameter information for the query. Null if there are no parameters.</param>
        /// <remarks>
        /// </remarks>
        public int ExecuteDml(string xml, SqlBinder binder)
        {
            OracleCommand cmd = null;
            try
            {
                // Sharad 3 Jun 2011: Handling the case when binder is null
                if (binder == null)
                {
                    cmd = this.CreateCommand(xml, null, null);
                    //PrepareConnection(null);
                }
                else
                {
                    cmd = this.CreateCommand(xml, binder.GetParameter, binder.ActionName);
                    //PrepareConnection(binder.ActionName);
                }

                var rowsAffected = cmd.ExecuteNonQuery();
                if (binder != null)
                {
                    binder.OnQueryExecuted(rowsAffected, cmd.Parameters);
                }
                return rowsAffected;
            }
            catch (OracleException ex)
            {
                if (_traceContext != null)
                {
                    this._traceContext.Warn("Exception", "", ex);
                }
                throw new OracleDataStoreException(ex, cmd);
            }
            finally
            {
                if (cmd != null)
                {
                    foreach (var parameter in cmd.Parameters.OfType<IDisposable>())
                    {
                        parameter.Dispose();
                    }
                    cmd.Dispose();
                }
                QueryLogging.TraceQueryEnd(_traceContext);
            }
        }

        /// <summary>
        /// Executes the query and returns the first row as a strongly typed object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="xmlQuery"></param>
        /// <param name="binder"></param>
        /// <returns>The first row as a strongly typed object, or null if no row was found</returns>
        /// <remarks>
        /// <para>
        /// See <see cref="OracleDatastore"/> for a code example.
        /// </para>
        /// </remarks>
        public T ExecuteSingle<T>(string xmlQuery, SqlBinder<T> binder)
        {
            Contract.Assert(binder != null);
            OracleCommand cmd = null;
            OracleDataReader reader = null;
            try
            {
                cmd = CreateCommand(xmlQuery, binder.GetParameter, binder.ActionName);
                //PrepareConnection(binder.ActionName);

                reader = cmd.ExecuteReader(CommandBehavior.SingleRow);
                var result = binder.MapRows(reader).FirstOrDefault();
                return result;
            }
            catch (OracleException ex)
            {
                if (_traceContext != null)
                {
                    this._traceContext.Warn("Exception", "", ex);
                }
                throw new OracleDataStoreException(ex, cmd);
            }
            finally
            {
                if (reader != null)
                {
                    reader.Dispose();
                }
                if (cmd != null)
                {
                    foreach (OracleParameter parameter in cmd.Parameters)
                    {
                        parameter.Dispose();
                    }
                    cmd.Dispose();
                }
                QueryLogging.TraceQueryEnd(_traceContext);
            }
        }


        public IList<T> ExecuteReader<T>(string xmlQuery, SqlBinder<T> binder)
        {
            return ExecuteReader(xmlQuery, binder, 0);
        }


        /// <summary>
        /// Executes the query using the information in the passed <paramref name="binder"/>. When no rows are found, an empty list is returned.
        /// </summary>
        /// <typeparam name="T">The type of each row in the list of results</typeparam>
        /// <param name="xmlQuery"></param>
        /// <param name="binder">Information which describes the query to execute</param>
        /// <param name="maxRows">Maximum number of rows to retrieve. 0 means all.</param>
        /// <returns>A list of rows returned by the query</returns>
        /// <exception cref="ArgumentNullException"><paramref name="binder"/> is null</exception>
        /// <exception cref="OperationCanceledException">Number of rows retturned by query exceeded <see cref="DefaultMaxRows"/></exception>
        /// <exception cref="OracleException">Query execution error</exception>
        /// <remarks>
        /// <para>
        /// The connection is opened if it is not already open.
        /// </para>
        /// <para>
        /// Sharad 17 Nov 2011: Added new parameter <paramref name="maxRows"/>
        /// </para>
        /// </remarks>
        public IList<T> ExecuteReader<T>(string xmlQuery, SqlBinder<T> binder, int maxRows)
        {
            if (binder == null)
            {
                throw new ArgumentNullException("binder");
            }
            OracleCommand cmd = null;
            OracleDataReader reader = null;
            try
            {
                cmd = CreateCommand(xmlQuery, binder.GetParameter, binder.ActionName);
                //PrepareConnection(binder.ActionName);

                reader = cmd.ExecuteReader(maxRows == 1 ? CommandBehavior.SingleRow : CommandBehavior.Default);
                if (reader.RowSize > 0 && maxRows > 0)
                {
                    // Safety check. FetchSize optimization is applied only if RowSize is known and maxRows is specified
                    // Treat default Fetch Size specified in configuration as the maximum allowed.
                    // See whether we can reduce the fetch size to save memory
                    reader.FetchSize = Math.Min(reader.RowSize * maxRows, reader.FetchSize);
                }
                // Lazy loading will not work because the map context will not be available for mapping
                //var rowcount = 0;
                //foreach (var result in from object row in reader select binder.Mapper.Engine.Map<IOracleDataRow, T>(dict))
                var results = binder.MapRows(reader).Take(maxRows > 0 ? maxRows : this.DefaultMaxRows).ToList();
                if (maxRows == 0 && reader.Read())
                {
                    // User did not specify maxRows and we did not read all of them. Generate error.
                    var msg = string.Format("Query aborted because more than {0} rows were retrieved",
                                            this.DefaultMaxRows);
                    if (_traceContext != null)
                    {
                        _traceContext.Warn(msg);
                    }
                    throw new OperationCanceledException(msg);
                    //}
                }
                if (_traceContext != null)
                {
                    _traceContext.Write(string.Format("ExecuteReader returned {0} rows", results.Count));
                }
                var b = reader.NextResult();
                return results;
            }
            catch (OracleException ex)
            {
                if (_traceContext != null)
                {
                    this._traceContext.Warn("Exception", "", ex);
                }
                if (ex.Number == 28150)
                {
                    var userId = _builder.UserID;
                    var proxyUser = _builder.ProxyUserId;
                    var msg = string.Format("User '{0}' was not authenticated to connect as proxy user", userId) + "\n\n Excute following script to get rid of this error: \n\n" +
                              string.Format("ALTER USER {0} GRANT CONNECT THROUGH {1};", userId, proxyUser);
                    throw new AuthenticationException(msg);
                }
                throw new OracleDataStoreException(ex, cmd);
            }
            finally
            {
                if (reader != null)
                {
                    reader.Dispose();
                }
                if (cmd != null)
                {
                    foreach (OracleParameter parameter in cmd.Parameters)
                    {
                        parameter.Dispose();
                    }
                    cmd.Dispose();
                }
                QueryLogging.TraceQueryEnd(_traceContext);
            }

        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        private OracleCommand CreateCommand(string xml, Func<string, OracleParameter> paramUpdater, string actionName)
        {

            OracleCommand cmd = _conn.CreateCommand();
            cmd.CommandText = xml;

            XmlToSql.BuildCommand(cmd, paramUpdater, (elem) =>
            {
                if (_proxyTagValue == null)
                {
                    return false;
                }
                elem.Value = _proxyTagValue;
                return true;
            });
            if (_conn.State == ConnectionState.Closed)
            {
                _conn.Open();
            }
            cmd.BindByName = true;
            cmd.InitialLONGFetchSize = 1024;    // Retrieve first 1K chars from a long column
            _conn.ActionName = actionName;
            QueryLogging.TraceOracleCommand(_traceContext, cmd, actionName);
            return cmd;
        }
        #endregion


    }
}
