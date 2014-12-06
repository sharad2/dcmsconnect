using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Configuration.Provider;
using System.Linq;
using System.Web;
using System.Web.Security;
using Oracle.ManagedDataAccess.Client;
using EclipseLibrary.Oracle.Helpers;

namespace EclipseLibrary.Oracle.Web.Security
{
    /// <summary>
    /// 4 Apr: Caching the roles in memory.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This provider can return roles for any user <see cref="GetRolesForUser"/>. It can add roles to a user <see cref="AddUsersToRoles"/>
    /// or remove roles from a user <see cref="RemoveUsersFromRoles"/>. It can tell whether a role exists <see cref="RoleExists"/>. For a given role,
    /// it can return users having that role <see cref="GetUsersInRole"/>.
    /// </para>
    /// <para>
    /// A role can be an oracle role or a user privilege defined in the table <c>upriv</c>. Functions which return roles for a user look at the upriv table
    /// and return all privileges for the user as well. Functions which add or remove roles from a user, first check whether the passed role is a privilege by
    /// looking at the <c>priv</c> table. If it turns out to be a privilege, then adding means that a row is inserted in the <c>upriv</c> table amd removing means removing row
    /// from the upriv table. If it is not a privilege, then an oracle role is granted or removed.
    /// </para>
    /// <para>
    /// Indirectly granted oracle roles are detected and are returned in the list of user roles. All roles are returned in upper case. Passed roles are converted to upper case
    /// before they are looked at.
    /// </para>
    /// <para>
    /// Role querying functions retrieve information using the proxy user rights. Granting and revoking functions work under the context of the currently logged in user.
    /// The logged in user must have these oracle privileges to be able to grant and revoke roles.
    /// </para>
    /// <code>
    /// <![CDATA[
    /// grant GRANT ANY ROLE to <user-name>
    /// grant select, insert, delete on UPRIV to <user-name>;
    /// grant select on PRIV to <user-name>;
    /// ]]>
    /// </code>
    /// <para>
    /// 12 Oct 2010: Each privilege specified in table upriv is treated as a role. If the upriv table does not exist,
    /// no error is raised. All retrieved roles are cached for 30 minutes. Therefore it is recommended that <c>cacheRolesInCookie</c>
    /// should always be set to false as shown below.
    /// </para>
    /// <code>
    /// <![CDATA[
    ///<roleManager defaultProvider="OracleProvider" enabled="true" cacheRolesInCookie="false">
    ///    <providers>
    ///    <clear />
    ///    <add name="OracleProvider" type="EclipseLibrary.Oracle.Web.Security.OracleRoleProvider" connectStringName="dcms8" />
    ///    </providers>
    ///</roleManager>
    /// ]]>
    /// </code>
    /// <para>
    /// GL - 21 Oct 2010: The cached roles to the logged - in  user is cleared in the <see cref="ClearRoleCache"/>.
    /// </para>
    /// <para>
    /// Sharad 12 Dec 2011: If the user whose roles are being requested is the same as the proxy user, then the only role returned is WEB_PROXYUSER.
    /// Authorization code can check for this role and decide to unconditionally authorize the request, thereby treating the proxy user as the super user.
    /// AuthorizeExAttribute takes advantage of this.
    /// </para>
    /// </remarks>
    public class OracleRoleProvider : RoleProvider
    {
        #region Initialization
        private readonly ConcurrentDictionary<string, RoleCache> _userRoles;
        /// <summary>
        /// 
        /// </summary>
        public OracleRoleProvider()
        {
            _userRoles = new ConcurrentDictionary<string, RoleCache>(StringComparer.InvariantCultureIgnoreCase);
        }
        /// <summary>
        /// Not used
        /// </summary>
        public override string ApplicationName
        {
            get;
            set;
        }

        /// <summary>
        /// This contains the connection string which refers to the proxy user. It should never be modified by any function.
        /// </summary>
        private OracleConnectionStringBuilder _connectionStringBuilder;

        /// <summary>
        /// How long before the cached roles should be discarded
        /// </summary>
        private readonly TimeSpan MAX_CACHE_DURATION = TimeSpan.FromMinutes(30);

        private class RoleCache
        {
            /// <summary>
            /// When was the role info retrieved
            /// </summary>
            public DateTime TimeStamp { get; set; }

