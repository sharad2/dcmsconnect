using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace DcmsMobile.BoxPick.Areas.BoxPick
{
    public class BoxPickAreaRegistration : AreaRegistration
    {
        [Display(Description = "ADR Pulling. Implements the ADR, ADRE and ADREPPWSS processes.", Name = "Box Picking", Order = 100, ShortName="BPI")]
        [UIHint("mobile", "DcmsMobile")]
        public override string AreaName
        {
            get
            {
                return "BoxPick";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "BoxPick_default",
                "BoxPick/{controller}/{action}/{id}",
                new { 
                    controller = MVC_BoxPick.BoxPick.Home.Name,     // "Home",
                    action = MVC_BoxPick.BoxPick.Home.ActionNames.AcceptBuilding,   // "Index",
                    id = UrlParameter.Optional
                },
                new string[] { typeof(DcmsMobile.BoxPick.Areas.BoxPick.Controllers.HomeController).Namespace /* "DcmsMobile.BoxPick.Areas.BoxPick.Controllers" */ }
            );
        }


        //public string DisplayName
        //{
        //    get { return "Box Picking"; }
        //}

        //public int DisplayOrder
        //{
        //    get { return 100; }
        //}

        //public IEnumerable<ScanHandlingResult> HandleScan(string scan, string scanType, UrlHelper helper)
        //{
        //    if (scanType == "BoxPallet")
        //    {
        //        yield return new ScanHandlingResult
        //                         {
        //                             DisplayText = "Pick Boxes on this pallet",
        //                             Url = helper.Action(MVC_BoxPick.BoxPick.Home.AcceptPallet().AddRouteValue("Scan", scan))
        //                         };
        //    }
        //    yield break;
        //}


        //public SupportedPlatforms Platforms
        //{
        //    get { return SupportedPlatforms.Mobile | SupportedPlatforms.Desktop; }
        //}

        //public string Description
        //{
        //    get { return "ADR Pulling. Implements the ADR, ADRE and ADREPPWSS processes."; }
        //}


        //public IT4MVCActionResult ActivityAction
        //{
        //    get
        //    {
        //        return null;  // MVC_BoxPick.BoxPick.Activity.PendingActivity() as IT4MVCActionResult; 
        //    }
        //}
    }
}




//$Id$
