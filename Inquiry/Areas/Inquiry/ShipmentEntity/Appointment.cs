using System;
using System.Collections.Generic;

namespace DcmsMobile.Inquiry.Areas.Inquiry.ShipmentEntity
{
    internal class Appointment
    {
        public int AppointmentNumber { get; set; }

        public int AppointmentId { get; set; }

        public string Carrier { get; set; }
                
        public DateTime AppointmentDate { get; set; }

        public TimeSpan? AppointmentArrivalDate { get; set; }

        public int? TotalBoxes { get; set; }

        public int? TotalPallets { get; set; }

        public int? LoadedBoxes { get; set; }

        public int? LoadedPallets { get; set; }

        public int? ShipmentCount { get; set; }

        public int SuspensePallets { get; set; }

        public IEnumerable<string> CustomerList { get; set; }

        public IEnumerable<string> BoxesInAreas { get; set; }

    }
}