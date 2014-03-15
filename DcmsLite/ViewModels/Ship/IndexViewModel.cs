using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DcmsMobile.DcmsLite.ViewModels.Ship
{
    public class IndexViewModel : ViewModelBase
    {
        public IList<PoModel> PoList { get; set; }
    }
}