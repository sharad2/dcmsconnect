
namespace DcmsMobile.PickWaves.Repository.BoxPickPallet
{
    public class BoxPickBucket
    {
        /// <summary>
        /// Name of the bucket
        /// </summary>
        public string BucketName { get; set; }

        public int CountTotalBox { get; set; }

        /// <summary>
        /// Number of expedited boxes. These are the boxes which have been assigned to a pallet, but may not have been picked yet.
        /// </summary>
        public int? ExpeditedBoxCount { get; set; }

        /// <summary>
        /// Pallet limit to enforce for this bucket
        /// </summary>
        public int PalletLimit { get; set; }

        /// <summary>
        /// Customer for whom the bucket was created
        /// </summary>
        public string MaxCustomerId { get; set; }

        public bool IsFrozen { get; set; }

        public string PullBuildingId { get; set; }

        public string PitchBuildingId { get; set; }
    }
}