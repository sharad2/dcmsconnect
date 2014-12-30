using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.Shipping.Repository.ScanToTruck
{
    public class Pallet
    {
        [Key]
        public string PalletId { get; set; }

        public string IaId { get; set; }

        public string LocationId { get; set; }

        public int? BoxesCount { get; set; }
    }
}