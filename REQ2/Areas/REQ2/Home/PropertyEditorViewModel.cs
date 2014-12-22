using System.Collections.Generic;
using System.Web.Mvc;
using EclipseLibrary.Mvc.Html;
using System.ComponentModel.DataAnnotations;
using System;

namespace DcmsMobile.REQ2.Areas.REQ2.Home
{
    public class PropertyEditorViewModel
    {
        public PropertyEditorViewModel()
        {

        }

        internal PropertyEditorViewModel(PullRequest entity)
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
           //this.IsHung = entity.PackagingPreferance == "H";
            this.RequestForConversion = entity.IsConversionRequest;
            this.SourceQualityCode = entity.SourceQuality;
            this.TargetQualityCode = entity.TargetQuality;
            //this.SaleTypeId = entity.SaleTypeId;
            this.TargetVwhId = entity.TargetVwhId;

        }
        //public RequestHeaderViewModel CurrentRequest { get; set; }

        public IEnumerable<SelectListItem> BuildingList { get; set; }

        public IEnumerable<SelectListItem> TargetQualityCodeList { get; set; }

        public IEnumerable<SelectListItem> SourceQualityCodeList { get; set; }

        public IEnumerable<SelectListItem> VirtualWareHouseList { get; set; }

        public IEnumerable<GroupSelectListItem> DestinationAreas { get; set; }

        public IEnumerable<GroupSelectListItem> SourceAreas { get; set; }

        public IEnumerable<SelectListItem> TargetVwhList { get; set; }

        public IEnumerable<SelectListItem> SaleTypes { get; set; }

        public string PriorityId { get; set; }

        [Display(Name = "Overpulling")]
        public bool OverPullCarton { get; set; }

        [Display(Name = "Building")]
        public string BuildingId { get; set; }

        [Display(Name = "Request")]
        public string ResvId { get; set; }

        [Required]
        [Display(Name = "Destination Area", ShortName = "To")]
        public string DestinationAreaId { get; set; }

        [Display(Name = "Hung")]
        public bool IsHung { get; set; }

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

        [Display(Name = "Remarks")]
        public string Remarks { get; set; }

        [Display(Name = "Sale Type")]
        public string SaleTypeId { get; set; }

        [Required]
        [Display(Name = "Source Area", ShortName = "From")]
        public string SourceAreaId { get; set; }

        [DisplayFormat(NullDisplayText = "None")]
        [Display(Name = "Convert To VWh")]
        public string TargetVwhId { get; set; }

        [Display(Name = "Perform Conversion")]
        public bool RequestForConversion { get; set; }

        [Required]
        [Display(Name = "VWh", ShortName = "VWh")]
        public string VirtualWareHouseId { get; set; }

        [Display(Name = "Source Quality")]
        public string SourceQualityCode { get; set; }

        [Display(Name = "Change Quality To")]
        public string TargetQualityCode { get; set; }

        [Display(Name = "Pull to Area", ShortName = "To")]
        public string DestinationAreaShortName { get; set; }

        [Display(Name = "Source Area", ShortName = "From")]
        public string SourceAreaShortName { get; set; }


        [Display(Name = "Requested By")]
        public string RequestedBy { get; set; }

        public bool UpdateQualityFlag { get; set; }

    }
}
//$Id$