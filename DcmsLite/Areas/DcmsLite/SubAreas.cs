using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System.Web.Routing;

namespace DcmsMobile.DcmsLite.Areas.DcmsLite
{
    public class SubAreas
    {
        [Display(ShortName = "DLR", Name = "Receive Cartons", Description = "Select an ASN from the list to receive it.")]
        [UIHint("desktop", "DcmsMobile")]
        public static RouteValueDictionary DcmsLiteReceiveIndex
        {
            get { return MVC_DcmsLite.DcmsLite.Receive.Index().GetRouteValueDictionary(); }
        }


        [Display(ShortName = "DLP", Name = "Print Labels for Pick Wave.", Description = "Select Pick Wave from the list to print labels in batches, these labels will serve as your pick ticket.")]
        [UIHint("desktop", "DcmsMobile")]
        public static RouteValueDictionary DcmsLitePickIndex
        {
            get { return MVC_DcmsLite.DcmsLite.Pick.Index().GetRouteValueDictionary(); }
        }

        [Display(ShortName = "DLV", Name = "Validate Boxes of Pick Wave.", Description = "Scan UCC label to validate the box.")]
        [UIHint("desktop", "DcmsMobile")]
        public static RouteValueDictionary DcmsLiteValidateIndex
        {
            get { return MVC_DcmsLite.DcmsLite.Validation.Index().GetRouteValueDictionary(); }
        }
    }
}