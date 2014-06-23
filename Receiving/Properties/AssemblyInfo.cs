using System.Reflection;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("DcmsMobile.Receiving")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Eclipse Systems Private Limited")]
[assembly: AssemblyCopyright("Copyright © Eclipse Systems 2012")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("344557ed-8e13-4a06-8013-becf544142de")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Revision and Build Numbers 
// by using the '*' as shown below:

// Change Log from 1.0.0.0 to 1.0.7.0

// Improved the performance of the query retrieving the processes in the main screen. 
// We are now sending SKU and UPC in email alert feature. 
// Using short name for display
// Improvements in Edit Process page. 
// Mobile view was not running. Made it workable. 
// Minor UI improvements

//change log from 1.0.7.0 to 1.0.8.0
//We will show carton count and pallet count from the table src_carton in GetProcess function in repository.

//change log from 1.0.8.0 to 1.0.8.1 (Sharad 18 Jan 2012)
// * Fixed bug. Style specific spot check percent was not being honored.


//* Changing Log 1.0.8.1 -> 1.0.9.0
//Documentation updated.
//Receiving Area list and spot check area list modified.
//Receiving area Id and spot check area id saved to cookie.
//Receiving area Id and spot check area id list show "Select Any Area"  as default value. 
//RAD can't change receiving area and spot check area.Removing receiving area and spot check area list from RAD.
//If no building is assigned to any area show default value "Multiple Bldg" against area.
//Now we wiil not considering old process id. 

// Change Log 1.0.9.0 -> 1.0.9.1 (Sharad 21 Jan 2012)
// Displaying area description in area combos.
// Minor style optimizations.

// Change Log 1.0.9.1 -> 1.0.10.0
// Showing error when duplicate carton is scanned
// Provided link for Report 40.103 to show the shipment and cartons detail against passed processId
// From now when carton/Pallet will be scanned the cursor will not leave the text box.
// Reverse numbering is used to show the sequence of each carton in the carton list.
// providing prominent style to the total carton count in grid
// Sidebar styling issues are fixed
// Added missing proxy tag
// Received cartons count is being shown as progress bar on recent processes page.
// Code reviewed

// Change Log 1.0.10.0 -> 1.0.10.1 (sharad 31 Jan 2012)
// Fixed bug in handle scan javascript

 //Change Log 1.0.10.1 -> 1.2.0.0 (Tagged By Rajesh on 23 Apr 2012)
 //If Receiver scans cartons faster than system receives them, any scan will not be lost, we will take care of them sequentially
 //All pending scans are being shown in dialog box along with last unsuccessful scan errors. This will allow a consolidated view of cartons which were not received successfully.
 //Improved the code of receiving for better performance
 //Added new Roles.txt file
 //Showing received cartons sequence in reverse order to show total no. of cartons received so far in Mobile screen
 //Showing whether the pallet is single sku pallet or a mixed pallet.
 //Added pallet limit at process level.
 //Receiving allows setting price season code at process level.
 //Removed pallet limit and disposition concept from RAD.
 //While editing process, receiving area and spot check area are updateable now.
 //Improved queries used during receiving.
 //We do not support label group concept
 //We try to send scanned carton to assigned forward pick area if it is needed there provided FIFO is not violated.
 //We do not use src_carton_process_detail in our queries now. 
 //New process is created from separate page now.
 //Updated document
 //Changed the AssemblyCopyright info

//Change Log 1.2.0.0 -> 2.0.0.0 (Tagged By Binod on 4 May 2012)
// Changed the build action of content files to none.
// Sharad 2 May 2012: Added link to Deepak's presentation

// Change Log 2.0.0.0 -> 2.0.1.0 (Branched By: Deepak Bhatt on 26-06-2012)
// In the Receiving progress dialog, error messages will be displayed in reverse order. They will also be time stamped.
// Bug fixed: In case of disposition mismatch we will suggest the existing pallet of same disposition if already opened.
// Minor UI styling improved.
// Playing new sound when Pallet is Changed.
// Showing overall receiving progress bar on Receiving page now.
// When a carton has already been received we give warning sound instead of an error sound and we also give the pallet on which carton was put.
// Now we show receiving start date and elapsed time in Recent processes page.
// Receiving Supports setting spot check at style and color level.
// Now we get PalletDisposition from database
// Removed unused code 
// Bug Fix: When a process is edited we showed the value from cookie. Now we have fixed it and show actual value from src_carton_process.
// Removed alert email checkbox.

// Change Log 2.0.1.0 to 2.0.2.0 (Branched by Binay Bhushan on 4 Aug 2012)
// Playing fixed numeric sounds when pallet is changed.
// We receive carton as soon as it is scanned even if there is a disposition mismatch but do not keep them on any pallet. 
// We always show received but unpalletized cartons in UI.
// Now we depend on package dcms8.PKG_REC_2
// Progress bar will show receiving progress based on palletized cartons.

// Change log from 2.0.2.0 2.0.3.0 (Branched by Rajesh Kandari on 27 Sep 2012)
// Rajesh Kandari 8 Aug 2012: Does not use IPluggableArea2. Will require updated version of DcmsMobile.
// Rajesh Kandari 30 Aug 2012: Added SearchRoute class to make the area searchable.
// Now we use OracleDatastore in place of IoracleDataStore3.
// Sharad 5 Sep 2012: Upgraded to T4MVC 2.10
// Hemant 20 sep 2012:  Dependency on EclipseLibrary.Oracle 
//                   :  Use new SqlBinder.
// Removed AutoMapper from Receiving application.
// Showing Retail Price of SKU in received Carton list.
// Removed Email-Alert feature from receiving.
// Backend Changes: Does not need ALERT_EMAIL_ADDRESS and AREA_ID_TO_CHECK columns in SRC_CARTON_PROCESS table.


// Change log from 2.0.3.0 to 2.0.4.0(Branched by Rajesh Kandari on 29 sep 2012)
// The option to unreceive carton has been removed. Now you can remove carton from pallet.

// Change log from 2.0.4.0 to 2.0.5.0(Branched by Binod Kumar on 8 Jan 2013)
// Binod 31 Dec 2012: Upgraded to MVC4
// Sharad 7 Jan 2013: Removed obsolete HandleAjaxErrorAttribute


// Change log from 2.0.5.0 to 2.0.6.0 (Branched by Rajesh Kandari on 25 Jan 2013)
// Handling Divide by Zero exception raised when expected cartons of a process is null.

//Change Log from 2.0.6.0 to 2.0.7.0 (Branched by Rajesh Kandari on 19 June 2013)
// RAD
// 1. User can select All option for Sewing plant.
// 2. Color is also part of spot check configuration.
// 3. All option for style.
// 4. Showing available spot check area in each building.
// 5. One can enable/disable spot check configuration.
// 6: Validating style and color.
// Receiving
// 1.Receiving honors spotcheck enable flag set by RAD.
// 2.Honoring IsReceving_area and IsSpotCheckArea flags introduced in tab_inventory_area while populating drop down in create process UI.
// 3.Showing top 20 processes.
// 4.Dependency on Jquery UI 1.10.0 
// Backend
// 1. Introduced SPOTCHECK_FLAG column in master_sewingplant_style table
// 2. Introduced IS_RECEIVING_AREA and IS_SPOTCHECK_AREA in tab_inventory_area table.
// 3. grant select on TAB_INVENTORY_AREA to SRC_RECEIVING_MGR;
// 4. grant select on TAB_COUNTRY to SRC_RECEIVING_MGR;
// 5. grant select on MASTER_STYLE to SRC_RECEIVING_MGR;
// 6. grant select on MASTER_COLOR to SRC_RECEIVING_MGR;

//Change Log from 2.0.7.0 to 2.0.8.0 (Branched by Ritesh verma on 3rd July 2013)
// 1. Removed carton length constraint


//Change Log from branch version 2.0.8.0 to 2.0.9.0 (Branched by Ankit Sharma on 4 Jan 2013)
// 1. Introduced new feature for closing shipments in Receving.This feature have dependency of column SRC_CARTON_INTRANSIT.IS_SHIPMENT_CLOSED.

//Change Log from branch version 2.0.9.0 to 2.0.9.9010(Branced by Anil Panwar on 16 Jan 2014)
// Added Expected and Received Quantity in Open Shipment List

//Change Log from branch version 2.0.9.9010 to 2.0.10.0(Branced by Ankit Sharma on 29 Jan 2014)
//1. ASHARMA: Added column Carton "Expected" and "Received" Cartons in The close shipment UI.
//2. HSINGH : #1734: supporting 11 digit source_order_id in receiving
//3. ASHARMA:Link to Inquiry for showing SKU level detail for each shipment in open Shipment UI.
//4. ASHARMA Dependencies : => (i) Receving is calling functions close_shipemnt and reopen_shipemnt of pkg_rec_2 so we should have latest pkg_rec_2 compiled.
//                  => (ii) We should have lastest pkg_sap_dcms_ctn_asn as it will popullated original shipping_id.   
//                  => (iii) We must have column original_shipment_id_2 in src_carton_intransit. 

// Chnage Log from branch version 2.0.10.0 -> 2.0.10.9010 (Branced by  Ankit Sharma  on 30 Jan 2014)
// 1.Fixed Bug:  shipmentlist query is now getting data partitioned by shipment and PO.


/*
// Chnage Log from branch version 2.0.10.9010 -> 2.0.10.9020 (Branched by Ankit Sharma on 1 FEB 2013)
 * Added Process number in open shipment list.
*/


/*
// Chnage Log from branch version 2.0.10.9020 -> 2.0.10.9030 (Branched by Ankit Sharma on 5 FEB 2013)
 // 1.Fixed Bug:  Redefined joining condition of src_carton_intransit and src_carton in GetShipmentList query.
*/


/*
// Chnage Log from branch version 2.0.10.9030 -> 2.0.10.9040 (Branched by Hemant K. Singh on 7 FEB 2014)
 // 1.Fixed Bug:  Passing PO as null when it is not coming.
*/

/*
// Chnage Log from branch version 2.0.10.9040 -> 2.0.10.9050 (Branched by Ankit Sharma on 14 FEB 2014)
 // 1.Fixed Bug:  We were including inventory of overrage cartons in shipment list on openshipment UI.
*/

/*
// Chnage Log from branch version 2.0.10.9050 -> 2.0.10.9060 (Branched by Ankit Sharma on 18 FEB 2014)
 // 1.Fixed Bug: We will accept a closed shipment carton only if any relevant shipment is open.
*/

/* Change Log version 2.0.10.9060 -> 2.0.11.0 (Branched by Shiva Pandey on 26 FEB 2014)(Redmine Task : 1734)
 *  Giving better message when a closed shipment carton is scanned first up.
 */

/* Change Log version 2.0.11.0 -> 2.0.12.0 (Branched by Shiva Pandey on 06 Mar 2014)(Redmine Task : 1782)
 *  Shiva : Fixed Bug: Now we add opened cartons also in received cartons.
 *  Shiva : Grant select role to SRC_OPEN_CARTON to SRC_RECEIVING.
 */


/* Change Log version 2.0.12.0 -> 2.0.13.0 (Branched by Anil Panwar on 24 Apr 2014)
 *  Minor Bug Fixed: When a garbage carton is scanned on Receiving Page. The code was throwing null reference exception. 
 *  The src_open_carton join in Recent Process List query was slowing down the query so we are now using an inline query to get number of cartons for a process in src_open_carton
*/

[assembly: AssemblyVersion("2.0.13.0")]
[assembly: AssemblyFileVersion("2.0.13.0")]
[assembly: AssemblyProduct(
@"
<ol>
<b>Receiving</b>
<li>
    When a garbage carton is scanned on Receiving Page. The code was throwing null reference exception has been fixed. 
</li>                          
</ol>")]

//<!--$Id$-->
