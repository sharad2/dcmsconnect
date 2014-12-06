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
namespace T4MVC.BoxPick
{
    public class SharedController
    {

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
                public readonly string _bottomPartial = "_bottomPartial";
                public readonly string _BoxList = "_BoxList";
                public readonly string _layoutBoxPick = "_layoutBoxPick";
                public readonly string _layoutBoxPick_mobile = "_layoutBoxPick.mobile";
                public readonly string _PalletStatus = "_PalletStatus";
                public readonly string _soundPartial = "_soundPartial";
                public readonly string _tabsPartial = "_tabsPartial";
            }
            public readonly string _bottomPartial = "~/Areas/BoxPick/Views/Shared/_bottomPartial.cshtml";
            public readonly string _BoxList = "~/Areas/BoxPick/Views/Shared/_BoxList.cshtml";
            public readonly string _layoutBoxPick = "~/Areas/BoxPick/Views/Shared/_layoutBoxPick.cshtml";
            public readonly string _layoutBoxPick_mobile = "~/Areas/BoxPick/Views/Shared/_layoutBoxPick.mobile.cshtml";
            public readonly string _PalletStatus = "~/Areas/BoxPick/Views/Shared/_PalletStatus.cshtml";
            public readonly string _soundPartial = "~/Areas/BoxPick/Views/Shared/_soundPartial.cshtml";
            public readonly string _tabsPartial = "~/Areas/BoxPick/Views/Shared/_tabsPartial.cshtml";
            static readonly _DisplayTemplatesClass s_DisplayTemplates = new _DisplayTemplatesClass();
            public _DisplayTemplatesClass DisplayTemplates { get { return s_DisplayTemplates; } }
            [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
            public partial class _DisplayTemplatesClass
            {
                public readonly string ValueWithShortName = "ValueWithShortName";
            }
            static readonly _EditorTemplatesClass s_EditorTemplates = new _EditorTemplatesClass();
            public _EditorTemplatesClass EditorTemplates { get { return s_EditorTemplates; } }
            [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
            public partial class _EditorTemplatesClass
            {
                public readonly string scan = "scan";
            }
        }
    }

}

#endregion T4MVC
#pragma warning restore 1591, 3008, 3009