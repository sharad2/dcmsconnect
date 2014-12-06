using System;

namespace DcmsMobile.DcmsLite.Repository.Pick
{
    public class PrintBatch
    {
        /// <summary>
        /// This represents a batch of print job for a bunch of UCC labels
        /// <remarks>
        /// Internally this is a PalletId against the Box
        /// </remarks>
        /// </summary>
        public string BatchId { get; set; }

        /// <summary>
        /// Last print date on which UCC was printed
        /// </summary>
        public DateTime? LastUccPrintDate { get; set; }

        /// <summary>
        /// labels of this batch was printed by whom
        /// </summary>
        public string PrintedBy { get; set; }

        /// <summary>
        /// This batch contains how many boxes
        /// </summary>
        public int CountBoxes { get; set; }

        public int CountUnprintedBoxes { get; set; }

        /// <summary>
        /// Box belongs to which Bucket   
        /// </summary>
        public int? BucketId { get; set; }
    }
}