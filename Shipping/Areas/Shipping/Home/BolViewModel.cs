using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Script.Serialization;
using EclipseLibrary.Mvc.Helpers;

namespace DcmsMobile.Shipping.ViewModels
{
    public class BolViewModel: LayoutTabsViewModel
    {
        public BolViewModel()
            : base(LayoutTabPage.Bol)
        {
        }

        public BolViewModel(string customerId, int? appointmentId, decimal? bolAtsDate)
            : base(LayoutTabPage.Bol)
        {
            this.PostedCustomerId = customerId;
            this.BolAtsDate = bolAtsDate;
            this.InitialAppointment = new AppointmentModel
            {
                id = appointmentId
            };
            
        }

      
        /// <summary>
        /// List of selected BOls
        /// </summary>
        [ReadOnly(false)]
        public ICollection<string> SelectedBols { get; set; }

        /// <summary>
        /// List of unshipped BOLs
        /// </summary>
        private ICollection<BolModel> _bol;
        public ICollection<BolModel> Bols
        {
            get
            {
                return _bol ?? new BolModel[0];
            }
            set
            {
                _bol = value;
            }
        }

        /// <summary>
        /// True->Show BOLs having appointment.
        /// </summary>
        public bool ShowScheduledAlso { get; set; }

        /// <summary>
        /// Initial appointmemnt to be focused while navigating from appointment screen
        /// </summary>
        public AppointmentModel InitialAppointment { get; set; }

       
        public IHtmlString JsonInitialAppointment
        {
            get
            {
                if (this.InitialAppointment == null)
                {
                    return MvcHtmlString.Empty;
                }
                var ser = new JavaScriptSerializer();
                return MvcHtmlString.Create(ser.Serialize(this.InitialAppointment));
            }
        }
        /// <summary>
        /// Returns a disctionary of appoitments with BOL Id as the key
        /// </summary>
        public IHtmlString JsonAppointments
        {
            get
            {
                var query = this.Bols.ToDictionary(p => p.ShippingId, p => p.Appointment);
                var ser = new JavaScriptSerializer();
                return MvcHtmlString.Create(ser.Serialize(query));
            }
        }

        /// <summary>
        /// Indicates whether BOLs need to be assigned or unassigned to/from an appointment.
        /// When true, then InitialAppointment.id must be passed.
        /// As a precaution, AssignAppointment() raises error if this flag is not passed (i.e. it is null).
        /// </summary>
        [ReadOnly(false)]
        public bool? AssignFlag { get; set; }

        /// <summary>
        /// Used to show a status message, which would act as a link to appointment page.  
        /// </summary>
        public int? AssignedAppointmentNumber { get; set; }
        public int? AssignedAppointmentId { get; set; }
        public int? AssignedBolCount { get; set; }

        /// <summary>
        /// Url of Report 110.21: Summary of the BOLs according to passed Shipping_Id.
        /// </summary>      
        public string BOLsDetailUrl
        {
            get
            {
                return System.Configuration.ConfigurationManager.AppSettings["DcmsLiveBaseUrl"] + "Reports/Category_110/R110_21.aspx";
            }
        }

        /// <summary>
        /// Ats date against each BOL.Used to highlight BOLs while navigating from Routed after BOL creation
        /// </summary>
        public decimal? BolAtsDate { get; set; }

    }

    internal class BolViewModelUnbinder : LayoutTabsViewModelUnbinder
    {
        protected override void DoUnbindModel(RouteValueDictionary routeValueDictionary, LayoutTabsViewModel model)
        {
            var bvm = (BolViewModel)model;
            if (bvm.InitialAppointment != null && bvm.InitialAppointment.id.HasValue)
            {
                routeValueDictionary.Add(bvm.NameFor(m => m.InitialAppointment.id), bvm.InitialAppointment.id);
            }
            if (bvm.AssignedAppointmentId.HasValue)
            {
                routeValueDictionary.Add(bvm.NameFor(m => m.AssignedAppointmentId), bvm.AssignedAppointmentId);
            }
            if (bvm.AssignedAppointmentNumber.HasValue)
            {
                routeValueDictionary.Add(bvm.NameFor(m => m.AssignedAppointmentNumber), bvm.AssignedAppointmentNumber);
            }
            if (bvm.AssignedBolCount.HasValue)
            {
                routeValueDictionary.Add(bvm.NameFor(m => m.AssignedBolCount), bvm.AssignedBolCount);
            }         
            if (bvm.BolAtsDate.HasValue)
            {
                routeValueDictionary.Add(bvm.NameFor(m => m.BolAtsDate), bvm.BolAtsDate);
            }
            base.DoUnbindModel(routeValueDictionary, model);
        }
    }
}