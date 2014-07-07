
using EclipseLibrary.Mvc.Controllers;
using System;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace DcmsMobile.MainArea.Home
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
        [Route()]
        public virtual ActionResult Index()
        {
            var model = new LauncherViewModel();
            //model.Init(this.ControllerContext, this.Url);
            model.MenuItems = (from item in AreaItem.Areas
                               orderby item.Order, item.Name
                               select new MenuItem(item, Url)).ToArray();
            model.UrlRcBase = this.UrlRcBase;

            return View(this.Views.ViewNames.Index, model);
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

    }
}


//<!--$Id$-->
