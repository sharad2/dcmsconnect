using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

namespace DcmsMobile.PickWaves.ViewModels.Config
{
    public class PackingRulesModel
    {
        /// <summary>
        /// This is style for which SKU cases are ignored.
        /// </summary>
        [Required(ErrorMessage = "Style cannot be null")]
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

        [Required(ErrorMessage = "Case cannot be null")]
        public string CaseId { get; set; }

        /// <summary>
        /// This is flag value to tell that the case is ignore or not.
        /// </summary>
        public bool IgnoreFlag { get; set; }

    }
}