using System.Reflection;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("DcmsMobile")]
[assembly: AssemblyDescription("The launcher for all DCMS Mobile programs")]
#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif
[assembly: AssemblyCompany("Eclipse Systems Private Limited")]
[assembly: AssemblyCopyright("Copyright ©  2012")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("77fafa67-85bb-43de-9066-7c82b3c69aa1")]

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
 * Change Log 2.1.2.0 to 2.1.2.1:
 * 1. DcmsMobile launcher screen shows the list of programs  according to device specifications,
 * but when user entered the codes displayed it was not linking to  the correct  application according to menu list.
 * Now this bug is fixed.
 * */
//Patch sent by Dr.Sharad is implemented successfully and now version is changed from 2.1.2.1 to 2.1.3.0

/*
 * Changing Log 2.1.3.0 -> 2.1.3.1
 *   - Added IE9 meta tag
 *   - Added validation-summary-errors css class to sitemobile.css
 *   - Submitting the menu choice is a get operation instead of a post operation. Action renamed to AcceptChoice.
 *   - Multidigit choices are now supported. Thus choice 10 is now valid. Earlier it would have been treated as an inquiry scan.
 *   - ChangePassword Dialog: Old password is stored in temp data. User is not prompted for it again.
 */

//BK:Removed the Default_old folder from DcmsMobile\Content folder to Sync with production copy.

/*
 * Changing Log 2.1.3.1 -> 2.1.4.0
 * - Added httpRuntime element to web.config
 * - Added new base URL to be used for report links
 * - Added T4MVC and jquery.min.css
 * - Fixed voluntary change password issues
 * - Changed AssemblyCopyright to 2012
 * - Displaying the link with WIP image for new security management feature on first page of DCMS mobile
 * - Displaying role and user information for all areas.
 * 
 * Changing Log 2.1.4.0 -> 2.1.5.0 (Tagged by Rajesh Kandari on 21 May 2012 )
 *  - Moved DcmsMobile Specific files from Content and Scripts to DcmsMobile/Content and DcmsMobile/Scripts respectively.
 *  - Changed reference paths in views accordingly
 *  - Improved diagnostics while logging in.
 *  - Sharad 15 May 2012: No longer supporting IPluggableArea2.ActivityAction
 *  
 * Change log 2.1.5.0 -> 2.2.0.0(Branched By Ankit Sharma on 14 Aug 2012)
 *   - Area descriptions can now contain HTML
 *   Sharad 7 Aug 2012: 
 *   - Added release Candidate Support. Entering 0 as main menu choice redirects to Release Candidate site specified in web.config appsettings
 *     <add key="RcUrl" value="~/ReleaseCandidate" lockItem="true" />
 *   - If RcUrl matches the URL of this site, then master layout pages display "These are Release Candidates" alert near top of page.
 *   - Improved footer layout
 *   - Using strongly typed T4MVC in more places.   
 *   Sharad 8 Aug 2012: 
 *   - Can parse attributes to discover IPluggableArea2 information. This is the first step in deprecating IPluggableArea2.
 */

/* Change log 2.2.0.0 -> 2.3.0.0(Branched by Rajesh Kandari on 03 Oct 2012)
 * - Some minor change in clue tip script.
 * - Sharad 29 Aug 2012: Can query areas for search results
 * - Sharad 4 Sep 2012: Major refactoring. Upgraded to T4MVC 2.10. Now this requires latest EclipseLibrary.MVC.
 * - Sharad 15 Sep 2012: Diagnostics screen now displays connection strings
 * - Sharad 20 Sep 2012: Search results page provides an option to search using Inquiry.
 * - Ankit Sharma : Use jquery_ui_css instead of jquery_ui_min_css in mobile layout.
 */

