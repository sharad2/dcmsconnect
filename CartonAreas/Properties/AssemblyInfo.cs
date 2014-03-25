using System.Reflection;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("CartonAreas")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Eclipse Systems")]
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
 * Change log from 1.0.0.0 to 1.0.0.1:
 *  - Added compatibility mode meta tag to _layOutCartonAreas
 *
 * Change log from 1.0.0.1 to 1.0.0.2:
 *  - Pallet count was wrong. Changed COUNT(SC.PALLET_ID) to COUNT(DISTINCT SC.PALLET_ID)
 * */

/*
 * Change log from 1.0.0.2 to 1.0.1.0 :  [ BK ]
 *  - Now the summary info of assigned location count for the area on Manage Carton Area page 
 *    is being refreshed at every new assignement in the Locations grid.
 *  - Some UI fixes
 *  - Removed the unversioned folders which were not necessary like Content and Scripts
 *  - Changed the Externals to copy the folders Scripts and Content from svn repos.
 * */

/*
 * Change log from 1.0.1.0 to 1.0.1.1 :  [ Sharad 23 Jan 2012 ]
 *  - Assigned/Unassigned matrix displays numbers right alighned
 *  - The clicked number is highlighted.
 * 
 *  Change log from 1.0.1.0 to 1.1.0.0 :  [ Shiva 14 Feb 2012 ]
 *  - Carton area flags and description are now editable.
 *  - Assignment update dialog now displays current value of max cartons.
 */
// Change Log from 1.1.0.0 to 1.1.1.0(Branch created by Rajesh kandari on 23 july 2012)
// Removed pluggable area warning
// Removed unused code
// Fixed bug :Handling argument exception for preventing the query from update more than one row in repos and service ref task 605


// Change log from 1.1.1.0 to 1.1.2.0(Branched by Rajesh kandari)
// Rajesh Kandari 8 Aug 2012: Does not use IPluggableArea2. Will require updated version of DcmsMobile.
// Use IOracleDatastore3 in place of IOracleDatastore2
// Rajesh Kandari 30 Aug 2012: Added SearchRoute class to make the area searchable.
// Now we use OracleDatastore in place of IOracleDatastore3.
// Sharad 5 Sep 2012: Upgraded to T4MVC 2.10
// You can now increase or decrease capacity(cartons) at a location without providing SKU and VWH.
// You have the flexibility to change assignment at a location which already has cartons in it. 
// Showing SKU of carton in Manage Carton Area page.

// Change log from 1.1.2.0 to 1.1.3.0(Branched by Ankit Sharma on 25 oct 2012.)
// Code refactored. Removed commented code and handled Null exceptions in repository.

// Change log from 1.1.3.0 to 1.1.4.0(Branched By Ankit Sharma on 4 jan 2013)
// Binod 29 Dec 2012: Upgraded to MVC4

// 1.1.4.0 -> ?
// Sharad 7 Jan 2013: Javascript for UpdateLocation() updated. In case of unhandled exception, it will display an alert before displaying the error as a validation error.
// Sharad 15 Mar 2013: Upgraded to jquery UI 1.10
//
[assembly: AssemblyVersion("1.1.4.0")]
[assembly: AssemblyFileVersion("1.1.4.0")]
[assembly: AssemblyProduct("1: You can now increase or decrease capacity(cartons) at a location without providing SKU and VWH. 2: You have the flexibility to change assignment at a location which already has cartons in it. 3: Showing SKU of carton in Manage Carton Area page")]
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("CamRepositoryTest")]
