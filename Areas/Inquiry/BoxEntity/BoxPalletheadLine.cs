using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DcmsMobile.Inquiry.Areas.Inquiry.BoxEntity
{
   public class BoxPalletheadLine
    {
            public string PalletId { get; set; }

        public int? BoxCount { get; set; }

        public string CustomerId { get; set; }

        public string CustomerName { get; set; }

        public string AreaShortName { get; set; }

        public int? BoxAreaCount { get; set; }

        public string WarehouseLocationId { get; set; }
    }
}
