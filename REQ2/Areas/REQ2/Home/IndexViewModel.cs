using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace DcmsMobile.REQ2.Areas.REQ2.Home
{
    public class RecentRequestModel
    {
        public RecentRequestModel()
        {
            
        }

        internal RecentRequestModel(RequestModel entity)
        {
            this.ResvId = entity.CtnResvId;
            //this.ReqId = entity.ReqId;
            this.BuildingId = entity.BuildingId;
            this.VirtualWareHouseId = entity.SourceVwhId;
            //this.SourceAreaId = entity.SourceAreaId;
            this.SourceAreaShortName = entity.SourceAreaShortName;
            //this.DestinationAreaId = entity.DestinationArea;
            this.DestinationAreaShortName = entity.DestinationAreaShortName;
            this.Priorities = Convert.ToInt32(entity.Priority);
            this.Remarks = entity.Remarks;
            this.RequestedBy = entity.RequestedBy;
            //this.OverPullCarton = entity.AllowOverPulling == "O";
            //this.IsHung = entity.PackagingPreferance == "H";
            this.RequestForConversion = entity.IsConversionRequest;
            //this.TargetQualityCode = entity.TargetQuality;
            //this.SaleTypeId = entity.SaleTypeId;
            //this.TargetVwhId = entity.TargetVwhId;

            //this.Header = new RequestHeaderViewModel(entity);


            this.QuantityRequested = entity.QuantityRequested;

            this.AssignedPieces = entity.AssignedPieces;
            this.DateCreated = entity.DateCreated;
        }

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

        [Display(Name = "Perform Conversion")]
        public bool RequestForConversion { get; set; }

        [Display(Name = "Request")]
        public string ResvId { get; set; }

        [Display(Name = "Building")]
        public string BuildingId { get; set; }

        [Display(Name = "Source Area", ShortName = "From")]
        public string SourceAreaShortName { get; set; }

        [Display(Name = "Pull to Area", ShortName = "To")]
        public string DestinationAreaShortName { get; set; }

        /// <summary>
        /// Date when request was created
        /// </summary>
        [Display(Name = "Created On")]
        public DateTime? DateCreated { get; set; }

        [Display(Name = "Requested By")]
        public string RequestedBy { get; set; }

        [Required]
        [Display(Name = "VWh", ShortName = "VWh")]
        public string VirtualWareHouseId { get; set; }

        [Display(Name = "Remarks")]
        public string Remarks { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int AssignedPieces { get; set; }

        [Display(Name = "Quantity Requested", ShortName = "Pieces")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int QuantityRequested { get; set; }

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

    public class IndexViewModel
    {

        private IList<RecentRequestModel> _recentRequests;

        /// <summary>
        /// We never return null.
        /// </summary>
        public IList<RecentRequestModel> RecentRequests
        {
            get
            {
                return _recentRequests ?? Enumerable.Empty<RecentRequestModel>().ToList();
            }
            set
            {
                _recentRequests = value;
            }
        }

        [Required]
        [Display(Name = "Existing Request ID")]
        public string CtnresvId { get; set; }
    }
}
//$Id$