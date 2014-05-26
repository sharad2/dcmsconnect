
namespace DcmsMobile.CartonAreas.Repository
{
    /// <summary>
    /// Count* properties are populated by the query only when a single area is retrieved
    /// </summary>
    internal class CartonArea : Area
    {
        public int TotalLocations { get; set; }

        public bool LocationNumberingFlag { get; set; }

        public bool IsPalletRequired { get; set; }

        public bool UnusableInventory { get; set; }

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
//$Id$