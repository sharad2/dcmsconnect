using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("EclipseLibrary.Oracle")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("http://vcs/svn/net35/Libraries/EclipseLibrary.Oracle/trunk")]
[assembly: AssemblyCompany("")]
#if DEBUG
[assembly: AssemblyProduct("EclipseLibrary.Oracle - Debug Version")]
#else
[assembly: AssemblyProduct("EclipseLibrary.Oracle")]
#endif
[assembly: AssemblyCopyright("Copyright ©  2008 Eclipse Systems Private Limited")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("5cae6226-476b-4c05-b204-dae2efe23b17")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
/*
 * 7.0.0.2 -> 7.0.0.3
 * - OracleMembershipProvider: ChangePassword logic corrected.
 * 
 * 7.0.0.3 -> 7.0.0.4 (Sharad 21 Jan 2012)
 *  - Removed obsolete interface IOracleDataStore
 *  - Mapping OracleXmlType to string. This supports PIVOT XML queries.
 *  
 * 7.0.0.4 -> 7.0.1.0 (Sharad 17 Feb 2012)
 *  - Oracle error 28150 Proxy not authorized to connect as client has been special handled to provide better diagnostics.
 *    It now explains how the error can be resolved.
 *  - Supported voluntary changes of passwords. Earlier only expired passwords could be changed.
 *  - GetRolesForUser uses modern Oracle 11g query to traverse the role hierarchy. The end result is still the same.
 *  - OracleRoleProvider now implements functions to add/remove roles and to query roles for a user.
 *  - OracleMembershipProvider bug fix. USER_ID should have been int, not string.
 *  - OracleMembershipUser class has been made public so that UIs can display oracle specific user info.
 *  
 * 7.0.1.0 -> 7.0.2.0 (DB:7 March 2012)
 *  - Critical bug fix: "ORA-01489: result of string concatenation is too long" can occur if the role hierarchy is very deep. Fixed this bug by
 *  explicitly setting the path length limit to 2000 which should be sufficient in most cases. This problem can occur for
 *  GetRolesForUser() or GetUsersInRole().
 *  - Automapper maps for queries now use MD5 hash. This should make key collisions much less likely. This should also resolve automapper errors which users
 *    encounter infrequently.
 *  - Individual query maps are now discarded if they have not been used in a long time. This should provide some memory optimizations.
 *  - New functions implemented in OracleMembershipProvider for future use in DcmsMobile
 *  - OracleRoleProvider code optimized
 *  
 *  7.0.2.0 -> 7.0.3.0 (Sharad:28 March 2012) tagged by Hemant on 28 mar 2012
 *   - Internal refactoring of OracleMembershipProvider code to support DcmsRights operations
 *   - Special handling for bool parameters in SqlBinder. true is treated as 1 and false is treated as null
 *   - Special handling array parameters in SqlBinder.
 *   
 * 7.0.3.0 -> 7.0.4.0 (Sharad:28 March 2012) tagged by Deepak on 29 march 2012
 *  - Introduced interface IOracleDatastore3 which has a new function ExecuteDml() which returns number of rows affected.
 *  - OracleRoleProvider now raises exception when trying to remove a role which has not been directly granted to the user. Providing good diagnostics.
 *  - OracleRoleProvider.GetUsersInRole() does not return users who own schema objects.
 *  
 * 7.0.4.0 -> 7.0.5.0 tagged by Hemant on 03 May 2012 
 * Sharad 30 Apr 2012: Wrote EnumMapper to provide support for enumerations.
 * Sharad 1 May 2012:
 *   OracleMembershipProvider - Only users with prespecified profiles are visible to the provider. The profiles are specified in web.config.
 *   OracleMembershipProvider - Create user now grants default profile and also grants CONNECT THROUGH privilege to newly created users.
 *   SqlBinder - Array parameters are now supported for parameter binding.
 *   
 *  7.0.5.0 -> 7.0.6.0 tagged by Dinesh on 6 Sep 2012
 *  Sharad 22 May 2012: OK to set parameter value multiple times. This is useful if you are inserting multiple rows. This was added to support PickWave box creation.
 *  Sharad 21 Aug 2012: Supporting OracleTimeStampTZ.
 *  Sharad 27 Aug 2012: EnumMapper made more robust. It only maps OracleString and OracleDecimal to Enum.
 *  Binod  29 Aug 2012: Now we are supporting the Nullable Types for OutParameter
 *  Sharad 30 Aug 2012: Invented OracleDataStoreException which shows the query on the yellow screen.
 *  Sharad 5 Sep 2012: Checking for null cmd in OracleDatastoreException
 *  
*  7.0.6.0 -> 7.1.0.0 tagged by Dinesh on 17 Sep 2012
 *  Sharad 7 Sep 2012: removed dependence on EclipseLibrary.Core. Copied all relevant code to within this project
 *  Sharad 10 Sep 2012: Refactoring parameter binding. Major change.
 *  Sharad 13 Sep 2012: All automapper based parameter binding functions have been deprecated. New alternatives have been provided. 
 *  Sharad 13 Sep 2012: Added ParameterAssociativeArray()
 *  Sharad 17 Sep 2012: OutRefCursorParameter() is now chainable. Becoming defensive while setting the FetchSize.
 *  
 * 7.1.0.0 -> 7.1.1.0 tagged by Hemant on 18th Sep 2012
 * Sharad 17 Sep 2012: Bug Fix. FindUsersByName() crashes if createDate or passwordExpiryDate is null.
 * 
 * 7.1.1.0 -> 7.2.0.0(Branched by Ankit Sharma on 7 dec 2012)
 * Sharad 24 Sep 2012: Introduced class SqlBinderBase. This provides better partitioning for functions required by PL/SQL blocks and queries.
 *   New Features: Ability to retrieve multiple ref cursors. Binding PL/SQL arrays. Support for DML Array binding.
 *   
 * Sharad 26 Sep 2012: Removed interfaces IOracleDatastore32 and IOracleDatastore3
 * Sharad 28 Sep 2012: Removed OracleDataSource and moved it to EclipseLibrary.WebForms.
 * Sharad 3 Oct 2012: Added support for Array DML
 * Sharad 9 Oct 2012: Added support for TimeStamp database columns. Fixed ChangePassword issues in OracleMembershipProvider.
 * Sharad 15 Oct 2012: Removed all obsolete code. No longer dependent on AutoMapper.
 * Sharad 19 Oct 2012: Wrote OracleDataStoreErrorEvent so that database exceptions can be logged even if the exception is caught and handled.
 * Deepak and Rajesh 07-11-2012: Suppoted TimeSpan in out parameter.
 */
/*
 * 7.2.0.0 -> 7.2.1.0
 *   Sharad 17 Jan 2013: Added function GetDateTimeOffset() in preparation for deprecating GetDate()
 */

/*  7.2.1.0 -> 7.2.2.0 (Branched by Ankit Sharma on 18 JAN 2014)
 * Sharad 30 Aug 2013: In OracleDataStore, storing the OracleConnectionStringBuilder to optimize resolving the <proxy/> tag.
 * Made ProxyTagResolver private. Module and ClientInfo properties are now set when the connection state changes from closed to opened
 * 
 * Ankit(18 JAN 2013): Added new function GetLong() for handling long integer values.
 */

/* 7.2.2.0 -> 7.2.3.0 (Branched by Ankit Sharma on 25 MAR 2014)
 * Now in Trace we have encrypted password to ****.
 */
[assembly: AssemblyVersion("7.2.3.0")]
[assembly: AssemblyFileVersion("7.2.3.0")]
