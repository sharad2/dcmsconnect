using DcmsMobile.Shipping.Helpers;
using DcmsMobile.Shipping.Repository;
using EclipseLibrary.Mvc.Helpers;
using EclipseLibrary.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace DcmsMobile.Shipping.ViewModels
{
    /// <summary>
    /// This model is posted when an appointment is created or updated
    /// </summary>
    /// <remarks>
    /// This model is serialized to JSON. make sure that you apply ScriptIgnoreAttribute to all properties which are not needed in the JSON array.
    /// </remarks>
    [ModelBinder(typeof(AppointmentModelBinder))]
    public class AppointmentModel
    {
        private readonly HashSet<string> _classNames;

        public AppointmentModel()
        {
            _classNames = new HashSet<string>();           
        }

        public AppointmentModel(Appointment entity)
        {
            _classNames = new HashSet<string>();           
            this.id = entity.AppointmentId;
            this.AppointmentNumber = entity.AppointmentNumber;
            this.AppointmentDate = entity.AppointmentTime;
            this.BuildingId = entity.BuildingId;
            this.PickUpDoor = entity.PickUpDoor;
            this.Remarks = entity.Remarks;
            this.CarrierId = entity.CarrierId;
            this.CarrierName = entity.CarrierName;
            this.TruckArrivalTime = entity.AppointmentTime + entity.ArrivalDelay;
            this.RowSequence = entity.RowSequence;
            this.BolCount = entity.BolCount;
            this.AppointmentBols = entity.GetCustomerNames().Select(p => new AppointmentBolModel
            {
                CustomerName = p,
                CustomerId=entity.CustomerId,
                BolPoCount=entity.BolPoCount,
                NoBolPoCount=entity.NoBolPoCount
            });           
            this.BolBoxCount = entity.BolBoxCount;           
            this.NoBolBoxCount = entity.NoBolBoxCount;
            this.IsShipped = entity.IsShipped;           
        }

        #region Javascript Event data
        /// <summary>
        /// This is used during JSON serialization which happens in the AJAX call. It is also posted
        /// </summary>
        /// <remarks>
        /// Do not change the name of this property. Javascript relies on it.
        /// </remarks>
        [ReadOnly(false)]
        public int? id
        {
            get;
            set;
        }

        private string _title;
        /// <summary>
        /// This is used during JSON serialization which happens in the AJAX call
        /// </summary>
        [ReadOnly(true)]
        public string title
        {
            get
            {
                if (_title == null)
                {
                    var list = new List<string>(4);
                    //if (this.IsTimeZoneDifferent.HasValue && this.IsTimeZoneDifferent.Value && this.AppointmentDate.HasValue)
                    //{
                    //    list.Add(string.Format("({0:h:mm tt} local)", this.AppointmentDate.Value));
                    //}
                    if (this.AppointmentNumber != null)
                    {
                        list.Add(string.Format("#{0}", this.AppointmentNumber));
                    }
                    list.Add(this.BuildingId);
                    if (!string.IsNullOrEmpty(this.PickUpDoor))
                    {
                        list.Add(string.Format("Door {0}", this.PickUpDoor));
                    }
                    if (this.id == null)
                    {
                        list.Add(":Unschedule Appointment");
                    }
                    if (this.IsShipped)
                    {
                        list.Add(":Shipped");
                    }
                    _title = string.Join(" ", list);
                }
                return _title;
            }
            set
            {
                _title = value;
            }
        }

        /// <summary>
        /// Descriptive HTML which displays information about the appointment. Used only in Day view
        /// </summary>
        public string appointmentHtml { get; set; }

        /// <summary>
        /// While creating an appointment, the BOL page posts this as a UNIX time stamp
        /// </summary>
        [ReadOnly(false)]
        public string start
        {
            get
            {
                var str = string.Format("{0:o}", this.AppointmentDate);
                return str;
            }
            set
            {
                this.AppointmentDate = UnixTimestampBinder.GetDateFromUnixTimeStamp(long.Parse(value));
                //throw new NotImplementedException(value);
            }
        }

        /// <summary>
        /// CSS class to be applied to this appointment
        /// </summary>
        public string className
        {
            get
            {
                return string.Join(" ", _classNames);
            }
        }

        /// <summary>
        /// This is part of eventData so that the dialog can show it
        /// </summary>
        [Display(Name = "Door")]
        [BindUpperCase]
        [ReadOnly(false)]
        public string PickUpDoor { get; set; }

        /// <summary>
        /// This is part of eventData so that the dialog can show it
        /// </summary>
        [Display(Name = "Building")]
        [BindUpperCase]
        [ReadOnly(false)]
        public string BuildingId { get; set; }

        /// <summary>
        /// This is part of eventData so that the dialog can show it. Also shown as tooltip of the event
        /// </summary>
        [Display(Name = "Remarks")]
        [ReadOnly(false)]
        public string Remarks { get; set; }

        /// <summary>
        /// This is part of eventData so that the dialog can show it
        /// </summary>
        [Display(Name = "Carrier")]
        [ReadOnly(false)]
        public string CarrierId
        {
            get
            {
                return _carrierId;
            }
            set
            {
                _carrierId = value;
                //if (string.IsNullOrWhiteSpace(_carrierId))
                //{
                //    _classNames.Add(CLASSNAME_NOCARRIER);
                //}
                //else
                //{
                //    _classNames.Remove(CLASSNAME_NOCARRIER);
                //}
            }
        }

        public decimal? RowSequence { get; set; }

        #endregion

        /// <summary>
        /// This will be null when an appointment is created
        /// </summary>
        [ReadOnly(false)]
        [ScriptIgnore]
        [Key]
        public int? AppointmentNumber { get; set; }


        [Display(Name = "Date & Time")]
        [ScriptIgnore]
        [ReadOnly(false)]
        public DateTimeOffset? AppointmentDate { get; set; }

        internal void AddClassName(string className)
        {
            _classNames.Add(className);
        }

        internal void ClearClassNames()
        {
            _classNames.Clear();
        }

        private string _carrierId;

        [ScriptIgnore]
        public string CarrierName { get; set; }

        [ScriptIgnore]
        [DisplayFormat(NullDisplayText = "No carrier")]
        public string CarrierDisplay
        {
            get
            {
                if (string.IsNullOrWhiteSpace(this.CarrierId))
                {
                    return string.Empty;
                }
                return string.Format("{0}:{1}", this.CarrierId, this.CarrierName);
            }
        }

        /// <summary>
        /// This value is posted. It is also part of event data
        /// </summary>
        [ScriptIgnore]
        public DateTimeOffset? TruckArrivalTime { get; set; }

        [ScriptIgnore]
        [DisplayFormat(NullDisplayText = "Truck has not yet arrived")]
        public string TruckArrivalTimeDisplay
        {
            get
            {
                return string.Format("{0:t}", this.TruckArrivalTime);
            }
        }

        [ScriptIgnore]
        public int? TotalPalletCount
        {
            get
            {
                return this.AppointmentBols.Select(p => p.TotalPalletCount).Sum();
            }
        }

        [ScriptIgnore]
        public int? LoadedPalletCount
        {
            get
            {
                return this.AppointmentBols.Select(p => p.LoadedPalletCount).Sum();
            }
        }

        [ScriptIgnore]
        public int? LoadedBoxesCount
        {
            get
            {
                return this.AppointmentBols.Select(p => p.BoxesLoadedCount).Sum();
            }
        }


        [ScriptIgnore]
        public int? TotalBoxesCount
        {
            get
            {
                var totalBoxesCount = this.AppointmentBols.Select(p => p.BoxesLoadedCount).Sum() + this.AppointmentBols.Select(p => p.BoxesAtDockCount).Sum() + this.AppointmentBols.Select(p => p.BoxesUnverifiedCount).Sum();
                return totalBoxesCount;
            }
        }

        [ScriptIgnore]
        [DisplayFormat(DataFormatString = "{0}%", NullDisplayText = "Nothing")]
        public int PercentLoaded
        {
            get
            {
                if (this.TotalBoxesCount == 0 || this.TotalBoxesCount == null || this.LoadedBoxesCount == null || this.LoadedBoxesCount == 0)
                {
                    return 0;
                }
                var boxLoaded = (int)(this.LoadedBoxesCount * 100 / this.TotalBoxesCount);
                return boxLoaded;
            }
        }

        [ScriptIgnore]
        public int? UnpalletizeBoxCount
        {
            get
            {
                return this.AppointmentBols.Select(p => p.BoxesUnpalletizeCount).Sum();
            }
        }

        /// <summary>
        /// Scheduled bol list show in Day view.
        /// </summary>
        [ScriptIgnore]
        private IEnumerable<AppointmentBolModel> _appointmentBols;
        [ScriptIgnore]
        public IEnumerable<AppointmentBolModel> AppointmentBols
        {
            get
            {
                return _appointmentBols ?? Enumerable.Empty<AppointmentBolModel>();
            }
            set
            {
                _appointmentBols = value;
            }
        }

        /// <summary>
        /// Url of Report 110.21: Summary of the Shipments as well as cartons received.
        /// </summary>
        ///[ScriptIgnore]
        public string BolDetailUrl
        {
            get
            {
                return System.Configuration.ConfigurationManager.AppSettings["DcmsLiveBaseUrl"] + "Reports/Category_110/R110_21.aspx";
            }
        }

        /// <summary>
        /// Laoding time is the time when truck arrived and user start loading,
        /// when loading is complete then time interval between truck arrive and loading time.
        /// </summary>
        [ScriptIgnore]
        public TimeSpan? LoadingTime
        {
            get
            {
                if (this.TruckArrivalTime != null)
                {
                    var loding = this.AppointmentBols.Select(p => p.EndTime).Max();
                    return (loding - this.TruckArrivalTime);
                }
                return null;
            }
        }

        /// <summary>
        /// Show current time in day view because if user print day view then show reality of all progress on that time.
        /// </summary>
        [ScriptIgnore]
        public DateTime? CurrentTime
        {
            get
            {
                return DateTime.Now;
            }
        }
        /// <summary>
        /// No of BOLs associated with appointment
        /// </summary>
        [ScriptIgnore]
        public int? BolCount { get; set; }

       
        /// <summary>
        /// No of BOXs associated with appointment
        /// </summary>
        [ScriptIgnore]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? BolBoxCount { get; set; }

          
        /// <summary>
        /// No of BOXs associated with appointment
        /// </summary>
        [ScriptIgnore]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? NoBolBoxCount { get; set; }
       
        /// <summary>
        /// Appointment is shipped or not 
        /// </summary>
        public bool IsShipped { get; set;}
  

        /// <summary>
        /// Dispaly UTC offset if timezone of user is different from time zone of appointment.
        /// </summary>
        public string AppointmentOffsetDisplay
        {
            get
            {

                if (this.AppointmentDate.HasValue)
                {
                    return string.Format("UTC {0}:{1}", this.AppointmentDate.Value.Offset.Hours, this.AppointmentDate.Value.Offset.Minutes);
                }

                else
                {
                    return string.Empty;
                }
            }
        }

        /// <summary>
        /// Offset of appointment date.Used for comparison with timezone of user.
        /// </summary>
        public TimeSpan? OffSet
        {
            get
            {
                if (this.AppointmentDate == null)
                {
                    return null;
                }
             
                return this.AppointmentDate.Value.Offset;

            }
        }

        /// <summary>
        /// Used to display local time if timezone of user is different from apoointment's timezone.
        /// </summary>
        public string InitialDateIso
        {
            get
            {
                if (this.AppointmentDate == null)
                {
                    return null;
                }
                return string.Format("{0:o}", this.AppointmentDate.Value);
            }
        }

      
    }

    /// <summary>
    /// AppointmentDate is posted as two values. One value is the date and the other value is the time. These two values are merged by this binder.
    /// </summary>
    internal class AppointmentModelBinder : DefaultModelBinderEx
    {
        protected override void SetProperty(ControllerContext controllerContext, ModelBindingContext bindingContext, PropertyDescriptor propertyDescriptor, object value)
        {
            if (propertyDescriptor.Name == ReflectionHelpers.NameFor((AppointmentModel m) => m.AppointmentDate))
            {
                var values = bindingContext.ValueProvider.GetValue(propertyDescriptor.Name).RawValue as string[];
                if (values != null && values.Length == 2)
                {
                    // Update the value to include the time
                    var dateString = values.First(p => p.Contains('/'));
                    var date = DateTimeOffset.Parse(dateString);
                    var timeString = values.First(p => !p.Contains('/'));
                    var time = long.Parse(timeString);
                    value = date + UnixTimestampBinder.GetDateFromUnixTimeStamp(time).TimeOfDay;
                }
            }
            base.SetProperty(controllerContext, bindingContext, propertyDescriptor, value);
        }
    }
}