/* Change log 2.3.0.0 -> 2.3.1.0(Branched by Rajesh Kandari on 09 Oct 2012)
 * Provided Google type searching of applications based on Keywords. For e.g.;  search keyword INQ for Inquiry for Rc website.
 * 
 * 2.3.1.0 -> 2.3.2.0(Branched by Rajesh Kandari on 10 Oct 2012)
 * Sharad 9 Oct 2012: Added web.config entries to ensure that login survives when the user navigates to the ReleaseCandidates website.
 *   It will also survive across server farms.
 *   Providing a mechanism to return to main site from RC site.
 */

/*
 * Change Log 2.3.2.0 -> 2.4.0.0 (Branched by Ankit sharma on 07 july 2012)
 * Sharad 15 Oct 2012: Removed support for IPluggableArea2
 * Sharad 19 Oct 2012:
 *   - Fixed paths to sound files in _soundPartial.
 *   - Removed the view error.cshtml since custom display of error proved not to be useful.
 *   - 404 errors are logged using the new ApplicationErrorEvent class
 * 
 * Sharad 21 Nov 2012:
 *   - Fixed Bug: Entering 0 as a menu choice should navigate to the Release Candidates web site
 */

/*
 * Change Log 2.4.0.0 -> 2.4.1.0 (Tagged by by Rajesh kandari on 3 Jan 2013)
 * Removed the hyperlink "go to main site" and edited the hyperlink "Click here to go main menu" to "Main menu" from Rc website.
 */

/*
 * Change Log 2.4.1.0 -> 2.4.2.0 (Branched By Rajesh Kandari on 20 Feb 2013)
 * Sharad 29 Dec 2012: Removed mobile emulation options from Diagnostic Index view. These were already available in master layout.
 * Sharad 30 Jan 2013: Upgraded to jquery 1.9.0. jQuery 1.8.2 files are still part of Script directory for backward compatibility
 * Sharad 31 Jan 2013: Removed unversioned jquery files. Now all apps will be forced to upgrade to jquery-1.9.0.js. Upgraded to jquery-ui-1.10.0. Added theme Start1.10.0.
 *   DcmsMobile now uses jquery ui 1.10. jquery.css is no longer included in mobile layout.
 * Rajesh Kandari 20 Feb 2013: Included missing script files jquery.validate.js, jquery.validate.min.js, jquery.validate.unobtrusive.js and jquery.validate.unobtrusive.min.js in DcmsMobile Scripts folder.
 */

/*
 * Change Log 2.4.2.0 -> ?
 * Sharad 22 Feb 2013: Added code in global.asax to Suppress the error A potentially dangerous Request.Path value was detected from the client
 * Sharad 25 Feb 2013: Updated NugetPackage FixedDisplayModes from 1.0.0 to 1.0.1
 * Sharad 26 Feb 2013: Updated to jquery 1.9.1, jquery Validation v1.11.0, jQuery.Unobtrusive.Validation v2.0.30116.0
 * Rajesh Kandari 21 May 2013: Updated to jquery Validation v1.11.1
 * Sharad 8 Aug 2013: Modernizing main menu.
 * Sharad 9 Aug 2013: Ignore signed assemblies while constructing Area menu
 * Sharad 12 Aug 2013: At the start of a session, clear the overridden browser. Suppose you emulate mobile, and then close the browser. When you restart the browser, the emulation
 *   would continue which was surprising. We now clear emulation at each session start. Code written in Session_Start of global.asax.
 *  Sharad 4 Oct 2013: Included jquery 2.0.3. Removed jQueryMigrate since we are not using it.
 *  Sharad 5 Oct 2013: Changed to <meta http-equiv="X-UA-Compatible" content="IE=Edge" />
 *  Sharad 31 Oct 2013: Not using RedirectToChoice in desktop views
 */

[assembly: AssemblyVersion("2.4.2.0")]
[assembly: AssemblyFileVersion("2.4.2.0")]
[assembly: AssemblyProduct("Provided google type searching for applications based on keywords in DcmsMobile for Rc Website and a mechanism to return to main site from RC site.")]
