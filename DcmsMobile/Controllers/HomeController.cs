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
        /// Without the trailing /
        /// </summary>
        private string UrlRcBase
        {
            get
            {
                var urlRcBase = ConfigurationManager.AppSettings["RcUrl"];
                if (!string.IsNullOrWhiteSpace(urlRcBase))
                {
                    urlRcBase = urlRcBase.TrimEnd(' ', '/').Trim();
                }
                return urlRcBase;
            }
        }
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
            model.UrlRcBase = this.UrlRcBase;

            return View(this.Views.Launcher, model);
        }

        /// <summary>
        /// Called from the main site to retrieve the list of programs available on RC
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public virtual ActionResult RcItems()
        {
            var query = AreaItem.Areas.Select(p => new MenuItem(p, Url))
                .Select(p => new
                {
                    itemid = p.ItemId,
                    url = p.Url
                });
            return Json(query);
        }

    }
}


//<!--$Id$-->
