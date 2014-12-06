using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.PickWaves.ViewModels.BoxPickPallet
{
    public class BoxPickPalletViewModel : ViewModelBase
    {
        [Required(ErrorMessage = "Pallet limit is required")]
        [Range(minimum: 1, maximum: 999, ErrorMessage = "Pallet limit must be in between 1 to 999")]
        [Display(Name = "Pallet Limit")]
        public int PalletLimit { get; set; }

        [Display(Name = "Pick Wave")]
        public string BucketName { get; set; }

        [Display(Name = "Customer")]
        public string CustomerName { get; set; }
        
        public string CustomerId { get; set; }

        [Display(Name = "Total Boxes")]
        public int TotalBoxes { get; set; }

        [Display(Name = "Frezze")]
        public bool IsFrozen { get; set; }

        [Display(Name = "Boxes Palletized")]
        public int? ExpeditedBoxCount { get; set; }

        [Required(ErrorMessage = "Pallet is required")]
        [RegularExpression(@"^([P|p]\S{1,7})", ErrorMessage = "Pallet Id must begin with P and max length should be less then 9.")]
        [Display(Name = "Scan Pallet")]
        public string PalletId { get; set; }

        public int PercentExpedited
        {
            get
            {
                if (ExpeditedBoxCount == null || ExpeditedBoxCount <= 0)
                {
                    return 0;       // Not full at all
                }
                              var pet = (int)((double)ExpeditedBoxCount * 100 / TotalBoxes);
                              return Math.Min(pet, 100);
            }
        }
        
        public int? BucketId { get; set; }

        public IList<PalletModel> PalletList { get; set; }

        public string PullCartonAreaId { get; set; }

        [Display(Name = "Pull from")]
        public string PullBuildingId { get; set; }

        [Display(Name = "Pitch from")]
        public string PitchBuildingId { get; set; }
    }
}