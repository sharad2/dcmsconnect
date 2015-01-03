using System;

namespace DcmsMobile.PickWaves.Areas.PickWaves.Home
{
    internal class ImportedOrderSummary
    {
        public string CustomerId { get; set; }

        public string CustonerName { get; set; }

        /// <summary>
        /// False if the customer has been marked as inactive
        /// </summary>
        public bool IsActiveCustomer { get; set; }

        public int PickslipCount { get; set; }

        public int PiecesOrdered { get; set; }

        public decimal DollarsOrdered { get; set; }

        public DateTime MinDcCancelDate { get; set; }

        public DateTime MaxDcCancelDate { get; set; }

        public DateTime MinPickslipImportDate { get; set; }

        public DateTime MaxPickslipImportDate { get; set; }
        
        public bool InternationalFlag { get; set; }
    }
}