using DcmsMobile.PickWaves.Helpers;
using DcmsMobile.PickWaves.Repository;
using System;
using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.PickWaves.Areas.PickWaves.Home
{
    /// <summary>
    /// Summarizes information on active buckets by state/customer
    /// </summary>
    internal class BucketSummary
    {
        [Key]
        internal ProgressStage BucketState { get; set; }

        //[Key]
        //[Obsolete]
        //public Customer Customer { get; set; }

        public string CustomerId { get; set; }

        public string CustomerName { get; set; }

        /// <summary>
        /// False if the customer has been marked as inactive
        /// </summary>
        public bool IsActiveCustomer { get; set; }

        /// <summary>
        /// Number of buckets
        /// </summary>
        public int BucketCount { get; set; }

        internal int OrderedPieces { get; set; }

        /// <summary>
        /// Pieces in boxes
        /// </summary>
        public int CurrentPieces { get; set; }

        /// <summary>
        /// If this is null in the database, we set it to be same as current pieces. That is, we expect whatever is in there.
        /// Number of pieces for which boxes have been created
        /// </summary>
        public int ExpectedPieces { get; set; }

        public int MaxPriorityId { get; set; }

        public DateTime MaxDcCancelDate { get; set; }

        public DateTime MinDcCancelDate { get; set; }
    }
}
