using System;
using System.ComponentModel.DataAnnotations;
using DcmsMobile.PalletLocating.Models;

namespace DcmsMobile.PalletLocating.ViewModels
{
    public class ReplenishmentSuggestionModel
    {
        public ReplenishmentSuggestionModel(ReplenishmentSuggestion entity)
        {
            DestinationLocationId = entity.DestinationLocationId;
            CartonCountAtDestinationLocation = entity.CartonCountAtDestinationLocation;
            CartonCountOnPallet = entity.CartonCountOnPallet;
            MaxCartonsAtDestinationLocation = entity.MaxCartonsAtDestinationLocation;
            PalletIdToPull = entity.PalletIdToPull;
            PalletLocationId = entity.PalletLocationId;
            SkuToPull = new SkuModel(entity.SkuToPull);
            VwhIdToPull = entity.VwhIdToPull;
            MinPickslipId = entity.MinPickslipId;
            MaxPickslipId = entity.MaxPickslipId;
            CountPickslipId = entity.CountPickslipId;
            SkuPriority = entity.SkuPriority;
        }

        [Display(Name = "Pallet")]
        public string PalletIdToPull { get; set; }

        [Display(Name = "# Cartons on Pallet")]
        public int CartonCountOnPallet { get; set; }

        [Display(ShortName = "Dest Location")]
        public string PalletLocationId { get; set; }

        [Display(ShortName = "To Location")]
        public string DestinationLocationId { get; set; }

        [Display(Name = "#Ctn at Dest")]
        public int CartonCountAtDestinationLocation { get; set; }

        [Display(Name = "Capacity")]
        public int MaxCartonsAtDestinationLocation { get; set; }

        public SkuModel SkuToPull { get; set; }

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

        /// <summary>
        /// This is set to bucket priority for SKUs in open buckets
        /// </summary>
        public int SkuPriority { get; set; }

        public string VwhIdToPull { get; set; }

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