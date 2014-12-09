using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.REQ2.Areas.REQ2.Home
{
    public class AssignedCartonViewModel
    {
        [Display(Name = "SKU")]
        public SkuViewModel Sku { get; set; }

        [Display(Name = "Total Cartons")]
        public int TotalCartons { get; set; }

        [Display(Name = "Pulled Cartons")]
        public int PulledCartons { get; set; }

        [Display(Name = "Total Pieces")]
        public int TotalPieces { get; set; }

        [Display(Name = "Pulled Pieces")]
        public int PulledPieces { get; set; }
    }
}
//$Id$