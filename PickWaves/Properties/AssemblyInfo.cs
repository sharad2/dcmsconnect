using System.Reflection;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Pick Waves")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Eclipse Systems")]
[assembly: AssemblyCopyright("Copyright © Eclipse Systems 2013")]
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

//Change Log from 1.0.0.0 to 1.0.0.1
// - Showing additional info in the ADRE bucket status grid e.g. PO, Total Pickslips, Customer Name & ID as table caption
// - Grouping buckets on the basis of customers and showing them in separate grids.
// - Area filter is required now
// - Check box is given in the first column of the grid to Select/Clear all check boxes

//Change Log from 1.0.0.1 to 1.0.0.2 (tagged by Binod on 23 March 2012)
// - New filter is given for Customer id
// - Provided the links for reports 130.28 and 130.33

//Change Log from 1.0.0.2 to 1.0.1.0 (tagged by Binod on 29 March 2012)
// - Bug Fix: Buckets for which no boxes had been created were not visible in the bucket list. Now they are visible.
// - Added ability to freeze and unfreeze buckets.
// - Provided bucket status filter - Unavailable, in progress and completed.
// - User can manually mark an in progress bucket as complete and vice versa.
// - The UI allows making unavailable buckets available. In progress buckets can be made unavailable.
// - Added report 130.28 url to bucket list to show the shortage of inventory against bucket.
// - Needs updated library. Using new function IOracleDatastore3.ExecuteDml().
// - More work has been done for CreateBuckets functionality. This functionality is still experimental.
// - Box creation is potentially a time consuming task. This is now performed in the background and the UI is not blocked.
// - AdrExclusive list now references area short names instead of exposing area ids.

//Change Log from 1.0.1.0 to 1.0.1.1 (tagged by Binod on 30 March 2012)
// - Updated ReadMe.txt file which contains application features
// - Hide the CreateWaves link
// - Added the title on pages

// Change Log from 1.0.1.1 to 1.0.2.0 (tagged by Binod on 2 April 2012)
// - 30 Mar 2012 Sharad: All buckets of the passed status are displayed. Earlier we were showing only those buckets for which
//   boxes needed to be created.
// - Now showing 'Box Expedited' column in the grid which shows info in format of histogram bars,
//   which represents the info in term of Expedited/Not Expedited and boxes picked which are expedited
// - Non Expedited boxes are linked to the 'Box Pick Pallet' UI where user can expedite box for that wave [UI will be fully functional soon..]
// - Showing pickslip instead of pickslip count in the grid, count will be shown as tooltip.
// - Given the link for Quick Start on the top of the filters, which is a precise User manual for this UI
// - Enhanced the UI and messages which were earlier shown for background process of 'Recreate Boxes'
// - Internal enhancements of queries for different processes of Manage Waves
// - Removed the link from purchase order column, becoz it was creating a garbage link due to MinPurchaseOrder/CustomerId/MinIteration
// - Read only info is now available on 'Box Pick Pallet' UI.

// Change Log from 1.0.2.0 to 1.1.0.0 (Tagged by Binod on 17 April 2012)
// - Home page displays imported order summary by customer.
// - Manage Pick Wave Enhancements
//   * Added ability to adjust bucket priorities. Pallet Locating will suggest SKUs of figher priority buckets first (requires updated Pallet Locating).
//   * Progress bar legend provided to help explain how to read the progress bar.
//   * Wave Completion date is displayed for completed buckets and box creation date displayed for In progress buckets.
//   * UI treats unavailable buckets as Inprogress buckets
// - Create Box Pick Pallet. functionality is now included. The functionality provided by DCMS Web is obsolete at this point.
//   * You can expedite Boxes to a new pallet or to an existing pallet.
//   * Pallet printing (implemented by Inquiry) allows printing of full pallet, unprinted cartons, or specific cartons.
//   * Unpicked boxes can be removed from pallet so that they become available for new pallets.
//   * The pick wave to expedite is automatically suggested keeping bucket priority in mind.
//   * Frozen buckets cannot be expedited.
// - Security Enhancements. Readonly access is available to everyone. Manage buttons will be available only if user has DCMS8_SELECTPO role.
//   * DCMS8_SELECTPO role now includes the ability to create box pick pallets.

// Change Log from 1.1.0.0 to 1.1.0.1 (Tagged by Binod on 20 April 2012)
// Critical Bug Fix. Not generating action link when box count is null.

// Change Log from 1.1.0.1 to 1.1.1.0 (Tagged by Binod on 23 April 2012)
// In Manage ADR Exclusive Pick Waves screen the option to expedite the pallet is visible for all conditions.
// Showing status massage when priority is successfully changed
// Mark Complete tasks will be performed in background. Clock Icon will be shown with title for schedule messaage in place of checkbox to select that bucket till it'll be completed.

// Change Log from 1.1.1.0 to 1.2.0.0
// This version of Pick waves will be further compatible with newer version of EclipseLibrary.Oracle 7.0.5.0 [Dependency]
// Wave creation feature is implemented in this version
// Many UI enhancements
// If bucket having multiple POs,then it will also show PO Count in tip of PO.
// Showing Bucket Creation Date to identify the last created waves
// Showing CUSTOMER_DIST_CENTER_ID previously it was showing empty column
// Given the login link on manage pick wave page 
// Bug Fixed : GetPickslips() Query was showing wrong list of pickslip
// Bug Fixed in Expedite Box Picking: Now it will only take pallet of its own bucket or any new pallet
// Bug Fixed: when Recreate Boxes will be attempted then bucket will be mark available. i.e. Status will be set to READYTOPULL and available for pitching flag will be set to 'Y'
// Bug Fix (Sharad 21 May 2012) Bucket summary on home page was showing pull to dock buckets incorrectly.

