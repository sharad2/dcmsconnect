using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.REQ2.ViewModels
{
    public class AssignedCartonModel
    {
        [Display(Name = "SKU")]
        public SkuModel Sku { get; set; }

        [Display(Name = "Total Cartons")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int TotalCartons { get; set; }

        [Display(Name = "Pulled Cartons")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int PulledCartons { get; set; }

        [Display(Name = "Total Pieces")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int TotalPieces { get; set; }
    }
}
//$Id$