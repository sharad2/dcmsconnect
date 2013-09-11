using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using EclipseLibrary.Mvc.Helpers;

namespace DcmsMobile.Shipping.ViewModels
{
    public enum LayoutTabPage
    {
        Summary,
        Unrouted,
        Routing,
        Routed,
        Bol,
        Appointment,
        PoSearchResults,
        ScanToTruck
    }
    public abstract class LayoutTabsViewModel: IValidatableObject
    {
        private readonly LayoutTabPage _selectedIndex;
        internal LayoutTabsViewModel(LayoutTabPage selectedIndex)
        {
            _selectedIndex = selectedIndex;
            this.Summary = new CustomerSummaryModel();
        }
        public LayoutTabPage SelectedIndex
        {
            get
            {
                return _selectedIndex;
            }
        }

        /// <summary>
        /// This represents the value entered by the user
        /// </summary>
        [ReadOnly(false)]
        [DisplayName("Customer")]
        [Required(ErrorMessage="Please select a customer")]
        public virtual string PostedCustomerId { get; set; }

        /// <summary>
        /// The layout tabs customer search form posts to this URL
        /// </summary>
        public string CustomerFormUrl { get; set; }

        public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // No special validations needed
            return Enumerable.Empty<ValidationResult>();
        }

        /// <summary>
        /// Summary information about the current customer. As a convenience, this will never be null
        /// </summary>
        public CustomerSummaryModel Summary { get; set; }

    }

    internal class LayoutTabsViewModelUnbinder : IModelUnbinder
    {
        public void UnbindModel(RouteValueDictionary routeValueDictionary, string routeName, object model)
        {
            var ltv = model as LayoutTabsViewModel;
            if (ltv == null)
            {
                return;
            }
            DoUnbindModel(routeValueDictionary, ltv);
        }

        protected virtual void DoUnbindModel(RouteValueDictionary routeValueDictionary, LayoutTabsViewModel model)
        {
            if (!string.IsNullOrEmpty(model.PostedCustomerId))
            {
                routeValueDictionary.Add(model.NameFor(m => m.PostedCustomerId), model.PostedCustomerId);
            }
        }
    }
}