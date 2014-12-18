
using DcmsMobile.PickWaves.ViewModels;
using EclipseLibrary.Mvc.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;

namespace DcmsMobile.PickWaves.Areas.PickWaves.CreateWave
{
    /// <summary>
    /// View Model passed to _pickslipMatrixPartial
    /// </summary>
    public class PickslipMatrixPartialViewModel : ViewModelBase
    {
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
        public int? RowDimIndex { get; set; }

        public int? ColDimIndex { get; set; }

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
    }

    [Obsolete]
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