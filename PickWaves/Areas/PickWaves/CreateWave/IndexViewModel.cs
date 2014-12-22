using DcmsMobile.PickWaves.ViewModels;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace DcmsMobile.PickWaves.Areas.PickWaves.CreateWave
{
    public class DimensionValueModel
    {
        internal DimensionValueModel(DimensionValue entity)
        {
            this.PickslipCount = entity.PickslipCount;
            this.OrderedPieces = entity.OrderedPieces;
        }

        public int PickslipCount { get; set; }

        public int OrderedPieces { get; set; }
    }

    /// <summary>
    /// The unbinder is capable of handling many properties.
    /// </summary>
    public class IndexViewModel : ViewModelBase
    {

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

        public SparseMatrix<string, string, DimensionValueModel> DimensionValues { get; set; }


        public PickslipDimension GroupDimIndex { get; set; }

        public PickslipDimension SubgroupDimIndex { get; set; }

        public IList<SelectListItem> GroupDimensionList { get; set; }

        public IList<SelectListItem> SubgroupDimensionList { get; set; }

        public string GroupDimDisplayName { get; set; }

        public string SubgroupDimDisplayName { get; set; }

        public static string OrderSummaryReportUrl
        {
            get
            {
                return System.Configuration.ConfigurationManager.AppSettings["DcmsLiveBaseUrl"] + "Reports/Category_110/R110_08.aspx";
            }
        }

    }

}