using System;
using System.Configuration;
using System.Configuration.Provider;
using System.Data.Common;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using DcmsMobile.REQ2.Models;
using DcmsMobile.REQ2.Repository;
using DcmsMobile.REQ2.ViewModels;
using EclipseLibrary.Mvc.Controllers;

namespace DcmsMobile.REQ2.Areas.REQ2.Controllers
{
    [AuthorizeEx("REQ3 requires Role {0}", Roles = "DCMS8_REQUEST")]
    public partial class ReqController : EclipseController
    {
        #region Initialize

        /// <summary>
        /// Required by T4MVC
        /// </summary>
        public ReqController()
        {

        }

        private ReqService _service;

        public ReqService Service
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
                _service = new ReqService(requestContext.HttpContext.Trace, connectString, userName, clientInfo, "REQ3");
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


        private SelectListItem MapCode(CodeDescriptionModel entity)
        {
            return new SelectListItem
            {
                Value = entity.Code,
                Text = entity.Code + " : " + entity.Description
            };
        }

        /// <summary>
        /// This is the GET Method for Create new Request. If resvId is passed, all values default to the properties of the passed resvid. This will useful when user clicked Create Like link.
        /// </summary>
        /// <returns></returns>
        public virtual ActionResult CreateRequest(string resvId)
        {
            var model = new CreateRequestViewModel();
            if (!string.IsNullOrEmpty(resvId))
            {
                //TC2: Ypu will get here when you clicked "Create Like:" link.
                model.CurrentRequest = new EditRequestHeaderModel(_service.GetRequestInfo(resvId));
                model.CurrentRequest.ResvId = null;
            }
            //TC1: If user wants to create a new request.
            PopulateIndexViewModel(model);
            return View(Views.CreateRequest, model);
        }

        /// <summary>
        /// Returns create request view to edit with posted values.
        /// </summary>
        /// <param name="resvId"></param>
        /// <returns></returns>
        public virtual ActionResult EditRequest(string resvId)
        {
            if (string.IsNullOrWhiteSpace(resvId))
            {
                //TC3: You get here if resvId is null. Which is not possible in normal situation.
                return RedirectToAction(Actions.DisplayRecentRequest());
            }
            var request = _service.GetRequestInfo(resvId);
            var model = new CreateRequestViewModel
                {
                    CurrentRequest = new EditRequestHeaderModel(request)
                };
            PopulateIndexViewModel(model);
            return View(Views.CreateRequest, model);
        }

        /// <summary>
        /// This method is used for existing request 
        /// when user gives reqId we get ctnresvId and redirect to DisplayRequest 
        /// </summary>
        /// <param name="reqId"></param>
        /// <returns></returns>
        public virtual ActionResult DisplayExistingRequest(string resvId)
        {
            if (string.IsNullOrWhiteSpace(resvId))
            {
                //TC6: Get here if user enters invalid reqId form recent request view.
                ModelState.AddModelError("resvId", "Please enter the Request ID");
                return RedirectToAction(Actions.DisplayRecentRequest());
            }
            return RedirectToAction(Actions.DisplayRequest(resvId));
        }

        /// <summary>
        /// This is the GET method for RecentRequest Page
        /// which populates the recent requests list
        /// </summary>
        /// <returns></returns>
        public virtual ActionResult DisplayRecentRequest()
        {
            var requests = _service.GetRequests();
            var model = new RecentRequestsViewModel
            {
                RecentRequests = requests.Select(p => new RequestModel(p)).ToList()
            };
            return View(Views.RecentRequests, model);
        }


        /// <summary>
        /// Method which populates all list.
        /// </summary>
        /// <param name="model"></param>
        private void PopulateIndexViewModel(CreateRequestViewModel model)
        {
            if (model.CurrentRequest == null)
            {
                model.CurrentRequest = new EditRequestHeaderModel();
            }

            var vwh = _service.GetVwhList().ToList();
            model.VirtualWareHouseList = vwh.Select(MapCode);
            model.TargetVwhList = vwh.Select(MapCode);

            var buildingList = _service.GetBuildingList();
            model.BuildingList = buildingList.Select(MapCode);

            var qualityCode = _service.GetQualityCodes().ToList();
            model.SourceQualityCodeList = qualityCode.Select(MapCode);
            model.CurrentRequest.TargetQualityCode = qualityCode.Select(p => p.Code).LastOrDefault();

            var sewingPlants = _service.GetSewingPlantCodes();
            model.SewingPlantCodes = sewingPlants.Select(MapCode);

            var priceSeasonCodes = _service.GetPriceSeasonCodes();
            model.PriceSeasonCodes = priceSeasonCodes.Select(MapCode);
        }

