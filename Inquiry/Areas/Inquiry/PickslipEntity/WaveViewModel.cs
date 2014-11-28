using DcmsMobile.Inquiry.Areas.Inquiry.SharedViews;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.Inquiry.Areas.Inquiry.PickslipEntity
{
   
    public class WaveViewModel
    {

        [Display(Name = "Wave ID")]
        [Required(ErrorMessage = "Wave ID is required")]
        [DataType("Alert")]
        public int BucketId { get; set; }

        [Display(Name = "Name")]
        [Required(ErrorMessage = "Wave Name is required")]
        [DataType("Alert")]
        public string BucketName { get; set; }

        [Display(Name = "Customer Name")]
        [Required(ErrorMessage = "Customer Name is required")]
        [DataType("Alert")]
        public string CustomerName { get; set; }

        public string CustomerId { get; set; }

        public string PitchBuilding { get; set; }

        [Display(Name = "Pick Area")]
        public string PitchArea { get; set; }

        public string PitchAreaDescription { get; set; }

        public string BuildingPullFrom { get; set; }

        public string PullArea { get; set; }

        public string PullAreaDescription { get; set; }

        [Display(Name = "Creation Time")]
        //[DisplayFormat(DataFormatString = "{0:d}")]
        [Required(ErrorMessage = "Date Created is required")]
        [DataType("Alert")]
        public DateTime DateCreated { get; set; }

        [Display(Name = "Created By")]
        [Required(ErrorMessage = "User is required")]
        [DataType("Alert")]
        public string CreatedBy { get; set; }

        [Display(Name = "Pull To Dock")]
        [DataType("Alert")]
        public string PullToDock { get; set; }

        [Display(Name = "Freeze")]
        public bool Freeze { get; set; }


        [Display(Name = "Pick Mode")]
        [DataType("Alert")]
        public string PickMode { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int TotalQuantityOrdered { get; set; }

        public DateTime? DcCancelDate { get; set; }

        public int? Priority { get; set; }


        [Display(Name = "Available for Pitching")]
        public bool AvailableForPitching { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? PickslipCount { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? PoCount { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int TotalSkuCount { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int AssignedSkuCount { get; set; }


        [Display(Name = "Export Order")]
        public bool ExportFlag { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int TotalBoxes { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int VerifiedBoxes { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int PickedPieces { get; set; }

        public string BucketStatus { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? UnavailableBoxCount { get; set; }


        public int PercentPiecesPicked
        {
            get
            {
                if (this.TotalQuantityOrdered == 0)
                {
                    return 0;
                }
                return this.PickedPieces * 100 / this.TotalQuantityOrdered;
            }
        }


        public int PercentBoxVarified
        {
            get
            {
                if (this.TotalBoxes == 0)
                {
                    return 0;
                }
                return this.VerifiedBoxes * 100 / this.TotalBoxes;
            }
        }




        public int UnassignedSkuCount 
        {
            get
            { 
                return this.TotalSkuCount- this.AssignedSkuCount;
            }
        }


        /// <summary>
        /// Url of Report 140.02: This report display details of buckets which are opened for processing.
        /// </summary>
        public string OpenBucketSummaryUrl
        {
            get
            {

                return System.Configuration.ConfigurationManager.AppSettings["DcmsLiveBaseUrl"] + "Reports/Category_140/R140_02.aspx";
            }
        }


        /// <summary>
        /// Url of Report 140.102: For the bucket this report lists all the SKUs of the unprocessed cartons.
        /// </summary>
        public string UnprocessedSkuReportUrl
        {
            get
            {

                return System.Configuration.ConfigurationManager.AppSettings["DcmsLiveBaseUrl"] + "Reports/Category_140/R140_102.aspx";
            }
        }

        /// <summary>
        /// Url of Report 140.05: It displays all SKUs of the bucket for which pieces are short in forward pick area.
        /// </summary>
        public string ForwardPickShortageReportUrl
        {
            get
            {

                return System.Configuration.ConfigurationManager.AppSettings["DcmsLiveBaseUrl"] + "Reports/Category_140/R140_105.aspx";
            }
        }

        /// <summary>
        /// Url of Report 140.05: In ProcessReport
        /// </summary>
        public string InProcessReport
        {
            get
            {
                return System.Configuration.ConfigurationManager.AppSettings["DcmsLiveBaseUrl"] + "Reports/Category_110/R110_07.aspx";
            }
        }

        public string UrlPullBoxes { get; set; }

        public string UrlManagePickwave { get; set; }

        
    }
}