using System;
using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.Shipping.Repository
{
    public class PoStatus
    {
        [Key]
        public string PoId { get; set; }

        [Key]
        public string DcId { get; set; }

        [Key]
        public string CustomerId { get; set;}

        public string CustomerName { get; set; }

        public int? CountUntoutedPO { get; set; }

        public int? CountRoutedPO { get; set; }

        public int? CountRoutingInprogressPo { get; set; }

        public string ShippingId { get; set; }

        public string BuildingId { get; set;}

        public DateTime? AtsDate { get; set;}

        public DateTime? DCCancelDate { get; set;}
    }
}
