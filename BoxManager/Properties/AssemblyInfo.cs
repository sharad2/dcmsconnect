using System.Reflection;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("DcmsMobile.BoxManager")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Eclipse Systems Private Limited")]
[assembly: AssemblyCopyright("Copyright © Eclipse Systems 2013")]
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

// Initial Changes
//This application allow users to create pallets based on defined rules for customers at dock area.
//Application also provides the feature of moving the pallets within or across areas.
//On moving pallets application also provide suggestion of locations so that the same criteria pallets can 
//be placed together. 

/*
Change Log (1.0.0.0 to 1.0.1.0) Branch By: MBisht(12th June 2012)
Incorporated Users feedbacks
 1. Box of a staging pallet will be placed in suspense. Those boxes will come out of suspense once they are
     placed on a real pallet.
 2. Pallet suggestion list will also display the percent full for each pallet based on cubic volume limit. 
 3. The list of suggested pallets will include only those pallets which have sufficient remaining capacity 
     to accommodate all cartons on the staging pallet.
 */

// Change Log (1.0.1.0 to 1.0.2.0) Branch By: Ankit Sharma(29th June 2012)
// When a pallet is moved from forward dock to door area, program will validate pallets first.
// We do not display box list in move pallet UI.

// Change Log(1.0.2.0 to 1.0.3.0)
// 1: Service: Using cache to optimize performance of box scan. Not redirecting to optimize performance on ring scanners.
// 2: Changed message during merge.

// Change log from 1.0.3.0 to 1.0.4.0 (Branched by Shiva Pandey on 18 Aug 2012)
// Rajesh Kandari 9 Aug 2012: Does not use IPluggableArea2. Will require updated version of DcmsMobile.
// Shiva 18-8-2012 (Bug Fixed): Now we make sure that the scanned box is shippable while performing validation during move pallet.

// Change log from 1.0.4.0 to 1.0.6.0 (Branched by Binay Bhushan on 21 Aug 2012)
// New application Box Editor is available in Box Manager with following functionality:
//1: Remove SKUs of Box.
//2: Re-pitch Box.
//3: Send box to Green area.
//4: Cancel Box. 

//Backend Depandency:
//1: For Cancelation of Box we are dependent on package "pkg_pickslip.cancel_box".
//2: For Repitching of Box we are dependent on package "pkg_mpc.create_mpc_for_box".
//3: Dependency on PKG_RESV.ADD_TO_IALOC2 for remove sku pieces.

// Change log from 1.0.6.0 to 1.0.7.0(Branched by Hemant K. Singh on 18th Oct 2012)
// Rajesh Kandari on 30 Aug 2012: Added SearchRoute class to make the area searchable.
// Sharad 5 Sep 2012: Upgraded to T4MVC 2.10
// Shiva 25-9-2012: Fix this bug, When enter no. of boxes more than 10 digit on text area in validate pallet UI got an error message.
// Hemant K. Singh 29-09-2012: Removed AutoMapper.
// Removed warnings
// Showing SKU retail price in Box Editor UI.
// Showing available inventory of the SKUs in the box in various areas.
// Hemant K. Singh 14-11-2012: Making trunk equivalent to branch version 1.0.7.1.
// Added the virtual warehouse to the SCM UI.
// Title Area Inventory be changed to "Inventory in" in SCM UI.
// Have corrected the sequence of sub programs.
// In the remove pieces section, added a privilege message "Tip: To remove pieces you need role ALLOW_REMOVE_PIECES." to remove pieces.
// Displayed a counter to tell how many pieces are removed. 
// Showing inventory of all areas. Updated the query of GetBoxSkuDetails(). 
// Added building to the SCM UI.
// Backend Dependency:
// Dependency on IALOC, IALOC_CONTENT and TAB_QUALITY_CODE tables for fetching the box sku details.
// Backend dependency : Require Grant Select on ps_vas to DCMS8_SCM 
//                      Require Grant Select , Update on box_vas to DCMS8_SCM 
//                      Require Grant Select on TAB_VAS

// Change log from 1.0.7.0 to 1.1.0.0 (Branched by Binod Kumar on 18 Dec 2012)
// 1. New UIs are being introduced in this release: VAS Configuration and V2P
// 2. Backend changes are well mentioned in task #1094.
//     # New tables TAB_VAS, PS_VAS, MASTER_CUSTOMER_VAS.
//     # BOX_PROCESS is renamed to BOX_VAS
//     # Changes done in PKG_DATA_EXCHANGE
//     # New roles are required DCMS8_VASTOPALLET, DCMS8_VAS see Roles.txt

// Change log from 1.1.0.0 to 1.1.1.0 (Branched by Binod Kumar on 09 May 2013)
// Binod  29 Dec 2012: Upgraded to MVC4
// Rajesh: Issue fixed: Hidden field value was not being passed in mvc4 for bool.
// Rajesh: Issue fixed: Display template was not working fine with Mcv4.
// Binod  07 Jan 2013: BoxEditor was showing huge error message on dialog. Fixed. 
// Rajesh 28 Jan 2012: No longer using sound file of DcmsMobile. Added new sound for error. These changes has already been fixed in branch version 1.0.7.3.
// Binod: Added new controller/Repos/Service for Validation UI
// Shiva: Bug fixed: ScanToPallet/MovePallet allowed to use unverified pallet to move and other work. Fixed this bug.
// Binod: 09 May 2013: V2P should assort the boxes for palletization on the basis of Customer only.

// Change log from 1.1.1.0 -> 1.2.0.0
// Shiva 21 May 2013: Show appointment information such as appointment number,appointment date and door id where to ship the order in move pallet UI.
// Binod 27 May 2013: Boxes of small shipments can be palletized for VAS now. (Merged from Branch 1.1.1.1)
// Binod 29 May 2013: Upgraded the VAS UI

// Change log from 1.2.0.0 to 1.2.1.0 (Branched by Binay Bhushan on 18 Sep 2013)
// Binay 29 Aug 2013: BoxManager is crashing when user tab to address bar and press enter, recovered successfully.
// Binay 17  Sep 2013: Issue: Unable to ship carton when the system generates a temporary pallet id which has already been used.

/* Change Log 1.2.1.0 -> 1.2.2.0(Branched by Shiva Pandey on 7 Nov 2013)
 *  Shiva 2 Nov 2013 : Bug Fixed : Trying to handle the situation of POSTING after login has expired.
*/
// Change Log 1.2.2.0 -> 1.2.3.0(Branch by Mbisht on 19 April 2014)
// V2P will not allow VAS on RED area boxes. 
// Done by HSing:- Presentations used in BoxManager are changed from .pptx to .ppt   

[assembly: AssemblyProduct(@"
<ol>
<P>
Changes from version 1.2.2.0 -> 1.2.3.0
</P>
<li>
V2P program will not accept RED area boxes for VAS.
</li>
</ol>
")]
[assembly: AssemblyVersion("1.2.3.0")]
[assembly: AssemblyFileVersion("1.2.3.0")]