using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

namespace DcmsMobile.PickWaves.ViewModels.Config
{
    public class CustomerSkuCaseModel
    {
        /// <summary>
        /// Id of Customer who have overwritten splh
        /// </summary>
        [Required(ErrorMessage = "Customer cannot be null")]
        public string CustomerId { get; set; }

        /// <summary>
        /// Customer Name who have overwritten splh
        /// </summary>
        public string CustomerName { get; set; }

        /// <summary>
        /// Id of Customer who have overwritten splh
        /// </summary>
        [Required(ErrorMessage = "Case Id cannot be null")]
        public string CaseId { get; set; }

        /// <summary>
        /// This property stores short description for case
        /// </summary>
        public string CaseDescription { get; set; }

        public decimal? MaxContentVolume { get; set; }

        public decimal? OuterCubeVolume { get; set; }
        /// <summary>
        /// Id of Customer who have overwritten splh
        /// </summary>
        public decimal? EmptyWeight { get; set; }

        /// <summary>
        /// This is comment that is gave while adding new customer sku case preference
        /// </summary>
        public string Comment { get; set; }

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




    }
}