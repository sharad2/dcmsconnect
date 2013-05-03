using EclipseLibrary.Mvc.ModelBinding;
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

        [ReadOnly(false)]
        [BindUpperCase]
        public string ScanText { get; set; }

        public string ConfirmScanText { get; set; }

        /// <summary>
        /// From which building,area the pallets for this appointment will be pulled to load on the truck
        /// </summary>
        //public string Context { get; set; }
    }
}