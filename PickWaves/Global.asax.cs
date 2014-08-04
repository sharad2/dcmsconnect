using EclipseLibrary.Mvc.Hosting;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Routing;

namespace DcmsMobile.PickWaves
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
                new { controller = MVC_PickWaves.PickWaves.Home.Name, action = MVC_PickWaves.PickWaves.Home.ActionNames.Index, id = UrlParameter.Optional }, // Parameter defaults
                new string[] { typeof(DcmsMobile.PickWaves.Areas.PickWaves.Controllers.HomeController).Namespace } // Add Namespace of controller
            ).DataTokens.Add("area", MVC_PickWaves.PickWaves.Name);
        }

        protected void Application_Start()
        {
            HostingEnvironment.RegisterVirtualPathProvider(new VirtualPathProviderEx("../DcmsMobile", new[] {
              Links_PickWaves.Content.Url(),
              Links_PickWaves.Scripts.Url()
            }));
            // Enabling Attribute routing
            RouteTable.Routes.MapMvcAttributeRoutes();

            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
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