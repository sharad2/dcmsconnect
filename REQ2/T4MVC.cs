﻿// <auto-generated />
// This file was generated by a T4 template.
// Don't change it directly as your change would get overwritten.  Instead, make changes
// to the .tt file (i.e. the T4 template) and save it to regenerate this file.

// Make sure the compiler doesn't complain about missing Xml comments and CLS compliance
// 0108: suppress "Foo hides inherited member Foo. Use the new keyword if hiding was intended." when a controller and its abstract parent are both processed
// 0114: suppress "Foo.BarController.Baz()' hides inherited member 'Qux.BarController.Baz()'. To make the current member override that implementation, add the override keyword. Otherwise add the new keyword." when an action (with an argument) overrides an action in a parent controller
#pragma warning disable 1591, 3008, 3009, 0108, 0114
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
public static partial class MVC_REQ2
{
    static readonly REQ2Class s_REQ2 = new REQ2Class();
    public static REQ2Class REQ2 { get { return s_REQ2; } }
}

namespace T4MVC
{
    [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
    public class REQ2Class
    {
        public readonly string Name = "REQ2";
        public DcmsMobile.REQ2.Areas.REQ2.AutoComplete.AutoCompleteController AutoComplete = new DcmsMobile.REQ2.Areas.REQ2.AutoComplete.T4MVC_AutoCompleteController();
        public DcmsMobile.REQ2.Areas.REQ2.Home.HomeController Home = new DcmsMobile.REQ2.Areas.REQ2.Home.T4MVC_HomeController();
        public T4MVC.REQ2.SharedViewsController SharedViews = new T4MVC.REQ2.SharedViewsController();
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



namespace Links_REQ2
{
    [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
    public static class Scripts {
        private const string URLPATH = "~/Scripts";
        public static string Url() { return T4MVCHelpers.ProcessVirtualPath(URLPATH); }
        public static string Url(string fileName) { return T4MVCHelpers.ProcessVirtualPath(URLPATH + "/" + fileName); }
        public static readonly string bootstrap_js = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/bootstrap.min.js") ? Url("bootstrap.min.js") : Url("bootstrap.js");
        public static readonly string jquery_3_3_1_js = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/jquery-3.3.1.min.js") ? Url("jquery-3.3.1.min.js") : Url("jquery-3.3.1.js");
        public static readonly string jquery_3_3_1_slim_js = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/jquery-3.3.1.slim.min.js") ? Url("jquery-3.3.1.slim.min.js") : Url("jquery-3.3.1.slim.js");
        public static readonly string jquery_ui_1_11_4_js = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/jquery-ui-1.11.4.min.js") ? Url("jquery-ui-1.11.4.min.js") : Url("jquery-ui-1.11.4.js");
        public static readonly string jquery_validate_js = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/jquery.validate.min.js") ? Url("jquery.validate.min.js") : Url("jquery.validate.js");
        public static readonly string jquery_validate_unobtrusive_bootstrap_js = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/jquery.validate.unobtrusive.bootstrap.min.js") ? Url("jquery.validate.unobtrusive.bootstrap.min.js") : Url("jquery.validate.unobtrusive.bootstrap.js");
        public static readonly string jquery_validate_unobtrusive_js = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/jquery.validate.unobtrusive.min.js") ? Url("jquery.validate.unobtrusive.min.js") : Url("jquery.validate.unobtrusive.js");
    }

    [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
    public static class Content {
        private const string URLPATH = "~/Content";
        public static string Url() { return T4MVCHelpers.ProcessVirtualPath(URLPATH); }
        public static string Url(string fileName) { return T4MVCHelpers.ProcessVirtualPath(URLPATH + "/" + fileName); }
        public static readonly string bootstrap_theme_css = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/bootstrap-theme.min.css") ? Url("bootstrap-theme.min.css") : Url("bootstrap-theme.css");
        public static readonly string bootstrap_css = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/bootstrap.min.css") ? Url("bootstrap.min.css") : Url("bootstrap.css");
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public static class themes {
            private const string URLPATH = "~/Content/themes";
            public static string Url() { return T4MVCHelpers.ProcessVirtualPath(URLPATH); }
            public static string Url(string fileName) { return T4MVCHelpers.ProcessVirtualPath(URLPATH + "/" + fileName); }
            [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
            public static class @base {
                private const string URLPATH = "~/Content/themes/base";
                public static string Url() { return T4MVCHelpers.ProcessVirtualPath(URLPATH); }
                public static string Url(string fileName) { return T4MVCHelpers.ProcessVirtualPath(URLPATH + "/" + fileName); }
                public static readonly string accordion_css = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/accordion.min.css") ? Url("accordion.min.css") : Url("accordion.css");
                public static readonly string all_css = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/all.min.css") ? Url("all.min.css") : Url("all.css");
                public static readonly string autocomplete_css = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/autocomplete.min.css") ? Url("autocomplete.min.css") : Url("autocomplete.css");
                public static readonly string base_css = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/base.min.css") ? Url("base.min.css") : Url("base.css");
                public static readonly string button_css = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/button.min.css") ? Url("button.min.css") : Url("button.css");
                public static readonly string core_css = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/core.min.css") ? Url("core.min.css") : Url("core.css");
                public static readonly string datepicker_css = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/datepicker.min.css") ? Url("datepicker.min.css") : Url("datepicker.css");
                public static readonly string dialog_css = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/dialog.min.css") ? Url("dialog.min.css") : Url("dialog.css");
                public static readonly string draggable_css = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/draggable.min.css") ? Url("draggable.min.css") : Url("draggable.css");
                [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
                public static class images {
                    private const string URLPATH = "~/Content/themes/base/images";
                    public static string Url() { return T4MVCHelpers.ProcessVirtualPath(URLPATH); }
                    public static string Url(string fileName) { return T4MVCHelpers.ProcessVirtualPath(URLPATH + "/" + fileName); }
                }
            
                public static readonly string menu_css = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/menu.min.css") ? Url("menu.min.css") : Url("menu.css");
                public static readonly string progressbar_css = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/progressbar.min.css") ? Url("progressbar.min.css") : Url("progressbar.css");
                public static readonly string resizable_css = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/resizable.min.css") ? Url("resizable.min.css") : Url("resizable.css");
                public static readonly string selectable_css = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/selectable.min.css") ? Url("selectable.min.css") : Url("selectable.css");
                public static readonly string selectmenu_css = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/selectmenu.min.css") ? Url("selectmenu.min.css") : Url("selectmenu.css");
                public static readonly string slider_css = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/slider.min.css") ? Url("slider.min.css") : Url("slider.css");
                public static readonly string sortable_css = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/sortable.min.css") ? Url("sortable.min.css") : Url("sortable.css");
                public static readonly string spinner_css = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/spinner.min.css") ? Url("spinner.min.css") : Url("spinner.css");
                public static readonly string tabs_css = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/tabs.min.css") ? Url("tabs.min.css") : Url("tabs.css");
                public static readonly string theme_css = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/theme.min.css") ? Url("theme.min.css") : Url("theme.css");
                public static readonly string tooltip_css = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/tooltip.min.css") ? Url("tooltip.min.css") : Url("tooltip.css");
            }
        
        }
    
    }


    [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
    public static partial class Areas {
        private const string URLPATH = "~/Areas";
        public static string Url() { return T4MVCHelpers.ProcessVirtualPath(URLPATH); }
        public static string Url(string fileName) { return T4MVCHelpers.ProcessVirtualPath(URLPATH + "/" + fileName); }
    
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public static partial class REQ2 {
            private const string URLPATH = "~/Areas/REQ2";
            public static string Url() { return T4MVCHelpers.ProcessVirtualPath(URLPATH); }
            public static string Url(string fileName) { return T4MVCHelpers.ProcessVirtualPath(URLPATH + "/" + fileName); }
            [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
            public static class AutoComplete {
                private const string URLPATH = "~/Areas/REQ2/AutoComplete";
                public static string Url() { return T4MVCHelpers.ProcessVirtualPath(URLPATH); }
                public static string Url(string fileName) { return T4MVCHelpers.ProcessVirtualPath(URLPATH + "/" + fileName); }
                public static readonly string Autocomplete_partial_css = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/Autocomplete.partial.min.css") ? Url("Autocomplete.partial.min.css") : Url("Autocomplete.partial.css");
            }
        
        }
    }

    public static partial class Areas {
    
        public static partial class REQ2 {
            [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
            public static class Home {
                private const string URLPATH = "~/Areas/REQ2/Home";
                public static string Url() { return T4MVCHelpers.ProcessVirtualPath(URLPATH); }
                public static string Url(string fileName) { return T4MVCHelpers.ProcessVirtualPath(URLPATH + "/" + fileName); }
                [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
                public static class images {
                    private const string URLPATH = "~/Areas/REQ2/Home/images";
                    public static string Url() { return T4MVCHelpers.ProcessVirtualPath(URLPATH); }
                    public static string Url(string fileName) { return T4MVCHelpers.ProcessVirtualPath(URLPATH + "/" + fileName); }
                    public static readonly string a1_gif = Url("a1.gif");
                    public static readonly string ajax_loader_gif = Url("ajax-loader.gif");
                    public static readonly string calendar_gif = Url("calendar.gif");
                }
            
                public static readonly string ManageSku_all_js = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/ManageSku-all.min.js") ? Url("ManageSku-all.min.js") : Url("ManageSku-all.js");
                public static readonly string PropertyEditor_js = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/PropertyEditor.min.js") ? Url("PropertyEditor.min.js") : Url("PropertyEditor.js");
                public static readonly string RecentRequest_all_js = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/RecentRequest-all.min.js") ? Url("RecentRequest-all.min.js") : Url("RecentRequest-all.js");
                public static readonly string REQ2_mobile_partial_css = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/REQ2.mobile.partial.min.css") ? Url("REQ2.mobile.partial.min.css") : Url("REQ2.mobile.partial.css");
                public static readonly string REQ2_partial_css = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/REQ2.partial.min.css") ? Url("REQ2.partial.min.css") : Url("REQ2.partial.css");
                public static readonly string standardized_partial_css = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/standardized.partial.min.css") ? Url("standardized.partial.min.css") : Url("standardized.partial.css");
                public static readonly string Stylesheets_chirp_config = Url("Stylesheets.chirp.config");
                public static readonly string REQ2_all_css = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/REQ2-all.min.css") ? Url("REQ2-all.min.css") : Url("REQ2-all.css");
            }
        
        }
    }

    public static partial class Areas {
    
        public static partial class REQ2 {
            [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
            public static class SharedViews {
                private const string URLPATH = "~/Areas/REQ2/SharedViews";
                public static string Url() { return T4MVCHelpers.ProcessVirtualPath(URLPATH); }
                public static string Url(string fileName) { return T4MVCHelpers.ProcessVirtualPath(URLPATH + "/" + fileName); }
            }
        
        }
    }
    
    [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
    public static partial class Bundles
    {
        public static partial class Scripts 
        {
            public static class Assets
            {
                public const string bootstrap_js = "~/Scripts/bootstrap.js"; 
                public const string jquery_3_3_1_js = "~/Scripts/jquery-3.3.1.js"; 
                public const string jquery_3_3_1_slim_js = "~/Scripts/jquery-3.3.1.slim.js"; 
                public const string jquery_ui_1_11_4_js = "~/Scripts/jquery-ui-1.11.4.js"; 
                public const string jquery_validate_js = "~/Scripts/jquery.validate.js"; 
                public const string jquery_validate_unobtrusive_bootstrap_js = "~/Scripts/jquery.validate.unobtrusive.bootstrap.js"; 
                public const string jquery_validate_unobtrusive_js = "~/Scripts/jquery.validate.unobtrusive.js"; 
            }
        }
        public static partial class Content 
        {
            public static partial class themes 
            {
                public static partial class @base 
                {
                    public static partial class images 
                    {
                        public static class Assets
                        {
                        }
                    }
                    public static class Assets
                    {
                        public const string accordion_css = "~/Content/themes/base/accordion.css";
                        public const string all_css = "~/Content/themes/base/all.css";
                        public const string autocomplete_css = "~/Content/themes/base/autocomplete.css";
                        public const string base_css = "~/Content/themes/base/base.css";
                        public const string button_css = "~/Content/themes/base/button.css";
                        public const string core_css = "~/Content/themes/base/core.css";
                        public const string datepicker_css = "~/Content/themes/base/datepicker.css";
                        public const string dialog_css = "~/Content/themes/base/dialog.css";
                        public const string draggable_css = "~/Content/themes/base/draggable.css";
                        public const string menu_css = "~/Content/themes/base/menu.css";
                        public const string progressbar_css = "~/Content/themes/base/progressbar.css";
                        public const string resizable_css = "~/Content/themes/base/resizable.css";
                        public const string selectable_css = "~/Content/themes/base/selectable.css";
                        public const string selectmenu_css = "~/Content/themes/base/selectmenu.css";
                        public const string slider_css = "~/Content/themes/base/slider.css";
                        public const string sortable_css = "~/Content/themes/base/sortable.css";
                        public const string spinner_css = "~/Content/themes/base/spinner.css";
                        public const string tabs_css = "~/Content/themes/base/tabs.css";
                        public const string theme_css = "~/Content/themes/base/theme.css";
                        public const string tooltip_css = "~/Content/themes/base/tooltip.css";
                    }
                }
                public static class Assets
                {
                }
            }
            public static class Assets
            {
                public const string bootstrap_theme_css = "~/Content/bootstrap-theme.css";
                public const string bootstrap_css = "~/Content/bootstrap.css";
            }
        }
        public static partial class Areas 
        {
            public static partial class REQ2 
            {
                public static partial class AutoComplete 
                {
                    public static class Assets
                    {
                        public const string Autocomplete_partial_css = "~/Areas/REQ2/AutoComplete/Autocomplete.partial.css";
                    }
                }
            }
        }
        public static partial class Areas 
        {
            public static partial class REQ2 
            {
                public static partial class Home 
                {
                    public static partial class images 
                    {
                        public static class Assets
                        {
                        }
                    }
                    public static class Assets
                    {
                        public const string ManageSku_all_js = "~/Areas/REQ2/Home/ManageSku-all.js"; 
                        public const string RecentRequest_all_js = "~/Areas/REQ2/Home/RecentRequest-all.js"; 
                        public const string REQ2_mobile_partial_css = "~/Areas/REQ2/Home/REQ2.mobile.partial.css";
                        public const string REQ2_partial_css = "~/Areas/REQ2/Home/REQ2.partial.css";
                        public const string standardized_partial_css = "~/Areas/REQ2/Home/standardized.partial.css";
                    }
                }
            }
        }
        public static partial class Areas 
        {
            public static partial class REQ2 
            {
                public static partial class SharedViews 
                {
                    public static class Assets
                    {
                    }
                }
            }
        }
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
#pragma warning restore 1591, 3008, 3009, 0108, 0114


