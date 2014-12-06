using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace DcmsMobile.Inquiry.Areas.Inquiry.BoxEntity
{
    public class BoxPalletHeadLineModel
    {
        public string PalletId { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? BoxCount { get; set; }

        public string CustomerId { get; set; }

        public string CustomerName { get; set; }

        public string AreaShortName { get; set; }

        public int? BoxAreaCount { get; set; }

        public string WarehouseLocationId { get; set; }

    }
    public class BoxPalletListViewModel
    {
        public IList<BoxPalletHeadLineModel> BoxPalletList { get; set; }
    }
}
