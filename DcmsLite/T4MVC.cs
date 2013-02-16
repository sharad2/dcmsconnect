﻿// <auto-generated />
// This file was generated by a T4 template.
// Don't change it directly as your change would get overwritten.  Instead, make changes
// to the .tt file (i.e. the T4 template) and save it to regenerate this file.

// Make sure the compiler doesn't complain about missing Xml comments
#pragma warning disable 1591
#region T4MVC

using System;
using System.Diagnostics;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using System.Web.Mvc.Html;
using System.Web.Routing;
using T4MVC;

[GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
public static class MVC_DcmsLite {
    static readonly DcmsLiteClass s_DcmsLite = new DcmsLiteClass();
    public static DcmsLiteClass DcmsLite { get { return s_DcmsLite; } }
}

namespace T4MVC {
    [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
    public class DcmsLiteClass {
        public readonly string Name = "DcmsLite";
        public DcmsMobile.DcmsLite.Areas.DcmsLite.Controllers.HomeController Home = new DcmsMobile.DcmsLite.Areas.DcmsLite.Controllers.T4MVC_HomeController();
        public DcmsMobile.DcmsLite.Areas.DcmsLite.Controllers.PickController Pick = new DcmsMobile.DcmsLite.Areas.DcmsLite.Controllers.T4MVC_PickController();
        public DcmsMobile.DcmsLite.Areas.DcmsLite.Controllers.ReceiveController Receive = new DcmsMobile.DcmsLite.Areas.DcmsLite.Controllers.T4MVC_ReceiveController();
        public DcmsMobile.DcmsLite.Areas.DcmsLite.Controllers.ValidationController Validation = new DcmsMobile.DcmsLite.Areas.DcmsLite.Controllers.T4MVC_ValidationController();
        public T4MVC.DcmsLite.SharedController Shared = new T4MVC.DcmsLite.SharedController();
    }
}

  

  

[GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
public class T4MVC_ActionResult : System.Web.Mvc.ActionResult, IT4MVCActionResult {
    public T4MVC_ActionResult(string area, string controller, string action, string protocol = null): base()  {
        this.InitMVCT4Result(area, controller, action, protocol);
    }
     
    public override void ExecuteResult(System.Web.Mvc.ControllerContext context) { }
    
    public string Controller { get; set; }
    public string Action { get; set; }
    public string Protocol { get; set; }
    public RouteValueDictionary RouteValueDictionary { get; set; }
}



namespace Links_DcmsLite {
    [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
    public static class Scripts {
        private const string URLPATH = "~/Scripts";
        public static string Url() { return T4MVCHelpers.ProcessVirtualPath(URLPATH); }
        public static string Url(string fileName) { return T4MVCHelpers.ProcessVirtualPath(URLPATH + "/" + fileName); }
        public static readonly string _references_js = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/_references.min.js") ? Url("_references.min.js") : Url("_references.js");
                      
        public static readonly string jquery_1_8_2_intellisense_js = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/jquery-1.8.2.intellisense.min.js") ? Url("jquery-1.8.2.intellisense.min.js") : Url("jquery-1.8.2.intellisense.js");
                      
        public static readonly string jquery_1_8_2_js = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/jquery-1.8.2.min.js") ? Url("jquery-1.8.2.min.js") : Url("jquery-1.8.2.js");
                      
        public static readonly string jquery_ui_1_9_0_js = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/jquery-ui-1.9.0.min.js") ? Url("jquery-ui-1.9.0.min.js") : Url("jquery-ui-1.9.0.js");
                      
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
            public static class start {
                private const string URLPATH = "~/Content/themes/start";
                public static string Url() { return T4MVCHelpers.ProcessVirtualPath(URLPATH); }
                public static string Url(string fileName) { return T4MVCHelpers.ProcessVirtualPath(URLPATH + "/" + fileName); }
                [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
                public static class images {
                    private const string URLPATH = "~/Content/themes/start/images";
                    public static string Url() { return T4MVCHelpers.ProcessVirtualPath(URLPATH); }
                    public static string Url(string fileName) { return T4MVCHelpers.ProcessVirtualPath(URLPATH + "/" + fileName); }
                }
            
                public static readonly string jquery_ui_1_9_1_css = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/jquery-ui-1.9.1.min.css") ? Url("jquery-ui-1.9.1.min.css") : Url("jquery-ui-1.9.1.css");
                     
            }
        
        }
    
    }

    [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
    public static class Areas {
        private const string URLPATH = "~/Areas";
        public static string Url() { return T4MVCHelpers.ProcessVirtualPath(URLPATH); }
        public static string Url(string fileName) { return T4MVCHelpers.ProcessVirtualPath(URLPATH + "/" + fileName); }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public static class DcmsLite {
            private const string URLPATH = "~/Areas/DcmsLite";
            public static string Url() { return T4MVCHelpers.ProcessVirtualPath(URLPATH); }
            public static string Url(string fileName) { return T4MVCHelpers.ProcessVirtualPath(URLPATH + "/" + fileName); }
            [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
            public static class Content {
                private const string URLPATH = "~/Areas/DcmsLite/Content";
                public static string Url() { return T4MVCHelpers.ProcessVirtualPath(URLPATH); }
                public static string Url(string fileName) { return T4MVCHelpers.ProcessVirtualPath(URLPATH + "/" + fileName); }
                public static readonly string _3PLDCMS_V3_pptx = Url("3PLDCMS V3.pptx");
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
            
                public static readonly string DcmsLite_all_css = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/DcmsLite-all.min.css") ? Url("DcmsLite-all.min.css") : Url("DcmsLite-all.css");
                     
            }
        
            [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
            public static class Controllers {
                private const string URLPATH = "~/Areas/DcmsLite/Controllers";
                public static string Url() { return T4MVCHelpers.ProcessVirtualPath(URLPATH); }
                public static string Url(string fileName) { return T4MVCHelpers.ProcessVirtualPath(URLPATH + "/" + fileName); }
            }
        
            [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
            public static class Scripts {
                private const string URLPATH = "~/Areas/DcmsLite/Scripts";
                public static string Url() { return T4MVCHelpers.ProcessVirtualPath(URLPATH); }
                public static string Url(string fileName) { return T4MVCHelpers.ProcessVirtualPath(URLPATH + "/" + fileName); }
                public static readonly string Batch_js = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/Batch.min.js") ? Url("Batch.min.js") : Url("Batch.js");
                              
                public static readonly string layout_js = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/layout.min.js") ? Url("layout.min.js") : Url("layout.js");
                              
                public static readonly string Receive_js = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/Receive.min.js") ? Url("Receive.min.js") : Url("Receive.js");
                              
                public static readonly string Wave_js = T4MVCHelpers.IsProduction() && T4Extensions.FileExists(URLPATH + "/Wave.min.js") ? Url("Wave.min.js") : Url("Wave.js");
                              
            }
        
            [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
            public static class Views {
                private const string URLPATH = "~/Areas/DcmsLite/Views";
                public static string Url() { return T4MVCHelpers.ProcessVirtualPath(URLPATH); }
                public static string Url(string fileName) { return T4MVCHelpers.ProcessVirtualPath(URLPATH + "/" + fileName); }
                public static readonly string _ViewStart_cshtml = Url("_ViewStart.cshtml");
                [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
                public static class Home {
                    private const string URLPATH = "~/Areas/DcmsLite/Views/Home";
                    public static string Url() { return T4MVCHelpers.ProcessVirtualPath(URLPATH); }
                    public static string Url(string fileName) { return T4MVCHelpers.ProcessVirtualPath(URLPATH + "/" + fileName); }
                    public static readonly string Index_cshtml = Url("Index.cshtml");
                }
            
                [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
                public static class Pick {
                    private const string URLPATH = "~/Areas/DcmsLite/Views/Pick";
                    public static string Url() { return T4MVCHelpers.ProcessVirtualPath(URLPATH); }
                    public static string Url(string fileName) { return T4MVCHelpers.ProcessVirtualPath(URLPATH + "/" + fileName); }
                    public static readonly string Batch_cshtml = Url("Batch.cshtml");
                    public static readonly string Index_cshtml = Url("Index.cshtml");
                    public static readonly string Wave_cshtml = Url("Wave.cshtml");
                }
            
                [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
                public static class Receive {
                    private const string URLPATH = "~/Areas/DcmsLite/Views/Receive";
                    public static string Url() { return T4MVCHelpers.ProcessVirtualPath(URLPATH); }
                    public static string Url(string fileName) { return T4MVCHelpers.ProcessVirtualPath(URLPATH + "/" + fileName); }
                    public static readonly string Index_cshtml = Url("Index.cshtml");
                }
            
                [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
                public static class Shared {
                    private const string URLPATH = "~/Areas/DcmsLite/Views/Shared";
                    public static string Url() { return T4MVCHelpers.ProcessVirtualPath(URLPATH); }
                    public static string Url(string fileName) { return T4MVCHelpers.ProcessVirtualPath(URLPATH + "/" + fileName); }
                    public static readonly string _layoutDcmsLite_cshtml = Url("_layoutDcmsLite.cshtml");
                    public static readonly string _soundPartial_cshtml = Url("_soundPartial.cshtml");
                }
            
                [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
                public static class Validation {
                    private const string URLPATH = "~/Areas/DcmsLite/Views/Validation";
                    public static string Url() { return T4MVCHelpers.ProcessVirtualPath(URLPATH); }
                    public static string Url(string fileName) { return T4MVCHelpers.ProcessVirtualPath(URLPATH + "/" + fileName); }
                    public static readonly string Index_cshtml = Url("Index.cshtml");
                }
            
            }
        
        }
    
    }

    [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
    public static partial class bundles {
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public static partial class scripts {}
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public static partial class styles {}
    }
}





#endregion T4MVC
#pragma warning restore 1591


