using System;
using System.ComponentModel.DataAnnotations;
using DcmsMobile.PickWaves.Helpers;

namespace DcmsMobile.PickWaves.Areas.PickWaves.Home
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

        /// <summary>
        /// Boxes are not created of these pieces.
        /// OrderedPieces - this.ExpectedPieces
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

        /// <summary>
        /// Showing highest priority of buckets.
        /// </summary>
        public int MaxPriorityId { get; set; }

        [DataType(DataType.Text)]
        public DateRange DcCancelDateRange { get; set; }

    }
}