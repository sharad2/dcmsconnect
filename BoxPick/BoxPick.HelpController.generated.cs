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
namespace DcmsMobile.BoxPick.Areas.BoxPick.Controllers {
    public partial class HelpController {
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public HelpController() { }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        protected HelpController(Dummy d) { }

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


        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public HelpController Actions { get { return MVC_BoxPick.BoxPick.Help; } }
        [GeneratedCode("T4MVC", "2.0")]
        public readonly string Area = "BoxPick";
        [GeneratedCode("T4MVC", "2.0")]
        public readonly string Name = "Help";
        [GeneratedCode("T4MVC", "2.0")]
        public const string NameConst = "Help";

        static readonly ActionNamesClass s_actions = new ActionNamesClass();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionNamesClass ActionNames { get { return s_actions; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionNamesClass {
            public readonly string ShowPalletHelp = "Pallet";
            public readonly string ShowCartonHelp = "Carton";
            public readonly string ShowUccHelp = "Ucc";
            public readonly string ShowSkipUccHelp = "SkipUcc";
            public readonly string ShowPartialPickPalletHelp = "PartialPickPallet";
        }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionNameConstants {
            public const string ShowPalletHelp = "Pallet";
            public const string ShowCartonHelp = "Carton";
            public const string ShowUccHelp = "Ucc";
            public const string ShowSkipUccHelp = "SkipUcc";
            public const string ShowPartialPickPalletHelp = "PartialPickPallet";
        }


        static readonly ViewNames s_views = new ViewNames();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ViewNames Views { get { return s_views; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ViewNames {
            public readonly string CartonHelp = "CartonHelp";
            public readonly string PalletHelp = "PalletHelp";
            public readonly string PartialPickPalletHelp = "PartialPickPalletHelp";
            public readonly string SkipUccHelp = "SkipUccHelp";
            public readonly string UccHelp = "UccHelp";
        }
    }

    [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
    public class T4MVC_HelpController: DcmsMobile.BoxPick.Areas.BoxPick.Controllers.HelpController {
        public T4MVC_HelpController() : base(Dummy.Instance) { }

        public override System.Web.Mvc.ActionResult ShowPalletHelp() {
            var callInfo = new T4MVC_ActionResult(Area, Name, ActionNames.ShowPalletHelp);
            return callInfo;
        }

        public override System.Web.Mvc.ActionResult ShowCartonHelp() {
            var callInfo = new T4MVC_ActionResult(Area, Name, ActionNames.ShowCartonHelp);
            return callInfo;
        }

        public override System.Web.Mvc.ActionResult ShowUccHelp() {
            var callInfo = new T4MVC_ActionResult(Area, Name, ActionNames.ShowUccHelp);
            return callInfo;
        }

        public override System.Web.Mvc.ActionResult ShowSkipUccHelp() {
            var callInfo = new T4MVC_ActionResult(Area, Name, ActionNames.ShowSkipUccHelp);
            return callInfo;
        }

        public override System.Web.Mvc.ActionResult ShowPartialPickPalletHelp() {
            var callInfo = new T4MVC_ActionResult(Area, Name, ActionNames.ShowPartialPickPalletHelp);
            return callInfo;
        }

    }
}

#endregion T4MVC
#pragma warning restore 1591
