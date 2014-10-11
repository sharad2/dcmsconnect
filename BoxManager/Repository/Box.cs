using System;
using System.Collections.Generic;

namespace DcmsMobile.BoxManager.Repository
{
    /// <summary>
    /// Encapsulates information about a box.
    /// </summary>
    public class Box
    {
        /// <summary>
        /// This is the key for box
        /// </summary>
        public string Ucc128Id { get; set; }

        public string IaId { get; set; }

        public string Case { get; set; }

        public string PalletId { get; set; }

        public decimal Volume { get; set; }

        public int? BucketId { get; set; }

        public string PoId { get; set; }

        public string CustomerId { get; set; }

        public string CustomerDcId { get; set; }

        public string SmallShipmentFlag { get; set; }

        public DateTime? ScanToPalletDate { get; set; }

        public DateTime? AppointmentDate { get; set; }

        public DateTime? SuspenseDate { get; set; }

        public DateTime? VerifyDate { get; set; }

        public DateTime? TransferDate { get; set; }

        public DateTime? StopProcessDate { get; set; }

        public string StopProcessReason { get; set; }

        public string RejectionCode { get; set; }

        public string LocationId { get; set; }

        public string ShippingId { get; set; }

        public int? AppointmentNo { get; set; }

        public string DoorId { get; set; }

        public bool IsVasRequired { get; set; }

        public bool IsVasCompleted { get; set; }

        #region box editor properties
        //MinPiecesPerBox and MaxPiecesPerBox are Limit of SKU  pieces in the Box according to customer preference.

        public int? MinPiecesPerBox { get; set; }

        public int? MaxPiecesPerBox { get; set; }

        public string CustomerName { get; set; }

        public DateTime? PitchingEndDate { get; set; }

        public int PickslipId { get; set; }

        public string VwhId { get; set; }

        public string IncompleteVasList { get; set; }

        public string CompletedVasList { get; set; }

        #endregion

    }
}

