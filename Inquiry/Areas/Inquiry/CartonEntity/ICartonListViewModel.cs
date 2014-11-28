using System.Collections.Generic;
namespace DcmsMobile.Inquiry.Areas.Inquiry.CartonEntity
{
    public interface ICartonListViewModel
    {
        IList<CartonHeadlineModel> AllCartons { get; set; }
    }
}