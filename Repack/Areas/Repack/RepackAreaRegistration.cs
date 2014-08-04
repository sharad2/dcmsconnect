using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace DcmsMobile.Repack.Areas.Repack.Controllers
{
    public class RepackAreaRegistration : AreaRegistration
    {
        [Display(Description = "Repack from shelfstock, optionally for conversion. Receive cartons in the absence of ASN. Efficiently create multiple similar cartons.", Name = "Repack", ShortName = "RPK", Order = 100)]
        [UIHint("desktop", "DcmsMobile")]
        public override string AreaName
        {
            get
            {
                return "Repack";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            // Public Url ~/Repack/Conversion reaches the Repack for conversion view
            context.MapRoute(
                 "Repack_conversion",
                "Repack/Conversion",
                new { controller = MVC_Repack.Repack.Home.Name, action = MVC_Repack.Repack.Home.ActionNames.RepackConversion },
                new string[] { typeof(HomeController).Namespace });

            context.MapRoute(
                 "Repack_default",
                "Repack/{controller}/{action}/{id}",
                new { controller = MVC_Repack.Repack.Home.Name, action = MVC_Repack.Repack.Home.ActionNames.Index, id = UrlParameter.Optional },
                new string[] { typeof(HomeController).Namespace }
         );
        }

        //public string DisplayName
        //{
        //    get { return "Repack"; }
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
        //    get { return SupportedPlatforms.Desktop; }
        //}

        //public string Description
        //{
        //    get { return "Repack from shelfstock, optionally for conversion. Receive cartons in the absence of ASN. Efficiently create multiple similar cartons."; }
        //}


        //public IT4MVCActionResult ActivityAction
        //{
        //    get { return null; }
        //}
    }
}



//$Id$