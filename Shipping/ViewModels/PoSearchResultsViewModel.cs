using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DcmsMobile.Shipping.ViewModels
{
    public class PoSearchResultsViewModel: LayoutTabsViewModel
    {
        public PoSearchResultsViewModel()
            : base(LayoutTabPage.PoSearchResults)
        {

        }
        public IList<PoStatusModel> PoList { get; set; }
    }
}