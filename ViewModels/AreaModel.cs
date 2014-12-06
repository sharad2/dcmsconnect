using DcmsMobile.PalletLocating.Models;
using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.PalletLocating.ViewModels
{
    public class AreaModel
    {
        public AreaModel(Area area)
        {

            AreaId = area.AreaId;
            AreaShortName = area.ShortName;
            BuildingId = area.BuildingId;
            ReplenishAreaShortName = area.ReplenishAreaShortName;
        }

        [Required(ErrorMessage = "Area is required")]
        public string AreaId { get; set; }

        [Display(Name = "Area")]
        public string AreaShortName { get; set; }

        [Display(Name = "Building")]
        [DisplayFormat(NullDisplayText = "Multiple Bldg")]
        public string BuildingId { get; set; }

        [Display(ShortName = "Replenished From")]
        public string ReplenishAreaShortName { get; set; }
    }

}


/*
    $Id$ 
    $Revision$
    $URL$
    $Header$
    $Author$
    $Date$
*/