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
public static partial class MVC_DcmsLite
{
    static readonly DcmsLiteClass s_DcmsLite = new DcmsLiteClass();
    public static DcmsLiteClass DcmsLite { get { return s_DcmsLite; } }
}

namespace T4MVC
{
    [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
    public class DcmsLiteClass
    {
        public readonly string Name = "DcmsLite";
        public DcmsMobile.DcmsLite.Areas.DcmsLite.Controllers.HomeController Home = new DcmsMobile.DcmsLite.Areas.DcmsLite.Controllers.T4MVC_HomeController();
        public DcmsMobile.DcmsLite.Areas.DcmsLite.Controllers.PickController Pick = new DcmsMobile.DcmsLite.Areas.DcmsLite.Controllers.T4MVC_PickController();
        public DcmsMobile.DcmsLite.Areas.DcmsLite.Controllers.ReceiveController Receive = new DcmsMobile.DcmsLite.Areas.DcmsLite.Controllers.T4MVC_ReceiveController();
        public DcmsMobile.DcmsLite.Areas.DcmsLite.Controllers.ShipController Ship = new DcmsMobile.DcmsLite.Areas.DcmsLite.Controllers.T4MVC_ShipController();
        public DcmsMobile.DcmsLite.Areas.DcmsLite.Controllers.ValidationController Validation = new DcmsMobile.DcmsLite.Areas.DcmsLite.Controllers.T4MVC_ValidationController();
        public T4MVC.DcmsLite.SharedController Shared = new T4MVC.DcmsLite.SharedController();
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



namespace Links_DcmsLite
{
    [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
    public static class Scripts {
        private const string URLPATH = "~/Scripts";
        public static string Url() { return T4MVCHelpers.ProcessVirtualPath(URLPATH); }
        public static string Url(string fileName) { return T4MVCHelpers.ProcessVirtualPath(URLPATH + "/" + fileName); }
        public static readonly string jquery_2_1_4_js = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/jquery-2.1.4.min.js") ? Url("jquery-2.1.4.min.js") : Url("jquery-2.1.4.js");
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
            public static class @base {
                private const string URLPATH = "~/Content/themes/base";
                public static string Url() { return T4MVCHelpers.ProcessVirtualPath(URLPATH); }
                public static string Url(string fileName) { return T4MVCHelpers.ProcessVirtualPath(URLPATH + "/" + fileName); }
                [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
                public static class images {
                    private const string URLPATH = "~/Content/themes/base/images";
                    public static string Url() { return T4MVCHelpers.ProcessVirtualPath(URLPATH); }
                    public static string Url(string fileName) { return T4MVCHelpers.ProcessVirtualPath(URLPATH + "/" + fileName); }
                    public static readonly string animated_overlay_gif = Url("animated-overlay.gif");
                }
            
                public static readonly string jquery_ui_css = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/jquery-ui.min.css") ? Url("jquery-ui.min.css") : Url("jquery-ui.css");
                public static readonly string jquery_ui_accordion_css = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/jquery.ui.accordion.min.css") ? Url("jquery.ui.accordion.min.css") : Url("jquery.ui.accordion.css");
                public static readonly string jquery_ui_all_css = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/jquery.ui.all.min.css") ? Url("jquery.ui.all.min.css") : Url("jquery.ui.all.css");
                public static readonly string jquery_ui_autocomplete_css = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/jquery.ui.autocomplete.min.css") ? Url("jquery.ui.autocomplete.min.css") : Url("jquery.ui.autocomplete.css");
                public static readonly string jquery_ui_base_css = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/jquery.ui.base.min.css") ? Url("jquery.ui.base.min.css") : Url("jquery.ui.base.css");
                public static readonly string jquery_ui_button_css = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/jquery.ui.button.min.css") ? Url("jquery.ui.button.min.css") : Url("jquery.ui.button.css");
                public static readonly string jquery_ui_core_css = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/jquery.ui.core.min.css") ? Url("jquery.ui.core.min.css") : Url("jquery.ui.core.css");
                public static readonly string jquery_ui_datepicker_css = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/jquery.ui.datepicker.min.css") ? Url("jquery.ui.datepicker.min.css") : Url("jquery.ui.datepicker.css");
                public static readonly string jquery_ui_dialog_css = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/jquery.ui.dialog.min.css") ? Url("jquery.ui.dialog.min.css") : Url("jquery.ui.dialog.css");
                public static readonly string jquery_ui_menu_css = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/jquery.ui.menu.min.css") ? Url("jquery.ui.menu.min.css") : Url("jquery.ui.menu.css");
                public static readonly string jquery_ui_progressbar_css = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/jquery.ui.progressbar.min.css") ? Url("jquery.ui.progressbar.min.css") : Url("jquery.ui.progressbar.css");
                public static readonly string jquery_ui_resizable_css = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/jquery.ui.resizable.min.css") ? Url("jquery.ui.resizable.min.css") : Url("jquery.ui.resizable.css");
                public static readonly string jquery_ui_selectable_css = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/jquery.ui.selectable.min.css") ? Url("jquery.ui.selectable.min.css") : Url("jquery.ui.selectable.css");
                public static readonly string jquery_ui_slider_css = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/jquery.ui.slider.min.css") ? Url("jquery.ui.slider.min.css") : Url("jquery.ui.slider.css");
                public static readonly string jquery_ui_spinner_css = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/jquery.ui.spinner.min.css") ? Url("jquery.ui.spinner.min.css") : Url("jquery.ui.spinner.css");
                public static readonly string jquery_ui_tabs_css = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/jquery.ui.tabs.min.css") ? Url("jquery.ui.tabs.min.css") : Url("jquery.ui.tabs.css");
                public static readonly string jquery_ui_theme_css = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/jquery.ui.theme.min.css") ? Url("jquery.ui.theme.min.css") : Url("jquery.ui.theme.css");
                public static readonly string jquery_ui_tooltip_css = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/jquery.ui.tooltip.min.css") ? Url("jquery.ui.tooltip.min.css") : Url("jquery.ui.tooltip.css");
                [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
                public static class minified {
                    private const string URLPATH = "~/Content/themes/base/minified";
                    public static string Url() { return T4MVCHelpers.ProcessVirtualPath(URLPATH); }
                    public static string Url(string fileName) { return T4MVCHelpers.ProcessVirtualPath(URLPATH + "/" + fileName); }
                    [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
                    public static class images {
                        private const string URLPATH = "~/Content/themes/base/minified/images";
                        public static string Url() { return T4MVCHelpers.ProcessVirtualPath(URLPATH); }
                        public static string Url(string fileName) { return T4MVCHelpers.ProcessVirtualPath(URLPATH + "/" + fileName); }
                        public static readonly string animated_overlay_gif = Url("animated-overlay.gif");
                    }
                
                }
            
            }
        
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
        public static partial class DcmsLite {
            private const string URLPATH = "~/Areas/DcmsLite";
            public static string Url() { return T4MVCHelpers.ProcessVirtualPath(URLPATH); }
            public static string Url(string fileName) { return T4MVCHelpers.ProcessVirtualPath(URLPATH + "/" + fileName); }
            [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
            public static class Scripts {
                private const string URLPATH = "~/Areas/DcmsLite/Scripts";
                public static string Url() { return T4MVCHelpers.ProcessVirtualPath(URLPATH); }
                public static string Url(string fileName) { return T4MVCHelpers.ProcessVirtualPath(URLPATH + "/" + fileName); }
                public static readonly string Batch_js_bundle = Url("Batch.js.bundle");
                public static readonly string Batch_js = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/Batch.min.js") ? Url("Batch.min.js") : Url("Batch.js");
                public static readonly string Batch_min_js_map = Url("Batch.min.js.map");
                public static readonly string Batch_partial_js = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/Batch.partial.min.js") ? Url("Batch.partial.min.js") : Url("Batch.partial.js");
                public static readonly string layout_js = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/layout.min.js") ? Url("layout.min.js") : Url("layout.js");
                public static readonly string layout_min_js_map = Url("layout.min.js.map");
                public static readonly string Receive_js_bundle = Url("Receive.js.bundle");
                public static readonly string Receive_js = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/Receive.min.js") ? Url("Receive.min.js") : Url("Receive.js");
                public static readonly string Receive_min_js_map = Url("Receive.min.js.map");
                public static readonly string Receive_partial_js = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/Receive.partial.min.js") ? Url("Receive.partial.min.js") : Url("Receive.partial.js");
                public static readonly string Wave_js_bundle = Url("Wave.js.bundle");
                public static readonly string Wave_js = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/Wave.min.js") ? Url("Wave.min.js") : Url("Wave.js");
                public static readonly string Wave_min_js_map = Url("Wave.min.js.map");
                public static readonly string Wave_partial_js = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/Wave.partial.min.js") ? Url("Wave.partial.min.js") : Url("Wave.partial.js");
            }
        
        }
    }

    public static partial class Areas {
    
        public static partial class DcmsLite {
            [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
            public static class Content {
                private const string URLPATH = "~/Areas/DcmsLite/Content";
                public static string Url() { return T4MVCHelpers.ProcessVirtualPath(URLPATH); }
                public static string Url(string fileName) { return T4MVCHelpers.ProcessVirtualPath(URLPATH + "/" + fileName); }
                public static readonly string _3PLDCMS_V3_pptx = Url("3PLDCMS V3.pptx");
                public static readonly string DcmsLite_all_css_bundle = Url("DcmsLite-all.css.bundle");
                public static readonly string DcmsLite_all_css = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/DcmsLite-all.min.css") ? Url("DcmsLite-all.min.css") : Url("DcmsLite-all.css");
                public static readonly string DcmsLite_partial_css = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/DcmsLite.partial.min.css") ? Url("DcmsLite.partial.min.css") : Url("DcmsLite.partial.css");
                [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
                public static class images {
                    private const string URLPATH = "~/Areas/DcmsLite/Content/images";
                    public static string Url() { return T4MVCHelpers.ProcessVirtualPath(URLPATH); }
                    public static string Url(string fileName) { return T4MVCHelpers.ProcessVirtualPath(URLPATH + "/" + fileName); }
                    public static readonly string a1_gif = Url("a1.gif");
                    public static readonly string ajax_loader_gif = Url("ajax-loader.gif");
                }
            
                [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
                public static class Sounds {
                    private const string URLPATH = "~/Areas/DcmsLite/Content/Sounds";
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
        public static partial class Scripts 
        {
            public static class Assets
            {
                public const string jquery_2_1_4_js = "~/Scripts/jquery-2.1.4.js"; 
                public const string jquery_ui_1_10_0_js = "~/Scripts/jquery-ui-1.10.0.js"; 
                public const string jquery_validate_js = "~/Scripts/jquery.validate.js"; 
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
                    public static partial class minified 
                    {
                        public static partial class images 
                        {
                            public static class Assets
                            {
                            }
                        }
                        public static class Assets
                        {
                        }
                    }
                    public static class Assets
                    {
                        public const string jquery_ui_css = "~/Content/themes/base/jquery-ui.css";
                        public const string jquery_ui_accordion_css = "~/Content/themes/base/jquery.ui.accordion.css";
                        public const string jquery_ui_all_css = "~/Content/themes/base/jquery.ui.all.css";
                        public const string jquery_ui_autocomplete_css = "~/Content/themes/base/jquery.ui.autocomplete.css";
                        public const string jquery_ui_base_css = "~/Content/themes/base/jquery.ui.base.css";
                        public const string jquery_ui_button_css = "~/Content/themes/base/jquery.ui.button.css";
                        public const string jquery_ui_core_css = "~/Content/themes/base/jquery.ui.core.css";
                        public const string jquery_ui_datepicker_css = "~/Content/themes/base/jquery.ui.datepicker.css";
                        public const string jquery_ui_dialog_css = "~/Content/themes/base/jquery.ui.dialog.css";
                        public const string jquery_ui_menu_css = "~/Content/themes/base/jquery.ui.menu.css";
                        public const string jquery_ui_progressbar_css = "~/Content/themes/base/jquery.ui.progressbar.css";
                        public const string jquery_ui_resizable_css = "~/Content/themes/base/jquery.ui.resizable.css";
                        public const string jquery_ui_selectable_css = "~/Content/themes/base/jquery.ui.selectable.css";
                        public const string jquery_ui_slider_css = "~/Content/themes/base/jquery.ui.slider.css";
                        public const string jquery_ui_spinner_css = "~/Content/themes/base/jquery.ui.spinner.css";
                        public const string jquery_ui_tabs_css = "~/Content/themes/base/jquery.ui.tabs.css";
                        public const string jquery_ui_theme_css = "~/Content/themes/base/jquery.ui.theme.css";
                        public const string jquery_ui_tooltip_css = "~/Content/themes/base/jquery.ui.tooltip.css";
                    }
                }
                public static partial class Start1_10_0 
                {
                    public static class Assets
                    {
                        public const string jquery_ui_css = "~/Content/themes/Start1.10.0/jquery-ui.css";
                    }
                }
                public static class Assets
                {
                }
            }
            public static class Assets
            {
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


