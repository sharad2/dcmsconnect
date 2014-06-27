using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

namespace DcmsMobile.Models
{

    public class MenuItem
    {
        private readonly IList<MenuItem> _submenu;
        private readonly string _panelId;

        //private static int __idSequence;

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
            ShortName = area.ShortName ?? "";
            IsMobileOptimized = area.IsMobileOptimized;
            IsDesktopOptimized = area.IsDesktopOptimized;
            Description = area.Description;
            if (area.SubAreas != null)
            {
                _submenu = (from subarea in area.SubAreas
                           orderby subarea.Order, subarea.Name
                           select new MenuItem(subarea, url)).ToArray();
            }
            _panelId = string.Format("panel_{0}", area.UniqueId);
        }

        public IList<MenuItem> SubMenu
        {
            get
            {
                return _submenu;
            }
        }

        public string Url { get; private set; }

        public string Name { get; private set; }

        /// <summary>
        /// Guaranteed to never return null.
        /// </summary>
        [DisplayFormat(DataFormatString = "[{0}]")]
        public string ShortName
        {
            get;
            private set;
        }

        public bool IsMobileOptimized { get; private set; }

        public string Description { get; private set; }

        public int Order { get; private set; }

        public bool IsDesktopOptimized { get; private set; }

        public string PanelId
        {
            get
            {
                return _panelId;
            }
        }
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
