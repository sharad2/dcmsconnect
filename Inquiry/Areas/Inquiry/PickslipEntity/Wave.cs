using System;

namespace DcmsMobile.Inquiry.Areas.Inquiry.PickslipEntity
{
    internal class WaveHeadline
    {

        public int BucketId { get; set; }

        public string BucketName { get; set; }

        public DateTime DateCreated { get; set; }

        public bool Freeze { get; set; }

        public string BucketStatus { get; set; }

        public int? Priority { get; set; }

        public string CustomerId { get; set; }

        public string CustomerName { get; set; }

        public int? PickslipCount { get; set; }

        public int? PoCount { get; set; }

        public string PickMode { get; set; }

        public string PitchBuilding { get; set; }

        public string PitchArea { get; set; }

        public string PitchAreaDescription { get; set; }

        public string BuildingPullFrom { get; set; }

        public string PullArea { get; set; }

        public string PullAreaDescription { get; set; }
    }
    internal class Wave : WaveHeadline
    {
        public string CreatedBy { get; set; }

        public string PullToDock { get; set; }

        public int TotalQuantityOrdered { get; set; }

        public DateTime? DcCancelDate { get; set; }

        public bool AvailableForPitching { get; set; }

        public int TotalSkuCount { get; set; }

        public int AssignedSkuCount { get; set; }

        public bool ExportFlag { get; set; }

        public int TotalBoxes { get; set; }

        public int VerifiedBoxes { get; set; }

        public int PickedPieces { get; set; }

        public int? UnavailableBoxCount { get; set; }
    }
}





//$Id$