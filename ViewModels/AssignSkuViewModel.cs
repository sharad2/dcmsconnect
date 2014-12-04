using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace DcmsMobile.CartonAreas.ViewModels
{
    /// <summary>
    /// Used for updating location assignment
    /// </summary>
    public class AssignSkuViewModel
    {
        [Required]        
        public string LocationId { get; set; }

        public string AreaId { get; set; }

        public string BuildingId { get; set; }

        [Display(Name = "SKU")]
        public int? SkuId { get; set; }

        [Required]
        public string SkuBarCode { get; set; }

        [Display(Name = "Max Cartons")]
        [Range(minimum: 1, maximum: 99999, ErrorMessage = "Number of cartons must be in between 1 to 99999")]
        [Required]
        public int? MaxAssignedCarton { get; set; }

        [Display(Name = "Assigned VWh")]
        [Required]
        public string AssignedVwhId { get; set; }

        public IEnumerable<SelectListItem> VwhList { get; set; }
    }
}


    //$Id$ 
    //$Revision$
    //$URL$
    //$Header$
    //$Author$
    //$Date$
