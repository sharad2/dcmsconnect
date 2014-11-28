using System;
using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.Inquiry.Helpers
{
    /// <summary>
    /// A helper entity which keeps the most basic properties of SKU
    /// </summary>
    public class SkuBase
    {
        [Key]
        public int SkuId { get; set; }

        public string Style { get; set; }

        public string Color { get; set; }

        public string Dimension { get; set; }

        public string SkuSize { get; set; }


    }
}