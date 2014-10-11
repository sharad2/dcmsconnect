using System.Reflection;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("CartonManager")]
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

//Change log from 1.0.0.3 to 1.0.1.0
//	Price season code can be changed now.
//	Conversion screen got rid of pre-conversion quality. Now you can upgrade quality of any downgraded carton. 
//	Conversion screen is no longer restricted to conversion area. Now it can work in any area. 

//Change log from 1.0.1.0 to 1.0.2.0
//Changed the Display Name of this module from CartonManager to Carton Editor
//Role changed to use the CartonEditor application, new role is SRC_CED in place of SRC_CONVERSION

// Change log from 1.0.2.0 to 1.0.3.0(Branch created by Ritesh Verma on 29th May 2012)  
// Changes in Advance UI:-Carton can be qualified by looking at status of remark work needed.e.g cartons marked for rework
// or cartons don't require rework or status doesn't matter.
// - Update rework status to complete or undo rework.
// New UI(C2P) to palletize those cartons which do not neeed rework. 
// New UI(MarkReworkComplete) to mark rework status of carton complete. Palletization is also possible.
// UI(AbandonRework) abondon rework on those cartons which require rework.Palletization of such cartons is also possible. 
// Sharad 22 May 2012: C2P UI now requires SRC_C2P role instead of SRC_CED role


//Change log from 1.0.3.0 to to 1.0.4.0 (Branch created by Rajesh kandari on 05 june 2012)
//Mobile UI provided for following:-
//  1.C2P- Carton to Pallet, palletize those cartons which do not need rework.
//  2.Mark Rework Complete, to mark rework status of carton complete. 
//  3.Abandon Rework, Scan Cartons or Pallet requiring rework to abandon it.
//  4.Dependency on EclipseLibrary.MVC.


//Change log from 1.0.4.0 to 1.0.5.0 (Branch created by Ritesh Verma on 20 June 2012) 
// Check box to remove carton from existing Pallet provided in Advance ui.
// Advance UI now supports updation of location only.
// Single carton editor provided.
// Single Carton editor requires role SRC_CED

//Change log from 1.0.5.0 to 1.0.6.0(Branch created by Ritesh Verma on 29th June 2012)
// Request in the carton is set to null once carton is edited.
// Showing reservation information in the carton editor.
// Update properties refreshed once carton is updated in CED.
// Tabs for Area,Pallet and Carton/Pallet provided in C2P mobile UI.

// Change log from 1.0.6.0 to 1.0.7.0 (Branch created by Rajesh Kandari on 21st July 2012)
// CED is single screen UI now.
// Provided feature to remove irregular and samples in CED.
// Fixed issue :Loss of focus in C2P,Mark Rework Comlete and Abandon Rework UI.
// Dependency on EclipseLibrary.MVC(version 2.1.5.0)
// Following Application must be send before sending EclipseLibrary.MVC(version 2.1.5.0)
// 1. CartonAreas version 1.0.2.0


// Change log from 1.0.7.0 to 1.0.8.0 (Branched by Rajesh Kandari on 04 Aug 2012) 
// Provided Ui for Locating.
// Locating requires role SRC_LOCATING
// We are providing Module Code and Action Code hardwired in query.
// Make Productivity_Id column nullable in CARTON_PRODUCTIVITY table.This is the prerequisite for deploying this release.


// Change log from 1.0.8.0 to 1.0.9.0 (Branched by Ritesh verma on 18 Aug 2012)
// Changed sound files, now we use .mp3 files for sound.
// Sharad 13 Aug 2012: Removed IPluggableArea2
// Sharad 14 Aug 2012: CED. When customer bar code is entered, the SKU description is cleared. It is populated again when the carton is updated.
// Binod  14 Aug 2012: Locating. New feature is given to ask the Pallet on Locating UI so that user can scan the cartons of that pallet 
//                     and remaining cartons which are not scanned will be marked in suspense.
// Binod 14 Aug 2012: Carton Manager. Now CaptureProductivity() will update the WAREHOUSE_LOCATION_ID in CARTON_PRODUCTIVITY table from MASTER_STORAGE_LOCATION instead of TAB_INVENTORY_AREA.
// Ritesh 17 Aug 2012: CED.New feature to edit pallet.

// Change log from 1.0.9.0 to 1.0.10.0 (Branched by Ritesh verma on 22 Aug 2012)
// Not using deleted column REMARK_WORK_NEEDED.
// CED. When customer bar code is entered, the SKU description is cleared. It is populated again when the carton is updated.
// Locating. New feature is given to ask the Pallet on Locating UI so that user can scan the cartons of that pallet 
//                     and remaining cartons which are not scanned will be marked in suspense.
// Carton Manager.Update WAREHOUSE_LOCATION_ID in CARTON_PRODUCTIVITY table from MASTER_STORAGE_LOCATION instead of TAB_INVENTORY_AREA while capturing productivity.
// CED.New feature to edit pallet.

