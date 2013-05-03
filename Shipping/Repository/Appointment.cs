using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.Shipping.Repository
{
    public class Appointment
    {
        /// <summary>
        /// Id is nullable as unschedule appointments do not have Id.
        /// </summary>
        [Key]
        public int? AppointmentId { get; set; }

        /// <summary>
        /// Number is nullable as unschedule appointments do not have number.
        /// </summary>
        public int? AppointmentNumber { get; set; }

        public DateTimeOffset? AppointmentTime { get; set; }

        public string BuildingId { get; set; }

        public string PickUpDoor { get; set; }

        public string CarrierId { get; set; }

        public string CarrierName { get; set; }

        public string Remarks { get; set; }

        /// <summary>
        /// How much was the delay when compared to AppointmentTime
        /// </summary>
        public TimeSpan? ArrivalDelay { get; set; }

        /// <summary>
        /// Row sequence of an appointment. Needed when we update an appointment.
        /// </summary>
        public decimal? RowSequence { get; set; }

        /// <summary>
        /// No of BOLs in the appointment.
        /// </summary>
        public int? BolCount { get; set; }

        /// <summary>
        /// '-' separated list of customers whose BOLs are associated with appointment
        /// </summary>
        public string CustomerNames { get; set; }

        public string[] GetCustomerNames()
        {
            if (string.IsNullOrEmpty(this.CustomerNames)) {
                return new string[0];   // empty array
            }
            return this.CustomerNames.Split(new [] {"|"}, StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// No of Boxes in the appointment for which BOLs have been created.
        /// </summary>
        public int? BolBoxCount { get; set; }

        /// <summary>
        /// No of POs in appointment
        /// </summary>
        public int? BolPoCount { get; set; }

      
        public string CustomerId { get; set; }


        public int? NoBolBoxCount { get; set; }

        public int? NoBolPoCount { get; set; }


        /// <summary>
        /// Appointment is shipped or not
        /// </summary>
        public bool IsShipped { get; set; }
    }
}