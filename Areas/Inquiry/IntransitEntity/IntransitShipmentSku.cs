using System;

namespace DcmsMobile.Inquiry.Areas.Inquiry.IntransitEntity
{

    internal class IntransitShipmentSku
    {
        public string ShipmentId { get; set; }

        public string SewingPlantCode { get; set; }

        public string SewingPlantName { get; set; }

        public DateTime? ShipmentDate { get; set; }
        
        public DateTime? MinReceiveDate { get; set; }

        public DateTime? MaxReceiveDate { get; set; }

        public DateTime? CreatedOn { get; set; }

        public int? ExpectedCartonCount { get; set; }

        public int? ExpectedPieces { get; set; }

        public int? ReceivedPieces { get; set; }

        public string Vwh { get; set; }

        public int VwhCount { get; set; }

        public int? IntransitId { get; set; }

        public int? ReceivedCartonCount { get; set; }

        public string Style { get; set; }

        public string Color { get; set; }

        public string Dimension { get; set; }

        public string SkuSize { get; set; }

        public string IsShipmentClosed { get; set; }

        public string IntransitType { get; set; }

        public string ErpId { get; set; }

        public int? UnderReceviedCartonCount { get; set; }

        public int? UnderReceviedPieces { get; set; }

        public int? OverReceviedCartonCount { get; set; }

        public int? CtnsReceivedInOtherShipment { get; set; }

        public int? OverReceviedPieces { get; set; }

        public int? PcsReceivedInOtherShipment { get; set; }

        public DateTime? UploadDate { get; set; }

        public string MinMergedToBuddyShipment { get; set; }

        public string MaxMergedToBuddyShipment { get; set; }

        public int CountMergedToBuddyShipment { get; set; }

        public string MinMergedInBuddyShipment { get; set; }

        public string MaxMergedInBuddyShipment { get; set; }

        public int CountMergedInBuddyShipment { get; set; }
    }
}