            public string[] Roles { get; set; }
        }

        /// <summary>
        /// Read <c>connectStringName</c> and <c>applicationName</c>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="config"></param>
        public override void Initialize(string name, NameValueCollection config)
        {
            foreach (string key in config)
            {
                switch (key)
                {
                    // The oracle service to connect to
                    case "connectStringName":
                        var str = config[key];
                        var connectionString = ConfigurationManager.ConnectionStrings[str].ConnectionString;
                        if (string.IsNullOrWhiteSpace(connectionString))
                        {
                            str = string.Format("{0} is not a valid connection string name", str);
                            throw new ArgumentNullException("connectStringName", str);
                        }
                        _connectionStringBuilder = new OracleConnectionStringBuilder(connectionString);
                        break;

                    // Name of the application using this provider. This value is not used.
                    case "applicationName":
                        this.ApplicationName = config[key];
                        break;
                }
            }

            if (_connectionStringBuilder == null)
            {
                // ReSharper disable NotResolvedInText
                throw new ArgumentNullException("connectStringName", "We need to connect to an oracle database to retrieve roles for users");
                // ReSharper restore NotResolvedInText
            }
            base.Initialize(name, config);
        }
        #endregion


        #region Queries
        /// <summary>
        /// Returns all roles assigned to the passed user
        /// </summary>
        /// <param name="username"></param>
        /// <returns>Array of roles</returns>
        /// <remarks>
        /// <para>
        /// Roles assigned to roles are also properly handled
        /// </para>
        /// </remarks>
        public override string[] GetRolesForUser(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentNullException("username");
            }

            RoleCache cached;
            if (_userRoles.TryGetValue(username, out cached))
            {
                // Found the roles in the cache.
                if (DateTime.Now - cached.TimeStamp > MAX_CACHE_DURATION)
                {
                    // Cache is stale. Ignore it.
                    _userRoles.TryRemove(username, out cached);
                }
                else
                {
                    //  Thankfully query is avoided
                    return cached.Roles;
                }
            }

            if (string.Compare(_connectionStringBuilder.ProxyUserId, username, true) == 0)
            {
                return new[] { "WEB_PROXYUSER" };
            }

            /*
             * TODO: Use this new query which uses recursive subquery syntax instead of CONNECT BY. This syntax was introduced in 11gR2
             * Inspired by http://technology.amis.nl/blog/6104/oracle-rdbms-11gr2-goodbye-connect-by-or-the-end-of-hierarchical-querying-as-we-know-it
             */
            const string QUERY_ALL_ROLES = @"
                WITH Q1(GRANTED_ROLE,
                PATH) AS
                 (SELECT P.GRANTED_ROLE, CAST(U.USERNAME AS VARCHAR2(2000))
                    FROM DBA_ROLE_PRIVS P
                   INNER JOIN DBA_USERS U
                      ON P.GRANTEE = U.USERNAME
                  UNION ALL
                  SELECT P.GRANTED_ROLE, CAST(Q1.PATH || '/' || P.GRANTEE AS VARCHAR2(2000))
                    FROM DBA_ROLE_PRIVS P
                   INNER JOIN Q1
                      ON Q1.GRANTED_ROLE = P.GRANTEE
                    LEFT OUTER JOIN DBA_USERS U
                      ON P.GRANTEE = U.USERNAME
                   WHERE U.USERNAME IS NULL)
                SELECT DISTINCT Q.GRANTED_ROLE AS ROLES
                  FROM Q1 Q
                 WHERE (Q.PATH = :username OR Q.PATH LIKE :username || '/%')
                 ORDER BY ROLES 
";
            const string QUERY_PRIVILEGES = @"
                SELECT T.PRIV_ID AS PRIVS 
                FROM <proxy />UPRIV T 
                WHERE T.ORACLE_USER_NAME = :username
                ORDER BY PRIVS
";
            cached = new RoleCache { TimeStamp = DateTime.Now };
            //var binder = new SqlBinder<string>("Querying Roles and privileges");
            var binder = SqlBinder.Create(row => row.GetString(0));
            binder.Parameter("username", username.ToUpper());
            //binder.Query = QUERY_ALL_ROLES;
            //binder.Factory = row => row.GetString();
            //binder.CreateMapper(QUERY_ALL_ROLES);
            using (var db = new OracleDatastore(HttpContext.Current.Trace))
            {
                db.CreateConnection(_connectionStringBuilder.ConnectionString, string.Empty);
                IEnumerable<string> roles = db.ExecuteReader(QUERY_ALL_ROLES, binder);
                //binder.Query = QUERY_PRIVILEGES;
                IEnumerable<string> privs;
                try
                {
                    privs = db.ExecuteReader(QUERY_PRIVILEGES, binder);
                }
                catch (OracleDataStoreException ex)
                {
                    if (ex.OracleErrorNumber == 942)
                    {
                        // Table or view does not exist. Stay silent
                        privs = Enumerable.Empty<string>();
                    }
                    else
                    {
                        throw;
                    }
                }
                cached.Roles = roles.Concat(privs).ToArray();
                _userRoles.TryAdd(username, cached);
                return cached.Roles;
            }
        }


