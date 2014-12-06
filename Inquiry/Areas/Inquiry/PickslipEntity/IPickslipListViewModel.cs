using System.Collections.Generic;

namespace DcmsMobile.Inquiry.Areas.Inquiry.PickslipEntity
{
    public interface IPickslipListViewModel
    {
        IList<PickslipHeadlineModel> AllPickslips { get; set; }

        //[Obsolete]
        //bool ShowInventoryStatus { get; set; }
        
    }
}