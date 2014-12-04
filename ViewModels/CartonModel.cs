using DcmsMobile.CartonManager.Models;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace DcmsMobile.CartonManager.ViewModels
{
    /// <summary>
    /// What is the rework status of the carton
    /// </summary>
    public enum ReworkStatus
    {
        /// <summary>
        /// Qualify: The rework status should not be looked at; Update: Should not be changed
        /// </summary>
        NotSet = 0,

        /// <summary>
        /// Qualify: The carton must need rework; Update: Rework has been completed on the carton
        /// </summary>
        NeedsRework = 1,
        CompleteRework = 1,

        /// <summary>
        /// Qualify: The carton does not need rework; Update: Undo Rework
        /// </summary>
        DoesNotNeedRework = 2
    }

    /// <summary>
    /// Defines qualification rules and values to be updated
    /// </summary>
    /// <remarks>
    /// Used for definining carton qualifications and carton update requirements. Maps to <see cref="Carton"/> model.
    /// </remarks>
    public sealed class CartonModel : SoundModel
    {

        private string _skuBarCode;
        /// <summary>
        /// A bar code which can uniquely identify the SKU
        /// </summary>
        /// <remarks>
        /// This bar code is usually a UPC code but can also be a customer specific non upc code.
        /// </remarks>
        [Display(Name = "SKU")]
        //[BindUpperCase]
        public string SkuBarCode
        {
            get
            {
                return _skuBarCode ?? string.Empty;
            }
            set
            {
                _skuBarCode = (value ?? string.Empty).ToUpper();
            }
        }

        /// <summary>
        /// The SKU Id selected
        /// </summary>
        [HiddenInput]
        public int SkuId { get; set; }

        /// <summary>
        /// When used as an updating rule, the carton will be changed to have these many pieces. When used as a qualification rule,
        /// the carton must have these many pieces before it can be updated.
        /// </summary>
        [Display(Name = "Pieces")]
        [Range(1, 999, ErrorMessage = "Pieces in carton must be bewteen 1 and 999")]
        public int? Pieces { get; set; }

        [Display(Name = "Virtual Warehouse")]
        public string VwhId { get; set; }

        [Display(Name = "Quality Code")]
        public string QualityCode { get; set; }

        private string _palletId;
        [Display(Name = "Pallet")]
        [RegularExpression(@"^([P|p]\S{1,7})", ErrorMessage = "Pallet Id must begin with P and max length should be less then 9.")]
        public string PalletId
        {
            get
            {
                return _palletId;
            }
            set
            {
                _palletId = (value ?? string.Empty).ToUpper();
            }
        }

        [Display(Name = "Destination Area")]
        public string AreaId { get; set; }

        public bool RemoveExistingPallet { get; set; }

        /// <summary>
        /// True if the carton needs work
        /// </summary>
        public ReworkStatus Rework { get; set; }

        [Display(Name = "Location")]
        public string LocationID { get; set; }

        [Display(Name = "Reason Code")]
        public string ReasonCode { get; set; }

        [Display(Name = "Season Code")]
        public string PriceSeasonCode { get; set; }

        public bool  IsReserved { get; set; }
    }
}



//$Id$
