
namespace DcmsMobile.CartonAreas.Repository
{
    internal class LocationCountMatrix
    {
        public int TotalLocations { get; set; }

        public int? CountAssignedLocations { get; set; }

        public int? CountUnassignedLocations { get; set; }

        public int? CountNonemptyUnassignedLocations { get; set; }

        public int? CountEmptyUnassignedLocations { get; set; }

        public int? CountNonemptyAssignedLocations { get; set; }

        public int? CountEmptyAssignedLocations { get; set; }

        public int? CountEmptyLocations { get; set; }

        public int? CountNonemptyLocations { get; set; }
    }
}