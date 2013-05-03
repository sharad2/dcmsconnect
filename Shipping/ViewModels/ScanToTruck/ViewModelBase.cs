
namespace DcmsMobile.Shipping.ViewModels.ScanToTruck
{
    public class ViewModelBase :LayoutTabsViewModel
    {
        public ViewModelBase()
            : base(LayoutTabPage.ScanToTruck)
        {
        }
        public char Sound { get; set; }
    }
}