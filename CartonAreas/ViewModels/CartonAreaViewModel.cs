using DcmsMobile.CartonAreas.Repository;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.CartonAreas.ViewModels
{
    /// <summary>
    /// Represents the display of a specific carton area. This model is posted when the area is updated.
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

        public string AreaId { get; set; }

        /// <summary>
        /// For only post value
        /// </summary>
        [Obsolete]
        public string BuildingId { get; set; }

        public bool LocationNumberingFlag { get; set; }

        public bool IsPalletRequired { get; set; }

        public bool UnusableInventory { get; set; }

        public string Description { get; set; }
    }

    public class CartonAreaViewModel
    {
        [Display(Name = "Name_AreaList", ResourceType = typeof(Resources.CartonAreasResource))]
        public IList<CartonAreaModel> CartonAreaList { get; set; }

        public string BuildingId { get; set; }

    }
}
//$Id$