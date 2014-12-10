using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

namespace DcmsMobile.REQ2.Areas.REQ2.Home
{
    public class AssignedCartonModel
    {
        [Display(Name = "SKU")]
        public SkuModel Sku { get; set; }

        [Display(Name = "Total Cartons")]
        public int TotalCartons { get; set; }

        [Display(Name = "Pulled Cartons")]
        public int PulledCartons { get; set; }

        [Display(Name = "Total Pieces")]
        public int TotalPieces { get; set; }

        [Display(Name = "Pulled Pieces")]
        public int PulledPieces { get; set; }
    }

    public enum ViewTab
    {
        AddSku,
        AssignCartons,
        CartonList
    }

    public class ManageSkuViewModel
    {
        public ManageSkuViewModel()
        {

        }

        internal ManageSkuViewModel(PullRequest entity)
        {
            //this.Header = new ManageRequestHeaderModel(entity);
            this.CartonRules = new RequestCartonRulesViewModel
            {
                CartonReceivedDate = entity.CartonReceivedDate,
                PriceSeasonCode = entity.PriceSeasonCode,
                QualityCode = entity.SourceQuality,
                SewingPlantCode = entity.SewingPlantCode,
                BuildingId = entity.BuildingId
            };

            this.AssignedDate = entity.AssignedDate;

            this.ResvId = entity.CtnResvId;
            //this.ReqId = entity.ReqId;
            this.BuildingId = entity.BuildingId;
            this.VirtualWareHouseId = entity.SourceVwhId;
            this.SourceAreaId = entity.SourceAreaId;
            this.SourceAreaShortName = entity.SourceAreaShortName;
            this.DestinationAreaId = entity.DestinationArea;
            this.DestinationAreaShortName = entity.DestinationAreaShortName;
            this.Priorities = Convert.ToInt32(entity.Priority);
            //this.Remarks = entity.Remarks;
            this.RequestedBy = entity.RequestedBy;
            this.OverPullCarton = entity.AllowOverPulling == "O";
            this.IsHung = entity.PackagingPreferance == "H";
            this.RequestForConversion = entity.IsConversionRequest;
            this.TargetQualityCode = entity.TargetQuality;
            this.SaleTypeId = entity.SaleTypeId;
            this.TargetVwhId = entity.TargetVwhId;
        }

        [Display(Name = "Request")]
        public string ResvId { get; set; }

        [Display(Name = "Perform Conversion")]
        public bool RequestForConversion { get; set; }

        [Display(Name = "Assigned?")]
        public DateTime? AssignedDate { get; set; }

        [Display(Name = "Pull to Area", ShortName = "To")]
        public string DestinationAreaShortName { get; set; }

        [DisplayFormat(NullDisplayText = "None")]
        [Display(Name = "Convert To VWh")]
        public string TargetVwhId { get; set; }

        [Display(Name = "Change Quality To")]
        public string TargetQualityCode { get; set; }

        [Display(Name = "Sale Type")]
        public string SaleTypeId { get; set; }

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

        [Display(Name = "Overpulling")]
        public bool OverPullCarton { get; set; }

        [Display(Name = "Hung")]
        public bool IsHung { get; set; }

        [Display(Name = "Requested By")]
        public string RequestedBy { get; set; }

        [Required]
        [Display(Name = "Source Area", ShortName = "From")]
        public string SourceAreaId { get; set; }

        [Required]
        [Display(Name = "Pull to Area", ShortName = "To")]
        public string DestinationAreaId { get; set; }

        [Required]
        [Display(Name = "VWh", ShortName = "VWh")]
        public string VirtualWareHouseId { get; set; }

        [Display(Name = "Building")]
        public string BuildingId { get; set; }

        [Display(Name = "Source Area", ShortName = "From")]
        public string SourceAreaShortName { get; set; }

        public RequestCartonRulesViewModel CartonRules { get; set; }

        #region SourceSKu Property
        [Required]
        //[BindUpperCase]
        public string NewStyle { get; set; }

        [Required]
        //[BindUpperCase]
        public string NewColor { get; set; }

        [Required]
        //[BindUpperCase]
        public string NewDimension { get; set; }

        [Required]
        //[BindUpperCase]
        public string NewSkuSize { get; set; }

        #endregion

        #region TargetSKu Property
      //  [BindUpperCase]
        public string TargetStyle { get; set; }

        //[BindUpperCase]
        public string TargetColor { get; set; }

        //[BindUpperCase]
        public string TargetDimension { get; set; }

        //[BindUpperCase]
        public string TargetSkuSize { get; set; }

        #endregion

        [Required(ErrorMessage = "Pieces are required")]
        [Display(Name = "Pieces")]
        [Range(minimum: 1, maximum: int.MaxValue, ErrorMessage = "Pieces must be greater then or equal to 1")]
        public int? NewPieces { get; set; }

        //[Display(Name = "Target")]
        //public ManageSkuRequestModel CurrentRequest { get; set; }

        private IList<RequestSkuViewModel> _requestedSkus;

        /// <summary>
        /// We never return null.
        /// </summary>
        public IList<RequestSkuViewModel> RequestedSkus
        {
            get
            {
                return _requestedSkus ??  Enumerable.Empty<RequestSkuViewModel>().ToList();                
                //return _requestedSkus;
            }
            set
            {
                _requestedSkus = value;
            }
        }

        public IList<AssignedCartonModel> AssignedCartonInfo { get; set; }

        public IEnumerable<SelectListItem> SewingPlantCodes { get; set; }

        public IEnumerable<SelectListItem> BuildingList { get; set; }

        public IEnumerable<SelectListItem> PriceSeasonCodes { get; set; }

        public IEnumerable<SelectListItem> Qualities { get; set; }

        public bool ShowAssignedCartonTab { get; set; }

        public ViewTab SelectedTab { get; set; }


        /// <summary>
        /// Rkandari
        /// Url of Report 40.16: Shows you the cartons of a particular area. You can see cartons in conversion area from this report.
        /// </summary>
        public string CartonDetailsForStoragAreaUrl
        {
            get
            {

                return System.Configuration.ConfigurationManager.AppSettings["DcmsLiveBaseUrl"] + "Reports/Category_040/R40_16.aspx";
            }
        }

        /// <summary>
        /// Url of Report 30.06: Show SKUs which are to be pulled for CON area.

        /// </summary>
        public string SkuToBePulledUrl
        {
            get
            {
                return System.Configuration.ConfigurationManager.AppSettings["DcmsLiveBaseUrl"] + "Reports/Category_030/R30_06.aspx";
            }
        }
    }
}
//$Id$