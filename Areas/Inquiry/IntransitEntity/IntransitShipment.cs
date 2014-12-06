using System;

namespace DcmsMobile.Inquiry.Areas.Inquiry.IntransitEntity
{
    internal class IntransitShipment
    {       
        public string ShipmentId { get; set; }

        public string MinBuddyShipmentId { get; set; }

        public string MaxBuddyShipmentId { get; set; }

        public int? CountBuddyShipmentId { get; set; }

        public string SewingPlantCode { get; set; }

        public string SewingPlantName { get; set; }

        public DateTime? ShipmentDate { get; set; }

        public DateTime? MinReceiveDate { get; set; }

        public DateTime? MaxReceiveDate { get; set; }

        public int? ExpectedCartonCount { get; set; }

        public int? ReceivedCartonCount { get; set; }

        public int? UnReceivedCartonCount { get; set; }

        public int? ExpectedPieces { get; set; }

        public int? ReceivedPieces { get; set; }

        public int? UnReceivedPieces { get; set; }        

        /// <summary>
        /// Number of cartons received on behalf of other shipments
        /// </summary>
        public int? BuddyCartonCount { get; set; }

        public int? BuddyReceivedPieces { get; set; }

        public string IntransitType { get; set; }

        public bool IsShipmentClosed { get; set; }

        public DateTime? MinUploadDate { get; set; }

        public DateTime? MaxUploadDate { get; set; }

        public string MinOtherShipmentId { get; set; }

        public string MaxOtherShipmentId { get; set; }

        public int? CountOtherShipmentId { get; set; }

        public int? CountOtherReceivedCarton { get; set; }

        public int? CountOtherReceivedPieces { get; set; }

        public int? TotalShipmentCount { get; set; }
    }
}