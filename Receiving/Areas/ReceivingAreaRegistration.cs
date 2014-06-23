using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using DcmsMobile.Receiving.Areas.Receiving;

namespace DcmsMobile.Receiving.Areas
{
// ReSharper disable UnusedMember.Global
    public class ReceivingAreaRegistration : AreaRegistration
// ReSharper restore UnusedMember.Global
    {
        [Display(ShortName = "REC", Description = "Receive cartons against an ASN.", Name = "Receiving", Order = 100)]
        [UIHint("desktop", "DcmsMobile")]
        [UIHint("mobile", "DcmsMobile")]
        public override string AreaName
        {
            get
            {
                return "Receiving";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Receiving_default",
                "Receiving/{controller}/{action}/{id}",
                new { controller = MVC_Receiving.Receiving.Home.Name, action = MVC_Receiving.Receiving.Home.ActionNames.Index, id = UrlParameter.Optional },
                new[] { typeof(Receiving.Controllers.HomeController).Namespace }
            );
        }

        public string DisplayName
        {
            get { return "Receiving"; }
        }

        public int DisplayOrder
        {
            get { return 100; }
        }
    }
}



//$Id$