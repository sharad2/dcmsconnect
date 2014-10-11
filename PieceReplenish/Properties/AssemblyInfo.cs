using System.Reflection;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Piece Replenish")]
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



// Change log of Piece Replenishment version 1.0.0.0(tagged by dpanwar on 7 september 2012)
// This application queries the data from Table REPLENISH_CARTON                        [ Dependency ]
// This application depends upon some new columns of IALOC table                        [ Dependency ]
// - IALOC.REPLENISHMENT_PRIORITY
// - IALOC.REPLENISHMENT_PRIORITY_EXPIRY
// - IALOC.REPLENISHMENT_PRIORITY_BY
// This application depends upon some new columns of TEMP_PULL_CARTON table             [ Dependency ]
// - ASSIGN_DATE
// - RESTOCK_AISLE_ID
// - BUILDING_ID
// And package PKG_REPLENISH
// Some new grants are also required for Roles DCMS8_REPLENISH, SRC_PULLING. Plz check the file Roles.txt in this project.
// Later version than 7.0.5.0 of EclipseLibrary.Oracle is required                      [ Dependency ]
// Later version than 2.1.5.0 of EclipseLibrary.Mvc is required                         [ Dependency ]

// Change log of restock
// Rajesh Kandari 8 Aug 2012: Does not use IPluggableArea2. Will require updated version of DcmsMobile.
// Restock depends on PKG_RESV.ADD_TO_IALOC2.
// Process_Id column of box productivity must be nullable.
// Require Role for Restock DCMS8_RESTOCK
// Rajesh Kandari 30 Aug 2012: Added SearchRoute class to make the area searchable.

// Change Log 1.0.0.0 -> 1.0.0.1
//  11 Sep 2012 (Binod): Bug Fix. Suggested aisle was lost when carton was placed in suspense.
// 12 Sep 2012 (Binod): Bug Fix. Suggested cartons are ordered by user priority. Earlier they were being ordered by wave
//     priority as well which was leading to jerky pull paths.
// 12 Sep 2012 (Binod): Bug Fix: while suggesting cartons, now we are ensuring that carton is still in BIR.
// 12 Sep 2012 (Binod): Bug fixed: changed SUM(L.ASSIGNED_UPC_MAX_PIECES - C.NUMBER_OF_UNITS) with SUM(L.ASSIGNED_UPC_MAX_PIECES - NVL(C.NUMBER_OF_UNITS,0)) in procedure PKG_REPLENISH.REFRESH_PULLABLE_CARTONS
// 13 Sep 2012 (Rajesh): Carton suggestions are strictly pick path based. They do not circle back to first location. High prioity cartons will be pulled in the order they are encountered.

// Change Log 1.0.0.1 -> 1.1.0.0 [Branched by Binod Kumar on 24 Sep 2012]
// 24 Sep 2012 (Binod): Bug fixed: If puller scanned a random pallet, then aware the puller for case of aisle mixing.
// 24 Sep 2012 (Binod): If puller just don't want to pull the suggested carton. Now he can remove it from list of suggestions.
// 24 Sep 2012 (Binod): Removed dependency on AutoMapper

//Change Log 1.1.0.0 -> 1.1.1.0 [Branched by Binod Kumar on 3 OCT 2012]
// 28 Sep 2012 : Added Retail price on UPC and Location Page in Restock.
// 28 Sep 2012 : Now we are not showing Hungry location grid that was visible on carton page.
//  3 Oct 2012 : UI enhancements, Showing page title.

//Change Log 1.1.1.0 -> 1.2.0.0 [Branched by Binod Kumar on 12 OCT 2012]
// 12 OCT 2012 : Replenishment Pulling: System allows overpulling to complete the orders which are currently being processed.
// 12 OCT 2012 : Replenishment Pulling: Dashboard is improved now to show operpulling status.
// 12 OCT 2012 : Replenishment Pulling: Now Puller activity table contains buttons to discard the remaining cartons which are reserved to the pallet.

//Change Log 1.2.0.0 -> 1.2.1.0 [Branched by Binod Kumar on 19 OCT 2012]
// 19 OCT 2012 : Replenishment Pulling: Updated presentation link.
// 19 OCT 2012 : Updated build action, and changed the assembly info before creating branch

