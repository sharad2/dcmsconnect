using EclipseLibrary.Mvc.Hosting;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Routing;

namespace DcmsMobile.BoxManager
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : HttpApplication
    {
        private static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        private static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            // By default show the area home page
            // http://www.dondevelopment.com/2011/02/09/asp-net-mvc-2-default-route-to-area/
            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = MVC_BoxManager.BoxManager.Home.Name, action = MVC_BoxManager.BoxManager.Home.ActionNames.Index, id = UrlParameter.Optional, area = "BoxManager" }, // Parameter defaults
                new[] { typeof(Areas.BoxManager.Controllers.HomeController).Namespace }
            ).DataTokens.Add("area", "BoxManager");

        }

        protected void Application_Start()
        {

            HostingEnvironment.RegisterVirtualPathProvider(new VirtualPathProviderEx("../DcmsMobile", new[] {
                Links_BoxManager.Content.Url(),
                Links_BoxManager.Scripts.Url()
              }));
            // Enabling Attribute routing
            RouteTable.Routes.MapMvcAttributeRoutes();
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




//$Id: Global.asax.cs 10911 2011-12-20 04:42:41Z rkandari $