using System.Collections.Generic;

namespace DcmsMobile.BoxManager.ViewModels.VasConfiguration
{
    public class CustomerGroupedListModel
    {

        private IDictionary<string, List<CustomerVasSettingModel>> _customerGroupedList;
        
        /// <summary>
        /// List of VAS settings group by customer id
        /// </summary>
        public IDictionary<string, List<CustomerVasSettingModel>> CustomerGroupedList
        {
            get
            {
                if(_customerGroupedList == null)
                {
                    _customerGroupedList = new Dictionary<string, List<CustomerVasSettingModel>>();
                }
                return _customerGroupedList;
            }
            set { _customerGroupedList = value; }
        }
        
        public string CustomerId { get; set; }

        public string VasId { get; set; }
    }
}