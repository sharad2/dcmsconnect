using DcmsMobile.Models;
using EclipseLibrary.Mvc.Controllers;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;

namespace DcmsMobile.Controllers
{
    /// <summary>
    /// Provides the list of menu choices and handles the postback of the selected choice
    /// </summary>
    /// <remarks>
    /// Menu choices are built by reflecting all assemblies in the bin directory looking for Area information
    /// </remarks>
    public partial class HomeController : EclipseController
    {
        /// <summary>
        /// It is important to order the menu choices, otherwise they can display in different order each time;
        /// </summary>
        /// <returns></returns>
        public virtual ActionResult Launcher()
        {
            var model = new LauncherViewModel();
            model.Init(this.ControllerContext, this.Url);
            model.MenuItems = (from item in AreaItem.Areas
                               orderby item.Order, item.Name
                               select new MenuItem(item, Url)).ToArray();
            var rcUrl = ConfigurationManager.AppSettings["RcUrl"];
            if (!string.IsNullOrWhiteSpace(rcUrl))
            {
                model.UrlRc = rcUrl.TrimEnd(' ', '/').Trim() + Url.Action(Actions.RcItems());
            }
            return View(this.Views.Launcher, model);
        }

        /// <summary>
        /// Called from the main site to retrieve the list of programs available on RC
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public virtual ActionResult RcItems()
        {
            return Json(AreaItem.Areas.Select(p => new MenuItem(p, Url)).Select(p => new
            {
                itemid = p.ItemId
            }));
        }

    }
}


//<!--$Id$-->
