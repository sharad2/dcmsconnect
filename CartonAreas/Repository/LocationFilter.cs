
namespace DcmsMobile.CartonAreas.Repository
{
    /// <summary>
    /// Supplies filters while querying locations. Null value of a property means that the property does not participate as a filter.
    /// </summary>
    public class LocationFilter
    {
        public string CartonAreaId { get; set; }

        /// <summary>
        /// Null means does not matter, true means empty locations only, false means non empty locations only
        /// </summary>
        public bool? EmptyLocations { get; set; }

        //public int? AssignedMaxCarton { get; set; }

        /// <summary>
        /// Null means does not matter, true means assigned locations only, false means non assigned locations only.
        /// If SkuId is given, this is ignored
        /// </summary>
        public bool? AssignedLocations { get; set; }

        public int? SkuId { get; set; }


        /// <summary>
        /// The text entered by the user for SKU
        /// </summary>
        public string SkuEntry { get; set; }

        public string LocationId { get; set; }
    }
}
//$Id$