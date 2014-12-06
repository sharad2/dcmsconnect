
using System;

namespace DcmsMobile.PalletLocating.Models
{
    public class Pallet
    {
        /// <summary>
        /// The area of the cartons on this pallet. If the cartons belong to multiple areas, then this is one of the areas.
        /// </summary>
        public Area PalletArea { get; set; }

        public string PalletId { get; set; }

        public int CartonCount { get; set; }

        public int SkuCount { get; set; }

        public int AreaCount { get; set; }

        /// <summary>
        /// The SKUs on the pallet. In the common case, there will be only one SKU per pallet.
        /// If pallet has multiple SKUs, this is one of the SKUs.
        /// </summary>
        public Sku PalletSku { get; set; }

        /// <summary>
        /// The number of locations at which this pallet exists. Should almost always be 1.
        /// </summary>
        public int LocationCount { get; set; }

        /// <summary>
        /// The location where this pallet exists. In the weird case where the pallet contains cartons of multiple locations,
        /// this is one of the locations.
        /// </summary>
        public string LocationId { get; set; }

        /// <summary>
        /// The time at which query was performed
        /// </summary>
        public DateTime QueryTime { get; set; }

        /// <summary>
        /// VWh of Carton on the Pallet
        /// </summary>
        public string CartonVwhId { get; set; }

        /// <summary>
        /// VWh count of all cartons on the pallet
        /// </summary>
        public int CartonVwhCount { get; set; }

        /// <summary>
        /// Quality of Carton on the Pallet
        /// </summary>
        public string CartonQuality { get; set; }

        /// <summary>
        /// Quality count of all cartons on the pallet
        /// </summary>
        public int CartonQualityCount { get; set; }
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