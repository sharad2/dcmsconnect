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
        public System.Web.Mvc.ActionResult EditPalletLimit() {
            return new T4MVC_ActionResult(Area, Name, ActionNames.EditPalletLimit);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public System.Web.Mvc.ActionResult EditAddressOfBuilding() {
            return new T4MVC_ActionResult(Area, Name, ActionNames.EditAddressOfBuilding);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public System.Web.Mvc.ActionResult UpdateAddress() {
            return new T4MVC_ActionResult(Area, Name, ActionNames.UpdateAddress);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public System.Web.Mvc.ActionResult AddBuilding() {
            return new T4MVC_ActionResult(Area, Name, ActionNames.AddBuilding);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public System.Web.Mvc.ActionResult CartonArea() {
            return new T4MVC_ActionResult(Area, Name, ActionNames.CartonArea);
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
        public System.Web.Mvc.ActionResult ApplyCartonAreaLocationFilter() {
            return new T4MVC_ActionResult(Area, Name, ActionNames.ApplyCartonAreaLocationFilter);
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
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public System.Web.Mvc.ActionResult PickingArea() {
            return new T4MVC_ActionResult(Area, Name, ActionNames.PickingArea);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public System.Web.Mvc.ActionResult UpdatePickingArea() {
            return new T4MVC_ActionResult(Area, Name, ActionNames.UpdatePickingArea);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public System.Web.Mvc.ActionResult ManagePickingArea() {
            return new T4MVC_ActionResult(Area, Name, ActionNames.ManagePickingArea);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public System.Web.Mvc.ActionResult ApplyPickingAreaLocationFilter() {
            return new T4MVC_ActionResult(Area, Name, ActionNames.ApplyPickingAreaLocationFilter);
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
            public readonly string EditPalletLimit = "EditPalletLimit";
            public readonly string EditAddressOfBuilding = "EditAddressOfBuilding";
            public readonly string UpdateAddress = "UpdateAddress";
            public readonly string AddNewBuilding = "AddNewBuilding";
            public readonly string AddBuilding = "AddBuilding";
            public readonly string CartonArea = "CartonArea";
            public readonly string UpdateArea = "UpdateArea";
            public readonly string ManageCartonArea = "ManageCartonArea";
            public readonly string ApplyCartonAreaLocationFilter = "ApplyCartonAreaLocationFilter";
            public readonly string UpdateLocation = "UpdateLocation";
            public readonly string UnassignLocation = "UnassignLocation";
            public readonly string PickingArea = "PickingArea";
            public readonly string Tutorial = "Tutorial";
            public readonly string UpdatePickingArea = "UpdatePickingArea";
            public readonly string ManagePickingArea = "ManagePickingArea";
            public readonly string ApplyPickingAreaLocationFilter = "ApplyPickingAreaLocationFilter";
        }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionNameConstants {
            public const string Index = "Index";
            public const string EditPalletLimit = "EditPalletLimit";
            public const string EditAddressOfBuilding = "EditAddressOfBuilding";
            public const string UpdateAddress = "UpdateAddress";
            public const string AddNewBuilding = "AddNewBuilding";
            public const string AddBuilding = "AddBuilding";
            public const string CartonArea = "CartonArea";
            public const string UpdateArea = "UpdateArea";
            public const string ManageCartonArea = "ManageCartonArea";
            public const string ApplyCartonAreaLocationFilter = "ApplyCartonAreaLocationFilter";
            public const string UpdateLocation = "UpdateLocation";
            public const string UnassignLocation = "UnassignLocation";
            public const string PickingArea = "PickingArea";
            public const string Tutorial = "Tutorial";
            public const string UpdatePickingArea = "UpdatePickingArea";
            public const string ManagePickingArea = "ManagePickingArea";
            public const string ApplyPickingAreaLocationFilter = "ApplyPickingAreaLocationFilter";
        }


        static readonly ActionParamsClass_EditPalletLimit s_params_EditPalletLimit = new ActionParamsClass_EditPalletLimit();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_EditPalletLimit EditPalletLimitParams { get { return s_params_EditPalletLimit; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_EditPalletLimit {
            public readonly string buildingId = "buildingId";
            public readonly string palletLimit = "palletLimit";
        }
        static readonly ActionParamsClass_EditAddressOfBuilding s_params_EditAddressOfBuilding = new ActionParamsClass_EditAddressOfBuilding();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_EditAddressOfBuilding EditAddressOfBuildingParams { get { return s_params_EditAddressOfBuilding; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_EditAddressOfBuilding {
            public readonly string buildingId = "buildingId";
        }
        static readonly ActionParamsClass_UpdateAddress s_params_UpdateAddress = new ActionParamsClass_UpdateAddress();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_UpdateAddress UpdateAddressParams { get { return s_params_UpdateAddress; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_UpdateAddress {
            public readonly string model = "model";
        }
        static readonly ActionParamsClass_AddBuilding s_params_AddBuilding = new ActionParamsClass_AddBuilding();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_AddBuilding AddBuildingParams { get { return s_params_AddBuilding; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_AddBuilding {
            public readonly string modal = "modal";
        }
        static readonly ActionParamsClass_CartonArea s_params_CartonArea = new ActionParamsClass_CartonArea();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_CartonArea CartonAreaParams { get { return s_params_CartonArea; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_CartonArea {
            public readonly string buildingId = "buildingId";
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
        }
        static readonly ActionParamsClass_ApplyCartonAreaLocationFilter s_params_ApplyCartonAreaLocationFilter = new ActionParamsClass_ApplyCartonAreaLocationFilter();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_ApplyCartonAreaLocationFilter ApplyCartonAreaLocationFilterParams { get { return s_params_ApplyCartonAreaLocationFilter; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_ApplyCartonAreaLocationFilter {
            public readonly string areaId = "areaId";
            public readonly string assignedSkuId = "assignedSkuId";
            public readonly string locationId = "locationId";
            public readonly string assignedLocation = "assignedLocation";
            public readonly string emptyLocations = "emptyLocations";
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
        }
        static readonly ActionParamsClass_PickingArea s_params_PickingArea = new ActionParamsClass_PickingArea();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_PickingArea PickingAreaParams { get { return s_params_PickingArea; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_PickingArea {
            public readonly string buildingId = "buildingId";
        }
        static readonly ActionParamsClass_UpdatePickingArea s_params_UpdatePickingArea = new ActionParamsClass_UpdatePickingArea();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_UpdatePickingArea UpdatePickingAreaParams { get { return s_params_UpdatePickingArea; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_UpdatePickingArea {
            public readonly string model = "model";
        }
        static readonly ActionParamsClass_ManagePickingArea s_params_ManagePickingArea = new ActionParamsClass_ManagePickingArea();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_ManagePickingArea ManagePickingAreaParams { get { return s_params_ManagePickingArea; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_ManagePickingArea {
            public readonly string areaId = "areaId";
        }
        static readonly ActionParamsClass_ApplyPickingAreaLocationFilter s_params_ApplyPickingAreaLocationFilter = new ActionParamsClass_ApplyPickingAreaLocationFilter();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_ApplyPickingAreaLocationFilter ApplyPickingAreaLocationFilterParams { get { return s_params_ApplyPickingAreaLocationFilter; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_ApplyPickingAreaLocationFilter {
            public readonly string areaId = "areaId";
            public readonly string assignedLocation = "assignedLocation";
            public readonly string emptyLocations = "emptyLocations";
            public readonly string assignedSkuId = "assignedSkuId";
            public readonly string locationId = "locationId";
        }
        static readonly ViewNames s_views = new ViewNames();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ViewNames Views { get { return s_views; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ViewNames {
            public readonly string _assignSkuDialogPartial = "_assignSkuDialogPartial";
            public readonly string _cartonAreaLocationCountMatrixPartial = "_cartonAreaLocationCountMatrixPartial";
            public readonly string _pickingAreaLocationCountMatrixPartial = "_pickingAreaLocationCountMatrixPartial";
            public readonly string _updateFlagDialogPartial = "_updateFlagDialogPartial";
            public readonly string AddBuilding = "AddBuilding";
            public readonly string CartonArea = "CartonArea";
            public readonly string EditAddressOfBuilding = "EditAddressOfBuilding";
            public readonly string Index = "Index";
            public readonly string ManageCartonArea = "ManageCartonArea";
            public readonly string ManagePickingArea = "ManagePickingArea";
            public readonly string PickingArea = "PickingArea";
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

        public override System.Web.Mvc.ActionResult EditPalletLimit(string buildingId, int? palletLimit) {
            var callInfo = new T4MVC_ActionResult(Area, Name, ActionNames.EditPalletLimit);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "buildingId", buildingId);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "palletLimit", palletLimit);
            return callInfo;
        }

        public override System.Web.Mvc.ActionResult EditAddressOfBuilding(string buildingId) {
            var callInfo = new T4MVC_ActionResult(Area, Name, ActionNames.EditAddressOfBuilding);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "buildingId", buildingId);
            return callInfo;
        }

        public override System.Web.Mvc.ActionResult UpdateAddress(DcmsMobile.CartonAreas.ViewModels.EditAddressOfBuildingViewModel model) {
            var callInfo = new T4MVC_ActionResult(Area, Name, ActionNames.UpdateAddress);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "model", model);
            return callInfo;
        }

        public override System.Web.Mvc.ActionResult AddNewBuilding() {
            var callInfo = new T4MVC_ActionResult(Area, Name, ActionNames.AddNewBuilding);
            return callInfo;
        }

        public override System.Web.Mvc.ActionResult AddBuilding(DcmsMobile.CartonAreas.ViewModels.AddBuildingViewModel modal) {
            var callInfo = new T4MVC_ActionResult(Area, Name, ActionNames.AddBuilding);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "modal", modal);
            return callInfo;
        }

        public override System.Web.Mvc.ActionResult CartonArea(string buildingId) {
            var callInfo = new T4MVC_ActionResult(Area, Name, ActionNames.CartonArea);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "buildingId", buildingId);
            return callInfo;
        }

        public override System.Web.Mvc.ActionResult UpdateArea(DcmsMobile.CartonAreas.ViewModels.CartonAreaModel cam) {
            var callInfo = new T4MVC_ActionResult(Area, Name, ActionNames.UpdateArea);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "cam", cam);
            return callInfo;
        }

        public override System.Web.Mvc.ActionResult ManageCartonArea(string areaId) {
            var callInfo = new T4MVC_ActionResult(Area, Name, ActionNames.ManageCartonArea);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "areaId", areaId);
            return callInfo;
        }

        public override System.Web.Mvc.ActionResult ApplyCartonAreaLocationFilter(string areaId, int? assignedSkuId, string locationId, bool? assignedLocation, bool? emptyLocations) {
            var callInfo = new T4MVC_ActionResult(Area, Name, ActionNames.ApplyCartonAreaLocationFilter);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "areaId", areaId);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "assignedSkuId", assignedSkuId);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "locationId", locationId);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "assignedLocation", assignedLocation);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "emptyLocations", emptyLocations);
            return callInfo;
        }

        public override System.Web.Mvc.ActionResult UpdateLocation(DcmsMobile.CartonAreas.ViewModels.AssignSkuViewModel model) {
            var callInfo = new T4MVC_ActionResult(Area, Name, ActionNames.UpdateLocation);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "model", model);
            return callInfo;
        }

        public override System.Web.Mvc.ActionResult UnassignLocation(string locationId) {
            var callInfo = new T4MVC_ActionResult(Area, Name, ActionNames.UnassignLocation);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "locationId", locationId);
            return callInfo;
        }

        public override System.Web.Mvc.ActionResult PickingArea(string buildingId) {
            var callInfo = new T4MVC_ActionResult(Area, Name, ActionNames.PickingArea);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "buildingId", buildingId);
            return callInfo;
        }

        public override System.Web.Mvc.ActionResult Tutorial() {
            var callInfo = new T4MVC_ActionResult(Area, Name, ActionNames.Tutorial);
            return callInfo;
        }

        public override System.Web.Mvc.ActionResult UpdatePickingArea(DcmsMobile.CartonAreas.ViewModels.PickingAreaViewModel model) {
            var callInfo = new T4MVC_ActionResult(Area, Name, ActionNames.UpdatePickingArea);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "model", model);
            return callInfo;
        }

        public override System.Web.Mvc.ActionResult ManagePickingArea(string areaId) {
            var callInfo = new T4MVC_ActionResult(Area, Name, ActionNames.ManagePickingArea);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "areaId", areaId);
            return callInfo;
        }

        public override System.Web.Mvc.ActionResult ApplyPickingAreaLocationFilter(string areaId, bool? assignedLocation, bool? emptyLocations, int? assignedSkuId, string locationId) {
            var callInfo = new T4MVC_ActionResult(Area, Name, ActionNames.ApplyPickingAreaLocationFilter);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "areaId", areaId);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "assignedLocation", assignedLocation);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "emptyLocations", emptyLocations);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "assignedSkuId", assignedSkuId);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "locationId", locationId);
            return callInfo;
        }

    }
}

#endregion T4MVC
#pragma warning restore 1591
