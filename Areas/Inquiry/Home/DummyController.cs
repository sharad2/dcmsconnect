using System.Web.Mvc;


// This code exists so that the Inquiry Home Page shows up when the Inquiry project is being run independent of DcmsMobile.
namespace DcmsMobile.Inquiry.Helpers
{
    public partial class DummyController : Controller
    {
        // Commented the Route Attribute because it gives error "The request has found the following matching controller types"
        // when Inquiry is run from DcmsMobile
#if DEBUG
        //[Route(Order=10000 )]
#endif
        public virtual ActionResult Index()
        {
            return RedirectToAction(MVC_Inquiry.Inquiry.Home.Index());
        }
    }
}

