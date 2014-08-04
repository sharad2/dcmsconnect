using EclipseLibrary.Mvc.Hosting;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Routing;

namespace DcmsMobile.REQ2
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
                new { controller = "Home", action = MVC_REQ2.REQ2.Home.ActionNames.Index, id = UrlParameter.Optional }, // Parameter defaults
                new string[] { "DcmsMobile.REQ2.Areas.REQ2.Controllers" } // Add Namespace of controller
            ).DataTokens.Add("area", "REQ2");
        }

        protected void Application_Start()
        {
            HostingEnvironment.RegisterVirtualPathProvider(new VirtualPathProviderEx("../DcmsMobile", new[] {
        Links_REQ2.Content.Url(),
        Links_REQ2.Scripts.Url()
      }));
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
            //ModelBinders.Binders.DefaultBinder = new DefaultModelBinderEx();
            this.Context.SkipAuthorization = true;
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
//$Id$