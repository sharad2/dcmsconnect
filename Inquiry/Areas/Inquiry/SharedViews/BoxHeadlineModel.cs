using DcmsMobile.Inquiry.Helpers;
using System;
using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.Inquiry.Areas.Inquiry.SharedViews
{
    public class BoxHeadlineModel
    {
        public BoxHeadlineModel()
        {

        }

        internal BoxHeadlineModel(BoxHeadline entity)
        {
            this.Ucc128Id = entity.Ucc128Id;
            this.CurrentPieces = entity.CurrentPieces;
            this.ExpectedPieces = entity.ExpectedPieces;
            this.ValidationDate = entity.VerificationDate;
            this.StopProcessDate = entity.StopProcessDate;
            this.StopProcessReason = entity.StopProcessReason;
            this.PickslipId = entity.PickslipId;
            this.CartonId = entity.CartonId;
            this.BucketId = entity.BucketId;           
            LastLabelPrintDate = entity.LastCclPrintedDate > entity.LastUccPrintedDate ? entity.LastCclPrintedDate : entity.LastUccPrintedDate;
            this.MinPickerName = entity.MinPickerName;
            this.PitchDate = entity.PitchingEndDate;
            this.TruckLoadDate = entity.TruckLoadDate;
            this.CustomerName = entity.CustomerName;
            this.CustomerId = entity.CustomerId;
        }

        public long PickslipId { get; set; }

        public string CartonId { get; set; }

        public int? BucketId { get; set; }

        public DateTime? LastLabelPrintDate { get; set; }

          [DisplayFormat(NullDisplayText = "None")]
        public string MinPickerName { get; set; }

        [Display(Name = "UCC", Order = 1)]
        [Required(ErrorMessage = "Box should have ID")]
        [StringLength(20, MinimumLength = 20, ErrorMessage = "Ucc must be exactly 20 digits")]
        [DataType("Alert")]
        public string Ucc128Id { get; set; }

        [ScaffoldColumn(false)]
        [DisplayFormat(DataFormatString = "{0:g}")]
        public DateTime? StopProcessDate { get; set; }

        [DisplayFormat(DataFormatString = "{0:g}")]
        [Display(ShortName = "Validated On", Order = 4)]
        [DataType(DataType.DateTime)]
        public DateTimeOffset? ValidationDate { get; set; }

        [Display(Name = "Expected Pieces", ShortName = "Pcs Expected", Order = 3)]
        [DisplayFormat(DataFormatString = "{0:N0}", NullDisplayText = "0")]
        public int? ExpectedPieces { get; set; }

        [Display(Name = "Current Pieces", ShortName = "Pcs Pitched", Order = 2)]
        [DisplayFormat(DataFormatString = "{0:N0}", NullDisplayText = "0")]
        [Range(0, int.MaxValue, ErrorMessage = "Negative pieces in box")]
        public int? CurrentPieces { get; set; }

        [ScaffoldColumn(false)]
        public string StopProcessReason { get; set; }

        [ScaffoldColumn(false)]
        public string DisplayPieces
        {
            get
            {
                return string.Format("{0} of {1} ", this.CurrentPieces != null ? this.CurrentPieces.ToString() : "0", this.ExpectedPieces != null ? ExpectedPieces.ToString() : "0");
            }
        }

        public int PctPiecesPicked
        {
            get
            {
                if ((this.ExpectedPieces ?? 0) == 0)
                {
                    return 0;
                }
                return (int)Math.Round(((double)(this.CurrentPieces ?? 0) / (double)this.ExpectedPieces.Value) * 100);
            }
        }

        [DisplayFormat(DataFormatString = "{0:g}", NullDisplayText = "None")]
        public DateTimeOffset? PitchDate { get; set; }

        [DisplayFormat(DataFormatString = "{0:g}")]
        public DateTimeOffset? TruckLoadDate { get; set; }


        public string CustomerName { get; set; }

        public string CustomerId { get; set; }
    }
}