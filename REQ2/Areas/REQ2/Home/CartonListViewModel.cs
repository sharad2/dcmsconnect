using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.REQ2.Areas.REQ2.Home
{
    public class CartonListViewModel
    {
        [Required]
        [Display(Name = "Existing Request ID")]
        public string CtnresvId { get; set; }

        public int ReqId { get; set; }

        [Display(Name = "Carton")]
        public string CartonId { get; set; }

        [Display(Name = "PalletId")]
        public string PalletId { get; set; }

        [Display(Name = "Area")]
        public string StoregeArea { get; set; }

        [Display(Name = "AreaDesc")]
        public string AreaDescription { get; set; }

        [Display(Name = "Quality")]
        public string QualityCode { get; set; }

        [Display(Name = "VwhId")]
        public string VwhId { get; set; }

        [Display(Name = "Pieces")]
        public int Quantity { get; set; }

        /// <summary>
        /// We never return null.
        /// </summary>
        public IList<CartonListViewModel> CartonList { get; set; }

    }
}
//$Id$