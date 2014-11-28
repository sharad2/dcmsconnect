using System.Collections.Generic;

namespace DcmsMobile.Inquiry.Areas.Inquiry.PickslipEntity
{
    public class PickslipListViewModel : IPickslipListViewModel
    {
        public IList<PickslipHeadlineModel> AllPickslips { get; set; }
        
    }
}