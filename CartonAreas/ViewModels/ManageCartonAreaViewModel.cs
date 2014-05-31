using DcmsMobile.CartonAreas.Repository;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.CartonAreas.ViewModels
{
    /// <summary>
    /// Used to display the counts of empty/assigned locations in a specific area
    /// </summary>
    public class ManageCartonAreaMatrixModel
    {
        public ManageCartonAreaMatrixModel()
        {

        }

        internal ManageCartonAreaMatrixModel(CartonArea area)
        {
            AreaId = area.AreaId;
            TotalLocations = area.TotalLocations;
            CountAssignedLocations = area.CountAssignedLocations;
            CountEmptyAssignedLocations = area.CountEmptyAssignedLocations;
            CountEmptyUnassignedLocations = area.CountEmptyUnassignedLocations;
            CountEmptyLocations = area.CountEmptyLocations;
            CountNonemptyAssignedLocations = area.CountNonemptyAssignedLocations;
            CountNonemptyUnassignedLocations = area.CountNonemptyUnassignedLocations;
            CountUnassignedLocations = area.CountUnassignedLocations;
            CountNonemptyLocations = area.CountNonemptyLocations;
        }

        public bool? AssignedLocationsFlag { get; set; }

        public bool? EmptyLocationsFlag { get; set; }

        public int? CountEmptyAssignedLocations { get; set; }

        public int? CountNonemptyAssignedLocations { get; set; }

        public int? CountAssignedLocations { get; set; }

        public int? CountEmptyUnassignedLocations { get; set; }

        public int? CountNonemptyUnassignedLocations { get; set; }

        public int? CountUnassignedLocations { get; set; }

        public int? CountEmptyLocations { get; set; }

        public int? CountNonemptyLocations { get; set; }

        public int? TotalLocations { get; set; }

        public string AreaId { get; set; }
    }

    public class ManageCartonAreaViewModel
    {

        public string BuildingId { get; set; }

        public string ShortName { get; set; }

        public IList<LocationViewModel> Locations { get; set; }

        //[Obsolete]
        //public CartonAreaModel CurrentArea { get; set; }

        public ManageCartonAreaMatrixModel Matrix { get; set; }

        /// <summary>
        /// Used to post the Assign SKU dialog
        /// </summary>
        public AssignSkuViewModel AssignedSku { get; set; }

    }
}

//$Id$ 
//$Revision$
//$URL$
//$Header$
//$Author$
//$Date$
