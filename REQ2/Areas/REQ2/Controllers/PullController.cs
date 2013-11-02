using System;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using DcmsMobile.REQ2.Repository.Pull;
using DcmsMobile.REQ2.ViewModels.Pull;
using EclipseLibrary.Mvc.Controllers;

namespace DcmsMobile.REQ2.Areas.REQ2.Controllers
{
    public partial class PullController : EclipseController
    {

        #region Initialize

        /// <summary>
        /// Required by T4MVC
        /// </summary>
        public PullController()
        {

        }

        private PullService _service;

        public PullService Service
        {
            get { return _service; }

            set { _service = value; }
        }

        protected override void Initialize(RequestContext requestContext)
        {
            base.Initialize(requestContext);

            if (_service == null)
            {
                var connectString = ConfigurationManager.ConnectionStrings["dcms8"].ConnectionString;
                var userName = requestContext.HttpContext.SkipAuthorization ? string.Empty : requestContext.HttpContext.User.Identity.Name;
                var clientInfo = string.IsNullOrEmpty(requestContext.HttpContext.Request.UserHostName) ? requestContext.HttpContext.Request.UserHostAddress :
                    requestContext.HttpContext.Request.UserHostName;
                _service = new PullService(requestContext.HttpContext.Trace, connectString, userName, clientInfo, "Pull");
            }
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

        #region Index
        /// <summary>
        /// Shows the pull area choices to the user.
        /// </summary>
        /// <returns></returns>
        public virtual ActionResult Index()
        {
            var areaList = _service.GetAreaSuggestions();

            var model = new IndexViewModel
            {
                AreaList = areaList.Select(p => new AreaModel
                {
                    SourceBuildingId = p.SourceBuildingId,
                    DestinationBuildingId = p.DestinationBuildingId,
                    SourceAreaId = p.SourceAreaId,
                    DestinationAreaId = p.DestinationAreaId,
                    TopRequestId = p.TopRequestId,
                    SourceShortName = p.SourceAreaShortName,
                    DestinationShortName = p.DestinationAreaShortName,
                    PullableCartonCount = p.PullableCartonCount

                }).ToArray()
            };

            if (areaList.Count() < 1)
            {
                // Nothing to pull
                AddStatusMessage("Nothing to PULL");

            }
            return View(Views.Index, model);
        }


        /// <summary>
        /// The list of sorurce and destinattion areaid are posted back
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public virtual ActionResult IndexPost(IndexViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(Actions.Index());
            }

            string reqId;
            if (model.Choice <= model.AreaList.Count)
            {
                // The user choce source/Dest. Find the best req id
                reqId = _service.GetBestRequest(model.AreaList[model.Choice.Value - 1].SourceAreaId, model.AreaList[model.Choice.Value - 1].DestinationAreaId);
            }
            else
            {
                // Assume user meant to enter reqId
                reqId = model.Choice.Value.ToString();
            }
            return RedirectToAction(Actions.Pallet(reqId));
        }
        #endregion

        #region Pallet
        /// <summary>
        /// TODO: Show some information of passed area
        /// </summary>
        /// <param name="choice"></param>
        /// <param name="destAreaId"></param>
        /// <returns></returns>
        [HttpGet]
        public virtual ActionResult Pallet(string resvId)
        {
            var request = _service.GetRequest(resvId);
            if (request == null)
            {
                ModelState.AddModelError("", string.Format("Invalid request id {0}", resvId));
                return RedirectToAction(Actions.Index());
            }

            var model = new PalletViewModel
            {
                RequestId = resvId,
                SourceShortName = request.SourceShortName,
                DestShortName = request.DestShortName,
                Requestedby = request.RequestedBy

            };


            return View(Views.Pallet, model);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpGet]
        public virtual ActionResult PalletPost(PalletViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.PalletId))
            {
                // User wants to Go back
                return RedirectToAction(this.Actions.Index());
            }

            if (string.IsNullOrWhiteSpace(model.RequestId))
            {
                // Ravneet promises this will never happen
                ModelState.AddModelError("", "Please make your selection again.");
                return RedirectToAction(this.Actions.Index());
            }

            // We have both pallet and request id
            if (!ModelState.IsValid)
            {
                // This should never happen because model has no validations. Ask for pallet again
                return RedirectToAction(this.Actions.Pallet(model.RequestId));
            }

            // Normal case. Call the Carton action directly to avoid PRG roundtrip.
            return Carton(model.PalletId, model.RequestId);
        }
        #endregion

        #region Carton

        /// <summary>
        /// Now we suggest carton for pulling for passed pallet and area
        /// </summary>
        /// <param name="areaId"></param>
        /// <param name="palletId"></param>
        /// <returns></returns>
        /// TODO: If no suggestions can be made then display appropiate message.
        [HttpGet]
        public virtual ActionResult Carton(string palletId, string requestId)
        {
            var suggestedCarton = _service.GetCartonSuggestions(palletId, requestId);
            if (suggestedCarton.Count() < 1)
            {
                // Nothing to pull
                AddStatusMessage(string.Format("All cartons have been pulled for pallet {0}.", palletId)); 
                return RedirectToAction(Actions.Index());
            }

            var request = _service.GetRequest(requestId);
            if (request == null)
            {
                ModelState.AddModelError("", string.Format("Invalid request id {0}", requestId));
                return RedirectToAction(Actions.Index());
            }

            var model = new CartonViewModel
            {
                PalletId = palletId,
                RequestId = requestId,
                SourceShortName = request.SourceShortName,
                DestShortName = request.DestShortName,
                CartonList = suggestedCarton.Select(p => new CartonModel
                {
                    CartonId = p.CartonId,
                    LocationId = p.LocationId,
                    SkuInCarton = new ViewModels.SkuModel
                    {
                        SkuId = p.SkuInCarton.SkuId,
                        Style = p.SkuInCarton.Style,
                        Color = p.SkuInCarton.Color,
                        Dimension = p.SkuInCarton.Dimension,
                        SkuSize = p.SkuInCarton.SkuSize,
                        UpcCode = p.SkuInCarton.UpcCode
                    }



                }).ToArray()
            };
            return View(Views.Carton, model);
        }



        /// <summary>
        /// Pulls carton and redirects to Carton Action
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public virtual ActionResult PostCarton(CartonViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Pallet or request missing? Should never happen
                return RedirectToAction(this.Actions.Index());
            }

            if (string.IsNullOrWhiteSpace(model.CartonId))
            {
                // User wants to go back
                return RedirectToAction(this.Actions.Pallet(model.RequestId));
            }
            try
            {
                _service.PullCarton(model.PalletId, model.CartonId, model.RequestId);
                AddStatusMessage(string.Format("Carton {0} is now on Pallet {1}", model.CartonId, model.PalletId));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("",ex.InnerException);
            }

            return RedirectToAction(Actions.Carton(model.PalletId, model.RequestId));
        }
        #endregion
    }
}