        /// <summary>
        /// Following function will return all users that are assigned with the passed role.  
        /// </summary>
        /// <param name="roleName"></param>
        /// <returns>Array of roles</returns>
        /// <remarks>
        /// Users who own schema objects are not returned by this function.
        /// </remarks>
        public override string[] GetUsersInRole(string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
            {
                throw new ArgumentNullException("roleName");
            }
            /* Oracle 11gR2 hierarchical query
             * Inspired by http://technology.amis.nl/blog/6104/oracle-rdbms-11gr2-goodbye-connect-by-or-the-end-of-hierarchical-querying-as-we-know-it
             */
            const string ROLE_USERS = @"
                                       WITH Q1(GRANTED_ROLE,
                                        PATH) AS
                                         (SELECT P.GRANTED_ROLE, CAST(U.USERNAME  AS VARCHAR2(2000))
                                            FROM DBA_ROLE_PRIVS P
                                           INNER JOIN DBA_USERS U
                                              ON P.GRANTEE = U.USERNAME
                                          UNION ALL
                                          SELECT P.GRANTED_ROLE, CAST(Q1.PATH || '/' || P.GRANTEE AS VARCHAR2(2000))
                                            FROM DBA_ROLE_PRIVS P
                                           INNER JOIN Q1
                                              ON Q1.GRANTED_ROLE = P.GRANTEE
                                            LEFT OUTER JOIN DBA_USERS U
                                              ON P.GRANTEE = U.USERNAME
                                           WHERE U.USERNAME IS NULL)
                                        SELECT DISTINCT NVL(SUBSTR(Q.PATH, 1, INSTR(Q.PATH, '/', 1 , 1) - 1), Q.PATH) AS USERS
                                          FROM Q1 Q
                                         WHERE Q.GRANTED_ROLE = :roleName
                                        AND NVL(SUBSTR(Q.PATH, 1, INSTR(Q.PATH, '/', 1 , 1) - 1), Q.PATH) not in
                                        (SELECT OWNER FROM DBA_OBJECTS)
                                        ORDER BY USERS";
            //var binder = new SqlBinder<string>("Retreving users.");
            var binder = SqlBinder.Create(row => row.GetString(0));
            binder.Parameter("rolename", roleName.ToUpper());
            //binder.CreateMapper(ROLE_USERS);
            //binder.Query = ROLE_USERS;
            //binder.Factory = row => row.GetString();
            using (var db = new OracleDatastore(HttpContext.Current.Trace))
            {
                db.CreateConnection(_connectionStringBuilder.ConnectionString, string.Empty);
                var roles = db.ExecuteReader(ROLE_USERS, binder);
                var usersInRole = roles.ToArray();
                return usersInRole;
            }
        }

