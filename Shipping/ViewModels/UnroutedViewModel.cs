using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Script.Serialization;
using EclipseLibrary.Mvc.Helpers;

namespace DcmsMobile.Shipping.ViewModels
{
    public class UnroutedPoGroup : IComparable<UnroutedPoGroup>, IEquatable<UnroutedPoGroup>
    {
        private  string _buildingId;
        private  DateTime? _dcCancelDate;
        private string _startDateDisplay;
        public UnroutedPoGroup()
        {

        }
        public UnroutedPoGroup(string buildingId, DateTime? dcCancelDate)
        {
            _buildingId = buildingId;
            _dcCancelDate = dcCancelDate == null ? (DateTime?)null : dcCancelDate.Value.Date;
        }

        public void UpdateStats(IList<UnroutedPoModel> list)
        {
            var minstart = list.Min(p => p.StartDate);
            var maxstart = list.Max(p => p.StartDate);
            if (minstart == maxstart)
            {
                _startDateDisplay = string.Format("{0:ddd d MMM}", minstart);
            }
            else
            {
                _startDateDisplay = string.Format("Ranges from: {0:ddd d MMM} to: {0:ddd d MMM}", minstart, maxstart);
            }
        }

        public string StartDateDisplay
        {
            get
            {
                return _startDateDisplay;
            }
        }

        public string BuildingId
        {
            get
            {
                return _buildingId;
            }
        }

        [DisplayFormat(DataFormatString = "{0:ddd d MMM}", NullDisplayText="(Not Specified)")]
        public DateTime? DcCancelDate
        {
            get
            {
                return _dcCancelDate;
            }
        }

        [ReadOnly(false)]
        public string HtmlId
        {
            get
            {
                return string.Format("{0}_{1:yyyyMMdd}", _buildingId, _dcCancelDate);
            }
            set
            {
                var tokens = value.Split('_');
                _buildingId = tokens[0];
                if (!string.IsNullOrEmpty(tokens[1]))
                {
                    _dcCancelDate = DateTime.ParseExact(tokens[1], "yyyyMMdd", CultureInfo.InvariantCulture);
                }
            }
        }

        public int CompareTo(UnroutedPoGroup other)
        {
            var ret = (_buildingId ?? string.Empty).CompareTo(other._buildingId);
            if (ret == 0)
            {

                ret = (_dcCancelDate ?? DateTime.MinValue).CompareTo(other._dcCancelDate ?? DateTime.MinValue);
            }
            return ret;
        }

        public override int GetHashCode()
        {
            return (_buildingId ?? string.Empty).GetHashCode();
        }

        public string Cancelling
        {
            get
            {
                if (_dcCancelDate == null)
                {
                    return null;
                }
                var days = (int)(_dcCancelDate.Value - DateTime.Today).TotalDays;

                if (days < 0)
                {
                    return string.Format("{0:ddd d MMM}", this._dcCancelDate);
                }
                return string.Format("in {0} days", days);
            }
        }

        public bool Equals(UnroutedPoGroup other)
        {
            return this.CompareTo(other) == 0;
        }
    }

    internal class UnroutedPoGroupUnbinder : IModelUnbinder<UnroutedPoGroup>
    {
        public void UnbindModel(RouteValueDictionary routeValueDictionary, string routeName, UnroutedPoGroup model)
        {
            if (!string.IsNullOrWhiteSpace(model.HtmlId))
            {
                routeValueDictionary.Add(routeName + "." + model.NameFor(m => m.HtmlId), model.HtmlId);
            }
        }
    }
    /// <summary>
    /// Displays a list of unrouted orders of a particular customer. The passed or posted customer ID dictates which POs are displayed.
    /// </summary>
    
    public class UnroutedViewModel : LayoutTabsViewModel
    {
        public UnroutedViewModel()
            : base(LayoutTabPage.Unrouted)
        {
            _groupedPoList = new SortedList<UnroutedPoGroup, IList<UnroutedPoModel>>();
            _dcCancelDatesByBuilding = new SortedList<string, IList<UnroutedPoGroup>>();
        }