//Change Log 1.2.1.0 -> 1.2.2.0 [Branched by Ankit Sharma on 26 OCT 2012]
// Creating branch for making it Backend compatible.

//Change Log 1.2.2.0 -> 1.2.3.0 [Branched by Binod Kumar on 31 OCT 2012]
// 30 OCT 2012 - Replenishment Pulling: Now service will be called with user name for those action method which having AuthorizeExAttribute, otherwise with super user.

//Change Log 1.2.3.0 -> 1.3.0.0 [Branched by Binod Kumar on 19 Nov 2012]
// 19 Nov 2012 - Replenishment Pulling: Now given a new UI to Diagnose the requirement and availability of SKU

//Change Log 1.3.0.0 -> 1.3.1.0 [Branched by Binod Kumar on 4 DEC 2012]
// Bug fixed. Now showing proper count of pullable cartons in diagnostic screen

// Change Log 1.3.1.0 -> 1.3.2.0(Branched by Ankit Sharma 4 jan 2013)
// Now showing location text bigger and bold, suggested by system in Restock. This fixes has already been deployed on Production.
// Upgraded to MVC4
// Issue fixed: Hidden field value was not being passed in mvc4 for bool.

// Change Log 1.3.2.0 -> 1.3.3.0(Branched by Shiva Pandey 28 jan 2013)
// Now showing PriceSeasonCode of scanned pallet's carton.
// Removed the dependency of EclipseLibrary.Mvc.Helpers 
// Passing 1 for Inquiry carton using "~/Inquiry/Carton/{0}/1" query string.

/*
 * Change Log 1.3.3.0 -> 1.3.4.0 (Branched by Ankit Sharma 17 jun 2013)
 * In GetLocation query of restock we have redefined join of ialoc and ialoc_content into left outer and added nvl function while gettin pieces required.
 * */

/*
 * Change Log 1.3.4.0 -> 1.3.5.0 (Branched by Binod Kumar 3 Sept 2013) (Sharad 4 Oct 2013: This version was never deployed to production. Superseded by next version).
 * Not showing exception error in case of if carton could not be pulled. 
 */
// Change Log 1.3.5.0 -> 1.4.1.0 (Branched by Shiva Pandey 18 oct 2013) (Redmine Task #1592)
// Binay 27 Sep 2013:
// New feature: Now Showing Rail capacity in the replenishment Restock UI.
// UI Improvement:Now showing a message on UI that option is available start again if restocker does not want to 
// restock the carton after scanning it. Following message is dispalyed "Enter to Go back"
// Bug fixed: Fixed null reference exceptions in Restock UI @ action methods UPC(),RestockCarton(),Location().
// Bug fixed: User is getting kicked off after logging in to the new Restock program,recovered successfully.
// Sharad 27 Sep 2013:
//  On Mobile devices, login is requested before the user can initiate replenishment pulling.
//  On the desktop, the carton textbox is disabled if the user is not logged in. A login link is also provided.
// Sharad 7 Oct 2013:
//  Allow Carton scan when UPC is expected.
//  Put the carton in suspense only when the user confirms that the carton contains a different SKU. There should be no other reason for the carton to go into suspense.
//  Use caching to avoid several per carton/UPC level queries.
// Sharad 8 Oct 2013: Not asking for building any more. We display location suggestions of all buildings.
//  Not validating that number of pieces in carton is in multiples of pieces per package
//  Caching scanned carton info for 15 min
// Sharad 9 Oct 2013: Accept private label scans
// Feature: Provided option on index page to emulate mobile.
// New feature: Font sizes have been increased.
// Sharad 30 Oct 2013: When the user scans a carton, we do not use the carton cache. The carton cache is only used durng UPC and location scans.
// Shiva 31 oct 2013
// In Replenishment Pulling :
// Error sound will come up instead of success when wrong carton is scanned.
// Now showing Short Name instead of Pick Area Id.

//  Sharad 31 Oct 2013: Fixed missing closing span tag in Building.mobile.cshtml


