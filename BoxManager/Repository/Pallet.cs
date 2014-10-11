using System;

namespace DcmsMobile.BoxManager.Repository
{
    public class Pallet
    {
        public string PalletId { get; set; }

        public string IaId { get; set; }

        public string LocationId { get; set; }

        public decimal PalletVolume { get; set; }

        public DateTime? IaChangeDate { get; set; }

        public int TotalBoxesOnPallet { get; set; }
    }
}