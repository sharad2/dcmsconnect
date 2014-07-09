
using EclipseLibrary.Mvc.Controllers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Script.Serialization;
using System.Xml.Linq;

namespace DcmsMobile.MainArea.Home
{
    /// <summary>
    /// Provides the list of menu choices and handles the postback of the selected choice. This is the default controller so it has no route prefix
    /// </summary>
    /// <remarks>
    /// Menu choices are built by reflecting all assemblies in the bin directory looking for Area information
    /// </remarks>
    [RoutePrefix("home")]
    public partial class HomeController : EclipseController
    {

        private readonly XNamespace _ns = "http://schemas.eclsys.com/dcmsconnect/menuitems";

        private const string MENUITEMS_XML_FILE_NAME = "~/App_Data/MenuItems.xml";

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
                    urlRcBase = urlRcBase.Trim().TrimEnd('/');
                }
                return urlRcBase;
            }
        }
        /// <summary>
        /// It is important to order the menu choices, otherwise they can display in different order each time;
        /// This is the action for the home page and is the default route
        /// </summary>
        /// <returns></returns>
        public virtual ActionResult Index()
        {
            var model = new LauncherViewModel();
            //model.Init(this.ControllerContext, this.Url);
            model.MenuItems = (from item in AreaItem.Areas
                               orderby item.Order, item.Name
                               select new MenuItem(item, Url)).ToArray();

            model.UrlRcBase = this.UrlRcBase;

            // Load only those categories which have some menu items
            model.Categories = GetMenuItems();

            return View(this.Views.ViewNames.Index, model);
        }

        /// <summary>
        /// If the passed string is one of the menu item short custs, then redirect to that menu item.
        /// Otherwise, pass the string to Inquiry
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Route(HomeController.ActionNameConstants.Search)]
        public virtual ActionResult Search(string id)
        {
            if (!string.IsNullOrWhiteSpace(id))
            {
                // See whether it is one of the link shortcuts. If it is, then just redirect to that link
                var path = HttpContext.Server.MapPath(MENUITEMS_XML_FILE_NAME);
                XDocument xdoc = XDocument.Load(path);

                var link = (from cat in GetMenuItems()
                            from item in cat.MenuItems
                            where string.Compare((string)item.ShortCut, id, true) == 0
                            select item).FirstOrDefault();

                if (link != null)
                {
                    return RedirectToRoute(link.Url);
                }
            }

            return Redirect(Url.RouteUrl("DcmsConnect_Search", new
            {
                id = id
            }));
            //var x = RouteTable.Routes.GetRouteData(ToArray();
            //throw new NotImplementedException(id);
        }

        /// <summary>
        /// Called from the main site to retrieve the list of programs available on RC
        /// The route to this action is public since it is used when main site contacts the RC site. Therefore the Route attribute should be changed with great caution.
        /// </summary>
        /// <param name="version">This parameter exists to support evlution of this function. The value can be used to determine which format the results should be returned in</param>
        /// <returns></returns>
        /// <remarks>
        /// jsonp data type is used to enable cross domain jquery requests as described in
        /// http://www.pureexample.com/jquery/cross-domain-ajax.html
        /// </remarks>
        [Route("RcItems")]
        public virtual ActionResult RcItems(int version)
        {
            if (version != 1)
            {
                throw new NotSupportedException("Only version 1 is supported");
            }
            var query = from area in AreaItem.Areas
                        select new
                        {
                            area = area.AreaName,
                            url = string.Format("/{0}", area.AreaName)
                        };

            var sb = new StringBuilder(Request["callback"]);
            sb.Append("(");
            var ser = new JavaScriptSerializer();
            ser.Serialize(query, sb);
            sb.Append(")");
            return new ContentResult
            {
                Content = sb.ToString(),
                ContentType = "jsonp"
            };
        }

        private IList<MenuCategory> GetMenuItems()
        {
            // MENUITEMS_XML_FILE_NAME is also used as the key
            var categories = MemoryCache.Default[MENUITEMS_XML_FILE_NAME] as IList<MenuCategory>;
            if (categories == null)
            {
                var path = HttpContext.Server.MapPath(MENUITEMS_XML_FILE_NAME);
                XDocument xdoc = XDocument.Load(path);
                categories = (from cat in xdoc.Root.Element(_ns + "categories").Elements(_ns + "category")
                              let catId = (string)cat.Attribute("id")
                              select new MenuCategory
                              {
                                  Id = catId,
                                  Name = (string)cat.Attribute("name"),
                                  MenuItems = (from item in xdoc.Root.Element(_ns + "items").Elements(_ns + "item")
                                               let itemCatId = (string)item.Attribute("categoryId")
                                               where itemCatId == catId
                                               let itemId = (string)item.Attribute("id")
                                               let route = Url.RouteCollection[itemId]
                                               where route != null  // If the route does not exist, do not show the link
                                               let elemDescription = item.Element(_ns + "description")
                                               select new MenuLink
                                               {
                                                   Id = itemId,
                                                   ShortCut = (string)item.Attribute("shortcut"),
                                                   Name = (string)item.Attribute("name"),
                                                   Description = elemDescription == null ? string.Empty : elemDescription.Value,
                                                   //Mobile = (bool)item.Attribute("mobile"),
                                                   Url = Url.RouteUrl(itemId)
                                               }).ToArray()
                              }).Where(p => p.MenuItems.Count > 0).ToArray();

                CacheItemPolicy policy = new CacheItemPolicy
                {
                    Priority = CacheItemPriority.Default,
                    SlidingExpiration = TimeSpan.FromMinutes(30)
                };

                List<string> filePaths = new List<string>();
                filePaths.Add(path);

                policy.ChangeMonitors.Add(new
                HostFileChangeMonitor(filePaths));

                MemoryCache.Default.Set(MENUITEMS_XML_FILE_NAME, categories, policy);

            }
            return categories;
        }

    }
}


//<!--$Id$-->
