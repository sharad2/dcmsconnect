using System;
using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.BoxManager.ViewModels.MovePallet
{
    public class ValidatePalletViewModel : ViewModelBase
    {
        public int? BoxCount { get; set; }

        public bool IsConfirm { get; set; }

        [Required]
        public string SourcePalletId { get; set; }

        public int VerifiedBoxes { get; set; }
        
        #region Sort Criteria
        [DisplayFormat(NullDisplayText = "Any")]
        [Display(ShortName = "Customer")]       
        public string CustomerId { get; set; }
        
        public int? BucketId { get; set; }

        /// <summary>
        /// PO of the box
        /// </summary>
        public string PoId { get; set; }

        /// <summary>
        /// Customer DC of the box
        /// </summary>       
        public string CustomerDcId { get; set; }
        #endregion

        /// <summary>
        /// Current box location
        /// </summary>     
        [DisplayFormat(NullDisplayText = "Unknown")]
        [Display(Name = "Location")]
        public string SourcePalletLocationId { get; set; }

        [DisplayFormat(NullDisplayText = "Unknown")]
        [Display(ShortName = "Area")]
        public string SourcePalletAreaId { get; set; }

    }
}