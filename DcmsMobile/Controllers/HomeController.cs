using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using DcmsMobile.Helpers;
using DcmsMobile.Models;
using EclipseLibrary.Mvc.Controllers;

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
        [ActionName("Index")]
        [RcActionSelector(IsRcAction = false)]
        public virtual ActionResult Index()
        {
            var model = new LauncherViewModel();
            PopulateModel(model);
            return View(this.Views.Launcher, model);
        }



        private void PopulateModel(LauncherViewModel model)
        {
            model.Init(this.ControllerContext, this.Url);
            model.MenuItems = (from item in AreaItem.Areas
                              orderby item.Order, item.Name
                              select new MenuItem(item, Url)).ToArray();

            model.UrlRc = RcActionSelectorAttribute.UrlRc + "?returnUrl=" + Url.Encode(this.Request.Url.AbsoluteUri);
            return;
        }

    }
}


//<!--$Id$-->
