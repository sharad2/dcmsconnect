using DcmsMobile.DcmsLite.ViewModels.Ship;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Linq;

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
            //var list = _service.GetPoList();
            var model = new IndexViewModel
            {
                PoList = (from item in _service.GetPoList()
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
                          }).ToArray()
            };

            return View(model);

        }

    }
}
