using System;
using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.Shipping.ViewModels.ScanToTruck
{
    public class PalletModel
    {
        [Key]
        public string PalletId { get; set; }

        [Display(Name = "Area")]
        public string IaId { get; set; }

        public string LocationId { get; set; }

        public int? BoxesCount { get; set; }

        public DateTime? TruckLoadDate { get; set; }
    }
}