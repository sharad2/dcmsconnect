using System;
using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.Inquiry.Areas.Inquiry.SkuAreaEntity
{
    public class LocationAuditModel
    {
        public LocationAuditModel()
        {
        }

        internal LocationAuditModel(LocationAudit entity)
        {
            this.ActionPerformed = entity.ActionPerformed;
            this.CreatedBy = entity.CreatedBy;
            this.DateCreated = entity.DateCreated;
            this.ModuleCode = entity.ModuleCode;
            this.Pieces = entity.Pieces;
            this.TransactionPieces = entity.TransactionPieces;
            this.UpcCode = entity.UpcCode;
            this.Style = entity.Style;
            this.Color = entity.Color;
            this.Dimension = entity.Dimension;
            this.SkuSize = entity.SkuSize;
            this.SkuId = entity.SkuId;
        }

        [Display(ShortName = "Date", Order = 3)]
        [DataType(DataType.DateTime)]
        public DateTime? DateCreated { get; set; }

        [Display(ShortName = "Performed By", Order = 2)]
        public string CreatedBy { get; set; }

        [Display(ShortName = "Action",Order=1)]
        public string ActionPerformed { get; set; }

        [Display(ShortName = "Module", Order = 4)]
        public string ModuleCode { get; set; }

        [Display(ShortName = "Available Pieces", Order = 7)]
        [DisplayFormat(DataFormatString="{0:N0}")]
        public int? Pieces { get; set; }

        [Display(ShortName = "Transaction Pieces", Order = 6)]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? TransactionPieces { get; set; }

        [Display(ShortName = "UPC", Order = 5)]
        public string UpcCode { get; set; }

        public string Style { get; set; }

        public string Color { get; set; }

        public string Dimension { get; set; }

        public string SkuSize { get; set; }

        public string DisplaySku
        {
            get
            {
                return string.Format("{0}, {1}, {2}, {3}", Style, Color, Dimension, SkuSize);
            }
        }

        public int SkuId { get; set; }
    }
}
