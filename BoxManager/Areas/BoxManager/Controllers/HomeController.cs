using DcmsMobile.BoxManager.Helpers;
using DcmsMobile.BoxManager.Repository;
using DcmsMobile.BoxManager.ViewModels;
using DcmsMobile.BoxManager.ViewModels.Home;
using EclipseLibrary.Mvc.Controllers;
using System;
using System.Configuration;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using System.Web.Routing;


//Reviewed By: MBisht and Deepak Bhatt 11 June 2012
namespace DcmsMobile.BoxManager.Areas.BoxManager.Controllers
{
    [RouteArea("BoxManager")]
    [RoutePrefix(HomeController.NameConst)]
    public partial class HomeController : EclipseController
    {
        #region Initialize

        /// <summary>
        /// Required by T4MVC
        /// </summary>
        public HomeController()
        {

        }

        private BoxManagerService _service;

        /// <summary>
        /// For service injection through unit tests
        /// </summary>
        public BoxManagerService Service
        {
            get { return _service; }

            set { _service = value; }
        }

        /// <summary>
        /// If the Action takes a ViewModelBase derived model as a parameter, then the action has a choice of using this value or model.Ui.
        /// The  recommendation is that this value should always be used within the controller in preference to model.Ui.
        /// </summary>
        private UiType _uiType;


        protected override void Initialize(RequestContext requestContext)
        {
            base.Initialize(requestContext);
            var str = requestContext.HttpContext.Request.Params[AuthorizeExUiAttribute.NAME_UITYPE];
            if (!string.IsNullOrWhiteSpace(str))
            {
                _uiType = (UiType)Enum.Parse(typeof(UiType), str);
            }

            if (_service == null)
            {
                var connectString = ConfigurationManager.ConnectionStrings["dcms8"].ConnectionString;
                var userName = requestContext.HttpContext.SkipAuthorization ? string.Empty : requestContext.HttpContext.User.Identity.Name;
                var clientInfo = string.IsNullOrEmpty(requestContext.HttpContext.Request.UserHostName) ? requestContext.HttpContext.Request.UserHostAddress :
                    requestContext.HttpContext.Request.UserHostName;
                var moduleCode = _uiType == UiType.Vas ? "V2P" : "ScanToPallet";
                _service = new BoxManagerService(requestContext.HttpContext.Trace, connectString, userName, clientInfo, moduleCode);
            }
        }

        /// <summary>
        /// overriding RedirectToRoute() to handle the issue with T4MVC, 
        /// T4MVC internally calls RedirectToRoute() for RedirectToAction()
        /// </summary>
        /// <param name="routeName"></param>
        /// <param name="routeValues"></param>
        /// <returns></returns>
        protected override RedirectToRouteResult RedirectToRoute(string routeName, RouteValueDictionary routeValues)
        {
            routeValues.Add(AuthorizeExUiAttribute.NAME_UITYPE, _uiType);
            return base.RedirectToRoute(routeName, routeValues);
        }

        protected override void Dispose(bool disposing)
        {
            if (_service != null)
            {
                _service.Dispose();
            }
            base.Dispose(disposing);
        }

        protected override ViewResult View(string viewName, string masterName, object model)
        {
            var vmb = model as ViewModelBase;
            if (vmb != null)
            {
                // Tutorial page does not use a derived model
                vmb.QueryCount = _service.QueryCount;
            }
            ViewData[AuthorizeExUiAttribute.NAME_UITYPE] = _uiType;
            return base.View(viewName, masterName, model);
        }

        #endregion

        #region Index

        public virtual ActionResult Index()
        {
            return View(this.Views.Index);
        }

        #endregion

        private readonly static Regex __regexPallet = new Regex(@"^([P|p]\S{1,19}$)");
        //private readonly static Regex __regLocation = new Regex(@"^911\w{5}$");
        private readonly static Regex __regexUcc = new Regex(@"^0000\d{16}$");

