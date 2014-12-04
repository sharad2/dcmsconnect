
using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.CartonManager.Repository.Locating
{
    public class Location
    {
        [Key]
        public string LocationId { get; set; }

        public int? CountCartons { get; set; }

        /// <summary>
        /// Area to which the location belongs
        /// </summary>
        public string AreaId { get; set; }

        /// <summary>
        /// Short name of area
        /// </summary>
        public string AreaShortName { get; set; }

        /// <summary>
        /// The building to which the location belongs
        /// </summary>
        public string BuildingId { get; set; }

        /// <summary>
        /// UI raises error if this is not a carton location
        /// </summary>
        public string StoresWhat { get; set; }

        public bool UnavailableFlag { get; set; }

        public int? TravelSequence { get; set; }

        public int? MaxCartons { get; set; }

    }
}