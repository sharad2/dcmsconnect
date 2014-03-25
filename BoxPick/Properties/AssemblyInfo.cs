using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("BoxPick")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Eclipse Systems Private Limited")]
[assembly: AssemblyCopyright("Copyright © Eclipse Systems 2011")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("7de577e5-2659-4abe-b735-08ba8ffcaeea")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Revision and Build Numbers 
// by using the '*' as shown below:
// Change log from 1.0.0.0 to 1.0.1.0
// Added carton suspense feature.

// Change log from 1.0.1.0 to 1.0.2.0
// Minor improvement: when a carton is not present in box table. we do not ask for putting it in suspense.

// Change log from 1.0.2.0 to 1.0.2.1
// Minor improvement:Using "BOXPICK" as Action code instead of "Picking carton".


// Change log from 1.0.2.1 -> 1.0.3.0
// - Changed the AssemblyCompany and AssemblyCopyright info
// - Optimized carton view HTML to make important information bigger. This was done pursuant to feedback from users.
// - Critical Bug Fix: Program crashes when the user attempts to under pitch a pallet during ADR pulling

// Change log from 1.0.3.0 -> 1.0.4.0(Updated by Dinesh Singh Panwar,12 july 2012).
// - Fixed bug. The area was forgotten if the user did not confirm pallet scan.
// - Fixed Bug. Clearing just box pick portion of the session, and not the entire session. Clearing entire session is bad and it breaks mobile emulation.
// - Removed obsolete warnings (Sharad 12 Jul 2012)

// Change log from 1.0.4.0 to 1.0.5.0 (Branched by Rajesh Kandari on 18 Aug 2012)
// Rajesh Kandari 8 Aug 2012: Does not use IPluggableArea2. Will require updated version of DcmsMobile.
// Rajesh Kandari 18 Aug 2012: Skipping carton scan when user enters 's'.


// Change log from 1.0.5.0 to 1.0.6.0 (Branched by Ankit Sharma on 9 oct 2012)
// Rajesh Kandari 30 Aug 2012: Added SearchRoute class to make the area searchable.
// Sharad 5 Sep 2012: Upgraded to T4MVC 2.10
// Backend Dependency : Need to remove carton sequence colomn fron pkg_boxpick.get_pallet_info().
// Fixed Bug : BoxPick was considering any Carton as Pallet that contains P in it.


// Change log from 1.0.6.0 to 1.0.7.0 (Branched by HSingh on 23 oct 2012)
// Code refactored. Removed interfaces,commented code and handled Null exceptions in controller and repository.

// Change log from 1.0.7.0 to 1.0.8.0 (Branched by Ankit Sharma on 26 feb 2013)
// Upgraded the T4MVC 2.12 ( Merged from Branch 1.0.7.1 )
// After picking box user will be intimated to take this box to implement VAS on it.
// Backend Dependency : Grant Select on ps_vas to DCMS8_BOXPICK. 
// Backend Dependency : Need to send script of pkg_boxpick.get_pallet_info() 
// Binod 29 Dec 2012: Upgraded to MVC4
// Sharad 9 Jan 2013: Moved scan.html to EditorTemplates folder from Shared folder
// Added new scripts and theme version.

// Change Log 1.0.8.0 -> 1.0.9.0(Branched by Shiva Pandey on 05 june 2013)
// Sharad 27 Feb 2013: Now using Start1.10.0 theme
// Rajesh Kandari 08 May 2013: Bug fixed: Application crashes while scanning empty carton.
// Shiva 05 june 2013 : Removed hardwiring, Number of copies to print was hardwired in the boxpick UCC/CCL labels printing UI.

/* Change Log 1.0.9.0 -> 1.1.0.0(Branched by Shiva Pandey on 14 Nov 2013) (Redmine task #1680)
 * Dependency on PKG_BOXPICK.GET_PALLET_INFO function.
 * Feature : Box Pick should allow picking any carton which is valid for the pallet. 
 */

/* Change Log 1.1.0.0 -> ? (Redmine task #1697)
 * Dependency on PKG_BOXPICK.GET_PALLET_INFO function. It will return new colomn area short name.
 */

[assembly: AssemblyProduct("Allow picking any carton which is valid for the pallet.")]
[assembly: AssemblyVersion("1.1.0.0")]
[assembly: AssemblyFileVersion("1.1.0.0")]
