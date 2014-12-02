using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.Inquiry.Areas.Inquiry.PickslipEntity
{
    public class PickslipSkuModel
    {
        public PickslipSkuModel()
        {

        }

        internal PickslipSkuModel(PickslipSku entity)
        {
            this.SkuId = entity.SkuId;
            this.Style = entity.Style;
            this.Color = entity.Color;
            this.Dimension = entity.Dimension;
            this.SkuSize = entity.SkuSize;
            this.Pieces = entity.Pieces;
            this.VwhId = entity.VwhId;
            this.QualityCode = entity.QualityCode;
            this.RetailPrice = entity.RetailPrice;
            this.MinPiecesPerBox = entity.MinPiecesPerBox;
            this.MaxPiecesPerBox = entity.MaxPiecesPerBox;
            this.PiecesPerPackage = entity.PiecesPerPackage;
        }

        public int SkuId { get; set; }

        [Required(ErrorMessage = "This SKU does not have a style")]
        [DisplayFormat(NullDisplayText = "No Style")]
        [Display(Name = "Style", Order = 2)]
        public string Style { get; set; }

        [Required(ErrorMessage = "This SKU does not have a color")]
        [DisplayFormat(NullDisplayText = "No Color")]
        [Display(Name = "Color", Order = 3)]
        public string Color { get; set; }

        [Required(ErrorMessage = "This SKU does not have a dimension")]
        [DisplayFormat(NullDisplayText = "No Dimension")]
        [Display(Name = "Dim", Order = 4)]
        public string Dimension { get; set; }

        [Required(ErrorMessage = "This SKU does not have a size")]
        [DisplayFormat(NullDisplayText = "No Size")]
        [Display(Name = "Size", Order = 5)]
        public string SkuSize { get; set; }

        [ScaffoldColumn(false)]
        public string DisplaySku
        {
            get
            {
                return string.Format("{0} {1} {2} {3}", this.Style, this.Color, this.Dimension, this.SkuSize);
            }
        }

        [Display(Name = "Pieces", Order = 6)]
        [Range(1, int.MaxValue, ErrorMessage = "SKU Pieces are negative or 0")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? Pieces { get; set; }

        [DisplayFormat(NullDisplayText = "None")]
        [Display(Name = "Quality", Order = 8)]
        public string QualityCode { get; set; }

        [Display(Name = "VWH", Order = 7)]
        public string VwhId { get; set; }

        [Display(Name = "Pieces Per Package", Order = 12)]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? PiecesPerPackage { get; set; }

        [DisplayFormat(DataFormatString = "$ {0:N2}")]
        [Display(Name = "Total Retail Price", Order = 9)]
        public decimal? RetailPrice { get; set; }

        [Display(Name = "Min Pcs Per Box", Order = 10)]
        public int? MinPiecesPerBox { get; set; }

        [Display(Name = "Max Pcs Per Box", Order = 11)]
        public int? MaxPiecesPerBox { get; set; }

        public string DisplayPiecesPerBox
        {
            get
            {
                if(MinPiecesPerBox == null && MaxPiecesPerBox == null)
                {
                    return "Not Mentioned";
                }
                return string.Format("{0:N0} to {1:N0}", MinPiecesPerBox, MaxPiecesPerBox);
            }
        }
    }

}