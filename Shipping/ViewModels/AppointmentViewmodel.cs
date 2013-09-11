using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using EclipseLibrary.Mvc.Helpers;

namespace DcmsMobile.Shipping.ViewModels
{
    public enum CalendarViewName
    {
        month,
        basicDay,
        basicWeek
    }

    public class AppointmentViewModel:LayoutTabsViewModel
    {
        public AppointmentViewModel(): base(LayoutTabPage.Appointment)
        {
        }
        /// <summary>
        /// The initial date to display
        /// </summary>

        public DateTimeOffset? InitialDate { get; set; }

        public CalendarViewName InitialViewName { get; set; }

        /// <summary>
        /// Stored in data-attr
        /// </summary>
        public string InitialDateIso
        {
            get
            {
                if (this.InitialDate == null)
                {
                    return string.Empty;
                }
                return string.Format("{0:o}", this.InitialDate);
            }
        }
        /// <summary>
        /// The appointment number to search for
        /// </summary>
        public int? AppointmentNumber { get; set; }

        
        public int? AppointmentId { get; set; }

        public IEnumerable<SelectListItem> BuildingList { get; set; }

    }

    internal class AppointmentViewModelUnbinder : LayoutTabsViewModelUnbinder
    {
        protected override void DoUnbindModel(RouteValueDictionary routeValueDictionary, LayoutTabsViewModel model)
        {
            var avm = (AppointmentViewModel)model;
            if (avm.AppointmentId.HasValue)
            {
                routeValueDictionary.Add(avm.NameFor(m => m.AppointmentId), avm.AppointmentId);
            }
            base.DoUnbindModel(routeValueDictionary, model);
        }
    }
}