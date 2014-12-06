using EclipseLibrary.Mvc.Hosting;
using System;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Mvc.Routing;
using System.Web.Routing;
using System.Web.WebPages;

namespace DcmsMobile.Receiving
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

            //// By default show the area home page
            //// http://www.dondevelopment.com/2011/02/09/asp-net-mvc-2-default-route-to-area/
            //routes.MapRoute(
            //    "Default", // Route name
            //    "{controller}/{action}/{id}", // URL with parameters
            //    new { controller = "Home", action = "Index", id = UrlParameter.Optional, area = "Receiving" }, // Parameter defaults
            //    new [] { typeof(Areas.Receiving.Home.HomeController).Namespace }
            //).DataTokens.Add("area", "Receiving");

        }

        protected void Application_Start()
        {
            RouteTable.Routes.MapMvcAttributeRoutes(new MyRouteProvider());
            //HostingEnvironment.RegisterVirtualPathProvider(new VirtualPathProviderEx("../DcmsMobile", new[] {
            //    T4MVCHelpers.ProcessVirtualPath("~/Content"),
            //    T4MVCHelpers.ProcessVirtualPath("~/Scripts"),
            //    T4MVCHelpers.ProcessVirtualPath("~/MainArea"),
            //    T4MVCHelpers.ProcessVirtualPath("~/fonts")
            //  }));
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);

            #region Display Modes
            // Sharad: Code which makes extensions .mobile.cshtml and .phone.cshtml recognizable
            // Conditions are checked in the order they are added
            DisplayModeProvider.Instance.Modes.Clear();

            // Smartphones are detected if the user agent contains one of these strings
            var phones = new[] {
                "Android",
                "iPhone",
                "iPad"
            };

            // Known Smart Phones will use the .phone.cshtml extension
            // Deepak's phone User Agent for Samsung Galaxy Y
            //  Mozilla/5.0 (Linux; U; Android 2.3.6; en-gb; GT-S5360 Build/GINGERBREAD) AppleWebKit/533.1 (KHTML, like Gecko) Version/4.0 Mobile Safari/533.1
            // Ankit's phone user agent Samsung Galaxy Note
            //  Mozilla/5.0 (Linux; U; Android 4.1.2; en-gb; GT-N7000 Build/JZO54K) AppleWebKit/534.30 (KHTML, like Gecko) Version/4.0 Mobile Safari/534.30
            // iPhone 5 UserAgent when using Safari:
            //   Mozilla/5.0 (iPhone; CPU iPhone OS 7_1_1 like Mac OS X) AppleWebKit/537.51.2 (KHTML, like Gecko) Version/7.0 Mobile/11D201 Safari/9537.53
            //DisplayModeProvider.Instance.Modes.Add(new DefaultDisplayMode("phone")
            //{
            //    ContextCondition = ctx =>  true
            //});

            // .mobile file is to be used only for ringscanners
            // RingScanner User Agent: Mozilla/4.0 (compatible; MSIE 6.0; Windows CE)
            DisplayModeProvider.Instance.Modes.Add(new DefaultDisplayMode("mobile")
            {
                ContextCondition = ctx => string.Compare(ctx.Request.Browser.Platform, "WinCE", StringComparison.InvariantCultureIgnoreCase) == 0
            });

            // If a .desktop.cshtml file exists, it will be served to non phone user agents
            // Change this to true to use .desktp files
            DisplayModeProvider.Instance.Modes.Add(new DefaultDisplayMode("desktop")
            {
                ContextCondition = ctx => false
            });

            // If no specific extension is found, simply use the .cshtml extension
            DisplayModeProvider.Instance.Modes.Add(new DefaultDisplayMode());
            #endregion

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

    internal class MyRouteProvider : DefaultDirectRouteProvider
    {
        protected override string GetAreaPrefix(ControllerDescriptor controllerDescriptor)
        {
            return string.Empty;  // Ignore area prefix when app is being run directly. This makes it possible to reach the right page with the URL http://localhost
        }
    }
}




//$Id$