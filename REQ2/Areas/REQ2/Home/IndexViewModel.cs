using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace DcmsMobile.REQ2.Areas.REQ2.Home
{
    public class IndexViewModel
    {

        private IList<RequestViewModel> _recentRequests;

        /// <summary>
        /// We never return null.
        /// </summary>
        public IList<RequestViewModel> RecentRequests
        {
            get
            {
                return _recentRequests ?? Enumerable.Empty<RequestViewModel>().ToList();
            }
            set
            {
                _recentRequests = value;
            }
        }

        [Required]
        [Display(Name = "Existing Request ID")]
        public string CtnresvId { get; set; }
    }
}
//$Id$