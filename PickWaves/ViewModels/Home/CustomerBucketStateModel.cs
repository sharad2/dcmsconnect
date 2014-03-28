using System;
using System.ComponentModel.DataAnnotations;
using DcmsMobile.PickWaves.Helpers;

namespace DcmsMobile.PickWaves.ViewModels.Home
{
    /// <summary>
    /// Use for manage multiple bucket status
    /// </summary>
    public class CustomerBucketStateModel
    {
        /// <summary>
        /// Status of buckets as Created,InProgress,Completed.
        /// </summary>
        [Key]
        public ProgressStage BucketStatus { get; set; }

        [Key]
        public string CustomerId { get; set; }

        public string CustomerName { get; set; }

        /// <summary>
        /// Is this an active customer
        /// </summary>
        public bool IsCustomerActive { get; set; }

        /// <summary>
        /// All created buckets of any customer.
        /// </summary>
        public int BucketCount { get; set; }

        /// <summary>
        /// Total ordered Pieces
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int OrderedPieces { get; set; }

        /// <summary>
        /// Number of pieces which have already been picked
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int PickedPieces { get; set; }

        ///// <summary>
        ///// Number of pieces which we want to pick for this customer. Regardless of whether they have already been picked or not.
        ///// </summary>
        //[DisplayFormat(DataFormatString = "{0:N0}")]
        //[Obsolete]
        //public int PickablePieces
        //{
        //    get
        //    {
        //        return this.OrderedPieces - this.CancelledPieces;
        //    }
        //}

        /// <summary>
        /// 0 is returned if nothing is picked, or there are no ordered pieces
        /// </summary>
        public int PercentPickedPieces
        {
            get
            {
                if (this.OrderedPieces == 0 || this.PickedPieces == 0)
                {
                    return 0;
                }
                return (int)Math.Round((decimal)(this.PickedPieces) * 100 / (decimal)this.OrderedPieces);
            }
        }

        /// <summary>
        /// Number of pieces for which boxes have been created
        /// </summary>
        internal int ExpectedPieces { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int UnpickedPieces
        {
            get
            {
                return this.OrderedPieces -  this.PickedPieces;
            }
        }

        public int PercentUnpickedPieces
        {
            get
            {
                if (this.OrderedPieces == 0 || this.UnpickedPieces == 0)
                {
                    return 0;
                }
                return (int)Math.Round((decimal)this.UnpickedPieces * 100 / (decimal)this.OrderedPieces);
            }
        }

        ///// <summary>
        ///// Sum of expected pieces in cancelled boxes
        ///// </summary>
        //[DisplayFormat(DataFormatString = "{0:N0}")]
        //[Obsolete]
        //public int CancelledPieces { get; set; }

        /// <summary>
        /// Max(OrderedPieces - CancelledPieces) - this.ExpectedPieces)
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int UncreatedPieces
        {
            get
            {
                var result = this.OrderedPieces - this.ExpectedPieces;
                if (result == 0)
                {
                    result = 0;
                }
                return result;
            }
        }

        public int PitchAreaCount { get; set; }

        public int PullAreaCount { get; set; }

        public InventoryAreaModel MaxPitchArea { get; set; }

        public InventoryAreaModel MinPitchArea { get; set; }

        public InventoryAreaModel MaxPullArea { get; set; }

        public InventoryAreaModel MinPullArea { get; set; }

        /// <summary>
        /// Showing highest priority of buckets.
        /// </summary>
        public int MaxPriorityId { get; set; }

        [DataType(DataType.Text)]
        public DateRange PickingDateRange { get; set; }

        [DataType(DataType.Text)]
        public DateRange DcCancelDateRange { get; set; }

    }
}