// Change Log from 1.2.0.0 to 1.2.1.0 (Branch created by Binod on 13 June 2012)
// Bug Fix (Sharad 21 May 2012) Bucket summary on home page was showing pull to dock buckets incorrectly.
// Bug fixed: Work around code for T4MVC. T4MVC is unable to generate reasonable action links when the value being passed in NULL.
// Now we are accepting NULL value as selected dimension value to populate pickslip list.
// Given the Login link on Create Box pick pallet index page
// (Sharad 23 may 2012) Changes on Branch 1.2.0.1 merged to trunk
//   -- Bug Fixed: Now Box Expediting will require the 'DCMS8_CREATEBPP' role
//   -- Properly handling null dimensions
// Improved the UI of Create Box Pick Pallet
// Showing Customer and Building on the Index page of Create Box Pick Pallet
// From now Create Box Pick Pallet will dependent on PKG_BOXEXPEDITE.CREATE_PALLET_2 function [Dependency]


// Change log from 1.2.1.0 to 2.0.0.0
// Rajesh Kandari 8 Aug 2012: Does not use IPluggableArea2. Will require updated version of DcmsMobile.
// Sharad 13 Aug 2012: Manage PickWaves Grid on home page is now Excluding buckets for which all pickslips have been transferred.
//    This reduces the number of in progress buckets and makes it more accurate.
//    - AdrExclusiveManage: Link to expedite does not pass a pick wave id. This causes the pick wave to get automatically selected
// Shiva 22 Aug 2012: 
// - Manage Pick Waves Grid on home page is manage bucket count per customer and pick mode.
// - Area, Pick mode and customer filter are remove from manage UI.
// - Dependency on EclipseLibrary.Oracle 
//      : EnumMapper made more robust. It only maps OracleString and OracleDecimal to Enum.
//      : Use OracleDataRow2 replace of OracleDataRow.
//      : Use new SqlBinder.
// Sharad 28 Aug 2012:
//  Home screen displays pick waves by customer, and provides customer filter
// Rajesh Kandari 30 Aug 2012: Added SearchRoute class to make the area searchable.
// Sharad 4 Sep 2012: Working on changing bucket pick mode. Upgraded to T4MVC 2.10.
// Sharad 27 Nov 2012: Upgraded to .NET 4.5. Upgraded to MVC 4
// Binod 29 Dec 2012: Degraded to .NET 4.0
// Rajesh Kandari 1 Feb 2013: Provided Pitching Box Creation Configuration Ui.
// Depends on new PKG_BUCKET.RECREATE_BOXES_FOR_BUCKET  [DEPENDENCY]
// Depends on bug fix of SelectPO against task id : 1252
// Shiva 18 April 2013 : Depending on IA.SHORT_NAME and BUILDINGS of area. [DEPENDENCY]
// Shiva 28 April 2013 : Depending on Bucket.Pull_Type(add new column in bucket table).
// Shiva 16 May 2013 : Depending on Bucket.CREATED_BY_MODULE(add new column in bucket table).
// Shiva 12 Aug 2013 : Depending on Bucket.SHIP_IA_ID(Default value 'ADR')
// Shiva 12 Aug 2013 : Depending on Bucket.AVAILABLE_FOR_PITCHING = 'Y'(Default value is Y) and 
// Binod 29 Aug 2013 : New Role DCMS8_WAVE_MANAGER is required to run this module.
// Binod 2 Sept 2013 : New Pick Wave Manager helps the user to create/manage pick wave in efficient manner. 
//					   Now this module create a wave, which can be processed in both way pulling or pitching and both at simultaneously.
//					   A wave can be processed from more than one areas/buildings when inventory is available.
//					   A new Pick wave configuration UI is introduced in this version.


/* Change log from 2.0.0.0 to 2.0.1.0 (Branch created by Shiva on 15 May 2014) (Redmine task : #1803)
 *  [DEPENDENCY] Depending on IA.SHORT_NAME and BUILDINGS of area.
 *  [DEPENDENCY] Depending on Bucket.CREATED_BY_MODULE(add new column in bucket table).
 *  [DEPENDENCY] Depending on Bucket.SHIP_IA_ID(Default value 'ADR').
 *  [DEPENDENCY] Depending on Bucket.AVAILABLE_FOR_PITCHING = 'Y'(Default value is Y)
 *  New Role DCMS8_WAVE_MANAGER is required to run this module.
 *  New Pick Wave Manager helps the user to create/manage pick wave in efficient manner. 
 *  Now this module create a wave, which can be processed in both way pulling or pitching and both at simultaneously.
 *  A wave can be processed from more than one areas/buildings when inventory is available.
 *  [DEPENDENCY] Depends on PKG_BUCKET
 *  [DEPENDENCY] Depends on PKG_BOXEXPEDITE.CREATE_ADR_PALLET
 *  [DEPENDENCY] Depends on new trigger TRG_MIG_BUCKET_FREEZE_BIUR
 */

[assembly: AssemblyVersion("2.0.1.0")]
[assembly: AssemblyFileVersion("2.0.1.0")]

[assembly: AssemblyProduct(@"
<ol>
<li>
New Pick Wave Manager helps the user to create/manage pick wave in efficient manner.
</li>
<li>
Now this module create a wave, which can be processed in both way pulling, pitching or both at simultaneously.
</li>
<li>
A wave can be processed from more than one areas/buildings when inventory is available.
</li>
</ol>
")]