using System.Collections.Generic;

namespace DcmsMobile.PickWaves.ViewModels.Config
{
    public class SkuCaseViewModel :ViewModelBase
    {
        private IList<CustomerSkuCaseModel> _customerSkuCaseList;

        /// <summary>
        /// This is the list of SKU cases that are recommended by Customers
        /// </summary>
        public IList<CustomerSkuCaseModel> CustomerSkuCaseList
        {
            get
            {
                return _customerSkuCaseList ?? new List<CustomerSkuCaseModel>(0);
            }
            set
            {
                _customerSkuCaseList = value;
            }
        }

        private IList<SkuCaseModel> _skuCaseList;

        /// <summary>
        /// This is list of SKU cases
        /// </summary>
        public IList<SkuCaseModel> SkuCaseList
        {
            get
            {
                return _skuCaseList ?? new List<SkuCaseModel>(0);
            }
            set
            {
                _skuCaseList = value;
            }
        }

        private IList<PackingRulesModel> _packingRuleList;

        /// <summary>
        /// This is the list of cases that are ignored for a style
        /// </summary>
        public IList<PackingRulesModel> PackingRuleList
        {
            get
            {
                return _packingRuleList ?? new List<PackingRulesModel>(0);
            }
            set
            {
                _packingRuleList = value;
            }
        }

                //this property added to get the value Case sleeted from dropdown list
        public string SelectedCase { get; set; }

        //this property added to get the customer Id sleeted from auto complete
        public string SelectedCustomerId { get; set; }

        /// <summary>
        /// This property is added to get detail of SKU case we need to add or update.
        /// </summary>
        public SkuCaseModel SkuCase { get; set; }

        /// <summary>
        /// this property is used to hold index of last selected tab or tab to make active
        /// </summary>
        public int? ActiveTab { get; set; }
    }
}