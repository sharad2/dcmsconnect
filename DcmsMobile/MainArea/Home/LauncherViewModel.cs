
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

namespace DcmsMobile.MainArea.Home
{
    //[Obsolete("Superseded by MenuLink")]
    //public class MenuItem
    //{
    //    private readonly IList<MenuItem> _submenu;
    //    private readonly string _panelId;

    //    internal MenuItem()
    //    {

    //    }

    //    /// <summary>
    //    /// Populates its properties based on the passed area
    //    /// </summary>
    //    /// <param name="area"></param>
    //    /// <param name="url"></param>
    //    internal MenuItem(AreaItem area, UrlHelper url)
    //    {
    //        if (area.RouteValues == null)
    //        {
    //            this.Url = url.Content(string.Format("~/{0}", area.AreaName));
    //        }
    //        else
    //        {
    //            this.Url = url.RouteUrl(area.RouteValues);
    //        }
    //        Name = area.Name;
    //        ShortName = area.ShortName ?? "";
    //        IsMobileOptimized = area.IsMobileOptimized;
    //        IsDesktopOptimized = area.IsDesktopOptimized;
    //        Description = area.Description;
    //        if (area.SubAreas != null)
    //        {
    //            _submenu = (from subarea in area.SubAreas
    //                        orderby subarea.Order, subarea.Name
    //                        select new MenuItem(subarea, url)).ToArray();
    //        }
    //        _panelId = string.Format("panel_{0}", area.AreaName);
    //        this.AreaName = area.AreaName;
    //    }

    //    public IList<MenuItem> SubMenu
    //    {
    //        get
    //        {
    //            return _submenu;
    //        }
    //    }

    //    public string Url { get; set; }

    //    public string Name { get; private set; }

    //    /// <summary>
    //    /// Guaranteed to never return null.
    //    /// </summary>
    //    [DisplayFormat(DataFormatString = "{0}")]
    //    public string ShortName
    //    {
    //        get;
    //        set;
    //    }

    //    public bool IsMobileOptimized { get; set; }

    //    public string Description { get; private set; }

    //    public int Order { get; private set; }

    //    public bool IsDesktopOptimized { get; private set; }

    //    public string PanelId
    //    {
    //        get
    //        {
    //            return _panelId;
    //        }
    //    }

    //    /// <summary>
    //    /// The area to which this menu item belongs
    //    /// </summary>
    //    public string AreaName
    //    {
    //        get;
    //        private set;
    //    }

    //}

    public class MenuLink
    {

        public string Id { get; set; }

        public string ShortCut { get; set; }

        public string Name { get; set; }

        public bool Mobile { get; set; }

        public string Url { get; set; }

        public string Description { get; set; }
    }



    public class MenuCategory
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public IList<MenuLink> MenuItems { get; set; }
    }

    /// <summary>
    /// View model for the launcher view
    /// </summary>
    public class LauncherViewModel
    {
        /// <summary>
        /// Base URL of the RC site
        /// </summary>
        public string UrlRcBase
        {
            get;
            internal set;
        }

        ///// <summary>
        ///// This list is ordered for display
        ///// </summary>
        //[Obsolete("Use Categories instead")]
        //public IList<MenuItem> MenuItems { get; set; }

        public IList<MenuCategory> Categories { get; internal set; }

        //[Obsolete]
        //public string UrlOf(UrlHelper helper, string routeName)
        //{
        //    var route = helper.RouteCollection[routeName];
        //    if (route == null)
        //    {
        //        return null;
        //    }

        //    return helper.RouteUrl(routeName);
        //}

    }

}

//<!--$Id$-->
