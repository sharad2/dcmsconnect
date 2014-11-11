
using System.Collections.Generic;

namespace DcmsMobile.Receiving.ViewModels.Rad
{
    public class RadViewModel
    {
        public bool EnableEditing { get; set; }

        public IList<SpotCheckViewModel> SpotCheckList { get; set; }

        public SpotCheckViewModel SpotCheckViewModel { get; set; }

        public IList<SpotCheckViewModel> SpotCheckAreaList { get; set; }
    }
}



//$Id$