        /// <summary>
        /// This function will inform whether the passed user is having the passed role.
        /// If yes then the function will return true else return false.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="roleName"></param>
        /// <returns>boolean</returns>
        public override bool IsUserInRole(string username, string roleName)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentNullException("username");
            }
            if (string.IsNullOrWhiteSpace(roleName))
            {
                throw new ArgumentNullException("roleName");
            }

            return GetRolesForUser(username).Contains(roleName);
        }


        /// <summary>
        /// This function is for checking whether the passed role exists or not.
        /// </summary>
        /// <param name="roleName"></param>
        /// <exception cref="NotImplementedException"></exception>
        /// <returns>boolean</returns>
        public override bool RoleExists(string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
            {
                throw new ArgumentNullException("roleName");
            }
            const string QUERY_PRIV = @"SELECT PRIV_ID FROM <proxy />PRIV WHERE PRIV_ID = :rolename";
            const string QUERY_ROLE = @"SELECT ROLE AS ROLE FROM DBA_ROLES  WHERE ROLE = :rolename";

            var strQuery = IsRolePriv(roleName) ? QUERY_PRIV : QUERY_ROLE;

            //var binder = new SqlBinder<string>("Retreving users.");
            var binder = SqlBinder.Create(row => row.GetString(0));
            binder.Parameter("rolename", roleName.ToUpper());
            //binder.CreateMapper(strQuery);
            //binder.Query = strQuery;
            //binder.Factory = row => row.GetString();
            using (var db = new OracleDatastore(HttpContext.Current.Trace))
            {
                db.CreateConnection(_connectionStringBuilder.ConnectionString, string.Empty);
                return !string.IsNullOrEmpty(db.ExecuteSingle(strQuery, binder));
            }
        }
        #endregion

        #region Grant/Revoke
        /// <summary>
        /// This function is for granting the passed roles to the passed users.
        /// </summary>
        /// <param name="usernames"></param>
        /// <param name="roleNames"></param>
        /// <remarks>
        /// <para>
        /// The logged in user must have the rights to add roles. The logged in user must also have the insert rights to add upriv.
        /// Following are the scripts.
        /// </para>
        /// <code>
        /// <![CDATA[
        /// grant GRANT ANY ROLE to <user-name>
        /// grant INSERT on URPIV to <user-name>
        /// ]]>
        /// </code>
        /// </remarks>
        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            if (usernames == null)
            {
                throw new ArgumentNullException("usernames");
            }
            if (roleNames == null)
            {
                throw new ArgumentNullException("roleNames");
            }

            if (HttpContext.Current == null || string.IsNullOrWhiteSpace(HttpContext.Current.User.Identity.Name))
            {
                throw new ProviderException("You must be logged in with proper credentials to add role to a user");
            }

            var joinUsersRoles = from user in usernames
                                 from role in roleNames
                                 where !string.IsNullOrEmpty(user) &&
                                 !string.IsNullOrEmpty(role)
                                 select new
                                 {
                                     Role = role.Trim().ToUpper(),
                                     User = user.Trim().ToUpper()
                                 };

            const string QUERY_PRIV = @"INSERT INTO <proxy />UPRIV (PRIV_ID,ORACLE_USER_NAME) VALUES ('{1}','{0}')";
            const string QUERY_ROLE = @"GRANT {1} to {0}";

            using (var db = new OracleDatastore(HttpContext.Current.Trace))
            {
                db.CreateConnection(_connectionStringBuilder.ConnectionString, HttpContext.Current.User.Identity.Name);
                //var binder = new SqlBinder<string>("Granting Roles.");
                var binder = SqlBinder.Create();
                foreach (var item in joinUsersRoles)
                {
                    var sqlQuery = string.Format(IsRolePriv(item.Role) ? QUERY_PRIV : QUERY_ROLE, item.User, item.Role);

                    ClearRoleCache(item.User);
                    try
                    {
                        db.ExecuteNonQuery(sqlQuery, null);
                    }
                    catch (OracleDataStoreException ex)
                    {
                        switch (ex.OracleErrorNumber)
                        {
                            case 1919:
                            case 942:
                            case 1031:
                                // 1919: Role does not exist
                                // 942 : UPRIV table does not exist. To us this means no rights to insert into table UPRIV
                                //1031 : Rights to insert the upriv are not avaliable

                                throw new ProviderException(string.Format("Role {0} does not exist. This could also mean that you do not have rights to grant this role", item.Role));

                            case 1917:
                                throw new ProviderException(string.Format("At least one of Role {0} or User {1} is invalid", item.Role, item.User));

                            case 1:
                                //Priv already assigned to the user(UNIQUE CONSTRAINT VOILATED) remain silent and move further.
                                continue;

                            default:
                                throw;
                        }
                    }
                }
            }

        }


        /// <summary>
        /// This function is for revoking the passed roles from the passed users.
        /// </summary>
        /// <param name="usernames"></param>
        /// <param name="roleNames"></param>
        /// <remarks>
        /// <para>
        /// Empty user names and roles are silently ignored. All user names and roles are converted to upper case before they are processed.
        /// </para>
        /// <para>
        /// The logged in user must have the rights to revoke roles. The logged in user must also have the delete rights on table upriv to delete user's priv.
        /// Follwing are the scripts.
        /// </para>
        /// <code>
        /// <![CDATA[
        /// grant GRANT ANY ROLE to <user-name>
        /// grant DELETE on URPIV to <user-name>
        /// ]]>
        /// </code>
        /// </remarks>
        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            if (usernames == null)
            {
                throw new ArgumentNullException("usernames");
            }
            if (roleNames == null)
            {
                throw new ArgumentNullException("roleNames");
            }

            if (HttpContext.Current == null || string.IsNullOrWhiteSpace(HttpContext.Current.User.Identity.Name))
            {
                throw new ProviderException("You must be logged in with proper credentials to remove roles from users");
            }

            var joinUsersRoles = from user in usernames
                                 from role in roleNames
                                 where !string.IsNullOrEmpty(user) &&
                                 !string.IsNullOrEmpty(role)
                                 select new
                                 {
                                     Role = role.Trim().ToUpper(),
                                     User = user.Trim().ToUpper()
                                 };

            const string QUERY_PRIV = @"DELETE <proxy />UPRIV WHERE ORACLE_USER_NAME ='{0}' AND PRIV_ID ='{1}'";
            const string QUERY_ROLE = @"REVOKE {1} FROM {0}";

            using (var db = new OracleDatastore(HttpContext.Current.Trace))
            {
                db.CreateConnection(_connectionStringBuilder.ConnectionString, HttpContext.Current.User.Identity.Name);
                foreach (var item in joinUsersRoles)
                {
                    var query = string.Format(IsRolePriv(item.Role) ? QUERY_PRIV : QUERY_ROLE, item.User, item.Role);
                    ClearRoleCache(item.User);
                    try
                    {
                        db.ExecuteNonQuery(query, null);
                    }
                    catch (OracleDataStoreException ex)
                    {
                        switch (ex.OracleErrorNumber)
                        {
                            case 1919:
                            case 942:
                            case 1031:
                                // 1919: Role does not exist
                                // 942 : UPRIV table does not exist. To us this means no rights to delete from UPRIV
                                // 1031: Rights to revoke the role are not avaliable.  
                                throw new ProviderException(string.Format("Role {0} does not exist. This could also mean that you do not have rights to revoke this role", item.Role));

                            case 1951:
                                // Role not granted. Check whether the role has been granted inderectly.
                                const string QUERY_ROLE_PATH = @"
                                                                WITH Q1(GRANTED_ROLE,
                                                                PATH) AS
                                                                 (SELECT P.GRANTED_ROLE, CAST(U.USERNAME AS VARCHAR2(2000))
                                                                    FROM DBA_ROLE_PRIVS P
                                                                   INNER JOIN DBA_USERS U
                                                                      ON P.GRANTEE = U.USERNAME
                                                                  UNION ALL
                                                                  SELECT P.GRANTED_ROLE, CAST(Q1.PATH || '/' || P.GRANTEE AS VARCHAR2(2000))
                                                                    FROM DBA_ROLE_PRIVS P
                                                                   INNER JOIN Q1
                                                                      ON Q1.GRANTED_ROLE = P.GRANTEE
                                                                    LEFT OUTER JOIN DBA_USERS U
                                                                      ON P.GRANTEE = U.USERNAME
                                                                   WHERE U.USERNAME IS NULL)
                                                                SELECT substr(path, instr(path, '/') + 1)
                                                                  FROM Q1 Q
                                                                 WHERE Q.PATH LIKE :username || '/%'
                                                                   and q.granted_role = :rolename
                                                                ";
                                // Execute this query as super user
                                db.CreateConnection(_connectionStringBuilder.ConnectionString, string.Empty);
                                //var binder = new SqlBinder<string>("Get Role Path");
                                var binder = SqlBinder.Create(row => row.GetString(0));
                                //binder.CreateMapper(QUERY_ROLE_PATH);
                                //binder.Query = QUERY_ROLE_PATH;
                                //binder.Factory = row => row.GetString();
                                binder.Parameter("username", item.User);
                                binder.Parameter("rolename", item.Role);
                                var path = db.ExecuteSingle<string>(QUERY_ROLE_PATH, binder);
                                if (!string.IsNullOrEmpty(path))
                                {
                                    var roleToRevoke = path.Split('/').First();
                                    throw new ProviderException(
                                        string.Format(
                                            "Role {0} has indirectly granted to user {1} and cannot be revoked directly. {2}/{0}. To revoke {0} role revoke {3} role.",
                                            item.Role, item.User, path, roleToRevoke));
                                }
                                throw  new ProviderException(ex.Message);
                            case 1917:
                                throw new ProviderException(string.Format("At least one of Role {0} or User {1} is invalid", item.Role, item.User));

                            default:
                                throw;
                        }
                    }
                }
            }
        }
        #endregion

        /// <summary>
        /// This function is for getting users in the passed role.
        /// </summary>
        /// <param name="roleName"></param>
        /// <param name="usernameToMatch"></param>
        /// <returns></returns>
        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            if (string.IsNullOrWhiteSpace(roleName))
            {
                throw new ArgumentNullException("roleName");
            }
            throw new NotImplementedException();

            //            const string ROLE_USERS = @"
            //            WITH Q1(GRANTED_ROLE,
            //            PATH) AS
            //             (SELECT P.GRANTED_ROLE, U.USERNAME
            //                FROM DBA_ROLE_PRIVS P
            //               INNER JOIN DBA_USERS U
            //                  ON P.GRANTEE = U.USERNAME
            //              UNION ALL
            //              SELECT P.GRANTED_ROLE, Q1.PATH || '/' || P.GRANTEE
            //                FROM DBA_ROLE_PRIVS P
            //               INNER JOIN Q1
            //                  ON Q1.GRANTED_ROLE = P.GRANTEE
            //                LEFT OUTER JOIN DBA_USERS U
            //                  ON P.GRANTEE = U.USERNAME
            //               WHERE U.USERNAME IS NULL)
            //            SELECT DISTINCT NVL(SUBSTR(Q.PATH, 1, INSTR(Q.PATH, '/', 1 , 1) - 1), Q.PATH) AS USERS 
            //              FROM Q1 Q
            //             WHERE Q.GRANTED_ROLE = :rolename
            //             AND NVL(SUBSTR(Q.PATH, 1, INSTR(Q.PATH, '/', 1) - 1), Q.PATH)  LIKE :username  || '%' 
            //             ORDER BY USERS
            //            ";

            //            var binder = new SqlBinder<string>("Retreving users.");
            //            binder.Parameter("rolename", roleName.ToUpper());
            //            binder.Parameter("username", usernameToMatch.ToUpper());
            //            binder.CreateMapper(ROLE_USERS);
            //            using (var db = new OracleDatastore(HttpContext.Current.Trace))
            //            {
            //                db.CreateConnection(_connectionStringBuilder.ConnectionString, string.Empty);
            //                var roles = db.ExecuteReader(binder);
            //                var usersFoundInRole = roles.ToArray();
            //                return usersFoundInRole;
            //            }
        }

        #region Helpers
        /// <summary>
        /// Called by <see cref="OracleMembershipProvider"/> whenever a user tries to log in
        /// </summary>
        /// <param name="username"></param>
        internal void ClearRoleCache(string username)
        {
            RoleCache val;
            _userRoles.TryRemove(username, out val);
        }

        /// <summary>
        /// This becomes false if we ever encounter table not exists error. Then it stays false. It helps us in avoiding queries if the table does not exist
        /// </summary>
        private bool _privTablesExist = true;

        /// <summary>
        /// This function is for checking whether the passed role is a priv.
        /// </summary>
        /// <param name="roleName"></param>
        private bool IsRolePriv(string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
            {
                throw new ArgumentNullException("roleName");
            }
            if (!_privTablesExist)
            {
                // This cannot be a privilege
                return false;
            }

            const string strQuery = @"
                SELECT PRIV.PRIV_ID FROM <proxy />PRIV PRIV WHERE PRIV.PRIV_ID = :rolename 
            ";
            //var binder = new SqlBinder<string>("Retreving priv.");
            var binder = SqlBinder.Create(row => row.GetString(0));
            binder.Parameter("rolename", roleName.ToUpper());
            //binder.CreateMapper(strQuery);
            //binder.Query = strQuery;
            //binder.Factory = row => row.GetString();
            using (var db = new OracleDatastore(HttpContext.Current.Trace))
            {
                db.CreateConnection(_connectionStringBuilder.ConnectionString, string.Empty);
                string strPriv = "";
                try
                {
                    strPriv = db.ExecuteSingle(strQuery, binder);
                }
                catch (OracleDataStoreException ex)
                {
                    if (ex.OracleErrorNumber == 942)
                    {
                        // Table PRIV does not exist. Stay silent
                        _privTablesExist = false;
                    }
                }
                return !string.IsNullOrEmpty(strPriv);
            }

        }


