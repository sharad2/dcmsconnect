using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.CartonAreas.ViewModels
{
    public class ManageCartonAreaViewModel
    {
        public IList<LocationViewModel> Locations { get; set; }

        public CartonAreaModel CurrentArea { get; set; }

        /// <summary>
        /// Used to post the Assign SKU dialog
        /// </summary>
        public AssignSkuViewModel AssignedSku { get; set; }

        /// <summary>
        /// Specific location to search for
        /// </summary>
        [Display(Name = "Specific Location")]
        [Required(ErrorMessage = "Which location are you looking for?")]
        public string LocationId { get; set; }

        /// <summary>
        /// Locations to which this SKU is assigned are posted
        /// </summary>
        public int? AssignedSkuId { get; set; }

        [Display(Name = "Assigned to SKU")]
        [Required(ErrorMessage = "Locations assigned to the SKU you specify here will be listed")]
        public string AssignedSkuText { get; set; }


    }
}

//$Id$ 
//$Revision$
//$URL$
//$Header$
//$Author$
//$Date$
