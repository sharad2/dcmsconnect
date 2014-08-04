using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Repack")]
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
[assembly: Guid("279cc079-ceed-420e-a410-b6a90dd3fbfc")]

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
 * Changed from 1.0.1.0 to 1.0.1.1: Repack from Shelfstock renamed to Repack from Shelfstock or SSS.
 * Source areas for this UI now display SSS as well. Printer is required in this UI.
 * Advanced UIs require the privilege DEPT_INV_ADMIN.
 * Added meta tag <meta http-equiv="X-UA-Compatible" content="IE=9" > to prevent IE8 from entering compatibility mode.
 * When enter pressed after entering UPC code, validating it and focusing piecesl
 */

//ChangeLog from 1.0.2.0 to 1.0.2.1: 
// UI improvements.
// The module code must be RPK because reason code philosophy embedded in 
// package IFR_ISI special handles reason codes for this module.

//ChangeLog from 1.0.2.1 to 1.0.3.0: 
// Tag created after testing changes in 1.0.2.1

//Change log from 1.0.3.0 to 1.0.6.0 (12 Apr 2012 )
// Changed the AssemblyCompany and AssemblyCopyright info
// Repack for conversion UI provides option to downgrade quality.
// You can enter price season code during repack in shelf stock, Blind receiving and conversion UI's. 
// Renamed AssemblyName “Repack “  to  “DcmsMobile. Repack”. 
// Added Roles.txt file.
// Added Repack documentation.
// In Blind Receiving UI of Repack, now we are able to Repack cartons in any unnumbered area. We have removed IS_RECEIIVNG_AREA flag.
// We have removed CONVERSION_AREA flag. Now you can convert in any area.
// Removed project reference of unused files in Scripts and Content.
// Updated theme image inclusions in project.
// Pallet_id is passed in lower case in database which was wrong. We have fixed this issue.
// For Blind Receiving Ui now we does not provide source area. 
// AutoCompleteController was wrongly using REQ2 as module code. Changed it to Repack.
// In blind receiving screen we take pallet when the destination area is a pallet area. 


// Change Log from 1.0.6.0 to 1.0.7.0 (Tagged By Rajesh Kandari on 17 May 2012)
// Now we use IoracleDataStore3 in place of IoracleDataStore2.
// Now all UI using src_rpk role.


//Change Log from 1.0.7.0 to 1.0.8.0 (Created branch on 29-05-2012 by Rajesh Kandari)
// Now we use package pkg_carton_work_2.mark_carton_for_work instead of pkg_carton_work.markctnforwork.


//Change Log from 1.0.8.0 to 1.0.9.0 (Branched by Rajesh Kandari on 11 july 2012)
// Option to repack from EXM source area is now available in all Ui.

// Change Log from 1.0.9.0 to 1.1.0.0 (Branched by Shiva on 6 Aug 2012)
// Added new UI Repack cancelled box, which allows you to repack cancelled boxes.
// Repack cancelled box UI requires "src_rpk" role.
// Repack will now use dcms8 connection string.
// User can also repack from Cancelled area in Advance UI.
// Dependency on package PKG_PICKSLIP.CANCEL_BOX.
// Dependency on SRC_OPEN_CARTON.SKU_ID column.

// Change log from 1.1.0.0 to 1.1.1.0 (Branched By Ankit Sharma on 18 oct 2012)
// Rajesh Kandari 8 Aug 2012: Does not use IPluggableArea2. Will require updated version of DcmsMobile.
// Rajesh Kandari 30 Aug 2012: Added SearchRoute class to make the area searchable.
// Now we use OracleDatastore in place of IoracleDataStore3.
// Sharad 5 Sep 2012: Upgraded to T4MVC 2.10//
// Removed Warnings from Repository
// Removed Automapper from controller.
// Fixed bug : Now when we provide Carton id through keyboard it will be inserted in Upper cases

// Change log from 1.1.1.0 to 1.1.2.0 (Branched by Ankit Sharma on 1 nov 2012)
// Fixed bug for creating conversion request even when user has unchecked the conversion check box
// Categorized destination areas in Pallet and Non Pallet area in Conversion and Bulk Conversion UI.
// Printer is not compulsory in Repack from Shelf Stock or SSS UI.
// Bug fixed that we were repacking on carton when user press enter while selecting targetsku from autocomplete

// Change log from 1.1.2.0 to 1.1.2.1 (Branched by Ankit Sharma on 6 nov 2012)
// Fixed bug that we were matching source and target sku even if user has unchecked the conversion check box.

// Change log from 1.1.2.1 to 1.1.3.0(Branched by Ankit Sharma on 4 jan 2013)
// Upgraded to MVC4 and removed warnings.

// Change log from 1.1.3.0 to 1.1.4.0 (Branched by Ankit Sharma on 10 Oct 2013)
// Binay 2 sep 2013: Provided new feature: Now application allows for Pallet Id even if destination area is a non pallet area but it's not mandatory in case of non pallet area.
// Binay 6 sep 2013: Bug fixed : When user scans wrong SKU with pieces, program shows an error message "Invalid SKU" but still creates carton,now it has been fixed.
// Binay 1 Oct 2013: Bug fixed :  On scanning wrong UPC Repack application is crashing,now it has been fixed.
// Binay 1 Oct 2013 :Provided new feature: Now application is showing only Conversion Destination areas for Repack for Conversion and Bulk Repack for Conversion feature.
// Sharad 1 Oct 2013: The Home/Index page no longer uses an accordion.
// Removed all code related to Repack cancelled box
// Sharad 3 Oct 2013: Conditionally including jquery 2.0 for IE >= 9
// Sharad 3 Oct 2013: Repack from shelf stock does not show conversion and receiving areas in destination.
// Blind Receiving shows receiving areas only
// Now we have included receving areas in Repack from Shelf Stock or SSS screen.
// Now we are showing No Change option in Target Quality Code drop down.
// In repack dialog now we have RESET feature for dialog properties.

// Change log from 1.1.4.0 to 1.1.4.001 (Merged in Branch by Ankit Sharma on 30 Oct 2013)
// Fixed Bug : Now we have fixed issue that SKU description is getting invisble after scanning same SKU back to back.
// Fixed bug : We were creating carton of given carton id even if user unchecks require carton id checkbox after giving it once.

/* 
 *  Change log from 1.1.4.001  to 1.1.5.0 (Branched by Ankit Sharma on 12 May 2014)
 *  Now We are showing Standard case quantity of SKU on SKU scan if available.
 *  Some internal changes has been made by Sharad sir like : 
 *      1. Added T4MVC 2.12 to packages.config.
 *      2. Using Web Essentials for bundling CSS and scripts, instead of Chirpy.
 *      3. Preventing keyword expansion in auto generated files.
 *      4. Updated Build script to prevent copying of *.bundle files to DcmsMobile/Areas.
 *      
 */

[assembly: AssemblyProduct(@"
<ol>
<li>
  If Standard case quantity of SKU is set we will show it on SKU scan.
</li>
</ol>
")]
[assembly: AssemblyVersion("1.1.5.0")]
[assembly: AssemblyFileVersion("1.1.5.0")]
