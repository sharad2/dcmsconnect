using DcmsMobile.PickWaves.ViewModels;
using System;
using System.Collections.Generic;

namespace DcmsMobile.PickWaves.Areas.PickWaves.Config
{


    public class SkuCaseViewModel : ViewModelBase
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



        ////this property added to get the value Case sleeted from dropdown list
        //[Obsolete]
        //public string SelectedCase { get; set; }

        ////this property added to get the customer Id sleeted from auto complete
        //[Obsolete]
        //public string SelectedCustomerId { get; set; }

        ///// <summary>
        ///// This property is added to get detail of SKU case we need to add or update.
        ///// </summary>
        //[Obsolete]
        //public SkuCaseModel SkuCase { get; set; }

    }
}