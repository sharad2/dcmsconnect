using DcmsMobile.Inquiry.Areas.Inquiry.SharedViews;
using System.Collections.Generic;

namespace DcmsMobile.Inquiry.Areas.Inquiry.BoxEntity
{
    public class BoxListViewModel : IBoxListViewModel
    {
        public IList<BoxHeadlineModel> AllBoxes { get; set; }

        public bool ShowPickslipLinks
        {
            get { return true; }
        }
    }
}