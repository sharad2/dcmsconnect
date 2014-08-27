
using System.Collections.Generic;

namespace DcmsMobile.MainArea.Home
{
    public class MenuLinkModel
    {
        public string RouteName { get; set; }

        public string ShortCut { get; set; }

        public string Name { get; set; }

        public bool Mobile { get; set; }

        public string Url { get; set; }

        public string Description { get; set; }

        //public int Order { get; set; }

        //public string CategoryId { get; set; }

        public bool Visible { get; set; }
    }



    public class MenuCategoryModel
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public IList<MenuLinkModel> MenuItems { get; set; }
    }

    /// <summary>
    /// View model for the launcher view
    /// </summary>
    public class LauncherViewModel
    {
        /// <summary>
        /// Base URL of the RC site.
        /// </summary>
        public string UrlRcBase
        {
            get;
            internal set;
        }

        public IList<MenuCategoryModel> Categories { get; internal set; }

        public bool IsRcSite { get; set; }

    }

}

//<!--$Id$-->
