using DcmsMobile.DcmsLite.Repository.Receive;
using DcmsMobile.DcmsLite.ViewModels.Receive;
using System;
using System.Linq;
using System.Web.Mvc;
using EclipseLibrary.Mvc.Controllers;

namespace DcmsMobile.DcmsLite.Areas.DcmsLite.Controllers
{
    public partial class ReceiveController : DcmsLiteControllerBase<ReceiveService>
    {
        // GET: /DcmsLite/Receive/

        public virtual ActionResult Index()
        {
            var buildingId = _buildingId;

            var model = new IndexViewModel
            {
                BuildingId = buildingId,
                AsnList = (from item in _service.GetAsnListToReceive(buildingId)
                           select new AsnModel
                           {
                               IntransitId = item.IntransitId,
                               ShipmentId = item.ShipmentId,
                               VwhId = item.VwhId,
                               Pieces = item.Pieces,
                               CartonCount = item.CartonCount,
                               ReceivedDate = item.ReceivedDate
                           }).ToArray()
            };
            try
            {
                model.RecevingAreaId = _service.GetRestockAreaList(buildingId).Single().AreaId;
            }
            catch (InvalidOperationException)
            {
                ModelState.AddModelError("", "DCMS Lite requires that the building contains exactly one pick area. Contact Technical Support.");
            }
            return View(Views.Index, model);
        }

        [HttpPost]
        [AuthorizeEx("Receive ASN requires role {0}", Roles = ROLE_DCMS_LITE)]
        public virtual ActionResult ReceiveCartonsOfAsn(IndexViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(Actions.Index());
            }
            try
            {
                var receivedCartons = _service.ReceiveCartonsOfAsn(model.IntransitId, model.ShipmentId, model.RecevingAreaId);
                AddStatusMessage(string.Format("Received {0} cartons of ASN : {1}.", receivedCartons, model.ShipmentId));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("ReceiveCartonsOfAsn", ex.InnerException.Message);
            }
            return RedirectToAction(Actions.Index());
        }

    }
}
