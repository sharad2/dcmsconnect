using System.Web;
using System.Web.Mvc;
using System.Web.WebPages;
using EclipseLibrary.Mvc.Controllers;

namespace DcmsMobile.REQ2.Areas.REQ2.Controllers
{
    public partial class HomeController : EclipseController
    {
        //
        // GET: /REQ2/Home/

        public virtual ActionResult Index()
        {
            return View(Views.Index);
        }

        /// <summary>
        /// Toggles Mobile emulation
        /// </summary>
        /// <param name="emulateMobile"></param>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        public virtual ActionResult ToggleEmulation()
        {
                if (HttpContext.GetOverriddenBrowser().IsMobileDevice)
                {
                    HttpContext.ClearOverriddenBrowser();
                }
                else
                {
                    HttpContext.SetOverriddenBrowser(BrowserOverride.Mobile);                    
                }

                return RedirectToAction(Actions.Index());
        }

    }
}
