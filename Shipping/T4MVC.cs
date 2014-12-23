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
public static partial class MVC_Shipping
{
    static readonly ShippingClass s_Shipping = new ShippingClass();
    public static ShippingClass Shipping { get { return s_Shipping; } }
}

namespace T4MVC
{
    [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
    public class ShippingClass
    {
        public readonly string Name = "Shipping";
        public DcmsMobile.Shipping.Areas.Shipping.Controllers.AutoCompleteController AutoComplete = new DcmsMobile.Shipping.Areas.Shipping.Controllers.T4MVC_AutoCompleteController();
        public DcmsMobile.Shipping.Areas.Shipping.Controllers.HomeController Home = new DcmsMobile.Shipping.Areas.Shipping.Controllers.T4MVC_HomeController();
        public DcmsMobile.Shipping.Areas.Shipping.Controllers.ScanToTruckController ScanToTruck = new DcmsMobile.Shipping.Areas.Shipping.Controllers.T4MVC_ScanToTruckController();
        public T4MVC.Shipping.ContentController Content = new T4MVC.Shipping.ContentController();
        public T4MVC.Shipping.ControllersController Controllers = new T4MVC.Shipping.ControllersController();
        public T4MVC.Shipping.ScriptsController Scripts = new T4MVC.Shipping.ScriptsController();
        public T4MVC.Shipping.SharedViewsController SharedViews = new T4MVC.Shipping.SharedViewsController();
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
[GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
internal partial class T4MVC_System_Web_Mvc_JsonResult : System.Web.Mvc.JsonResult, IT4MVCActionResult
{
    public T4MVC_System_Web_Mvc_JsonResult(string area, string controller, string action, string protocol = null): base()
    {
        this.InitMVCT4Result(area, controller, action, protocol);
    }
    
    public string Controller { get; set; }
    public string Action { get; set; }
    public string Protocol { get; set; }
    public RouteValueDictionary RouteValueDictionary { get; set; }
}



namespace Links_Shipping
{

    [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
    public static partial class Areas {
        private const string URLPATH = "~/Areas";
        public static string Url() { return T4MVCHelpers.ProcessVirtualPath(URLPATH); }
        public static string Url(string fileName) { return T4MVCHelpers.ProcessVirtualPath(URLPATH + "/" + fileName); }
    
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public static partial class Shipping {
            private const string URLPATH = "~/Areas/Shipping";
            public static string Url() { return T4MVCHelpers.ProcessVirtualPath(URLPATH); }
            public static string Url(string fileName) { return T4MVCHelpers.ProcessVirtualPath(URLPATH + "/" + fileName); }
            [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
            public static class Home {
                private const string URLPATH = "~/Areas/Shipping/Home";
                public static string Url() { return T4MVCHelpers.ProcessVirtualPath(URLPATH); }
                public static string Url(string fileName) { return T4MVCHelpers.ProcessVirtualPath(URLPATH + "/" + fileName); }
                public static readonly string Appointment_css_bundle = Url("Appointment.css.bundle");
                public static readonly string Appointment_css = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/Appointment.min.css") ? Url("Appointment.min.css") : Url("Appointment.css");
                     
                public static readonly string Appointment_partial_css = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/Appointment.partial.min.css") ? Url("Appointment.partial.min.css") : Url("Appointment.partial.css");
                     
                public static readonly string fullcalendar_partial_css = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/fullcalendar.partial.min.css") ? Url("fullcalendar.partial.min.css") : Url("fullcalendar.partial.css");
                     
                [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
                public static class images {
                    private const string URLPATH = "~/Areas/Shipping/Home/images";
                    public static string Url() { return T4MVCHelpers.ProcessVirtualPath(URLPATH); }
                    public static string Url(string fileName) { return T4MVCHelpers.ProcessVirtualPath(URLPATH + "/" + fileName); }
                    public static readonly string a1_gif = Url("a1.gif");
                    public static readonly string ajax_loader_gif = Url("ajax-loader.gif");
                    public static readonly string calendar_gif = Url("calendar.gif");
                }
            
            }
        
        }
    }

    public static partial class Areas {
    
        public static partial class Shipping {
            [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
            public static class ScanToTruck {
                private const string URLPATH = "~/Areas/Shipping/ScanToTruck";
                public static string Url() { return T4MVCHelpers.ProcessVirtualPath(URLPATH); }
                public static string Url(string fileName) { return T4MVCHelpers.ProcessVirtualPath(URLPATH + "/" + fileName); }
            }
        
        }
    }

    public static partial class Areas {
    
        public static partial class Shipping {
            [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
            public static class Content {
                private const string URLPATH = "~/Areas/Shipping/Content";
                public static string Url() { return T4MVCHelpers.ProcessVirtualPath(URLPATH); }
                public static string Url(string fileName) { return T4MVCHelpers.ProcessVirtualPath(URLPATH + "/" + fileName); }
            }
        
        }
    }

    public static partial class Areas {
    
        public static partial class Shipping {
            [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
            public static class Controllers {
                private const string URLPATH = "~/Areas/Shipping/Controllers";
                public static string Url() { return T4MVCHelpers.ProcessVirtualPath(URLPATH); }
                public static string Url(string fileName) { return T4MVCHelpers.ProcessVirtualPath(URLPATH + "/" + fileName); }
            }
        
        }
    }

    public static partial class Areas {
    
        public static partial class Shipping {
            [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
            public static class Scripts {
                private const string URLPATH = "~/Areas/Shipping/Scripts";
                public static string Url() { return T4MVCHelpers.ProcessVirtualPath(URLPATH); }
                public static string Url(string fileName) { return T4MVCHelpers.ProcessVirtualPath(URLPATH + "/" + fileName); }
                public static readonly string Appointment_js = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/Appointment.min.js") ? Url("Appointment.min.js") : Url("Appointment.js");
                public static readonly string Appointment_min_js_map = Url("Appointment.min.js.map");
                public static readonly string Bol_js_bundle = Url("Bol.js.bundle");
                public static readonly string Bol_js = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/Bol.min.js") ? Url("Bol.min.js") : Url("Bol.js");
                public static readonly string Bol_min_js_map = Url("Bol.min.js.map");
                public static readonly string Bol_partial_js = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/Bol.partial.min.js") ? Url("Bol.partial.min.js") : Url("Bol.partial.js");
                public static readonly string fullcalendar_js = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/fullcalendar.min.js") ? Url("fullcalendar.min.js") : Url("fullcalendar.js");
                public static readonly string Index_desktop_js = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/Index-desktop.min.js") ? Url("Index-desktop.min.js") : Url("Index-desktop.js");
                public static readonly string Index_desktop_min_js_map = Url("Index-desktop.min.js.map");
                public static readonly string Routing_js_bundle = Url("Routing.js.bundle");
                public static readonly string Routing_js = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/Routing.min.js") ? Url("Routing.min.js") : Url("Routing.js");
                public static readonly string Routing_min_js_map = Url("Routing.min.js.map");
                public static readonly string Routing_partial_js = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/Routing.partial.min.js") ? Url("Routing.partial.min.js") : Url("Routing.partial.js");
                public static readonly string selectable_partial_js = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/selectable.partial.min.js") ? Url("selectable.partial.min.js") : Url("selectable.partial.js");
                public static readonly string Unrouted_js_bundle = Url("Unrouted.js.bundle");
                public static readonly string Unrouted_js = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/Unrouted.min.js") ? Url("Unrouted.min.js") : Url("Unrouted.js");
                public static readonly string Unrouted_min_js_map = Url("Unrouted.min.js.map");
                public static readonly string Unrouted_partial_js = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/Unrouted.partial.min.js") ? Url("Unrouted.partial.min.js") : Url("Unrouted.partial.js");
            }
        
        }
    }

    public static partial class Areas {
    
        public static partial class Shipping {
            [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
            public static class SharedViews {
                private const string URLPATH = "~/Areas/Shipping/SharedViews";
                public static string Url() { return T4MVCHelpers.ProcessVirtualPath(URLPATH); }
                public static string Url(string fileName) { return T4MVCHelpers.ProcessVirtualPath(URLPATH + "/" + fileName); }
                public static readonly string GuidedTruckLoading_ppt = Url("GuidedTruckLoading.ppt");
                public static readonly string Print_css = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/Print.min.css") ? Url("Print.min.css") : Url("Print.css");
                     
                public static readonly string Shipping_all_css_bundle = Url("Shipping-all.css.bundle");
                public static readonly string Shipping_all_css = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/Shipping-all.min.css") ? Url("Shipping-all.min.css") : Url("Shipping-all.css");
                     
                public static readonly string Shipping_mobile_all_css_bundle = Url("Shipping-mobile-all.css.bundle");
                public static readonly string Shipping_mobile_all_css = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/Shipping-mobile-all.min.css") ? Url("Shipping-mobile-all.min.css") : Url("Shipping-mobile-all.css");
                     
                public static readonly string Shipping_mobile_partial_css = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/Shipping.mobile.partial.min.css") ? Url("Shipping.mobile.partial.min.css") : Url("Shipping.mobile.partial.css");
                     
                public static readonly string Shipping_partial_css = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/Shipping.partial.min.css") ? Url("Shipping.partial.min.css") : Url("Shipping.partial.css");
                     
                public static readonly string SiteMobile_partial_css = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/SiteMobile.partial.min.css") ? Url("SiteMobile.partial.min.css") : Url("SiteMobile.partial.css");
                     
                [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
                public static class Sounds {
                    private const string URLPATH = "~/Areas/Shipping/SharedViews/Sounds";
                    public static string Url() { return T4MVCHelpers.ProcessVirtualPath(URLPATH); }
                    public static string Url(string fileName) { return T4MVCHelpers.ProcessVirtualPath(URLPATH + "/" + fileName); }
                    public static readonly string Error_wav = Url("Error.wav");
                    public static readonly string success_wav = Url("success.wav");
                    public static readonly string warning_wav = Url("warning.wav");
                }
            
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


