using DcmsMobile.Inquiry.Areas.Inquiry.IntransitEntity;
using System.Web.Mvc;

namespace DcmsMobile.Inquiry.Areas.Inquiry
{
    public class InquiryAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get { return MVC_Inquiry.Inquiry.Name; }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            ModelUnbinderHelpers.ModelUnbinders.Add(new ShipmentListFilterModelUnbinder());
            ModelUnbinderHelpers.ModelUnbinders.Add(new ShipmentSkuFilterModelUnbinder());
        }
    }
}