        public UnroutedViewModel(string customerId)
            : base(LayoutTabPage.Unrouted)
        {
            this.PostedCustomerId = customerId;
            _groupedPoList = new SortedList<UnroutedPoGroup, IList<UnroutedPoModel>>();
            _dcCancelDatesByBuilding = new SortedList<string, IList<UnroutedPoGroup>>();
        }

        public UnroutedViewModel(string customerId, string buildingId,bool showUnavailableBucket)
            : base(LayoutTabPage.Unrouted)
        {
            this.BuildingId = buildingId;
            this.PostedCustomerId = customerId;
            this.ShowUnavailableBucket = showUnavailableBucket;
            _groupedPoList = new SortedList<UnroutedPoGroup, IList<UnroutedPoModel>>();
            _dcCancelDatesByBuilding = new SortedList<string, IList<UnroutedPoGroup>>();
        }

        public UnroutedViewModel(string customerId, DateTime? dcCancelDate)
            : base(LayoutTabPage.Unrouted)
        {
            this.PostedCustomerId = customerId;
            this.DcCancelDate = dcCancelDate;
            _groupedPoList = new SortedList<UnroutedPoGroup, IList<UnroutedPoModel>>();
            _dcCancelDatesByBuilding = new SortedList<string, IList<UnroutedPoGroup>>();
        }

        public UnroutedViewModel(string customerId, UnroutedPoGroup initialGroup=null)
            : base(LayoutTabPage.Unrouted)
        {
            this.PostedCustomerId = customerId;
            _groupedPoList = new SortedList<UnroutedPoGroup, IList<UnroutedPoModel>>();
            _dcCancelDatesByBuilding = new SortedList<string, IList<UnroutedPoGroup>>();
            this.InitialGroup = initialGroup;
        }


        private readonly SortedList<UnroutedPoGroup, IList<UnroutedPoModel>> _groupedPoList;
        private readonly SortedList<string, IList<UnroutedPoGroup>> _dcCancelDatesByBuilding;
        public SortedList<UnroutedPoGroup, IList<UnroutedPoModel>> GroupedPoList
        {
            get
            {
                return _groupedPoList;
            }
        }

        /// <summary>
        /// Show/Hide unavailable bucket orders
        /// </summary>
        public bool ShowUnavailableBucket { get; set; }

        /// <summary>
        /// Count of POs in bucket.
        /// </summary>
        public int TotalPosInBucket
        {
            get {
                return this.GroupedPoList.SelectMany(p=>p.Value).Where(q=>q.BucketId !=null).Count();
            }
        }

        /// <summary>
        /// Count of POs not in bucket.
        /// </summary>
        public int TotalPosNotInBucket
        {
            get
            {
                return this.GroupedPoList.SelectMany(p=>p.Value).Where(q=>q.BucketId == null).Count();
            }
        }
        /// <summary>
        /// Dc Cancel dates grouped by building
        /// </summary>
        public SortedList<string, IList<UnroutedPoGroup>> DcCancelDatesByBuilding
        {
            get
            {
                return _dcCancelDatesByBuilding;
            }
        }

        private IEnumerable<Tuple<DateTime, int?, int>> _atsDateList;

        /// <summary>
        /// List of available ATS dates
        /// </summary>
        public IEnumerable<Tuple<DateTime, int?, int>> AtsDateList
        {
            get
            {
                return _atsDateList ?? Enumerable.Empty<Tuple<DateTime, int?, int>>();
            }
            set
            {
                _atsDateList = value;
            }
        }

        public string JsonDateList
        {
            get
            {
                var baseDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                var ser = new JavaScriptSerializer();
                var str = ser.Serialize(this.AtsDateList.Select(p => new
                {
                    date = p.Item1.Date.Year * 10000 + p.Item1.Date.Month * 100 + p.Item1.Date.Day,
                    count = p.Item3
                }));
                return str;
            }
        }

