// <auto-generated />
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
namespace DcmsMobile.BoxManager.Areas.BoxManager.Controllers
{
    public partial class MovePalletController
    {
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        protected MovePalletController(Dummy d) { }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        protected RedirectToRouteResult RedirectToAction(ActionResult result)
        {
            var callInfo = result.GetT4MVCResult();
            return RedirectToRoute(callInfo.RouteValueDictionary);
        }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        protected RedirectToRouteResult RedirectToAction(Task<ActionResult> taskResult)
        {
            return RedirectToAction(taskResult.Result);
        }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        protected RedirectToRouteResult RedirectToActionPermanent(ActionResult result)
        {
            var callInfo = result.GetT4MVCResult();
            return RedirectToRoutePermanent(callInfo.RouteValueDictionary);
        }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        protected RedirectToRouteResult RedirectToActionPermanent(Task<ActionResult> taskResult)
        {
            return RedirectToActionPermanent(taskResult.Result);
        }

        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult MovePallet()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.MovePallet);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult SourcePallet()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.SourcePallet);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult ValidatePallet()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.ValidatePallet);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult HandleBoxCount()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.HandleBoxCount);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult ValidateBoxesOnPallet()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.ValidateBoxesOnPallet);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult ScanBoxOfSourcePallet()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.ScanBoxOfSourcePallet);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult Destination()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.Destination);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult HandleDestinationScan()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.HandleDestinationScan);
        }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public MovePalletController Actions { get { return MVC_BoxManager.BoxManager.MovePallet; } }
        [GeneratedCode("T4MVC", "2.0")]
        public readonly string Area = "BoxManager";
        [GeneratedCode("T4MVC", "2.0")]
        public readonly string Name = "MovePallet";
        [GeneratedCode("T4MVC", "2.0")]
        public const string NameConst = "MovePallet";

        static readonly ActionNamesClass s_actions = new ActionNamesClass();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionNamesClass ActionNames { get { return s_actions; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionNamesClass
        {
            public readonly string Index = "Index";
            public readonly string MovePallet = "MovePallet";
            public readonly string SourcePallet = "SourcePallet";
            public readonly string ValidatePallet = "ValidatePallet";
            public readonly string HandleBoxCount = "HandleBoxCount";
            public readonly string ValidateBoxesOnPallet = "ValidateBoxesOnPallet";
            public readonly string ScanBoxOfSourcePallet = "ScanBoxOfSourcePallet";
            public readonly string Destination = "Destination";
            public readonly string HandleDestinationScan = "HandleDestinationScan";
        }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionNameConstants
        {
            public const string Index = "Index";
            public const string MovePallet = "MovePallet";
            public const string SourcePallet = "SourcePallet";
            public const string ValidatePallet = "ValidatePallet";
            public const string HandleBoxCount = "HandleBoxCount";
            public const string ValidateBoxesOnPallet = "ValidateBoxesOnPallet";
            public const string ScanBoxOfSourcePallet = "ScanBoxOfSourcePallet";
            public const string Destination = "Destination";
            public const string HandleDestinationScan = "HandleDestinationScan";
        }


        static readonly ActionParamsClass_Index s_params_Index = new ActionParamsClass_Index();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_Index IndexParams { get { return s_params_Index; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_Index
        {
            public readonly string sound = "sound";
        }
        static readonly ActionParamsClass_MovePallet s_params_MovePallet = new ActionParamsClass_MovePallet();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_MovePallet MovePalletParams { get { return s_params_MovePallet; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_MovePallet
        {
            public readonly string id = "id";
        }
        static readonly ActionParamsClass_SourcePallet s_params_SourcePallet = new ActionParamsClass_SourcePallet();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_SourcePallet SourcePalletParams { get { return s_params_SourcePallet; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_SourcePallet
        {
            public readonly string model = "model";
        }
        static readonly ActionParamsClass_ValidatePallet s_params_ValidatePallet = new ActionParamsClass_ValidatePallet();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_ValidatePallet ValidatePalletParams { get { return s_params_ValidatePallet; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_ValidatePallet
        {
            public readonly string palletId = "palletId";
        }
        static readonly ActionParamsClass_HandleBoxCount s_params_HandleBoxCount = new ActionParamsClass_HandleBoxCount();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_HandleBoxCount HandleBoxCountParams { get { return s_params_HandleBoxCount; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_HandleBoxCount
        {
            public readonly string model = "model";
        }
        static readonly ActionParamsClass_ValidateBoxesOnPallet s_params_ValidateBoxesOnPallet = new ActionParamsClass_ValidateBoxesOnPallet();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_ValidateBoxesOnPallet ValidateBoxesOnPalletParams { get { return s_params_ValidateBoxesOnPallet; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_ValidateBoxesOnPallet
        {
            public readonly string palletId = "palletId";
            public readonly string sound = "sound";
        }
        static readonly ActionParamsClass_ScanBoxOfSourcePallet s_params_ScanBoxOfSourcePallet = new ActionParamsClass_ScanBoxOfSourcePallet();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_ScanBoxOfSourcePallet ScanBoxOfSourcePalletParams { get { return s_params_ScanBoxOfSourcePallet; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_ScanBoxOfSourcePallet
        {
            public readonly string model = "model";
        }
        static readonly ActionParamsClass_Destination s_params_Destination = new ActionParamsClass_Destination();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_Destination DestinationParams { get { return s_params_Destination; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_Destination
        {
            public readonly string palletId = "palletId";
            public readonly string sound = "sound";
        }
        static readonly ActionParamsClass_HandleDestinationScan s_params_HandleDestinationScan = new ActionParamsClass_HandleDestinationScan();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_HandleDestinationScan HandleDestinationScanParams { get { return s_params_HandleDestinationScan; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_HandleDestinationScan
        {
            public readonly string model = "model";
        }
        static readonly ViewsClass s_views = new ViewsClass();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ViewsClass Views { get { return s_views; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ViewsClass
        {
            static readonly _ViewNamesClass s_ViewNames = new _ViewNamesClass();
            public _ViewNamesClass ViewNames { get { return s_ViewNames; } }
            public class _ViewNamesClass
            {
                public readonly string _palletDetailsPartial = "_palletDetailsPartial";
                public readonly string Destination = "Destination";
                public readonly string Index = "Index";
                public readonly string ValidateBoxes = "ValidateBoxes";
                public readonly string ValidatePallet = "ValidatePallet";
            }
            public readonly string _palletDetailsPartial = "~/Areas/BoxManager/Views/MovePallet/_palletDetailsPartial.cshtml";
            public readonly string Destination = "~/Areas/BoxManager/Views/MovePallet/Destination.cshtml";
            public readonly string Index = "~/Areas/BoxManager/Views/MovePallet/Index.cshtml";
            public readonly string ValidateBoxes = "~/Areas/BoxManager/Views/MovePallet/ValidateBoxes.cshtml";
            public readonly string ValidatePallet = "~/Areas/BoxManager/Views/MovePallet/ValidatePallet.cshtml";
        }
    }

    [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
    public partial class T4MVC_MovePalletController : DcmsMobile.BoxManager.Areas.BoxManager.Controllers.MovePalletController
    {
        public T4MVC_MovePalletController() : base(Dummy.Instance) { }

        [NonAction]
        partial void IndexOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, DcmsMobile.BoxManager.Repository.Sound sound);

        [NonAction]
        public override System.Web.Mvc.ActionResult Index(DcmsMobile.BoxManager.Repository.Sound sound)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.Index);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "sound", sound);
            IndexOverride(callInfo, sound);
            return callInfo;
        }

        [NonAction]
        partial void MovePalletOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, string id);

        [NonAction]
        public override System.Web.Mvc.ActionResult MovePallet(string id)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.MovePallet);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "id", id);
            MovePalletOverride(callInfo, id);
            return callInfo;
        }

        [NonAction]
        partial void SourcePalletOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, DcmsMobile.BoxManager.ViewModels.MovePallet.IndexViewModel model);

        [NonAction]
        public override System.Web.Mvc.ActionResult SourcePallet(DcmsMobile.BoxManager.ViewModels.MovePallet.IndexViewModel model)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.SourcePallet);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "model", model);
            SourcePalletOverride(callInfo, model);
            return callInfo;
        }

        [NonAction]
        partial void ValidatePalletOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, string palletId);

        [NonAction]
        public override System.Web.Mvc.ActionResult ValidatePallet(string palletId)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.ValidatePallet);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "palletId", palletId);
            ValidatePalletOverride(callInfo, palletId);
            return callInfo;
        }

        [NonAction]
        partial void HandleBoxCountOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, DcmsMobile.BoxManager.ViewModels.MovePallet.ValidatePalletViewModel model);

        [NonAction]
        public override System.Web.Mvc.ActionResult HandleBoxCount(DcmsMobile.BoxManager.ViewModels.MovePallet.ValidatePalletViewModel model)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.HandleBoxCount);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "model", model);
            HandleBoxCountOverride(callInfo, model);
            return callInfo;
        }

        [NonAction]
        partial void ValidateBoxesOnPalletOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, string palletId, DcmsMobile.BoxManager.Repository.Sound sound);

        [NonAction]
        public override System.Web.Mvc.ActionResult ValidateBoxesOnPallet(string palletId, DcmsMobile.BoxManager.Repository.Sound sound)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.ValidateBoxesOnPallet);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "palletId", palletId);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "sound", sound);
            ValidateBoxesOnPalletOverride(callInfo, palletId, sound);
            return callInfo;
        }

        [NonAction]
        partial void ScanBoxOfSourcePalletOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, DcmsMobile.BoxManager.ViewModels.MovePallet.ValidateBoxesViewModel model);

        [NonAction]
        public override System.Web.Mvc.ActionResult ScanBoxOfSourcePallet(DcmsMobile.BoxManager.ViewModels.MovePallet.ValidateBoxesViewModel model)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.ScanBoxOfSourcePallet);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "model", model);
            ScanBoxOfSourcePalletOverride(callInfo, model);
            return callInfo;
        }

        [NonAction]
        partial void DestinationOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, string palletId, DcmsMobile.BoxManager.Repository.Sound sound);

        [NonAction]
        public override System.Web.Mvc.ActionResult Destination(string palletId, DcmsMobile.BoxManager.Repository.Sound sound)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.Destination);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "palletId", palletId);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "sound", sound);
            DestinationOverride(callInfo, palletId, sound);
            return callInfo;
        }

        [NonAction]
        partial void HandleDestinationScanOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, DcmsMobile.BoxManager.ViewModels.MovePallet.DestinationViewModel model);

        [NonAction]
        public override System.Web.Mvc.ActionResult HandleDestinationScan(DcmsMobile.BoxManager.ViewModels.MovePallet.DestinationViewModel model)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.HandleDestinationScan);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "model", model);
            HandleDestinationScanOverride(callInfo, model);
            return callInfo;
        }

    }
}

#endregion T4MVC
#pragma warning restore 1591, 3008, 3009