        /// <summary>
        /// Creates a new Request 
        /// </summary>
        /// <param name="model">
        /// </param>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// Input: CreateRequestViewModel
        /// </para>
        /// <para>
        /// OutPut: Redirects to ManageSku with created request id.
        /// If input is invalid, redisplays the Create view.
        /// </para>
        /// </remarks>
        [HttpPost]
        public virtual ActionResult CreateOrUpdateRequest(CreateRequestViewModel model)
        {
            if (!ModelState.IsValid)
            {
                //TC7: Get here if user enters some invalid request info to update.
                // Unable to Create or Update Populate RequestViewModel again 
                //rvm.CurrentRequest = model.CurrentRequest;
                PopulateIndexViewModel(model);
                return View(Views.CreateRequest, model);
            }
            var requestModel = PopulateRequestModel(model);
            if (string.IsNullOrEmpty(model.CurrentRequest.ResvId))
            {
                //TC8: You will get here if you are creating new request.
                try
                {
                    _service.CreateCartonRequest(requestModel);
                }
                catch (ProviderException ex)
                {
                    PopulateIndexViewModel(model);
                    ModelState.AddModelError("", ex.Message);
                    return View(Views.CreateRequest, model);
                }
            }
            else
            {
                try
                {
                    //TC9: You will get here if you are updating request.
                    //updating existing Request
                    _service.UpdateCartonRequest(requestModel, RequestProperties.BuildingId | RequestProperties.SourceAreaId | RequestProperties.Priority |
                        RequestProperties.DestinationArea | RequestProperties.TargetQualityCode |
                        RequestProperties.TargetVwhId | RequestProperties.SourceVwhId | RequestProperties.Remarks | RequestProperties.QualityCode |
                        RequestProperties.CartonReceivedDate | RequestProperties.SewingPlantCode | RequestProperties.PriceSeasonCode | RequestProperties.IsConversionRequest);
                }
                catch (ProviderException ex)
                {
                    PopulateIndexViewModel(model);
                    ModelState.AddModelError("", ex.Message);
                    return View(Views.CreateRequest, model);
                }
            }
            return RedirectToAction(MVC_REQ2.REQ2.Req.Actions.DisplayRequest(requestModel.CtnResvId));
        }

        /// <summary>
        /// Populate Request.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private static Request PopulateRequestModel(CreateRequestViewModel model)
        {
            var requestModel = new Request
            {
                BuildingId = model.CurrentRequest.BuildingId,
                CtnResvId = model.CurrentRequest.ResvId,
                DestinationArea = model.CurrentRequest.DestinationAreaId,
                Priority = model.CurrentRequest.Priority.ToString(),
                Remarks = model.CurrentRequest.Remarks,
                SourceAreaId = model.CurrentRequest.SourceAreaId,
                SourceVwhId = model.CurrentRequest.VirtualWareHouseId,
                IsConversionRequest = model.CurrentRequest.IsConversionRequest,
                TargetQuality = model.CurrentRequest.IsConversionRequest ? model.CurrentRequest.TargetQualityCode : null,
                TargetVwhId = model.CurrentRequest.IsConversionRequest ? model.CurrentRequest.TargetVwhId : null,
                DestinationAreaShortName = model.CurrentRequest.DestinationAreaShortName,
                //ReqId = model.CurrentRequest.ReqId,
                RequestedBy = model.CurrentRequest.RequestedBy,
                SourceAreaShortName = model.CurrentRequest.SourceAreaShortName,
                SourceQuality = model.CurrentRequest.SorceQualityCode,
                SewingPlantCode = model.CurrentRequest.SewingPlantCode,
                PriceSeasonCode = model.CurrentRequest.PriceSeasonCode,
                CartonReceivedDate = model.CurrentRequest.CartonReceivedDate,
            };
            return requestModel;
        }

        /// <summary>
        /// Deleting Existing request
        /// </summary>
        /// <param name="resvId"></param>
        /// <returns></returns>
        //[HttpPost]
        public virtual ActionResult DeleteRequest(string resvId)
        {
            if (string.IsNullOrEmpty(resvId))
            {
                //TC12: You will get here if user you enters blank. This will not come in normal user practise.
                //RequestId should be passed
                ModelState.AddModelError("", "Inappropriate data can't perform delete operation");
            }
            //TC13: If user wants to delete request.
            _service.DeleteCartonRequest(resvId);
            return RedirectToAction(Actions.DisplayRecentRequest());
        }

