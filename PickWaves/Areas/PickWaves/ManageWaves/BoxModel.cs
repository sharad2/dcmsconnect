using System;
using System.ComponentModel.DataAnnotations;
using DcmsMobile.PickWaves.Helpers;

namespace DcmsMobile.PickWaves.Areas.PickWaves.ManageWaves
{
    public class BoxModel
    {
        public string Ucc128Id { get; set; }

        internal string AreaId { get; set; }

        public long PickslipId { get; set; }

        [DisplayFormat(DataFormatString = "{0:g}")]
        public DateTimeOffset CreatedDate { get; set; }

        public string CreatedBy { get; set; }        

        [DisplayFormat(DataFormatString = "{0:d}")]
        public DateTimeOffset? VerifyDate { get; set; }

        [DisplayFormat(DataFormatString = "{0:d}")]
        internal DateTimeOffset? CancelDate { get; set; }

        [DisplayFormat(DataFormatString = "{0:d}")]
        public DateTimeOffset? MaxPitchingEndDate { get; set; }

        [DisplayFormat(NullDisplayText="Pitch")]
        public string CartonId { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? ExpectedPieces { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? CurrentPieces { get; set; }

        [DisplayFormat(DataFormatString = "{0}", HtmlEncode = false)]
        public string DisplayStateStyle
        {
            get
            {
                if (this.CancelDate != null)
                {
                    return "label-danger";
                }
                if (this.VerifyDate != null)
                {
                    return "label-success";
                }
                if (string.IsNullOrWhiteSpace(AreaId))
                {
                    return "label-default";
                }
                return "label-info";
            }
        }

        public string DisplayStateText
        {
            get
            {
                if (this.CancelDate != null)
                {
                    return "Cancelled";
                }
                if (this.VerifyDate != null)
                {
                    return "Completed";
                }
                if (string.IsNullOrWhiteSpace(AreaId))
                {
                    return "Not Started";
                }
                return "In Progress";
            }
        }

        public BoxState State
        {
            get
            {
                if (this.CancelDate != null)
                {
                    return BoxState.Cancelled;
                }
                if (this.VerifyDate != null)
                {
                    return BoxState.Completed;
                }
                return BoxState.InProgress;
            }
        }

        public string VWhId { get; set; }

        public string UrlInquiryBox { get; set; }
    }
}