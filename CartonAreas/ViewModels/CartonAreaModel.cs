using DcmsMobile.CartonAreas.Repository;
using System;
using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.CartonAreas.ViewModels
{
    /// <summary>
    /// Represents the display of a specific carton area
    /// </summary>
    public class CartonAreaModel
    {
        public CartonAreaModel()
        {

        }

        internal CartonAreaModel(CartonArea src)
        {
            AreaId = src.AreaId;
            Description = src.Description;
            ShortName = src.ShortName;
            TotalLocations = src.TotalLocations;
            LocationNumberingFlag = src.LocationNumberingFlag;
            IsPalletRequired = src.IsPalletRequired;
            UnusableInventory = src.UnusableInventory;
        }

        public string ShortName { get; set; }

        [Display(Name = "# Locations")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int TotalLocations { get; set; }

        //[Display(Name = "Numbered")]
        //public bool LocationNumberingFlag { get; set; }

        //[DisplayFormat(DataFormatString = "{0:N0}")]
        //public int? CountAssignedLocations { get; set; }

        //[DisplayFormat(DataFormatString = "{0:N0}")]
        //public int? CountUnassignedLocations { get; set; }

        //[DisplayFormat(DataFormatString = "{0:N0}")]
        //public int? CountNonemptyUnassignedLocations { get; set; }

        //[DisplayFormat(DataFormatString = "{0:N0}")]
        //public int? CountEmptyUnassignedLocations { get; set; }

        //[DisplayFormat(DataFormatString = "{0:N0}")]
        //public int? CountNonemptyAssignedLocations { get; set; }

        //[DisplayFormat(DataFormatString = "{0:N0}")]
        //public int? CountEmptyAssignedLocations { get; set; }

        //[DisplayFormat(DataFormatString = "{0:N0}")]
        //public int? CountEmptyLocations { get; set; }

        //[DisplayFormat(DataFormatString = "{0:N0}")]
        //public int? CountNonemptyLocations { get; set; }

        //public bool? AssignedLocationsFlag { get; set; }

        //public bool? EmptyLocationsFlag { get; set; }

        //[Display(Name = "Pallet required")]
        //public bool IsPalletRequired { get; set; }

        //[Display(Name = "Unusable Inventory")]
        //public bool UnusableInventory { get; set; }


        public string AreaId { get; set; }

        public bool LocationNumberingFlag { get; set; }

        public bool IsPalletRequired { get; set; }

        public bool UnusableInventory { get; set; }

        public string Description { get; set; }
    }
}

//$Id$ 
//$Revision$
//$URL$
//$Header$
//$Author$
//$Date$
