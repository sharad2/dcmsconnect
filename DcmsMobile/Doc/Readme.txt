Sharad 4 Nov 2011: Mobile Specific Pages  
----------------------------------------
DcmsMobile enables the use of Mobile specific pages, thanks to the implementation of DcmsMobile.Helpers.MobileCapableRazorViewEngine
class.

1. An entry must exist in App_Browsers\Ringscanner.browser to identify the device which should be treated as mobile.
To make the Ringscanners get detected as a mobile device, the following entry is used:

  <browser id="RingScanner" parentID="IE6to9">
    <identification>
      <capability name="majorversion" match="6" />
      <capability name="platform" match="WinCE" />
    </identification>
    <capabilities>
      <capability name="isMobileDevice" value="true" />
    </capabilities>
  </browser>

2. MobileCapableRazorViewEngine should be added to view engines in global.asax.

protected void Application_Start()
{
    AreaRegistration.RegisterAllAreas();
    RegisterGlobalFilters(GlobalFilters.Filters);
    RegisterRoutes(RouteTable.Routes);

    ViewEngines.Engines.Clear();
    ViewEngines.Engines.Add(new MobileCapableRazorViewEngine());
}

3. For mobile devices, MobileCapableRazorViewEngine will search for "ViewName.Mobile.cshtml" whenever the view "ViewName" is requested.
If this view is not found, then the original search is performed.

4. You can look at the mobile interface from the Diagnostic screen by choosing an option there. This works by setting a cookie of predefined name which tells
MobileCapableRazorViewEngine to render mobile views regardless of device type.

Deploying DcmsMobile (Sharad 21 May 2012)
-----------------------------------------
Overwrite folders DcmsMobile, Views. Copy DLL to bin.

All other folders are public folders used by all apps. They will be changed only if we know that a change has been made to them (e.g. jquery upgrade).

Sharad 8 Aug 2012: IPluggableArea2 Deprecated
---------------------------------------------
The AreaRegistration derived class should now be decorated with attributes to supply display information. Implementing IPluggable2 is no longer necessary. Getting away from
interface makes extensibility easier.

        [Display(Description = "Create DCMS Users and assign rights to run DCMS programs", Name = "DCMS Rights Assignment", Order = 1000)]
        [UIHint("desktop", "DcmsMobile")]
        public override string AreaName
        {
            get
            {
                return "DcmsRights";
            }
        }

Sharad 8 Aug 2012: Release Candidate support
--------------------------------------------
Specify a URL for the RC site in web.config
<add key="RcUrl" value="~/ReleaseCandidate" lockItem="true" />

Now DCMSMobile layout page will display link to this Release Candidate URL. If the URL of the currently running site is same as RCUrl, then a heading will inform the user that this
is a Release Candidates site.

Entering 0 on main menu will redirect to RC web site. Test this on ring scanner.

Provide link on the desktop layout page.

Changelog provided in AssemblyInfo.cs file:
[assembly: AssemblyProduct("This should be the change log")]



