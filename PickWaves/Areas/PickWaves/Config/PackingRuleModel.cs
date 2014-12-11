using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

namespace DcmsMobile.PickWaves.Areas.PickWaves.Config
{
    public class PackingRuleModel
    {
        /// <summary>
        /// This is style for which SKU cases are ignored.
        /// </summary>
        [Required(ErrorMessage = "Specify the Style to which this packing rule applies")]
        public string Style { get; set; }

        private IEnumerable<SelectListItem> _skuCases;

        public IEnumerable<SelectListItem> SkuCaseList
        {
            get
            {
                return _skuCases ?? Enumerable.Empty<SelectListItem>();
            }
            set
            {
                _skuCases = value;
            }
        }

        [Required(ErrorMessage = "Specify the SKU case which must be used for the style")]
        public string CaseId { get; set; }

        /// <summary>
        /// This is flag value to tell that the case is ignore or not.
        /// </summary>
        public bool IgnoreFlag { get; set; }

    }
}