using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

namespace DcmsMobile.Models
{

    public class MenuItem
    {
        private readonly IList<MenuItem> _submenu;

        /// <summary>
        /// Populates its properties based on the passed area
        /// </summary>
        /// <param name="area"></param>
        /// <param name="url"></param>
        internal MenuItem(AreaItem area, UrlHelper url)
        {
            //this.Url = url.Content(string.Format("~/{0}", area.AreaName));
            this.Url = url.RouteUrl(area.RouteValues);
            Name = area.Name;
            ShortName = area.ShortName;
            IsMobileOptimized = area.IsMobileOptimized;
            IsDesktopOptimized = area.IsDesktopOptimized;
            Description = area.Description;
            if (area.SubAreas != null)
            {
                _submenu = (from subarea in area.SubAreas
                           orderby subarea.Order, subarea.Name
                           select new MenuItem(subarea, url)).ToArray();
            }
        }

        public IList<MenuItem> SubMenu
        {
            get
            {
                return _submenu;
            }
        }

        public string Url { get; set; }

        public string Name { get; set; }

        string _shortName = string.Empty;

        /// <summary>
        /// Guaranteed to never return null.
        /// </summary>
        [DisplayFormat(DataFormatString="[{0}]")]
        public string ShortName
        {
            get
            {
                return _shortName;
            }
            set
            {
                _shortName = (value ?? string.Empty).Trim();
            }
        }

        public bool IsMobileOptimized { get; set; }

        public string Description { get; set; }

        public int Order { get; set; }

        public bool IsDesktopOptimized { get; set; }
    }

    /// <summary>
    /// View model for the launcher view
    /// </summary>
    public class LauncherViewModel : ViewModelBase
    {
        public string UrlRc
        {
            get;
            set;
        }

        /// <summary>
        /// This list is ordered for display
        /// </summary>
        public IList<MenuItem> MenuItems { get; set; }

    }

}

//<!--$Id$-->
