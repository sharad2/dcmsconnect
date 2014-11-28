using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DcmsMobile.Inquiry.Areas.Inquiry.SkuEntity
{
    public class SkuHeadlineModel
    {
        public int sku_id { get; set; }

        public string Style { get; set; }

        public string Color { get; set; }

        public string Dimension { get; set; }

        public string Sku_Size { get; set; }

        public string Upc { get; set; }

        [DisplayFormat(DataFormatString = "{0:d}")]
        public DateTime? PickslipOrderDate { get; set; }

        public string DisplaySku
        {
            get
            {
                return string.Format("{0}, {1}, {2}, {3}", this.Style, this.Color, this.Dimension, this.Sku_Size);
            }
        }
    }


    public class SkuListViewModel
    {
        public IList<SkuHeadlineModel> SkuList { get; set; }


    }
}