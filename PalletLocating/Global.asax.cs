using EclipseLibrary.Mvc.Hosting;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Routing;

namespace DcmsMobile.PalletLocating
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
                new { controller = MVC_PalletLocating.PalletLocating.Home.Name, action = MVC_PalletLocating.PalletLocating.Home.ActionNames.Building, id = UrlParameter.Optional }, // Parameter defaults
                new string[] { typeof(DcmsMobile.PalletLocating.Areas.PalletLocating.Controllers.HomeController).Namespace } // Add Namespace of controller
            ).DataTokens.Add("area", "PalletLocating");
        }

        protected void Application_Start()
        {
            HostingEnvironment.RegisterVirtualPathProvider(new VirtualPathProviderEx("../DcmsMobile", new[] {
              Links_PalletLocating.Content.Url(),
              Links_PalletLocating.Scripts.Url()
            }));
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);

            //ModelBinders.Binders.DefaultBinder = new DefaultModelBinderEx();

            //ViewEngines.Engines.Clear();
            //ViewEngines.Engines.Add(new MobileCapableRazorViewEngine());
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


/*
    $Id$ 
    $Revision$
    $URL$
    $Header$
    $Author$
    $Date$
*/