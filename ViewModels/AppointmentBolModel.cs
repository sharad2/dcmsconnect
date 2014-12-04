using System;
using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.Shipping.ViewModels
{
    /// <summary>
    /// A BOL which is part of an appointment. This model focuses on showing progress
    /// </summary>
    public class AppointmentBolModel
    {

        /// <summary>
        /// Nullabe as unschedule appointment don't have number.
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
        internal DateTime AppointmentDateTime { get; set; }

        /// <summary>
        /// Time of appointment.
        /// </summary>
        public string AppointmentTimeDisplay
        {

            get
            {
                return string.Format("{0:t}", this.AppointmentDateTime);
            }
        }

        public string AppointmentDateDisplay
        {

            get
            {
                return string.Format("{0:d}", this.AppointmentDateTime);
            }
        }

        public string CustomerId { get; set; }

        public string CustomerName { get; set; }

        public string CustomerDisplay
        {

            get
            {
                return string.Format(this.CustomerId + ":" + this.CustomerName);
            }
        }

        internal DateTime? StartTime { get; set; }

        internal DateTime? EndTime { get; set; }

        public string StartTimeDisplay
        {

            get
            {
                return string.Format("{0:t}", this.StartTime);
            }
        }

        public string EndTimeDisplay
        {

            get
            {
                return string.Format("{0:t}", this.EndTime);
            }
        }

        public int? TotalPalletCount { get; set; }

        public int? LoadedPalletCount { get; set; }

        public int? BoxesUnverifiedCount { get; set; }

        public string BoxesUnverifiedDisplay
        {
            get
            {
                return string.Format("{0:N0}", this.BoxesUnverifiedCount);
            }
        }

        public int BoxesUnverifiedPercent
        {
            get
            {
                if (this.BoxesTotalCount == 0 || this.BoxesUnverifiedCount == null || this.BoxesUnverifiedCount == 0)
                {
                    return 0;       // Not loaded at all
                }
                var pct = (int)Math.Round((decimal)this.BoxesUnverifiedCount.Value * 100 / (decimal)this.BoxesTotalCount);
                return Math.Max(pct, 10);
            }
        }

        /// <summary>
        /// Number of boxes in dock or door areas
        /// </summary>       
        public int? BoxesAtDockCount
        {
            get;
            set;
        }

        public string BoxesAtDockDisplay
        {
            get
            {
                return string.Format("{0:N0}", BoxesAtDockCount);
            }
        }

        public int BoxesAtDockPercent
        {
            get
            {
                if (this.BoxesTotalCount == 0 || this.BoxesAtDockCount == null || this.BoxesAtDockCount == 0)
                {
                    return 0;       // Not loaded at all
                }
                var pct = (int)Math.Round((decimal)this.BoxesAtDockCount.Value * 100 / (decimal)this.BoxesTotalCount);
                return Math.Max(pct, 10);
            }
        }

        public int? BoxesLoadedCount { get; set; }

        public string BoxesLoadedDisplay
        {
            get
            {
                return string.Format("{0:N0}", BoxesLoadedCount);
            }
        }

        public int BoxesLoadedPercent
        {
            get
            {
                if (this.BoxesTotalCount == 0 || this.BoxesLoadedCount == null || this.BoxesLoadedCount == 0)
                {
                    return 0;       // Not loaded at all
                }
                var pct = (int)Math.Round((decimal)this.BoxesLoadedCount.Value * 100 / (decimal)this.BoxesTotalCount);
                return Math.Max(pct, 10);
            }
        }

        public int BoxesTotalCount
        {
            get
            {
                return (this.BoxesUnverifiedCount ?? 0) + (this.BoxesAtDockCount ?? 0) + (this.BoxesLoadedCount ?? 0);
            }
        }

        public int? BoxesUnpalletizeCount { get; set; }
     
        /// <summary>
        /// POs in BOL
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? BolPoCount { get; set; }
 
        /// <summary>
        /// POs not in BOL
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? NoBolPoCount { get; set; }

    }
}
