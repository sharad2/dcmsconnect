using DcmsMobile.PickWaves.ViewModels;
using System.Collections.Generic;

namespace DcmsMobile.PickWaves.Areas.PickWaves.Config
{
    public class CustSkuCaseViewModel: ViewModelBase
    {
        private IList<CustSkuCaseModel> _customerSkuCaseList;

        /// <summary>
        /// This is the list of SKU cases that are recommended by Customers
        /// </summary>
        public IList<CustSkuCaseModel> CustomerSkuCaseList
        {
            get
            {
                return _customerSkuCaseList ?? new List<CustSkuCaseModel>(0);
            }
            set
            {
                _customerSkuCaseList = value;
            }
        }
    }
}