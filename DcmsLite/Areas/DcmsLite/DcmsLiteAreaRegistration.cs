using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace DcmsMobile.DcmsLite.Areas.DcmsLite
{
    [MetadataType(typeof(SubAreas))]
    public class DcmsLiteAreaRegistration : AreaRegistration
    {
        [Display(Description = "UI for bulk Receiving and Printing the UCC/CCL labels.", Name = "DcmsLite", Order = 100, ShortName = "BPK")]
        [UIHint("desktop", "DcmsMobile")]
        public override string AreaName
        {
            get
            {
                return "DcmsLite";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "DcmsLite_default",
                "DcmsLite/{controller}/{action}/{id}",
                new
                {
                    controller = MVC_DcmsLite.DcmsLite.Home.Name,
                    action = MVC_DcmsLite.DcmsLite.Home.ActionNames.Index,
                    id = UrlParameter.Optional
                },
                new[] { typeof(Controllers.HomeController).Namespace }
            );
        }

    }
}
