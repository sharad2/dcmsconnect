using System;
using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.PickWaves.ViewModels.BoxPickPallet
{
    public class PalletModel
    {
        [Display( Name = "Pallet")]
        public string PalletId { get; set; }

        [Display( Name = "Total Boxes")]
        public int? TotalBoxesOnPallet { get; set; }

        public int PercentPickedBoxes
        {
            get
            {
                if (this.TotalBoxesOnPallet == null || this.TotalBoxesOnPallet <= 0)
                {
                    return 0;       // Not full at all
                }
                var pet = (int)((double)this.PickedBoxes * 100 / (double)this.TotalBoxesOnPallet);
                return Math.Min(pet, 100);
            }
        }

        [Display(Name = "Picked Boxes")]
        public int? PickedBoxes { get; set; }

        [Display(Name = "Print Date")]
        [DisplayFormat(DataFormatString = "{0:d}")]
        public DateTime? PrintDate { get; set; }

        [DisplayFormat(DataFormatString = "{0:d}")]
        public DateTime? IaChangeDate { get; set; }        
    }
}