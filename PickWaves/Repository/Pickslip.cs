using System;
using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.PickWaves.Repository
{
    public class Pickslip
    {
        [Key]
        public long PickslipId { get; set; }

        public string CustomerId { get; set; }

        public DateTime? DcCancelDate { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? CancelDate { get; set; }

        public DateTime? PickslipImportDate { get; set; }

        public string PurchaseOrder { get; set; }

        public string CustomerDcId { get; set; }

        public string CustomerStoreId { get; set; }
        
        public string VwhId { get; set; }

        public int OrderedPieces { get; set; }

        public int CurrentPieces { get; set; }

        internal int BoxCount { get; set; }

        /// <summary>
        /// Total number of cancelled boxes in this pickslip
        /// </summary>
        public int CancelledBoxCount { get; set; }

        /// <summary>
        /// Total number of pieces in all cancelled boxes of this pickslip 
        /// </summary>
        public int PiecesInCancelledBoxes { get; set; }

        public bool IsFrozenWave { get; set; }

        public int? Iteration { get; set; }
    }
}