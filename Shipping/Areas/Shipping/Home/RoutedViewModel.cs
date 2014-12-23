using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;
using System.Web.Routing;
using EclipseLibrary.Mvc.Helpers;

namespace DcmsMobile.Shipping.ViewModels
{
    /// <summary>
    /// IComparable used by SortedList. IEquatable used by GroupBy(). Key is Building, ATS Date, EDI Id
    /// </summary>
    public class RoutedPoGroup : IComparable<RoutedPoGroup>, IEquatable<RoutedPoGroup>
    {
        public RoutedPoGroup()
        {

        }
        public RoutedPoGroup(DateTime atsDate)
        {
            this.AtsDate = atsDate;
        }
        [Key]
        public string BuildingId { get; set; }     

        public string EdiIdList { get; set; }

        [Key]
        [DisplayFormat(DataFormatString = "{0:ddd d MMM}")]
        public DateTime? AtsDate { get; set; }

        public int CompareTo(RoutedPoGroup other)
        {
            var ret = (this.AtsDate ?? DateTime.MinValue).CompareTo(other.AtsDate);
            if (ret != 0)
            {
                return ret;
            }
            //ret = (this.EdiId).CompareTo(other.EdiId);
            return ret;
        }


        public string HtmlId
        {
            get
            {
                return string.Format("{0:yyyyMMdd}", AtsDate);
            }
            set
            {
                this.AtsDate = DateTime.ParseExact(value, "yyyyMMdd", CultureInfo.InvariantCulture);
            }
        }
        public override int GetHashCode()
        {
            return (AtsDate ?? DateTime.MinValue).GetHashCode();
        }

        public bool Equals(RoutedPoGroup other)
        {
            return this.CompareTo(other) == 0;
        }

        /// <summary>
        /// Number of POs in this group
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int PoCount { get; set; }
    }

    internal class RoutedgPoGroupUnbinder : IModelUnbinder<RoutedPoGroup>
    {

        public void UnbindModel(RouteValueDictionary routeValueDictionary, string routeName, RoutedPoGroup model)
        {
            if (!string.IsNullOrWhiteSpace(model.HtmlId))
            {
                routeValueDictionary.Add(routeName + "." + model.NameFor(m => m.HtmlId), model.HtmlId);
            }
        }
    }



    public class RoutedViewModel : LayoutTabsViewModel
    {
        private readonly SortedList<RoutedPoGroup, IList<RoutablePoModel>> _groupedPoList;
        public RoutedViewModel()
            : base(LayoutTabPage.Routed)
        {
            _groupedPoList = new SortedList<RoutedPoGroup, IList<RoutablePoModel>>();
        }

        public RoutedViewModel(string customerId)
            : base(LayoutTabPage.Routed)
        {
            this.PostedCustomerId = customerId;
            _groupedPoList = new SortedList<RoutedPoGroup, IList<RoutablePoModel>>();
        }

        public RoutedViewModel(string customerId, RoutedPoGroup initialGroup)
            : base(LayoutTabPage.Routed)
        {
            this.PostedCustomerId = customerId;
            _groupedPoList = new SortedList<RoutedPoGroup, IList<RoutablePoModel>>();
            this.InitialGroup = initialGroup;
        }     

        /// <summary>
        /// Comma sepated list of EDIs posted for BOL creation
        /// </summary>
        public string EdiId { get; set; }

        /// <summary>
        /// Number of BOLS created
        /// </summary>
        public int? CreatedBolCount { get; set; }

        /// <summary>
        /// Ats date for which BOLs created
        /// </summary>
        public DateTime? BolAtsdate { get; set; }

        public SortedList<RoutedPoGroup, IList<RoutablePoModel>> GroupedPoList
        {
            get
            {
                return _groupedPoList;
            }
        }

        /// <summary>
        /// Returns 0 if spanning is not necessary. > 0 If the value should be spanned. -1 if the value has already been spanned and should not be rendered
        /// </summary>
        /// <param name="expr">Property to evaluate</param>
        /// <returns></returns>
        public int SpanFor<TValue>(Expression<Func<RoutablePoModel, TValue>> expr, int i, int j)
        {
            var polist = GroupedPoList.Values[i];
            var poModel = polist[j];
            var func = expr.Compile();

            var curCellValue = func(poModel);
            var curBolRowNumber = poModel.BolRowNumber;
            if (j > 0)
            {
                // If this is not the first row in the table, see whether the cell needs to be rendered
                var poPreviousModel = polist[j - 1];
                var prevCellValue = func(poPreviousModel);
                if (curCellValue.Equals(prevCellValue) && poPreviousModel.BolRowNumber == curBolRowNumber)
                {
                    // This value is same as previous. Do not render any td for this.
                    return -1;
                }
            }

            // Number of rows where current cell val = previous cell val AND current BOL# = previous BOL#

            return polist.Skip(j)
                .Where(p => p.BolRowNumber == curBolRowNumber)
                .TakeWhile(p => func.Invoke(p).Equals(func.Invoke(poModel))).Count();
        }

        public RoutedPoGroup InitialGroup { get; set; }
        
    }

    internal class RoutedViewModelUnbinder : LayoutTabsViewModelUnbinder
    {
        protected override void DoUnbindModel(RouteValueDictionary routeValueDictionary, LayoutTabsViewModel model)
        {
            var rvm = (RoutedViewModel)model;
            if (rvm.CreatedBolCount.HasValue)
            {
                routeValueDictionary.Add(rvm.NameFor(m => m.CreatedBolCount), rvm.CreatedBolCount);
            }
            if (rvm.BolAtsdate.HasValue)
            {
                routeValueDictionary.Add(rvm.NameFor(m => m.BolAtsdate), rvm.BolAtsdate);
            }
            if (rvm.InitialGroup != null)
            {
                ModelUnbinderHelpers.ModelUnbinders.FindUnbinderFor(rvm.InitialGroup.GetType())
                    .UnbindModel(routeValueDictionary, rvm.NameFor(m => m.InitialGroup), rvm.InitialGroup);
            }
            base.DoUnbindModel(routeValueDictionary, model);
        }
    }

}