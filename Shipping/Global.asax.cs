using System.Web.Mvc;
using System.Web.Routing;
using EclipseLibrary.Mvc.ModelBinding;

namespace DcmsMobile.Shipping
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = MVC_Shipping.Shipping.Home.Name, action = MVC_Shipping.Shipping.Home.ActionNames.Index, id = UrlParameter.Optional }, // Parameter defaults
                new string[] { typeof(DcmsMobile.Shipping.Areas.Shipping.Controllers.HomeController).Namespace } // Add Namespace of controller
            ).DataTokens.Add("area", MVC_Shipping.Shipping.Name);
        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
            //ModelBinders.Binders.DefaultBinder = new DefaultModelBinderEx();
        }

#if DEBUG
        /// <summary>
        /// Bypasses login when the app is run directly, but only in debug mode
        /// </summary>
        protected void Application_AuthenticateRequest()
        {
            this.Context.SkipAuthorization = true;
        }
#endif
    }
}