using System;
using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.Shipping.Repository
{
    public class Po
    {
    
        [Key]
        public string PoId { get; set; }

        [Key]
        public string CustomerId { get; set; }

        [Key]
        public int Iteration { get; set; }

        [Key]
        public string CustomerDcId { get; set; }   

        public int? BucketId { get; set; }

        public string CustomerName { get; set; }            

        public int? PiecesOrdered { get; set; }

        public int? PickedPieces { get; set; }

        public int? NumberOfBoxes { get; set; }

        public DateTime? StartDate { get; set; }

        public decimal? Weight { get; set; }

        public decimal? Volume { get; set; }

        public DateTime? MinDcCancelDate { get; set; }

        public string BuildingId { get; set; }

        public int PoIterationCount { get; set; }

        public int? BuidlingCount { get; set; }

        public bool IsEdiCustomer { get; set; }
    }
}