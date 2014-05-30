using System;
using System.Configuration.Provider;
using System.Data.Common;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using DcmsMobile.REQ2.Models;
using DcmsMobile.REQ2.Repository;
using DcmsMobile.REQ2.ViewModels;
using EclipseLibrary.Mvc.Controllers;
using EclipseLibrary.Mvc.Html;

namespace DcmsMobile.REQ2.Areas.REQ2.Controllers
{
    [AuthorizeEx("REQ2 requires Role {0}", Roles = "DCMS8_REQUEST")]
    public partial class HomeController : EclipseController
    {
        private const string COOKIE_BUILDING = "Building";

        static HomeController()
        {

        }

        #region Intialization

        /// <summary>
        /// Required by T4MVC
        /// </summary>
        public HomeController()
        {

        }

        private ReqService _service;

        protected override void Initialize(RequestContext requestContext)
        {
            if (_service == null)
            {
                _service = new ReqService(requestContext);
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
        /// This is the GET Method for Create new Request 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public virtual ActionResult CreateRequest(string ctnresvId)
        {
            var model = new SelectRequestViewModel();
            if (!string.IsNullOrEmpty(ctnresvId))
            {
                model.CurrentRequest = new RequestHeaderViewModel(_service.GetRequestInfo(ctnresvId));
            }
            PopulateIndexViewModel(model);
            return View(Views.CreateRequest, model);
        }

        /// <summary>
        /// This is the GET method for RecentRequest Page
        /// which populates the recent requests list
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public virtual ActionResult Index()
        {
            var requests = _service.GetRequests();
            var model = new RecentRequestsViewModel
            {
                RecentRequests = requests.Select(p => new RequestViewModel(p)).ToList()
            };
            return View(Views.RecentRequests, model);
        }

        private GroupSelectListItem MapArea(CartonArea entity)
        {
            return new GroupSelectListItem
            {
                Value = entity.AreaId,
                GroupText = entity.BuildingId,
                Text = entity.ShortName + " : " + entity.Description
            };
        }

        private void PopulateIndexViewModel(SelectRequestViewModel model)
        {
            if (model.CurrentRequest == null)
            {
                model.CurrentRequest = new RequestHeaderViewModel();
            }

            var vwh = _service.GetVwhList().ToList();
            model.VirtualWareHouseList = vwh.Select(p => MapCode(p));

            model.TargetVwhList = vwh.Select(p => MapCode(p));

            var areas = _service.GetCartonAreas().ToList();

            //model.SourceAreas = Mapper.Map<IEnumerable<GroupSelectListItem>>(areas.Where(p => (p.LocationNumberingFlag) && (p.IsCartonArea)));
            model.SourceAreas = areas.Where(p => (p.LocationNumberingFlag) && (p.IsCartonArea)).Select(p => MapArea(p)).ToList();

            //model.DestinationAreas =
            //Enumerable.Repeat(new GroupSelectListItem
            //{
            //    Text = "(Please Select)",
            //    Value = ""
            //}, 1).Concat(
            //Mapper.Map<IEnumerable<GroupSelectListItem>>(areas.Where(p => !p.UnusableInventory && !p.LocationNumberingFlag)));
            model.DestinationAreas =
            Enumerable.Repeat(new GroupSelectListItem
            {
                Text = "(Please Select)",
                Value = ""
            }, 1).Concat(areas.Where(p => !p.UnusableInventory && !p.LocationNumberingFlag).Select(p => MapArea(p))).ToList();

            var qualityCode = _service.GetQualityCodes();
            model.TargetQualityCodeList = qualityCode.Select(p => MapCode(p));

            var saleTypes = _service.GetSaleTypeList();
            model.SaleTypes = saleTypes.Select(p => MapCode(p));

        }



        /// <summary>
        /// Creates a new request 
        /// </summary>
        /// <param name="model">
        /// </param>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// Input: RequestViewModel
        /// </para>
        /// <para>
        /// OutPut: Redirects to ManageSku with created request id.
        /// If input is invalid, redisplays the Create view.
        /// </para>
        /// </remarks>
        [HttpPost]
        public virtual ActionResult UpdateRequest(SelectRequestViewModel model)
        {
            var rvm = new SelectRequestViewModel();
            if (!ModelState.IsValid)
            {
                // Unable to Create or Update Populate RequestViewModel again 
                rvm.CurrentRequest = model.CurrentRequest;
                PopulateIndexViewModel(rvm);
                return View(Views.CreateRequest, rvm);
            }
            var requestModel = new RequestModel
            {
                AllowOverPulling = model.CurrentRequest.OverPullCarton ? "O" : "U",
                BuildingId = model.CurrentRequest.BuildingId,
                CtnResvId = model.CurrentRequest.ResvId,
                DestinationArea = model.CurrentRequest.DestinationAreaId,
                PackagingPreferance = model.CurrentRequest.IsHung ? "H" : "",
                Priority = model.CurrentRequest.Priorities.ToString(),
                Remarks = model.CurrentRequest.Remarks,
                SaleTypeId = model.CurrentRequest.SaleTypeId,
                SourceAreaId = model.CurrentRequest.SourceAreaId,
                TargetVwhId = model.CurrentRequest.TargetVwhId,
                SourceVwhId = model.CurrentRequest.VirtualWareHouseId,
                IsConversionRequest = model.CurrentRequest.RequestForConversion,
                TargetQuality = model.CurrentRequest.TargetQualityCode,
                DestinationAreaShortName = model.CurrentRequest.DestinationAreaShortName,
                ReqId = model.CurrentRequest.ReqId,
                RequestedBy = model.CurrentRequest.RequestedBy,
                SourceAreaShortName = model.CurrentRequest.SourceAreaShortName
            };

            if (string.IsNullOrEmpty(model.CurrentRequest.ResvId))
            {
                //Creating New Request
                try
                {
                    _service.CreateCartonRequest(requestModel);
                }
                catch (ProviderException ex)
                {

                    PopulateIndexViewModel(rvm);
                    ModelState.AddModelError("", ex.Message);
                    return View(Views.CreateRequest, rvm);
                }
            }
            else
            {
                try
                {
                    //updating existing Request
                    _service.UpdateCartonRequest(requestModel, RequestProperties.BuildingId | RequestProperties.SourceAreaId | RequestProperties.Priority |
                         RequestProperties.TargetVwhId | RequestProperties.DestinationArea | RequestProperties.AllowOverPulling | RequestProperties.PackagingPreference |
                         RequestProperties.SaleTypeId | RequestProperties.SourceVwhId | RequestProperties.Remarks | RequestProperties.QualityCode | RequestProperties.TargetQualityCode);
                }
                catch (ProviderException ex)
                {
                    PopulateIndexViewModel(rvm);
                    rvm.CurrentRequest.ResvId = requestModel.CtnResvId;
                    ModelState.AddModelError("", ex.Message);
                    return View(Views.CreateRequest, rvm);
                }
            }
            var cookie = new HttpCookie(COOKIE_BUILDING)
                             {
                                 Value = requestModel.BuildingId,
                                 Expires = DateTime.Now.AddDays(7)
                             };
            // Remember building for 7 days
            this.Response.Cookies.Add(cookie);
            return RedirectToAction(MVC_REQ2.REQ2.Home.Actions.DisplayRequest(requestModel.CtnResvId));
        }

        /// <summary>
        /// this method is use for existing request 
        /// when user give reqId we get ctnresvId and redirect to DisplayRequest 
        /// </summary>
        /// <param name="reqId"></param>
        /// <returns></returns>
        [HttpGet]
        public virtual ActionResult DisplayExistingRequest(string reqId)
        {
            int _reqId;
            if (!int.TryParse(reqId, out _reqId))
            {
                AddStatusMessage("Please enter the valid Request ID");
                return Index();
            }

            var ctnresvId = _service.GetCtnRevId(reqId);
            return RedirectToAction(MVC_REQ2.REQ2.Home.Actions.DisplayRequest(ctnresvId));
        }

        /// <summary>
        /// Deleting Existing request
        /// </summary>
        /// <param name="resvId"></param>
        /// <returns></returns>
        public virtual ActionResult DeleteRequest(string resvId)
        {
            if (string.IsNullOrEmpty(resvId))
            {
                //RequestId should be passed
                this.Response.StatusCode = 203;
                return Content("Inappropriate data can't perform delete operation");
            } try
            {
                _service.DeleteCartonRequest(resvId);

                //Request deleted successfully
                var requests = _service.GetRequests();
                var model = new RecentRequestsViewModel
                    {
                        RecentRequests = requests.Select(p => new RequestViewModel(p))
                    };

                return PartialView(Views._recentRequestListPartial, model);
            }
            catch (Exception ex)
            {
                // Simulate the behavior of the obsolete HandleAjaxError attribute
                this.Response.StatusCode = 203;
                return Content(ex.Message);
            }
        }

        /// <summary>
        /// This method is used to get the info about existing request
        /// </summary>
        /// <param name="ctnresvId">
        /// DO NOT CHANGE THE PARAMETER NAME FROM 'ctnresvId'
        /// Because its being used by the partial view model to map with this action method parameter
        /// </param>
        /// <returns></returns>
        /// <remarks> TODO: Remove hardwired name of the parameter as 'ctnresvId'
        /// Any exception will lead to a yellow screen.
        /// </remarks>
        public virtual ActionResult DisplayRequest(string ctnresvId)
        {
            RequestModel requestInfo;
            if (string.IsNullOrEmpty(ctnresvId))
            {
                this.AddStatusMessage("Please enter the valid Request ID");
                requestInfo = null;
            }
            else
            {
                requestInfo = _service.GetRequestInfo(ctnresvId);
                if (requestInfo == null)
                {
                    //non existing Request passed by search text box
                    this.AddStatusMessage("Please enter a valid Request ID");
                }
            }

            if (requestInfo == null)
            {
                return RedirectToAction(MVC_REQ2.REQ2.Home.Actions.Index());
            }
            return DoDisplayRequest(requestInfo, requestInfo.AssignedFlag ? ViewTab.CartonList : ViewTab.AddSku);
        }

        private SelectListItem MapCode(CodeDescriptionModel entity)
        {
            return new SelectListItem
            {
                Value = entity.Code,
                Text = entity.Code + " : " + entity.Description
            };
        }

        private ActionResult DoDisplayRequest(RequestModel requestInfo, ViewTab selectedTab)
        {
            var model = new ManageSkuViewModel();

            var qualities = _service.GetQualityCodes();
            model.Qualities = qualities.Select(p => MapCode(p));

            var sewingPlants = _service.GetSewingPlantCodes();
            model.SewingPlantCodes = sewingPlants.Select(p => MapCode(p));

            var priceSeasonCodes = _service.GetPriceSeasonCodes();
            model.PriceSeasonCodes = priceSeasonCodes.Select(p => MapCode(p));

            var buildings = _service.GetBuildingList();
            model.BuildingList = buildings.Select(p => MapCode(p));

            model.CurrentRequest = new RequestViewModel(requestInfo);

            var skus = _service.GetRequestSKUs(requestInfo.CtnResvId);
            model.RequestedSkus = skus.Select(p => new RequestSkuViewModel(p)).ToList();

            var result = _service.GetAssignedCartons(requestInfo.CtnResvId);
            model.AssignedCartonInfo = result.Select(row => new AssignedCartonViewModel
            {
                PulledCartons = row.PulledCartons,
                TotalCartons = row.TotalCartons,
                PulledPieces = row.PulledPieces,
                TotalPieces = row.TotalPieces,
                Sku = new SkuViewModel
                {
                    Style = row.Sku.Style,
                    Color = row.Sku.Color,
                    Dimension = row.Sku.Dimension,
                    SkuSize = row.Sku.SkuSize,
                    SkuId = row.Sku.SkuId,
                    UpcCode = row.Sku.UpcCode
                }
            });
            model.SelectedTab = selectedTab;

            return View(Views.ManageSku, model);
        }
        /// <summary>
        /// this method is used for show carton list for existing request
        /// </summary>
        /// <param name="ctnresvId"></param>
        /// <returns>
        /// return a view which show Cartonlist
        /// </returns>
        [HttpGet]
        public virtual ActionResult DisplayCartonList(string ctnresvId)
        {
            var result = _service.GetCartonList(ctnresvId);
            var model = new CartonListViewModel
                            {
                                CartonList = result.Select(p => new CartonListViewModel
                                {
                                    CartonId = p.CartonId,
                                    AreaDescription = p.AreaDescription,
                                    PalletId = p.PalletId,
                                    StoregeArea = p.StoregeArea,
                                    QualityCode = p.QuilityCode,
                                    ReqId = p.ReqId,
                                    CtnresvId = p.CtnresvId,
                                    Quantity = p.Quantity,
                                    VwhId = p.VwhId
                                }),
                                CtnresvId = ctnresvId
                            };
            if (model.CartonList.Any())
            {
                model.ReqId = model.CartonList.First().ReqId;
            }
            return View(Views.CartonList, model);
        }

        /// <summary>
        /// This method is used to add an SKU to an existing request
        /// </summary>
        /// <param name="model"></param>
        /// <returns>
        /// Return a partial view which shows the containing SKUs in list
        /// </returns>
        public virtual ActionResult AddSku(ManageSkuViewModel model)
        {
            //User can convert only VwhId without providing target sku.Target sku is only required when user wants to convert sku
            SkuModel targetSku = null;
            if (model.CurrentRequest.Header.RequestForConversion)
            {
                if (string.IsNullOrEmpty(model.TargetStyle) && string.IsNullOrEmpty(model.TargetColor) && string.IsNullOrEmpty(model.TargetDimension) && string.IsNullOrEmpty(model.TargetSkuSize))
                {
                    // Target SKU is required.
                    this.Response.StatusCode = 203;
                    return Content("Target SKU is required.");
                }
                targetSku = _service.GetSku(model.TargetStyle, model.TargetColor, model.TargetDimension, model.TargetSkuSize);
                if (targetSku == null)
                {
                    // Target SKU is required.
                    this.Response.StatusCode = 203;
                    return Content(string.Format("Target SKU {0},{1},{2},{3} is invalid", model.TargetStyle, model.TargetColor, model.TargetDimension, model.TargetSkuSize));
                }
            }
            var newSku = _service.GetSku(model.NewStyle, model.NewColor, model.NewDimension, model.NewSkuSize);
            if (newSku == null)
            {
                this.Response.StatusCode = 203;
                return Content(string.Format("SKU {0},{1},{2},{3} is invalid", model.NewStyle, model.NewColor, model.NewDimension, model.NewSkuSize));
            }
            try
            {
                //Adding sku to request
                _service.AddSkutoRequest(model.CurrentRequest.Header.ResvId, newSku.SkuId, model.NewPieces.Value,
                                         targetSku == null ? (int?)null : targetSku.SkuId);

                //getting list of added all SKUs to request.
                var skus = _service.GetRequestSKUs(model.CurrentRequest.Header.ResvId);
                model.RequestedSkus = skus.Select(p => new RequestSkuViewModel(p));
                return PartialView(Views._manageSkuListPartial, model);
            }
            catch (Exception ex)
            {
                // Simulate the behavior of the obsolete HandleAjaxError attribute
                this.Response.StatusCode = 203;
                return Content(ex.Message);
            }
        }

        /// <summary>
        /// This method is used to delete an SKU from existing request
        /// </summary>
        /// <param name="skuId"></param>
        /// <param name="resvId"></param>
        /// <returns>
        /// Return a partial view which shows the containing SKUs in list
        /// </returns>
        public virtual ActionResult DeleteSku(int? skuId, string resvId)
        {
            if (skuId == null || string.IsNullOrEmpty(resvId))
            {
                this.Response.StatusCode = 203;
                return Content("Inappropriate data can't perform delete operation");
            }
            try
            {
                //Deleting SKU from the Request
                _service.DeleteSkuFromRequest((int)skuId, resvId);
                var model = new ManageSkuViewModel();
                //Getting all remaining SKUs of Request
                var skus = _service.GetRequestSKUs(resvId);
                model.RequestedSkus = skus.Select(p => new RequestSkuViewModel(p));
                return PartialView(MVC_REQ2.REQ2.Home.Views._manageSkuListPartial, model);
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
        public virtual ActionResult AssignCartons()
        {
            var ctnresvId = this.ValueProvider.GetValue(EclipseLibrary.Mvc.Helpers.ReflectionHelpers.NameFor((ManageSkuViewModel m) => m.CurrentRequest.Header.ResvId))
                .AttemptedValue;
            if (string.IsNullOrEmpty(ctnresvId))
            {
                throw new ApplicationException("Internal Error. Request Id was not passed.");
            }
            var rules = new RequestCartonRulesViewModel();
            RequestModel requestUpdated;
            if (TryUpdateModel(rules, EclipseLibrary.Mvc.Helpers.ReflectionHelpers.NameFor((ManageSkuViewModel m) => m.CurrentRequest.CartonRules)))
            {
                try
                {
                    //If already cartons assigned to request, Firstly we have to unassign this
                    // Unassign carton is intelligent enough to not do anything if cartons have not been assigned
                    _service.UnAssignCartons(ctnresvId);
                    requestUpdated = new RequestModel
                    {
                        SourceQuality = rules.QualityCode,
                        SewingPlantCode = rules.SewingPlantCode,
                        PriceSeasonCode = rules.PriceSeasonCode,
                        CartonReceivedDate = rules.CartonReceivedDate,
                        BuildingId = rules.BuildingId
                    };
                    requestUpdated.CtnResvId = ctnresvId;
                    //Trying to update the carton rules of request
                    _service.UpdateCartonRequest(requestUpdated, RequestProperties.CartonReceivedDate | RequestProperties.BuildingId | RequestProperties.QualityCode |
                        RequestProperties.SewingPlantCode | RequestProperties.PriceSeasonCode);
                    //Assigning cartons to the request
                    _service.AssignCartons(ctnresvId);
                }
                catch (DbException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
                catch (ProviderException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            requestUpdated = _service.GetRequestInfo(ctnresvId);
            return DoDisplayRequest(requestUpdated, ModelState.IsValid ? ViewTab.CartonList : ViewTab.AssignCartons);
        }

        /// <summary>
        /// Unassigning the cartons from request
        /// </summary>
        /// <returns></returns>
        public virtual ActionResult UnAssignCartons()
        {
            var ctnresvId = this.ValueProvider.GetValue(EclipseLibrary.Mvc.Helpers.ReflectionHelpers.NameFor((ManageSkuViewModel m) => m.CurrentRequest.Header.ResvId))
                .AttemptedValue;
            if (string.IsNullOrEmpty(ctnresvId))
            {
                throw new ApplicationException("Internal Error. Request Id was not passed.");
            }
            try
            {
                //Unassigning cartons
                _service.UnAssignCartons(ctnresvId);
            }
            catch (DbException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            var requestUpdated = _service.GetRequestInfo(ctnresvId);
            return DoDisplayRequest(requestUpdated, ModelState.IsValid ? ViewTab.CartonList : ViewTab.AssignCartons);
        }

        /// <summary>
        /// For Tutorial View.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public virtual ActionResult Tutorial()
        {
            return View(Views.Tutorial);
        }
    }
}

//$Id$
