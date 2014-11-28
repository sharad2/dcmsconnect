using System;
using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.Inquiry.Areas.Inquiry.IntransitEntity
{
    internal class IntransitShipmentSkuSummary
    {
        [Key]
        public string ShipmentId { get; set; }

        public DateTime? ShipmentDate { get; set; }

        public DateTime? UploadDate { get; set; }

        [Key]
        public string Style { get; set; }

        [Key]
        public string Color { get; set; }

        [Key]
        public string Dimension { get; set; }

        [Key]
        public string SkuSize { get; set; }

        [Key]
        public string VwhId { get; set; }

        public int? ExpectedPieces { get; set; }

        public int? ExpectedCartonCount { get; set; }

        public int? ReceivedPiecesMine { get; set; }

        public int? ReceivedCartonsMine { get; set; }

        public string IntransitType { get; set; }

        public int? ReceivedCtnByBuddies { get; set; }

        public int? ReceivedCtnOfBuddies { get; set; }

        public int? ReceivedPiecesByBuddies { get; set; }

        public int? ReceivedPiecesOfBuddies { get; set; }

        public string MinOtherShipmentId { get; set; }

        public string MaxOtherShipmentId { get; set; }

        public int? CountOtherShipments { get; set; }

        public string MinBuddyShipmentId { get; set; }

        public string MaxBuddyShipmentId { get; set; }

        public int? CountBuddyShipments { get; set; }

        public string SewingPlantCode { get; set; }

        public string SewingPlantName { get; set; }

        
        public int? TotalShipmentCount { get; set; }
    }

}