using System;
using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.Inquiry.Helpers
{
    internal class PoHeadline
    {
        [Key]
        public string CustomerId { get; set; }

        [Key]
        public int Iteration { get; set; }

        //public string VWhId { get; set; }

        [Key]
        public string PO { get; set; }

        public string CustomerName { get; set; }

        public DateTime? ImportDate { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? DcCancelDate { get; set; }

        public int? PiecesOrdered { get; set; }

        public int? PiecesInBox { get; set; }

        public int TotalPickslip { get; set; }

        //public string BuildingId { get; set; }

        public int TotalPO { get; set; }

        //public int? BoxCount { get; set; }        
        
    }
}