using System;
using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.BoxManager.ViewModels.Home
{
    public class LocationViewModel : ViewModelBase
    {
        public string ScanText { get; set; }

        public string PalletId { get; set; }

        public decimal TotalBoxVolume { get; set; }

        [Display(ShortName = "Customer")]
        [DisplayFormat(NullDisplayText = "Unknown")]
        public string CustomerId { get; set; }
        
        /// <summary>
        /// Current pallet's location
        /// </summary>
        /// <remarks>
        /// Location of current pallet. Comma seperated, if there are multiple locations.
        /// </remarks>

        [DisplayFormat(NullDisplayText = "Unknown")]
        [Display(Name = "Current Location")]
        public string CurrentPalletLocation { get; set; }

        /// <summary>
        /// Current pallet's location
        /// </summary>
        /// <remarks>
        /// Location of current pallet. Comma seperated, if there are multiple locations.
        /// </remarks>
        [DisplayFormat(NullDisplayText = "Unknown")]
        [Display(Name = "Current Area")]
        public string CurrentPalletArea { get; set; }

        [Display(Name = "Pallet Limit")]
        public decimal PalletLimit { get; set; }

        /// <summary>
        /// How full is the pallet
        /// </summary>
        public int PercentFull
        {
            get
            {
                if (this.PalletLimit == 0)
                {
                    return 0;
                }
                var limit = (int)(this.TotalBoxVolume * 100 / this.PalletLimit);
                return Math.Min(100, limit);
            }
        }

        public int BoxesOnPalletCount { get; set; }        
    }
}