using DcmsMobile.Helpers;
using EclipseLibrary.Mvc.ModelBinding;
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.WebPages;

namespace DcmsMobile
{

    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : HttpApplication
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
                new { controller = MVC_DcmsMobile.Home.Name, action = MVC_DcmsMobile.Home.ActionNames.Index, id = UrlParameter.Optional }, // Parameter defaults
                new string[] { typeof(DcmsMobile.Controllers.HomeController).Namespace }
            );

        }

        protected void Application_Start()
        {

            // Enabling Attribute routing
            RouteTable.Routes.MapMvcAttributeRoutes();

            AreaRegistration.RegisterAllAreas();
            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);

            ViewEngines.Engines.Clear();
            ViewEngines.Engines.Add(new RazorViewEngine());

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
            // iPad 2 User agent Safari
            //   Mozilla/5.0 (iPad; CPU OS 7_1_1 like Mac OS X) AppleWebKit/537.51.2 (KHTML, like Gecko) Version/7.0 Mobile/11D201 Safari/9537.53
            // iPad 2 User agent Chrome
            //   Mozilla/5.0 (iPad; CPU OS 7_1_1 like Mac OS X) AppleWebKit/537.51.1 (KHTML, like Gecko) CriOS/35.0.1916.38 Mobile/11D201 Safari/9537.53
            // Dinesh's phone Lenovo S720
            //  Mozilla/5.0 (Linux; Android 4.0.4; Lenovo S720 Build/IMM76D) AppleWebKit/535.19 (KHTML, like Gecko) Chrome/18.0.1025.166 Mobile Safari/535.19
            DisplayModeProvider.Instance.Modes.Add(new DefaultDisplayMode("phone")
            {
                ContextCondition = ctx => phones.Any(p => ctx.GetOverriddenUserAgent().IndexOf(p, StringComparison.InvariantCultureIgnoreCase) >= 0)
            });

            // .mobile file is to be used only for ringscanners
            // RingScanner User Agent: Mozilla/4.0 (compatible; MSIE 6.0; Windows CE)
            DisplayModeProvider.Instance.Modes.Add(new DefaultDisplayMode("mobile")
            {
                ContextCondition = ctx => string.Compare(ctx.Request.Browser.Platform, "WinCE", StringComparison.InvariantCultureIgnoreCase) == 0
            });

            // If a .desktop.cshtml file exists, it will be served to non phone user agents
            //DisplayModeProvider.Instance.Modes.Add(new DefaultDisplayMode("desktop")
            //{
            //    ContextCondition = ctx => !phones.Any(p => ctx.GetOverriddenUserAgent().IndexOf(p, StringComparison.InvariantCultureIgnoreCase) >= 0)
            //});

            // If no specific extension is found, simply use the .cshtml extension
            DisplayModeProvider.Instance.Modes.Add(new DefaultDisplayMode());
            #endregion

            ModelBinders.Binders.DefaultBinder = new DefaultModelBinderEx();

            // Suppress the error A potentially dangerous Request.Path value was detected from the client. This error occurs when the user types special characters such as <span> in the textbox.
            // Addidng this entry suppresses the error as suggested by http://stackoverflow.com/questions/9232213/a-potentially-dangerous-request-form-value-was-detected-from-the-client
            GlobalFilters.Filters.Add(new ValidateInputAttribute(false));
        }

        /// <summary>
        /// This is necessary so that the response can be flushed without causing errors. DisplayModelForDebug wants to flush the
        /// response.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>
        /// <para>
        /// http://stackoverflow.com/questions/904952/whats-causing-session-state-has-created-a-session-id-but-cannot-save-it-becaus
        /// </para>
        /// <para>
        /// At the start of a session, clear the overridden browser.
        /// </para>
        /// </remarks>
        protected void Session_Start(object sender, EventArgs e)
        {
            string sessionId = Session.SessionID;
            var wrapper = new HttpContextWrapper(this.Context);
            wrapper.ClearOverriddenBrowser();

        }

        protected void Application_Error()
        {
            var error = Server.GetLastError();
            if (error != null)
            {
                var ev = new ApplicationErrorEvent("Error caught in global.asax Application_Error", this, error);
                ev.Raise();
            }
        }

    }


}