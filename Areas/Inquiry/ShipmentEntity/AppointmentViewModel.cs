using DcmsMobile.Inquiry.Areas.Inquiry.SharedViews;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.Inquiry.Areas.Inquiry.ShipmentEntity
{
    public class AppointmentViewModel
    {
        internal AppointmentViewModel(Appointment entity)
        {
            this.AppointmentArrivalDate = entity.AppointmentArrivalDate;
            this.AppointmentDate = entity.AppointmentDate;
            this.AppointmentId = entity.AppointmentId;
            this.AppointmentNumber = entity.AppointmentNumber;
            this.Carrier = entity.Carrier;
            this.LoadedBoxes = entity.LoadedBoxes;
            this.LoadedPallets = entity.LoadedPallets;
            this.ShipmentCount = entity.ShipmentCount;
            this.TotalBoxes = entity.TotalBoxes;
            this.TotalPallets = entity.TotalPallets;          
            this.SuspensePallets = entity.SuspensePallets;
            this.CustomerList = entity.CustomerList;
            this.BoxesInAreas = entity.BoxesInAreas;
        }

        public int AppointmentNumber { get; set; }

        public int AppointmentId { get; set; }

        [Display(Name = "Carrier")]
        [DisplayFormat(NullDisplayText = "Not Defined")]
        public string Carrier { get; set; }

        [Display(Name = "Appointment Date")]
        public DateTime AppointmentDate { get; set; }

        [Display(Name = "Arrival Date")]
        [DisplayFormat(NullDisplayText="Not Defined")]
        public TimeSpan? AppointmentArrivalDate { get; set; }

        public int? TotalBoxes { get; set; }

        public int? TotalPallets { get; set; }

        public int? LoadedBoxes { get; set; }

        public int? LoadedPallets { get; set; }
        
        public IEnumerable<string> CustomerList { get; set; }
        
        public int? ShipmentCount { get; set; }

        public int SuspensePallets { get; set; }

        public IEnumerable<string> BoxesInAreas { get; set; }

        public string UrlManageAppointmentLink { get; set; }

        public string UrlLoadTruck { get; set; }

        
    }
}