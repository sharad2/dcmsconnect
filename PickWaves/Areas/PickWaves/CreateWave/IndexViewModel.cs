using DcmsMobile.PickWaves.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Web.Mvc;

namespace DcmsMobile.PickWaves.Areas.PickWaves.CreateWave
{
    public class DimensionValueModel
    {
        internal DimensionValueModel(MatrixCellValue entity)
        {
            this.PickslipCount = entity.PickslipCount;
            this.OrderedPieces = entity.OrderedPieces;
        }

        public int PickslipCount { get; set; }

        public int OrderedPieces { get; set; }
    }

    /// <summary>
    /// The formatting is applied based on type of the value. Equality is is also based on underlying value. This makes it possible to used Distinct()
    /// </summary>
    public struct DimensionValue: IEquatable<DimensionValue>
    {
        private readonly object _rawValue;
        public DimensionValue(object value)
        {
            _rawValue = value;
        }

        /// <summary>
        /// Post value shows date as YYYY-MM-DD
        /// </summary>
        public string PostValue
        {
            get
            {
                if (_rawValue == null)
                {
                    return string.Empty;
                }
                if (_rawValue is DateTime)
                {
                    return string.Format("{0:yyyy-MM-dd}", _rawValue, CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern);
                }
                return _rawValue.ToString();
            }
        }

        /// <summary>
        /// Display value shows date only
        /// </summary>
        public string DisplayValue
        {
            get
            {
                if (_rawValue == null)
                {
                    return string.Empty;
                }
                if (_rawValue is DateTime)
                {
                    return string.Format("{0:d}", _rawValue);
                }
                return _rawValue.ToString();
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is DimensionValue)
            {
                return this.Equals((DimensionValue)obj);
            }
            return false;
        }

        public override int GetHashCode()
        {
            if (_rawValue == null)
            {
                return 0;
            }
            return _rawValue.GetHashCode();
        }


        public bool Equals(DimensionValue other)
        {
            if (_rawValue == null)
            {
                return other._rawValue == null;
            }
            return _rawValue.Equals(other._rawValue);
        }
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

        public SparseMatrix<DimensionValue, DimensionValue, DimensionValueModel> DimensionMatrix { get; set; }


        public PickslipDimension GroupDimIndex { get; set; }

        public PickslipDimension SubgroupDimIndex { get; set; }

        /// <summary>
        /// If this is set, the the view attempts to make the tab of this group active. It is set after pickslips are added
        /// </summary>
        public DimensionValue? GroupDimVal { get; set; }

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