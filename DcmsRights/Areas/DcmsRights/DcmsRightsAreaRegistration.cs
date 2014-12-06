using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace DcmsMobile.DcmsRights.Areas.DcmsRights
{
    public class DcmsRightsAreaRegistration : AreaRegistration
    {
        [Display(Description = "Create DCMS Users and assign rights to run DCMS programs", Name = "DCMS Rights Assignment", Order = 1000)]
        [UIHint("desktop", "DcmsMobile")]
        public override string AreaName
        {
            get
            {
                return "DcmsRights";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "DcmsRights_default",
                "DcmsRights/{controller}/{action}/{id}",               
                new { controller = MVC_DcmsRights.DcmsRights.Home.Name,
                    action = MVC_DcmsRights.DcmsRights.Home.ActionNames.Index, id = UrlParameter.Optional },
                new [] { typeof(Controllers.HomeController).Namespace }
            );
            //var route = new SearchRoute();
            //context.Routes.Add(route);
        }

        //public string DisplayName
        //{
        //    get { return "DCMS Rights Assignment"; }
        //}

        /// <summary>
        /// We want to display at the end.
        /// All administrative programs should have display order 1000
        /// </summary>
        //public int DisplayOrder
        //{
        //    get { return 1000; }
        //}

        //public IEnumerable<ScanHandlingResult> HandleScan(string scan, string scanType, UrlHelper helper)
        //{
        //    return null;
        //}


        //public SupportedPlatforms Platforms
        //{
        //    get { return SupportedPlatforms.Desktop; }
        //}

        //public IT4MVCActionResult ActivityAction
        //{
        //    get { return null; }
        //}

        //[Obsolete]
        //public string Description
        //{
        //    get { return "Create DCMS Users and assign rights to run DCMS programs"; }
        //}
    }
}
