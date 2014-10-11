using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace DcmsMobile.PieceReplenish.Areas.PieceReplenish
{
    [MetadataType(typeof(SubAreas))]
    public class PieceReplenishAreaRegistration : AreaRegistration
    {
        [Display(Description = "Manages Forward Pick replenishment. Includes guided mobile pulling of cartons for restock areas. Restocking cartons to Forward Pick area.",
            Name = "Replenishment", Order = 100, ShortName="RPL")]
        [UIHint("mobile", "DcmsMobile")]
        [UIHint("desktop", "DcmsMobile")]
        public override string AreaName
        {
            get
            {
                return "PieceReplenish";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "PieceReplenish_default",
                "PieceReplenish/{controller}/{action}/{id}",
                new
                {
                    controller = MVC_PieceReplenish.PieceReplenish.Home.Name,
                    action = MVC_PieceReplenish.PieceReplenish.Home.ActionNames.Index,
                    id = UrlParameter.Optional
                },
                new [] { typeof(Controllers.HomeController).Namespace }
            );
        }


        //public string DisplayName
        //{
        //    get { return "Replenishment"; }
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

        //public IT4MVCActionResult ActivityAction
        //{
        //    get { return null; }
        //}

        //public string Description
        //{
        //    get { return "Manages Forward Pick replenishment. Includes guided mobile pulling of cartons for restock areas. Coming soon: Restocking cartons to Forward Pick area."; }
        //}
    }
}



/*
    $Id: PieceReplenishAreaRegistration.cs 17725 2012-07-26 08:18:57Z bkumar $
    $Revision: 17725 $
    $URL: svn://vcs/net4/Projects/Mvc/DcmsMobile.Pull/trunk/Pull/Areas/PieceReplenish/PieceReplenishAreaRegistration.cs $
    $Header: svn://vcs/net4/Projects/Mvc/DcmsMobile.Pull/trunk/Pull/Areas/PieceReplenish/PieceReplenishAreaRegistration.cs 17725 2012-07-26 08:18:57Z bkumar $
    $Author: bkumar $
    $Date: 2012-07-26 13:48:57 +0530 (Thu, 26 Jul 2012) $
*/
