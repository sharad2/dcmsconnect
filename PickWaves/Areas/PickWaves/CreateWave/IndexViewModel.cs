using DcmsMobile.PickWaves.ViewModels;
using EclipseLibrary.Mvc.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System.Web.Routing;
using System.Linq;

namespace DcmsMobile.PickWaves.Areas.PickWaves.CreateWave
{
    /// <summary>
    /// The unbinder is capable of handling many properties.
    /// </summary>
    public class IndexViewModel : ViewModelBase
    {
        public IndexViewModel()
        {
        }

        //public IndexViewModel(string customerId, int? bucketId = null)
        //{
        //    CustomerId = customerId;
        //    LastBucketId = bucketId;
        //}

        //internal IndexViewModel(string customerId, PickslipDimension rowDimIndex, PickslipDimension colDimIndex, string vwhId, string pullAreaId, string pitchAreaId, int? lastBucketId)
        //{
        //    CustomerId = customerId;
        //    RowDimIndex = rowDimIndex;
        //    ColDimIndex = colDimIndex;
        //    VwhId = vwhId;
        //    PullAreaId = pullAreaId;
        //    PitchAreaId = pitchAreaId;
        //    LastBucketId = lastBucketId;
        //}

        public string CustomerName { get; set; }

        /// <summary>
        /// Value of dimension
        /// </summary>
        public string DimensionDisplayValue { get; set; }

        /// <summary>
        /// Imported order, customer dc, priority, purchase order etc..
        /// </summary>
        public string DimensionDisplayName { get; set; }

        /// <summary>
        /// Return the bucket Id when bucket is created.
        /// </summary>
        public int? LastBucketId { get; set; }

        #region Posted Values while creating new pick wave

        [Display(Name = "Pulling")]
        public string PullAreaId { get; set; }

        [Display(Name = "Pitching")]
        public string PitchAreaId { get; set; }

        [Display(Name = "Require Box Expediting")]
        public bool RequiredBoxExpediting { get; set; }

        [Display(Name = "Quick Pitch")]
        public bool QuickPitch { get; set; }
        #endregion

        public IList<SelectListItem> PullAreas { get; set; }

        public IList<SelectListItem> PitchAreas { get; set; }

        #region Only for display

        [DisplayFormat(NullDisplayText = "Undecided")]
        public string PullAreaShortName { get; set; }

        [DisplayFormat(NullDisplayText = "Undecided")]
        public string PitchAreaShortName { get; set; }

        public int PickslipCount { get; set; }

        #endregion

        [Display(Name = "Virtual Warehouse")]
        public string VwhId { get; set; }

        private IList<SelectListItem> _vwhList;
        public IList<SelectListItem> VwhList
        {
            get
            {
                return _vwhList ?? new SelectListItem[0];
            }
            set
            {
                _vwhList = value;
            }
        }

        public string CustomerId { get; set; }

        private IList<RowDimensionModel> _rows;
        public IList<RowDimensionModel> Rows
        {
            get
            {
                return _rows ?? new List<RowDimensionModel>();
            }
            set
            {
                _rows = value;
            }
        }

        /// <summary>
        /// Unique dimension values for the column
        /// </summary>
        public IList<string> ColDimensionValues { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int GrandTotalPickslips
        {
            get
            {
                return Rows.Select(p => p.PickslipCounts.Values.Sum()).Sum();
            }
        }

        #region Posted Values
        public PickslipDimension RowDimIndex { get; set; }

        public PickslipDimension ColDimIndex { get; set; }

        /// <summary>
        /// Value of the dimension in the selected column. This is posted.
        /// </summary>        
        public string ColDimVal { get; set; }

        public string RowDimVal { get; set; }

        #endregion

        public IList<SelectListItem> RowDimensionList { get; set; }

        public IList<SelectListItem> ColDimensionList { get; set; }

        public string RowDimDisplayName { get; set; }

        public string ColDimDisplayName { get; set; }

        public static string OrderSummaryReportUrl
        {
            get
            {
                return System.Configuration.ConfigurationManager.AppSettings["DcmsLiveBaseUrl"] + "Reports/Category_110/R110_08.aspx";
            }
        }

    }

}