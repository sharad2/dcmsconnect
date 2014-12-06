using DcmsMobile.PickWaves.Repository;
using System;
using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.PickWaves.Areas.PickWaves.Home
{
    internal class ImportedOrderSummary
    {
        [Key]
        public Customer Customer { get; set; }

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