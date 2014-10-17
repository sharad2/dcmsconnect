
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
        /// <summary>
        /// Without the trailing /. Public because it is also accessed by layout view
        /// </summary>
        public static string UrlRcBase
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
        /// This is the action for the home page and is the default route. ~/ Overrides [RoutePrefix("home")] set on the controller
        /// </summary>
        /// <returns></returns>
        [Route("~/")]
        public virtual ActionResult Index()
        {


            // If we are the RC site, then our RC URL is null. This turns off all RC handling in the view.
            var isRc = HttpContext.Request.Url.AbsoluteUri.TrimEnd('/').StartsWith(UrlRcBase, StringComparison.InvariantCultureIgnoreCase);
            if (isRc)
            {
                HttpContext.Trace.Write("RC", string.Format("This is an RC site. All RC links will be invisible", isRc));
            }
            else
            {
                HttpContext.Trace.Write("RC", string.Format("This is a production site. RC links will be invisible", isRc));
            }


            var model = new LauncherViewModel
            {
                UrlRcBase = UrlRcBase,
                //Categories = query.Where(p => p.MenuItems.Count > 0).ToArray(),
                Categories = CreateMenuCategories().ToList(),
                IsRcSite = !isRc
            };

            return View(this.Views.ViewNames.Index, model);
        }

        private IEnumerable<MenuCategoryModel> CreateMenuCategories()
        {
            var query = from cat in this.MenuCategories
                        orderby cat.Sequence ?? 10000
                        select new MenuCategoryModel
                        {
                            Id = cat.Id,
                            Name = cat.Name,
                            Description = cat.Description,
                            MenuItems = (from link in this.MenuLinks
                                         where link.CategoryId == cat.Id && (!link.Visible.HasValue || link.Visible.Value)
                                         let route = Url.RouteCollection[link.RouteName]
                                         where route != null
                                         orderby link.Rating ?? 0 descending, link.Sequence ?? 10000, link.ShortCut
                                         select new MenuLinkModel
                                         {
                                             Description = link.Description,
                                             Mobile = link.Mobile ?? false,
                                             Name = link.Name,
                                             RouteName = link.RouteName,
                                             ShortCut = link.ShortCut,
                                             Url = Url.RouteUrl(link.RouteName)
                                         }).ToArray()
                        };

            return query.Where(p => p.MenuItems.Count > 0);
        }

        /// <summary>
        /// If the passed string is one of the menu item short custs, then redirect to that menu item.
        /// Otherwise, pass the string to Inquiry
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Route("search", Name = "DcmsMobile_Search")]
        public virtual ActionResult Search(string id)
        {
            if (!string.IsNullOrWhiteSpace(id))
            {
                // See whether it is one of the link shortcuts. If it is, then just redirect to that link
                var link = (from item in this.MenuLinks
                            where string.Compare((string)item.ShortCut, id, true) == 0
                            let route = Url.RouteCollection[item.RouteName]
                            where route != null
                            select Url.RouteUrl(item.RouteName)).FirstOrDefault();

                if (link != null)
                {
                    return Redirect(link);
                }
            }

            return Redirect(Url.RouteUrl(DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_Search1, new
            {
                id = id
            }));
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

            var query = CreateMenuCategories().SelectMany(p => p.MenuItems).Select(p => new
            {
                route = p.RouteName,
                url = Url.RouteUrl(p.RouteName)
            });

            var sb = new StringBuilder(Request["callback"]);
            sb.Append("(");
            var ser = new JavaScriptSerializer();
            ser.Serialize(query, sb);
            sb.Append(")");
            HttpContext.Trace.Write("RcItems", sb.ToString());
            return new ContentResult
            {
                Content = sb.ToString(),
                ContentType = "jsonp"
            };

        }

        #region XML File
        private const string CACHE_XML_KEY = "App_Data/MenuItems.xml";
        private IList<MenuCategory> MenuCategories
        {
            get
            {
                var tuple = MemoryCache.Default[CACHE_XML_KEY] as Tuple<IList<MenuCategory>, IList<MenuLink>>;
                if (tuple == null)
                {
                    tuple = ReadMenuXml();
                }
                return tuple.Item1;
            }
        }

        private IList<MenuLink> MenuLinks
        {
            get
            {
                var tuple = MemoryCache.Default[CACHE_XML_KEY] as Tuple<IList<MenuCategory>, IList<MenuLink>>;
                if (tuple == null)
                {
                    tuple = ReadMenuXml();
                }
                return tuple.Item2;
            }
        }

        private Tuple<IList<MenuCategory>, IList<MenuLink>> ReadMenuXml()
        {
            XNamespace _ns = "http://schemas.eclsys.com/dcmsconnect/menuitems";

            //var path = HttpContext.Server.MapPath(MENUITEMS_XML_FILE_NAME);

            var root = XElement.Parse(DcmsLibrary.Mvc.Resources.MenuItems);

            IList<MenuCategory> cats = (from cat in root.Element(_ns + "processes").Elements(_ns + "process")
                                        select new MenuCategory
                                        {
                                            Id = (string)cat.Attribute("id"),
                                            Name = (string)cat.Attribute("name"),
                                            Description = (string)cat.Element(_ns + "description"),
                                            Sequence = (int?)cat.Element(_ns + "sequence")
                                        }).ToArray();

            IList<MenuLink> links = (from item in root.Element(_ns + "items").Elements(_ns + "item")
                                     select new MenuLink
                                     {
                                         RouteName = (string)item.Attribute("route"),
                                         ShortCut = (string)item.Attribute("shortcut"),
                                         Name = (string)item.Attribute("name"),
                                         Description = (string)item.Element(_ns + "description"),
                                         Mobile = (bool?)item.Attribute("mobile"),
                                         Visible = (bool?)item.Attribute("visible"),
                                         Rating = (int?)item.Attribute("rating"),
                                         CategoryId = (string)item.Attribute("processidref"),
                                         Sequence = (int?)item.Attribute("sequence")
                                     }).ToArray();

            CacheItemPolicy policy = new CacheItemPolicy
            {
                Priority = CacheItemPriority.Default,
                SlidingExpiration = TimeSpan.FromMinutes(30)
            };

            //List<string> filePaths = new List<string>();
            //filePaths.Add(path);

            //policy.ChangeMonitors.Add(new HostFileChangeMonitor(filePaths));

            var tuple = Tuple.Create(cats, links);
            MemoryCache.Default.Set(CACHE_XML_KEY, tuple, policy);

            return tuple;
        }


        #endregion
    }
}


//<!--$Id$-->
