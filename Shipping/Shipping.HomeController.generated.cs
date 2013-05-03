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
namespace DcmsMobile.Shipping.Areas.Shipping.Controllers {
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
        public System.Web.Mvc.ActionResult RoutingSummaryAll() {
            return new T4MVC_ActionResult(Area, Name, ActionNames.RoutingSummaryAll);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public System.Web.Mvc.ActionResult RoutingSummary() {
            return new T4MVC_ActionResult(Area, Name, ActionNames.RoutingSummary);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public System.Web.Mvc.ActionResult Unrouted() {
            return new T4MVC_ActionResult(Area, Name, ActionNames.Unrouted);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public System.Web.Mvc.ActionResult PrepareToRoute() {
            return new T4MVC_ActionResult(Area, Name, ActionNames.PrepareToRoute);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public System.Web.Mvc.ActionResult Routing() {
            return new T4MVC_ActionResult(Area, Name, ActionNames.Routing);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public System.Web.Mvc.ActionResult UndoRouting() {
            return new T4MVC_ActionResult(Area, Name, ActionNames.UndoRouting);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public System.Web.Mvc.ActionResult UpdateRouting() {
            return new T4MVC_ActionResult(Area, Name, ActionNames.UpdateRouting);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public System.Web.Mvc.ActionResult ValidateDC() {
            return new T4MVC_ActionResult(Area, Name, ActionNames.ValidateDC);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public System.Web.Mvc.ActionResult Routed() {
            return new T4MVC_ActionResult(Area, Name, ActionNames.Routed);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public System.Web.Mvc.ActionResult CreateBol() {
            return new T4MVC_ActionResult(Area, Name, ActionNames.CreateBol);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public System.Web.Mvc.ActionResult AllAppointments() {
            return new T4MVC_ActionResult(Area, Name, ActionNames.AllAppointments);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public System.Web.Mvc.ActionResult Appointment() {
            return new T4MVC_ActionResult(Area, Name, ActionNames.Appointment);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public System.Web.Mvc.ActionResult CreateUpdateAppointment() {
            return new T4MVC_ActionResult(Area, Name, ActionNames.CreateUpdateAppointment);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public System.Web.Mvc.ActionResult DeleteAppointment() {
            return new T4MVC_ActionResult(Area, Name, ActionNames.DeleteAppointment);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public System.Web.Mvc.ActionResult GetAppointments() {
            return new T4MVC_ActionResult(Area, Name, ActionNames.GetAppointments);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public System.Web.Mvc.ActionResult GetAppointmentByNumber() {
            return new T4MVC_ActionResult(Area, Name, ActionNames.GetAppointmentByNumber);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public System.Web.Mvc.ActionResult UpdateTruckArrival() {
            return new T4MVC_ActionResult(Area, Name, ActionNames.UpdateTruckArrival);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public System.Web.Mvc.ActionResult GetAppointmentsForBol() {
            return new T4MVC_ActionResult(Area, Name, ActionNames.GetAppointmentsForBol);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public System.Web.Mvc.ActionResult AssignAppointmentToBol() {
            return new T4MVC_ActionResult(Area, Name, ActionNames.AssignAppointmentToBol);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public System.Web.Mvc.ActionResult Bol() {
            return new T4MVC_ActionResult(Area, Name, ActionNames.Bol);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public System.Web.Mvc.ActionResult DeleteBol() {
            return new T4MVC_ActionResult(Area, Name, ActionNames.DeleteBol);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public System.Web.Mvc.ActionResult PoSearch() {
            return new T4MVC_ActionResult(Area, Name, ActionNames.PoSearch);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public System.Web.Mvc.ActionResult PoSearchResults() {
            return new T4MVC_ActionResult(Area, Name, ActionNames.PoSearchResults);
        }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public HomeController Actions { get { return MVC_Shipping.Shipping.Home; } }
        [GeneratedCode("T4MVC", "2.0")]
        public readonly string Area = "Shipping";
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
            public readonly string RoutingSummaryAll = "RoutingSummaryAll";
            public readonly string RoutingSummary = "RoutingSummary";
            public readonly string Unrouted = "Unrouted";
            public readonly string PrepareToRoute = "PrepareToRoute";
            public readonly string Routing = "Routing";
            public readonly string UndoRouting = "UndoRouting";
            public readonly string UpdateRouting = "UpdateRouting";
            public readonly string ValidateDC = "ValidateDC";
            public readonly string Routed = "Routed";
            public readonly string CreateBol = "CreateBol";
            public readonly string AllAppointments = "AllAppointments";
            public readonly string Appointment = "Appointment";
            public readonly string CreateUpdateAppointment = "CreateUpdateAppointment";
            public readonly string DeleteAppointment = "DeleteAppointment";
            public readonly string GetAppointments = "GetAppointments";
            public readonly string GetAppointmentByNumber = "GetAppointmentByNumber";
            public readonly string UpdateTruckArrival = "UpdateTruckArrival";
            public readonly string GetAppointmentsForBol = "GetAppointmentsForBol";
            public readonly string AssignAppointmentToBol = "AssignAppointmentToBol";
            public readonly string Bol = "Bol";
            public readonly string DeleteBol = "DeleteBol";
            public readonly string PoSearch = "PoSearch";
            public readonly string PoSearchResults = "PoSearchResults";
        }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionNameConstants {
            public const string Index = "Index";
            public const string RoutingSummaryAll = "RoutingSummaryAll";
            public const string RoutingSummary = "RoutingSummary";
            public const string Unrouted = "Unrouted";
            public const string PrepareToRoute = "PrepareToRoute";
            public const string Routing = "Routing";
            public const string UndoRouting = "UndoRouting";
            public const string UpdateRouting = "UpdateRouting";
            public const string ValidateDC = "ValidateDC";
            public const string Routed = "Routed";
            public const string CreateBol = "CreateBol";
            public const string AllAppointments = "AllAppointments";
            public const string Appointment = "Appointment";
            public const string CreateUpdateAppointment = "CreateUpdateAppointment";
            public const string DeleteAppointment = "DeleteAppointment";
            public const string GetAppointments = "GetAppointments";
            public const string GetAppointmentByNumber = "GetAppointmentByNumber";
            public const string UpdateTruckArrival = "UpdateTruckArrival";
            public const string GetAppointmentsForBol = "GetAppointmentsForBol";
            public const string AssignAppointmentToBol = "AssignAppointmentToBol";
            public const string Bol = "Bol";
            public const string DeleteBol = "DeleteBol";
            public const string PoSearch = "PoSearch";
            public const string PoSearchResults = "PoSearchResults";
        }


        static readonly ActionParamsClass_RoutingSummaryAll s_params_RoutingSummaryAll = new ActionParamsClass_RoutingSummaryAll();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_RoutingSummaryAll RoutingSummaryAllParams { get { return s_params_RoutingSummaryAll; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_RoutingSummaryAll {
            public readonly string model = "model";
        }
        static readonly ActionParamsClass_RoutingSummary s_params_RoutingSummary = new ActionParamsClass_RoutingSummary();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_RoutingSummary RoutingSummaryParams { get { return s_params_RoutingSummary; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_RoutingSummary {
            public readonly string model = "model";
        }
        static readonly ActionParamsClass_Unrouted s_params_Unrouted = new ActionParamsClass_Unrouted();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_Unrouted UnroutedParams { get { return s_params_Unrouted; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_Unrouted {
            public readonly string model = "model";
        }
        static readonly ActionParamsClass_PrepareToRoute s_params_PrepareToRoute = new ActionParamsClass_PrepareToRoute();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_PrepareToRoute PrepareToRouteParams { get { return s_params_PrepareToRoute; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_PrepareToRoute {
            public readonly string model = "model";
        }
        static readonly ActionParamsClass_Routing s_params_Routing = new ActionParamsClass_Routing();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_Routing RoutingParams { get { return s_params_Routing; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_Routing {
            public readonly string model = "model";
        }
        static readonly ActionParamsClass_UndoRouting s_params_UndoRouting = new ActionParamsClass_UndoRouting();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_UndoRouting UndoRoutingParams { get { return s_params_UndoRouting; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_UndoRouting {
            public readonly string key = "key";
        }
        static readonly ActionParamsClass_UpdateRouting s_params_UpdateRouting = new ActionParamsClass_UpdateRouting();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_UpdateRouting UpdateRoutingParams { get { return s_params_UpdateRouting; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_UpdateRouting {
            public readonly string model = "model";
        }
        static readonly ActionParamsClass_ValidateDC s_params_ValidateDC = new ActionParamsClass_ValidateDC();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_ValidateDC ValidateDCParams { get { return s_params_ValidateDC; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_ValidateDC {
            public readonly string customerId = "customerId";
            public readonly string customerDC = "customerDC";
        }
        static readonly ActionParamsClass_Routed s_params_Routed = new ActionParamsClass_Routed();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_Routed RoutedParams { get { return s_params_Routed; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_Routed {
            public readonly string model = "model";
        }
        static readonly ActionParamsClass_CreateBol s_params_CreateBol = new ActionParamsClass_CreateBol();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_CreateBol CreateBolParams { get { return s_params_CreateBol; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_CreateBol {
            public readonly string model = "model";
        }
        static readonly ActionParamsClass_AllAppointments s_params_AllAppointments = new ActionParamsClass_AllAppointments();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_AllAppointments AllAppointmentsParams { get { return s_params_AllAppointments; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_AllAppointments {
            public readonly string model = "model";
        }
        static readonly ActionParamsClass_Appointment s_params_Appointment = new ActionParamsClass_Appointment();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_Appointment AppointmentParams { get { return s_params_Appointment; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_Appointment {
            public readonly string model = "model";
        }
        static readonly ActionParamsClass_CreateUpdateAppointment s_params_CreateUpdateAppointment = new ActionParamsClass_CreateUpdateAppointment();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_CreateUpdateAppointment CreateUpdateAppointmentParams { get { return s_params_CreateUpdateAppointment; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_CreateUpdateAppointment {
            public readonly string model = "model";
        }
        static readonly ActionParamsClass_DeleteAppointment s_params_DeleteAppointment = new ActionParamsClass_DeleteAppointment();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_DeleteAppointment DeleteAppointmentParams { get { return s_params_DeleteAppointment; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_DeleteAppointment {
            public readonly string appointmentId = "appointmentId";
        }
        static readonly ActionParamsClass_GetAppointments s_params_GetAppointments = new ActionParamsClass_GetAppointments();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_GetAppointments GetAppointmentsParams { get { return s_params_GetAppointments; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_GetAppointments {
            public readonly string start = "start";
            public readonly string end = "end";
            public readonly string buildingIdList = "buildingIdList";
            public readonly string customerId = "customerId";
            public readonly string carrierId = "carrierId";
            public readonly string viewName = "viewName";
            public readonly string scheduled = "scheduled";
            public readonly string shipped = "shipped";
        }
        static readonly ActionParamsClass_GetAppointmentByNumber s_params_GetAppointmentByNumber = new ActionParamsClass_GetAppointmentByNumber();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_GetAppointmentByNumber GetAppointmentByNumberParams { get { return s_params_GetAppointmentByNumber; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_GetAppointmentByNumber {
            public readonly string appointmentNumber = "appointmentNumber";
        }
        static readonly ActionParamsClass_UpdateTruckArrival s_params_UpdateTruckArrival = new ActionParamsClass_UpdateTruckArrival();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_UpdateTruckArrival UpdateTruckArrivalParams { get { return s_params_UpdateTruckArrival; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_UpdateTruckArrival {
            public readonly string id = "id";
            public readonly string truckArrivalTime = "truckArrivalTime";
            public readonly string appointmentTime = "appointmentTime";
        }
        static readonly ActionParamsClass_GetAppointmentsForBol s_params_GetAppointmentsForBol = new ActionParamsClass_GetAppointmentsForBol();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_GetAppointmentsForBol GetAppointmentsForBolParams { get { return s_params_GetAppointmentsForBol; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_GetAppointmentsForBol {
            public readonly string start = "start";
            public readonly string end = "end";
        }
        static readonly ActionParamsClass_AssignAppointmentToBol s_params_AssignAppointmentToBol = new ActionParamsClass_AssignAppointmentToBol();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_AssignAppointmentToBol AssignAppointmentToBolParams { get { return s_params_AssignAppointmentToBol; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_AssignAppointmentToBol {
            public readonly string model = "model";
        }
        static readonly ActionParamsClass_Bol s_params_Bol = new ActionParamsClass_Bol();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_Bol BolParams { get { return s_params_Bol; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_Bol {
            public readonly string model = "model";
        }
        static readonly ActionParamsClass_DeleteBol s_params_DeleteBol = new ActionParamsClass_DeleteBol();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_DeleteBol DeleteBolParams { get { return s_params_DeleteBol; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_DeleteBol {
            public readonly string customerId = "customerId";
            public readonly string shippingIdList = "shippingIdList";
        }
        static readonly ActionParamsClass_PoSearch s_params_PoSearch = new ActionParamsClass_PoSearch();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_PoSearch PoSearchParams { get { return s_params_PoSearch; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_PoSearch {
            public readonly string poId = "poId";
        }
        static readonly ActionParamsClass_PoSearchResults s_params_PoSearchResults = new ActionParamsClass_PoSearchResults();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_PoSearchResults PoSearchResultsParams { get { return s_params_PoSearchResults; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_PoSearchResults {
            public readonly string poPattern = "poPattern";
        }
        static readonly ViewNames s_views = new ViewNames();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ViewNames Views { get { return s_views; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ViewNames {
            public readonly string _bolAppointmentHtmlPartial = "_bolAppointmentHtmlPartial";
            public readonly string _dayHtmlPartial = "_dayHtmlPartial";
            public readonly string _monthHtmlPartial = "_monthHtmlPartial";
            public readonly string Appointment = "Appointment";
            public readonly string Bol = "Bol";
            public readonly string Index = "Index";
            public readonly string Index_mobile = "Index.mobile";
            public readonly string PoSearchResults = "PoSearchResults";
            public readonly string Routed = "Routed";
            public readonly string Routing = "Routing";
            public readonly string RoutingSummary = "RoutingSummary";
            public readonly string Unrouted = "Unrouted";
        }
    }

    [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
    public class T4MVC_HomeController: DcmsMobile.Shipping.Areas.Shipping.Controllers.HomeController {
        public T4MVC_HomeController() : base(Dummy.Instance) { }

        public override System.Web.Mvc.ActionResult Index() {
            var callInfo = new T4MVC_ActionResult(Area, Name, ActionNames.Index);
            return callInfo;
        }

        public override System.Web.Mvc.ActionResult RoutingSummaryAll(DcmsMobile.Shipping.ViewModels.RoutingSummaryViewModel model) {
            var callInfo = new T4MVC_ActionResult(Area, Name, ActionNames.RoutingSummaryAll);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "model", model);
            return callInfo;
        }

        public override System.Web.Mvc.ActionResult RoutingSummary(DcmsMobile.Shipping.ViewModels.RoutingSummaryViewModel model) {
            var callInfo = new T4MVC_ActionResult(Area, Name, ActionNames.RoutingSummary);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "model", model);
            return callInfo;
        }

        public override System.Web.Mvc.ActionResult Unrouted(DcmsMobile.Shipping.ViewModels.UnroutedViewModel model) {
            var callInfo = new T4MVC_ActionResult(Area, Name, ActionNames.Unrouted);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "model", model);
            return callInfo;
        }

        public override System.Web.Mvc.ActionResult PrepareToRoute(DcmsMobile.Shipping.ViewModels.UnroutedViewModel model) {
            var callInfo = new T4MVC_ActionResult(Area, Name, ActionNames.PrepareToRoute);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "model", model);
            return callInfo;
        }

        public override System.Web.Mvc.ActionResult Routing(DcmsMobile.Shipping.ViewModels.RoutingViewModel model) {
            var callInfo = new T4MVC_ActionResult(Area, Name, ActionNames.Routing);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "model", model);
            return callInfo;
        }

        public override System.Web.Mvc.ActionResult UndoRouting(string key) {
            var callInfo = new T4MVC_ActionResult(Area, Name, ActionNames.UndoRouting);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "key", key);
            return callInfo;
        }

        public override System.Web.Mvc.ActionResult UpdateRouting(DcmsMobile.Shipping.ViewModels.RoutingViewModel model) {
            var callInfo = new T4MVC_ActionResult(Area, Name, ActionNames.UpdateRouting);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "model", model);
            return callInfo;
        }

        public override System.Web.Mvc.ActionResult ValidateDC(string customerId, string customerDC) {
            var callInfo = new T4MVC_ActionResult(Area, Name, ActionNames.ValidateDC);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "customerId", customerId);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "customerDC", customerDC);
            return callInfo;
        }

        public override System.Web.Mvc.ActionResult Routed(DcmsMobile.Shipping.ViewModels.RoutedViewModel model) {
            var callInfo = new T4MVC_ActionResult(Area, Name, ActionNames.Routed);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "model", model);
            return callInfo;
        }

        public override System.Web.Mvc.ActionResult CreateBol(DcmsMobile.Shipping.ViewModels.RoutedViewModel model) {
            var callInfo = new T4MVC_ActionResult(Area, Name, ActionNames.CreateBol);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "model", model);
            return callInfo;
        }

        public override System.Web.Mvc.ActionResult AllAppointments(DcmsMobile.Shipping.ViewModels.AppointmentViewModel model) {
            var callInfo = new T4MVC_ActionResult(Area, Name, ActionNames.AllAppointments);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "model", model);
            return callInfo;
        }

        public override System.Web.Mvc.ActionResult Appointment(DcmsMobile.Shipping.ViewModels.AppointmentViewModel model) {
            var callInfo = new T4MVC_ActionResult(Area, Name, ActionNames.Appointment);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "model", model);
            return callInfo;
        }

        public override System.Web.Mvc.ActionResult CreateUpdateAppointment(DcmsMobile.Shipping.ViewModels.AppointmentModel model) {
            var callInfo = new T4MVC_ActionResult(Area, Name, ActionNames.CreateUpdateAppointment);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "model", model);
            return callInfo;
        }

        public override System.Web.Mvc.ActionResult DeleteAppointment(int? appointmentId) {
            var callInfo = new T4MVC_ActionResult(Area, Name, ActionNames.DeleteAppointment);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "appointmentId", appointmentId);
            return callInfo;
        }

        public override System.Web.Mvc.ActionResult GetAppointments(System.DateTimeOffset start, System.DateTimeOffset end, string[] buildingIdList, string customerId, string carrierId, DcmsMobile.Shipping.ViewModels.CalendarViewName? viewName, bool? scheduled, bool? shipped) {
            var callInfo = new T4MVC_ActionResult(Area, Name, ActionNames.GetAppointments);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "start", start);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "end", end);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "buildingIdList", buildingIdList);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "customerId", customerId);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "carrierId", carrierId);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "viewName", viewName);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "scheduled", scheduled);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "shipped", shipped);
            return callInfo;
        }

        public override System.Web.Mvc.ActionResult GetAppointmentByNumber(int appointmentNumber) {
            var callInfo = new T4MVC_ActionResult(Area, Name, ActionNames.GetAppointmentByNumber);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "appointmentNumber", appointmentNumber);
            return callInfo;
        }

        public override System.Web.Mvc.ActionResult UpdateTruckArrival(int id, System.DateTimeOffset? truckArrivalTime, System.DateTimeOffset appointmentTime) {
            var callInfo = new T4MVC_ActionResult(Area, Name, ActionNames.UpdateTruckArrival);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "id", id);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "truckArrivalTime", truckArrivalTime);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "appointmentTime", appointmentTime);
            return callInfo;
        }

        public override System.Web.Mvc.ActionResult GetAppointmentsForBol(System.DateTimeOffset start, System.DateTimeOffset end) {
            var callInfo = new T4MVC_ActionResult(Area, Name, ActionNames.GetAppointmentsForBol);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "start", start);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "end", end);
            return callInfo;
        }

        public override System.Web.Mvc.ActionResult AssignAppointmentToBol(DcmsMobile.Shipping.ViewModels.BolViewModel model) {
            var callInfo = new T4MVC_ActionResult(Area, Name, ActionNames.AssignAppointmentToBol);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "model", model);
            return callInfo;
        }

        public override System.Web.Mvc.ActionResult Bol(DcmsMobile.Shipping.ViewModels.BolViewModel model) {
            var callInfo = new T4MVC_ActionResult(Area, Name, ActionNames.Bol);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "model", model);
            return callInfo;
        }

        public override System.Web.Mvc.ActionResult DeleteBol(string customerId, string shippingIdList) {
            var callInfo = new T4MVC_ActionResult(Area, Name, ActionNames.DeleteBol);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "customerId", customerId);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "shippingIdList", shippingIdList);
            return callInfo;
        }

        public override System.Web.Mvc.ActionResult PoSearch(string poId) {
            var callInfo = new T4MVC_ActionResult(Area, Name, ActionNames.PoSearch);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "poId", poId);
            return callInfo;
        }

        public override System.Web.Mvc.ActionResult PoSearchResults(string poPattern) {
            var callInfo = new T4MVC_ActionResult(Area, Name, ActionNames.PoSearchResults);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "poPattern", poPattern);
            return callInfo;
        }

    }
}

#endregion T4MVC
#pragma warning restore 1591
