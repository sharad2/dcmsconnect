using EclipseLibrary.Mvc.Hosting;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Mvc.Routing;
using System.Web.Routing;


namespace BoxPick
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

            //routes.MapRoute(
            //    "Default", // Route name
            //    "{controller}/{action}/{id}", // URL with parameters
            //    new { controller = "Home", action = "Index", id = UrlParameter.Optional }, // Parameter defaults
            //    new string[] { "DcmsMobile.BoxPick.Areas.BoxPick.Controllers" }
            //).DataTokens.Add("area", "BoxPick");

        }

        protected void Application_Start()
        {
            RouteTable.Routes.MapMvcAttributeRoutes(new MyRouteProvider());
            //HostingEnvironment.RegisterVirtualPathProvider(new VirtualPathProviderEx("../DcmsMobile", new[] {
            //    Links_BoxPick.Content.Url(),
            //    //Links_BoxPick.Scripts.Url()
            //  }));
            // Enabling Attribute routing
            //RouteTable.Routes.MapMvcAttributeRoutes();
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
        }

#if DEBUG
        protected void Application_AuthenticateRequest()
        {
            this.Context.SkipAuthorization = true;
        }
#endif
    }

    internal class MyRouteProvider : DefaultDirectRouteProvider
    {
        protected override string GetAreaPrefix(ControllerDescriptor controllerDescriptor)
        {
            return string.Empty;  // Ignore area prefix when app is being run directly. This makes it possible to reach the right page with the URL http://localhost
        }
    }
}
