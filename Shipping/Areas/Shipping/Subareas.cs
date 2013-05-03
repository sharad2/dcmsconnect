using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System.Web.Routing;

namespace DcmsMobile.Shipping.Areas.Shipping
{
    public class Subareas
    {
        [Display(ShortName = "COR", Name = "Customer Order Routing", Description = "Create EDI 753 to route customer orders")]
        [UIHint("desktop", "DcmsMobile")]
        public static RouteValueDictionary RoutingSummary
        {
            get
            {
                return MVC_Shipping.Shipping.Home.RoutingSummaryAll().GetRouteValueDictionary();
            }
        }

        [Display(ShortName = "APP", Name = "Manage Appointments", Description = "Create and manage appointments")]
        [UIHint("desktop", "DcmsMobile")]
        public static RouteValueDictionary Appointment
        {
            get
            {
                return MVC_Shipping.Shipping.Home.AllAppointments().GetRouteValueDictionary();
            }
        }

        [Display(ShortName = "STT", Name = "Scan To Truck", Description = "Load boxes on truck to ensure accuracy of shipment")]
        [UIHint("mobile", "DcmsMobile")]
        public static RouteValueDictionary ScanToTruck
        {
            get
            {
                return MVC_Shipping.Shipping.ScanToTruck.Index().GetRouteValueDictionary();
            }
        }

    }
}
