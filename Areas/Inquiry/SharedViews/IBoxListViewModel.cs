using DcmsMobile.Inquiry.Areas.Inquiry.PickslipEntity;
using System.Collections.Generic;

namespace DcmsMobile.Inquiry.Areas.Inquiry.SharedViews
{
    public interface IBoxListViewModel
    {
        IList<BoxHeadlineModel> AllBoxes { get;}

        /// <summary>
        /// Whether pickslip and pick wave links should be displayed
        /// </summary>
        bool ShowPickslipLinks { get; }
    }
}