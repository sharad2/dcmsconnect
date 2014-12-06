using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Configuration.Provider;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Security;
using Oracle.ManagedDataAccess.Client;
using EclipseLibrary.Oracle.Helpers;

namespace EclipseLibrary.Oracle.Web.Security
{
    /// <summary>
    /// Tries to log into oracle with the passed user name and password.
    /// Thus users need to be managed using oracle tools.
    /// </summary>
    /// <remarks>
    /// You must specify the <c>connectionStringName</c> property in web.config to indicate which oracle
    /// server should be used for authenticating users.
    /// <code>
    /// <![CDATA[
    ///     <membership defaultProvider="OracleMembershipProvider">
    ///  <providers>
    ///    <clear/>
    ///    <add name="OracleMembershipProvider" type="EclipseLibrary.Oracle.Web.Security.OracleMembershipProvider"
    ///       connectionStringName="dcms4" applicationName="DcmsWebMF" />
    ///  </providers>
    /// </membership>
    /// ]]>
    /// </code>
    /// <para>
    /// The <c>User ID</c> specified in the connect string must have ALTER USER system privilege.
    /// </para>
    /// <para>
    /// The connections created do not participate in connection pooling. This ensures that recently locked
    /// or deleted users will not be able to authenticate.
    /// </para>
    /// <para>
    /// Sharad 21 Oct 2010: Role cache is flushed each time the user logs in. See <see cref="ValidateUser"/>.
    /// </para>
    /// <para>
    /// Sharad 4 Jan 2012: ChangePassword logic corrected
    /// </para>
    /// <para>
    /// For creating a new user one should logged in with required credentials. Following are the required grants. A new user will 
    /// always be created with expiry password. Thinking behind this is that we are allowing user to create his/her own password.
    /// </para>
    ///  <code>
    /// <![CDATA[
    /// GRANT CREATE USER TO <user-name>
    /// ]]>
    /// </code>
    /// <para>
    /// For deleting a user the logged in user must have the rights to drop a User. Following is the script for this.
    /// </para>
    /// <code>
    /// <![CDATA[
    /// GRANT DROP USER To <user-name>;
    /// ]]>
    /// </code>
    /// <para>
    /// For resetting a password or for un-locking an account of a user the logged user must have following rights.
    /// </para>
    /// <code>
    /// <![CDATA[
    /// GRANT ALTER USER TO <user-name>;
    /// ]]>
    /// </code>
    /// </remarks>
    public partial class OracleMembershipProvider : MembershipProvider
    {
        /// <summary>
        /// Not used
        /// </summary>
        public override string ApplicationName
        {
            get;
            set;
        }

        private string _connectionString;

        /// <summary>
        /// These proxy users will be granted CONNECT THROUGH rights when a new user is created.
        /// </summary>
        private string[] _proxyUsers;

        /// <summary>
        /// Users having one of these profiles will be visible to this provider. All other users will be invisible.
        /// CreateUser grants the first profile to created users. 
        /// DeleteUser will only delete users who have one of these profiles.
        /// </summary>
        private string[] _visibleProfiles;

        /// <summary>
        /// Read <c>connectionStringName</c> and <c>applicationName</c>.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="config"></param>
        /// <remarks>
        /// Multiple values for <c>connectionStringName</c> can be specified, comma seperated. The first connection string is used for all operations. The proxy users of the subsequent
        /// connection strings are used when a user is created. Each proxy user is granted connect through rights for the user.
        /// </remarks>
        public override void Initialize(string name, NameValueCollection config)
        {
            string str;
            string[] tokens;
            foreach (string key in config)
            {
                switch (key)
                {
                    case "connectionStringName":
                        str = config[key];
                        tokens = str.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        _connectionString = ConfigurationManager.ConnectionStrings[tokens[0]].ConnectionString;
                        _proxyUsers = tokens.Select(p => ConfigurationManager.ConnectionStrings[p].ConnectionString)
                            .Select(p => new OracleConnectionStringBuilder(p))
                            .Select(p => p.ProxyUserId)
                            .ToArray();
                        break;

                    // Name of the application using this provider. This value is not used.
                    case "applicationName":
                        this.ApplicationName = config[key];
                        break;

                    case "defaultProfile":
                        str = config[key].ToUpper();
                        _visibleProfiles = str.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToArray();
                        break;
                }
            }
            base.Initialize(name, config);
        }

