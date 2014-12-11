using DcmsMobile.PickWaves.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

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

    public class StyleSkuCaseViewModel:ViewModelBase
    {
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
    }
}