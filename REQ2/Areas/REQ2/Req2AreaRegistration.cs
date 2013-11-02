using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace DcmsMobile.REQ2.Areas.REQ2
{
    public class REQ2AreaRegistration : AreaRegistration
    {
        [Display(ShortName = "REQ", Description = "Create requests for carton pulling. Pull requests for conversion are supported.", Name = "Pull Requests", Order = 100)]
        [UIHint("mobile", "DcmsMobile")]
        [UIHint("desktop", "DcmsMobile")]
        public override string AreaName
        {
            get
            {
                return "REQ2";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "REQ2_default",
                "REQ2/{controller}/{action}/{id}",
                new
                {
                    controller = MVC_REQ2.REQ2.Req.Name,
                    action = MVC_REQ2.REQ2.Home.ActionNames.Index,
                    id = UrlParameter.Optional
                },
                new[] { typeof(Controllers.HomeController).Namespace }
            );
        }
    }
}
//$Id$
