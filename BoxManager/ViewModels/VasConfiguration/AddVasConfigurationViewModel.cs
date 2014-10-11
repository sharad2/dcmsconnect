using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace DcmsMobile.BoxManager.ViewModels.VasConfiguration
{
    public class AddVasConfigurationViewModel : VasConfigurationViewModelBase
    {
        [Required(ErrorMessage = "Customer is required")]
        [Display(Name = "Customer")]
        public string CustomerId { get; set; }

        public string CustomerName { get; set; }

        [Display(Name = "For Labels")]
        public string Labels { get; set; }

        public VasPoPatternType? PatternType { get; set; }

        public string PatternAlphabet { get; set; }

        public IEnumerable<SelectListItem> VasCodeList { get; set; }

        /// <summary>
        /// VAS ID for which customer created a pattern
        /// </summary>
        [Display(Name = "VAS Type")]
        public string VasId { get; set; }

        /// <summary>
        /// Discriptive text of VAS ID
        /// </summary>
        public string VasDescription { get; set; }

        /// <summary>
        /// User remarks entered by customer for a specific VAS pattern
        /// </summary>
        [Display(Name = "Remarks")]
        public string UserRemarks { get; set; }

        /// <summary>
        /// Description of VAS pattern which is already applied for customer (System generated)
        /// </summary>
        public string VasPatternDescription { get; set; }
    }
}