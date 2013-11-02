using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DcmsMobile.REQ2.Repository.Pull
{
    public class PullRequest
    {
        public string SourceShortName { get; set; }
        public string DestShortName { get; set; }
        public string RequestedBy { get; set; }
    }
}