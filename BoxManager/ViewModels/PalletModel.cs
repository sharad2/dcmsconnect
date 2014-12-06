using System;
using System.ComponentModel.DataAnnotations;
using DcmsMobile.BoxManager.Repository;

namespace DcmsMobile.BoxManager.ViewModels
{
    public class PalletModel
    {
        public PalletModel() {
        
        }

        public PalletModel(Pallet src) {
            this.PalletId = src.PalletId;
            this.IaId = src.IaId;
            this.PalletVolume = src.PalletVolume;
            this.LocationId = src.LocationId;
            this.TotalBoxesOnPallet = src.TotalBoxesOnPallet;
            this.IaChangeDate = src.IaChangeDate;
        }
        [Display(Name = "Pallet")]
        public string PalletId { get; set; }

        [Display(Name = "Area")]
        public string IaId { get; set; }

        [Display(Name = "Location")]
        public string LocationId { get; set; }

        public decimal PalletVolume { get; set; }

        public DateTime? IaChangeDate { get; set; }

        [Display(Name = "Boxes")]
        public int TotalBoxesOnPallet { get; set; }
    }
}