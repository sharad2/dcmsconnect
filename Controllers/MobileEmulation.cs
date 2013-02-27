using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EclipseLibrary.Mvc.Controllers
{
    /// <summary>
    /// Provides functions to emulate mobile screens on desktop browsers. The emulation mode is stored in the session
    /// </summary>
    [Obsolete("Not needed in MVC 4")]
    public static class MobileEmulation
    {
        /// <summary>
        /// The value <c>EmulateMobile</c> is well known and should not be changed. PalletLocating manipulates this cookie to force rendering of mobile views.
        /// </summary>
        private const string SESSION_EMULATEMOBILE = "EmulateMobile";

        /// <summary>
        /// Alwas returns false for actual Mobile Devices. For non mobile devices, returns whether a session variable is forcing emulation of a mobile device.
        /// </summary>
        /// <param name="controllerContext"></param>
        /// <returns></returns>
        public static bool IsEmulatingMobileDevice(ControllerContext controllerContext)
        {
            return !controllerContext.HttpContext.Request.Browser.IsMobileDevice && controllerContext.HttpContext.Session[SESSION_EMULATEMOBILE] != null;
        }

        /// <summary>
        /// Sets up the session variable which will cause emulation of a mobile browser
        /// </summary>
        /// <param name="controllerContext"></param>
        /// <param name="bEmulate">true to begin emulation. false to end emulation</param>
        public static void EmulateMobile(ControllerContext controllerContext, bool bEmulate)
        {
            //var cookie = new HttpCookie(COOKIE_EMULATEMOBILE);
            if (bEmulate)
            {
                // We will emulate mobile for this session
                //cookie.Expires = DateTime.Now.AddHours(4);
                controllerContext.HttpContext.Session[SESSION_EMULATEMOBILE] = true;
            }
            else
            {
                //cookie.Expires = DateTime.Now.AddHours(-1);
                controllerContext.HttpContext.Session.Remove(SESSION_EMULATEMOBILE);
            }
            //controllerContext.HttpContext.Response.Cookies.Add(cookie);
        }
    }
}