        /// <summary>
        /// Returns true if we are able to successfully connect to oracle using the supplied username and password.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <exception cref="System.Web.Security.MembershipPasswordException">Password has expired and needs to be changed before login can be allowed</exception>
        /// <returns></returns>
        public override bool ValidateUser(string username, string password)
        {
            var builder = new OracleConnectionStringBuilder(_connectionString)
                              {
                                  UserID = username,
                                  Password = password,
                                  Pooling = false,
                                  ProxyUserId = string.Empty,
                                  ProxyPassword = string.Empty
                              };
            OracleDatastore db = null;
            try
            {
                db = new OracleDatastore(HttpContext.Current.Trace);
                db.CreateConnection(builder.ConnectionString, string.Empty);
                db.Connection.Open();
                return true;
            }
            catch (OracleException ex)
            {
                // Connection could not be opened
                Trace.TraceWarning(ex.Message);
                switch (ex.Number)
                {
                    case 1017:
                        // Invalid user name password
                        Trace.TraceWarning("Invalid password specified for user {0}", username);
                        return false;

                    case 28001:
                        // Password expired
                        throw new MembershipPasswordException("Password has expired. Please change your password and try again.", ex);

                    default:
                        throw;
                }

            }
            finally
            {
                // For clearing the cached roles of the user.
                OracleRoleProvider orp = Roles.Providers.OfType<OracleRoleProvider>().SingleOrDefault();
                if (orp != null)
                {
                    orp.ClearRoleCache(username);
                }
                if (db != null)
                {
                    db.Dispose();
                }
            }
        }

