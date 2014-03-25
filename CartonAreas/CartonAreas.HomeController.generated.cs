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
namespace DcmsMobile.CartonAreas.Areas.CartonAreas.Controllers {
    public partial class HomeController {
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
        public System.Web.Mvc.ActionResult UpdateArea() {
            return new T4MVC_ActionResult(Area, Name, ActionNames.UpdateArea);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public System.Web.Mvc.ActionResult ManageCartonArea() {
            return new T4MVC_ActionResult(Area, Name, ActionNames.ManageCartonArea);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public System.Web.Mvc.ActionResult ApplyAssignedSkuFilter() {
            return new T4MVC_ActionResult(Area, Name, ActionNames.ApplyAssignedSkuFilter);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public System.Web.Mvc.ActionResult ApplyLocationIdFilter() {
            return new T4MVC_ActionResult(Area, Name, ActionNames.ApplyLocationIdFilter);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public System.Web.Mvc.ActionResult UpdateLocation() {
            return new T4MVC_ActionResult(Area, Name, ActionNames.UpdateLocation);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public System.Web.Mvc.ActionResult UnassignLocation() {
            return new T4MVC_ActionResult(Area, Name, ActionNames.UnassignLocation);
        }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public HomeController Actions { get { return MVC_CartonAreas.CartonAreas.Home; } }
        [GeneratedCode("T4MVC", "2.0")]
        public readonly string Area = "CartonAreas";
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
            public readonly string UpdateArea = "UpdateArea";
            public readonly string ManageCartonArea = "ManageCartonArea";
            public readonly string ApplyAssignedSkuFilter = "ApplyAssignedSkuFilter";
            public readonly string ApplyLocationIdFilter = "ApplyLocationIdFilter";
            public readonly string UpdateLocation = "UpdateLocation";
            public readonly string UnassignLocation = "UnassignLocation";
            public readonly string Tutorial = "Tutorial";
        }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionNameConstants {
            public const string Index = "Index";
            public const string UpdateArea = "UpdateArea";
            public const string ManageCartonArea = "ManageCartonArea";
            public const string ApplyAssignedSkuFilter = "ApplyAssignedSkuFilter";
            public const string ApplyLocationIdFilter = "ApplyLocationIdFilter";
            public const string UpdateLocation = "UpdateLocation";
            public const string UnassignLocation = "UnassignLocation";
            public const string Tutorial = "Tutorial";
        }


        static readonly ActionParamsClass_UpdateArea s_params_UpdateArea = new ActionParamsClass_UpdateArea();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_UpdateArea UpdateAreaParams { get { return s_params_UpdateArea; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_UpdateArea {
            public readonly string cam = "cam";
        }
        static readonly ActionParamsClass_ManageCartonArea s_params_ManageCartonArea = new ActionParamsClass_ManageCartonArea();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_ManageCartonArea ManageCartonAreaParams { get { return s_params_ManageCartonArea; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_ManageCartonArea {
            public readonly string areaId = "areaId";
            public readonly string assigned = "assigned";
            public readonly string emptyLocations = "emptyLocations";
        }
        static readonly ActionParamsClass_ApplyAssignedSkuFilter s_params_ApplyAssignedSkuFilter = new ActionParamsClass_ApplyAssignedSkuFilter();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_ApplyAssignedSkuFilter ApplyAssignedSkuFilterParams { get { return s_params_ApplyAssignedSkuFilter; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_ApplyAssignedSkuFilter {
            public readonly string model = "model";
        }
        static readonly ActionParamsClass_ApplyLocationIdFilter s_params_ApplyLocationIdFilter = new ActionParamsClass_ApplyLocationIdFilter();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_ApplyLocationIdFilter ApplyLocationIdFilterParams { get { return s_params_ApplyLocationIdFilter; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_ApplyLocationIdFilter {
            public readonly string model = "model";
        }
        static readonly ActionParamsClass_UpdateLocation s_params_UpdateLocation = new ActionParamsClass_UpdateLocation();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_UpdateLocation UpdateLocationParams { get { return s_params_UpdateLocation; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_UpdateLocation {
            public readonly string model = "model";
        }
        static readonly ActionParamsClass_UnassignLocation s_params_UnassignLocation = new ActionParamsClass_UnassignLocation();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_UnassignLocation UnassignLocationParams { get { return s_params_UnassignLocation; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_UnassignLocation {
            public readonly string locationId = "locationId";
            public readonly string areaId = "areaId";
        }
        static readonly ViewNames s_views = new ViewNames();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ViewNames Views { get { return s_views; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ViewNames {
            public readonly string _areaInfoPartial = "_areaInfoPartial";
            public readonly string _assignSkuDialogPartial = "_assignSkuDialogPartial";
            public readonly string _updateFlagDialogPartial = "_updateFlagDialogPartial";
            public readonly string Index = "Index";
            public readonly string ManageCartonArea = "ManageCartonArea";
            public readonly string Tutorial = "Tutorial";
        }
    }

    [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
    public class T4MVC_HomeController: DcmsMobile.CartonAreas.Areas.CartonAreas.Controllers.HomeController {
        public T4MVC_HomeController() : base(Dummy.Instance) { }

        public override System.Web.Mvc.ActionResult Index() {
            var callInfo = new T4MVC_ActionResult(Area, Name, ActionNames.Index);
            return callInfo;
        }

        public override System.Web.Mvc.ActionResult UpdateArea(DcmsMobile.CartonAreas.ViewModels.CartonAreaModel cam) {
            var callInfo = new T4MVC_ActionResult(Area, Name, ActionNames.UpdateArea);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "cam", cam);
            return callInfo;
        }

        public override System.Web.Mvc.ActionResult ManageCartonArea(string areaId, bool? assigned, bool? emptyLocations) {
            var callInfo = new T4MVC_ActionResult(Area, Name, ActionNames.ManageCartonArea);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "areaId", areaId);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "assigned", assigned);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "emptyLocations", emptyLocations);
            return callInfo;
        }

        public override System.Web.Mvc.ActionResult ApplyAssignedSkuFilter(DcmsMobile.CartonAreas.ViewModels.ManageCartonAreaViewModel model) {
            var callInfo = new T4MVC_ActionResult(Area, Name, ActionNames.ApplyAssignedSkuFilter);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "model", model);
            return callInfo;
        }

        public override System.Web.Mvc.ActionResult ApplyLocationIdFilter(DcmsMobile.CartonAreas.ViewModels.ManageCartonAreaViewModel model) {
            var callInfo = new T4MVC_ActionResult(Area, Name, ActionNames.ApplyLocationIdFilter);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "model", model);
            return callInfo;
        }

        public override System.Web.Mvc.ActionResult UpdateLocation(DcmsMobile.CartonAreas.ViewModels.AssignSkuViewModel model) {
            var callInfo = new T4MVC_ActionResult(Area, Name, ActionNames.UpdateLocation);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "model", model);
            return callInfo;
        }

        public override System.Web.Mvc.ActionResult UnassignLocation(string locationId, string areaId) {
            var callInfo = new T4MVC_ActionResult(Area, Name, ActionNames.UnassignLocation);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "locationId", locationId);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "areaId", areaId);
            return callInfo;
        }

        public override System.Web.Mvc.ActionResult Tutorial() {
            var callInfo = new T4MVC_ActionResult(Area, Name, ActionNames.Tutorial);
            return callInfo;
        }

    }
}

#endregion T4MVC
#pragma warning restore 1591
