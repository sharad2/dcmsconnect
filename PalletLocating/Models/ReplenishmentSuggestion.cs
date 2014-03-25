using System;
using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.PalletLocating.Models
{
    /// <summary>
    /// Each suggestion represents a pallet which can be moved from CFD to CPK. Since we only consider pallets containing single SKU,
    /// the SKU is part of the key too.
    /// </summary>
    public class ReplenishmentSuggestion
    {
        [Key]
        public string PalletIdToPull { get; set; }

        [Key]
        public Sku SkuToPull { get; set; }

        /// <summary>
        /// This is set to bucket priority for SKUs in open buckets
        /// </summary>
        public int SkuPriority { get; set; }

        public string VwhIdToPull { get; set; }

        /// <summary>
        /// The location where the pallet exists.
        /// </summary>
        [Key]
        public string PalletLocationId { get; set; }

        /// <summary>
        /// Number of cartons on the pallet. Since only pallets having a single SKU are considered, all these cartons
        /// are guaranteed to contain <see cref="SkuToPull"/>. In the weird case in chich the pallet contains cartons of
        /// multiple locations, this count includes only those cartons which are at <see cref="PalletLocationId"/>
        /// </summary>
        public int CartonCountOnPallet { get; set; }


        /// <summary>
        /// Where should pallet be moved to? This will be a location in CPK which can accommodate this pallet.
        /// </summary>
        public string DestinationLocationId { get; set; }

        /// <summary>
        /// Number of cartons in the destination location in CPK
        /// </summary>
        public int CartonCountAtDestinationLocation { get; set; }

        public int MaxCartonsAtDestinationLocation { get; set; }

        /// <summary>
        /// One of the pickslips for which this SKU is needed
        /// </summary>
        public int? MinPickslipId { get; set; }

        /// <summary>
        /// Another pickslip for which this SKU is needed
        /// </summary>
        public int? MaxPickslipId { get; set; }

        /// <summary>
        /// Number of pickslips for which this SKU is needed
        /// </summary>
        public int CountPickslipId { get; set; }

        public DateTime QueryTime { get; set; }


    }
}



/*
    $Id: Area.Mobile.cshtml 10926 2011-12-20 07:13:50Z bkumar $ 
    $Revision: 10926 $
    $URL: svn://vcs/net4/Projects/Mvc/DcmsMobile.PalletLocating/trunk/PalletLocating/Areas/PalletLocating/Views/Home/Area.Mobile.cshtml $
    $Header: svn://vcs/net4/Projects/Mvc/DcmsMobile.PalletLocating/trunk/PalletLocating/Areas/PalletLocating/Views/Home/Area.Mobile.cshtml 10926 2011-12-20 07:13:50Z bkumar $
    $Author: bkumar $
    $Date: 2011-12-20 12:43:50 +0530 (Tue, 20 Dec 2011) $
*/