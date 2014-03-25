
using System;
namespace DcmsMobile.PalletLocating.Models
{
    public class CartonLocation
    {
        /// <summary>
        /// Area of the location
        /// </summary>
        public Area Area { get; set; }

        public string LocationId { get; set; }

        public Sku AssignedSku { get; set; }

        public int? MaxCartons { get; set; }

        /// <summary>
        /// Number of cartons at this location. If this is 0, it means that the location is empty and SkuCount will be 0 as well.
        /// </summary>
        public int CartonCount { get; set; }

        /// <summary>
        /// Number of SKUs at this location. If this is 0, it means that the location is empty.
        /// </summary>
        public int SkuCount { get; set; }

        /// <summary>
        /// SKU of the cartons at this location. If there are multiple SKUs at this location, then this is one of the SKUs.
        /// For empty locations, this is null
        /// </summary>
        public Sku CartonSku { get; set; }

        public DateTime QueryTime { get; set; }

        public bool UnavailableFlag { get; set; }

        public string AssignedVWhId { get; set; }
    }

}


/*
    $Id$ 
    $Revision$
    $URL$
    $Header$
    $Author$
    $Date$
*/