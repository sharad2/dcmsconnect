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

        [DisplayFormat(DataFormatString = "<div style=\"width:1em; height:1em; background-color:{0}; display:inline-block\"></div>", HtmlEncode = false)]
        public string DisplayState
        {
            get
            {
                if (this.CancelDate != null)
                {
                    return "red";
                }
                if (this.VerifyDate != null)
                {
                    return "green";
                }
                if (string.IsNullOrWhiteSpace(AreaId))
                {
                    return "gray";
                }
                return "deepskyblue";
            }
        }

        public string DisplayStateTip
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