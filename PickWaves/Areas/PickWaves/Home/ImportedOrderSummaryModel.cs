using System.ComponentModel.DataAnnotations;
using DcmsMobile.PickWaves.Helpers;

namespace DcmsMobile.PickWaves.Areas.PickWaves.Home
{
    public class ImportedOrderSummaryModel
    {
        public string CustomerId { get; set; }

        public string CustomerName { get; set; }

        /// <summary>
        /// Is this an active customer
        /// </summary>
        public bool IsCustomerActive { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int PickslipCount { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int PiecesOrdered { get; set; }

        [DisplayFormat(DataFormatString = "${0:N0}")]
        public double DollarsOrdered { get; set; }

        [DataType(DataType.Text)]
        public DateRange DcCancelDateRange { get; set; }

        [DataType(DataType.Text)]
        public DateRange ImportDateRange { get; set; }

        public bool InternationalFlag { get; set; }
    }
}