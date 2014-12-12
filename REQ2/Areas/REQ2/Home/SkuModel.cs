using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.REQ2.Areas.REQ2.Home
{
    public class SkuModel
    {
        public int? SkuId { get; set; }

        [Display(Name = "Style")]
        public string Style { get; set; }

        [Display(Name = "Color")]
        public string Color { get; set; }

        [Display(Name = "Dim")]
        public string Dimension { get; set; }

        [Display(Name = "Size")]
        public string SkuSize { get; set; }

        public string UpcCode { get; set; }

        [Display(Name = "SKU")]
        public string DisplaySku
        {
            get {
                return SkuId != null ? string.Format(Style + "," + Color + "," + Dimension + "," + SkuSize) : string.Empty;
            }
        }
    }
}
//$Id$
