using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace DcmsMobile.Inquiry.Areas.Inquiry.ReturnEntity
{
    public class ReturnViewModel
    {
        public string ReturnAuthorizationNumber { get; set; }

        public IList<ReturnReceiptModel> ReturnRecipts { get; set; }

        [DisplayFormat(DataFormatString="{0:N0}")]
        public int TotalReceipts
        {
            get
            {
                return this.ReturnRecipts.Max(p => p.TotalReceipts);
            }
        }

    }
}