using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DcmsMobile.REQ2.Areas.REQ2.Home
{
    public class PullRequestSkuModel
    {
        public PullRequestSkuModel()
        {

        }

        internal PullRequestSkuModel(RequestSku entity)
        {

        }

    }

    public class PullRequestViewModel
    {
        public PullRequestViewModel()
        {

        }

        internal PullRequestViewModel(PullRequest entity)
        {

        }

        public IList<PullRequestSkuModel> SkuList { get; set; }
    }
}