using System.Collections.Generic;
using System.Linq;
using System.Web.Routing;
using EclipseLibrary.Mvc.Helpers;

namespace DcmsMobile.Shipping.ViewModels
{
    public enum RoutingSummaryFilter
    {
        NotSet,
        UnroutedOnly,
        RoutingOnly,
        RoutedOnly,
        BolOnly
    }
    public class RoutingSummaryViewModel : LayoutTabsViewModel
    {
        public RoutingSummaryViewModel()
            : base(LayoutTabPage.Summary)
        {

        }


        public RoutingSummaryViewModel(string customerId, RoutingSummaryFilter routingFilter = RoutingSummaryFilter.NotSet)
            : base(LayoutTabPage.Summary)
        {
            this.PostedCustomerId = customerId;
            this.RoutingFilter = routingFilter;
        }

        private IEnumerable<CustomerSummaryModel> _customerRoutingDetails;
        public IEnumerable<CustomerSummaryModel> CustomerRoutingDetails
        {
            get
            {
                return _customerRoutingDetails ?? Enumerable.Empty<CustomerSummaryModel>();
            }
            set
            {
                _customerRoutingDetails = value;
            }
        }

        /// <summary>
        /// This is posted. Filters the customer list to ensure that the customer has at least one of of this routing status
        /// </summary>
        public RoutingSummaryFilter RoutingFilter { get; set; }
        

        #region
        /// <summary>
        /// Display name for Routing Summary Filters.
        /// </summary>
        public string DisplayName
        {
            get
            {
                switch (RoutingFilter)
                {
                    case RoutingSummaryFilter.UnroutedOnly:
                        return "Unrouted";

                    case RoutingSummaryFilter.RoutingOnly:
                        return "Routing";

                    case RoutingSummaryFilter.RoutedOnly:
                        return "Routed";

                    case RoutingSummaryFilter.BolOnly:
                        return "Bol";

                    default:
                        // Unexpected
                        return null;
                }
            }
        }
        #endregion
    }

    internal class RoutingSummaryViewModelUnbinder : LayoutTabsViewModelUnbinder
    {
        protected override void DoUnbindModel(RouteValueDictionary routeValueDictionary, LayoutTabsViewModel model)
        {
            var rvm = (RoutingSummaryViewModel)model;
            if (rvm.RoutingFilter != RoutingSummaryFilter.NotSet)
            {
                routeValueDictionary.Add(rvm.NameFor(m => m.RoutingFilter), rvm.RoutingFilter.ToString());
            }
            base.DoUnbindModel(routeValueDictionary, model);
        }
    }

}