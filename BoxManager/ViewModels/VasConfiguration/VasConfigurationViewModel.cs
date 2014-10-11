using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System.Web.Routing;
using DcmsMobile.BoxManager.Helpers;
using EclipseLibrary.Mvc.Helpers;

namespace DcmsMobile.BoxManager.ViewModels.VasConfiguration
{
    public class VasConfigurationViewModel
    {
        private readonly SortedList<string, List<CustomerVasSettingModel>> _customerGroupedList;
        
        public VasConfigurationViewModel()
        {
            _customerGroupedList = new SortedList<string, List<CustomerVasSettingModel>>();

        }

        public VasConfigurationViewModel(string customerId, string vasId)
        {
            _customerGroupedList = new SortedList<string, List<CustomerVasSettingModel>>();
            this.CustomerId = customerId;
            this.VasId = vasId;
        }

        [Required(ErrorMessage = "Customer is required")]
        [Display(Name = "Customer")]
        public string CustomerId { get; set; }

        public string CustomerName { get; set; }

        [Display(Name = "For Labels")]
        public string Labels { get; set; }

        public VasPoPatternType PatternType { get; set; }

        public string PatternAlphabet { get; set; }

        /// <summary>
        /// List of VAS settings group by customer id
        /// </summary>
        public SortedList<string, List<CustomerVasSettingModel>> CustomerGroupedList
        {
            get
            {
                return _customerGroupedList;
            }
        }

        public IEnumerable<CustomerVasSettingModel> VasSettingList { get; set; }

        public IEnumerable<SelectListItem> VasCodeList { get; set; }

        /// <summary>
        /// VAS ID for which customer created a pattern
        /// </summary>
        [Display(Name="VAS Type")]
        public string VasId { get; set; }

        /// <summary>
        /// Discriptive text of VAS ID
        /// </summary>
        public string VasDescription { get; set; }

        /// <summary>
        /// User remarks entered by customer for a specific VAS pattern
        /// </summary>
        [Display(Name = "Remarks")]
        public string UserRemarks { get; set; }

        /// <summary>
        /// Description of VAS pattern which is already applied for customer (System generated)
        /// </summary>
        public string VasPatternDescription { get; set; }

        public bool IsEditable { get; set; }

        public string EditableRoleName { get; set; }
    }

    internal class VasConfigurationViewModelUnbinder: IModelUnbinder<VasConfigurationViewModel>
    {
        public void UnbindModel(RouteValueDictionary routeValueDictionary, string routeName, VasConfigurationViewModel model)
        {
            if (!string.IsNullOrWhiteSpace(model.CustomerId))
            {
                routeValueDictionary.Add(model.NameFor(m => m.CustomerId), model.CustomerId);
            }
            if (!string.IsNullOrWhiteSpace(model.VasId))
            {
                routeValueDictionary.Add(model.NameFor(m => m.VasId), model.VasId);
            }
        }
    }
}