using System.Collections.Generic;

namespace DcmsMobile.Shipping.ViewModels
{
    public class PoSearchResultsViewModel : LayoutTabsViewModel
    {
        public PoSearchResultsViewModel()
            : base(LayoutTabPage.PoSearchResults)
        {
        }
        public IList<PoStatusModel> PoList { get; set; }
    }
}