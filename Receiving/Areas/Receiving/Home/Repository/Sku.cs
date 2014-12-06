using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.Receiving.Areas.Receiving.Home.Repository
{
    /// <summary>
    /// Model for Sku
    /// </summary>
    /// <remarks>
    /// Validation rules exist for each property through attributes
    /// </remarks>
    public class Sku
    {
        public int SkuId { get; set; }

        [Display(ShortName = "Style", Name = "Style")]
        public string Style { get; set; }

        [Display(ShortName = "Color", Name = "Color")]
        public string Color { get; set; }

        [Display(ShortName = "Dim", Name = "Dim")]
        public string Dimension { get; set; }

        [Display(ShortName = "Size", Name = "Size")]
        public string SkuSize { get; set; }

        [Display(Name = "Retail Price($)")]
        [DisplayFormat(NullDisplayText = "Not available")]
        public decimal? SkuPrice { get; set; }

        public override string ToString()
        {
            return string.Format("{0}/{1}/{2}/{3}", this.Style, this.Color, this.Dimension, this.SkuSize);
        }
    }
}




//$Id$