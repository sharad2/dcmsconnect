using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using DcmsMobile.Helpers;
using EclipseLibrary.Mvc.ModelBinding;
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
            AreaRegistration.RegisterAllAreas();
            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);

            ViewEngines.Engines.Clear();
            ViewEngines.Engines.Add(new Microsoft.Web.Mvc.FixedRazorViewEngine());

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