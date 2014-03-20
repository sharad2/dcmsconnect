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
namespace DcmsMobile.DcmsLite.Areas.DcmsLite.Controllers {
    public partial class ShipController {
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ShipController() { }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        protected ShipController(Dummy d) { }

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
        public System.Web.Mvc.ActionResult SearchCustomer() {
            return new T4MVC_ActionResult(Area, Name, ActionNames.SearchCustomer);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public System.Web.Mvc.ActionResult CreateBol() {
            return new T4MVC_ActionResult(Area, Name, ActionNames.CreateBol);
        }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ShipController Actions { get { return MVC_DcmsLite.DcmsLite.Ship; } }
        [GeneratedCode("T4MVC", "2.0")]
        public readonly string Area = "DcmsLite";
        [GeneratedCode("T4MVC", "2.0")]
        public readonly string Name = "Ship";
        [GeneratedCode("T4MVC", "2.0")]
        public const string NameConst = "Ship";

        static readonly ActionNamesClass s_actions = new ActionNamesClass();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionNamesClass ActionNames { get { return s_actions; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionNamesClass {
            public readonly string Index = "Index";
            public readonly string SearchCustomer = "SearchCustomer";
            public readonly string CreateBol = "CreateBol";
        }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionNameConstants {
            public const string Index = "Index";
            public const string SearchCustomer = "SearchCustomer";
            public const string CreateBol = "CreateBol";
        }


        static readonly ActionParamsClass_SearchCustomer s_params_SearchCustomer = new ActionParamsClass_SearchCustomer();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_SearchCustomer SearchCustomerParams { get { return s_params_SearchCustomer; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_SearchCustomer {
            public readonly string customerId = "customerId";
        }
        static readonly ActionParamsClass_CreateBol s_params_CreateBol = new ActionParamsClass_CreateBol();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_CreateBol CreateBolParams { get { return s_params_CreateBol; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_CreateBol {
            public readonly string model = "model";
        }
        static readonly ViewNames s_views = new ViewNames();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ViewNames Views { get { return s_views; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ViewNames {
            public readonly string Index = "Index";
        }
    }

    [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
    public class T4MVC_ShipController: DcmsMobile.DcmsLite.Areas.DcmsLite.Controllers.ShipController {
        public T4MVC_ShipController() : base(Dummy.Instance) { }

        public override System.Web.Mvc.ActionResult Index() {
            var callInfo = new T4MVC_ActionResult(Area, Name, ActionNames.Index);
            return callInfo;
        }

        public override System.Web.Mvc.ActionResult SearchCustomer(string customerId) {
            var callInfo = new T4MVC_ActionResult(Area, Name, ActionNames.SearchCustomer);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "customerId", customerId);
            return callInfo;
        }

        public override System.Web.Mvc.ActionResult CreateBol(DcmsMobile.DcmsLite.ViewModels.Ship.IndexViewModel model) {
            var callInfo = new T4MVC_ActionResult(Area, Name, ActionNames.CreateBol);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "model", model);
            return callInfo;
        }

    }
}

#endregion T4MVC
#pragma warning restore 1591