        #region Staging Pallet
        [AuthorizeExUi("Scan To Pallet requires role {0}", UiType.ScanToPallet, Roles = "DCMS8_SCANTOPALLET")]
        [AuthorizeExUi("VAS To Pallet requires role {0}", UiType.Vas, Roles = "DCMS8_VASTOPALLET")]
        [Route(HomeController.ActionNameConstants.CreatingPalletIndex, Name = DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_ScanToPallet)]
        public virtual ActionResult CreatingPalletIndex()
        {
            var model = new ScanToPalletViewModel();
            return View(Views.ScanToPallet, model);
        }

        /// <summary>
        /// Creates a pallet for VAS
        /// </summary>
        /// <returns></returns>
        [AuthorizeExUi("Scan To Pallet requires role {0}", UiType.ScanToPallet, Roles = "DCMS8_SCANTOPALLET")]
        [AuthorizeExUi("VAS To Pallet requires role {0}", UiType.Vas, Roles = "DCMS8_VASTOPALLET")]
        [Route(HomeController.ActionNameConstants.VasPallet, Name = DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_ApplyVasToBox)]
        public virtual ActionResult VasPallet()
        {
            _uiType = UiType.Vas;
            var model = new ScanToPalletViewModel();
            return View(Views.ScanToPallet, model);
        }

        /// <summary>
        /// Activates the passed pallet, if any. 
        /// Gets and sets sorting criteria.
        /// Retrieve pallet suggestions along with remaining boxes for the criteria pallet which is getting activated.
        /// Ensures that the same criteria boxes are on the pallet. 
        /// </summary>
        /// <returns>ScanToPallet view</returns>
        /// <remarks>
        /// After each scanned box, control gets redirected here.
        /// </remarks>
        [HttpGet]
        [AuthorizeExUi("Scan To Pallet requires role {0}", UiType.ScanToPallet, Roles = "DCMS8_SCANTOPALLET")]
        [AuthorizeExUi("VAS To Pallet requires role {0}", UiType.Vas, Roles = "DCMS8_VASTOPALLET")]
        public virtual ActionResult ActivatePallet(string palletId, string lastScanText, Sound sound = Sound.None, string confirmPalletScan = null)
        {
            var model = new ScanToPalletViewModel
                {
                    PalletLimit = _service.GetPalletVolumeLimit(),
                    Sound = (char)sound,
                    PalletId = palletId,
                    LastBoxId = lastScanText,
                    ConfirmScanText = confirmPalletScan
                };
            //TC1: When a box is scanned who is not on any pallet.
            if (string.IsNullOrEmpty(palletId))
            {
                return View(this.Views.ScanToPallet, model);
            }
            var boxes = _service.GetBoxesOfPallet(palletId).ToArray();
            model.TotalBoxVolume = boxes.Sum(p => p.Volume);
            model.CountBoxesOnPallet = boxes.Count();
            model.PalletLocationList = string.Join(",", boxes.Select(p => p.LocationId).Distinct());
            model.PalletAreaList = string.Join(",", boxes.Select(p => p.IaId).Distinct());
            //TC2: When box(es) count is equal to zero.
            if (!boxes.Any())
            {
                AddStatusMessage(string.Format("Start scanning boxes to keep them on {0}.", palletId));
                return View(this.Views.ScanToPallet, model);
            }
            else
            {
                //Validate boxes of pallet.
                foreach (var box in boxes)
                {
                    //TC3: Boxes are not valid.[i.e box is not verified or shipped].
                    if (!TryValidateModel(new BoxModel(box)) || (_uiType == UiType.ScanToPallet && box.VerifyDate == null)
                        || (_uiType == UiType.ScanToPallet && !string.IsNullOrWhiteSpace(box.SmallShipmentFlag)))
                    {
                        if (_uiType == UiType.ScanToPallet && box.VerifyDate == null)
                        {
                            ModelState.AddModelError("box.VerifyDate", string.Format("Boxes of pallet {0} have not been verified yet.", palletId));
                        }
                        if (_uiType == UiType.ScanToPallet && !string.IsNullOrWhiteSpace(box.SmallShipmentFlag))
                        {
                            ModelState.AddModelError("box.SmallShipmentFlag", "Boxes of small shipments cannot be palletized.");
                        }
                        // Boxes are not appropriate for palletizing. Not Verified, shipped, etc.
                        return View(this.Views.ScanToPallet, new ScanToPalletViewModel());
                    }
                }
            }

            SortCriteria sortCriteria;
            try
            {
                sortCriteria = _service.EnsureCriteriaPure(boxes, _uiType == UiType.Vas);
            }
            catch (BoxManagerServiceException ex)
            {
                switch (ex.ErrorCode)
                {
                    case BoxManagerServiceErrorCode.MultipleBucketPallet:
                        ModelState.AddModelError("", string.Format("Pallet {0} does not satisfy criteria because it contains boxes of multiple buckets {1}.",
                            model.PalletId, ex.Data["Data"]));
                        break;

                    case BoxManagerServiceErrorCode.MultipleCustomerPallet:
                        ModelState.AddModelError("", string.Format("Pallet {0} does not satisfy criteria because it contains boxes of multiple customer {1}.",
                            model.PalletId, ex.Data["Data"]));
                        break;

                    case BoxManagerServiceErrorCode.MultipleDcPallet:
                        ModelState.AddModelError("", string.Format("Pallet {0} does not satisfy criteria because it contains boxes of multiple Dc {1}.",
                             model.PalletId, ex.Data["Data"]));
                        break;
                    case BoxManagerServiceErrorCode.MultiplePoPallet:
                        ModelState.AddModelError("", string.Format("Pallet {0} does not satisfy criteria because it contains boxes of multiple Po {1}.",
                            model.PalletId, ex.Data["Data"]));
                        break;
                }
                return View(this.Views.ScanToPallet, new ScanToPalletViewModel());
            }

            var firstBox = boxes.First();
            //Set sorting criteria 
            model.CustomerId = firstBox.CustomerId;

            //TC4: if the AllowPoMixing  that are set in flag are also set in the HomeController class.
            if (!sortCriteria.HasFlag(SortCriteria.AllowPoMixing))
            {
                model.PoId = firstBox.PoId;
            }
            //TC5: if the AllowCustomerDcMixing  that are set in flag are also set in the HomeController class.
            if (!sortCriteria.HasFlag(SortCriteria.AllowCustomerDcMixing))
            {
                model.CustomerDcId = firstBox.CustomerDcId;
            }
            //TC6: if the AllowBucketMixing that are set in flag are also set in the HomeController class.
            if (!sortCriteria.HasFlag(SortCriteria.AllowBucketMixing))
            {
                model.BucketId = firstBox.BucketId;
            }
            model.PalletSuggestionList = _service.SuggestPallets(model.PalletId, model.CustomerId, model.BucketId, model.PoId, model.CustomerDcId, _uiType == UiType.Vas).Select(p => new PalletModel(p)).ToList();

            if (_uiType == UiType.Vas)
            {
                model.QualifyingBoxCount = _service.GetQualifyingBoxCountForVas(model.CustomerId);
            }
            else
            {
                model.QualifyingBoxCount = _service.GetQualifyingBoxCount(model.CustomerId, model.PoId, model.CustomerDcId, model.BucketId);
            }
            return View(this.Views.ScanToPallet, model);
        }

