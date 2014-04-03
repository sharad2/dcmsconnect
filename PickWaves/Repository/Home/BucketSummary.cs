using System;
using System.ComponentModel.DataAnnotations;
using DcmsMobile.PickWaves.Helpers;

namespace DcmsMobile.PickWaves.Repository.Home
{
    /// <summary>
    /// Summarizes information on active buckets by state/customer
    /// </summary>
    public class BucketSummary
    {
        [Key]
        internal ProgressStage BucketState { get; set; }

        [Key]
        public Customer Customer { get; set; }

        /// <summary>
        /// Number of buckets
        /// </summary>
        public int BucketCount { get; set; }

        internal int OrderedPieces { get; set; }

        /// <summary>
        /// Pieces in not cancelled boxes
        /// </summary>
        public int CurrentPieces { get; set; }

        public InventoryArea MaxPitchArea { get; set; }

        public InventoryArea MinPitchArea { get; set; }

        public InventoryArea MaxPullArea { get; set; }

        public InventoryArea MinPullArea { get; set; }

        public int PitchAreaCount { get; set; }

        public int PullAreaCount { get; set; }

        /// <summary>
        /// If this is null in the database, we set it to be same as current pieces. That is, we expect whatever is in there.
        /// Number of pieces for which boxes have been created
        /// </summary>
        public int ExpectedPieces { get; set; }

        ///// <summary>
        ///// Number of pieces which have been cancelled.
        ///// </summary>
        //[Obsolete]
        //public int CancelledPieces { get; set; }

        public int MaxPriorityId { get; set; }
        
        public DateTime MaxDcCancelDate { get; set; }

        public DateTime MinDcCancelDate { get; set; }
    }
}
