using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Web.Mvc;
using System.Web.Routing;
using EclipseLibrary.Mvc.Helpers;

namespace DcmsMobile.Shipping.ViewModels
{
    /// <summary>
    /// IComparable used by SortedList. IEquatable used by GroupBy()
    /// </summary>
    public class RoutingPoGroup : IComparable<RoutingPoGroup>, IEquatable<RoutingPoGroup>
    {
        public RoutingPoGroup()
        {

        }
        public RoutingPoGroup(string buildingId, DateTime atsDate)
        {
            this.BuildingId = buildingId;
            this.AtsDate = atsDate;
        }
        [Key]
        public string BuildingId { get; set; }

        [Key]
        [DisplayFormat(DataFormatString = "{0:ddd d MMM}")]
        public DateTime? AtsDate { get; set; }

        /// <summary>
        /// The value of HtmlId can be posted and it will reconstruct this model
        /// </summary>
        public string HtmlId
        {
            get
            {
                return string.Format("{0}_{1:yyyyMMdd}", BuildingId, AtsDate);
            }
            set
            {
                var tokens = value.Split('_');
                this.BuildingId = tokens[0];
                this.AtsDate = DateTime.ParseExact(tokens[1], "yyyyMMdd", CultureInfo.InvariantCulture);
            }
        }

        public int CompareTo(RoutingPoGroup other)
        {
            if (other == null)
            {
                return -1;          //  nulls first
            }
            var ret = (this.BuildingId ?? string.Empty).CompareTo(other.BuildingId);
            if (ret != 0)
            {
                return ret;
            }

            ret = (this.AtsDate ?? DateTime.MinValue).CompareTo(other.AtsDate);
            return ret;
        }

        public override int GetHashCode()
        {
            return (AtsDate ?? DateTime.MinValue).GetHashCode();
        }

        public bool Equals(RoutingPoGroup other)
        {
            return this.CompareTo(other) == 0;
        }

        /// <summary>
        /// Number of POs in this group
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int PoCount { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal? TotalWeight { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? TotalPieces { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal? TotalVolume { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal? TotalCountBoxes { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal? TotalDollarsOrdered { get; set; }

        /// <summary>
        /// If any Po in group has no routing info on scheduled ATS date 
        /// </summary>
        public bool PoCancelToday { get; set; }

    }

    internal class RoutingPoGroupUnbinder : IModelUnbinder<RoutingPoGroup>
    {

        public void UnbindModel(RouteValueDictionary routeValueDictionary, string routeName, RoutingPoGroup model)
        {
            if (!string.IsNullOrWhiteSpace(model.HtmlId))
            {
                routeValueDictionary.Add(routeName + "." + model.NameFor(m => m.HtmlId), model.HtmlId);
            }
        }
    }

    public class RoutingViewModel : LayoutTabsViewModel
    {
        private readonly SortedList<RoutingPoGroup, IList<RoutablePoModel>> _groupedPoList;
        private readonly SortedList<string, IList<RoutingPoGroup>> _atsDatesByBuilding;
        public RoutingViewModel()
            : base(LayoutTabPage.Routing)
        {
            _groupedPoList = new SortedList<RoutingPoGroup, IList<RoutablePoModel>>();
            _atsDatesByBuilding = new SortedList<string, IList<RoutingPoGroup>>();
        }
        /// <summary>
        /// This constructor use to apply filter on building.
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="buildingId"></param>
        public RoutingViewModel(string customerId, string buildingId,bool showRoutedOrders=false)
            : base(LayoutTabPage.Routing)
        {
            this.PostedCustomerId = customerId;
            this.BuildingId = buildingId;
            this.ShowRoutedOrders = showRoutedOrders;
            _groupedPoList = new SortedList<RoutingPoGroup, IList<RoutablePoModel>>();
            _atsDatesByBuilding = new SortedList<string, IList<RoutingPoGroup>>();
        }
        public RoutingViewModel(string customerId, bool showRoutedOrders = false, RoutingPoGroup initialGroup = null)
            : base(LayoutTabPage.Routing)
        {
            this.PostedCustomerId = customerId;
            this.ShowRoutedOrders = showRoutedOrders;
            _groupedPoList = new SortedList<RoutingPoGroup, IList<RoutablePoModel>>();
            _atsDatesByBuilding = new SortedList<string, IList<RoutingPoGroup>>();
            this.InitialGroup = initialGroup;
        }
      
        /// <summary>
        /// Initial group which should be focused on the screen
        /// </summary>
        [ReadOnly(false)]
        public RoutingPoGroup InitialGroup { get; set; }
    

        /// <summary>
        /// List of POs grouped by Building, ATS date
        /// </summary>
        public SortedList<RoutingPoGroup, IList<RoutablePoModel>> GroupedPoList
        {
            get
            {
                return _groupedPoList;
            }
        }

        /// <summary>
        /// ATS dates grouped by building
        /// </summary>
        public SortedList<string, IList<RoutingPoGroup>> AtsDatesByBuilding
        {
            get
            {
                return _atsDatesByBuilding;
            }
        }

        /// <summary>
        /// Selected keys comprises of Customer,PO,DC,Iteration
        /// </summary>
        public ICollection<string> SelectedKeys { get; set; }

      
        /// <summary>
        /// Property used while creating load
        /// </summary>
        public RoutablePoModel RoutingInfo { get; set; }
        public bool UpdateCarrier { get; set; }
        public bool UpdateDc { get; set; }
        public bool UpdateLoad { get; set; }
        public bool UpdatePickUpDate { get; set; }
      
        /// <summary>
        /// Url of Report 110.16: Summary of the POs according to passed Customer_Id.
        /// </summary>       
        public string POslDetailUrl
        {
            get
            {
                return System.Configuration.ConfigurationManager.AppSettings["DcmsLiveBaseUrl"] + "Reports/Category_110/R110_16.aspx";
            }
        }

        /// <summary>
        /// Whether routed orders should be displayed
        /// </summary>
        public bool ShowRoutedOrders { get; set; }

        //Used for building filter.
        [DisplayName("Building")]
        public string BuildingId { get; set; }
        /// <summary>
        /// Properties used to filter orders
        /// </summary>
        [DisplayName("Start Date")]
        [DisplayFormat(DataFormatString = "{0:d}")]
        public DateTime? StartDate { get; set; }

        [DisplayName("DC Cancel Date")]
        [DisplayFormat(DataFormatString = "{0:d}")]
        public DateTime? DcCancelDate { get; set; }

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            foreach (var result in base.Validate(validationContext))
            {
                yield return result;
            }
            if (this.UpdateCarrier || this.UpdateLoad || this.UpdateDc || this.UpdatePickUpDate )
            {
                if (this.SelectedKeys == null || this.SelectedKeys.Count == 0)
                {
                    yield return new ValidationResult("Please select at least one PO.");
                }
            }
        }
    }

    internal class RoutingViewModelUnbinder : LayoutTabsViewModelUnbinder
    {
        protected override void DoUnbindModel(RouteValueDictionary routeValueDictionary, LayoutTabsViewModel model)
        {
            var rvm = (RoutingViewModel)model;
            if (rvm.ShowRoutedOrders)
            {
                routeValueDictionary.Add(rvm.NameFor(m => m.ShowRoutedOrders), rvm.ShowRoutedOrders);
            }
            if (!string.IsNullOrEmpty(rvm.BuildingId))
            {
                routeValueDictionary.Add(rvm.NameFor(m => m.BuildingId), rvm.BuildingId);
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