        /// <summary>
        /// The password change will succeed only if the old password is valid.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="oldPassword"></param>
        /// <param name="newPassword"></param>
        /// <returns>true if password successfully changed. false if the old password is invalid</returns>
        /// <remarks>
        /// Any data base exception encountered will be propagated to the caller.
        /// Sharad 15 Feb 2012: Supported voluntary changes of passwords. Earlier only expired passwords could be changed.
        /// Sharad 21 Feb 2012: Raising ValidatingPassword event
        /// </remarks>
        public override bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentNullException("username");
            }
            if (string.IsNullOrWhiteSpace(oldPassword))
            {
                throw new ArgumentNullException("oldPassword");
            }
            if (string.IsNullOrWhiteSpace(newPassword))
            {
                throw new ArgumentNullException("newPassword");
            }
            var e = new ValidatePasswordEventArgs(username, newPassword, true);
            OnValidatingPassword(e);
            if (e.Cancel)
            {
                // App decided to cancel user creation
                return false;
            }
            var builder = new OracleConnectionStringBuilder(_connectionString)
                              {
                                  UserID = username,
                                  Password = oldPassword,
                                  Pooling = false,
                                  ProxyUserId = string.Empty,
                                  ProxyPassword = string.Empty
                              };
            // Try to login as passed user with old password to ensure that the old password is valid
            using (var db = new OracleDatastore(HttpContext.Current.Trace))
            {
                var msg = string.Format("Opening connection to {0} for user {1}",
                                          builder.DataSource, builder.UserID);
                Trace.WriteLine(msg, "OracleMembershipProvider");
                db.CreateConnection(builder.ConnectionString, builder.UserID);
                Trace.WriteLine(msg, "Opening connection with old password");
                try
                {
                    db.Connection.Open();
                }
                catch (OracleException ex)
                {
                    switch (ex.Number)
                    {
                        case 1017:
                            // Invalid user name password
                            Trace.TraceWarning("Invalid password specified for user {0}", username);
                            return false;

                        case 28001:
                            // If we are using ODP.NET, we can change the password now
                            // This will only work if the user's password has expired
                            Trace.WriteLine(msg, "Password expired error oracle exception encountered");
                            db.Connection.OpenWithNewPassword(newPassword);
                            return true;

                        default:
                            throw;
                    }
                }

                // If we get here, the old password was valid. Now we will change the password
                //REPLACE is used to remove exception ORA-28221
                Trace.WriteLine(msg, "Executing ALTER USER with new password");
                var query = string.Format("ALTER USER {0} IDENTIFIED BY \"{1}\" REPLACE \"{2}\"", username, newPassword, oldPassword);
                db.ExecuteNonQuery(query, null);

            }
            return true;
        }

        /// <summary>
        /// Returns full information about the passed user
        /// </summary>
        /// <param name="username"></param>
        /// <param name="userIsOnline"></param>
        /// <returns></returns>
        /// <remarks>
        /// The returned info contains audit log of the user as well
        /// and also returned info contains session log of user.
        /// Users who own schema objects are not visible to this function since our goal is to manage application users only. We do not want to manage
        /// application owners.
        /// </remarks>
        public override MembershipUser GetUser(string username, bool userIsOnline)
        {
            if (string.IsNullOrEmpty(username))
            {
                throw new ArgumentNullException("username");
            }

            OracleMembershipUser user;
            using (var db = new OracleDatastore(HttpContext.Current.Trace))
            {
                //db.ConnectionString = _connectionString;
                //db.ProviderName = _providerName;
                db.CreateConnection(_connectionString, string.Empty);
                const string QUERY = @"
                                        SELECT U.USERNAME    AS USERNAME,
                                               U.LOCK_DATE   AS LOCK_DATE,
                                               U.CREATED     AS CREATION_DATE,
                                               U.USER_ID     AS USER_ID,
                                               U.EXPIRY_DATE AS EXPIRYDATE
                                          FROM DBA_USERS U
                                         WHERE U.USERNAME = :USERNAME
                                           AND U.USERNAME NOT IN (SELECT OWNER FROM DBA_OBJECTS)";
                //var binder = new SqlBinder<OracleMembershipUser>("Querying User properties");
                var binder = SqlBinder.Create(src => new OracleMembershipUser(
                                                                                    userName: src.GetString("USERNAME"),
                                                                                    providerUserKey: src.GetInteger("USER_ID").ToString(),
                                                                                    lastLockoutDate: src.GetDate("LOCK_DATE") ?? DateTime.MinValue,
                                                                                    createDate: src.GetDate("CREATION_DATE").Value,
                                                                                    passwordExpiryDate: src.GetDate("ExpiryDate") ?? DateTime.MinValue
                                                                                    ));
                binder.Parameter("username", username.ToUpper());
                //binder.CreateMapper(QUERY, config => config.CreateMap<OracleMembershipUser>()
                //                                         .ConstructUsing(src => new OracleMembershipUser
                //                                                                    (
                //                                                                    userName: src.GetValue<string>("USERNAME"),
                //                                                                    providerUserKey: src.GetValue<int>("USER_ID").ToString(),
                //                                                                    lastLockoutDate: src.GetValue<DateTime>("LOCK_DATE"),
                //                                                                    createDate: src.GetValue<DateTime>("CREATION_DATE"),
                //                                                                    passwordExpiryDate: src.GetValue<DateTime>("ExpiryDate")
                //                                                                    )).ForAllMembers(opt => opt.Ignore()));

                //binder.Query = QUERY;
                //binder.Factory = src => new OracleMembershipUser(
                //                                                                    userName: src.GetString("USERNAME"),
                //                                                                    providerUserKey: src.GetInteger("USER_ID").ToString(),
                //                                                                    lastLockoutDate: src.GetDate("LOCK_DATE").Value,
                //                                                                    createDate: src.GetDate("CREATION_DATE").Value,
                //                                                                    passwordExpiryDate: src.GetDate("ExpiryDate").Value
                //                                                                    );
                user = db.ExecuteSingle(QUERY, binder);

                if (user != null)
                {
                    user.AuditLog = DoGetUserAudit(username, db);
                    user.Sessions = DoGetUserSessions(username, db);
                }
            }

            return user;
        }

        /// <summary>
        /// This method return session log of user 
        /// </summary>
        /// <param name="username"></param>
        /// <param name="db"></param>
        /// <returns>
        /// The returned info contains session log of the user as well.
        /// </returns>
        private IEnumerable<OracleMembershipUserSession> DoGetUserSessions(string username, OracleDatastore db)
        {
            const string QUERY_SESSIONS = @"
                                           SELECT S.SID,
                                                  S.SERIAL#,
                                                  S.PROGRAM,
                                                  S.STATUS,
                                                  S.OSUSER,
                                                  S.MACHINE,
                                                  S.MODULE,
                                                  S.ACTION,
                                                  S.CLIENT_INFO,
                                                  S.LOGON_TIME,
                                                  S.STATE
                                           FROM GV$SESSION S
                                           WHERE S.TYPE = 'USER'
                                                 AND S.USERNAME IS NOT NULL
                                                 AND S.USERNAME = :USERNAME
                                            ORDER BY S.USERNAME DESC
                                            ";
            //var binderSession = new SqlBinder<OracleMembershipUserSession>("Querying User properties");
            var binderSession = SqlBinder.Create(row => new OracleMembershipUserSession
            {
                SessionId = row.GetInteger("SID").Value,
                SerialNumber = row.GetInteger("SERIAL#").Value,
                OsExecutableName = row.GetString("PROGRAM"),
                IsActive = row.GetString("STATUS") == "ACTIVE",
                OsUserName = row.GetString("OSUSER"),
                MachineName = row.GetString("MACHINE"),
                Module = row.GetString("MODULE"),
                ActionName = row.GetString("ACTION"),
                ClientInfo = row.GetString("CLIENT_INFO"),
                LogonTime = row.GetDate("LOGON_TIME").Value,
                State = row.GetString("STATE")
            });
            binderSession.Parameter("USERNAME", username.ToUpper());
            //binderSession.Query = QUERY_SESSIONS;
            //binderSession.Factory = row => new OracleMembershipUserSession
            //{
            //    SessionId = row.GetInteger("SID").Value,
            //    SerialNumber = row.GetInteger("SERIAL#").Value,
            //    OsExecutableName = row.GetString("PROGRAM"),
            //    IsActive = row.GetString("STATUS") == "ACTIVE",
            //    OsUserName = row.GetString("OSUSER"),
            //    MachineName = row.GetString("MACHINE"),
            //    Module = row.GetString("MODULE"),
            //    ActionName = row.GetString("ACTION"),
            //    ClientInfo = row.GetString("CLIENT_INFO"),
            //    LogonTime = row.GetDate("LOGON_TIME").Value,
            //    State = row.GetString("STATE")
            //};
            //binderSession.CreateMapper(QUERY_SESSIONS, config => config.CreateMap<OracleMembershipUserSession>()
            //    .MapField("SID", p => p.SessionId)
            //    .MapField("SERIAL#", p => p.SerialNumber)
            //    .MapField("PROGRAM", p => p.OsExecutableName)
            //    .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.GetValue<string>("STATUS") == "ACTIVE"))
            //    .MapField("OSUSER", p => p.OsUserName)
            //    .MapField("MACHINE", p => p.MachineName)
            //    .MapField("MODULE", p => p.Module)
            //    .MapField("ACTION", p => p.ActionName)
            //    .MapField("CLIENT_INFO", p => p.ClientInfo)
            //    .MapField("LOGON_TIME", p => p.LogonTime)
            //    .MapField("STATE", p => p.State)
            //);
            return db.ExecuteReader(QUERY_SESSIONS, binderSession);
        }

        /// <summary>
        /// This method return audit log of user 
        /// </summary>
        /// <param name="username"></param>
        /// <param name="db"></param>
        /// <returns>
        /// The returned info contains audit log of the user as well.
        /// </returns>
        private static IList<OracleMembershipUserAudit> DoGetUserAudit(string username, OracleDatastore db)
        {
            const string QUERY_AUDIT = @"
                                                (SELECT T.ACTION_NAME AS ACTION_NAME,
                                                    NULL          AS ROLE_NAME,
                                                    T.USERNAME    AS USERNAME,
                                                    T.OS_USERNAME AS OS_USERNAME,
                                                    T.TERMINAL    AS TERMINAL,
                                                    DECODE(T.RETURNCODE, 0, 'SUCCESS', 'FAILURE') AS RESULT,
                                                    T.TIMESTAMP   AS TIMESTAMP
                                                FROM DBA_AUDIT_TRAIL T
                                                WHERE T.ACTION_NAME IN ('CREATE USER', 'DROP USER', 'ALTER USER')
                                                AND T.OBJ_NAME = :USERNAME
                                            UNION ALL
                                            SELECT T.ACTION_NAME,
                                                    T.OBJ_NAME,
                                                    T.USERNAME,
                                                    T.OS_USERNAME,
                                                    T.TERMINAL,
                                                    DECODE(T.RETURNCODE, 0, 'SUCCESS', 'FAILURE') AS RESULT,
                                                    T.TIMESTAMP
                                                FROM DBA_AUDIT_TRAIL T
                                                WHERE T.ACTION_NAME IN ('GRANT ROLE', 'REVOKE ROLE')
                                                AND T.GRANTEE = :USERNAME
                                            UNION ALL
                                            SELECT 'GRANT DCMS PRIVILEGE',
                                                    PRIV_ID,
                                                    NVL(MODIFIED_BY, CREATED_BY),
                                                    NULL,
                                                    NULL,
                                                    'SUCCESS',
                                                    NVL(DATE_MODIFIED, DATE_CREATED)
                                                FROM UPRIV
                                                WHERE UPRIV.ORACLE_USER_NAME = :USERNAME)
                                                ORDER BY TIMESTAMP DESC";
            //var binderAudit = new SqlBinder<OracleMembershipUserAudit>("Querying User properties");
            var binderAudit = SqlBinder.Create(row => new OracleMembershipUserAudit
            {
                ActionName = row.GetString("ACTION_NAME"),
                RoleName = row.GetString("ROLE_NAME"),
                Result = row.GetString("RESULT"),
                ByOsUserName = row.GetString("OS_USERNAME"),
                TerminalName = row.GetString("TERMINAL"),
                ActionTime = row.GetDate("TIMESTAMP").Value,
                ByOracleUserName = row.GetString("USERNAME"),
            });
            binderAudit.Parameter("USERNAME", username.ToUpper());
            //binderAudit.Query = QUERY_AUDIT;
            //binderAudit.Factory = row => new OracleMembershipUserAudit
            //{
            //    ActionName = row.GetString("ACTION_NAME"),
            //    RoleName = row.GetString("ROLE_NAME"),
            //    Result = row.GetString("RESULT"),
            //    ByOsUserName = row.GetString("OS_USERNAME"),
            //    TerminalName = row.GetString("TERMINAL"),
            //    ActionTime = row.GetDate("TIMESTAMP").Value,
            //    ByOracleUserName = row.GetString("USERNAME"),
            //};
            //binderAudit.CreateMapper(QUERY_AUDIT, config => config.CreateMap<OracleMembershipUserAudit>()
            //    .MapField("ACTION_NAME", p => p.ActionName)
            //    .MapField("ROLE_NAME", p => p.RoleName)
            //    .MapField("RESULT", p => p.Result)
            //    .MapField("OS_USERNAME", p => p.ByOsUserName)
            //    .MapField("TERMINAL", p => p.TerminalName)
            //    .MapField("TIMESTAMP", p => p.ActionTime)
            //    .MapField("USERNAME", p => p.ByOracleUserName)
            //);
            return db.ExecuteReader(QUERY_AUDIT, binderAudit);
        }


        /// <summary>
        /// This function is for creating a new user.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="email">Ignored</param>
        /// <param name="passwordQuestion">Ignored</param>
        /// <param name="passwordAnswer">Ignored</param>
        /// <param name="isApproved">Ignored</param>
        /// <param name="providerUserKey">Ignored</param>
        /// <param name="status">
        /// <para>
        /// Can return InvalidUserName, DuplicateUserName, InvalidPassword or Success
        /// </para>
        /// </param>
        /// <returns>User object when <paramref name="status"/> = Success; null otherwise. </returns>
        /// <remarks>
        /// <para>
        /// The user is always created with an expired password. The default profile is assigned to the user. CONNECT THROUGH rights are given to the proxy user.
        /// </para>
        /// <para>
        /// The logged in user must have the rights to crete User. Following is the script.
        /// </para>
        /// <code>
        /// <![CDATA[
        /// GRANT CREATE USER TO <user-name> 
        /// ]]>
        /// </code>
        /// </remarks>
        public override MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer,
            bool isApproved, object providerUserKey, out MembershipCreateStatus status)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentNullException("username");
            }
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentNullException("password");
            }
            var e = new ValidatePasswordEventArgs(username, password, true);
            OnValidatingPassword(e);
            if (e.Cancel)
            {
                // App decided to cancel user creation
                status = MembershipCreateStatus.InvalidPassword;
                return null;
            }

            if (HttpContext.Current == null || string.IsNullOrWhiteSpace(HttpContext.Current.User.Identity.Name))
            {
                throw new MembershipCreateUserException("You must be logged in with proper credentials to create a user");
            }

            EnsureDefaultProfile();
            //var builder = new OracleConnectionStringBuilder(_connectionString);

            using (var db = new OracleDatastore(HttpContext.Current.Trace))
            {
                db.CreateConnection(_connectionString, HttpContext.Current.User.Identity.Name);
                try
                {
                    var sqlQuery = string.Format("CREATE USER {0} IDENTIFIED BY \"{1}\" PROFILE {2} PASSWORD EXPIRE", username, password, _visibleProfiles[0]);
                    db.ExecuteNonQuery(sqlQuery, null);
                    foreach (var proxy in _proxyUsers)
                    {
                        sqlQuery = string.Format("ALTER USER {0} GRANT CONNECT THROUGH {1}", username, proxy);
                        db.ExecuteNonQuery(sqlQuery, null);
                    }
                    status = MembershipCreateStatus.Success;
                    // GetUser gets too much information, so we are using FindUserByName.
                    //return GetUser(username, false);
                    int totalRecords;
                    return FindUsersByName(username, 0, 100, out totalRecords).Cast<MembershipUser>().First();
                }
                catch (OracleDataStoreException ex)
                {
                    switch (ex.OracleErrorNumber)
                    {
                        case 1935:
                        //1935: missing user or role name (comes when passing null username). Not expected as we are already checking the passed user.
                        case 922:
                            //922: Missing or invalid option (comes when password contains special chars or whitespace)
                            throw new MembershipCreateUserException("User name or password is invalid", ex);

                        case 1031:
                            //1031: insufficient privileges
                            throw new MembershipCreateUserException("You do not have sufficient privileges for creating users.", ex);

                        case 1920:
                            //1920: user name 'user-name' conflicts with another user 
                            throw new MembershipCreateUserException(string.Format("User {0} already exists", username));
                        case 28003:
                            // ORA-28003: password verification for the specified password failed
                            throw new MembershipCreateUserException(ex.Message, ex);

                        default:
                            throw;
                    }
                }
            }
        }

        private void EnsureDefaultProfile()
        {
            if (_visibleProfiles == null || _visibleProfiles.Length == 0)
            {
                throw new MembershipCreateUserException(@"Please specify the defaultProfile for new users in web.config, e.g.
<add name=""OracleMembershipProvider"" type=""EclipseLibrary.Oracle.Web.Security.OracleMembershipProvider""
connectionStringName=""dcms8"" applicationName=""DcmsWeb"" defaultProfile=""SO_DCMS_SINGLE"" />
");
            }
        }

        /// <summary>
        /// This function is for deleting an existing user.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="deleteAllRelatedData">Ignored</param>
        /// <returns>This function will return True if user successfully deleted else return False</returns>
        /// <remarks>
        /// <para>
        /// The logged in user must have the rights to drop a User. Following is the script.
        /// </para>
        /// <code>
        /// <![CDATA[
        /// GRANT DROP USER To <user-name>;
        /// ]]>
        /// </code>
        /// </remarks>
        public override bool DeleteUser(string username, bool deleteAllRelatedData)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentNullException("username");
            }

            if (HttpContext.Current == null || string.IsNullOrWhiteSpace(HttpContext.Current.User.Identity.Name))
            {
                throw new ProviderException("You must be logged in with proper credentials for deleting a user");
            }

            using (var db = new OracleDatastore(HttpContext.Current.Trace))
            {
                db.CreateConnection(_connectionString, HttpContext.Current.User.Identity.Name);
                try
                {
                    var sqlQuery = string.Format("DROP USER {0}", username);
                    db.ExecuteNonQuery(sqlQuery, null);
                    return true;
                }
                catch (OracleDataStoreException ex)
                {
                    switch (ex.OracleErrorNumber)
                    {

                        case 1031:
                            //1031: insufficient privileges
                            throw new ProviderException("You do not have sufficient privileges for deleting a user.", ex);

                        case 1918:
                            //1918: user does not exist
                            throw new ProviderException(string.Format("User {0} does not exits", username), ex);

                        case 921:
                            //921: invalid username  
                            throw new ProviderException("User name is invalid", ex);

                        case 1940:
                            //1940: Already logged in user is trying to delete itself.
                            throw new ProviderException("Cannot drop a user that is currently connected");

                        default:
                            throw;
                    }
                }
            }
        }

        /// <summary>
        /// This function is for un-locking a locked user account.
        /// </summary>
        /// <param name="userName"></param>
        /// <remarks>
        /// <para>
        /// The logged in user must have the rights for resetting password of a user. Following is the script.
        /// </para>
        /// <code>
        /// <![CDATA[
        /// GRANT ALTER USER TO <user-name>;
        /// ]]>
        /// </code>
        ///</remarks>
        /// <returns>This function will return True on successful unlock else return False</returns>
        public override bool UnlockUser(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
            {
                throw new ArgumentNullException("userName");
            }

            if (HttpContext.Current == null || string.IsNullOrWhiteSpace(HttpContext.Current.User.Identity.Name))
            {
                throw new ProviderException("You must be logged in with proper credentials for un locking a user account");
            }

            using (var db = new OracleDatastore(HttpContext.Current.Trace))
            {
                db.CreateConnection(_connectionString, HttpContext.Current.User.Identity.Name);
                try
                {
                    var sqlQuery = string.Format("ALTER USER {0} ACCOUNT UNLOCK", userName);
                    db.ExecuteNonQuery(sqlQuery, null);
                    return true;
                }
                catch (OracleDataStoreException ex)
                {
                    switch (ex.OracleErrorNumber)
                    {

                        case 1031:
                            //1031: insufficient privileges
                            throw new ProviderException("You do not have sufficient privileges for unlocking a locked user account.", ex);

                        case 1918:
                            //1918: user does not exist
                            throw new ProviderException(string.Format("User {0} does not exits", userName), ex);

                        default:
                            throw;
                    }
                }
            }
        }

        /// <summary>
        /// The password is changed to <paramref name="answer"/>. The password is set to expire immediately which will force the user to change password at next login.
        /// </summary>
        /// <param name="username">Name of the user need to reset password</param>
        /// <param name="answer">The new password, or empty to randomply generate a password</param>
        /// <returns>This function will return the new assigned password</returns>
        /// <remarks>
        /// <para>
        /// The logged in user must have the rights for resetting password of a user. Following is the script.
        /// </para>
        /// <code>
        /// <![CDATA[
        /// GRANT ALTER USER TO <user-name>;
        /// ]]>
        /// </code>
        ///</remarks>
        ///
        public override string ResetPassword(string username, string answer)
        {
            var rand = new Random();
            if (string.IsNullOrEmpty(answer))
            {
                answer = rand.Next(1, (int)Math.Pow(10, this.MinRequiredPasswordLength) - 1).ToString().PadRight(this.MinRequiredPasswordLength, '1');
            }

            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentNullException("username");
            }

            if (HttpContext.Current == null || string.IsNullOrWhiteSpace(HttpContext.Current.User.Identity.Name))
            {
                throw new ProviderException("You must be logged in with proper credentials for resetting a user's password");
            }

            using (var db = new OracleDatastore(HttpContext.Current.Trace))
            {
                db.CreateConnection(_connectionString, HttpContext.Current.User.Identity.Name);
                try
                {
                    var sqlQuery = string.Format("ALTER USER {0} IDENTIFIED BY \"{1}\" PASSWORD EXPIRE", username, answer);
                    db.ExecuteNonQuery(sqlQuery, null);
                    return answer;
                }
                catch (OracleDataStoreException ex)
                {
                    switch (ex.OracleErrorNumber)
                    {
                        case 1935:
                        //1935: missing user or role name (comes when username is null). Not expected as we are already checking the passed user.
                        case 922:
                            //922: Missing or invalid option (comes when username contains special chars or whitespace)
                            throw new ProviderException("User name is invalid", ex);

                        case 1031:
                            //1031: insufficient privileges
                            throw new ProviderException("You do not have sufficient privileges for resetting password.", ex);

                        default:
                            throw;
                    }
                }
            }
        }
        /// <summary>
        /// Returns all matching users who do not own any schema objects. Thus all users returned can potentially be deleted.
        /// </summary>
        /// <param name="usernameToMatch">This can contain the wildcard character %</param>
        /// <param name="pageIndex">Not used</param>
        /// <param name="pageSize">Not used</param>
        /// <param name="totalRecords">Not used</param>
        /// <returns></returns>
        public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            EnsureDefaultProfile();
            const string QUERY = @"SELECT U.USERNAME, U.USER_ID, U.LOCK_DATE, U.CREATED, U.EXPIRY_DATE
                                          FROM DBA_USERS U
                                         WHERE UPPER(U.USERNAME) LIKE :TERM
                                           AND U.USERNAME NOT IN (SELECT OWNER FROM DBA_OBJECTS)
