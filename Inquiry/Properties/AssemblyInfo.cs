using System.Reflection;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Inquiry")]
[assembly: AssemblyDescription("Provides information about anything that is scanned")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Eclipse Systems Private Limited")]
[assembly: AssemblyCopyright("Copyright © Eclipse 2012")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("AA16FB2A-19BF-43F4-B779-5116F3514227")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Revision and Build Numbers 
// by using the '*' as shown below:

//* Change Log from 1.1.0.0 to 1.1.1.0
//*  * Carton Inquiry screen is now showing carton history also.
//*  * Location Inquiry screen is now showing list of pallets which are there on the scanned location along with unpallatized cartons on that location.
//*  * SKU Inquiry is shwoing top location in each area where the SKU can be found.
//*  * 



//change log from 1.1.1.0 to 1.1.2.0
//Carton process history for recently repacked cartons are available now.
//changes made in query for carton's process list

//change log from 1.1.2.0 to 1.1.2.1
// Added support for UPS tracking number.

//change log from 1.1.2.1 to 1.1.3.0
//* Archive Box query was not working correctly. This bug has been fixed.
//* Bug fixed in EPC scan which was occurred after applied the patch 1.1.2.1
//* Pro number has been added to the box model and displaying it for each box.
//* Scan against the Tracking number in dem_box and dem_box_h tables has been implemented.
//* Fixed ambiguity in earlier tracking number scan.
//* Intransit Carton Shipment scan has been implemented.

// 1.1.3.0 -> 1.1.3.1 (Sharad 17 Jan 2012)
// * Cosmetic UI changes
// * Renamed enum Shipment to InboundShipment. This is to distinguish it from OutboundShipment which we will be supporting soon.
// * Carton history now shows date. How could you have forgotten this !
// * Created index on dem_box.pro_number in the database
// * Suppressed searching in the history tables. Searching in history tables is a time consuming operation due to lack of indexes.
//   I certainly don't want to create too many indexes in the history tables. I would like a UI which pops up after an unsuccesful
//   scan. This UI will ask the user whether he would like to look through history tables as well. If the user so chooses, then
//   the history tables hould be parsed. Please call me to discuss this idea further.

// 1.1.3.1 -> 1.1.5.0
//Added information about intransit id and style on shipment scan
//Added Shipment scan example on Index page
//Removed commented code fron DetailRepository
//Added links in bucket page.
// Added a seperate mobile UI for bucket scan.

// 1.1.5.0 -> 1.1.5.1 (Ankit 18 Feb 2012) (In progress)
// - Supporting intransit carton scan
// - Outbound shipment scan supported
// - Box page: Added last UCC printed by,last UCC printed Date,last CCL printed date and last CCL printed by.
// - Bucket page: Added description to report links. Links are hidden for completed buckets.
// - Box Pallet Page: Now displaying boxes on pallet also.
// - Carton Pallet: Added last pulled date.
// - Style: Added country of origin and deleted Label Group information.
// - Home page: Included bullet for inbound shipment, intransit carton ,outbound shipment and Tracking Number.
// - Carton Page: Added Shippment Id and Shipment Date Information on carton page.
// - Carton Location Page: Added Links for pallet and cartons in Location content grid.
// - Label Page: Deleted Restock Carton Limited. 
// - Removed IALOC_CONTENT.SKU_ID from GetSkuAreaInventoryForSku() function of repository because it is not being populated currently,
//   used UPC_CODE instead.
// - InboundShipment Scan is now IntransitShipment.


//1.1.5.1 -> 1.1.5.2 (Tagged on 31 March 2012 by Deepak)
// - All the Models, Views and scantypes that were previously called as InboundShipment and Tracking is now called as IntransitShipment and TrackingNumber.
// - Removed hardwiring of _layoutInquiry from _ViewStart.cshtml.
// - Povided autocomplete for UPC scan on index page.
// - Wave page : Added export flag information.
// - Pickslip page : Added bucket link and picking status on Pickslip Page.
// - OutboundShipment page : 1) Supporting scan for both shipping_id and parent_shipping_id.                           
//2) Added both To and From address of shippment                           
//3) Added information for customer DC Id, Onhold flag ,master bol Id.                           
//4) Added summary of shipment like number of pickslips,buckets and boxes against shipment                           
//5) Added problematic suggestion means those pickslips and boxes which are restricting the shipment.           
// - Carton page : Added link for style.Added condition of showing shipment of carton
// - CartonArea page : Added label link.
// - CartonPallet page : added Location link.
// - SKU page : Added style link.
// - Wave Page : Added link for report R110.07
// - Customer page : Added condition on showing customer documents.
// - Box page : 1) Added suspense date,rejection code and information  box is at door or not.
//              2) Added customer dc id ,store id and box's shipping address.
//              3) Added information about who picked the box.

// 1.1.5.2 -> 1.1.6.0 (Tagged on 17 April by Ankit Sharma)
// - Sharad 3 Apr 2012: Pickslip shows max 1000 boxes. Earlier it was crashing when more than 1000 boxes were encountered.
// - Sharad 3 Apr 2012: Improved cosmetic of pickslip page
// - Box page : 1) Added feature to Print UCC and CCL labels of scanned boxes.
//              2) Last ucc/ccl print date and print by is only visible if it is not null
// - EPC page : Added feature of printing UCC and CCL label's for the boxes of scanned epc.
// - Tracking Number page : Added feature of printing UCC and CLL label's for the box of scanned Tracking number.
// - Box Pallet page : 1) Added feature of printing Pallet's header, UCC and CLL of boxes on scanned pallet.
//                     2) Added last_ucc_print_date and last_ccl_print_date in box grid
//                     3) Added information of number of UCC and CLL labels already printed for boxes on pallet
//                     4) Added information about who picked the pallet
// - Carton Page : 1) Pallet Id in carton history grid is now non clickable.
//                 2) Link to report 40.23 is available for those carton's that have history.
// - Added Transferred EPC scan


//1.1.6.0 -> 1.1.7.0 (Tagged on 20 April by Ankit Sharma)
// - Fixed bug's of mobile view.
// - Box Pallet page : Cosmetic changes on Boxes on pallet and Pallet content's grid.
// - Carton Area page : Inventory grid is only visible on desktop version.

//1.1.7.0 -> 1.2.0.0 (Tagged on 19 May by Ankit Sharma)
// - Carton Page - 1) Showing whether a carton is reserved or not.
//                 2) If you scan any carton we will show the assigned location and area.
//                 3) Added feature for carton Ticket printing for active cartons.
//                 4) Added mobile view for carton page that incorporates subset of information from desktop view of carton.
// - Carton Pallet Page : 1) Showing SKU summary and the locations where it is assigned.
//                        2) Added mobile view for carton pallet that incorporates subset of information from desktop view of carton.
// - Showing carton history of opened carton also.(Already Reviewed).
// - Box Pallet Page : 1) Provided feature to user to print only Non printed boxes or all boxes.
//                     2) Grid for boxes on pallet is now sorting on UCC id.                   
// - Box :   1) Showing message incase of archive that box can't be print.
// - Style page : showing grid that displays which color of scanned style comes from which country.

//1.2.0.0 -> 1.2.1.0 (Branched on 22 May 2012 Ankit Sharma)
//Carton Pallet Page : 1) The grid SKU assigned at is now called as Pallet's content.And managed its heading.

//1.2.1.0 -> 1.2.1.1 (Branched on 8 june 2012 Ankit Sharma)
// - Sku PAge  : Added information about pieces per package and retail price.


//1.2.1.1 -> 1.2.2.0 (Branched on 15 June 2012 By Ankit Sharma )
//	Box Scan : Box Audit information is now  available.
//	Box Pallet Scan: Added feature to print pallet summary for ADREPPWSS buckets.
//                  2)Added information about number of boxes will be printed.
//	Carton Scan:  When user scans a carton marked for some work, we show the information related to the work to be done on carton.
//                  2) Added old and new suspense date in carton history grid.
//	Printer Friendly Pages: Created printer friendly pages in Inquiry including the one for pallet printing. 
//	Carton Pallet Scan : We show the restock aisles where the SKUs present in cartons are assigned.
//	UPC scan :  Added information about Additional Retail Price.
//	SKU Location Scan: Current contents of the location are available.
//                    2) Added inormation for PitchAisle and RestockAisle.
//                    3) Added information for Sku Assigned on a location.
//  Carton Pallet Scan : Showing restock aisle assigned for cartons on pallet.
//  _layoutInquiry.cshtml : Added meta tag for IE9 compatibilty.


//1.2.2.0 -> 1.2.2.1 (Branched on 11 july 2012 Ankit Sharma)
// Carton Page : Added information of To and From Location in Carton History grid.

//1.2.2.1 -> 1.2.3.0(Branched on 20 july 2012 Ankit Sharma)
// Showing short name instead of Carton storage area in all pages in application expect the carton history grid on carton page.
// Handled issue of page break on Open carton scan.
// Box Page : Added feature of Catalog Printing.

//1.2.3.0 -> 1.2.3.1 (Branched on 30 july 2012 Ankit Sharma)
// Supported scan of Customer sku id.

//1.2.3.1 -> 1.2.4.0 ( Branched on 3 Aug 2012 by Ankit Sharma)
//Carton Page : Added application action in carton history grid.
//Sku Location Page : Added location history. We also need to send this script in with this version of release for index of LocationId  on locationAudit table
//                    Create index LOCATION_AUDIT_LOCATIONID_I on LOCATION_AUDIT (LOCATION_ID)


//1.2.4.0 -> 1.2.5.0 (Branched on 22 Aug 2012 by Ankit Sharma)
// Sharad 8 Aug 2012: Updated AreaRegistration class. Now this app depends on post 2.1.5.0 version of DcmsMobile.
// Removed the use of deleted coloumn Remark_Work_Needed of src_carton from inquiry


//1.2.5.0 -> 1.2.6.0 (Branched on 24 Sep 2012 by Ankit Sharma)
// Rajesh Kandari 30 Aug 2012: Added SearchRoute class to make the area searchable.
// Sku page - Deleted use of obsoleted coloumns Auto_Open_Carton , Parent_IaId and CYc_Area_Flag 
// Sharad 5 Sep 2012: Upgraded to T4MVC 2.10
// Sharad 11 Sep 2012: Removing Oracle mapping warnings. Now post 7.0.6.0 version of EclipseLibrary.Oracle is required
// Carton Page - Added Old and New quantity in carton history grid.
// Location Page - Added limit of 500 for location audit

//1.2.6.0 -> 1.2.6.1 (Tagged by Ankit Sharma on 18 jan 2013 We had created this branch from production version 1.2.6.0 to emergency issue)
//fixed crashing issue on scanning rework cartons

//1.2.6.1 -> 1.2.6.2 (Tagged by Ankit Sharma on 18 jan 2013 We had created this branch from branch version 1.2.6.1 to emergency issue)
// Carton page : Added default carton type parameter in HandleCartonScan() of Active Contorller so that other applications do not need to edit 
//               their query strings of carton page link.

//1.2.6.0 -> 1.2.7.0 (Branched by Ankit Sharma on 7 jan 2013)
// Pickslip page - Removed title Transferred Pickslip now we are only saying Pickslip.
// Sharad 4 Oct 2012: Critical Bug Fix. Dispose() method was missing in all controllers which can cause major memory leaks. Added the method and disposed repository in it.
// Carton Scan : Fixed isuue revealed in error report by user about crashing Carton page.
// Box Print : Fixed crashing of page on printing catalog or UCC or CCL label if document is not specified in custsplh.
// Carton Area page : Calling cartons as carton in Inventory grid.
//                  : Added condition on showing Assigned SKU.
// Carton Location : Fixed bug : Now we are showing message 100 out of total number of cartons on location.
// Box Pallet page : Fixed bug : Now we are conditionally showing Area and Customer link.Added links in box grid.
// Sharad 30 Oct 2012: Changed attribute on InquiryAreaRegistration.AreaName to [DisplayFormat(DataFormatString = "~/{0}/Home/Index?id={1}")].
//    This permits DCMS Mobile to invoke Inquiry evenn when the search text contains invalid characters such as : or &. The previous value of this attribute was
//    [DisplayFormat(DataFormatString = "~/{0}/Home/Index/{1}")] which would lead to the error Http 400 when search text contained invalid characters.
// UPC page : Fixed bug in query for that fetches SKU inventory per Area , VWh.Added $ in front of retail price.
// Added AppointmentNumber Scan in inquiry.
// Box Page : Added VAS related information.
//Backend dependency : Need to implement VAS related and Appointment related tables on production.
// Rajesh Kandari 19 Dec 2012: Commented VAS related information.
// Upgrading Inquiry module to MVC4 (Binod Kumar)
// Removed the dependency of EclipseLibrary.Mvc.Html.ModelBinding.ReflectionExtensions.NameFor
// Bucket : Now we are calling Created On as Creation Time and creation time with creation date.
// Outbound Shipment : Showing appointment number on Outbound Shipment page if shipment have any appointment 
// PO page : Showing export flag and reporting status in pickslip grid.
// Handing the Exceptions during printing of CCL, UUC, Carton ticket.
// Updated buid actions for Chirpy.ReadMe.txt and T4MVC.TT
// Removed Depemdency of EclipseLibrary.Mvc.Helpers

//1.2.7.0 -> 1.2.8.0 (Branched by Ankit Sharma 19 jan 2013)
// PO page : Added information for number of UCCs and CCls printed for PO.
// Carton Page : Provided default type of carton in HandleCartonScan to make query strings flexible for other applications.
// Carton Page : Fixed crashing issue on scanning rework carton.

//1.2.8.0 -> 1.2.9.0 (Branched by Ankit Sharma 18 feb 2013)
// Box Pallet : Added feature to print pallet header only.
// Added new scripts and theme version.

/*  1.2.9.0 -> 1.3.0.0 (Branched by Ankit Sharma on 26 feb 2013)
 * Box Scan : Added VAS related information on box page
 */

/*1.3.0.0 -> 1.3.1.0 (Branched by Ankit Sharma on 30 april 2013)
 * Sku location : Now we are showing location audit in two seperate grids one for Assign/Unassign audit and another for inventory audit.
 * UPC scan : Now we are showing description.
 */

/*1.3.1.0  -> 1.3.2.0 (Branched by Ankit SHarma on 9 MAy 2013)
 * Removed the use of table SRC_SHIPMENT instead of that we are now using SRC_CARTON_INTRANSIT for Active and Open carton query.
 * 
 */

/*1.3.2.0  -> 1.3.3.0 (Branched by Ankit Sharma on 06 JUN 2013)
 * Pickslip page : Added feature to print packing slips agains pickslip
 * Now we are using Tab_warehouse_location instead of warehouseloc
 */

/*1.3.3.0 -> 1.3.4.0 (Branched by Ankit Sharma on 11 JUN 2013)
 * Box Pallet page : Fixed bug in box pallet query
 * 
*/

/*1.3.4.0 -> 1.3.5.0 (Branched by Ankit Sharma on 6 Aug 2013) (Abandoned in favor of next version)
 * Outbound Shipment : Fixed bug of using wrong column in query from table TAb_Warehouse_location
 * 
 * 1.3.5.0 -> 1.3.6.0 (Branched by Ankit Sharma on  23 Aug 2013) (Abandoned in favor of next version)
 *   Sharad 13 Aug 2013: Removed AutocompleteItem warning. No longer using <meta http-equiv="X-UA-Compatible" content="IE=9" /> to force browser to render in standards mode
 *   Now we donot have any Transfer controller and repository.So we will not support transfer scans like PickSLip, PO, UCC, Tracking #, EPC.
 *   We are now having a scan for In Order Bucket Pickslip and PO.
 *   PO page: Po page is redesigned to overcome the need of PMG.
 *   Pickslip Page : Pickslip age is redeigned to overcome the need of PMG.
 *   Box Pallet Page : We have redsigned box pallet page.Like we have added picker name in box list and also all the lists are visible any time.
 *   Sku Location : Sku location page is redesigned now we are not showing assigned SKU in header information even it will be available in Location content grid with a check icon.
 *   Deleted Jquery and validation Scripts from layout and inserted where it is required.
*/

/*
 *1.3.6.0 -> 1.3.7.0 (Branched By Ankit Sharma on 04 oct 2013) (Redmine Deployment
 * Sku Location : Now we are showing Short Name of IaID instead of IaId.
 * Box :  1) Now we are showing Picker name in SKU grid.
 *        2) Printing is only available for non cancelled boxes and Catalog label printing is available for user's who have catalog label  printing document.
 *        3) Now we are showing short name of IaId instead of IaId.
 * Bug Fixed : On EPC and Tracking page when we print a box the page got redirected to index page, but it is fixed now.
 * Upgraded jquery from 2.0.2 to 2.0.3.
 * Carton Pallet Page : Now we are showing Short Name of IaID instead of IaId in carton list.
 * Sku page : Now we are showing Short Name of IaID instead of IaId in Sku inventory grid.
 * Sku Area page : Now we are showing Short Name of IaID instead of IaId .
 * Box Pallet page : Now we are showing Short Name of IaID instead of IaId.
 * Bug Fixed :Inventory grid is rendering below footer, it is fixed now.
 * */

/*
 * Change log 1.3.7.0 -> 1.3.8.0 (Branched by Ankit Sharma on 16 Nov 2013)
 *  I have changed SKU area and carton area scans to support both area id and short name.
 *  Added building on Box , box pallet page, sku area page. 
 */

/*
 * Change log 1.3.8.0 -> 1.3.9.0 (Branched by Ankit Sharma on 18 Jan 2014)
 * Bug Fix: Added handling for more than 9 digit pickslip id for SAP.
 */

/*
 * Change log 1.3.9.0 - 1.3.10.0 (Branched by Anil Panwar on 27 Jan 2014)
 * Intransit SHipment :  Added SKU details for shipemnt.
 * Pickslip page : Added Shipping Adress and Vendor number for pickslip.
 */

/*
 * Change log  1.3.10.0 -> 1.3.11.0 (Branched by Anil Panwar on 28 Jan 2014)
 * Showing pickslip carrier on pickslip scan.
*/

/*
 * Change log  1.3.11.0 -> 1.3.12.0(Branched by Ankit Sharma on 30 Jan 2013)
 * Bug FIxed : We were not showing actual pieces in BOL in case of pickslip is shipped but not transferred.
*/

/*
 * Change log  1.3.12.0 -> 1.3.13.0(Branched by Shiva Pandey on 7 Feb 2014)
 * Bug FIxed : We were not showing sewing plant of any shipment in case of if sewing plant is new.
*/

/*
 * Change log  1.3.13.0 -> 1.3.14.0(Branched by Ankit Sharma on 14 Feb 2014)
 * Intransit Shipment :Added information for Received, Expeted and over rage cartons, we are also showing ERP type, Shipment status and intransit Type
*/

/*
 * Change log  1.3.14.0 -> 1.3.14.9010(Branched by Ankit Sharma on 15 Feb 2014)
 * Intransit Shipment : Now we are include those cartons that were received by another shipment in expected and non-received cartons.
*/

/*
 * Change log  1.3.14.9010 -> 1.3.14.9011(Branched by Ankit Sharma on 19 Feb 2014)
 * Intransit Shipment : Bug Fix that we were not showing shipment that were completly got merged in some other shipment.This is happened because while parsing intransit shipement as valid scan we were looking for shipment id that got changed by buddy shipment concept.
*/
/*
 * Change log  1.3.14.9011 -> 1.3.14.9021(Branched By Ankit Sharma on 25 Feb 2014)
 * Feature : Added new scan of Return Authorization Number.
 * Enhancement : HAndled null handling on pickslip scan.
 * Bug Fixed : Improved exception handling on print packsing slip and on print bol.
 */

/*
 * Change log  1.3.14.9021 -> 1.3.14.9031(Branched By Ankit Sharma on 18 Mar 2014)
 * Outbound Shipment page : Added feature to print bol.
 * Added autocomplete for customerId on index page.
 * We are showing price of sku on pickslip page.
 * Customer page : Added information about customer order summary on customer page.
 */

/*
 *  * Change log  1.3.14.9012 -> 1.3.14.9022(Branched by Ankit Sharma on 28 Mar 2014)
 *  Outbound SHipment page.The outbound shipment page is parent shipment specific now and will show list of BOLs contained in or is related to scanned shipment.
 */

/*
 * Change Log 1.3.14.9022 -> 1.3.14.3 (Branched By Ankit Sharma on 15 Apr 2014)
 * SKU page : Added feature about order summary of SKU.
 * Return Scan : On scanning a return authorization number we will get list of recent 500 receipt's for that return,summary level information is available at this page but we can further query for receipt detail by clicking on receipt number from grid.
 * Box page : Added feature to cancel box.We need to Login as a DCMS8_POMGR user to use this feature.
 * Enhancement : Removed Eclipse Library Grid from cshtml apges and used table instead of them.
 * Customer Page : The order summary on customer page is redefined to have more useful information for customer.
 */

/*
 * Change Log 1.3.14.3 -> 1.3.14.9013 (Branched By Ankit Sharma on 21 Apr 2014)
 * Outbound Shipment : 1) Fixed bug that we were showing ToZipCOde in from address.
 *                          2) Fixed bug of wrong weight calculation for shipments in query.
 * Return Receipt : Now we are showing price of return per SKU instead of Retail Price.
 * 
 */

/* 
 * Change Log 1.3.14.9013 -> 1.3.15.0 (Branch by Ankit Sharma on 16 May 2014)
 * Pickslip page : Showing picklsip constarint information on pickslip scan.
 * Upc page : Showing Standard case quantity on  UPC scan. 
 * Added Master BOL scan in inquiry.
 * Added Export to excel feature in Inquiry on pages that have grid in them. 
 * Added Intransit Shipment summary report.
 * Internal enhancements : Now Inquiry is following standards of MVC archeitecture and we have made all entities as internal.
 */

/* 
 * Change Log 1.3.15.0 -> 1.3.16.0 (Branch by Shiva on 22 May 2014)
 * Showing Shipment summary and Shipment SKU summary Page.
 */

/* 
 * Change Log 1.3.16.0 -> 1.3.17.0 (Branch by HKV on 27 May 2014)
 *  Fixed the issue where Building transfer filter was not working correctly.
 *  Total of Carton variance was not correct, which is not fixed.
 *  Closed shipments page are now sorted on "Sent To ERP" date 
 *  Open Shipments page is now sorted on Shipment Date. 
 */

/*
 * Change Log 1.3.17.0 -> 1.3.18.0 (Branch by Ankit Sharma on 3 Jun 2014)
 * Fixed Bug that we were not sending absolute address while exporting to excel.
 */

/*
 * Change Log 1.3.18.0 -> ? 
 * For closed shipment – we do not care about the Sent to ERP date.
 * Put the shipment closed date in the column that was Sent to ERP and remove from the shipment box.
 * Always reference the shipment close date as such.
 * Added a filter by plant.
 * Added a column just for the original shipment id when the pcs received is > 0.
 */


[assembly: AssemblyVersion("1.3.18.0")]
[assembly: AssemblyFileVersion("1.3.18.0")]
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Inquiry.Tests")]
[assembly: AssemblyProduct(@"
<ol>
<li>
Showing Shipment summary and Shipment SKU summary Page.
</li>
</ol>")]