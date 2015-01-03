using DcmsMobile.PickWaves.ViewModels;
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

    }
}