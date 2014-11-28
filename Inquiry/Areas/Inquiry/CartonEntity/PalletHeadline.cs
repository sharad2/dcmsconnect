using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DcmsMobile.Inquiry.Areas.Inquiry.CartonEntity
{
    public class PalletHeadLine
    {
        public string PalletId { get; set; }

         public int? TotalCarton { get; set; }

         public DateTime ? MaxAreaChangeDate { get; set; }

         public DateTime ? MinAreaChangeDate { get; set; }

         public int CartonAreaCount { get; set; }

         public string AreaShortName { get; set; }

         public string WarehouseLocationId { get; set; }
    }
}
