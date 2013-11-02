using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DcmsMobile.REQ2.Models;

namespace DcmsMobile.REQ2.ViewModels
{
    /// <summary>
    /// This view model supplies information for request header editing and creating
    /// </summary>
    public class RequestHeaderModel
    {

        public RequestHeaderModel(Request entity)
        {
            this.ResvId = entity.CtnResvId;
            //this.ReqId = entity.ReqId;
            this.BuildingId = entity.BuildingId;
            this.VirtualWareHouseId = entity.SourceVwhId;
            this.SourceAreaId = entity.SourceAreaId;
            this.SourceAreaShortName = entity.SourceAreaShortName;
            this.DestinationAreaId = entity.DestinationArea;
            this.DestinationAreaShortName = entity.DestinationAreaShortName;
            this.Priority = Convert.ToInt32(string.IsNullOrWhiteSpace(entity.Priority) ? "0" : entity.Priority);
            this.Remarks = entity.Remarks;
            this.RequestedBy = entity.RequestedBy;
            this.TargetQualityCode = entity.TargetQuality;
            this.TargetVwhId = entity.TargetVwhId;
            this.SorceQualityCode = entity.SourceQuality;
            this.SewingPlantCode = entity.SewingPlantCode;
            this.PriceSeasonCode = entity.PriceSeasonCode;
            this.CartonReceivedDate = entity.CartonReceivedDate;
            this.IsConversionRequest = entity.IsConversionRequest;
            this.RowSequence = entity.RowSequence;
        }

        public bool IsConversionRequest { get; set; }

        public RequestHeaderModel()
        {
            //this.OverPullCarton = true;
        }

        [Display(Name = "Request")]
        public string ResvId { get; set; }

        [Display(Name = "Building")]
        public virtual string BuildingId { get; set; }

        [Display(Name = "VWh", ShortName = "VWh")]
        public virtual string VirtualWareHouseId { get; set; }

        [Display(Name = "Pull From Area", ShortName = "From")]
        public virtual string SourceAreaId { get; set; }

        [Display(Name = "Pull From Area", ShortName = "From")]
        public string SourceAreaShortName { get; set; }

        // Target (Task to perform)
        int _priority = 10; //set Default Value here

        [Range(minimum: 1, maximum: 99, ErrorMessage = "Priority must be in between 1 to 99")]
        [Display(Name = "Priority")]
        public virtual int Priority
        {
            get
            {
                return _priority;
            }
            set
            {
                _priority = value;
            }
        }

        [DisplayFormat(NullDisplayText = "None")]
        [Display(Name = "Convert To VWh")]
        public string TargetVwhId { get; set; }

        [Display(Name = "Pull To Area", ShortName = "To")]
        public virtual string DestinationAreaId { get; set; }

        [Display(Name = "Pull To Area", ShortName = "To")]
        public string DestinationAreaShortName { get; set; }

        [Display(Name = "Remarks")]
        public string Remarks { get; set; }

        [Display(Name = "Requested By")]
        public string RequestedBy { get; set; }

        [Display(Name = "Convert To Quality")]
        public string TargetQualityCode { get; set; }

        public bool UpdateQualityFlag { get; set; }

        [Display(Name = "Quality")]
        public string SorceQualityCode { get; set; }

        [Display(Name = "Sewing Plant")]
        public string SewingPlantCode { get; set; }


        [Display(Name = "Price Season")]
        public string PriceSeasonCode { get; set; }

        [Display(Name = "Received on")]
        [DisplayFormat(DataFormatString = "{0:d}", ApplyFormatInEditMode = true)]
        public DateTime? CartonReceivedDate { get; set; }

        /// <summary>
        /// A string which can display the carton filters which will be applied for this request
        /// </summary>
        [DisplayFormat(HtmlEncode = false, NullDisplayText = "None")]
        [Display(Name = "Carton Options")]
        public string CartonFiltersDisplayString
        {
            get
            {
                var tokens = new List<string>();
                if (!string.IsNullOrWhiteSpace(this.SewingPlantCode))
                {
                    tokens.Add(string.Format("Received from Sewing Plant <em>{0}</em>", this.SewingPlantCode));
                }
                if (this.CartonReceivedDate.HasValue)
                {
                    tokens.Add(string.Format("Received On <em>{0:d}</em>", this.CartonReceivedDate));
                }
                if (!string.IsNullOrWhiteSpace(this.PriceSeasonCode))
                {
                    tokens.Add(string.Format("Price Season must be <em>{0}</em>", this.PriceSeasonCode));
                }

                if (!string.IsNullOrWhiteSpace(this.SorceQualityCode))
                {
                    tokens.Add(string.Format("Quality must be <em>{0}</em>", this.SorceQualityCode));
                }

                if (tokens.Count == 0)
                {
                    return null;
                }
                return string.Join(", ", tokens);
            }

        }

        /// <summary>
        /// Row sequence of an appointment. Needed when we update an appointment.
        /// </summary>
        public decimal? RowSequence { get; set; }
    }

    /// <summary>
    /// Contains all available Request properties
    /// </summary>
    public class RequestModel : RequestHeaderModel
    {
        public RequestModel()
        {
        }

        public RequestModel(Request entity)
            : base(entity)
        {

            //this.Header = new RequestHeaderModel(entity);
            this.AssignedCartonCount = entity.AssignedCartonCount;
            this.QuantityRequested = entity.QuantityRequested;
            //this.AssignedFlag = entity.AssignedFlag;
            this.AssignedPieces = entity.AssignedPieces;
            this.DateCreated = entity.DateCreated;
            this.PulledCartons = entity.PulledCartons;
            this.RequestedSkuCount = entity.RequestedSkuCount;
            this.AssignDate = entity.AssignDate;
            this.ReworkCartonCount = entity.ReworkCartonCount;
        }

        [Display(Name = "Quantity Requested", ShortName = "Pieces")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int QuantityRequested { get; set; }

        //[Display(Name = "Assigned?")]
        //[Obsolete]
        //public bool AssignedFlag { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int AssignedCartonCount { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int AssignedPieces { get; set; }

        /// <summary>
        /// Date when request was created
        /// </summary>
        [Display(Name = "Created On")]
        public DateTime? DateCreated { get; set; }

        //public RequestHeaderModel Header { get; set; }

        /// <summary>
        /// Total no of pulled cartons.
        /// </summary>
        public int? PulledCartons { get; set; }

        /// <summary>
        /// Total no of sku added to the request.
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? RequestedSkuCount { get; set; }


        /// <summary>
        /// Date when cartons was assigned to request.
        /// </summary>
        [Display(Name = "Assigned On")]
        public DateTime? AssignDate { get; set; }

        /// <summary>
        /// Total no of carton which needed rework.
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? ReworkCartonCount { get; set; }
    }

}
//$Id$
