using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace DcmsMobile.BoxManager.ViewModels.VasConfiguration
{
    public class IndexViewModel : VasConfigurationViewModelBase
    {
        public IndexViewModel()
        {
            _vasGroupedList = new SortedList<string, List<CustomerVasSettingModel>>();
        }

        private readonly SortedList<string, List<CustomerVasSettingModel>> _vasGroupedList;

        /// <summary>
        /// List of VAS settings group by VAS Type
        /// </summary>
        public SortedList<string, List<CustomerVasSettingModel>> VasGroupedList
        {
            get
            {
                return _vasGroupedList;
            }
        }

        public IList<CustomerVasSettingModel> VasSettingList { get; set; }

        [Required(ErrorMessage = "Customer is required")]
        [Display(Name = "Customer")]
        public string CustomerId { get; set; }

        /// <summary>
        /// VAS ID for which customer created a pattern
        /// </summary>
        [Display(Name = "VAS Type")]
        public string VasId { get; set; }

        public IEnumerable<SelectListItem> VasCodeList { get; set; }
    }
}