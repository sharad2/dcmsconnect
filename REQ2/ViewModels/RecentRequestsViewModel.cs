using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace DcmsMobile.REQ2.ViewModels
{
    public class RecentRequestsViewModel
    {

        private IList<RequestModel> _recentRequests;

        /// <summary>
        /// We never return null.
        /// </summary>
        public IList<RequestModel> RecentRequests
        {
            get
            {
                return _recentRequests ?? new RequestModel[0];
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