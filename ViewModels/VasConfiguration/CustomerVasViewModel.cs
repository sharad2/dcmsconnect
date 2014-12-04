using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace DcmsMobile.BoxManager.ViewModels.VasConfiguration
{
    public enum OrderType
    {
        [Display(Name = "All Orders")]
        AllOrders = 1,

        [Display(Name = "All Excluding Current Orders")]
        AllExcludingCurrentOrders = 2,

        [Display(Name = "Current Orders Only")]
        CurrentOrdersOnly = 3
    }

    public class CustomerVasViewModel : VasConfigurationViewModelBase
    {
        protected IDictionary<OrderType, DisplayAttribute> GetOrderTypeEnumDisplayAttributes()
        {
            return (from OrderType item in Enum.GetValues(typeof(OrderType))
                    select new
                    {
                        Value = item.GetType().GetMember(item.ToString())
                                         .Cast<MemberInfo>().Single().GetCustomAttributes(typeof(DisplayAttribute), false)
                                         .Cast<DisplayAttribute>()
                                         .SingleOrDefault(),
                        Key = item
                    }).Where(p => p.Value != null).ToDictionary(p => p.Key, p => p.Value);
        }


        [Required(ErrorMessage = "Customer is required")]
        [Display(Name = "Customer")]
        public string CustomerId { get; set; }

        public string CustomerName { get; set; }

        /// <summary>
        /// VAS ID for which customer created a pattern
        /// </summary>
        [Display(Name = "VAS Type")]
        public string VasId { get; set; }

        /// <summary>
        /// Descriptive text of VAS ID
        /// </summary>
        public string VasDescription { get; set; }

        /// <summary>
        /// User remarks entered by customer for a specific VAS pattern
        /// </summary>
        [Display(Name = "Remarks")]
        [StringLength(255, ErrorMessage = "User Remarks should be less than 255 characters.")]
        public string UserRemarks { get; set; }

        /// <summary>
        /// Description of VAS pattern which is already applied for customer (System generated)
        /// </summary>
        public string VasPatternDescription { get; set; }

        /// <summary>
        /// Regular expression generated for VAS settings
        /// </summary>
        public string PatternRegEx { get; set; }

        /// <summary>
        /// Is VAS setting is active or not
        /// </summary>
        public bool InactiveFlag { get; set; }
    }
}