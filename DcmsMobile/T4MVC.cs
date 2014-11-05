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
public static partial class MVC_DcmsMobile
{
    public static DcmsMobile.MainArea.Diagnostic.DiagnosticController Diagnostic = new DcmsMobile.MainArea.Diagnostic.T4MVC_DiagnosticController();
    public static DcmsMobile.MainArea.Home.HomeController Home = new DcmsMobile.MainArea.Home.T4MVC_HomeController();
    public static DcmsMobile.MainArea.Logon.LogonController Logon = new DcmsMobile.MainArea.Logon.T4MVC_LogonController();
    public static T4MVC.SharedViewsController SharedViews = new T4MVC.SharedViewsController();
}

namespace T4MVC
{
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



namespace Links_DcmsMobile
{
    [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
    public static class Scripts {
        private const string URLPATH = "~/Scripts";
        public static string Url() { return T4MVCHelpers.ProcessVirtualPath(URLPATH); }
        public static string Url(string fileName) { return T4MVCHelpers.ProcessVirtualPath(URLPATH + "/" + fileName); }
        public static readonly string bootstrap_js = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/bootstrap.min.js") ? Url("bootstrap.min.js")+"?"+T4MVCHelpers.TimestampString(URLPATH + "/bootstrap.min.js") : Url("bootstrap.js")+"?"+T4MVCHelpers.TimestampString(URLPATH + "/bootstrap.js");
                public static readonly string jquery_2_1_1_js = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/jquery-2.1.1.min.js") ? Url("jquery-2.1.1.min.js")+"?"+T4MVCHelpers.TimestampString(URLPATH + "/jquery-2.1.1.min.js") : Url("jquery-2.1.1.js")+"?"+T4MVCHelpers.TimestampString(URLPATH + "/jquery-2.1.1.js");
                public static readonly string jquery_ui_1_10_0_js = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/jquery-ui-1.10.0.min.js") ? Url("jquery-ui-1.10.0.min.js")+"?"+T4MVCHelpers.TimestampString(URLPATH + "/jquery-ui-1.10.0.min.js") : Url("jquery-ui-1.10.0.js")+"?"+T4MVCHelpers.TimestampString(URLPATH + "/jquery-ui-1.10.0.js");
                }

    [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
    public static class Content {
        private const string URLPATH = "~/Content";
        public static string Url() { return T4MVCHelpers.ProcessVirtualPath(URLPATH); }
        public static string Url(string fileName) { return T4MVCHelpers.ProcessVirtualPath(URLPATH + "/" + fileName); }
        public static readonly string bootstrap_theme_css = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/bootstrap-theme.min.css") ? Url("bootstrap-theme.min.css")+"?"+T4MVCHelpers.TimestampString(URLPATH + "/bootstrap-theme.min.css") : Url("bootstrap-theme.css")+"?"+T4MVCHelpers.TimestampString(URLPATH + "/bootstrap-theme.css");
             
        public static readonly string bootstrap_theme_css_map = Url("bootstrap-theme.css.map")+"?"+T4MVCHelpers.TimestampString(URLPATH + "/bootstrap-theme.css.map");
        public static readonly string bootstrap_css = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/bootstrap.min.css") ? Url("bootstrap.min.css")+"?"+T4MVCHelpers.TimestampString(URLPATH + "/bootstrap.min.css") : Url("bootstrap.css")+"?"+T4MVCHelpers.TimestampString(URLPATH + "/bootstrap.css");
             
        public static readonly string bootstrap_css_map = Url("bootstrap.css.map")+"?"+T4MVCHelpers.TimestampString(URLPATH + "/bootstrap.css.map");
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public static class Sounds {
            private const string URLPATH = "~/Content/Sounds";
            public static string Url() { return T4MVCHelpers.ProcessVirtualPath(URLPATH); }
            public static string Url(string fileName) { return T4MVCHelpers.ProcessVirtualPath(URLPATH + "/" + fileName); }
            public static readonly string Error_mp3 = Url("Error.mp3")+"?"+T4MVCHelpers.TimestampString(URLPATH + "/Error.mp3");
            public static readonly string success_mp3 = Url("success.mp3")+"?"+T4MVCHelpers.TimestampString(URLPATH + "/success.mp3");
            public static readonly string warning_mp3 = Url("warning.mp3")+"?"+T4MVCHelpers.TimestampString(URLPATH + "/warning.mp3");
        }
    
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public static class themes {
            private const string URLPATH = "~/Content/themes";
            public static string Url() { return T4MVCHelpers.ProcessVirtualPath(URLPATH); }
            public static string Url(string fileName) { return T4MVCHelpers.ProcessVirtualPath(URLPATH + "/" + fileName); }
            public static readonly string Readme_txt = Url("Readme.txt")+"?"+T4MVCHelpers.TimestampString(URLPATH + "/Readme.txt");
            [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
            public static class Start1_10_0 {
                private const string URLPATH = "~/Content/themes/Start1.10.0";
                public static string Url() { return T4MVCHelpers.ProcessVirtualPath(URLPATH); }
                public static string Url(string fileName) { return T4MVCHelpers.ProcessVirtualPath(URLPATH + "/" + fileName); }
                [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
                public static class images {
                    private const string URLPATH = "~/Content/themes/Start1.10.0/images";
                    public static string Url() { return T4MVCHelpers.ProcessVirtualPath(URLPATH); }
                    public static string Url(string fileName) { return T4MVCHelpers.ProcessVirtualPath(URLPATH + "/" + fileName); }
                }
            
                public static readonly string jquery_ui_css = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/jquery-ui.min.css") ? Url("jquery-ui.min.css")+"?"+T4MVCHelpers.TimestampString(URLPATH + "/jquery-ui.min.css") : Url("jquery-ui.css")+"?"+T4MVCHelpers.TimestampString(URLPATH + "/jquery-ui.css");
                     
                public static readonly string jquery_ui_partial_css = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/jquery-ui.partial.min.css") ? Url("jquery-ui.partial.min.css")+"?"+T4MVCHelpers.TimestampString(URLPATH + "/jquery-ui.partial.min.css") : Url("jquery-ui.partial.css")+"?"+T4MVCHelpers.TimestampString(URLPATH + "/jquery-ui.partial.css");
                     
                public static readonly string jquery_ui_theme_partial_css = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/jquery.ui.theme.partial.min.css") ? Url("jquery.ui.theme.partial.min.css")+"?"+T4MVCHelpers.TimestampString(URLPATH + "/jquery.ui.theme.partial.min.css") : Url("jquery.ui.theme.partial.css")+"?"+T4MVCHelpers.TimestampString(URLPATH + "/jquery.ui.theme.partial.css");
                     
            }
        
        }
    
    }


    [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
    public static partial class MainArea {
        private const string URLPATH = "~/MainArea";
        public static string Url() { return T4MVCHelpers.ProcessVirtualPath(URLPATH); }
        public static string Url(string fileName) { return T4MVCHelpers.ProcessVirtualPath(URLPATH + "/" + fileName); }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public static class Diagnostic {
            private const string URLPATH = "~/MainArea/Diagnostic";
            public static string Url() { return T4MVCHelpers.ProcessVirtualPath(URLPATH); }
            public static string Url(string fileName) { return T4MVCHelpers.ProcessVirtualPath(URLPATH + "/" + fileName); }
        }
    
    }

    public static partial class MainArea {
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public static class Home {
            private const string URLPATH = "~/MainArea/Home";
            public static string Url() { return T4MVCHelpers.ProcessVirtualPath(URLPATH); }
            public static string Url(string fileName) { return T4MVCHelpers.ProcessVirtualPath(URLPATH + "/" + fileName); }
            public static readonly string Index_js = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/Index.min.js") ? Url("Index.min.js")+"?"+T4MVCHelpers.TimestampString(URLPATH + "/Index.min.js") : Url("Index.js")+"?"+T4MVCHelpers.TimestampString(URLPATH + "/Index.js");
                    public static readonly string Tutorial_pptx = Url("Tutorial.pptx")+"?"+T4MVCHelpers.TimestampString(URLPATH + "/Tutorial.pptx");
            public static readonly string wifi_jpg = Url("wifi.jpg")+"?"+T4MVCHelpers.TimestampString(URLPATH + "/wifi.jpg");
        }
    
    }

    public static partial class MainArea {
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public static class Logon {
            private const string URLPATH = "~/MainArea/Logon";
            public static string Url() { return T4MVCHelpers.ProcessVirtualPath(URLPATH); }
            public static string Url(string fileName) { return T4MVCHelpers.ProcessVirtualPath(URLPATH + "/" + fileName); }
        }
    
    }

    public static partial class MainArea {
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public static class SharedViews {
            private const string URLPATH = "~/MainArea/SharedViews";
            public static string Url() { return T4MVCHelpers.ProcessVirtualPath(URLPATH); }
            public static string Url(string fileName) { return T4MVCHelpers.ProcessVirtualPath(URLPATH + "/" + fileName); }
            [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
            public static class Sounds {
                private const string URLPATH = "~/MainArea/SharedViews/Sounds";
                public static string Url() { return T4MVCHelpers.ProcessVirtualPath(URLPATH); }
                public static string Url(string fileName) { return T4MVCHelpers.ProcessVirtualPath(URLPATH + "/" + fileName); }
                public static readonly string Error_wav = Url("Error.wav")+"?"+T4MVCHelpers.TimestampString(URLPATH + "/Error.wav");
                public static readonly string success_wav = Url("success.wav")+"?"+T4MVCHelpers.TimestampString(URLPATH + "/success.wav");
                public static readonly string warning_wav = Url("warning.wav")+"?"+T4MVCHelpers.TimestampString(URLPATH + "/warning.wav");
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


