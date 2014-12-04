using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

namespace DcmsMobile.BoxManager.ViewModels.VasConfiguration
{
    public class EnableVasConfigurationViewModel : CustomerVasViewModel
    {
        /// <summary>
        /// VAS used to apply and still applies
        /// </summary>
        public IList<string> ListBeingApplied { get; set; }

        [Required]
        public OrderType OrderType { get; set; }

        public IList<SelectListItem> OrderTypes
        {
            get
            {
                return (from item in GetOrderTypeEnumDisplayAttributes()
                       where item.Key != OrderType.CurrentOrdersOnly //Excluding current order option from choice list, because it will be implied explicitly
                       select new SelectListItem
                       {
                           Text = item.Value.Name,
                           Value = Convert.ToInt32(item.Key).ToString(),
                           Selected = item.Key==OrderType.AllOrders
                       }).ToArray();
            }
        }
    }
}