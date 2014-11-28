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
namespace DcmsMobile.Inquiry.Areas.Inquiry.BoxEntity
{
    public partial class BoxEntityController
    {
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public BoxEntityController() { }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        protected BoxEntityController(Dummy d) { }

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
        public virtual System.Web.Mvc.ActionResult Box()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.Box);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult BoxExcel()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.BoxExcel);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult PrintBoxUccOrCcl()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.PrintBoxUccOrCcl);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult CancelBox()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.CancelBox);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult BoxPallet()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.BoxPallet);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult PrintBoxesOfPallet()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.PrintBoxesOfPallet);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult BoxPalletExcel()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.BoxPalletExcel);
        }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public BoxEntityController Actions { get { return MVC_Inquiry.Inquiry.BoxEntity; } }
        [GeneratedCode("T4MVC", "2.0")]
        public readonly string Area = "Inquiry";
        [GeneratedCode("T4MVC", "2.0")]
        public readonly string Name = "BoxEntity";
        [GeneratedCode("T4MVC", "2.0")]
        public const string NameConst = "BoxEntity";

        static readonly ActionNamesClass s_actions = new ActionNamesClass();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionNamesClass ActionNames { get { return s_actions; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionNamesClass
        {
            public readonly string Box = "Box";
            public readonly string BoxExcel = "BoxExcel";
            public readonly string PrintBoxUccOrCcl = "PrintBoxUccOrCcl";
            public readonly string CancelBox = "CancelBox";
            public readonly string BoxPallet = "BoxPallet";
            public readonly string PrintBoxesOfPallet = "PrintBoxesOfPallet";
            public readonly string BoxPalletExcel = "BoxPalletExcel";
            public readonly string BoxList = "BoxList";
            public readonly string BoxPalletList = "BoxPalletList";
        }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionNameConstants
        {
            public const string Box = "Box";
            public const string BoxExcel = "BoxExcel";
            public const string PrintBoxUccOrCcl = "PrintBoxUccOrCcl";
            public const string CancelBox = "CancelBox";
            public const string BoxPallet = "BoxPallet";
            public const string PrintBoxesOfPallet = "PrintBoxesOfPallet";
            public const string BoxPalletExcel = "BoxPalletExcel";
            public const string BoxList = "BoxList";
            public const string BoxPalletList = "BoxPalletList";
        }


        static readonly ActionParamsClass_Box s_params_Box = new ActionParamsClass_Box();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_Box BoxParams { get { return s_params_Box; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_Box
        {
            public readonly string id = "id";
            public readonly string showPrintDialog = "showPrintDialog";
        }
        static readonly ActionParamsClass_BoxExcel s_params_BoxExcel = new ActionParamsClass_BoxExcel();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_BoxExcel BoxExcelParams { get { return s_params_BoxExcel; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_BoxExcel
        {
            public readonly string id = "id";
        }
        static readonly ActionParamsClass_PrintBoxUccOrCcl s_params_PrintBoxUccOrCcl = new ActionParamsClass_PrintBoxUccOrCcl();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_PrintBoxUccOrCcl PrintBoxUccOrCclParams { get { return s_params_PrintBoxUccOrCcl; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_PrintBoxUccOrCcl
        {
            public readonly string ucc128Id = "ucc128Id";
            public readonly string printerId = "printerId";
            public readonly string printCcl = "printCcl";
            public readonly string printUcc = "printUcc";
            public readonly string printCatalog = "printCatalog";
        }
        static readonly ActionParamsClass_CancelBox s_params_CancelBox = new ActionParamsClass_CancelBox();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_CancelBox CancelBoxParams { get { return s_params_CancelBox; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_CancelBox
        {
            public readonly string id = "id";
        }
        static readonly ActionParamsClass_BoxPallet s_params_BoxPallet = new ActionParamsClass_BoxPallet();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_BoxPallet BoxPalletParams { get { return s_params_BoxPallet; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_BoxPallet
        {
            public readonly string id = "id";
            public readonly string showPrintDialog = "showPrintDialog";
        }
        static readonly ActionParamsClass_PrintBoxesOfPallet s_params_PrintBoxesOfPallet = new ActionParamsClass_PrintBoxesOfPallet();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_PrintBoxesOfPallet PrintBoxesOfPalletParams { get { return s_params_PrintBoxesOfPallet; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_PrintBoxesOfPallet
        {
            public readonly string palletId = "palletId";
            public readonly string printerId = "printerId";
            public readonly string palletSummary = "palletSummary";
            public readonly string printedBoxes = "printedBoxes";
            public readonly string unprintedBoxes = "unprintedBoxes";
        }
        static readonly ActionParamsClass_BoxPalletExcel s_params_BoxPalletExcel = new ActionParamsClass_BoxPalletExcel();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_BoxPalletExcel BoxPalletExcelParams { get { return s_params_BoxPalletExcel; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_BoxPalletExcel
        {
            public readonly string id = "id";
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
                public readonly string Box = "Box";
                public readonly string BoxList = "BoxList";
                public readonly string BoxPallet = "BoxPallet";
                public readonly string BoxPalletList = "BoxPalletList";
            }
            public readonly string Box = "~/Areas/Inquiry/BoxEntity/Box.cshtml";
            public readonly string BoxList = "~/Areas/Inquiry/BoxEntity/BoxList.cshtml";
            public readonly string BoxPallet = "~/Areas/Inquiry/BoxEntity/BoxPallet.cshtml";
            public readonly string BoxPalletList = "~/Areas/Inquiry/BoxEntity/BoxPalletList.cshtml";
        }
    }

    [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
    public partial class T4MVC_BoxEntityController : DcmsMobile.Inquiry.Areas.Inquiry.BoxEntity.BoxEntityController
    {
        public T4MVC_BoxEntityController() : base(Dummy.Instance) { }

        [NonAction]
        partial void BoxOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, string id, bool showPrintDialog);

        [NonAction]
        public override System.Web.Mvc.ActionResult Box(string id, bool showPrintDialog)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.Box);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "id", id);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "showPrintDialog", showPrintDialog);
            BoxOverride(callInfo, id, showPrintDialog);
            return callInfo;
        }

        [NonAction]
        partial void BoxExcelOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, string id);

        [NonAction]
        public override System.Web.Mvc.ActionResult BoxExcel(string id)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.BoxExcel);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "id", id);
            BoxExcelOverride(callInfo, id);
            return callInfo;
        }

        [NonAction]
        partial void PrintBoxUccOrCclOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, string ucc128Id, string printerId, bool printCcl, bool printUcc, bool printCatalog);

        [NonAction]
        public override System.Web.Mvc.ActionResult PrintBoxUccOrCcl(string ucc128Id, string printerId, bool printCcl, bool printUcc, bool printCatalog)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.PrintBoxUccOrCcl);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "ucc128Id", ucc128Id);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "printerId", printerId);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "printCcl", printCcl);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "printUcc", printUcc);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "printCatalog", printCatalog);
            PrintBoxUccOrCclOverride(callInfo, ucc128Id, printerId, printCcl, printUcc, printCatalog);
            return callInfo;
        }

        [NonAction]
        partial void CancelBoxOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, string id);

        [NonAction]
        public override System.Web.Mvc.ActionResult CancelBox(string id)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.CancelBox);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "id", id);
            CancelBoxOverride(callInfo, id);
            return callInfo;
        }

        [NonAction]
        partial void BoxPalletOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, string id, bool showPrintDialog);

        [NonAction]
        public override System.Web.Mvc.ActionResult BoxPallet(string id, bool showPrintDialog)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.BoxPallet);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "id", id);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "showPrintDialog", showPrintDialog);
            BoxPalletOverride(callInfo, id, showPrintDialog);
            return callInfo;
        }

        [NonAction]
        partial void PrintBoxesOfPalletOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, string palletId, string printerId, bool palletSummary, bool printedBoxes, bool unprintedBoxes);

        [NonAction]
        public override System.Web.Mvc.ActionResult PrintBoxesOfPallet(string palletId, string printerId, bool palletSummary, bool printedBoxes, bool unprintedBoxes)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.PrintBoxesOfPallet);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "palletId", palletId);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "printerId", printerId);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "palletSummary", palletSummary);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "printedBoxes", printedBoxes);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "unprintedBoxes", unprintedBoxes);
            PrintBoxesOfPalletOverride(callInfo, palletId, printerId, palletSummary, printedBoxes, unprintedBoxes);
            return callInfo;
        }

        [NonAction]
        partial void BoxPalletExcelOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, string id);

        [NonAction]
        public override System.Web.Mvc.ActionResult BoxPalletExcel(string id)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.BoxPalletExcel);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "id", id);
            BoxPalletExcelOverride(callInfo, id);
            return callInfo;
        }

        [NonAction]
        partial void BoxListOverride(T4MVC_System_Web_Mvc_ActionResult callInfo);

        [NonAction]
        public override System.Web.Mvc.ActionResult BoxList()
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.BoxList);
            BoxListOverride(callInfo);
            return callInfo;
        }

        [NonAction]
        partial void BoxPalletListOverride(T4MVC_System_Web_Mvc_ActionResult callInfo);

        [NonAction]
        public override System.Web.Mvc.ActionResult BoxPalletList()
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.BoxPalletList);
            BoxPalletListOverride(callInfo);
            return callInfo;
        }

    }
}

#endregion T4MVC
#pragma warning restore 1591, 3008, 3009
