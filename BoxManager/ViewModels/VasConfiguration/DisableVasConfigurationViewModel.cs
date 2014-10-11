using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

namespace DcmsMobile.BoxManager.ViewModels.VasConfiguration
{
    public class DisableVasConfigurationViewModel : CustomerVasViewModel
    {
        public IList<string> ListBeingApplied { get; set; }

        [Required]
        public OrderType OrderType { get; set; }

        public IList<SelectListItem> OrderTypes
        {
            get
            {
                return (from item in GetOrderTypeEnumDisplayAttributes()
                       select new SelectListItem
                       {
                           Text = item.Value.Name,
                           Value = Convert.ToInt32(item.Key).ToString(),
                           Selected = item.Key == OrderType.AllOrders
                       }).ToArray();
            }
        }
    }
}