        /// <summary>
        /// The posted ScanText is ucc1281d. 
        /// We try to keep every scanned box on the same temporary pallet. If the criteria does not match
        /// we ask user to scan a new pallet and transfer the temp pallet contents to the scanned pallet. 
        /// If a box is scanned which is already on some pallet, we treat it as a pallet scan.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <remarks>
        /// Posted values expected:
        ///   model.ScanText: The box scanned by the user
        ///   model.PalletId: The pallet on which we wish to place the box
        ///   model.CustomerId, PoId, CustomerDcId, BucketId: The the box must qualify this criteria before we allow it to be placed on the pallet
        /// </remarks>
        private ActionResult DoHandleBoxScan(ScanToPalletViewModel model)
        {
            //TC7: When model state is not valid.
            if (!ModelState.IsValid)
            {
                return View(Views.ScanToPallet, model);
            }

            var box = _service.GetBox(model.ScanText, _uiType == UiType.Vas);

            //TC8: If any invalid box is scanned.[For eg: not verified,shipped]
            if (box == null)
            {
                // Not a valid box
                ModelState.AddModelError("", string.Format("Box {0} does not exist.", model.ScanText));
                return RedirectToAction(this.Actions.ActivatePallet(model.PalletId, model.ScanText, Sound.Error));
            }

            //TC9: if box is not marked for VAS and trying to palletize for VAS
            if (_uiType == UiType.Vas)
            {
                if (!box.IsVasRequired)
                {
                    ModelState.AddModelError("box.IsVasRequired", string.Format("VAS is not required for the box {0}.", box.Ucc128Id));
                    return RedirectToAction(this.Actions.ActivatePallet(model.PalletId, model.ScanText, Sound.Error));
                }
                if (box.IsVasCompleted)
                {
                    AddStatusMessage(string.Format("VAS is already completed on box {0}.", model.ScanText));
                    return RedirectToAction(this.Actions.ActivatePallet(model.PalletId, model.ScanText, Sound.Error));
                }
                if (string.Compare(box.IaId, _service.GetBadPitchArea()) == 0)
                {
                    ModelState.AddModelError("", "Scanned box is not valid for VAS as it is not pitched successfully.");
                    return RedirectToAction(this.Actions.ActivatePallet(model.PalletId, model.ScanText, Sound.Error));
                }
            }

            //Handling those Boxes which are already on pallet, that was assigned by any other module
            if (box.ScanToPalletDate == null)
            {
                //If box is on any pallet which was not assigned by ScanToPallet, we will not entertain here.
                box.PalletId = null;
            }

            //TC10: Box is not valid.[i.e box is not verified or shipped].
            if (!TryValidateModel(new BoxModel(box)) || (_uiType == UiType.ScanToPallet && box.VerifyDate == null)
                || (_uiType == UiType.ScanToPallet && !string.IsNullOrWhiteSpace(box.SmallShipmentFlag)))
            {
                if (_uiType == UiType.ScanToPallet && box.VerifyDate == null)
                {
                    ModelState.AddModelError("box.VerifyDate", "Box has not been verified yet.");
                }
                if (_uiType == UiType.ScanToPallet && !string.IsNullOrWhiteSpace(box.SmallShipmentFlag))
                {
                    ModelState.AddModelError("box.SmallShipmentFlag", "Boxes of small shipments cannot be palletized.");
                }
                // Box is not appropriate for palletizing. Not Verified, shipped, etc.
                return RedirectToAction(this.Actions.ActivatePallet(model.PalletId, model.ScanText, Sound.Error));
            }
            // Redirecting to activate Pallet since this box is already on a pallet and currently no pallet is active. 
            //In case if there is an active pallet and the scanned box is on a pallet then box will leave its old pallet 
            //and become part of the active pallet.
            //TODO:If the active pallet is same as the box pallet then no need to try to put box on pallet again. Due to this qualifying box count is also getting disturbed (MBisht 6 July 2012)
            //TC8: when box pallet is not null and pallet passed as null from the model.
            if (!string.IsNullOrEmpty(box.PalletId) && string.IsNullOrEmpty(model.PalletId))
            {
                this.AddStatusMessage(string.Format("This box is already on pallet {0}. Start scanning boxes to keep them on this pallet.", box.PalletId));
                return RedirectToAction(this.Actions.ActivatePallet(box.PalletId, model.ScanText));
            }
            //TC9: When either bucket or customer distribution centre or purchase order or customer of box and model are not equal. 
            if (!WildEqual(box.BucketId, model.BucketId) || !WildEqual(box.CustomerDcId, model.CustomerDcId) ||
                !WildEqual(box.PoId, model.PoId) || !WildEqual(box.CustomerId, model.CustomerId))
            {
                ModelState.AddModelError("", "Criteria mismatch, if you have a staging pallet then scan a new or suggested pallet and put the current boxes on it.");
                return RedirectToAction(this.Actions.ActivatePallet(model.PalletId, model.ScanText, Sound.Error));
            }

            model.PalletId = _service.PutBoxOnPallet(model.PalletId, model.ScanText, _uiType == UiType.Vas);

            if (_uiType == UiType.Vas)
            {
                //Here box required VAS, Mark VAS completed for the scanned Box.
                _service.MarkVasComplete(model.ScanText);
                AddStatusMessage(string.Format("VAS completed on box {0}.", model.ScanText));
            }

            // Sharad 6 Jul 2012: Not redirecting to optimize performance on ring scanners
            //return RedirectToAction(this.Actions.ActivatePallet(model.PalletId, model.ScanText));
            return ActivatePallet(model.PalletId, model.ScanText);
        }

