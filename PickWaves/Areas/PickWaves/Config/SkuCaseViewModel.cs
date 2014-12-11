using DcmsMobile.PickWaves.ViewModels;
using System.Collections.Generic;

namespace DcmsMobile.PickWaves.Areas.PickWaves.Config
{
    public class StyleSkuCaseModel
    {
        /// <summary>
        /// This is style for which SKU cases are ignored.
        /// </summary>
        public string Style { get; set; }


        public string CaseId { get; set; }

        /// <summary>
        /// This is flag value to tell that the case is ignore or not.
        /// </summary>
        public bool IgnoreFlag { get; set; }
    }

    public class SkuCaseViewModel :ViewModelBase
    {
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

        private IList<StyleSkuCaseModel> _packingRuleList;

        /// <summary>
        /// This is the list of cases that are ignored for a style
        /// </summary>
        public IList<StyleSkuCaseModel> PackingRuleList
        {
            get
            {
                return _packingRuleList ?? new List<StyleSkuCaseModel>(0);
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