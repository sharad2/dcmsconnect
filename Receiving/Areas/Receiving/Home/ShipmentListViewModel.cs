using DcmsMobile.Receiving.Areas.Receiving.SharedViews;
using System.Collections.Generic;

namespace DcmsMobile.Receiving.Areas.Receiving.Home
{
    public class ShipmentListViewModel : ViewModelBase
    {
        public IList<ShipmentListModel> ShipmentList { get; set; }
    }
}