        /// <summary>
        /// This method is used to get the info about existing request
        /// </summary>
        /// <param name="ctnrsvId"></param>
        /// <returns></returns>
        public virtual ActionResult DisplayRequest(string ctnrsvId)
        {
            Request requestInfo = null;
            if (string.IsNullOrEmpty(ctnrsvId))
            {
                //TC14: Get here if resvId is null. This will not come in normal user practise.
                ModelState.AddModelError("ctnresvId", "Please enter the valid Request ID");
                return RedirectToAction(Actions.DisplayRecentRequest());
            }
            requestInfo = _service.GetRequestInfo(ctnrsvId);

            if (requestInfo == null)
            {
                //TC14: If no info found for passed resvId.
                //non existing Request passed by search text box
                ModelState.AddModelError("ctnresvId", "Please enter the valid Request ID");
                return RedirectToAction(Actions.DisplayRecentRequest());
            }
            var model = new ManageSkuViewModel
            {
                CurrentRequest = new RequestModel(requestInfo),
                RequestedSkus = _service.GetRequestSKUs(requestInfo.CtnResvId).Select(p => new RequestSkuModel(p)).ToList()
            };
            return View(Views.ManageSku, model);
        }


        /// <summary>
        /// this method is used for show carton list for existing request
        /// </summary>
        /// <param name="ctnresvId"></param>
        /// <returns>
        /// return a view which show Cartonlist
        /// </returns>
        public virtual ActionResult DisplayCartonList(string ctnresvId)
        {
            var result = _service.GetCartonList(ctnresvId);
            var model = new CartonListViewModel
                            {
                                CartonList = result.Select(p => new CartonModel
                                {
                                    CartonId = p.CartonId,
                                    AreaDescription = p.AreaDescription,
                                    AreaShortName = p.AreaShortName,
                                    // PalletId = p.PalletId,
                                    StoregeArea = p.StoregeArea,
                                    QualityCode = p.QuilityCode,
                                    Quantity = p.Quantity,
                                    VwhId = p.VwhId,
                                    ReworkNeeded = p.ReworkNeeded
                                }).ToList(),
                                CtnresvId = ctnresvId
                            };
            return View(Views.CartonList, model);
        }

        /// <summary>
        /// This method is used to add an SKU to an existing request
        /// </summary>
        /// <param name="model"></param>
        /// <returns>
        /// Return a partial view which shows the containing SKUs in list
        /// </returns>
        [HttpPost]
        public virtual ActionResult AddSku(ManageSkuViewModel model)
        {
            System.Threading.Thread.Sleep(1000);
            //User can convert only VwhId without providing target sku.Target sku is only required when user wants to convert sku
            Sku targetSku = null;
            if (ModelState.IsValid && !string.IsNullOrWhiteSpace(model.TargetStyle))
            {
                // if TargetStyle is non null, validation gurantees that all other target components are non null
                targetSku = _service.GetSku(model.TargetStyle, model.TargetColor, model.TargetDimension, model.TargetSkuSize);
                if (targetSku == null)
                {
                    //TC16: If invalid SKU passed.
                    ModelState.AddModelError("", string.Format("Target SKU {0},{1},{2},{3} is invalid", model.TargetStyle, model.TargetColor, model.TargetDimension, model.TargetSkuSize));
                }
            }
            if (ModelState.IsValid)
            {
                var newSku = _service.GetSku(model.NewStyle, model.NewColor, model.NewDimension, model.NewSkuSize);
                if (newSku == null)
                {
                    //TC17: If invalid Source SKU passed.
                    ModelState.AddModelError("", string.Format("SKU {0},{1},{2},{3} is invalid", model.NewStyle, model.NewColor, model.NewDimension, model.NewSkuSize));
                }
                else
                {
                    //Adding sku to request
                    var result = _service.AddSkutoRequest(model.CurrentRequest.ResvId, newSku.SkuId, model.NewPieces.Value,
                                             targetSku == null ? (int?)null : targetSku.SkuId);
                    if (result == 0)
                    {
                        //If AddSku funtion returns 1. This indicate a new sku added to the request.
                        AddStatusMessage(string.Format("SKU <strong>{0}</strong> with <strong>{1}</strong> pieces added.", newSku, model.NewPieces));
                    }
                    else
                    {
                        //If AddSku funtion returns 0. This indicate an existing sku is updated in request.
                        AddStatusMessage(string.Format("SKU <strong>{0}</strong> with <strong>{1}</strong> pieces updated.", newSku, model.NewPieces));
                    }
                    model.SkuIdLastAdded = newSku.SkuId;
                }
            }
            //getting list of added all SKUs to request.
            var skus = _service.GetRequestSKUs(model.CurrentRequest.ResvId);
            model.RequestedSkus = skus.Select(p => new RequestSkuModel(p)).ToList();

            return PartialView(Views._manageSkuListPartial, model);
        }

