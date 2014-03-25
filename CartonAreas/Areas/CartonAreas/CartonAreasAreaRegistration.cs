using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace DcmsMobile.CartonAreas.Areas.CartonAreas
{
    public class CartonAreasAreaRegistration : AreaRegistration
    {
        [Display(Description = "Manage SKU assignments at Carton Locations", Name = "Carton Areas", Order = 500, ShortName="CAM")]
        [UIHint("desktop", "DcmsMobile")]
        public override string AreaName
        {
            get
            {
                return "CartonAreas";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "CartonAreas_default",
                "CartonAreas/{controller}/{action}/{id}",               
                new { controller = MVC_CartonAreas.CartonAreas.Home.Name,
                    action = MVC_CartonAreas.CartonAreas.Home.ActionNames.Index, id = UrlParameter.Optional },
                new [] { typeof(Controllers.HomeController).Namespace }
            );
            //var route = new SearchRoute();
            //context.Routes.Add(route);
        }


        //public string DisplayName
        //{
        //    get { return "Carton Areas"; }
        //}

        ///// <summary>
        ///// We want this program towards the end since it is a managerial program
        ///// </summary>
        //public int DisplayOrder
        //{
        //    get { return 500; }
        //}

        //public IEnumerable<ScanHandlingResult> HandleScan(string scan, string scanType, UrlHelper helper)
        //{
        //    return null;
        //}


        //public SupportedPlatforms Platforms
        //{
        //    get { return SupportedPlatforms.Desktop; } 
        //}

        //public string Description
        //{
        //    get { return "Manage SKU assignments at Carton Locations."; }
        //}

        //public IT4MVCActionResult ActivityAction
        //{
        //    get { return null; }
        //}
    }
}
//$Id$