using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.Shipping.Repository
{
    public class Bol
    {
        [Key]
        public string ShippingId { get; set; }

        public string CarrierId { get; set; }

        public string CarrierDescription { get; set; }

        public DateTime? ShipDate { get; set; }

        public string BolCreatedBy { get; set; }

        public DateTime? BolCreatedOn { get; set; }

        public DateTime? StartDate { get; set; }

        public string ShipBuilding { get; set; }

        public DateTime? DcCancelDate { get; set; }

        public DateTime? CancelDate { get; set; }

        public int? PoCount { get; set; }

        public string CustomerDcId { get; set; }
  
        public int? AppointmentNumber { get; set; }

        public DateTime? AppointmentDateTime { get; set; }

        public int? AppointmentId { get; set; }

        public DateTime? AtsDate { get; set; }

        public int? EdiId { get; set; }

        public IEnumerable<DateTime?> PickupDateList { get; set; }
    }
}