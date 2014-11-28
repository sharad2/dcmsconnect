using System;
using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.Inquiry.Areas.Inquiry.CartonEntity
{
    /// <summary>
    /// This model is used to keep track on Carton History.
    /// It's helpful to store the audit history of a carton.
    /// </summary>
    /// <remarks>
    /// TODO: Create class CartonProcessModel and then make this class internal
    /// </remarks>
    internal class CartonProcess
    {
        [Required]
        public string ModuleCode { get; set; }

        public string InsertedBy { get; set; }

        public string FromPalletId { get; set; }

        public string ToPalletId { get; set; }

        public string FromCartonArea { get; set; }

        public string ToCartonArea { get; set; }

        public DateTime? InsertDate { get; set; }

        public DateTime? OldSuspenseDate { get; set; }

        public DateTime? NewSuspenseDate { get; set; }

        public string FromLocation { get; set; }

        public string ToLocation { get; set; }

        public string ActionPerformed { get; set; }

        public int? OldCartonQuantity { get; set; }

        public int? NewCartonQuantity { get; set; }

    }
}
