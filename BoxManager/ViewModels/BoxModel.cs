using System;
using System.ComponentModel.DataAnnotations;
using DcmsMobile.BoxManager.Helpers;
using DcmsMobile.BoxManager.Repository;

namespace DcmsMobile.BoxManager.ViewModels
{
    /// <summary>
    /// Represents a box which can be placed on a pallet if it passes validations
    /// </summary>
    public class BoxModel
    {
        public BoxModel()
        {

        }

        public BoxModel(Box entity)
        {
            this.PalletId = entity.PalletId;
            this.Ucc128Id = entity.Ucc128Id;
            this.Volume = entity.Volume;
            this.Case = entity.Case;
            this.RejectionCode = entity.RejectionCode;
            this.SmallShipmentFlag = entity.SmallShipmentFlag;
            this.StopProcessDate = entity.StopProcessDate;
            this.TransferDate = entity.StopProcessDate;
            this.VerifyDate = entity.VerifyDate;
        }
        [Key]
        [Display(Name = "Recent Boxes")]
        public string Ucc128Id { get; set; }

        [Display(Name = "Case")]
        public string Case { get; set; }

        public string PalletId { get; set; }

        [Display(Name = "cu ft")]
        public decimal Volume { get; set; }

        /// <summary>
        /// VerifyDate will be required conditionally on Controller basis of UI.
        /// </summary>
        public DateTime? VerifyDate { get; set; }

        [MustBeNull(ErrorMessage = "Box/Pallet has failed quality check")]
        public string RejectionCode { get; set; }

        [MustBeNull(ErrorMessage = "The box/Pallet has already been closed and no modifications can be made to it")]
        public DateTime? TransferDate { get; set; }

        [MustBeNull(ErrorMessage = "The box/Pallet has either been shipped or cancelled")]
        public DateTime? StopProcessDate { get; set; }

        
        /// <summary>
        /// Boxes of small shipments can be palletized for VAS, but can't be palletized for normal palletization in STP and M2P UI
        /// </summary>
        public string SmallShipmentFlag { get; set; }

    }
}