using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using DcmsMobile.PickWaves.Repository.CreateWave;
using EclipseLibrary.Mvc.Helpers;
using System;

namespace DcmsMobile.PickWaves.ViewModels.CreateWave
{
    /// <summary>
    /// View Model passed to _pickslipMatrixPartial
    /// </summary>
    public class PickslipMatrixPartialViewModel : ViewModelBase
    {
        public PickslipMatrixPartialViewModel()
        {
            // Factory defaults
            this.RowDimIndex = (int)PickslipDimension.CustomerDcCancelDate;

            this.ColDimIndex = (int)PickslipDimension.CustomerDc;
        }

        [Display(Name = "Virtual Warehouse")]
        public string VwhId { get; set; }

        private IEnumerable<SelectListItem> _vwhList;
        public IEnumerable<SelectListItem> VwhList
        {
            get
            {
                return _vwhList ?? Enumerable.Empty<SelectListItem>();
            }
            set
            {
                _vwhList = value;
            }
        }

        public string CustomerId { get; set; }

        private IList<RowDimensionModel> _rowDimensions;
        public IList<RowDimensionModel> RowDimensions
        {
            get
            {
                return _rowDimensions ?? new List<RowDimensionModel>();
            }
            set
            {
                _rowDimensions = value;
            }
        }

        private IDictionary<string, int> _colDimensionValues;

        /// <summary>
        /// Unique dimension values for the column
        /// </summary>
        public IList<string> ColDimensionValues
        {
            get
            {
                if (_colDimensionValues == null)
                {
                    _colDimensionValues = new Dictionary<string, int>();
                    foreach (var item in RowDimensions.SelectMany(p => p.PickslipCounts))
                    {
                        int count;
                        if (_colDimensionValues.TryGetValue(item.Key, out count))
                        {
                            // The data exists
                            _colDimensionValues[item.Key] = count + item.Value;      // Sum the pickslip count
                        }
                        else
                        {
                            _colDimensionValues.Add(item);
                        }
                    }
                }
                return _colDimensionValues.Keys.OrderBy(p => p).ToArray();
            }
        }

        public int GetPickslipCount(string distributionCenter)
        {
            return _colDimensionValues[distributionCenter];
        }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int GrandTotalPickslips
        {
            get
            {
                if (_colDimensionValues == null)
                {
                    // This happens because it seems that DisplayFor() pre-calculates metadata
                    return 0;
                }
                return _colDimensionValues.Values.Sum();
            }
        }

        #region Posted Values
        public int? RowDimIndex { get; set; }

        public int? ColDimIndex { get; set; }

        /// <summary>
        /// Value of the dimension in the selected column. This is posted.
        /// </summary>        
        public string ColDimVal { get; set; }

        public string RowDimVal { get; set; }

        #endregion

        public IEnumerable<SelectListItem> DimensionList { get; set; }        

        public string RowDimDisplayName { get; set; }

        public string ColDimDisplayName { get; set; }
    }

    internal abstract class PickslipMatrixPartialViewModelUnbinder : IModelUnbinder
    {
        public virtual void UnbindModel(RouteValueDictionary routeValueDictionary, string routeName, object value)
        {
            var model = value as PickslipMatrixPartialViewModel;
            if (!string.IsNullOrEmpty(model.CustomerId))
            {
                routeValueDictionary.Add(model.NameFor(m => m.CustomerId), model.CustomerId);
            }
            if (model.RowDimIndex.HasValue)
            {
                routeValueDictionary.Add(model.NameFor(m => m.RowDimIndex), model.RowDimIndex);
            }
            if (model.ColDimIndex.HasValue)
            {
                routeValueDictionary.Add(model.NameFor(m => m.ColDimIndex), model.ColDimIndex);
            }
            if (!string.IsNullOrEmpty(model.RowDimVal))
            {
                routeValueDictionary.Add(model.NameFor(m => m.RowDimVal), model.RowDimVal);
            }
            if (!string.IsNullOrEmpty(model.ColDimVal))
            {
                routeValueDictionary.Add(model.NameFor(m => m.ColDimVal), model.ColDimVal);
            }
            if (!string.IsNullOrEmpty(model.VwhId))
            {
                routeValueDictionary.Add(model.NameFor(m => m.VwhId), model.VwhId);
            }
        }
    }
}