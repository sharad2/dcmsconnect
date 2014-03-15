using System;
using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.DcmsLite.ViewModels.Ship
{
    public class PoModel
    {
        public string PoId { get; set; }

        public string CustomerId { get; set; }

        public string CustomerDcId { get; set; }

        public int? BucketId { get; set; }

        public string CustomerName { get; set; }

        public int? PiecesOrdered { get; set; }

        public int? PickedPieces { get; set; }

        public int? NumberOfBoxes { get; set; }

        [DisplayFormat(DataFormatString = "{0:d}")]
        public DateTime? StartDate { get; set; }

        [DisplayFormat(DataFormatString = "{0:d}")]
        public DateTime? MinDcCancelDate { get; set; }

        public string BuildingId { get; set; }



    }
}
