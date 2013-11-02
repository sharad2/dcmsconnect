using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.REQ2.ViewModels
{
    public class CartonModel
    {
        [Display(Name = "Carton")]
        public string CartonId { get; set; }

        [Display(Name = "Area")]
        public string StoregeArea { get; set; }

        [Display(Name = "AreaShortName")]
        public string AreaShortName { get; set; }

        [Display(Name = "AreaDesc")]
        public string AreaDescription { get; set; }

        [Display(Name = "Quality")]
        public string QualityCode { get; set; }

        [Display(Name = "VwhId")]
        public string VwhId { get; set; }

        [Display(Name = "Pieces")]
        public int Quantity { get; set; }

        [Display(Name = "Rewok Needed")]
        public string ReworkNeeded { get; set; }
    }

    public class CartonListViewModel
    {
        [Required]
        [Display(Name = "Existing Request ID")]
        public string CtnresvId { get; set; }

        public int ReqId { get; set; }

        /// <summary>
        /// We never return null.
        /// </summary>
        public IList<CartonModel> CartonList { get; set; }
    }
}


//$Id$