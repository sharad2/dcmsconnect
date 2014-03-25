using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace DcmsMobile.PalletLocating.Areas.PalletLocating
{
    public class PalletLocatingAreaRegistration : AreaRegistration
    {
        [Display(Description = "Locate full pallets manually or under system guidance. Helps in replenishing carton picking areas.",  ShortName = "PLOC", Name = "Pallet Locating", Order = 100)]
        [UIHint("desktop", "DcmsMobile")]
        [UIHint("mobile", "DcmsMobile")]
        public override string AreaName
        {
            get
            {
                return "PalletLocating";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "PalletLocating_default",
                "PalletLocating/{controller}/{action}/{id}",
                new
                {
                    controller = MVC_PalletLocating.PalletLocating.Home.Name,
                    action = MVC_PalletLocating.PalletLocating.Home.ActionNames.Building,
                    id = UrlParameter.Optional
                },
                new[] { typeof(Controllers.HomeController).Namespace }
            );
        }


        //public string DisplayName
        //{
        //    get { return "Pallet Locating"; }
        //}

        //public int DisplayOrder
        //{
        //    get { return 100; }
        //}

        //public IEnumerable<ScanHandlingResult> HandleScan(string scan, string scanType, UrlHelper helper)
        //{
        //    return null;
        //}


        //public SupportedPlatforms Platforms
        //{
        //    get { return SupportedPlatforms.All; }
        //}

        //public string Description
        //{
        //    get { return "Locate full pallets manually or under system guidance. Helps in replenishing carton picking areas."; }
        //}

        //public IT4MVCActionResult ActivityAction
        //{
        //    get { return null; }
        //}
    }
}



/*
    $Id$ 
    $Revision$
    $URL$
    $Header$
    $Author$
    $Date$
*/