        /// <summary>
        /// This method is used to delete an SKU from existing request
        /// </summary>
        /// <param name="skuId"></param>
        /// <param name="resvId"></param>
        /// <returns>
        /// Return a partial view which shows the containing SKUs in list
        /// </returns>
        [HttpPost]
        public virtual ActionResult DeleteSku(int? skuId, string resvId)
        {
            if (skuId == null || string.IsNullOrEmpty(resvId))
            {
                //TC18: If null skuid or rescId passed. this will not come in normal request practise.
                this.Response.StatusCode = 203;
                return Content("Inappropriate data can't perform delete operation");
            }
            try
            {
                //Deleting SKU from the Request
                _service.DeleteSkuFromRequest((int)skuId, resvId);
                var model = new ManageSkuViewModel
                    {
                        CurrentRequest = new RequestModel
                            {
                                ResvId = resvId
                            },
                        //Getting all remaining SKUs of Request
                        RequestedSkus = _service.GetRequestSKUs(resvId).Select(p => new RequestSkuModel(p)).ToArray()
                    };

                return PartialView(MVC_REQ2.REQ2.Req.Views._manageSkuListPartial, model);
            }
            catch (Exception ex)
            {
                // Simulate the behavior of the obsolete HandleAjaxError attribute
                this.Response.StatusCode = 203;
                return Content(ex.Message);
            }
        }

        /// <summary>
        /// This method Assigns carton to the request
        /// </summary>
        /// This method will be called from ManageSKU page which post the model RequestViewModel
        /// we are passing values to update Request.
        /// <returns>
        /// Returning the ManageSKU View  with ModelState Errors if exist
        /// </returns>
        /// <remarks>
        /// The posted model should be of type <see cref="ManageSkuViewModel"/>. This action will access 
        /// <c>CurrentRequest.Header.ResvId</c> within the model to get the id. It will then access the values in
        /// <c>CurrentRequest.CartonRules</c> to perform the assignment
        /// </remarks>
        [HttpPost]
        public virtual ActionResult AssignCartons(string ctnresvId)
        {
            if (string.IsNullOrEmpty(ctnresvId))
            {
                //TC19: If request Id was not passed. This will not come in normal user practise.
                throw new ApplicationException("Internal Error. Request Id was not passed.");
            }

            try
            {
                var pieces = _service.AssignCartons(ctnresvId);
                AddStatusMessage(string.Format("{0} pieces has been assigned to request {1}", pieces, ctnresvId));
            }
            catch (DbException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            catch (ProviderException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            return RedirectToAction(Actions.DisplayRequest(ctnresvId));
        }

        /// <summary>
        /// Unassigning the cartons from request
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public virtual ActionResult UnAssignCartons(string ctnresvId)
        {
            if (string.IsNullOrEmpty(ctnresvId))
            {
                //TC20: If request Id was not passed. This will not come in normal user practise.
                throw new ApplicationException("Internal Error. Request Id was not passed.");
            }
            try
            {
                //Unassigning cartons
                var cartons = _service.UnAssignCartons(ctnresvId);
                AddStatusMessage(string.Format("{0} cartons have been unassigned from the request", cartons));
            }
            catch (DbException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            return RedirectToAction(Actions.DisplayRequest(ctnresvId));
        }


        /// <summary>
        /// Populates cascading dropdown list for passed building
        /// </summary>
        /// <param name="buildingId"></param>
        /// <returns></returns>
        public virtual ActionResult GetCartonAreas(string buildingId)
        {
            var data = from area in _service.GetCartonAreas(buildingId)
                       select new
                       {
                           Text = string.Format("{0}: {1} [{2:N0} cartons]", area.ShortName ?? area.AreaId, area.Description, area.CartonCount),
                           Value = area.AreaId,
                           Numbered = area.LocationNumberingFlag,
                           Count = area.CartonCount,
                           ReworkArea = area.IsReworkArea
                       };
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// For Tutorial View.
        /// </summary>
        /// <returns></returns>
        public virtual ActionResult Tutorial()
        {
            return View(Views.Tutorial);
        }
    }
}

//$Id$