AND u.profile IN <a pre='(' sep=',' post=')'>:profile</a>
                                         ORDER BY U.USERNAME";

            var binder = SqlBinder.Create(src => new OracleMembershipUser(
                                                                                userName: src.GetString("USERNAME"),
                                                                                providerUserKey: src.GetInteger("USER_ID").ToString(),
                                                                                lastLockoutDate: src.GetDate("LOCK_DATE") ?? DateTime.MinValue,
                                                                                createDate: src.GetDate("created") ?? DateTime.MinValue,
                                                                                passwordExpiryDate: src.GetDate("expiry_date") ?? DateTime.MinValue
                                                                                ));
            binder.Parameter("TERM", usernameToMatch.ToUpper());
            binder.ParameterXmlArray("profile", _visibleProfiles);

            var result = new MembershipUserCollection();
            using (var db = new OracleDatastore(HttpContext.Current.Trace))
            {
                db.CreateConnection(_connectionString, string.Empty);
                var usersList = db.ExecuteReader(QUERY, binder);
                foreach (var user in usersList)
                {
                    result.Add(user);
                }
            }
            totalRecords = result.Count;
            return result;
        }

        #region Unused functionality

        /// <summary>
        /// NotImplementedException
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="newPasswordQuestion"></param>
        /// <param name="newPasswordAnswer"></param>
        /// <exception cref="NotImplementedException"></exception>
        /// <returns></returns>
        public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// We allow resetting of password
        /// </summary>
        public override bool EnablePasswordReset
        {
            get { return true; }
        }

        /// <summary>
        /// Not possible to retrieve password
        /// </summary>
        public override bool EnablePasswordRetrieval
        {
            get { return false; }
        }

        /// <summary>
        /// NotImplementedException
        /// </summary>
        /// <param name="emailToMatch"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalRecords"></param>
        /// <returns></returns>
        public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns all users. Sessions and audit is not returned.
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalRecords"></param>
        /// <returns></returns>
        public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            return FindUsersByName("%", 0, int.MaxValue, out totalRecords);
        }

        /// <summary>
        /// NotImplementedException
        /// </summary>
        /// <returns></returns>
        public override int GetNumberOfUsersOnline()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// NotImplementedException
        /// </summary>
        /// <param name="username"></param>
        /// <param name="answer"></param>
        /// <returns></returns>
        public override string GetPassword(string username, string answer)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// NotImplementedException
        /// </summary>
        /// <param name="providerUserKey"></param>
        /// <param name="userIsOnline"></param>
        /// <returns></returns>
        public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// NotImplementedException
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public override string GetUserNameByEmail(string email)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// NotImplementedException
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
        public override int MaxInvalidPasswordAttempts
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// We do not worry about password strength
        /// </summary>
        public override int MinRequiredNonAlphanumericCharacters
        {
            get { return 0; }
        }

        /// <summary>
        /// Password cannot be empty
        /// </summary>
        public override int MinRequiredPasswordLength
        {
            get { return 9; }
        }

        /// <summary>
        /// NotImplementedException
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
        public override int PasswordAttemptWindow
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// NotImplementedException
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
        public override MembershipPasswordFormat PasswordFormat
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// NotImplementedException
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
        public override string PasswordStrengthRegularExpression
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// We do not care about secret questions
        /// </summary>
        public override bool RequiresQuestionAndAnswer
        {
            get { return false; }
        }

        /// <summary>
        /// We have no use for emails
        /// </summary>
        public override bool RequiresUniqueEmail
        {
            get { return false; }
        }



        /// <summary>
        /// NotImplementedException
        /// </summary>
        /// <param name="user"></param>
        /// <exception cref="NotImplementedException"></exception>
        public override void UpdateUser(MembershipUser user)
        {
            throw new NotSupportedException();
        }
        #endregion
    }
}
