using System.Reflection;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("REQ2")]
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

// Change log from 1.0.1.0 to 1.0.2.0
// * Autocomplete does not suggest inactive SKUs

// Change Log from 1.0.2.0 to 1.0.3.0

//1. Destination area can not be unusable area. Also removed CAN_REQUEST_CARTON flag.
//2. Source area should be carton area and numbered area. 
//3. Supports default button on ManageSku page. 
//4. Links to reports added.
//5. Added missing proxy tag.
//6. Provided document link.

// Change Log from 1.0.3.0 to 1.0.4.0
// - Created new tutorial page (.cshtml) and removed REQ2.docx file.
// - Removed conversion flag. Providing flexibility to create request in any numbered area and can be requested for conversion in any non numbered carton area.
// - Provided a checkbox to make conversion request at create request page.
// - Added script to show-hide target VwhId dropdownlist.
// - For normal request “Conversion SKU” will be not required.
// - Showing "Is Conversion Request" flag in Recent Request List.
// - showing Quality Code dropdownlist in create request page.
// - Provided inventory hiding concept by downgrading the target qality code.
// - Documentation updated.
// - UI improvement along with styling issues.
// - Code reveiwed and removed commented code.
// - Changed report url




//Change log from 1.0.4.0 to 1.0.5.0 (Rajesh kandari 22 Feb 2012)
// - Added new file calendar.gif to REQ2/Areas/REQ2/Content/images folder.
// - Removed hardwiring in links. Getting links from t4MVC.
// - Provided link for Conversion screen in Palletize module.
// - Added new roles.txt file.
// - Provided description in the link to Reports.

//Change log from 1.0.5.0 to 1.0.6.0 (Tagged By Binod 24 April 2012)
// - Bug Fixed: Add sku button was doubling quantities requested when a Sku was being added to the order.
// - Code cleaned up


//Change log from 1.0.6.0 to 1.0.8.0(Tagged By Rajesh Kandari 4 May 2012)
// - Showing who created the request in REQ2 now.

//Change log from 1.0.8.0 -> 1.0.8.1(Branch created by Shiva 28 May 2012)
// - (Sharad 17 May 2012) Using IOracleDataStore3 instead of Using IOracleDataStore2.
// - (Shiva 28 May 2012) Autocomplete does help to choose SKUs and Now it help to enter similar SKUs.
// - (Shiva 28 May 2012) The cursor should return to Source SKU line after a SKU has been added to the list.
// - (Shiva 28 May 2012) Pieces value should not be erased when a SKU is added.
// - (Shiva 28 May 2012) Given checkbox to change quality code. Changing quality code will be possible only when this checkbox is checked.


//Change log from 1.0.8.1 to 1.0.9.0(Branched by Rajesh Kandari on 08 Aug 2012)
//Introduce building concept in REQ2. If user enters building during request creation ,only the cartons of entered building will be reserved.

// Change log from 1.0.9.0 to 1.0.10.0 (Branched by Rajesh kandari on 17 Aug 2012)
// Rajesh Kandari 8 Aug 2012: Application does not use IPluggableArea2 and it will require latest version of DcmsMobile.


// Change log from 1.0.10.0 to 1.0.11.0(Branched by Ankit Sharma on 15 oct 2012)
// Rajesh Kandari 30 Aug 2012: Added SearchRoute class to make the area searchable.
// Now we use OracleDatastore in place of IoracleDataStore3.
// Sharad 4 Sep 2012:Upgraded to T4MVC 2.10
// Removed warning
// Removed AutoMapper
// Fixed bug for exposing carton reservation id in message on Delete request.
// Removed Interfaces from ReqServices and ReqRepository

// Change log from 1.0.11.0 to 1.0.12.0(Branched by Ankit Sharma on 22 oct 2012)
// Bug Fixed : Autocomplete on SKU page were not working.
// Changed URL of Convert Carton link on Recent Request page.
// Bug Fixed : Handled error redundancy on manage Sku page for pieces required.
// On carton list page now the header is Carton List instead of Cartonlist.
// Added exception handling script for source and target sku auto complete.

// Change log from 1.0.12.0 to 1.0.13.0 (Branched by Binod on 8 jan 2013)
// Bug Fixed : When we delete a request from recent requests then all the delete icons get enable.Issue is fixed now.
// Upgraded to MVC4 and remove warnings.
// Binod 7 Jan 2013: Removed obsolete HandleAjaxErrorAttribute

// Change log from 1.0.13.0 -> 1.1.0.0 (Branched by Rajesh Kandari on 25 jun 2013)
// Sharad 5 Feb 2013: Upgraded to jquery UI 1.10
// Now user can search cartons while creating request.
// Copy request feature: Allows you to create new request for same source and destination area quickly. You just need to change the SKU.
// Provided cascading dropdown list for selected building.
// Autocomplete now suggest inactive SKUs also.

// Change log from 1.1.0.0 -> ?
// Sharad 2 Jul 2013: Upgraded to jquery 2.0.2 for IE > 8
// Major UI changes in REQ2.
// Backend Dependency
// 1: Dependency on SKU_ID column of SRC_REQ_DETAIL
// 2: Renamed CONVERSION_AREA flag name to IS_CONVERSION_AREA of TAB_INVENTORY_AREA table.
// 

[assembly: AssemblyProduct(@"
<ol>
<li>
Now user can also provide the search criteria for the cartons.
</li>
<li>
Copy request feature: Allows you to create new request for same source and destination area quickly. You just need to change the SKU.
</li>
<li>
Provided cascading dropdown list for selected building.
</li>
<li>
Autocomplete now suggest inactive SKUs also.
</li>
</ol>
")]
[assembly: AssemblyVersion("1.1.0.0")]
[assembly: AssemblyFileVersion("1.1.0.0")]
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("REQ2.Test")]