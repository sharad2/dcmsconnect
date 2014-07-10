
using System.Collections.Generic;

namespace DcmsMobile.MainArea.Home
{
    public class MenuLink
    {
        public string RouteName { get; set; }

        public string ShortCut { get; set; }

        public string Name { get; set; }

        public bool Mobile { get; set; }

        public string Url { get; set; }

        public string Description { get; set; }

        public int Order { get; set; }

        public string CategoryId { get; set; }

        public bool Visible { get; set; }
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

        public IList<MenuCategory> Categories { get; internal set; }

    }

}

//<!--$Id$-->
