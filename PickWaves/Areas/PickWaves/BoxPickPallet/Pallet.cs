using System;

namespace DcmsMobile.PickWaves.Repository.BoxPickPallet
{
    public class Pallet
    {
        public string PalletId { get; set; }

        public int TotalBoxesOnPallet { get; set; }

        public int? PickedBoxes { get; set; }

        public DateTime? PrintDate { get; set; }

        public DateTime? IaChangeDate { get; set; }

        /// <summary>
        /// Pallet contains boxes of this Bucket 
        /// </summary>
        public int? BucketId { get; set; }
    }
}