//        /// <summary>
//        /// This function is for checking whether the logged in user has systme priv to grant any role.
//        /// </summary>
//        /// <param name="username"></param>
//        private bool IsGrantAnyRoleAllowed(string username)
//        {
//            if (string.IsNullOrWhiteSpace(username))
//            {
//                throw new ArgumentNullException("roleName");
//            }
           
//            const string USER_NAME = @"
//                SELECT  USERNAME 
//                  FROM SYS.USER_SYS_PRIVS 
//                WHERE USERNAME = :username
//                AND PRIVILEGE = 'GRANT ANY ROLE'
//                AND ADMIN_OPTION = 'YES'
//            ";
//            var binder = new SqlBinder<string>("Retreving priv.");
//            binder.Parameter("username", username.ToUpper());
//            binder.CreateMapper(USER_NAME);
//            using (var db = new OracleDatastore(HttpContext.Current.Trace))
//            {
//                db.CreateConnection(_connectionStringBuilder.ConnectionString, username);
//                string strUser = "";
//                try
//                {
//                    strUser = db.ExecuteSingle(binder);
//                }
//                catch
//                {
//                    throw;
//                }

//                return !string.IsNullOrEmpty(strUser); ;
//            }
//        }

//        /// <summary>
//        /// This function is for checking whether the logged in user has role priv to grant the passed role.
//        /// </summary>
//        /// <param name="username"></param>
//        /// <param name="roleName"></param>
//        private bool IsRoleGrantable(string username, string roleName)
//        {
//            if (string.IsNullOrWhiteSpace(roleName))
//            {
//                throw new ArgumentNullException("roleName");
//            }
//            if (string.IsNullOrWhiteSpace(username))
//            {
//                throw new ArgumentNullException("username");
//            }

