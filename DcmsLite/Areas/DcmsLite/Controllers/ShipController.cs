using DcmsMobile.DcmsLite.ViewModels.Ship;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Linq;
using System;

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
                this.AddStatusMessage("Please enter customer");
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
                                PoId = item.PoId,
                                Iteration = item.Iteration
                            }).ToArray();

            return View(Views.Index, model);
        }

        public virtual ActionResult CreateBol(IndexViewModel model)
        {
            var tuples = from key in model.SelectedKeys
                         let upm = new PoModel
                         {
                             Key = key
                         }
                         select new
                         {
                             customerId = upm.CustomerId,
                             PoKey = Tuple.Create(upm.PoId, upm.Iteration, upm.CustomerDcId)
                         };
            var pokeys = tuples.Select(p => p.PoKey).ToArray();
            var postedCustomerId = tuples.Select(p => p.customerId).First();
            var count = _service.CreateBol(postedCustomerId, pokeys);
            AddStatusMessage(string.Format("{0} BOL created.", count));
            return RedirectToAction(this.Actions.SearchCustomer(postedCustomerId));
        }
    }
}
