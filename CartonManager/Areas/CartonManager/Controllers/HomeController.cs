using DcmsMobile.CartonManager.Models;
using DcmsMobile.CartonManager.Repository;
using DcmsMobile.CartonManager.ViewModels;
using EclipseLibrary.Mvc.Controllers;
using EclipseLibrary.Mvc.Html;
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.WebPages;

namespace DcmsMobile.CartonManager.Areas.CartonManager.Controllers
{
    /// <summary>
    /// The main controller of this application
    /// </summary>
    /// <remarks>
    /// <para>
    /// The main view is rendered by <see cref="Index"/> which lists the various UI choices available. The UI choices are actually initiated by
    /// <see cref="AdvancedUi"/> and <see cref="PalletizeUi"/>. The carton is actually updated by
    /// <see cref="UpdateCartonOrPallet(UpdateCartonModel)"/> which is called for each carton scan.
    /// </para>
    /// </remarks>
    [RouteArea("CartonManager")]
    [RoutePrefix(HomeController.NameConst)]
    public partial class HomeController : EclipseController
    {
        private GroupSelectListItem Map(CartonArea src)
        {
            return new GroupSelectListItem
                {
                    Text = src.ShortName + " : " + src.Description,
                    Value = src.AreaId,
                    GroupText = string.IsNullOrEmpty(src.Building) ? "All Building" : src.Building
                };
        }

        private Carton Map(CartonModel src)
        {
            return new Carton
                {
                    PalletId = src.PalletId,
                    Pieces = src.Pieces ?? 0,
                    QualityCode = src.QualityCode,
                    VwhId = src.VwhId,
                    ReasonCode = src.ReasonCode,
                    PriceSeasonCode = src.PriceSeasonCode,
                    LocationId = src.LocationID,
                    IsReserved = src.IsReserved,
                    CartonArea = new CartonArea
                        {
                            AreaId = src.AreaId
                        },
                    SkuInCarton = new Sku
                        {
                            UpcCode = src.SkuBarCode,
                            SkuId = src.SkuId
                        }
                };
        }

        #region Intialization

        /// <summary>
        /// Required by T4MVC
        /// </summary>
        public HomeController()
        {

        }

        private CartonManagerService _service;

