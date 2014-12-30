using System;
using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.Shipping.Repository.ScanToTruck
{
    public class Box
    {
        [Key]
        public string Ucc128Id { get; set; }

        public string PalletId { get; set; }

        public DateTime? StopProcessDate { get; set; }

        public DateTime? VerifyDate { get; set; }

        public DateTime? TruckLoadDate { get; set; }

        public int? AppointmentNumber { get; set; }
    }
}