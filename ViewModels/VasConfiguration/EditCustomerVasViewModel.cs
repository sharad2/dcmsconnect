using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace DcmsMobile.BoxManager.ViewModels.VasConfiguration
{
    public class EditCustomerVasViewModel : EditVasModelBase
    {
        /// <summary>
        /// User can create VAS pattern on the basis of labels only no need to pass specific PO pattern.
        /// False: Labels only True: PO pattern will also apply.
        /// </summary>
        public bool DoApplyPoPattern { get; set; }

        public IEnumerable<SelectListItem> PatternTypes
        {
            get
            {
                return from item in GetEnumDisplayAttributes<VasPoPatternType>()
                       select new SelectListItem
                           {
                               Text = item.Value.Name,
                               Value = Convert.ToInt32(item.Key).ToString()
                           };
            }
        }

        public IEnumerable<SelectListItem> PatternSubTypes
        {
            get
            {
                return from item in GetEnumDisplayAttributes<VasPoTextType>()
                       select new SelectListItem
                       {
                           Text = item.Value.Name,
                           Value = Convert.ToInt32(item.Key).ToString()
                       };
            }
        }
    }
}