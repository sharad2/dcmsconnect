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
namespace DcmsMobile.PickWaves.Areas.PickWaves.Controllers {
    public partial class BoxPickPalletController {
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        protected BoxPickPalletController(Dummy d) { }

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
        public System.Web.Mvc.ActionResult Index() {
            return new T4MVC_ActionResult(Area, Name, ActionNames.Index);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public System.Web.Mvc.ActionResult CreatePallet() {
            return new T4MVC_ActionResult(Area, Name, ActionNames.CreatePallet);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public System.Web.Mvc.ActionResult RemoveUnPickedBoxesFromPallet() {
            return new T4MVC_ActionResult(Area, Name, ActionNames.RemoveUnPickedBoxesFromPallet);
        }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public BoxPickPalletController Actions { get { return MVC_PickWaves.PickWaves.BoxPickPallet; } }
        [GeneratedCode("T4MVC", "2.0")]
        public readonly string Area = "PickWaves";
        [GeneratedCode("T4MVC", "2.0")]
        public readonly string Name = "BoxPickPallet";
        [GeneratedCode("T4MVC", "2.0")]
        public const string NameConst = "BoxPickPallet";

        static readonly ActionNamesClass s_actions = new ActionNamesClass();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionNamesClass ActionNames { get { return s_actions; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionNamesClass {
            public readonly string Index = "Index";
            public readonly string CreatePallet = "CreatePallet";
            public readonly string RemoveUnPickedBoxesFromPallet = "RemoveUnPickedBoxesFromPallet";
        }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionNameConstants {
            public const string Index = "Index";
            public const string CreatePallet = "CreatePallet";
            public const string RemoveUnPickedBoxesFromPallet = "RemoveUnPickedBoxesFromPallet";
        }


        static readonly ActionParamsClass_Index s_params_Index = new ActionParamsClass_Index();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_Index IndexParams { get { return s_params_Index; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_Index {
            public readonly string bucketId = "bucketId";
        }
        static readonly ActionParamsClass_CreatePallet s_params_CreatePallet = new ActionParamsClass_CreatePallet();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_CreatePallet CreatePalletParams { get { return s_params_CreatePallet; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_CreatePallet {
            public readonly string model = "model";
        }
        static readonly ActionParamsClass_RemoveUnPickedBoxesFromPallet s_params_RemoveUnPickedBoxesFromPallet = new ActionParamsClass_RemoveUnPickedBoxesFromPallet();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_RemoveUnPickedBoxesFromPallet RemoveUnPickedBoxesFromPalletParams { get { return s_params_RemoveUnPickedBoxesFromPallet; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_RemoveUnPickedBoxesFromPallet {
            public readonly string bucketId = "bucketId";
            public readonly string palletId = "palletId";
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
    public class T4MVC_BoxPickPalletController: DcmsMobile.PickWaves.Areas.PickWaves.Controllers.BoxPickPalletController {
        public T4MVC_BoxPickPalletController() : base(Dummy.Instance) { }

        public override System.Web.Mvc.ActionResult Index(int? bucketId) {
            var callInfo = new T4MVC_ActionResult(Area, Name, ActionNames.Index);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "bucketId", bucketId);
            return callInfo;
        }

        public override System.Web.Mvc.ActionResult CreatePallet(DcmsMobile.PickWaves.ViewModels.BoxPickPallet.BoxPickPalletViewModel model) {
            var callInfo = new T4MVC_ActionResult(Area, Name, ActionNames.CreatePallet);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "model", model);
            return callInfo;
        }

        public override System.Web.Mvc.ActionResult RemoveUnPickedBoxesFromPallet(int? bucketId, string palletId) {
            var callInfo = new T4MVC_ActionResult(Area, Name, ActionNames.RemoveUnPickedBoxesFromPallet);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "bucketId", bucketId);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "palletId", palletId);
            return callInfo;
        }

    }
}

#endregion T4MVC
#pragma warning restore 1591
