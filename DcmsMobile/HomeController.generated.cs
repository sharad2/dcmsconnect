// <auto-generated />
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
namespace DcmsMobile.Controllers {
    public partial class HomeController {
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public HomeController() { }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        protected HomeController(Dummy d) { }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        protected RedirectToRouteResult RedirectToAction(ActionResult result) {
            var callInfo = result.GetT4MVCResult();
            return RedirectToRoute(callInfo.RouteValueDictionary);
        }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        protected RedirectToRouteResult RedirectToActionPermanent(ActionResult result) {
            var callInfo = result.GetT4MVCResult();
            return RedirectToRoutePermanent(callInfo.RouteValueDictionary);
        }

        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public System.Web.Mvc.ActionResult RcIndex() {
            return new T4MVC_ActionResult(Area, Name, ActionNames.RcIndex);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public System.Web.Mvc.ActionResult AcceptChoice() {
            return new T4MVC_ActionResult(Area, Name, ActionNames.AcceptChoice);
        }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public HomeController Actions { get { return MVC_DcmsMobile.Home; } }
        [GeneratedCode("T4MVC", "2.0")]
        public readonly string Area = "";
        [GeneratedCode("T4MVC", "2.0")]
        public readonly string Name = "Home";
        [GeneratedCode("T4MVC", "2.0")]
        public const string NameConst = "Home";

        static readonly ActionNamesClass s_actions = new ActionNamesClass();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionNamesClass ActionNames { get { return s_actions; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionNamesClass {
            public readonly string Index = "Index";
            public readonly string Categorized = "Categorized";
            public readonly string RcIndex = "Index";
            public readonly string AcceptChoice = "AcceptChoice";
        }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionNameConstants {
            public const string Index = "Index";
            public const string Categorized = "Categorized";
            public const string RcIndex = "Index";
            public const string AcceptChoice = "AcceptChoice";
        }


        static readonly ActionParamsClass_RcIndex s_params_RcIndex = new ActionParamsClass_RcIndex();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_RcIndex RcIndexParams { get { return s_params_RcIndex; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_RcIndex {
            public readonly string returnUrl = "returnUrl";
        }
        static readonly ActionParamsClass_AcceptChoice s_params_AcceptChoice = new ActionParamsClass_AcceptChoice();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_AcceptChoice AcceptChoiceParams { get { return s_params_AcceptChoice; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_AcceptChoice {
            public readonly string choice = "choice";
            public readonly string isMobile = "isMobile";
        }
        static readonly ViewNames s_views = new ViewNames();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ViewNames Views { get { return s_views; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ViewNames {
            public readonly string Categorized = "Categorized";
            public readonly string Launcher = "Launcher";
            public readonly string Launcher_Mobile = "Launcher.Mobile";
            public readonly string RcLauncher = "RcLauncher";
            public readonly string RcLauncher_Mobile = "RcLauncher.Mobile";
            public readonly string Search = "Search";
            public readonly string Search_mobile = "Search.mobile";
        }
    }

    [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
    public class T4MVC_HomeController: DcmsMobile.Controllers.HomeController {
        public T4MVC_HomeController() : base(Dummy.Instance) { }

        public override System.Web.Mvc.ActionResult Index() {
            var callInfo = new T4MVC_ActionResult(Area, Name, ActionNames.Index);
            return callInfo;
        }

        public override System.Web.Mvc.ActionResult Categorized() {
            var callInfo = new T4MVC_ActionResult(Area, Name, ActionNames.Categorized);
            return callInfo;
        }

        public override System.Web.Mvc.ActionResult RcIndex(string returnUrl) {
            var callInfo = new T4MVC_ActionResult(Area, Name, ActionNames.RcIndex);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "returnUrl", returnUrl);
            return callInfo;
        }

        public override System.Web.Mvc.ActionResult AcceptChoice(string choice, bool isMobile) {
            var callInfo = new T4MVC_ActionResult(Area, Name, ActionNames.AcceptChoice);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "choice", choice);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "isMobile", isMobile);
            return callInfo;
        }

    }
}

#endregion T4MVC
#pragma warning restore 1591