        protected override void Initialize(RequestContext requestContext)
        {
            base.Initialize(requestContext);
            if (_service == null)
            {
                _service = new CartonManagerService(requestContext);
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

        /// <summary>
        /// Toggles Mobile emulation
        /// </summary>
        /// <param name="emulateMobile"></param>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        public virtual ActionResult ToggleEmulation()
        {
            if (HttpContext.GetOverriddenBrowser().IsMobileDevice)
            {
                HttpContext.ClearOverriddenBrowser();
            }
            else
            {
                HttpContext.SetOverriddenBrowser(BrowserOverride.Mobile);
            }

            return RedirectToAction(Actions.Index());
        }

        /// <summary>
        /// Display a list of possible UI available
        /// </summary>
        /// <returns>View Index.cshtml</returns>
        /// <remarks>
        /// <para>
        /// The UI choices are <see cref="AdvancedUi"/> and <see cref="PalletizeUi"/>.
        /// </para>
        /// </remarks>
        public virtual ActionResult Index()
        {
            return View(Views.Index);
        }

        /// <summary>
        /// Returns the initial view which asks the user to enter information and then scan carton
        /// </summary>
        /// <returns>The view AdvancedUi.cshtml</returns>
        /// <remarks>
        /// <para>
        /// This UI exposes all carton editing options available for the user. The user must first specify what kinds of cartons qualify for updating
        /// by choosing values for <see cref="UpdateCartonModel.QualificationRules"/>. If the carton does not qualify based on the 
        /// <c>QualificationRules</c>, it will not be updated. Important qualification rules include <see cref="CartonModel.SkuBarCode"/>,
        /// <see cref="CartonModel.Pieces"/>, <see cref="CartonModel.VwhId"/>, <see cref="CartonModel.QualityCode"/>,
        /// <see cref="CartonModel.PriceSeasonCode"/>, <see cref="ReworkStatus"/>
        /// and many others. See the properties of <see cref="CartonModel"/>.
        /// </para>
        /// <para>
        /// <see cref="CartonModel"/> also defines what needs to be updated if the carton does qualify. All this processing takes place
        /// when user input from the GUI is posted to <see cref="UpdateCartonOrPallet(UpdateCartonModel)"/>.
        /// </para>
        /// </remarks>
        [AuthorizeEx("Bulk Carton Update  requires Role {0}", Roles = "SRC_CED_MGR", Purpose = "Enables Bulk carton updating")]
        [Route(HomeController.ActionNameConstants.AdvancedUi, Name = DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_BulkUpdateCarton)]
        public virtual ActionResult AdvancedUi()
        {
            var model = new AdvancedUiViewModel();
            model.OnViewExecuting(_service, this.ControllerContext);
            if (Request.Cookies[COOKIE_PRINTER] != null)
            {
                model.PrinterId = Request.Cookies[COOKIE_PRINTER][COOKIE_SUB_PRINTERID];
            }
            return View(Views.AdvancedUi, model);
        }

        /// <summary>
        /// Palletize cartons which do not require rework.
        /// </summary>
        /// <returns>The view PalletizeUi.cshtml</returns>
        /// <remarks>
        /// <para>
        /// UI asks for area and Pallet to be created.Palletizes scanned carton or Pallet.
        /// </para>
        /// </remarks>
        [AuthorizeEx("Create pallet requires Role {0}", Roles = "SRC_C2P", Purpose = "Enables palletization of cartons")]
        [Route(HomeController.ActionNameConstants.PalletizeUi, Name = DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_CartonToPallet)]
        public virtual ActionResult PalletizeUi()
        {
            var model = new PalletizeViewModel();
            var areas = _service.GetCartonAreas(null, null);
            model.AreaList = areas.Where(p => !p.IsNumberedLocationArea).Select(Map);
            return View(Views.PalletizeUi, model);
        }


        /// <summary>
        /// Mark rework complete for carton, can palletize it as well.
        /// </summary>
        /// <returns>The View MarkReworkCompleteUi.cshtml</returns>
        /// <remarks>
        /// <para>
        /// UI asks for carton which require rework. On scanning such cartons Mark Rework is completed. UI allows palletization of such cartons as well.
        /// </para>
        /// </remarks>
        [AuthorizeEx("Mark rework complete requires Role {0}", Roles = "SRC_CED", Purpose = "Enables marking carton rework status complete")]
        [Route(HomeController.ActionNameConstants.MarkReworkCompleteUi, Name = DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_MarkReworkComplete)]
        public virtual ActionResult MarkReworkCompleteUi()
        {
            var model = new MarkReworkCompleteViewModel();
            var areas = _service.GetCartonAreas(null, null);
            model.AreaList = areas.Where(p => !p.IsNumberedLocationArea).Select(Map);
            return View(Views.MarkReworkComplete, model);
        }


        /// <summary>
        /// Abandon rework to be done upon carton. Carton can be palletized as well.
        /// </summary>
        /// <returns>The view AbandonRewokUi.cshtml</returns>
        /// <remarks>
        /// <para>
        /// UI asks for cartons requiring rework. Successful scan abandons rework to be done upon carton.Palletization of carton is also possible
        /// provided carton requires rework.
        /// </para>
        /// </remarks>
        [AuthorizeEx("Abandon rework requires Role {0}", Roles = "SRC_CED", Purpose = "Allows abandoning of carton rework.")]
        [Route(HomeController.ActionNameConstants.AbandonReworkUi, Name = DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_AbandonRework)]
        public virtual ActionResult AbandonReworkUi()
        {
            var model = new AbandonReworkViewModel();
            var areas = _service.GetCartonAreas(null, null);
            model.AreaList = areas.Where(p => !p.IsNumberedLocationArea).Select(Map);
            return View(Views.AbandonRework, model);
        }


        /// <summary>
        /// This UI ask to user to scan destination area on which you have to put cartons or pallet.
        /// </summary>
        /// <returns>view DestinationArea.mobile.cshtml</returns>
        [AuthorizeEx("Palletize requires Role {0}", Roles = "SRC_C2P", Purpose = "Enables palletization of cartons")]
        public virtual ActionResult PalletizeMobile()
        {
            return View(Views.DestinationArea_mobile, new DestinationAreaForMobileViewModel());
        }


        /// <summary>
        /// If model state is invalid, returns validation errors, else returns status messages
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Different results are returned based on whether the request is an ajax request or not. Non ajax request (i.e. postbacks) are sent from mobile views.
        /// In this case, the view model is constructed based on the posted type and updated based on the posted values.
        /// </remarks>
        private ActionResult AdaptiveResult(int? statusCode, RouteValueDictionary responseHeaders, ViewModelBase model)
        {
            if (ModelState.IsValid && model.StatusMessages.Count == 0)
            {
                // If there are no or single status messages( about scanned pallet info)  and model is valid, it means that the carton did not need updating
                model.StatusMessages.Clear();
                model.StatusMessages.Add(string.Format("Carton# {0} did not require updating.", model.ScanText));
            }
            if (this.Request.IsAjaxRequest())
            {
                // Returns status or error messages as content
                if (ModelState.IsValid)
                {
                    if (statusCode != null)
                    {
                        this.Response.StatusCode = statusCode.Value;
                    }
                    if (responseHeaders != null)
                    {
                        foreach (var item in responseHeaders)
                        {
                            this.Response.AppendHeader(item.Key, item.Value.ToString());
                        }
                    }
                    return Content(string.Join("; ", model.StatusMessages));
                }
                //We get here when no update property specified
                return ValidationErrorResult();
            }
            model.OnViewExecuting(_service, this.ControllerContext);
            if (model.StatusMessages.Count > 0)
            {
                AddStatusMessage(string.Join(",", model.StatusMessages));
            }
            return View(model.ViewName, model);
        }

        /// <summary>
        /// Accepts information needed to update and/or move the carton.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// This function can accept both AJAX and postback calls. For postback calls, additional fields must be posted.
        /// </para>
        /// <code>
        /// <![CDATA[
        ///@Html.HiddenFor(m => m.ModelTemplateTypes)
        ///<input type="hidden" value="@Model.ViewModelType" name="@Model.NameFor(m => m.ViewModelType)" />
        ///<input type="hidden" value="CartonEditor" name="@Model.NameFor(m => m.ViewName)" />
        /// ]]>
        /// </code>
        /// <list type="table">
        /// <listheader>
        /// <term>
        /// Response Status Code
        /// </term>
        /// <description>
        /// Response Text and Response Headers
        /// </description>
        /// </listheader>
        /// <item>
        /// <term>
        /// 200
        /// </term>
        /// <description>
        /// The carton was updated and/or moved. The response text is status message which can be displayed to the user.
        /// Response header <c>CartonCount</c> contains the number of cartons updated.
        /// </description>
        /// </item>
        /// <item>
        /// <term>
        /// 203
        /// </term>
        /// <description>
        /// The carton could not be updated or moved. The response text is error message which can be displayed to the user.
        /// </description>
        /// </item>
        /// <item>
        /// <term>
        /// 201
        /// </term>
        /// <description>
        /// Confirmation scan required. This occurs when a pallet is scanned. The user must confirm that he wants to update all cartons on the pallet.
        /// Response text contains the confirmation message which can be displayed to the user. Response header <c>ConfirmScan</c> contains the scan
        /// which needs to be confirmed.
        /// </description>
        /// </item>
        /// </list>
        /// </remarks>

        private const string COOKIE_PRINTER = "Cookie_Printer";
        private const string COOKIE_SUB_PRINTERID = "Cookie_PrinterId";

        [HttpPost]
        [AuthorizeEx("Bulk Carton Update  requires Role {0}", Roles = "SRC_CED_MGR", Purpose = "Enables Bulk carton updating")]
        [AuthorizeEx("Create pallet requires Role {0}", Roles = "SRC_C2P", Purpose = "Enables palletization of cartons")]
        [AuthorizeEx("Mark rework complete requires Role {0}", Roles = "SRC_CED", Purpose = "Enables marking carton rework status complete")]
        [AuthorizeEx("Abandon rework requires Role {0}", Roles = "SRC_CED", Purpose = "Allows abandoning of carton rework.")]
        [AuthorizeEx("Palletize requires Role {0}", Roles = "SRC_C2P", Purpose = "Enables palletization of cartons")]
        public virtual ActionResult UpdateCartonOrPallet(ViewModelBase model)
        {
            if (!ModelState.IsValid)
            {
                //TC1:We get here when no update property specified
                return AdaptiveResult(202, null, model);
            }

            if (string.IsNullOrEmpty(model.ScanText))
            {
                // TC2: Scan nothing and press Go.
                ModelState.AddModelError("", "Please scan carton/pallet.");
                return AdaptiveResult(202, null, model);
            }

            try
            {
                if (_service.IsPallet(model.ScanText))
                {
                    // TC3:Pallet has been scanned
                    var cartons = _service.GetCartonsOnPallet(model.ScanText).ToArray();
                    if (cartons.Count() == 0)
                    {
                        model.StatusMessages.Add(string.Format("Pallet {0} does not exist.", model.ScanText));
                        return AdaptiveResult(202, null, model);
                    }
                    if (model.ConfirmScanText != model.ScanText)
                    {
                        //TC4: Give a non matching confirmation scan
                        model.StatusMessages.Add(
                            string.Format("Please rescan Pallet {0} to confirm updating of {1} cartons.", model.ScanText,
                                          cartons.Count()));
                        return AdaptiveResult(201, new RouteValueDictionary
                            {
                                {"ConfirmScan", model.ScanText}
                            }, model);
                    }

                    foreach (var carton in cartons)
                    {
                        EnsureCartonQualifies(carton, model.QualificationRules, true);
                        // If any carton does not qualify, we do not check other cartons
                        if (!ModelState.IsValid)
                        {
                            // TC5:Scan a pallet containg a non qualifying carton
                            return AdaptiveResult(202, null, model);
                        }
                    }


                    var nUpdated = 0; // Number of cartons updated
                    using (var trans = _service.BeginTransaction())
                    {
                        foreach (var carton in cartons)
                        {
                            var bUpdated = _service.UpdateMoveCarton(carton, Map(model.UpdatingRules), model.UpdateFlags,
                                                                     model.UpdatingRules.ReasonCode);
                            if (bUpdated)
                            {
                                ++nUpdated;
                            }
                        }
                        trans.Commit();
                    }
                    if (nUpdated == cartons.Count())
                    {
                        // All cartons were updated
                        model.StatusMessages.Add(string.Format("{0} cartons updated.", nUpdated));
                    }
                    else
                    {
                        model.StatusMessages.Add(string.Format("{0} of {1} cartons needed updating", nUpdated,
                                                               cartons.Count()));
                    }
                    this.Response.AppendHeader("CartonCount", nUpdated.ToString());
                }
                else
                {
                    // TC6:Carton has been scanned
                    var currentCarton = _service.GetCarton(model.ScanText);
                    if (currentCarton == null)
                    {
                        // TC7:Scan invalid carton
                        ModelState.AddModelError("", string.Format("No such carton : {0}", model.ScanText));
                        return AdaptiveResult(202, null, model);
                    }

                    EnsureCartonQualifies(currentCarton, model.QualificationRules, false);
                    if (!ModelState.IsValid)
                    {
                        // TC8:Scan a non qualifying carton
                        return AdaptiveResult(202, null, model);
                    }
                    //Using transaction, because possibly UpdateCarton() or moveCarton() method can fail. In this case either process should not be completed. 
                    using (var trans = _service.BeginTransaction())
                    {
                        var bUpdated = _service.UpdateMoveCarton(currentCarton, Map(model.UpdatingRules),
                                                                 model.UpdateFlags,
                                                                 model.UpdatingRules.ReasonCode);
                        if (bUpdated)
                        {
                            model.StatusMessages.Add(string.Format("Carton# {0} with SKU {1} updated.",
                                                                   currentCarton.CartonId,
                                                                   currentCarton.SkuInCarton.ToString()));
                            this.Response.AppendHeader("CartonCount", "1");
                        }
                        else
                        {
                            this.Response.AppendHeader("CartonCount", "0");
                        }
                        trans.Commit();
                    }
                }

                //Add seleted printer in cookie
                var cookiePrinter = new HttpCookie(COOKIE_PRINTER);

                if (!string.IsNullOrEmpty(model.PrinterId))
                {
                    cookiePrinter.Values.Add(COOKIE_SUB_PRINTERID, model.PrinterId);
                    cookiePrinter.Expires = DateTime.Now.AddDays(15);
                    this.HttpContext.Response.Cookies.Add(cookiePrinter);
                }
                // Giving printing message in success case when user select printer.
                if (!string.IsNullOrEmpty(model.PrinterId))
                {
                    _service.PrintCartonTicket(model.ScanText, model.PrinterId);
                    model.StatusMessages.Add(string.Format("Carton printed on '{0}' printer successfully.",
                                                           model.PrinterId));
                }
                model.OnCartonUpdated(_service);
                return AdaptiveResult(202, null, model);
            }
            catch (Exception ex)
            {
                // Simulate the behavior of the obsolete HandleAjaxError attribute
                this.Response.StatusCode = 203;
                return Content(ex.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="carton"></param>
        /// <param name="rules"></param>
        /// <param name="isPallet"></param>
        private void EnsureCartonQualifies(Carton carton, CartonModel rules, bool isPallet)
        {
            string msg;
            if (!string.IsNullOrEmpty(rules.SkuBarCode))
            {
                // Cache the result of this query for optimization
                var sku = _service.GetSku(rules.SkuBarCode);
                if (sku == null)
                {
                    ModelState.AddModelError("", string.Format("Bad SKU {0} passed to update carton", rules.SkuBarCode));
                }
                else
                {
                    if (carton.SkuInCarton == null || carton.SkuInCarton.SkuId != sku.SkuId)
                    {
                        // Displays style color etc of SKUId needed by rule
                        msg = string.Format("Qualifying SKU:{0}, SKU in Carton : {1}", string.Format("{0}/{1}/{2}/{3}", sku.Style, sku.Color, sku.Dimension, sku.SkuSize), carton.SkuInCarton == null ? "No SKU" : carton.SkuInCarton.ToString());
                        ModelState.AddModelError("", msg);
                    }
                }
            }
            if (!string.IsNullOrEmpty(rules.QualityCode) && carton.QualityCode != rules.QualityCode)
            {
                msg = string.Format("Qualifying quality: {0}, carton quality {1} ", rules.QualityCode, carton.QualityCode);
                ModelState.AddModelError("", msg);
            }
            if (!string.IsNullOrEmpty(rules.VwhId) && carton.VwhId != rules.VwhId)
            {
                msg = string.Format("Qualifying Virtual Warehouse {0}, carton's virtual warehouse {1}", rules.VwhId, carton.VwhId);
                ModelState.AddModelError("", msg);

            }
            if (rules.Pieces != null && carton.Pieces != rules.Pieces)
            {
                msg = string.Format("Qualifying pieces {0}, pieces in carton {1} ", rules.Pieces, carton.Pieces);
                ModelState.AddModelError("", msg);
            }
            if (rules.AreaId != null && carton.CartonArea.AreaId != rules.AreaId)
            {
                var area = _service.GetCartonArea(rules.AreaId);
                msg = string.Format("Scanned Carton {0} does not belongs to area {1} ", carton.CartonId, area.ShortName);
                ModelState.AddModelError("", msg);
            }
            if (rules.PriceSeasonCode != null && carton.PriceSeasonCode != rules.PriceSeasonCode)
            {
                msg = string.Format("Qualifying price season code is {0}, but carton contains price season code {1}.", rules.PriceSeasonCode, carton.PriceSeasonCode);
                ModelState.AddModelError("", msg);
            }
            if (rules.Rework == ReworkStatus.DoesNotNeedRework && carton.RemarkWorkNeeded)
            {
                if (isPallet)
                {
                    msg = string.Format("Pallet does not qualify because at least one of carton has been marked for rework for e.g. {0}. ", carton.CartonId);
                    ModelState.AddModelError("", msg);
                }
                else
                {

                    msg = "Carton does not qualify because it has been marked for rework.";
                    ModelState.AddModelError("", msg);
                }
            }
            else if (rules.Rework == ReworkStatus.NeedsRework && carton.RemarkWorkNeeded == false)
            {
                if (isPallet)
                {
                    msg = string.Format("Pallet does not qualify because at least one of carton does not need rework for e.g. {0}.", carton.CartonId);
                    ModelState.AddModelError("", msg);
                }
                else
                {
                    msg = "Carton does not qualify because it does not need rework.";
                    ModelState.AddModelError("", msg);
                }
            }
        }


        /// <summary>
        /// Validates destination pallet. Called to show info of destination pallet.
        /// </summary>
        /// <param name="palletId"></param>
        /// <remarks>
        /// Must post: PalletId.
        /// Success : Data contains a one line readable status of the pallet.
        /// </remarks>
        public virtual ActionResult HandleDestinationPallet(string palletId)
        {
            // Information of destination pallet on which cartons are placed.
            var pallet = _service.GetPallet(palletId.ToUpper());
            // Give message in spPalletInfo. 
            string msg;
            if (pallet.CartonCount == 0)
            {
                msg = string.Format("Pallet {0} is a new pallet", pallet.PalletId);
            }
            else if (pallet.CartonAreaCount == 1)
            {
                msg = string.Format("Pallet {0} already has {1} cartons in area {2}", pallet.PalletId, pallet.CartonCount, pallet.MinShortName);
            }
            else
            {
                msg = string.Format("Pallet {0} has {1} cartons, belonging to multiple areas, e.g. {2},{3}", pallet.PalletId, pallet.CartonCount, pallet.MinShortName, pallet.MaxShortName);
            }
            return Content(msg);
        }


        /// <summary>
        /// Handles destination Area for Mobile view. Only non numbered areas qualify.
        /// </summary>
        /// <param name="model"></param>
        /// Must Post: AreaId
        /// <returns>view DestinationBuildingForMobile.mobile.cshtml if area belongs to multiple building. Else return view DestinationPallet_Mobile.cshtml.</returns>
        /// <remarks>
        /// <para>
        /// UI ask for area to place carton or pallet. On successful scan, next Ui ask for destination Pallet or Destination building.
        /// </para>
        /// </remarks>
        public virtual ActionResult HandleDestinationAreaForMobile(DestinationAreaForMobileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                //TC1: Scan nothing.
                return View(Views.DestinationArea_mobile, model);
            }
            var areas = _service.GetCartonAreas(model.AreaShortName, null).ToArray();
            if (!areas.Any())
            {
                //TC2: Scanned invalid shortAreaname.
                ModelState.AddModelError("", string.Format("No such Area {0}", model.AreaShortName));
                return View(Views.DestinationArea_mobile, model);
            }
            if (areas.Any(p => p.IsNumberedLocationArea))
            {
                //Tc3: Scanned a non numbered area.
                ModelState.AddModelError("", "Please scan a non numbered Area");
                return View(Views.DestinationArea_mobile, model);
            }
            var buildings = areas.Where(p => !string.IsNullOrWhiteSpace(p.Building)).Select(p => p.Building).ToArray();
            if (buildings.Count() > 1)
            {
                // TC4: If multiple building found for passed areaShorName, ask user to enter one of building as suggested by system.
                return View(Views.DestinationBuilding_mobile, new DestinationBuildingForMobileViewModel { BuildingList = buildings, AreaShortName = model.AreaShortName });
            }
            var dpmv = new DestinationPalletForMobileViewModel { AreaId = areas.Select(p => p.AreaId).FirstOrDefault(), ShortName = model.AreaShortName };
            //If areaShortname does not belong to any building we will treated shortname as areaId.
            if (!buildings.Any())
            {
                dpmv.AreaId = model.AreaShortName;
                return View(Views.DestinationPallet_mobile, dpmv);
            }
            //TC5: If only one building found, we will skip to scan building, ask user to scan pallet directly.
            dpmv.BuildingId = buildings.FirstOrDefault();
            return View(Views.DestinationPallet_mobile, dpmv);
        }


        /// <summary>
        /// Handle destination building where pallet has to move.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public virtual ActionResult HandleDestinationBuildingForMobile(DestinationBuildingForMobileViewModel model)
        {
            model.BuildingList = _service.GetCartonAreas(model.AreaShortName, null).Select(p => p.Building);
            if (!ModelState.IsValid)
            {
                //TC6: Nothing scanned.
                return View(Views.DestinationBuilding_mobile, model);
            }
            var result = _service.GetCartonAreas(model.AreaShortName, model.BuildingId);
            if (result == null || !result.Any())
            {
                //TC7: Invalid building or for scanned shortName and building, no area found. 
                ModelState.AddModelError("", string.Format("{0} is not a valid Building for Area {1}", model.BuildingId, model.AreaShortName));
                return View(Views.DestinationBuilding_mobile, model);
            }
            var dpmv = new DestinationPalletForMobileViewModel { AreaId = result.FirstOrDefault().AreaId, ShortName = model.AreaShortName, BuildingId = model.BuildingId };
            return View(Views.DestinationPallet_mobile, dpmv);
        }



        /// <summary>
        /// Handles  destination pallet scan for mobile view.
        /// </summary>
        /// <param name="model"></param>
        /// Must Post:PalletId,AreaId
        /// <returns>view Palletize.mobile.cshtml</returns>
        /// <remarks>
        /// <para>
        /// UI ask for destination Pallet.On successfull scan Pallet info is shown and carton or pallet to be palletized is asked for scanning. 
        /// </para>
        /// </remarks>
        public virtual ActionResult HandleDestinationPalletForMobile(DestinationPalletForMobileViewModel model)
        {
            if (!ModelState.IsValid || string.IsNullOrWhiteSpace(model.PalletId))
            {
                return View(Views.DestinationPallet_mobile, model);
            }
            var pallet = _service.GetPallet(model.PalletId);
            string msg;
            if (pallet.CartonCount == 0)
            {
                msg = string.Format("Pallet {0} is a new pallet", model.PalletId);
            }
            else if (pallet.CartonAreaCount == 1)
            {
                msg = string.Format("Pallet {0} already has {1} cartons in area {2}", pallet.PalletId, pallet.CartonCount, pallet.MinShortName);
            }
            else
            {
                msg = string.Format("Pallet {0} has {1} cartons belonging to multiple areas, e.g. {2},{3}", pallet.PalletId, pallet.CartonCount, pallet.MinShortName, pallet.MaxShortName);
            }
            AddStatusMessage(msg);
            var pvm = new PalletizeMobileViewModel();
            pvm.UpdatingRules.PalletId = model.PalletId;
            pvm.UpdatingRules.AreaId = model.AreaId;
            pvm.BuidingId = model.BuildingId;
            pvm.AreaShorName = model.ShortName;
            return View(Views.Palletize_mobile, pvm);

        }

        #region Carton Editor
        /// <summary>
        /// Name of cookie which stores area info while removing irregular and sample..
        /// </summary>
        private const string COOKIE_AREA = "Cookie_Area";

        private const string COOKIE_IRREGULAR_AREA = "irregularArea";

        private const string COOKIE_SAMPLE_AREA = "sampleArea";

        /// <summary>
        /// This UI ask for carton to update
        /// </summary>
        /// <returns>View CartonScan.cshtml</returns>
        [AuthorizeEx("Carton Editor requires Role {0}", Roles = "SRC_CED", Purpose = "Enables editing of carton properties")]
        [Route(HomeController.ActionNameConstants.CartonEditorIndex, Name = DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_CartonEditor)]
        public virtual ActionResult CartonEditorIndex()
        {
            return View(Views.CartonEditor, new CartonEditorViewModel());
        }

        /// <summary>
        /// THis is the public route for exiting pased carton
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Route("Edit", Name = DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_EditCarton1)]
        [AuthorizeEx("Carton Editor requires Role {0}", Roles = "SRC_CED", Purpose = "Enables editing of carton properties")]
        public virtual ActionResult Edit(string id)
        {
            //throw new NotImplementedException();
            var model = new CartonEditorViewModel
            {
                ScanText = id
            };

            return CartonEditor(model);
        }

        /// <summary>
        /// Shows properties of carton
        /// </summary>
        /// <param name="model"></param>
        /// Must Post model.cartonId
        /// <returns>View CartonEditor.cshtml</returns>
        [AuthorizeEx("Carton Editor requires Role {0}", Roles = "SRC_CED", Purpose = "Enables editing of carton properties")]
        public virtual ActionResult CartonEditor(CartonEditorViewModel model)
        {
            ModelState.Clear();     // No errors matter
            if (string.IsNullOrEmpty(model.ScanText))
            {
                // User forgot to enter carton id?
                ModelState.AddModelError("", "Please scan carton");
                return View(Views.CartonEditor, model);
            }
            var cartonId = model.ScanText;
            model.OnViewExecuting(_service, this.ControllerContext);

            if (model.StatusMessages.Count > 0)
            {
                // This can only mean that the carton is invalid
                ModelState.AddModelError("", string.Format("Invalid Carton {0}", cartonId));
            }
            //Read value of area from cookie.
            if (Request.Cookies[COOKIE_AREA] != null)
            {
                model.IrregularAreaId = Request.Cookies[COOKIE_AREA][COOKIE_IRREGULAR_AREA];
                model.SamplesAreaId = Request.Cookies[COOKIE_AREA][COOKIE_SAMPLE_AREA];
            }

            if (Request.Cookies[COOKIE_PRINTER] != null)
            {
                model.PrinterId = Request.Cookies[COOKIE_PRINTER][COOKIE_SUB_PRINTERID];
            }

            return View(Views.CartonEditor, model);
        }


        /// <summary>
        /// Remove irregular and sample pieces in carton
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [AuthorizeEx("Carton Editor requires Role {0}", Roles = "SRC_CED", Purpose = "Enables editing of carton properties")]
        public virtual ActionResult RemovePieces(CartonEditorViewModel model)
        {
            if (model.PiecesFlag == PiecesRemoveFlag.Irregular && model.IrregularPieces == null || model.IrregularPieces <= 0 || model.IrregularPieces > 999)
            {
                ModelState.AddModelError("", "Pieces to transfer must be between 1 and 999");
                return AdaptiveResult(null, null, model);
            }
            if (model.PiecesFlag == PiecesRemoveFlag.Samples && model.SamplePieces == null || model.SamplePieces <= 0 || model.SamplePieces > 999)
            {
                ModelState.AddModelError("", "Pieces to transfer must be between 1 and 999");
                return AdaptiveResult(null, null, model);
            }
            if (model.PiecesFlag == PiecesRemoveFlag.Irregular && model.IrregularAreaId == null)
            {
                ModelState.AddModelError("", "Please provide area to transfer irregular pieces");
                return AdaptiveResult(null, null, model);
            }
            if (model.PiecesFlag == PiecesRemoveFlag.Samples && model.SamplesAreaId == null)
            {
                ModelState.AddModelError("", "Please provide area to transfer sample pieces");
                return AdaptiveResult(null, null, model);
            }
            if ((model.PiecesFlag == PiecesRemoveFlag.Irregular || model.PiecesFlag == PiecesRemoveFlag.Samples) && (model.IrregularPieces > model.UpdatingRules.Pieces || model.SamplePieces > model.UpdatingRules.Pieces))
            {
                ModelState.AddModelError("", "Pieces to transfer must not exceed pieces in carton");
                return AdaptiveResult(null, null, model);
            }


            // Create cookie to add area for removing irregular and sample pieces.
            var cookieArea = new HttpCookie(COOKIE_AREA);

            if (!string.IsNullOrEmpty(model.IrregularAreaId) || !string.IsNullOrEmpty(model.SamplesAreaId))
            {
                cookieArea.Values.Add(COOKIE_IRREGULAR_AREA, model.IrregularAreaId);
                cookieArea.Values.Add(COOKIE_SAMPLE_AREA, model.SamplesAreaId);
                cookieArea.Expires = DateTime.Now.AddDays(15);
                this.HttpContext.Response.Cookies.Add(cookieArea);
            }

            //Create cookie to add selected printer
            var cookiePrinter = new HttpCookie(COOKIE_PRINTER);
            if (!string.IsNullOrEmpty(model.PrinterId))
            {
                cookiePrinter.Values.Add(COOKIE_SUB_PRINTERID, model.PrinterId);
                cookiePrinter.Expires = DateTime.Now.AddDays(15);
                this.HttpContext.Response.Cookies.Add(cookiePrinter);

                //Print carton ticket
                _service.PrintCartonTicket(model.ScanText, model.PrinterId);
                model.StatusMessages.Add(string.Format("Carton printed on '{0}' printer successfully.", model.PrinterId));
            }
            if (model.PiecesFlag == PiecesRemoveFlag.Irregular)
            {
                _service.RemoveIrregularSamples(model.ScanText, model.BundleId, model.IrregularAreaId, model.IrregularPieces, null);
                model.StatusMessages.Add(string.Format("{0} Pieces transferred to {1} area ", model.IrregularPieces, model.IrregularAreaId));
            }
            if (model.PiecesFlag == PiecesRemoveFlag.Samples)
            {
                var reasonCode = _service.GetSampleReasonCode();
                _service.RemoveIrregularSamples(model.ScanText, model.BundleId, model.SamplesAreaId, model.SamplePieces, reasonCode);
                model.StatusMessages.Add(string.Format("{0} Pieces transferred to {1} area ", model.SamplePieces, model.SamplesAreaId));
            }
            return AdaptiveResult(null, null, model);
        }

        #endregion

        /// <summary>
        /// For Tutorial View.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public virtual ActionResult Tutorial()
        {
            return View(Views.Tutorial);
        }

        /// <summary>
        /// If we get an unknown action while posting, it means that the user has just been redirected from a login page. The return URL
        /// will not accept the redirect since it is POST only. So we redirect to the Box Manager Home Page.
        /// </summary>
        /// <param name="actionName">actionName</param>
        /// <example>
        ///  ~/DcmsMobile2011/BoxManager/Home/HandleScan?UiType=ScanToPallet
        /// </example>
        protected override void HandleUnknownAction(string actionName)
        {
            ActionResult result = null;
            // Is this a valid action ?
            var methods = this.GetType().GetMethods().Where(p => p.Name == actionName).ToArray();
            if (methods.Length == 0)
            {
                // This action no longer exists. Does the user have a book mark which is now broken?
                AddStatusMessage("The page you requested was not found. You have been redirected to the main page.");
                result = RedirectToActionPermanent(this.Actions.Index());
            }
            else
            {

                if (this.HttpContext.Request.HttpMethod == "GET")
                {
                    var attrPost = methods.SelectMany(p => p.GetCustomAttributes(typeof(HttpPostAttribute), true)).FirstOrDefault();
                    if (attrPost != null)
                    {
                        // GET request for an action which requires POST. Assume that the user has been redirected from the login screen
                        AddStatusMessage("Please start again.");
                        result = RedirectToAction(this.Actions.Index());
                    }
                }
            }

            if (result == null)
            {
                // We really don't know what to do. Let base class handle it
                base.HandleUnknownAction(actionName);
            }
            else
            {
                result.ExecuteResult(this.ControllerContext);
            }


        }
    }
}

//$Id$