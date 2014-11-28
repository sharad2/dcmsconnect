using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace DcmsMobile.Inquiry.Areas.Inquiry.ReturnEntity
{
    public class ReturnReceiptHeadlineModel
    {
        public string ReturnNumber { get; set; }

        public string CustomerId { get; set; }

        public string CustomerName { get; set; }

        public int TotalReceipts { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? NoOfCartons { get; set; }

        [DisplayFormat(DataFormatString="{0:N0}")]
        public int? ExpectedPieces { get; set; }

          [Display(Name = "Received Date")]
        [DisplayFormat(DataFormatString = "{0:d}")]
        public DateTime ReceivedDate { get; set; }

        public int? CustomerCount { get; set; }
    }
    public class ReturnReceiptListViewModel
    {
        public IList<ReturnReceiptHeadlineModel> ReturnReceiptList { get; set; }
    }
}
