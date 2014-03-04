using DcmsMobile.DcmsLite.ViewModels.Ship;
using System.Web.Mvc;

namespace DcmsMobile.DcmsLite.Areas.DcmsLite.Controllers
{
    public partial class ShipController : DcmsLiteControllerBase<ShipService>
    {
        //
        // GET: /DcmsLite/Ship/

        public virtual ActionResult Index()
        {
            var buildingId = _buildingId;
            //TODO: Show a list of POs. 

            _service.GetPoList();
            var model = new IndexViewModel();
            return View(model);

        }

    }
}
