using System;
using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.REQ2.Areas.REQ2.Home
{

    public class RequestCartonRulesViewModel
    {
        [Display(Name = "Quality")]
        public string QualityCode { get; set; }


        [Display(Name = "Sewing Plant")]
        public string SewingPlantCode { get; set; }


        [Display(Name = "Price Season")]
        public string PriceSeasonCode { get; set; }

        [Display(Name = "Received on")]
        [DisplayFormat(DataFormatString = "{0:d}", ApplyFormatInEditMode = true)]
        public DateTime? CartonReceivedDate { get; set; }

        [Display(Name = "Building")]
        public string BuildingId { get; set; }
    }

    /// <summary>
    /// This view model supplies information for request header editing and creating
    /// </summary>
    [Obsolete]
    public class RequestHeaderViewModel
    {

        public RequestHeaderViewModel(RequestModel entity)
        {
            this.ResvId = entity.CtnResvId;
            //this.ReqId = entity.ReqId;
            this.BuildingId = entity.BuildingId;
            this.VirtualWareHouseId = entity.SourceVwhId;
            this.SourceAreaId = entity.SourceAreaId;
            this.SourceAreaShortName = entity.SourceAreaShortName;
            this.DestinationAreaId = entity.DestinationArea;
            this.DestinationAreaShortName = entity.DestinationAreaShortName;
            this.Priorities = Convert.ToInt32(entity.Priority);
            this.Remarks = entity.Remarks;
            this.RequestedBy = entity.RequestedBy;
            this.OverPullCarton = entity.AllowOverPulling == "O";
            this.IsHung = entity.PackagingPreferance == "H";
            this.RequestForConversion = entity.IsConversionRequest;
            this.TargetQualityCode = entity.TargetQuality;
            this.SaleTypeId = entity.SaleTypeId;
            this.TargetVwhId = entity.TargetVwhId;
        }

        public RequestHeaderViewModel()
        {
            this.OverPullCarton = true;
        }

        [Display(Name = "Request")]
        public string ResvId { get; set; }

        /// <summary>
        /// Id of the request in src_req_detail
        /// </summary>
        [Obsolete]
        public int? ReqId { get; set; }

        [Display(Name = "Building")]
        public string BuildingId { get; set; }

        [Required]
        [Display(Name = "VWh", ShortName = "VWh")]
        public string VirtualWareHouseId { get; set; }

        [Required]
        [Display(Name = "Source Area", ShortName = "From")]
        public string SourceAreaId { get; set; }

        [Display(Name = "Source Area", ShortName = "From")]
        public string SourceAreaShortName { get; set; }

        // Target (Task to perform)
        int _priorities = 10; //set Default Value here
        [Required]
        [Range(minimum: 1, maximum: 99, ErrorMessage = "Priority must be in between 1 to 99")]
        [Display(Name = "Priority")]
        public int Priorities
        {
            get
            {
                return _priorities;
            }
            set
            {
                _priorities = value;
            }
        }

        [DisplayFormat(NullDisplayText = "None")]
        [Display(Name = "Convert To VWh")]
        public string TargetVwhId { get; set; }

        [Required]
        [Display(Name = "Pull to Area", ShortName = "To")]
        public string DestinationAreaId { get; set; }

        [Display(Name = "Pull to Area", ShortName = "To")]
        public string DestinationAreaShortName { get; set; }

        [Display(Name = "Overpulling")]
        public bool OverPullCarton { get; set; }

        [Display(Name = "Hung")]
        public bool IsHung { get; set; }

        [Display(Name = "Sale Type")]
        public string SaleTypeId { get; set; }

        [Display(Name = "Remarks")]
        public string Remarks { get; set; }

        [Display(Name = "Requested By")]
        public string RequestedBy { get; set; }

        /// <summary>
        /// It help the manage SKU page to add SKU properly for conversion case also.
        /// </summary>
        /// <remarks>
        /// TODO: This property should be in RequestViewModel
        /// </remarks>
        [Display(Name = "Perform Conversion")]
        public bool RequestForConversion { get; set; }

        [Display(Name = "Change Quality To")]
        public string TargetQualityCode { get; set; }

        public bool UpdateQualityFlag { get; set; }
    }

    /// <summary>
    /// Contains all available Request properties
    /// </summary>
    [Obsolete]
    public class RequestViewModel
    {
        public RequestViewModel()
        {
        }
        public RequestViewModel(RequestModel entity)
        {

            this.Header = new RequestHeaderViewModel(entity);
            this.CartonRules = new RequestCartonRulesViewModel
            {
                CartonReceivedDate = entity.CartonReceivedDate,
                PriceSeasonCode = entity.PriceSeasonCode,
                QualityCode = entity.SourceQuality,
                SewingPlantCode = entity.SewingPlantCode,
                BuildingId = entity.BuildingId
            };
            this.AssignedCartonCount = entity.AssignedCartonCount;
            this.QuantityRequested = entity.QuantityRequested;
            this.AssignedDate = entity.AssignedDate;
            this.AssignedPieces = entity.AssignedPieces;
            this.DateCreated = entity.DateCreated;
        }

        [Display(Name = "Quantity Requested", ShortName = "Pieces")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int QuantityRequested { get; set; }


        [Display(Name = "Assigned?")]
        public DateTime? AssignedDate { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int AssignedCartonCount { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int AssignedPieces { get; set; }

        /// <summary>
        /// Date when request was created
        /// </summary>
        [Display(Name = "Created On")]
        public DateTime? DateCreated { get; set; }

        public RequestHeaderViewModel Header { get; set; }

        public RequestCartonRulesViewModel CartonRules { get; set; }

        public int PercentAssigned
        {
            get
            {
                if (QuantityRequested == 0 || AssignedPieces > QuantityRequested)
                {
                    // We are done
                    return 100;
                }
                return (int)Math.Round((double)AssignedPieces * 100.0 / (double)QuantityRequested);
            }
        }
    }

}
//$Id$