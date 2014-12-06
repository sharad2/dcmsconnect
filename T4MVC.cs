﻿// <auto-generated />
// This file was generated by a T4 template.
// Don't change it directly as your change would get overwritten.  Instead, make changes
// to the .tt file (i.e. the T4 template) and save it to regenerate this file.

// Make sure the compiler doesn't complain about missing Xml comments and CLS compliance
#pragma warning disable 1591, 3008, 3009
#region T4MVC

using System;
using System.Diagnostics;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using System.Web.Mvc.Html;
using System.Web.Routing;
using T4MVC;

[GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
public static partial class MVC_DcmsRights
{
    static readonly DcmsRightsClass s_DcmsRights = new DcmsRightsClass();
    public static DcmsRightsClass DcmsRights { get { return s_DcmsRights; } }
}

namespace T4MVC
{
    [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
    public class DcmsRightsClass
    {
        public readonly string Name = "DcmsRights";
        public DcmsMobile.DcmsRights.Areas.DcmsRights.Controllers.AutoCompleteController AutoComplete = new DcmsMobile.DcmsRights.Areas.DcmsRights.Controllers.T4MVC_AutoCompleteController();
        public DcmsMobile.DcmsRights.Areas.DcmsRights.Controllers.HomeController Home = new DcmsMobile.DcmsRights.Areas.DcmsRights.Controllers.T4MVC_HomeController();
        public T4MVC.DcmsRights.SharedController Shared = new T4MVC.DcmsRights.SharedController();
    }
}

namespace T4MVC
{
    [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
    public class Dummy
    {
        private Dummy() { }
        public static Dummy Instance = new Dummy();
    }
}

[GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
internal partial class T4MVC_System_Web_Mvc_ActionResult : System.Web.Mvc.ActionResult, IT4MVCActionResult
{
    public T4MVC_System_Web_Mvc_ActionResult(string area, string controller, string action, string protocol = null): base()
    {
        this.InitMVCT4Result(area, controller, action, protocol);
    }
     
    public override void ExecuteResult(System.Web.Mvc.ControllerContext context) { }
    
    public string Controller { get; set; }
    public string Action { get; set; }
    public string Protocol { get; set; }
    public RouteValueDictionary RouteValueDictionary { get; set; }
}



namespace Links_DcmsRights
{
    [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
    public static class Scripts {
        private const string URLPATH = "~/Scripts";
        public static string Url() { return T4MVCHelpers.ProcessVirtualPath(URLPATH); }
        public static string Url(string fileName) { return T4MVCHelpers.ProcessVirtualPath(URLPATH + "/" + fileName); }
        public static readonly string jquery_2_1_1_js = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/jquery-2.1.1.min.js") ? Url("jquery-2.1.1.min.js") : Url("jquery-2.1.1.js");
        public static readonly string jquery_ui_1_10_0_js = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/jquery-ui-1.10.0.min.js") ? Url("jquery-ui-1.10.0.min.js") : Url("jquery-ui-1.10.0.js");
        public static readonly string jquery_validate_js = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/jquery.validate.min.js") ? Url("jquery.validate.min.js") : Url("jquery.validate.js");
        public static readonly string jquery_validate_unobtrusive_js = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/jquery.validate.unobtrusive.min.js") ? Url("jquery.validate.unobtrusive.min.js") : Url("jquery.validate.unobtrusive.js");
    }

    [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
    public static class Content {
        private const string URLPATH = "~/Content";
        public static string Url() { return T4MVCHelpers.ProcessVirtualPath(URLPATH); }
        public static string Url(string fileName) { return T4MVCHelpers.ProcessVirtualPath(URLPATH + "/" + fileName); }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public static class themes {
            private const string URLPATH = "~/Content/themes";
            public static string Url() { return T4MVCHelpers.ProcessVirtualPath(URLPATH); }
            public static string Url(string fileName) { return T4MVCHelpers.ProcessVirtualPath(URLPATH + "/" + fileName); }
            [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
            public static class Start1_10_0 {
                private const string URLPATH = "~/Content/themes/Start1.10.0";
                public static string Url() { return T4MVCHelpers.ProcessVirtualPath(URLPATH); }
                public static string Url(string fileName) { return T4MVCHelpers.ProcessVirtualPath(URLPATH + "/" + fileName); }
                public static readonly string jquery_ui_css = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/jquery-ui.min.css") ? Url("jquery-ui.min.css") : Url("jquery-ui.css");
                     
            }
        
        }
    
    }


    [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
    public static partial class Areas {
        private const string URLPATH = "~/Areas";
        public static string Url() { return T4MVCHelpers.ProcessVirtualPath(URLPATH); }
        public static string Url(string fileName) { return T4MVCHelpers.ProcessVirtualPath(URLPATH + "/" + fileName); }
    
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public static partial class DcmsRights {
            private const string URLPATH = "~/Areas/DcmsRights";
            public static string Url() { return T4MVCHelpers.ProcessVirtualPath(URLPATH); }
            public static string Url(string fileName) { return T4MVCHelpers.ProcessVirtualPath(URLPATH + "/" + fileName); }
            [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
            public static class Scripts {
                private const string URLPATH = "~/Areas/DcmsRights/Scripts";
                public static string Url() { return T4MVCHelpers.ProcessVirtualPath(URLPATH); }
                public static string Url(string fileName) { return T4MVCHelpers.ProcessVirtualPath(URLPATH + "/" + fileName); }
                public static readonly string DcmsRights_all_js = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/DcmsRights-all.min.js") ? Url("DcmsRights-all.min.js") : Url("DcmsRights-all.js");
                public static readonly string DcmsRights_all_min_js_map = Url("DcmsRights-all.min.js.map");
            }
        
        }
    }

    public static partial class Areas {
    
        public static partial class DcmsRights {
            [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
            public static class Content {
                private const string URLPATH = "~/Areas/DcmsRights/Content";
                public static string Url() { return T4MVCHelpers.ProcessVirtualPath(URLPATH); }
                public static string Url(string fileName) { return T4MVCHelpers.ProcessVirtualPath(URLPATH + "/" + fileName); }
                public static readonly string Autocomplete_partial_css = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/Autocomplete.partial.min.css") ? Url("Autocomplete.partial.min.css") : Url("Autocomplete.partial.css");
                     
                public static readonly string DcmsRights_all_css_bundle = Url("DcmsRights-all.css.bundle");
                public static readonly string DcmsRights_all_css = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/DcmsRights-all.min.css") ? Url("DcmsRights-all.min.css") : Url("DcmsRights-all.css");
                     
                public static readonly string DcmsRights_partial_css = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/DcmsRights.partial.min.css") ? Url("DcmsRights.partial.min.css") : Url("DcmsRights.partial.css");
                     
                public static readonly string RestrictedPrograms_PDF = Url("RestrictedPrograms.PDF");
                public static readonly string standardized_partial_css = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/standardized.partial.min.css") ? Url("standardized.partial.min.css") : Url("standardized.partial.css");
                     
            }
        
        }
    }
    [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
    public static partial class Bundles
    {
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public static partial class Scripts {}
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public static partial class Styles {}
    }
}

[GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
internal static class T4MVCHelpers {
    // You can change the ProcessVirtualPath method to modify the path that gets returned to the client.
    // e.g. you can prepend a domain, or append a query string:
    //      return "http://localhost" + path + "?foo=bar";
    private static string ProcessVirtualPathDefault(string virtualPath) {
        // The path that comes in starts with ~/ and must first be made absolute
        string path = VirtualPathUtility.ToAbsolute(virtualPath);
        
        // Add your own modifications here before returning the path
        return path;
    }

    // Calling ProcessVirtualPath through delegate to allow it to be replaced for unit testing
    public static Func<string, string> ProcessVirtualPath = ProcessVirtualPathDefault;

    // Calling T4Extension.TimestampString through delegate to allow it to be replaced for unit testing and other purposes
    public static Func<string, string> TimestampString = System.Web.Mvc.T4Extensions.TimestampString;

    // Logic to determine if the app is running in production or dev environment
    public static bool IsProduction() { 
        return (HttpContext.Current != null && !HttpContext.Current.IsDebuggingEnabled); 
    }
}





#endregion T4MVC
#pragma warning restore 1591, 3008, 3009


