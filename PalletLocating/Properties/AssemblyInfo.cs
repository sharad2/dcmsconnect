using System.Reflection;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("PalletLocating")]
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

//Change Log

// Change Log from 1.0.0.0 to 1.0.0.1
//* Repos: GetCartonAreas() returns null building areas for any passed building.
//* Service: LocatePallet() takes areaId to avoid a query
//* Building is required
//* Displaying Pallet Area and location for scanned pallet

//* Change Log from 1.0.0.1 to 1.0.0.2
//*  * Validating the entered building
//*  * Default area deduction logic considers building specific areas only.
//*  * User can scan carton where pallet is expected. Pallet will be deduced.
//*  * 

//* Change Log from 1.0.0.2 to 1.0.5.0
//*  * Added Sounds

//Change log from 1.0.6.0 to 1.0.7.0 
// Applied the patch 1.0.0.4 sent by Sharad Sir.
// Optimizing replenishment suggestions. 
// Simplified the suggestion query and simplified the display of the suggestions.

//ChangeLog from 1.0.7.0 to 1.0.8.0
// Pallet Locating will generate an error tone when a mixed SKU pallet is scanned.
// Allows orphaned cartons to be put in a pallet
// You can merge two pallets

//ChangeLog from 1.0.8.0 to 1.0.8.1
// * Removed InfoModel and the associated feature. I am not sure what this feature was supposed to be since the grid was confusing,
// and the Model name InfoModel was a disaster. Just the name convinced me that it is better to remove the feature rather
// than spend time to debug and optimize it.
// * Refactored model names related to PalletMovement feature. The word Info was being overused and I am now using the term
// * movement. I have also spent some time in optimizing the queries

//Change Log from 1.0.8.1 to 1.0.9.0
//1.	Documentation has been improved.
//2.	I had suppressed the requirement that the suggested area must be honoured now suggestions are honoured as earlier. 
//3.	Providing proper message if Pallet2 does not exist at a location. 
//4.	Handled null exception in Merge Pallet code. If the pallet to merge to was not found in database code was breaking. 
//5.	Short Name is being used instead of area id in Pallet Movement. 
//6.	Now we are considering assigned_vwh_id in our suggestions and during locating, earlier on assigned_sku_id was considered.  
//7.    Productivity is captured in carton_productivity
//8.	Includes UI improvements from patch 1.0.8.2.

//Change Log from 1.0.9.0 to 1.0.10.0
//During capacity violation the program asked for confirmation from user but it did not allow pallet to be located. 
//We have fixed it now. (Sharad applied this patch manually)

// Change Log from 1.0.10.0 to 1.0.10.1 (Sharad 9 Jan 2012)
// - Destination area can be unnumbered. In this case, user is not expected to scan a location.

// Change Log from 1.0.10.0 to 1.0.11.0 
// - Improved UI message in case we try to merge to same pallet.
// - Improved UI message when location is unavailable.
// - Code review, removed commented code

// Change Log from 1.0.11.0 to 1.0.12.0
// -You can scan a location instead of area and area will be deduced from scanned location..
// -Date filters in PalletMovement report was not showing today's data. We have fixed it.
// -Now productivity info are updating properly, single row per pallet is now being inserted

// Change log from 1.0.12.0 to 1.0.12.1
// - Max 15 suggestions shown on mobile screen.
// - When a pallet is located, set suspense date of each carton to null, because cartons are no longer in suspense
// - SKUs in open buckets show up first in replenishment suggestion list
// - On the desktop, replenishment suggestions provide hyperlinks to Inquiry for pallets and locations
// - MaxReplenishmentSuggestions hardwiring moved from service to controller. This is still bad. This hardwiring needs
//   to move to the view

// Change log from 1.0.12.1 to 1.0.13.0
// - Added missing proxy tag in query
// - Bug fixed: In Replenishment Suggestions grid on the pallet screen, links with Inquery app is working fine
// - Minor improvement in query of GetReplenishmentSuggestions()
//
// Change log from 1.0.13.0 to 1.0.13.1 Sharad 30 Jan 2012
// - Replenishment Suggestions take unavailable buckets into account
// - Replenishment Suggestions are being cached to improve performance
// - When a pallet is located, suspense date of each carton is set to null
// - Added link to documentation

// Change log from 1.0.13.1 to 1.0.14.0
// - Changed Expiration time to 10 mins from 20 mins
// - Minor improvement in query of GetReplenishmentSuggestions()
// - Changed the build action of documentation file
// - Added missing proxy tag in query

// Change log from 1.0.14.0 to 1.0.14.1
// - Optimized layout style
// - removed DCMS_DBA role

// Change log from 1.0.14.1 to 1.0.14.2 (Sharad 2 Feb 2012)
// - Provided link to Update Replenishment suggestions on the desktop Pallet page. This is useful if the cache becomes stale.
// - Minor style improvements
// - Fixed bug. Area and Suggestion caches are now absolute expiration. They were wrongly set to sliding.

// Change log from 1.0.14.2 to 1.0.14.3
// - Replenishment Suggestion was not shown properly after refresh using link, Bug Fixed. Now trying to remove the suggestion before adding to cache


// Change log from 1.0.14.3 to 1.0.14.4
// - Bug Fixed in Replenishment Suggestion query


// Change log from 1.0.14.4 to 1.0.15.0 (Deepak 27 Feb 2012)
// - Now we consider orders for making replenishment suggestions.

// Change log from 1.0.15.0 to 1.0.16.0 (Deepak 28 Feb 2012)
// - CPK quantity is subtracted from demand. Now suggestions closely match Report 130.25

// Change log from 1.0.16.0 to 1.0.17.0 (Deepak 17-04-2012 )
// - We now honor priority when making suggestions. The SKUs with higher priority are suggested first
// - We do not suggest freezed buckets


// Change log from 1.0.17.0 to 1.0.18.0 (Rajesh Kandari on 12 july 2012)
// - Removed PluggableArea warnings

// Change log from 1.0.18.0 to 1.0.19.0 (Branched by Ankit Sharma on 29 oct 2012)
// Rajesh 8 Aug 2012: Does not use IPluggableArea2. Will require updated version of DcmsMobile.
// Now we use IOracleDatastore3 in place of IOracleDatastore2.
// Rajesh Kandari 30 Aug 2012: Added SearchRoute class to make the area searchable.
// Now we use OracleDatastore in place of IOracleDatastore3.
// Sharad 5 Sep 2012: Upgraded to T4MVC 2.10
// Removed AutoMapper
// Removed warnings
// Null handling in the Pallet locating repository.
// Removed Interface.

// 1.0.19.0 -> 1.0.20.0(Branched by Binod Kumar 9 jan 2013)
// Sharad 27 Nov: Removed Option to for Mobile View
// Binod 29 Dec 2012: Upgraded to MVC4
// Removed the dependency of EclipseLibrary.Mvc.Html.ModelBinding.ReflectionExtensions.NameFor 
// Bug fixed: Orphan carton was not being located and if carton does not contain any SKU, application was crashing.
// Messages improved.

// 1.0.20.0 -> 1.0.21.0(Branched by Rajesh Kandar on 28 jan 2013)
// No longer using sound file of DcmsMobile. 
[assembly: AssemblyProduct("No visible change, only stability improvements")]
[assembly: AssemblyVersion("1.0.21.0")]
[assembly: AssemblyFileVersion("1.0.21.0")]


//$Id$
