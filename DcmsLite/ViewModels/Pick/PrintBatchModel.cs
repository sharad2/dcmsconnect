using System;
using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.DcmsLite.ViewModels.Pick
{
    public class PrintBatchModel
    {
        /// <summary>
        /// This represents a batch of print job for a bunch of UCC labels
        /// <remarks>
        /// Internally this is a PalletId against the Box
        /// </remarks>
        /// </summary>
        [Display(Name="Batch ID")]
        public string BatchId { get; set; }

        /// <summary>
        /// Last print date on which UCC was printed
        /// </summary>
        [Display(Name = "Last Printed Date")]
        public DateTime? LastUccPrintDate { get; set; }

        /// <summary>
        /// labels of this batch was printed by whom
        /// </summary>
        [Display(Name = "Print By")]
        public string PrintedBy { get; set; }

        /// <summary>
        /// This batch contains how many boxes
        /// </summary>
        [Display(Name = "Total Boxes")]
        public int TotalBoxes { get; set; }

        [Display(Name = "Bucket ID")]
        public int? BucketId { get; set; }

        [DisplayFormat(DataFormatString="({0:N0} Not Printed)")]
        public int CountUnprintedBoxes { get; set; }
    }
}