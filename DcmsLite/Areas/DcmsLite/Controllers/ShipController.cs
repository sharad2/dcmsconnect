using DcmsMobile.DcmsLite.ViewModels.Ship;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Linq;

namespace DcmsMobile.DcmsLite.Areas.DcmsLite.Controllers
{
    public partial class ShipController : DcmsLiteControllerBase<ShipService>
    {

        public virtual ActionResult Index()
        {
            var model = new IndexViewModel();
            return View(Views.Index, model);
        }

        public virtual ActionResult SearchCustomer(string customerId)
        {
            var buildingId = _buildingId;
            var model = new IndexViewModel();
            if (string.IsNullOrWhiteSpace(customerId))
            {
                this.AddStatusMessage("Please scan customer");
                return RedirectToAction(this.Actions.Index());
            }
            var poList = _service.GetPoList(customerId);
            if (poList.Count == 0)
            {
                ModelState.AddModelError("", string.Format("Invalid customer {0} ", customerId));
                return RedirectToAction(this.Actions.Index());
            }
            model.PoList = (from item in poList
                            select new PoModel
                            {
                                BucketId = item.BucketId,
                                BuildingId = item.BuildingId,
                                CustomerDcId = item.CustomerDcId,
                                CustomerId = item.CustomerId,
                                CustomerName = item.CustomerName,
                                MinDcCancelDate = item.MinDcCancelDate,
                                NumberOfBoxes = item.NumberOfBoxes,
                                PickedPieces = item.PickedPieces ?? 0,
                                PiecesOrdered = item.PiecesOrdered ?? 0,
                                StartDate = item.StartDate,
                                PoId = item.PoId
                            }).ToArray();
            return View(Views.Index, model);
        }
    }
}
