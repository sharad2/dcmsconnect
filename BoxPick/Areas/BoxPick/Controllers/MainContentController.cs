using System.Web.Mvc;
using System.Web.Routing;
using DcmsMobile.BoxPick.Repositories;
using DcmsMobile.BoxPick.ViewModels.MainContent;
using EclipseLibrary.Mvc.Controllers;
using System.Linq;

namespace DcmsMobile.BoxPick.Areas.BoxPick.Controllers
{
    [Route("main/{action}")]
    public partial class MainContentController : BoxPickControllerBase
    {

        #region Intialization

        private MainContentService _service;

        public MainContentController()
        {

        }

        protected override void Initialize(RequestContext requestContext)
        {
            if (_service == null)
            {
                _service = new MainContentService(requestContext);
            }
            base.Initialize(requestContext);
        }

        protected override void Dispose(bool disposing)
        {
            if (_service != null)
            {
                _service.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion


        /// <summary>
        /// This function invokes when we Scan Building from desktop view 
        /// </summary>
        /// <returns></returns>
        //[Route("Sharad")]
        public virtual ActionResult Building()
        {
            //var model = new BuildingViewModel { PendingActivities = Mapper.Map<IEnumerable<ActivityModel>>(_service.GetPendingActivity()) };
            var model = new BuildingViewModel
            {
                PendingActivities = _service.GetPendingActivity().Select(p => new ActivityModel
                {
                    BuildingId = p.BuildingId,
                    CountBoxes = p.CountBoxes,
                    CountPallets = p.CountPallets,
                    DestinationArea = p.DestinationArea,
                    AreaShortName = p.AreaShortName,
                    PickableBoxCount = p.PickableBoxCount,
                    PickModeText = p.PickModeText
                }).ToList()
            };
            return PartialView(Views._buildingPartial, model);
        }

        /// <summary>
        /// This function invokes when we Scan Pallet from desktop view 
        /// </summary>
        /// <returns></returns>
        public virtual ActionResult Carton(string palletId)
        {
            var boxes = _service.GetBoxesOnPallet(palletId);
            var model = new CartonViewModel
            {
                BoxesOnPallet = boxes == null ? null : boxes.Select(p => new BoxModel
                {
                    AssociatedCartonId = p.AssociatedCarton.CartonId == null ? null : p.AssociatedCarton.CartonId,
                    CartonLocationId = p.AssociatedCarton.LocationId == null ? null : p.AssociatedCarton.LocationId,
                    IaId = p.IaId,
                    Pieces = p.Pieces,
                    QualityCode = p.QualityCode == null ? null : p.QualityCode,
                    SkuInBox = p.SkuInBox.DisplayName == null ? null : p.SkuInBox.DisplayName,
                    SkuInCarton = p.AssociatedCarton.SkuInCarton == null ? null : p.AssociatedCarton.SkuInCarton.DisplayName,
                    UccId = p.UccId,
                    VwhId = p.VwhId
                }).ToArray()
            };
            return PartialView(Views._cartonPartial, model);
        }
    }
}

/*
    $Id$ 
    $Revision$
    $URL$
    $Header$
    $Author$
    $Date$
*/