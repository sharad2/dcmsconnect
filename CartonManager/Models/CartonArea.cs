

namespace DcmsMobile.CartonManager.Models
{
    public class CartonArea
    {
        public string AreaId { get; set; }

        public string Description { get; set; }

        public string ShortName { get; set; }

        /// <summary>
        /// The pallet limit associated with the building of the area. It is null if there is no building, or no limit is defined for the building
        /// </summary>
        public int? PalletLimit { get; set; }
       
        public bool IsNumberedLocationArea { get; set; }

        public string Building { get; set; }
    }
}



//$Id$