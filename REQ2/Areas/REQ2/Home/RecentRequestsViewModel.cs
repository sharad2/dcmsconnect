using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace DcmsMobile.REQ2.Areas.REQ2.Home
{
    public class RecentRequestsViewModel
    {

        private IEnumerable<RequestViewModel> _recentRequests;

        /// <summary>
        /// We never return null.
        /// </summary>
        public IEnumerable<RequestViewModel> RecentRequests
        {
            get
            {
                return _recentRequests ?? Enumerable.Empty<RequestViewModel>();
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