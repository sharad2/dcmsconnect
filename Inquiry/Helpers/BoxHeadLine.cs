using System;

namespace DcmsMobile.Inquiry.Helpers
{
    /// <summary>
    /// Returns headline information about a box
    /// </summary>
    internal class BoxHeadline
    {
        public string Ucc128Id { get; set; }

        #region Pickslip Properties
        public long PickslipId { get; set; }

        public int? BucketId { get; set; }


        public string CustomerId { get; set; }


        public string CustomerName { get; set; }

        #endregion


        public string PalletId { get; set; }


        public string IaId { get; set; }

        public string ShortName { get; set; }

        public string Building { get; set; }

        //The property is added because we show's carton on box pallet scan

        public string CartonId { get; set; }


        public DateTimeOffset? VerificationDate { get; set; }

        public DateTimeOffset? TruckLoadDate { get; set; }

        /// <summary>
        /// Not showing year in the display format
        /// </summary>

        public DateTimeOffset? PitchingEndDate { get; set; }



        public int? ExpectedPieces { get; set; }

        public int? CurrentPieces { get; set; }


        public DateTime? LastUccPrintedDate { get; set; }


        public DateTime? LastCclPrintedDate { get; set; }

        public string MinPickerName { get; set; }


        public DateTime? StopProcessDate { get; set; }


        public string StopProcessReason { get; set; }

        /// <summary>
        /// Total number of boxes in the retrieved list
        /// </summary>
        public int TotalBoxes { get; set; }

      
    }
}