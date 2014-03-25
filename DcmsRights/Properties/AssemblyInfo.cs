using System.Reflection;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("DcmsRights")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Eclipse Systems")]
[assembly: AssemblyProduct("DcmsMobile.DcmsRights")]
[assembly: AssemblyCopyright("Copyright © Eclipse Systems 2011")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Revision and Build Numbers 
// by using the '*' as shown below:

/*
 * Change log from 1.0.0.0 to (Shiva 28 Mar 2012.) Tagged by Hemant on 28 Mar 2012
 * - Create a group of users along with roles.
 * - Grant / Revoke roles from a specific user. This is from Manage users page.
 * - Show Audit info of any particular user.
 * - Remove group of users from a specific role.
 * - Reset password / Password expiry.
 * - Delete user.
 * - Lock / Unlock account.
 * - Kill session forcefully of any particular user.
 * 
 * Change log from 1.0.0.0 -> 1.0.1.0(Tagged By Ankit Sharma on 1 May 2012)
 * Sharad 1 May 2012: Createuser now relies on OraclemembershipProvider to set profile and CONNECT THROUGH rights.
 * Provided login link along with the message which indicates that the user is viewing a readonly screen.
 * 
 * Change log from 1.0.1.0 -> 1.0.1.1(Tagged By Ankit Sharma on 1 May 2012)
 * Display order changed from 500 to 1000
 * 
 * Change log from 1.0.1.1 -> 1.0.1.2(Tagged By Ankit Sharma on 2 May 2012)
 * Sharad 2 May 2012: Added code to prevent anyone from giving rights to this application.
 * 
 * Change log from 1.0.1.2 -> 1.0.2.0 (Branched by Ankit Sharma 22 May 2012)
 *   Sharad 22 May 2012:
 *     -- Excluded critical programs from LegacyProgramRoles.xml as suggested by John Campos. Updated the home page to indicate that some programs
 *        have been removed and provided a link to the scanned page.
 *     -- C2P has been removed from LegacyProgramRoles.xml because MVC version of C2P is expected to get installed within the next few days.
 *     
 * Change log from 1.0.2.0 -> 1.0.3.0 (Branched by Ankit Sharma 8 June 2012 )
 *   Deepak 8 June 2012:
 *   We do not allow any user to assign rights to himself/herself. This was requested by compliance department of Maidenform.   
 *   
 * Change log from 1.0.3.0 -> 1.0.4.0 (Branched by Ankit Sharma 22 oct 2012)
 * Sharad 8 Aug 2012: Does not use IPluggableArea2. Will require updated version of DcmsMobile.
 * Sharad 9 Aug 2012: Applications which use IPluggableArea2 will be invisible to DcmsRights. So we should install this version only after every other app has been installed.
 * Rajesh Kandari 30 Aug 2012: Added SearchRoute class to make the area searchable.
 * Sharad 4 Sep 2012: Upgraded to T4MVC 2.10
 * Ankit Sharma: Bug Fixed for kill session we were only allowing first session of list to get killed.
 * Ankit Sharma: Added message if user do not have any role granted on Manage User page.
 * Ankit Sharma: Added count of user removed from role on Remove User from role view.
 * Ankit Sharma: Removed warnings
 * Ankit Sharma: Removed AutoMapper
 */

/* Change log from 1.0.4.0 -> 1.0.5.0(Branched by Binod Kumar 08 jan 2013)
 * Sharad 23 Nov 2012: Now parsing AuthorizeEx applied to Controllers
 * Binod  29 Dec 2012: Upgraded to MVC4
 * Binod 29 Dec 2012: Removed the dependency of EclipseLibrary.Mvc.Helpers.ReflectionHelpers.NameFor
 */
[assembly: AssemblyVersion("1.0.5.0")]
[assembly: AssemblyFileVersion("1.0.5.0")]
