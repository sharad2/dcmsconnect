using DcmsMobile.PickWaves.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DcmsMobile.PickWaves.Areas.PickWaves.Config
{
    public class CustSkuCaseViewModel: ViewModelBase
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
    }
}