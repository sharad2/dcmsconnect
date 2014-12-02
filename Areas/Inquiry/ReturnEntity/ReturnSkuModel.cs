using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.Inquiry.Areas.Inquiry.ReturnEntity
{
    public class ReturnSkuModel 
    {
        public ReturnSkuModel()
        {
            
        }

        #region Sku

        public int SkuId { get; set; }

        [Required(ErrorMessage = "This SKU does not have a UPC")]
        [StringLength(12, MinimumLength = 12, ErrorMessage = "Upc must be exactly 12 digits")]
        [Display(Name = "UPC",Order=1)]
        [DataType("Alert")]
        [DisplayFormat(NullDisplayText = "None")]
        public string Upc { get; set; }

        [Required(ErrorMessage = "This SKU does not have a style")]
        [DisplayFormat(NullDisplayText = "No Style")]
        [Display(Name = "Style", Order = 2)]
        [DataType("Alert")]
        public string Style { get; set; }

        [Required(ErrorMessage = "This SKU does not have a color")]
        [DisplayFormat(NullDisplayText = "No Color")]
        [Display(Name = "Color", Order = 3)]
        [DataType("Alert")]
        public string Color { get; set; }

        [Required(ErrorMessage = "This SKU does not have a dimension")]
        [DisplayFormat(NullDisplayText = "No Dimension")]
        [Display(Name = "Dim", Order = 4,ShortName="Dimension")]
        [DataType("Alert")]
        public string Dimension { get; set; }

        [Required(ErrorMessage = "This SKU does not have a size")]
        [DisplayFormat(NullDisplayText = "No Size")]
        [Display(Name = "Size", Order = 5)]
        [DataType("Alert")]
        public string SkuSize { get; set; }

        [ScaffoldColumn(false)]
        public string DisplaySku
        {
            get
            {
                return string.Format("{0}, {1}, {2}, {3}", this.Style, this.Color, this.Dimension, this.SkuSize);
            }
        }

        [Display(Name = "Pieces Per Package")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        [ScaffoldColumn(false)]
        public int? PiecesPerPackage { get; set; }

        [DisplayFormat(DataFormatString = "${0:N2}")]
        [Display(Name = "Retail Price", ShortName = "Retail Price $", Order = 7)]
        public decimal? RetailPrice { get; set; }

        #endregion

        [Display(Name = "Pieces", Order = 6)]
        [Range(1, int.MaxValue, ErrorMessage = "SKU Pieces are negative or 0")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        [DataType("Alert")]
        public int? Pieces { get; set; }

        [DisplayFormat(NullDisplayText = "None")]
        [Display(Name = "Quality")]
        [ScaffoldColumn(false)]
        public string QualityCode { get; set; }

        [Display(Name = "VWH")]
        [ScaffoldColumn(false)]
        public string VwhId { get; set; }
    }
}