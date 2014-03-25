using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.CartonAreas.ViewModels
{
    /// <summary>
    /// Represents the display of a specific carton area
    /// </summary>
    public class CartonAreaModel
    {
        [Display(Name = "Name_AreaId", ResourceType = typeof(Resources.CartonAreasResource))]
        public string AreaId { get; set; }

        [Display(Name = "Name_Description", ResourceType = typeof(Resources.CartonAreasResource))]
        public string Description { get; set; }

        [Display(Name = "Building")]
        [DisplayFormat(NullDisplayText = "(All)")]
        public string BuildingId { get; set; }

        [Display(Name = "# Locations")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int TotalLocations { get; set; }

        [Display(Name = "Numbered")]
        public bool LocationNumberingFlag { get; set; }

        public string ShortName { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? CountAssignedLocations { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? CountUnassignedLocations { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? CountNonemptyUnassignedLocations { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? CountEmptyUnassignedLocations { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? CountNonemptyAssignedLocations { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? CountEmptyAssignedLocations { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? CountEmptyLocations { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? CountNonemptyLocations { get; set; }

        public bool? AssignedLocationsFlag { get; set; }

        public bool? EmptyLocationsFlag { get; set; }

        [Display(Name = "Pallet required")]
        public bool IsPalletRequired { get; set; }

        [Display(Name = "Unusable Inventory")]
        public bool UnusableInventory { get; set; }

    }
}

//$Id$ 
//$Revision$
//$URL$
//$Header$
//$Author$
//$Date$
