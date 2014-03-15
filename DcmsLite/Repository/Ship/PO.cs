using System;

namespace DcmsMobile.DcmsLite.Repository.Ship
{
    public class PO
    {
        public string PoId { get; set; }

        public string CustomerId { get; set; }

        public string CustomerDcId { get; set; }

        public int? BucketId { get; set; }

        public string CustomerName { get; set; }

        public int? PiecesOrdered { get; set; }

        public int? PickedPieces { get; set; }

        public int? NumberOfBoxes { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? MinDcCancelDate { get; set; }

        public string BuildingId { get; set; }

    }
}
