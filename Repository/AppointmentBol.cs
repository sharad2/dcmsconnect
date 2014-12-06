using System;
using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.Shipping.Repository
{
    /// <summary>
    /// A BOL which is part of an appointment. This model focuses on showing progress
    /// </summary>
    public class AppointmentBol
    {

        /// <summary>
        /// Nullabel as unschedule appointment don't have number.
        /// </summary>
        [Key]
        public int? AppointmentNumber { get; set; }

        /// <summary>
        /// Nullabel as unschedule appointment don't have Id.
        /// </summary>
        [Key]
        public int? AppointmentId { get; set; }

        [Key]
        public string ShippingId { get; set; }
       

        [Key]
        public DateTime AppointmentDateTime { get; set; }

        public string CustomerId { get; set; }

        public string CustomerName { get; set; }

        public int? AtDockBoxCount { get; set; }

        public int? LoadedBoxCount { get; set; }

        public int? TotalPalletCount { get; set; }

        public int? LoadedPalletCount { get; set; }

        public int? UnverifiedBoxCount { get; set; }

        public DateTime? StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public int? UnpalletizeBoxCount { get; set; }

        public string BuildingId { get; set; }
                 
        public int? NoBolPoCount { get; set; }

        public int? BolPoCount { get; set; }
    }
}