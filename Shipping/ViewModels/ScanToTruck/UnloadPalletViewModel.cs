using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.Shipping.ViewModels.ScanToTruck
{
    public class UnloadPalletViewModel : ViewModelBase
    {
        [Key]
        [ReadOnly(false)]
        [Required]
        public int AppointmentNumber { get; set; }

        private string _scanText;
        [ReadOnly(false)]
        public string ScanText
        {
            get
            {
                return _scanText;
            }
            set
            {
                _scanText = value.ToUpper();
            }
        }

        public string ConfirmScanText { get; set; }       
    }
}