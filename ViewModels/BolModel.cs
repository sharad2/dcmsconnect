using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc;
using DcmsMobile.Shipping.Helpers;

namespace DcmsMobile.Shipping.ViewModels
{
    public class BolModel
    {
        [Key]
        public string ShippingId { get; set; }

        public string BolCreatedBy { get; set; }

        internal DateTime? BolCreatedOn { get; set; }

        internal DateTime? PickUpDate { get; set; }

        internal DateTime? ShipDate { get; set; }

        internal DateTime? StartDate { get; set; }

        internal DateTime? DcCancelDate { get; set; }

        internal DateTime? CancelDate { get; set; }

        public int? PoCount { get; set; }
        
        [DisplayFormat(DataFormatString="{0:d}")]
        public IEnumerable<DateTime?> PickupDateList { get; set; }

        public int NumberOfPickupDates { get; set; }

        public string CustomerDcId { get; set; }

        /// <summary>
        /// Appointment associated with the model. The id of this appointment will be null if it does not exist in the database
        /// </summary>
        public AppointmentModel Appointment { get; set; }

        public IHtmlString BolCreatedOnDisplay
        {
            get
            {
                if (this.BolCreatedOn == null)
                {
                    return MvcHtmlString.Empty;
                }
                string str;
                var span = this.BolCreatedOn.Value - DateTime.Today;
                if (span.TotalDays < 0)
                {
                    str = string.Format("<span class='ui-state-error' title='The Bol was created {1} days ago'>{0:d}</span>", this.BolCreatedOn, Math.Abs(span.TotalDays));
                }
                else
                {
                    // Normal case
                    str = string.Format("<span title='The Bol creation date is {0}'>{0:ddd d MMM}</span>", this.BolCreatedOn);
                }
                return MvcHtmlString.Create(str);
            }
        }

        public IHtmlString ShipDateDisplay
        {
            get
            {
                if (this.ShipDate == null)
                {
                    return MvcHtmlString.Empty;
                }
                string str;
                var span = this.ShipDate.Value - DateTime.Today;
                if (span.TotalDays < 0)
                {
                    str = string.Format("<span class='ui-state-error' title='The Bol ship date was {1} days ago'>{0:d}</span>", this.ShipDate, Math.Abs(span.TotalDays));
                }
                else
                {
                    // Normal case
                    str = string.Format("<span title='The ship date is {0}'>{0:ddd d MMM}</span>", this.ShipDate);
                }
                return MvcHtmlString.Create(str);
            }
        }
        
        /// <summary>
        /// If PickUpDate date is null then Ats is PickUpDate.
        /// </summary>
        public IHtmlString PickUpDateDisplay
        {
            get
            {
                var date=new DateTime();
                if (this.PickUpDate != null)
                {
                    date = this.PickUpDate.Value;                   
                }
                else if (this.AtsDate != null)
                {
                    date = this.AtsDate.Value;
                }
                else
                {
                    return MvcHtmlString.Empty;
                }
                string str;
                var span = date - DateTime.Today;
                if (span.TotalDays < 0)
                {
                    str = string.Format("<span class='ui-state-error' title='The Bol pickup date was {1} days ago'>{0:d}</span>", date, Math.Abs(span.TotalDays));
                }
                else
                {
                    // Normal case
                    str = string.Format("<span  title='The pickup date is {0}'>{0:ddd d MMM}</span>", date);
                }
                if (!this.HasPickupdate)
                {
                    str += "<span class='ui-icon ui-icon-info' style='display: inline-block;cursor: pointer' title='Pickup date is not available hence considering Ats date as pickup date.' ></span>";
                }
                if (this.NumberOfPickupDates > 1)
                {
                    str += string.Format("<span class='ui-icon ui-icon-alert' style='display: inline-block;cursor: pointer' title='The BOL have multiple pickup dates {0}'></span>", string.Join(",", this.PickupDateList));
                }
                return MvcHtmlString.Create(str);
            }
        }

        // Considered as pickupdate in case pickup date is null
        internal DateTime? AtsDate { get; set; }

        public bool HasPickupdate
        {
            get
            {
                return this.PickUpDate != null;
            }
        }

        public IHtmlString StartDateDisplay
        {
            get
            {
                if (this.StartDate == null)
                {
                    return MvcHtmlString.Empty;
                }
                string str;
                var span = this.StartDate.Value - DateTime.Today;
                if (span.TotalDays < 0)
                {
                    str = string.Format("<span class='ui-state-error' title='The Bol start date was {1} days ago'>{0:d}</span>", this.StartDate, Math.Abs(span.TotalDays));
                }
                else
                {
                    // Normal case
                    str = string.Format("<span title='The start date is {0}'>{0:ddd d MMM}</span>", this.StartDate);
                }
                return MvcHtmlString.Create(str);
            }
        }

        public IHtmlString DcCancelDateDisplay
        {
            get
            {
                if (this.DcCancelDate == null)
                {
                    return MvcHtmlString.Empty;
                }
                string str;
                var span = this.DcCancelDate.Value - DateTime.Today;
                if (span.TotalDays < 0)
                {
                    str = string.Format("<span class='ui-state-error' title='The Bol DC cancel date was {1} days ago'>{0:d}</span>", this.DcCancelDate, Math.Abs(span.TotalDays));
                }
                else
                {
                    // Normal case
                    str = string.Format("<span title='The DC cancel date is {0}'>{0:ddd d MMM}</span>", this.DcCancelDate);
                }
                return MvcHtmlString.Create(str);
            }
        }

        public IHtmlString CancelDateDisplay
        {
            get
            {
                if (this.CancelDate == null)
                {
                    return MvcHtmlString.Empty;
                }
                string str;
                var span = this.CancelDate.Value - DateTime.Today;
                if (span.TotalDays < 0)
                {
                    str = string.Format("<span class='ui-state-error' title='The Bol cancel date was {1} days ago'>{0:d}</span>", this.CancelDate, Math.Abs(span.TotalDays));
                }
                else
                {
                    // Normal case
                    str = string.Format("<span title='The cancel date is {0}'>{0:ddd d MMM}</span>", this.CancelDate);
                }
                return MvcHtmlString.Create(str);
            }
        }

        /// <summary>
        /// Used to show the calendar icon
        /// </summary>
        public bool IsScheduled
        {
            get
            {
                return this.Appointment.id != null;
            }
        }

        /// <summary>
        /// Edi associated with BOL,helps to select recently crearted BOLs
        /// </summary>
        public int? EdiId { get; set; }


        /// <summary>
        /// Ats Date of BOL
        /// </summary>
        public decimal? BolAtsDate
        {
            get
            {
                return AtsDate.HasValue ? UnixTimestampBinder.GetUnixTimeStamp(AtsDate.Value) : (decimal?)null;
            }
        }

    }
}