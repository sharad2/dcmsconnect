using System.Collections.Generic;
using System.Web.Mvc;

namespace DcmsMobile.Models
{
    /// <summary>
    /// Includes additional property ChangeLog
    /// </summary>
    public class RcMenuItem: MenuItem
    {

        public string ChangeLog { get; set; }

        /// <summary>
        /// Populates its properties based on the passed area
        /// </summary>
        /// <param name="area"></param>
        /// <param name="url"></param>
        internal RcMenuItem(AreaItem area, UrlHelper url): base(area, url)
        {
            ChangeLog = area.ChangeLog;
        }
    }

    public class RcLauncherViewModel : ViewModelBase
    {
        public IEnumerable<RcMenuItem> MenuItems { get; set; }

        /// <summary>
        /// Url of the main site
        /// </summary>
        public string MainSiteUrl { get; set; }
    }
}
