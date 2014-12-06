using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace DcmsMobile.BoxManager.ViewModels.Home
{
    /// <summary>
    /// The view model for the main page
    /// </summary>
    /// <remarks>
    /// </remarks>
    public class ScanToPalletViewModel : ViewModelBase
    {
        /// <summary>
        /// What did the user scan ? This value is posted
        /// </summary>
        public string ScanText
        {
            get;
            set;
        }

        /// <summary>
        /// Current Pallet. This will be a temporary pallet not visible in the UI
        /// </summary>
        [HiddenInput]
        public string PalletId { get; set; }

        /// <summary>
        /// The UCC Id of the box which was last put on the pallet        
        /// </summary>
        /// <remarks>
        /// The last box scanned by ther user
        /// </remarks>
        public string LastBoxId { get; set; }

        #region Sort Criteria
        /// <summary>
        /// Customer of current pallet. This cannot be null if there is an active pallet
        /// </summary>
        [Display(ShortName = "Customer")]
        [DisplayFormat(NullDisplayText = "Unknown")]
        public string CustomerId { get; set; }

        /// <summary>
        /// PO of the boxes of current pallet. Null if PO mixing is allowed.
        /// </summary>
        [Display(ShortName = "PO")]
        [DisplayFormat(NullDisplayText="Any")]
        public string PoId { get; set; }

        /// <summary>
        /// Customer DC of the boxes of current pallet  
        /// </summary>
        //[Display (Name = "DC")]
        [Display(ShortName = "DC")]
        [DisplayFormat(NullDisplayText = "Any")]
        public string CustomerDcId { get; set; }

        /// <summary>
        /// Bucket of the boxes of current pallet
        /// </summary>
        //[Display (Name ="Bucket")]
        [Display(ShortName = "Wave")]
        [DisplayFormat(NullDisplayText = "Any")]
        public int? BucketId { get; set; }
        #endregion

        /// <summary>
        /// Current pallet's location
        /// </summary>
        /// <remarks>
        /// Location of current pallet. Comma seperated, if there are multiple locations.
        /// </remarks>
        [DisplayFormat(NullDisplayText = "Unknown")]
        [Display(Name = "Location")]
        public string PalletLocationList
        {
            get;
            set;
        }

        /// <summary>
        /// Current pallet's location
        /// </summary>
        /// <remarks>
        /// Location of current pallet. Comma seperated, if there are multiple locations.
        /// </remarks>
        [DisplayFormat(NullDisplayText = "Unknown")]
        [Display(Name = "Area")]
        public string PalletAreaList
        {
            get;
            set;
        }

        /// <summary>
        /// The volume of boxes currently on pallet
        /// </summary>
        public decimal TotalBoxVolume
        {
            get;
            set;
        }

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
        /// <summary>
        /// Total number of boxes for the set criteria
        /// </summary>
        public int QualifyingBoxCount { get; set; }

        public int CountBoxesOnPallet { get; set; }

        //private IEnumerable<PalletModel> _palletSuggestionList; 

        /// <summary>
        /// Pallet suggestion list.
        /// </summary>
        public IList<PalletModel> PalletSuggestionList
        {
            get;
        //{ return _palletSuggestionList ?? (_palletSuggestionList = Enumerable.Empty<PalletModel>().ToList()); }
            set;
            //{
            //    _palletSuggestionList = value;
            //}
        }

        public string ConfirmScanText { get; set; }
    }
}
