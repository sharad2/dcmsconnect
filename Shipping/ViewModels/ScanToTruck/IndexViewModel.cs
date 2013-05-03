using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.Shipping.ViewModels.ScanToTruck
{
    public class IndexViewModel : ViewModelBase
    {
        [Key]
        public int? AppointmentNo { get; set; }
    }
}