//            const string ROLE_GRANTABLE = @"
//                SELECT GRANTEE 
//                 FROM DBA_ROLE_PRIVS 
//                WHERE GRANTEE = :username
//                AND GRANTED_ROLE = :rolename
//                AND ADMIN_OPTION = 'YES'
//            ";
//            var binder = new SqlBinder<string>("Retreving priv.");
//            binder.Parameter("username", roleName.ToUpper());
//            binder.Parameter("roleName", roleName.ToUpper());
//            binder.CreateMapper(ROLE_GRANTABLE);
//            using (var db = new OracleDatastore(HttpContext.Current.Trace))
//            {
//                db.CreateConnection(_connectionStringBuilder.ConnectionString, string.Empty);
//                string strGrantee = "";
//                try
//                {

//                    strGrantee = db.ExecuteSingle(binder);
//                }
//                catch
//                {
//                    throw;
//                }
//                return !string.IsNullOrEmpty(strGrantee);
//            }

//        }

        #endregion

        #region Unused functionality

        /// <summary>
        /// NotImplementedException
        /// </summary>
        /// <param name="roleName"></param>
        /// <exception cref="NotImplementedException"></exception>
        public override void CreateRole(string roleName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// NotImplementedException
        /// </summary>
        /// <param name="roleName"></param>
        /// <param name="throwOnPopulatedRole"></param>
        /// <returns></returns>
        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// NotImplementedException
        /// </summary>
        /// <returns></returns>
        public override string[] GetAllRoles()
        {
            throw new NotImplementedException();
        }




        #endregion


    }
}
