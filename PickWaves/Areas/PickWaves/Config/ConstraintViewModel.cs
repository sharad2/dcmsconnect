using DcmsMobile.PickWaves.ViewModels;
using System.Collections.Generic;

namespace DcmsMobile.PickWaves.Areas.PickWaves.Config
{
    public class ConstraintViewModel : ViewModelBase
    {
        private readonly SortedList<CustomerModel, ConstraintModel> _customerOverrides;

        public ConstraintViewModel()
        {
            _customerOverrides = new SortedList<CustomerModel, ConstraintModel>();
        }
        /// <summary>
        /// List of SPLH settings group by customer id
        /// </summary>
        public SortedList<CustomerModel, ConstraintModel> CustomerGroupedList
        {
            get
            {
                return _customerOverrides;
            }
        }

        public ConstraintModel DefaultConstraints { get; set; }

        /// <summary>
        /// Active tab index
        /// </summary>
        public int? ActiveTab { get; set; }
    }
}