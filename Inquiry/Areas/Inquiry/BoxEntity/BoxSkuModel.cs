using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.Inquiry.Areas.Inquiry.BoxEntity
{
    public class BoxSkuModel
    {
        public BoxSkuModel()
        {

        }

        internal BoxSkuModel(BoxSku boxSku)
        {
            this.SkuId = boxSku.SkuId;
            //this.Upc = boxSku.Upc;
            this.Style = boxSku.Style;
            this.Color = boxSku.Color;
            this.Dimension = boxSku.Dimension;
            this.SkuSize = boxSku.SkuSize;
            this.ExtendedPrice = boxSku.ExtendedPrice;
            this.ExpectedPieces = boxSku.ExpectedPieces;
            this.Pieces = boxSku.CurrentPieces;
            //this.MinPicker = boxSku.MinPicker;
            this.VwhId = boxSku.VwhId;
        }

        [Key]
        [ScaffoldColumn(false)]
        [Display(Name = "SKU ID")]
        public int SkuId { get; set; }

        [Required(ErrorMessage = "This SKU does not have a style")]
        [DisplayFormat(NullDisplayText = "No Style")]
        [Display(Name = "Style")]
        [DataType("Alert")]
        public string Style { get; set; }

        [Required(ErrorMessage = "This SKU does not have a color")]
        [DisplayFormat(NullDisplayText = "No Color")]
        [Display(Name = "Color")]
        [DataType("Alert")]
        public string Color { get; set; }

        [Required(ErrorMessage = "This SKU does not have a dimension")]
        [DisplayFormat(NullDisplayText = "No Dimension")]
        [Display(Name = "Dim")]
        [DataType("Alert")]
        public string Dimension { get; set; }

        [Required(ErrorMessage = "This SKU does not have a size")]
        [DisplayFormat(NullDisplayText = "No Size")]
        [Display(Name = "Size")]
        [DataType("Alert")]
        public string SkuSize { get; set; }

        public string VwhId { get; set; }

        [ScaffoldColumn(false)]
        public string DisplaySku
        {
            get
            {
                return string.Format("{0} {1} {2} {3}", this.Style, this.Color, this.Dimension, this.SkuSize);
            }
        }

        [Display(Name = "Expected Pieces",ShortName="Pcs Expected")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? ExpectedPieces { get; set; }

        [Display(Name = "Pieces in Box",ShortName="Pcs in Box")]
        [Range(1, int.MaxValue, ErrorMessage = "SKU Pieces are negative or 0")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        [DataType("Alert")]
        public int? Pieces { get; set; }

        [Display(Name = "Price",ShortName=" $ Price")]
        public decimal? ExtendedPrice { get; set; }

        [Display(Name = "EPC")]
        public IList<string> AllEpc { get; set; }

        //[Display(Name = "Picked By")]
        //public string MinPicker { get; set; }

        /// <summary>
        /// ScaffoldColumn(false) ensures that this column is not displayed in Excel
        /// </summary>
        [ScaffoldColumn(false)]
        public string DisplayPieces
        {
            get
            {
                return string.Format("{0} of {1} ", this.Pieces ?? 0, this.ExpectedPieces ?? 0);
            }
        }
    }
}