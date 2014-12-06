using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.BoxPick.Models
{
    /// <summary>
    /// Represents the properties of an SKU
    /// </summary>
    public class Sku
    {
        [Key]
        public int SkuId { get; set; }

        [Required]
        public string Style { get; set; }

        [Required]
        public string Color { get; set; }

        [Required]
        public string Dimension { get; set; }

        [Required]
        public string SkuSize { get; set; }

        [Display(Name = "SKU")]
        public string DisplayName
        {
            get
            {
                if (string.IsNullOrEmpty(this.Style))
                {
                    return string.Empty;
                }
                else
                {
                    return string.Format("{0},{1},{2},{3}", this.Style, this.Color, this.Dimension, this.SkuSize);
                }
            }
        }
    }
}



//$Id$