// Change log from 1.0.10.0 to 1.0.12.0 (Branched by Rajesh Kandari on 27 Aug 2012)
// Fixed minor bug in Locating UI: The option to skip pallet was not evident in the UI. Now we give it as a tip in the scan Pallet screen.
// Locating:UI can accept pallet scan after locating non pallet cartons.

// Change log from 1.0.12.0 to 1.0.12.1 (Tagged By Rajesh Kandari on 13 Sep 2012)
// Fixed performance issue.
// When the carton is scanned we do not ask for confirmation if the carton does not belong to the scanned pallet.

// Change log from 1.0.12.1 -> 1.0.13.0(tagged by dpanwar on 21 sep 2012)
// Sharad 29 Aug 2012: Added SearchRoute class to make the area searchable
// Sharad 5 Sep 2012: Upgraded to T4MVC 2.10
// Dependency on EclipseLibrary.Oracle 
//          : Use new SqlBinder.
// 
// Improved performance of locating. Now location will be validated after carton is scanned. 
// Now we do not ask for pallet reconfirmation to speed up locating.

// Change log from 1.0.13.0 -> 1.0.14.0( tagged by dpanwar on 1st oct 2012)
// Sharad 26 Sep 2012: Location carton count is displayed. Carton count on pallet is more accurate. Honoring master_storage_location.assigned_max_cartons.
// Locating: If you locate multiple cartons on the same location you now need to scan the location only once.
// Locating: Now we give you a warning if number of cartons at location exceed its capacity.

// Change log from 1.0.14.0 to 1.0.15.0(Branched by Ankit Sharma on 26 oct 2012.)
// Now we are only executing an update query in MarkCartonInSuspense method in locating repository instead of using a plsql block which were also selecting no. of records for the passed pallet. 
// Removed commented code.
// Removed warnings.
// Removed AutoMapper.
// Ankit : I have seen Rajesh has edited Oracle exception number in LocatingRepository for LocateCarton() and HandleScan() on LocatingController


// Change log from 1.0.15.0 to 1.0.16.0(Branched by Rajesh Kandari 0n 30 Oct 2012)
// Rearrange the list of subareas in CartonManager.

//change log from 1.0.16.0 to 1.0.17.0 (Branched by Binod Kumar 08 jan 2013)
// Added maxlength on all texboxes of all UI where it is required
// Binod 29 Dec 2012: Upgraded to MVC4
// Binod 7 Jan 2013: Removed obsolete HandleAjaxErrorAttribute

// Change Log 1.0.17.0 -> 1.0.18.0 (Branched By Rajesh Kandari on 14 Jun 2013)
// Sharad 15 Mar 2013: Upgraded to jquery UI 1.10. Need to test.
// Rajesh Kandari 21 May 2013: C2P(Mobile screen)-> Supported short name in mobile version of C2P.  Now you need to scan building to palletize cartons.
// Showing short name instead of areaid.

// Change Log 1.0.18.0 -> 1.0.19.0 (Branched By Binay Bhushan on 02 july 2013)
//  Removed length constraint of 11 characters while scanning carton 

// Change log from 1.0.19.0 -> 1.0.20.0 (Branched by Rajesh Kandari on 07 Aug 2013) (Abandoned in favor of next version)
// Bug fixed: Now user can print cartons ticket of a pallet in Update Cartons-Advanced UI.


// Change log from 1.0.20.0 -> 1.1.0.0 (Branched by Shiva Pandey on 05 Oct 2013)(Redmine Deployment Task# 1591)
// Sharad 20 Sep 2013. Provided option on index page to emulate mobile. Removed inclusion of unnecessary scripts from mobile pages.
// Providing a text area for scanning cartons. Postback of multiple cartons occurs when user pauses. This should provide major performance improvement. Cartons should never get lost.
// Removed library warnings for BindUpperCase and AUtoComplete
// Avoided location query on each carton scan by caching pallet cartons properly.
// Upgraded to jquery 2.0.3 for IE > 8
// Turned off backward compatibility in jquery ui
// Sharad 30 Sep 2013: Locating does not raise exception if invalid carton is scanned

/* Change Log 1.1.0.0 -> 1.1.1.0 (Branched by Shiva Pandey on 12 Nov 2013)(Redmine Deployment Task# 1678)
 *  Shiva 2 Nov 2013 : Trying to handle the situation of POSTING after login has expired.
 *  Shiva 15 Nov 2013 : Showing ShortName of area in Locating UI
*/

/* Change Log 1.1.1.0 -> 1.1.2.0 (Branched by Shiva Pandey on 12 Nov 2013) (Redmine Deployment Task# 1682)
 *  Shiva 21 Nov 2013 : Now Locating will warn user if the carton being located is marked for rework.
*/

[assembly: AssemblyProduct(@"
<ol>
<li>
Feature : Now Locating will warn user if the carton being located is marked for rework.
</li>
</ol>
")]

[assembly: AssemblyVersion("1.1.2.0")]
[assembly: AssemblyFileVersion("1.1.2.0")]


