using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DcmsMobile.MainArea.Home
{
    public class MenuLink
    {
        public string RouteName { get; set; }

        public string ShortCut { get; set; }

        public string Name { get; set; }

        public bool? Mobile { get; set; }

        public string Description { get; set; }

        /// <summary>
        /// Higher rating means that the link will show up first within results
        /// </summary>
        public int? Rating { get; set; }

        public string CategoryId { get; set; }

        public bool? Visible { get; set; }

        /// <summary>
        /// During normal operations, what is the sequence of this activity
        /// </summary>
        public int? Sequence { get; set; }
    }
}
