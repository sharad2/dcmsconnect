using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace DcmsMobile.Shipping.ViewModels.ScanToTruck
{
    public class PalletViewModel : ViewModelBase
    {
        [Key]
        [ReadOnly(false)]
        [Required]
        public int AppointmentNumber { get; set; }

        private IEnumerable<PalletModel> _palletSuggestion;
        public IEnumerable<PalletModel> PalletSuggestionList
        {
            get
            {
                return _palletSuggestion ?? Enumerable.Empty<PalletModel>();
            }
            set
            {
                _palletSuggestion = value;
            }
        }

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

        public string CarrierId { get; set; }

        public int? TotalPalletCount { get; set; }

        public int? LoadedPalletCount { get; set; }

        public int? LoadedBoxCount { get; set; }

        public int? TotalBoxCount { get; set; }

        public int? UnLoadedPalletCount
        {
            get
            {
                return TotalPalletCount - LoadedPalletCount;
            }
        }

        public int? UnPalletizeBoxCount { get; set; }

        public int PercentFull
        {
            get
            {
                if (this.TotalBoxCount == 0 || this.TotalBoxCount == null || this.LoadedBoxCount == null || this.LoadedBoxCount == 0)
                {
                    return 0;
                }
                var loadedBox = (int)(this.LoadedBoxCount * 100 / this.TotalBoxCount);
                return loadedBox;
            }
        }

        [DisplayFormat(NullDisplayText = "Unknown")]
        public string DoorId { get; set; }

        /// <summary>
        /// Building from where the appointment will ship.
        /// </summary>
        public string AppointmentBuildingId { get; set; }

        /// <summary>
        /// From which building,area the pallets for this appointment will be pulled to load on to the truck
        /// </summary>
        //public string Context { get; set; }

        /// <summary>
        /// Pallet which is suggested by system.
        /// </summary>
        public string SuggestedPallet { get; set; }

        public int? PalletsInSuspenseCount { get; set; }
    }
}