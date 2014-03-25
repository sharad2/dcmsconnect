using System.Collections.Generic;

namespace DcmsMobile.Models
{


    public class CategorizedViewModel: ViewModelBase
    {
        public CategorizedViewModel()
        {

        }

        public IDictionary<string, MenuItem> MenuItems
        {
            get;
            set;
        }
    }
}