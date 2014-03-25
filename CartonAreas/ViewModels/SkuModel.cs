using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.CartonAreas.ViewModels
{
    public class SkuModel
    {
        [Display(Name = "SKU")]
        [Required]
        public int? SkuId { get; set; }

        public string Style { get; set; }

        public string Color { get; set; }

        public string Dimension { get; set; }

        public string SkuSize { get; set; }

        [Display(Name = "Upc Code")]
        public string UpcCode { get; set; }
     }
}
//$Id$