        /// <summary>
        /// List of POIds which are posted
        /// </summary>
        [ReadOnly(false)]
        public IEnumerable<string> SelectedKeys { get; set; }

        /// <summary>
        /// This is when the order is available to ship.
        /// </summary>
        [ReadOnly(false)]
        public DateTime? AtsDate { get; set; }


        /// <summary>
        /// Following properties are  for filtering orders
        /// </summary>        

        [DisplayName("DCCancel Date")]
        [DisplayFormat(DataFormatString = "{0:ddd d MMM}")]
        public DateTime? DcCancelDate { get; set; }

        [DisplayName("Start Date")]
        public DateTime? StartDate { get; set; }

        [DisplayName("Building")]
        public string BuildingId { get; set; }

        /// <summary>
        /// Number of Pos which were just assigned an ATS Date
        /// </summary>
        public int? RecentlyAssignedPoCount { get; set; }

        /// <summary>
        /// When we assign POs, this group can be used to navigate to the Routing page
        /// </summary>
        public RoutingPoGroup RecentlyAssignedGroup { get; set; }

        /// <summary>
        /// Initial group which should be focused on the screen
        /// </summary>
        [ReadOnly(false)]
        public UnroutedPoGroup InitialGroup { get; set; }


        /// <summary>
        /// Display info regarding availability of buckets for orders
        /// </summary>
        public IHtmlString BucketAvailabilityInfo 
        {
            get
            {
                if (this.TotalPosNotInBucket > 0)
                {
                    return MvcHtmlString.Create(string.Format("Waves created for <strong class='ui-state-highlight'>{0}</strong> orders and <strong class='ui-state-highlight'>{1}</strong> orders not in wave.", this.TotalPosInBucket, this.TotalPosNotInBucket));
                }
                if (this.TotalPosNotInBucket == 0 && this.ShowUnavailableBucket == true)
                {
                    return MvcHtmlString.Create("<span class='ui-state-highlight'>Waves created for all orders.</span>");
                }
                return MvcHtmlString.Empty;
            }
        
        }

        /// <summary>
        /// True if EDI is sent electronically
        /// </summary>
        public bool IsAutomaticEdi { get; set; }
    }

    internal class UnroutedViewModelUnbinder : LayoutTabsViewModelUnbinder
    {
        protected override void DoUnbindModel(RouteValueDictionary routeValueDictionary, LayoutTabsViewModel model)
        {
            var uvm = (UnroutedViewModel)model;
            if (uvm.DcCancelDate.HasValue)
            {
                routeValueDictionary.Add(uvm.NameFor(m => m.DcCancelDate), string.Format(CultureInfo.InvariantCulture, "{0:d}", uvm.DcCancelDate));
            }
            if (!string.IsNullOrEmpty(uvm.BuildingId))
            {
                routeValueDictionary.Add(uvm.NameFor(m => m.BuildingId), uvm.BuildingId);
            }
           
            if (uvm.RecentlyAssignedPoCount.HasValue)
            {
                routeValueDictionary.Add(uvm.NameFor(m => m.RecentlyAssignedPoCount), uvm.RecentlyAssignedPoCount);
            }
            // We  need to unbind the RecentlyAssignedGroup for redirecting to Routing UI.
            if (uvm.RecentlyAssignedGroup != null)
            {
                ModelUnbinderHelpers.ModelUnbinders.FindUnbinderFor(uvm.RecentlyAssignedGroup.GetType())
                    .UnbindModel(routeValueDictionary, uvm.NameFor(m => m.RecentlyAssignedGroup), uvm.RecentlyAssignedGroup);
            }
            if (uvm.InitialGroup != null)
            {
                ModelUnbinderHelpers.ModelUnbinders.FindUnbinderFor(uvm.InitialGroup.GetType())
                    .UnbindModel(routeValueDictionary, uvm.NameFor(m => m.InitialGroup), uvm.InitialGroup);
            }
            base.DoUnbindModel(routeValueDictionary, model);
        }
    }


}