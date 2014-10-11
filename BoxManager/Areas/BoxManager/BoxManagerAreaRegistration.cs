using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
//using System.Web.Optimization;
using DcmsMobile.BoxManager.Areas.BoxManager;

namespace DcmsMobile.BoxManager.Areas
{
    [MetadataType(typeof(SubAreas))]
    public class BoxManagerAreaRegistration : AreaRegistration
    {
        //public const string BOXEDITOR_INDEX_MOBILE_JS = "~/bundles/BoxManager/BoxEditorIndexMobile";
        //public const string BOXEDITOR_INDEX_JS = "~/bundles/BoxManager/BoxEditorIndex";
        //public const string VASCONFIG_INDEX_JS = "~/bundles/BoxManager/VasConfigIndex";
        //public const string VASCONFIG_EDIT_JS = "~/bundles/BoxManager/VasConfigEdit";

        //public const string BOXMANAGER_ALL_CSS = "~/bundles/BoxManager/BoxManager-all";
        //public const string BOXMANAGER_ALL_MOBILE_CSS = "~/bundles/BoxManager/BoxManager-all-mobile";

        [Display(Description = "Creates Pallet, Move Pallets and provide merging of two pallets.", Name = "Box Manager", Order = 100, ShortName = "BOX")]
        [UIHint("desktop", "DcmsMobile")]
        [UIHint("mobile", "DcmsMobile")]
        public override string AreaName
        {
            get
            {
                return "BoxManager";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "BoxManager_default",
                "BoxManager/{controller}/{action}/{id}",
                new { controller = MVC_BoxManager.BoxManager.Home.Name, action = MVC_BoxManager.BoxManager.Home.ActionNames.Index, id = UrlParameter.Optional },
                new[] { typeof(BoxManager.Controllers.HomeController).Namespace }
            );


            //AddBundle(new ScriptBundle(VASCONFIG_INDEX_JS),
            //    Links_BoxManager.Areas.BoxManager.Scripts.AutoComplete_bundle_js);

            //AddBundle(new ScriptBundle(VASCONFIG_EDIT_JS),
            //    Links_BoxManager.Areas.BoxManager.Scripts.VasConfigurationEdit_bundle_js);

            //AddBundle(new ScriptBundle(BOXEDITOR_INDEX_JS),
            //    Links_BoxManager.Areas.BoxManager.Scripts.BoxEditor_bundle_js);

            //AddBundle(new ScriptBundle(BOXEDITOR_INDEX_MOBILE_JS),
            //    Links_BoxManager.Areas.BoxManager.Scripts.Index_mobile_bundle_js);

            //AddBundle(new StyleBundle(BOXMANAGER_ALL_CSS),
            //   Links_BoxManager.Areas.BoxManager.Content.standardized_bundle_css,
            //   Links_BoxManager.Areas.BoxManager.Content.BoxManager_bundle_css);
            
            //AddBundle(new StyleBundle(BOXMANAGER_ALL_MOBILE_CSS),
            //   Links_BoxManager.Areas.BoxManager.Content.SiteMobile_bundle_css,
            //   Links_BoxManager.Areas.BoxManager.Content.BoxManagerMobile_bundle_css);
        }

        //private static void AddBundle(Bundle b, params string[] virtualPaths)
        //{
        //    foreach (var path in virtualPaths)
        //    {
        //        b.Include("~/" + path);
        //    }
        //    BundleTable.Bundles.Add(b);
        //}
    }
}



//$Id: ReceivingAreaRegistration.cs 10910 2011-12-20 04:40:55Z rkandari $