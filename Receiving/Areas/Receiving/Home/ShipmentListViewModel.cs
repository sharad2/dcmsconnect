using System.Collections.Generic;

namespace DcmsMobile.Receiving.ViewModels.Home
{
    public class ShipmentListViewModel : ViewModelBase
    {
        public IList<ShipmentListModel> ShipmentList { get; set; }
    }
}