        /// <summary>
        /// null value is treated as a wild card and matches everything
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="val1"></param>
        /// <param name="val2"></param>
        /// <returns>boolean: TRUE if val1 is same as val2 else return FALSE</returns>
        private bool WildEqual<T>(T val1, T val2)
        {
            return val1 == null || val2 == null || val1.Equals(val2);
        }

        /// <summary>
        /// The contents of the current pallet will be transferred to the scanned pallet after proper checks and balances.
        /// </summary>
        /// <param name="scanText"></param>
        /// <param name="palletId"></param>
        /// <param name="confirmPalletScan"></param>
        /// <returns>scan to pallet view</returns>
        /// <remarks>
        /// Posted values expected: scanText, palletId, confirmPalletScan
        /// </remarks>
        private ActionResult DoHandlePalletScan(string scanText, string palletId, string confirmPalletScan)
        {
            //TC11: When scan text is empty.
            if (string.IsNullOrEmpty(scanText))
            {
                throw new ArgumentNullException("model.ScanText");
            }
            //TC12: When there is currently no active pallet.
            if (string.IsNullOrEmpty(palletId))
            {
                // There is no pallet which is currently active.
                //redirect to ActivatePallet
                return RedirectToAction(this.Actions.ActivatePallet(scanText, null));
            }

            //We try to transfer the staging pallet to the scanned pallet. 
            try
            {
                var boxes = _service.GetBoxesOfPallet(scanText);
                //Validate boxes of pallet.
                foreach (var box in boxes)
                {
                    //TC3: Boxes are not valid.[i.e box is not verified or shipped].
                    if (!TryValidateModel(new BoxModel(box)) || (_uiType == UiType.ScanToPallet && box.VerifyDate == null)
                        || (_uiType == UiType.ScanToPallet && !string.IsNullOrWhiteSpace(box.SmallShipmentFlag)))
                    {
                        if (_uiType == UiType.ScanToPallet && box.VerifyDate == null)
                        {
                            ModelState.AddModelError("box.VerifyDate", string.Format("Boxes of pallet {0} have not been verified yet.", scanText));
                        }
                        if (_uiType == UiType.ScanToPallet && !string.IsNullOrWhiteSpace(box.SmallShipmentFlag))
                        {
                            ModelState.AddModelError("box.SmallShipmentFlag", "Boxes of small shipments cannot be palletized.");
                        }
                        // Boxes are not appropriate for palletizing. Not Verified, shipped, etc.                        
                        return RedirectToAction(this.Actions.ActivatePallet(palletId, scanText, Sound.Error));
                    }
                }
                if (confirmPalletScan == scanText)
                {
                    //TC13: when you rescan the pallet on which the boxes of another pallet are to be placed second time.
                    _service.MergePallets(palletId, scanText, _uiType == UiType.Vas);
                    this.AddStatusMessage(string.Format("Boxes of {0} placed on {1} successfully.", palletId, scanText));

                    //If pallets are merged successfully then we have to check if it was for VAS we should back to Creating Pallet page should not ask for Location 
                    if (_uiType == UiType.Vas)
                    {
                        return RedirectToAction(this.Actions.CreatingPalletIndex());
                    }
                    return RedirectToAction(this.Actions.Location(scanText));
                }
                decimal mergeVolume = _service.EnsureMergePallet(palletId, scanText, _uiType == UiType.Vas);
                confirmPalletScan = scanText;
                var limit = (int)(mergeVolume * 100 / _service.GetPalletVolumeLimit());
                AddStatusMessage(string.Format("Rescan {0} to place boxes of {1} on it. Pallet {0} will become {2}% full. ", confirmPalletScan, palletId, limit));
                return RedirectToAction(this.Actions.ActivatePallet(palletId, null, Sound.Warning, confirmPalletScan));
            }
            catch (BoxManagerServiceException ex)
            {
                switch (ex.ErrorCode)
                {

                    case BoxManagerServiceErrorCode.MergingPalletWithSelf:
                        ModelState.AddModelError("", string.Format("Pallets {0} and {1} could not be merged because they are same.", scanText, palletId));
                        break;

                    case BoxManagerServiceErrorCode.BothPalletEmpty:
                        ModelState.AddModelError("", string.Format("Pallets {0} and {1} could not be merged because they are empty.", scanText, palletId));
                        break;

                    case BoxManagerServiceErrorCode.MultipleAreaPallet:
                        ModelState.AddModelError("", string.Format("Pallet {0} contain boxes of more than one areas {1}.", scanText, ex.Data["Data"]));
                        break;

                    case BoxManagerServiceErrorCode.MultipleLocationPallet:
                        ModelState.AddModelError("", string.Format("Pallet {0} contain boxes of more than one locations {1}.", scanText, ex.Data["Data"]));
                        break;

                    case BoxManagerServiceErrorCode.MultipleBucketPallet:
                        ModelState.AddModelError("", string.Format("Pallets {0} and {1} could not be merged because they would contain boxes of multiple buckets {2}.",
                             scanText, palletId, ex.Data["Data"]));
                        break;

                    case BoxManagerServiceErrorCode.MultipleCustomerPallet:
                        ModelState.AddModelError("", string.Format("Pallets {0} and {1} could not be merged because they would contain boxes of multiple customers {2}.",
                             scanText, palletId, ex.Data["Data"]));
                        break;

                    case BoxManagerServiceErrorCode.MultipleDcPallet:
                        ModelState.AddModelError("", string.Format("Pallets {0} and {1} could not be merged because they would contain boxes of multiple DCs {2}.",
                            scanText, palletId, ex.Data["Data"]));
                        break;

                    case BoxManagerServiceErrorCode.MultiplePoPallet:
                        ModelState.AddModelError("", string.Format("Pallets {0} and {1} could not be merged because they would contain boxes of multiple POs {2}.",
                           scanText, palletId, ex.Data["Data"]));
                        break;
                }
                return RedirectToAction(this.Actions.ActivatePallet(palletId, null, Sound.Error));
            }
        }