/* Change Log 1.4.1.0 -> 1.4.1.9020 (Merged in branch by Ankit SHarma on 05 Nov 2013)
 *  Shiva 2 Nov 2013 : Trying to handle the situation of POSTING after login has expired.
 *  Sharad 2 Nov 2013: Capturing building and pallet id in carton productivity.
 *  Bug fixed: Fixed null reference exceptions in Restock UI.
     
*/
/*Change loh 1.4.1.9020 -> 1.4.1.9030 (merged in branch by Ankit Sharma on 05 Nov 2013)
 * FIXED BUG : On carton scan we were caching cartons location assignments  but now we are not caching in case of no location is assigned to carton.
 */

/*
 * Change Log 1.4.1.9030 -> 1.4.1.9040 (Merged in Branch By Ankit sharma on 07 Nov 2013)
 * Bug fixed : Building choice textbox was not working on firefox.
 */

/*
 * Change Log 1.4.1.9040 -> 1.4.1.9050 (Merged in Branch By Shiva Pandey on 07 Nov 2013)
 * Bug Fixed : Set Priority method not found error.
 */

/*Change Log 1.4.1.9050 -> 1.4.1.9060 (Merging in branch by Ankit Sharma on 12 Nov 2013)
 * 
 * Fixed minor bug in CSS that footers border were overlapping in other content.
 */

/*Change Log 1.4.1.9060 -> 1.4.1.9070 (Merging in branch by Ankit Sharma on 13 Nov 2013)
 * 
   Bug fixed : Building choice textbox was not posting choice in mozilla firefox but it is working now.
 */


/*Change Log 1.4.1.9070 -> 1.4.1.9080 (Merging in branch by Ankit Sharma on 15 Nov 2013)
 * Sharad 14 Nov 2013: You need to be a puller or manager to pull a carton. Before this fix, only managers were being allowed to pull carton.
 * On SKU search we are ignoring cartons that requires work on it 
 * Now we are allowing Rework cartons to get restocked.
 * Added Select grant on IA table for src_pulling role.
 * Added  Select grant on mater_customer_sku table for dcms8_restock role.
 */

/* Change Log 1.4.1.9080 -> 1.4.2.0 (Branched by Shiva Pandey 25 Feb 2014)(Redmine Task #1767)
 * Deepak 25 Feb 2014 : Restock now allows restocking to special system locations created for MOL orders.
 */

/* Change Log 1.4.1.9080 -> 1.4.2.0 (Branched by Shiva Pandey 10 Mar 2014)(Redmine Task #1783)
 * Shiva 26 Feb 2014 : Now agreed to manually manage MOL locations. So we do not need any mirror location specific customizations.So we revert all works doing for MOL location.
 * Shiva 26 Feb 2014 : Delete the branch version 1.4.2.0 also.
 * Shiva 10 Mar 2014 : Bug Fixed : Now showing Pullar name is original not dcms8.
 */

[assembly: AssemblyVersion("1.4.2.0")]
[assembly: AssemblyFileVersion("1.4.2.0")]
[assembly: GuidAttribute("B8A0B2C7-D843-4E34-A74A-4BDA2274199B")]
[assembly: AssemblyProduct(@"
<ol>    
   <li>
        New feature: Now Showing Rail capacity in the replenishment Restock UI on carton scan.
   </li>
    <li>
        New feature: Allow Carton scan when UPC is expected.
    </li>
    <li>
        New feature: Accept private label scans.
    </li>
    <li>
        New feature: Not asking for building any more. We display location suggestions of all buildings..
    </li>
    <li>
        New feature: Put the carton in suspense only when the user confirms that the carton contains a different SKU.
    </li>
    <li>
        New feature: Font sizes have been increased.
    </li>
    <li>
        UI Improvement: Now showing a message on UI that option is available start again if restocker does not want to 
        restock the carton after scanning it. Following message is dispalyed 'Enter to Go back'
    </li> 
    <li> 
        Bug fixed: User is getting kicked off after logging in to the new Restock program,recovered successfully.Now---
            1. On Mobile devices, login is requested before the user can initiate replenishment pulling.
            2. On the desktop, the carton textbox is disabled if the user is not logged in. A login link is also provided.
    </li>
    <li>
        New Feature : Now we are allowing Rework cartons to get restocked.
    </li>
</ol>
")]