using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using DcmsMobile.PickWaves.Helpers;
using DcmsMobile.PickWaves.Repository.BoxPickPallet;
using DcmsMobile.PickWaves.ViewModels.BoxPickPallet;
using EclipseLibrary.Mvc.Controllers;

namespace DcmsMobile.PickWaves.Areas.PickWaves.Controllers
{
    [AuthorizeEx("Create pallet requires role {0}", Roles = ROLE_EXPEDITE_BOXES)]
    [RouteArea("PickWaves")]
    [RoutePrefix(BoxPickPalletController.NameConst)]
    public partial class BoxPickPalletController : PickWavesControllerBase
    {

        #region Intialization

        /// <summary>
        /// Required by T4MVC
        /// </summary>
        public BoxPickPalletController()
        {

        }

        private BoxPickPalletService _service;

        protected override void Initialize(RequestContext requestContext)
        {
            base.Initialize(requestContext);
            if (_service == null)
            {
                _service = new BoxPickPalletService(this.HttpContext.Trace,
                    HttpContext.User.IsInRole(ROLE_EXPEDITE_BOXES) ? HttpContext.User.Identity.Name : string.Empty,
                    Request.UserHostName ?? Request.UserHostAddress);
            }
        }

        protected override void Dispose(bool disposing)
        {
            _service.Dispose();
            base.Dispose(disposing);
        }

        #endregion

        private PalletModel Map(Pallet src)
        {
            return new PalletModel
            {
                PalletId = src.PalletId,
                PickedBoxes = src.PickedBoxes,
                TotalBoxesOnPallet = src.TotalBoxesOnPallet,
                PrintDate = src.PrintDate,
                IaChangeDate = src.IaChangeDate
            };
        }

        /// <summary>
        /// called to show pallet list for passed bucket.
        /// </summary>
        /// <param name="bucketId"></param>
        /// <returns></returns>
        [Route(BoxPickPalletController.ActionNameConstants.Index, Name = "DcmsConnect_BoxPickPallet")]
        public virtual ActionResult Index(int? bucketId)
        {
            var model = new BoxPickPalletViewModel();
            //TC1: bucket not passed, get best bucket to expedite.
            if (bucketId == null)
            {
                bucketId = _service.GetBucketToExpedite();
            }
            //TC2: best bucket to expedite not found, Show index page with message.
            if (bucketId == null)
            {
                AddStatusMessage("There are no pick wave to expedite.");
                return View(Views.Index, model);
            }
            var bucket = _service.GetBucketDetail(bucketId.Value);
            //TC3: bucket must be valid, get bucket details.
            if (bucket == null)
            {
                ModelState.AddModelError("BucketId", "Invalid pick wave passed.");
                return View(Views.Index, model);
            }

            model = new BoxPickPalletViewModel
                {
                    BucketId = bucketId.Value,
                    BucketName = bucket.BucketName,
                    CustomerId = bucket.MaxCustomerId,
                    ExpeditedBoxCount = bucket.ExpeditedBoxCount,
                    PalletLimit = bucket.PalletLimit,
                    TotalBoxes = bucket.CountTotalBox,
                    PullBuildingId = bucket.PullBuildingId,
                    PitchBuildingId = bucket.PitchBuildingId,
                    IsFrozen = bucket.IsFrozen,
                    PalletList = _service.GetPalletsOfBucket(bucketId.Value).Select(Map).ToArray()
                };
            model.BucketId = bucketId;
            model.CustomerName = _service.GetCustomer(model.CustomerId).Name;
            return View(Views.Index, model);
        }

        /// <summary>
        /// Expedite the boxes.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public virtual ActionResult CreatePallet(BoxPickPalletViewModel model)
        {
            model.PalletId = model.PalletId.ToUpper();
            //TC4: Bucket must be passed
            if (model.BucketId == null)
            {
                ModelState.AddModelError("", "Wave ID is required.");
                return RedirectToAction(this.Actions.Index(model.BucketId));
            }
            var pallet = _service.GetPallet(model.PalletId);

            //TC5: Pallet is exist but not belongs to any bucket.
            if (pallet != null && pallet.BucketId == null)
            {
                ModelState.AddModelError("pallet.BucketId", string.Format("Pallet {0} does not belong to any pick wave. Please scan another pallet.", pallet.PalletId));
                return RedirectToAction(this.Actions.Index(model.BucketId));
            }

            //TC6: Existing pallet which contains the boxes of passed bucket
            if (pallet != null && pallet.BucketId == model.BucketId && pallet.TotalBoxesOnPallet >= model.PalletLimit)
            {
                ModelState.AddModelError("PalletLimit", string.Format("To add more cartons on Pallet {0}, Please enter pallet limit more than {1}.",
                                             pallet.PalletId, pallet.TotalBoxesOnPallet));
                return RedirectToAction(this.Actions.Index(model.BucketId));
            }

            //TC7: Pallet is exist but bucket is different.
            if (pallet != null && pallet.BucketId != model.BucketId)
            {
                ModelState.AddModelError("pallet.BucketId", string.Format("Pallet {0} belongs to {1} pick wave. Please scan another pallet.",
                                             pallet.PalletId, pallet.BucketId));
                return RedirectToAction(this.Actions.Index(model.BucketId));
            }

            try
            {
                var count = _service.CreatePallet(model.BucketId.Value, model.PalletId, model.PalletLimit);
                AddStatusMessage(string.Format(count > 0 ? "{0} boxes put on the Pallet {1} successfully." : "No new boxes kept on the Pallet {1}. This could be because inventory is not available at the moment.", count, model.PalletId));
            }
            catch (ApplicationException ex)
            {
                ModelState.AddModelError("pallet", ex.Message);
            }

            model.PalletList = _service.GetPalletsOfBucket(model.BucketId.Value).Select(Map).ToArray();
            return RedirectToAction(this.Actions.Index(model.BucketId));
        }

        /// <summary>
        /// Remove unpicked boxes from pallet.
        /// </summary>
        /// <param name="bucketId"></param>
        /// <param name="palletId"></param>
        /// <returns></returns>
        [HttpPost]
        public virtual ActionResult RemoveUnPickedBoxesFromPallet(int? bucketId, string palletId)
        {
            //TC8: Pallet or bucket is not passed
            if (bucketId == null || palletId == null)
            {
                throw new ArgumentNullException("Wave or palletId is null");
            }
            _service.RemoveUnPickedBoxesFromPallet(palletId);
            this.AddStatusMessage(string.Format("Unpicked boxes removed from Pallet {0}", palletId));
            return RedirectToAction(this.Actions.Index(bucketId));
        }

        protected override string ManagerRoleName
        {
            get { return ROLE_EXPEDITE_BOXES; }
        }
    }
}
