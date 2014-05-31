using DcmsMobile.CartonAreas.Repository;
using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.CartonAreas.ViewModels
{
    /// <summary>
    /// Used to display the counts of empty/assigned locations in a specific area
    /// </summary>
    public class LocationCountMatrixViewModel
    {
        public LocationCountMatrixViewModel()
        {

        }

        internal LocationCountMatrixViewModel(CartonArea area)
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

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? CountEmptyAssignedLocations { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? CountNonemptyAssignedLocations { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? CountAssignedLocations { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? CountEmptyUnassignedLocations { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? CountNonemptyUnassignedLocations { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? CountUnassignedLocations { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? CountEmptyLocations { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? CountNonemptyLocations { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? TotalLocations { get; set; }

        public string AreaId { get; set; }
    }
}