        [Route(HomeController.ActionNameConstants.ApplyVasToBox,Name=DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_ApplyVasToBox1)]
        public virtual ActionResult ApplyVasToBox(string id)
        {
            _uiType = UiType.Vas;
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException();
            }

            var model = new ScanToPalletViewModel 
            {
                ScanText = id
            };
            return DoHandleBoxScan(model);
        }

        [Route(HomeController.ActionNameConstants.ScanToPallet, Name = DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_ScanToPallet1)]
        public virtual ActionResult ScanToPallet(string id)
        {
            _uiType = UiType.ScanToPallet;
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException();
            }

            var model = new ScanToPalletViewModel
            {
                ScanText = id
            };
            return DoHandleBoxScan(model);
        }     

        /// <summary>
        /// ScanText and PalletId must be posted.
        /// User can press enter to restart for creating a pallet.
        /// </summary>
        /// <param name="model"></param>
        /// <returns>scantopallet view model</returns>
        /// <remarks>
        /// This is the public action which receives whatever the user has scanned. It determines the type of scan and calls the appropriate
        /// private function.
        /// Values posted
        /// :ScanText
        /// :PalletId
        /// :ConfirmScanText
        /// </remarks>
        [HttpPost]
        [AuthorizeExUi("Scan To Pallet requires role {0}", UiType.ScanToPallet, Roles = "DCMS8_SCANTOPALLET")]
        [AuthorizeExUi("VAS To Pallet requires role {0}", UiType.Vas, Roles = "DCMS8_VASTOPALLET")]
        public virtual ActionResult HandleScan(ScanToPalletViewModel model)
        {
            //TC14: when box is not passed or we just press the spacebar[i.e creating whitespace] and press enter.
            if (string.IsNullOrWhiteSpace(model.ScanText))
            {
                return RedirectToAction(Actions.ActivatePallet(null, null));
                //return View(Views.ScanToPallet, new ScanToPalletViewModel());
            }
            //TC15: when the model state is not valid.
            if (!ModelState.IsValid)
            {
                return RedirectToAction(Actions.ActivatePallet(null, null));
                //return View(Views.ScanToPallet, model);
            }
            model.ScanText = model.ScanText.Trim();
            //TC16: If a valid box is passed.
            if (__regexUcc.IsMatch(model.ScanText))
            {
                return DoHandleBoxScan(model);
            }
            //TC17: If a valid pallet is passed[for eg starts with p.]
            if (__regexPallet.IsMatch(model.ScanText))
            {
                return DoHandlePalletScan(model.ScanText, model.PalletId, model.ConfirmScanText);

            }
            ModelState.AddModelError("", string.Format("Scan text {0} is not recognized.", model.ScanText));
            return RedirectToAction(this.Actions.ActivatePallet(model.PalletId, model.ScanText, Sound.Error));
        }

        #endregion

        /// <summary>
        /// Redirects to Location view.
        /// </summary>
        /// <param name="palletId"></param>
        /// <returns>
        /// return all useful information of the passed Pallet.
        /// </returns>     
        /// <remarks>
        /// Posted values expected: palletId
        /// </remarks>
        [AuthorizeExUi("Scan To Pallet requires role {0}", UiType.ScanToPallet, Roles = "DCMS8_SCANTOPALLET")]
        [AuthorizeExUi("VAS To Pallet requires role {0}", UiType.Vas, Roles = "DCMS8_VASTOPALLET")]
        public virtual ActionResult Location(string palletId)
        {
            var boxes = _service.GetBoxesOfPallet(palletId).ToArray();
            var model = new LocationViewModel
                {
                    PalletLimit = _service.GetPalletVolumeLimit(),
                    PalletId = palletId,
                    TotalBoxVolume = boxes.Sum(p => p.Volume),
                    BoxesOnPalletCount = boxes.Count(),
                    CurrentPalletLocation = boxes.Select(p => p.LocationId).First(),
                    CurrentPalletArea = boxes.Select(p => p.IaId).First(),
                    CustomerId = boxes.Select(p => p.CustomerId).First()
                };
            AddStatusMessage(string.Format("Scan any location to place pallet {0} on it.", palletId));
            return View(Views.Location, model);
        }

        /// <summary>
        /// Place the pallet on the scanned location after performing due validation.
        /// Expects Location and palletid.
        /// Press enter to skip.
        /// </summary>
        /// <param name="model"></param>
        /// <returns>
        /// redirect to ActivatePallet.
        /// </returns>
        /// <remarks>
        /// Posted values expected: ScanText, palletId
        /// </remarks>
        [HttpPost]
        [AuthorizeExUi("Scan To Pallet requires role {0}", UiType.ScanToPallet, Roles = "DCMS8_SCANTOPALLET")]
        [AuthorizeExUi("VAS To Pallet requires role {0}", UiType.Vas, Roles = "DCMS8_VASTOPALLET")]
        public virtual ActionResult LocatePallet(LocationViewModel model)
        {
            //TC18: Location must be empty.When after merging pallets, we are trying to put the pallet on some location.
            if (string.IsNullOrEmpty(model.ScanText))
            {
                return View(Views.ScanToPallet, new ScanToPalletViewModel());
            }

            try
            {
                _service.EnsureLocationIsValid(model.ScanText);
            }
            catch (BoxManagerServiceException ex)
            {
                switch (ex.ErrorCode)
                {
                    case BoxManagerServiceErrorCode.InvalidLocation:
                        ModelState.AddModelError("", string.Format("Location {0} is invalid, Please scan a valid location.", model.ScanText));
                        break;
                }
                return Location(model.PalletId);
            }
            _service.UpdatePalletLocation(model.PalletId, model.ScanText);
            this.AddStatusMessage(string.Format("Pallet {0} placed on location {1}.", model.PalletId, model.ScanText));
            return CreatingPalletIndex();
        }

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
                        AddStatusMessage("Please scan the box or pallet again");
                        result = RedirectToAction(this.Actions.CreatingPalletIndex());
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



