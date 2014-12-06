using System;
using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.Inquiry.Areas.Inquiry.PickslipEntity
{
    internal class PurchaseOrder
    {
        public string PoId { get; set; }

        public int Iteration { get; set; }

        public string CustomerId { get; set; }

        public string CustomerName { get; set; }

        public DateTime? OrderDate { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? CancelDate { get; set; }

        public DateTime? DcCancelDate { get; set; }

        public int CountOfCclPrinted { get; set; }

        public int CountOfUccPrinted { get; set; }

        public int TotalBoxes { get; set; }

        /// <summary>
        /// How many iterations exist for this PO
        /// </summary>
        public int CountIterations { get; set; }

    }
}


//$Id$