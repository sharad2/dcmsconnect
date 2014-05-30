using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using EclipseLibrary.Mvc.ModelBinding;

namespace DcmsMobile.REQ2.ViewModels
{
    public enum ViewTab
    {
        AddSku,
        AssignCartons,
        CartonList
    }

    public class ManageSkuViewModel
    {

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

        [Display(Name = "Target")]
        public RequestViewModel CurrentRequest { get; set; }

        private IEnumerable<RequestSkuViewModel> _requestedSkus;

        /// <summary>
        /// We never return null.
        /// </summary>
        public IEnumerable<RequestSkuViewModel> RequestedSkus
        {
            get
            {
                return _requestedSkus ?? Enumerable.Empty<RequestSkuViewModel>();
            }
            set
            {
                _requestedSkus = value;
            }
        }

        public IEnumerable<AssignedCartonViewModel> AssignedCartonInfo { get; set; }

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