using DcmsMobile.BoxManager.Repository;
using DcmsMobile.BoxManager.ViewModels;
using DcmsMobile.BoxManager.ViewModels.MovePallet;
using EclipseLibrary.Mvc.Controllers;
using System;
using System.Configuration;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using System.Web.Routing;


//Reviewed By: Deepak Bhatt 11 June 2012

namespace DcmsMobile.BoxManager.Areas.BoxManager.Controllers
{
    [AuthorizeEx("Move Pallet requires Role {0}", Roles = "DCMS8_SCANTOPALLET")]
    [RouteArea("BoxManager")]
    [RoutePrefix(MovePalletController.NameConst)]
    public partial class MovePalletController : EclipseController
    {
        #region Initialize
        /// <summary>
        /// Required by T4MVC
        /// </summary>
        public MovePalletController()
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


        protected override void Initialize(RequestContext requestContext)
        {
            if (_service == null)
            {
                var connectString = ConfigurationManager.ConnectionStrings["dcms8"].ConnectionString;
                var userName = requestContext.HttpContext.SkipAuthorization ? string.Empty : requestContext.HttpContext.User.Identity.Name;
                var clientInfo = string.IsNullOrEmpty(requestContext.HttpContext.Request.UserHostName) ? requestContext.HttpContext.Request.UserHostAddress :
                    requestContext.HttpContext.Request.UserHostName;
                _service = new BoxManagerService(requestContext.HttpContext.Trace, connectString, userName, clientInfo, "MovePallet");
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

        #region Move Pallets

        private readonly static Regex __regexPallet = new Regex(@"^([P|p]\S{1,19}$)");
        //private readonly static Regex __regLocation = new Regex(@"^911\w{5}$");
        private readonly static Regex __regexUcc = new Regex(@"^0000\d{16}$");

        /// <summary>
        /// Showing the information of passed pallet.
        /// </summary>
        /// <returns>
        /// It returns MovePallet view with all information of Passed Pallet.
        /// </returns>
        /// 
        [Route(MovePalletController.ActionNameConstants.Index, Name = DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_MoveBoxPallet)]
        public virtual ActionResult Index(Sound sound = Sound.None)
        {
            return View(Views.Index, new IndexViewModel { Sound = (char)sound });
        }

        [Route(MovePalletController.ActionNameConstants.MovePallet,Name = DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_MoveBoxPallet1)]
        public virtual ActionResult MovePallet(string id)
        {
            if(string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException();
            }
            var model = new IndexViewModel
            {
                ScanText = id
            };
            return SourcePallet(model);
        }

        /// <summary>
        ///  Returns source pallet information. If a box is scanned which is on a pallet 
        ///  we consider it as equivalent to pallet scan.
        /// </summary>
        /// <returns>
        /// Returns source pallet information.
        /// </returns>
        public virtual ActionResult SourcePallet(IndexViewModel model)
        {
            //TC1: If the model state is not valid.[For eg: when no pallet is passed and press enter].
            if (!ModelState.IsValid)
            {
                return View(Views.Index, model);
            }
            //TC2: If the entered value is a box.
            if (__regexUcc.IsMatch(model.ScanText))
            {
                // Box is on a pallet. Consider it equivalent to pallet scan.
                var box = _service.GetBox(model.ScanText, false);
                //TC3: If the entered box is not on any pallet.
                if (box == null || string.IsNullOrEmpty(box.PalletId))
                {
                    ModelState.AddModelError("", string.Format("Scanned Box {0} is not on any pallet.", model.ScanText));
                    return View(Views.Index, model);
                }
                else
                {
                    //TC: Box is not valid.[i.e box is not verified or shipped].
                    if (!TryValidateModel(new BoxModel(box)) || box.VerifyDate == null || !string.IsNullOrWhiteSpace(box.SmallShipmentFlag))
                    {
                        if (box.VerifyDate == null)
                        {
                            ModelState.AddModelError("box.VerifyDate", "Box has not been verified yet.");
                        }
                        if (!string.IsNullOrWhiteSpace(box.SmallShipmentFlag))
                        {
                            ModelState.AddModelError("box.SmallShipmentFlag", "Boxes of small shipments cannot be palletized.");
                        }
                        // Box is not appropriate for palletizing. Not Verified, shipped, etc.
                        return RedirectToAction(this.Actions.Index(Sound.Error));
                    }
                }
                model.PalletId = box.PalletId;
            }
            //TC4: If the entered value is a pallet.
            else if (__regexPallet.IsMatch(model.ScanText))
            {
                model.PalletId = model.ScanText;
            }
            else
            {
                ModelState.AddModelError("", string.Format("Scan text {0} is not recognized.", model.ScanText));
                return View(Views.Index, model);
            }
            var boxesOfPallet = _service.GetBoxesOfPallet(model.PalletId);
            //TC5: If an empty pallet is scanned.
            if (boxesOfPallet != null && !boxesOfPallet.Any())
            {
                //Pallet is empty,Can't move or merge.
                ModelState.AddModelError("", string.Format("Pallet {0} does not contain any boxes.", model.PalletId));
                return View(Views.Index, model);
            }
            else
            {
                //Validate boxes of pallet.
                foreach (var box in boxesOfPallet)
                {
                    //TC: Boxes are not valid.[i.e box is not verified or shipped].
                    if (!TryValidateModel(new BoxModel(box)) || box.VerifyDate == null || !string.IsNullOrWhiteSpace(box.SmallShipmentFlag))
                    {
                        if (box.VerifyDate == null)
                        {
                            ModelState.AddModelError("box.VerifyDate", string.Format("Boxes of pallet {0} have not been verified yet.", model.PalletId));
                        }
                        if (!string.IsNullOrWhiteSpace(box.SmallShipmentFlag))
                        {
                            ModelState.AddModelError("box.SmallShipmentFlag", "Boxes of small shipments cannot be palletized.");
                        }
                        // Boxes are not appropriate for palletizing. Not Verified, shipped, etc.
                        return RedirectToAction(this.Actions.Index(Sound.Error));
                    }
                }
            }
            try
            {
                _service.EnsureCriteriaPure(boxesOfPallet, false);
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
                        ModelState.AddModelError("", string.Format("Pallet {0} does not satisfy criteria because it contains boxes of multiple customers {1}.",
                            model.PalletId, ex.Data["Data"]));
                        break;

                    case BoxManagerServiceErrorCode.MultipleDcPallet:
                        ModelState.AddModelError("", string.Format("Pallet {0} does not satisfy criteria because it contains boxes of multiple DCs {1}.",
                             model.PalletId, ex.Data["Data"]));
                        break;
                    case BoxManagerServiceErrorCode.MultiplePoPallet:
                        ModelState.AddModelError("", string.Format("Pallet {0} does not satisfy criteria because it contains boxes of multiple POs {1}.",
                            model.PalletId, ex.Data["Data"]));
                        break;
                }
                return RedirectToAction(this.Actions.Index(Sound.Error));
            }
            // Now ask for validation
            return RedirectToAction(this.Actions.ValidatePallet(model.PalletId));
        }

        /// <summary>
        /// Redirects to Validation view.
        /// </summary>
        /// <param name="palletId"></param>
        /// <returns></returns>
        public virtual ActionResult ValidatePallet(string palletId)
        {
            var box = _service.GetBoxesOfPallet(palletId).FirstOrDefault();
            //TC6: If the pallet doesn't contain any boxes.
            if (box == null)
            {
                ModelState.AddModelError("", string.Format("Pallet {0} does not contain any boxes.", palletId));
                return RedirectToAction(this.Actions.Index(Sound.Error));
            }
            var model = new ValidatePalletViewModel
                            {
                                SourcePalletId = palletId,
                                CustomerId = box.CustomerId,
                                SourcePalletAreaId = box.IaId,
                                SourcePalletLocationId = box.LocationId,
                                //verified boxes count of passed customer criteria.
                                VerifiedBoxes = _service.GetVerifiedBoxes(box.CustomerId, box.PoId, box.CustomerDcId, box.BucketId)
                            };

            return View(Views.ValidatePallet, model);
        }

        /// <summary>
        /// This function handles the box count entered by the User while moving a pallet. 
        /// </summary>
        /// <param name="model"></param>
        /// <remarks>
        /// 1. User enters correct box count he will be directed to destination location view.
        /// 2. If user enters incorrect box count then he will asked to provide
        /// box count again. If the box count does not match again then UI redirects to method 
        /// ValidateBoxesOnPallet and puts all the boxes of pallet in suspense. 
        /// 3. On enter key press we forget everything and redirect to index page.
        /// Posted Values
        /// ScanText:The scan text is box count
        /// ConfirmBoxCount: Confirmation box count
        /// </remarks>
        [HttpPost]
        public virtual ActionResult HandleBoxCount(ValidatePalletViewModel model)
        {
            //TC7: If nothing is passed when the UI asked for the no of boxes already placed on the pallet.
            if (model.BoxCount == null)
            {
                return RedirectToAction(this.Actions.Index());
            }
            var boxes = _service.GetBoxesOfPallet(model.SourcePalletId).ToArray();
            //TC8: when we doesn't scan any box for the pallet.
            if (!boxes.Any())
            {
                ModelState.AddModelError("", string.Format("Pallet {0} does not contain any boxes.", model.SourcePalletId));
                return RedirectToAction(this.Actions.Index(Sound.Error));
            }
            var box = boxes.FirstOrDefault();
            model.SourcePalletAreaId = box.IaId;
            model.SourcePalletLocationId = box.LocationId;
            model.CustomerId = box.CustomerId;
            model.VerifiedBoxes = _service.GetVerifiedBoxes(box.CustomerId, box.PoId, box.CustomerDcId, box.BucketId);
            //TC9: When we entered exactly the same no. of boxes that are already placed on the pallet. 
            if (boxes.Count() == model.BoxCount)
            {
                return RedirectToAction(this.Actions.Destination(model.SourcePalletId));
            }
            //TC10: When we entered wrong no. of boxes that are already on pallet on repetitive basis. 
            if (model.IsConfirm)
            {
                _service.PutBoxOfPalletInSuspense(model.SourcePalletId);
                ModelState.AddModelError("", string.Format("Incorrect box count. Please scan each box on pallet {0} to validate it.", model.SourcePalletId));
                return RedirectToAction(this.Actions.ValidateBoxesOnPallet(model.SourcePalletId, Sound.Error));
            }
            model.IsConfirm = true;
            ModelState.AddModelError("", string.Format("Incorrect box count.Please enter correct box count on pallet {0}.", model.SourcePalletId));
            return View(Views.ValidatePallet, model);
        }

        /// <summary>
        /// Redirects to view ValidateBoxes .
        /// </summary>
        /// <param name="palletId"></param>
        /// <param name="sound"> </param>
        /// <returns>view ValidateBoxesOnPallet</returns>
        public virtual ActionResult ValidateBoxesOnPallet(string palletId, Sound sound = Sound.None)
        {

            return View(Views.ValidateBoxes, new ValidateBoxesViewModel { SourcePalletId = palletId, Sound = (char)sound });
        }

        /// <summary>
        /// Validates each box of the passed pallet.
        /// </summary>
        /// <param name="model"></param>
        /// <remarks>
        /// This function accepts a box scan against a pallet to validate it. On successful validation this function
        /// will remove the suspense date of the scanned box.
        /// When enter is pressed we check whether pallet validation is done. After validation is complete we redirect to destination view.  
        /// Those boxes which could not be validated are marked in suspense and remove from pallet.
        /// Post Values: 
        /// SourcePalletId ,
        /// ConfirmScanText
        /// </remarks>
        [HttpPost]
        public virtual ActionResult ScanBoxOfSourcePallet(ValidateBoxesViewModel model)
        {
            //TC11: When we scan an empty box against the pallet.
            if (string.IsNullOrEmpty(model.ScanText))
            {
                var boxesOnPallet = _service.GetBoxesOfPallet(model.SourcePalletId);
                var boxesOnSuspence = boxesOnPallet.Where(p => p.SuspenseDate != null);
                //TC12: When we successfully scanned all the boxes of the passed pallet.
                if (boxesOnSuspence.Count() == 0)
                {
                    this.AddStatusMessage(string.Format("Pallet {0} successfully validated.", model.SourcePalletId));
                    return RedirectToAction(this.Actions.Destination(model.SourcePalletId));
                }
                //TC13: When we completed scanning of all the boxes of the passed pallet.
                if (!model.IsConfirmScanText)
                {
                    model.ScanBoxCount = _service.GetValidBoxesCount(model.SourcePalletId);
                    this.AddStatusMessage(string.Format("Are you sure that you have scanned all the boxes of pallet {0}? If 'Yes' press ENTER again otherwise scan remaining boxes one by one.", model.SourcePalletId));
                    model.IsConfirmScanText = true;
                    model.Sound = (char)Sound.Warning;
                    return View(Views.ValidateBoxes, model);
                }
                var boxes = boxesOnSuspence.Select(p => p.Ucc128Id);
                foreach (var box in boxes)
                {
                    _service.RemovePalletFromBox(box);
                }

                this.AddStatusMessage(string.Format("Pallet {0} successfully validated.", model.SourcePalletId));
                return RedirectToAction(this.Actions.Destination(model.SourcePalletId));
            }
            model.IsConfirmScanText = false;
            //TC14: If we are scanning a valid box.
            if (__regexUcc.IsMatch(model.ScanText))
            {
                try
                {
                    var box = _service.GetBox(model.ScanText, false);
                    //TC15: If no box is passed.
                    if (box == null)
                    {
                        ModelState.AddModelError("", "Box is not valid");
                        return View(Views.ValidateBoxes, model);
                    }

                    //TC16: When the box of the passed pallet is not verified and shipped.
                    if (!TryValidateModel(new BoxModel(box)) || box.VerifyDate == null || !string.IsNullOrWhiteSpace(box.SmallShipmentFlag))
                    {
                        if (box.VerifyDate == null)
                        {
                            ModelState.AddModelError("box.VerifyDate", string.Format("Box {0} has not been verified yet.", model.ScanText));
                        }
                        if (!string.IsNullOrWhiteSpace(box.SmallShipmentFlag))
                        {
                            ModelState.AddModelError("box.SmallShipmentFlag", "Boxes of small shipments cannot be palletized.");
                        }
                        model.ScanBoxCount = _service.GetValidBoxesCount(model.SourcePalletId);
                        // Box is not appropriate for palletizing. Not Verified, shipped, etc.
                        ModelState.AddModelError("", string.Format("Box {0} does not belong to pallet {1}.Please remove it from the pallet.", model.ScanText, model.SourcePalletId));
                        return View(Views.ValidateBoxes, model);
                    }
                    _service.TryUpdateBox(model.SourcePalletId, model.ScanText, false);
                    this.AddStatusMessage(string.Format("Box {0} successfully validated.", model.ScanText));
                }
                catch (BoxManagerServiceException)
                {
                    ModelState.AddModelError("", string.Format("Box {0} does not belong to pallet {1}.Please remove it from the pallet.", model.ScanText, model.SourcePalletId));
                }
            }
            else
            {
                ModelState.AddModelError("", string.Format("Scan text {0} is not recognized.", model.ScanText));
            }
            model.ScanBoxCount = _service.GetValidBoxesCount(model.SourcePalletId);
            return View(Views.ValidateBoxes, model);
        }

        /// <summary>
        /// Displays the view which will ask for destination of the passed pallet.
        /// </summary>
        /// <param name="palletId"></param>
        /// <param name="sound"></param>
        /// <returns></returns>
        /// <remarks>
        /// The destination can be location or pallet
        /// </remarks>
        [HttpGet]
        public virtual ActionResult Destination(string palletId, Sound sound = Sound.None)
        {
            //TC17:When no pallet is passed during verifying the boxes with the correct quantity. 
            if (palletId == null) throw new ArgumentNullException("palletId");
            var boxes = _service.GetBoxesOfPallet(palletId).ToArray();
            //TC18: When no box is passed/count of the box is zero.
            if (!boxes.Any())
            {
                ModelState.AddModelError("", string.Format("Pallet {0} does not contain any boxes.", palletId));
                return RedirectToAction(this.Actions.Index(Sound.Error));
            }
            else
            {
                foreach (var box in boxes)
                {
                    if (!TryValidateModel(new BoxModel(box)) || box.VerifyDate == null || !string.IsNullOrWhiteSpace(box.SmallShipmentFlag))
                    {
                        if (box.VerifyDate == null)
                        {
                            ModelState.AddModelError("box.VerifyDate", string.Format("Boxes of pallet {0} have not been verified yet.", palletId));
                        }
                        if (!string.IsNullOrWhiteSpace(box.SmallShipmentFlag))
                        {
                            ModelState.AddModelError("box.SmallShipmentFlag", "Boxes of small shipments cannot be palletized.");
                        }
                        return RedirectToAction(this.Actions.Index(Sound.Error));
                    }
                }
            }
            var firstBox = boxes.First();
            var model = new DestinationViewModel
                {
                    SourcePalletId = palletId,
                    TotalBoxVolume = boxes.Sum(p => p.Volume),
                    TotalBoxesOnPallet = boxes.Count(),
                    PalletVolumeLimit = _service.GetPalletVolumeLimit(),
                    SourcePalletLocationId = firstBox.LocationId,
                    SourcePalletAreaId = firstBox.IaId, //TODO: Show the short Name of Area instead of AreaId
                    CustomerId = firstBox.CustomerId,
                    AppointmentDate = firstBox.AppointmentDate,
                    AppointmentNo = firstBox.AppointmentNo,
                    DoorId = firstBox.DoorId,
                    Sound = (char)sound
                };
            // Ensure that pallet is pure.
            var sortCriteria = _service.EnsureCriteriaPure(boxes, false);
            //TC19: if the AllowPoMixing  that are set in flag are also set in the MovePalletController class.
            if (!sortCriteria.HasFlag(SortCriteria.AllowPoMixing))
            {
                model.PoId = firstBox.PoId;
            }
            //TC20: if the AllowCustomerDcMixing  that are set in flag are also set in the MovePalletController class.
            if (!sortCriteria.HasFlag(SortCriteria.AllowCustomerDcMixing))
            {
                model.CustomerDcId = firstBox.CustomerDcId;
            }
            //TC21: if the AllowBucketMixing  that are set in flag are also set in the MovePalletController class.
            if (!sortCriteria.HasFlag(SortCriteria.AllowBucketMixing))
            {
                model.BucketId = firstBox.BucketId;
            }
            //Suggest location list.
            var suggestLocation = _service.SuggestLocation(model.CustomerId, model.BucketId, model.PoId, model.CustomerDcId, model.SourcePalletAreaId);
            model.LocationSuggestionList = suggestLocation.Select(p => new PalletModel(p)).ToList();
            return View(Views.Destination, model);
        }

        /// <summary>
        /// ScanText and PalletId are posted.
        /// If enter is pressed redirects to previous UI.
        /// </summary>
        /// <param name="model"></param>
        /// <remarks>
        /// This is the public action which receives whatever the user has scanned.
        /// </remarks>
        /// Posted Values
        /// ScanText:The scan text is pallet or location
        /// PalletId: Source Pallet
        /// <returns>
        /// </returns>
        [HttpPost]
        public virtual ActionResult HandleDestinationScan(DestinationViewModel model)
        {
            //TC22: Location must be empty. 
            if (!ModelState.IsValid || string.IsNullOrEmpty(model.ScanText))
            {
                // Invalid source pallet
                return Index();
            }
            try
            {
                ////Compatibility Code to handle location scan 
                ////#####################################################################################
                //int nLength = model.ScanText.Length;
                //if (nLength <= 4 && nLength > 0)
                //{
                //    int nDiffChars = 5 - nLength;
                //    char padChar = '0';
                //    model.ScanText = "911" + model.ScanText.PadLeft(nLength + nDiffChars, padChar);
                //}
                ////######################################################################################
                //if (__regLocation.IsMatch(model.ScanText))
                //{
                //As per our discussion we will assume default scan as a location scan. MB,DB and SHIVA(June 19th 2012)

                _service.EnsureLocationIsValid(model.ScanText);
                // Location scan
                _service.UpdatePalletLocation(model.SourcePalletId, model.ScanText);
                this.AddStatusMessage(string.Format("Pallet {0} placed on Location {1}.", model.SourcePalletId, model.ScanText));
                return Index();
            }
            catch (BoxManagerServiceException ex)
            {
                switch (ex.ErrorCode)
                {
                    case BoxManagerServiceErrorCode.MergingPalletWithSelf:
                        ModelState.AddModelError("", string.Format("Pallet {0} and {1} could not be merged because they are same.", model.ScanText, model.SourcePalletId));
                        break;

                    case BoxManagerServiceErrorCode.MultipleAreaPallet:
                        ModelState.AddModelError("", string.Format("Pallet {0} contains boxes of multiple areas {1}.", model.ScanText, ex.Data["Data"]));
                        break;

                    case BoxManagerServiceErrorCode.MultipleLocationPallet:
                        ModelState.AddModelError("", string.Format("Pallet {0} contain boxes of multiple locations {1}.", model.ScanText, ex.Data["Data"]));
                        break;

                    case BoxManagerServiceErrorCode.MultipleBucketPallet:
                        ModelState.AddModelError("", string.Format("Merging denied because merged pallet {0} and {1} would contain boxes of multiple buckets {2}.",
                           model.ScanText, model.SourcePalletId, ex.Data["Data"]));
                        break;

                    case BoxManagerServiceErrorCode.MultipleCustomerPallet:
                        ModelState.AddModelError("", string.Format("Merging denied because merged pallet {0} and {1} would contain boxes of multiple customers {2}.",
                           model.ScanText, model.SourcePalletId, ex.Data["Data"]));
                        break;

                    case BoxManagerServiceErrorCode.MultipleDcPallet:
                        ModelState.AddModelError("", string.Format("Merging denied because merged pallet {0} and {1} would contain boxes of multiple DCs {2}.",
                           model.ScanText, model.SourcePalletId, ex.Data["Data"]));
                        break;
                    case BoxManagerServiceErrorCode.MultiplePoPallet:
                        ModelState.AddModelError("", string.Format("Merging denied because merged pallet {0} and {1} would contain boxes of multiple POs {2}.",
                         model.ScanText, model.SourcePalletId, ex.Data["Data"]));
                        break;

                    case BoxManagerServiceErrorCode.InvalidLocation:
                        ModelState.AddModelError("", string.Format("Location {0} is invalid.", model.ScanText));
                        break;

                }
                // The service generates this exception. The pallets cannot be merged.  
                return RedirectToAction(this.Actions.Destination(model.SourcePalletId, Sound.Error));
            }
        }
        #endregion

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
                result = RedirectToActionPermanent(MVC_BoxManager.BoxManager.Home.Index());
            }
            else
            {

                if (this.HttpContext.Request.HttpMethod == "GET")
                {
                    var attrPost = methods.SelectMany(p => p.GetCustomAttributes(typeof(HttpPostAttribute), true)).FirstOrDefault();
                    if (attrPost != null)
                    {
                        // GET request for an action which requires POST. Assume that the user has been redirected from the login screen
                        AddStatusMessage("Please scan the Pallet again.");
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
