using System.Reflection;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("EclipseLibrary.Mvc")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Eclipse Systems Private Limited")]
[assembly: AssemblyProduct("EclipseLibrary.Mvc")]
[assembly: AssemblyCopyright("Copyright ©  2011 $Id$")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("535838a3-bc15-414b-8026-cba7a5cf3b40")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]

// 2.1.0.1 -> 2.1.1.0 (Sharad 14 Feb 2012)
// Added property Purpose to AuthorizeEx attribute 

// 2.1.1.0 -> 2.1.2.0 (Sharad 28 Mar 2012) tagged by Hemant on 28 Mar 2012
// Added static function AuthorizeEx.IsSuperUser(). DcmsRights application uses this function to determine whether the executing user is a super user.

// 2.1.2.0 -> 2.1.3.0 (Tagged By Binod on 17 May 2012)
// Sharad 24 Apr 2012: GroupDropListFor now generates Validation attributes for client side validation
// Sharad 15 May 2012: IPluggableArea2.ActivityAction marked obsolete

// 2.1.3.0 -> 2.1.4.0 (Tagged by Deepak Bhatt on 5th June 2012)
// Added EclipseController.GetStatusMessages()

// 2.1.4.0 -> 2.1.5.0 (Tagged by Rajesh kandari on 12 July 2012)
// Sharad 9 Jul 2012 -> Removed obsolete class FormActionSelectorAttribute
// Removed obsolete property RegisteredAreas.PluggableAreas
// Sharad 12 Jul 2012: GroupDropListFor() Can specify html attributes along with option label. 

//  2.1.5.0 -> 2.1.6.0 (Already tagged)
// Sharad 4 Sep 2012: Upgraded to T4MVC 2.10.1
// Sharad 5 Sep 2012: Removed obsolete class EclipseController.AutoCompleteItem
// Sharad 15 Oct 2012: Removed support for IPluggableArea2
// Sharad 20 Oct 2012: EclipseController.OnException() automatically handles AJAX request exceptions. Also logs them to health monitoring.

// 2.1.6.0 -> 2.1.7.0(Branched by Ankit Sharma on 7 dec 2012)
// Sharad 27 Nov 2012: ReflectionExtensions class moved to EclipseLibrary.Mvc.Html.ModelBinding namespace in preparation for upgrading to MVC 4

// 2.1.7.0 -> 2.1.8.0(Branched by Binod Kumar on 08 Jan 2013)
// Sharad 7 Jan 2013: For AJAX requests, EclipseController.OnException() does nothing if the exception has already been handled. This makes it compatible with other
//  other error handling attributes, such as HandleAjaxError, which might exist on the action or controller.

// 2.1.8.0 -> ?
// Sharad 27 Feb 2013: Deprecated class AutocompleteItem. Deprecated WebGridEx family of classes.
//          Removed previously deprecated functionality:
//            class MobileEmulation,  class ReflectionExtensions, ReflectionHelpers.FieldNameFor, class HandleAjaxErrorAttribute, class MobileCapableRazorViewEngine

[assembly: AssemblyVersion("2.1.8.0")]
[assembly: AssemblyFileVersion("2.1.8.0")]


//<!--$Id$-->
