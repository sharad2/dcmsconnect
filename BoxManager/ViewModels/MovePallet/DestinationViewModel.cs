using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace DcmsMobile.BoxManager.ViewModels.MovePallet
{
    public class DestinationViewModel : ViewModelBase
    {
        public string ScanText { get; set; }

        /// <summary>
        /// This must always be posted
        /// </summary>
        [Required]
        public string SourcePalletId { get; set; }

        /// <summary>
        /// Current box location
        /// </summary>     
        [DisplayFormat(NullDisplayText = "Unknown")]
        [Display(Name = "Location")]
        public string SourcePalletLocationId { get; set; }

        [DisplayFormat(NullDisplayText = "Unknown")]
        [Display(ShortName = "Area")]
        public string SourcePalletAreaId { get; set; }

        #region Sort Criteria
        [DisplayFormat(NullDisplayText = "Any")]
        [Display(ShortName = "Customer")]
        [Required]
        public string CustomerId { get; set; }

        /// <summary>
        /// Bucket of the box
        /// </summary>
        [DisplayFormat(NullDisplayText = "Any")]
        [Display(ShortName = "Wave")]
        public int? BucketId { get; set; }

        /// <summary>
        /// PO of the box
        /// </summary>
        [DisplayFormat(NullDisplayText = "Any")]
        [Display(ShortName = "PO")]
        public string PoId { get; set; }

        /// <summary>
        /// Customer DC of the box
        /// </summary>       
        [DisplayFormat(NullDisplayText = "Any")]
        [Display(ShortName = "DC")]
        public string CustomerDcId { get; set; }
        #endregion

        public int TotalBoxesOnPallet { get; set; }


        //private IEnumerable<BoxModel> _boxList;
        //[Obsolete]
        //public IEnumerable<BoxModel> BoxList
        //{
        //    get
        //    {
        //        return _boxList ?? Enumerable.Empty<BoxModel>();
        //    }
        //    set
        //    {
        //        _boxList = value;
        //    }
        //}

        public decimal TotalBoxVolume { get; set; }

        [Display(Name = "Pallet Limit")]
        public decimal PalletVolumeLimit { get; set; }

        /// <summary>
        /// How full is the pallet
        /// </summary>
        public int PercentFull
        {
            get
            {
                if (this.PalletVolumeLimit == 0)
                {
                    return 0;
                }
                var limit = (int)(this.TotalBoxVolume * 100 / this.PalletVolumeLimit);
                return Math.Min(100, limit);
            }
        }

        /// <summary>
        /// Location suggestion list.
        /// </summary>
        private IList<PalletModel> _locationSuggestionList;
        public IList<PalletModel> LocationSuggestionList
        {
            get
            {
                return _locationSuggestionList ?? (_locationSuggestionList = Enumerable.Empty<PalletModel>().ToList());
            }
            set
            {
                _locationSuggestionList = value;
            }
        }

        [DisplayFormat(NullDisplayText = "Unknown")]
        [Display(ShortName = "Appointment")]
        public int? AppointmentNo { get; set; }

        [DisplayFormat(NullDisplayText = "Unknown")]
        [Display(ShortName = "Door")]
        public string DoorId { get; set; }

        [DisplayFormat(NullDisplayText = "Unknown")]
        [Display(ShortName = "Appointment Date")]
        public DateTime? AppointmentDate { get; set; }
    }
}