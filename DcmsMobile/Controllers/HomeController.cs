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

        public virtual ActionResult Categorized()
        {
            var model = new CategorizedViewModel
            {
                MenuItems = AreaItem.Areas.Where(p => !string.IsNullOrWhiteSpace(p.ShortName)).ToDictionary(p => p.ShortName, p => new MenuItem(p, Url))
            };
            return View(this.Views.Categorized, model);
        }

        private const string SESSION_KEY_MAINSITEURL = "DcmsMobile_RcLauncher_MainSiteUrl";

        [ActionName("Index")]
        [RcActionSelector(IsRcAction = true)]
        public virtual ActionResult RcIndex(string returnUrl)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl))
            {
                this.Session[SESSION_KEY_MAINSITEURL] = returnUrl;
            }
            var model = new RcLauncherViewModel
            {
                MainSiteUrl = string.Format("{0}", this.Session[SESSION_KEY_MAINSITEURL])
            };

            model.Init(this.ControllerContext, this.Url);
            model.MenuItems = from item in AreaItem.Areas
                              orderby item.Order, item.Name
                              select new RcMenuItem(item, Url);
            return View(this.Views.RcLauncher, model);
        }

        /// <summary>
        /// Accepts the choice entered by the user. If the entry does not correspond to a menu choice, it will be treated
        /// as an inquiry scan.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <remarks>
        /// If the choice was from the list of menu items displayed, the javascript is supposed to perform the redirection.
        /// Choice is posted.
        /// </remarks>
        public virtual ActionResult AcceptChoice(string choice, bool isMobile)
        {
            if (string.IsNullOrEmpty(choice))
            {
                var url = string.Format("{0}", this.Session[SESSION_KEY_MAINSITEURL]);
                if (string.IsNullOrWhiteSpace(url))
                {
                    return RedirectToAction(this.Actions.Index());
                }
                return Redirect(url);
            }

                var ar = CheckSearchResults(choice, isMobile);
                if (ar != null)
                {
                    return ar;
                }

                var handler = AreaItem.Areas.FirstOrDefault(p => !string.IsNullOrEmpty(p.ScanUrlFormatString));
                if (handler != null)
                {
                    // This means that Inquiry is installed and we will pass the choice to Inquiry
                    return Redirect(string.Format(handler.ScanUrlFormatString, handler.AreaName, choice));
                }
                return RedirectToAction(this.Actions.Index());
        }

        /// <summary>
        /// Checks whether any area is interested in displaying results for the passed search text
        /// </summary>
        /// <param name="searchText"></param>
        /// <returns></returns>
        /// <remarks>
        /// NonAction attribute has been applied only for readability.
        /// </remarks>
        [NonAction]
        private ActionResult CheckSearchResults(string searchText, bool isMobile)
        {
            searchText = searchText.ToUpper();

            // Exact match not found. Iterate all areas and subareas which are a potential match
            // Order By: If the hit is in the name, we subtract a large value from Order property. This means name its will display on top.
            //   subarea hits will be displayed before area hits.
            var results = (from subarea in AreaItem.Areas.Where(p => p.SubAreas != null).SelectMany(p => p.SubAreas)
                           where (!isMobile || subarea.IsMobileOptimized) && !string.IsNullOrWhiteSpace(subarea.ShortName)
                           let hitName = subarea.Name.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0
                           let hitDescription = subarea.Description.IndexOf(searchText, StringComparison.OrdinalIgnoreCase)
                           let hitKeyword = string.Compare(subarea.ShortName, searchText, true) == 0
                           where hitName || hitDescription >= 0 || hitKeyword
                           select new SearchResultModel(subarea, searchText)
                           {
                               Url = Url.RouteUrl(subarea.RouteValues),  //Url.RouteUrl((RouteValueDictionary)subarea.PropertyToCall.GetValue(null, null)),
                               Order = hitKeyword ? int.MinValue : (hitName ? subarea.Order - 100 : subarea.Order - 50)
                           }).Concat(
                          from area in AreaItem.Areas
                          where (!isMobile || area.IsMobileOptimized) && !string.IsNullOrWhiteSpace(area.ShortName)
                          let hitName = area.Name.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0
                          let hitDescription = area.Description.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0
                          let hitKeyword = string.Compare(area.ShortName, searchText, true) == 0
                          where hitName || hitDescription || hitKeyword
                          select new SearchResultModel(area, searchText)
                          {
                              Url = Url.Content(string.Format("~/{0}", area.AreaName)),
                              Order = hitKeyword ? int.MinValue : (hitName ? area.Order - 10 : area.Order)
                          }
            ).OrderBy(p => p.Order).ToArray();

            if (results.Length == 0)
            {
                return null;
            }

            if (results[0].Order == int.MinValue)
            {
                // Keyword hit. Redirect
                return Redirect(results[0].Url);
            }

            var model = new SearchViewModel
            {
                SearchText = searchText,
                Results = results
            };

            var x = AreaItem.Areas.Where(p => !string.IsNullOrEmpty(p.ScanUrlFormatString)).FirstOrDefault();
            if (x != null)
            {
                model.InquiryUrl = Url.Content(string.Format(x.ScanUrlFormatString, x.AreaName, searchText));
            }
            return View(Views.Search, model);
        }

        private void PopulateModel(LauncherViewModel model)
        {
            model.Init(this.ControllerContext, this.Url);
            model.MenuItems = (from item in AreaItem.Areas
                              orderby item.Order, item.Name
                              select new MenuItem(item, Url)).ToArray();

            //model.MenuItems = model.MenuItems.OrderBy(p => p.Order);
            //model.UrlRc = ConfigurationManager.AppSettings["RcUrl"] + "?returnUrl=" + Url.Encode(this.Request.Url.AbsoluteUri);
            model.UrlRc = RcActionSelectorAttribute.UrlRc + "?returnUrl=" + Url.Encode(this.Request.Url.AbsoluteUri);
            return;
        }

    }
}


//<!--$Id$-->
