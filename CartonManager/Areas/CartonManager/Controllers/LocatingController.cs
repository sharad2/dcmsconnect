using DcmsMobile.CartonManager.Repository.Locating;
using DcmsMobile.CartonManager.ViewModels.Locating;
using EclipseLibrary.Mvc.Controllers;
using EclipseLibrary.Oracle.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;

namespace DcmsMobile.CartonManager.Areas.CartonManager.Controllers
{
    [RouteArea("CartonManager")]
    [RoutePrefix(LocatingController.NameConst)]

    [AuthorizeEx("Locating requires Role {0}", Roles = "SRC_LOCATING", Purpose = "Enables locating of carton")]
    public partial class LocatingController : EclipseController
    {
        #region Initialization

        private LocatingService _service;

        protected override void Initialize(RequestContext requestContext)
        {
            if (_service == null)
            {
                _service = new LocatingService(requestContext);
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



        [Route(LocatingController.ActionNameConstants.Pallet, Name = DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_CartonLocating)]
        public virtual ActionResult Pallet(Sound sound = Sound.None)
        {
            return View(Views.Pallet, new PalletViewModel { Sound = (char)sound });
        }

        /// <summary>
        /// This method pass the pallet id to the action method AcceptPallet for locating the carton.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
       [Route(LocatingController.ActionNameConstants.CartonLocating, Name = DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_CartonLocating1)]
        public virtual ActionResult CartonLocating(string id)
        {
            if (string.IsNullOrEmpty(id))
            {               
                return View(Views.LocationCarton, id);
            }
            return AcceptPallet(id);
        }



        /// <summary>
        /// Validates the pallet entered by the users
        /// </summary>
        /// <param name="palletId"></param>
        /// <returns></returns>
        [HttpPost]
      
        public virtual ActionResult AcceptPallet([Bind]string palletId)
        {
            var model = new LocationCartonViewModel();
            palletId = palletId.ToUpper();
            //TC1: Press enter in Pallet scrren to continue locating without pallet.
            if (string.IsNullOrEmpty(palletId))
            {
                // Pallet not scanned
                return View(Views.LocationCarton, model);
            }

            //TC2: Pallet must be start with P.
            if (!palletId.StartsWith("P"))
            {
                // You will get here when you scanned a pallet not starts with "P".
                ModelState.AddModelError("", "Pallet must be start with P");
                return RedirectToAction(Actions.Pallet(Sound.Error));
            }
            var rows = _service.GetCartonsOfPallet(palletId);
            //TC3: If pallet contains no cartons this will execute.
            if (rows.Count == 0)
            {
                this.AddStatusMessage(string.Format("Pallet {0} is an empty pallet, scan another pallet.", palletId));
                return RedirectToAction(Actions.Pallet(Sound.Error));
            }
            model.PalletId = palletId;
            model.CartonsOnPallet = rows.Count;

            //Check if the cartons marked for rework.
            var reworkCarton = rows.Where(p => p.RemarkWorkNeeded == true).Count();
            if (reworkCarton > 0)
            {
                AddStatusMessage(string.Format("{0} carton of pallet is mark for rework.", reworkCarton));
                model.Sound = (char)Sound.Warning;
            }
            try
            {
                //Put cartons of pallet in suspence
                _service.MarkCartonInSuspense(palletId);
                AddStatusMessage(
                string.Format(
                    "{0} cartons of pallet {1} have been moved to suspense. They will come out of suspense when you locate them.",
                    rows.Count, palletId));
            }
            catch (DbException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            return View(Views.LocationCarton, model);
        }

        /// <summary>
        /// Called after the user scans something
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <remarks>
        /// Post: PalletId, CurrentLocationId, ScanText
        /// The first scan is possibly a location scan. All other scans are presumed to be carton scans.
        /// </remarks>
        public virtual ActionResult HandleScan(LocationCartonViewModel model)
        {
            //TC4:  Enter pressed back to change the pallet.
            var scans = new Queue<string>((model.ScanText ?? string.Empty).Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries));
            if (scans.Count == 0)
            {
                // TC5: Go back to pallet screen. Tell the user that some cartons remain in suspense.
                if (!string.IsNullOrWhiteSpace(model.PalletId))
                {
                    var carton = _service.GetCartonsOfPallet(model.PalletId);
                    if (carton.Count > 0)
                    {
                        AddStatusMessage(string.Format("{0} cartons of Pallet {1} remain in suspense", carton.Count, model.PalletId));
                        model.Sound = (char)Sound.Warning;
                    }
                }
                return RedirectToAction(Actions.Pallet());
            }

            string locationId;
            Location location;
            if (string.IsNullOrWhiteSpace(model.CurrentLocationId))
            {
                // Assume that the first scan represents a location
                locationId = scans.Dequeue();
            }
            else
            {
                // Check whether the first scan is a location
                var scanType = _service.GetScanType(scans.Peek());
                if (scanType == ScanType.LocationId)
                {
                    locationId = scans.Dequeue();
                }
                else
                {
                    locationId = model.CurrentLocationId;
                }
            }

            try
            {
                location = _service.GetLocation(locationId);  // If this ends up being null, we will be simply ignoring the current location
                //TC6: If entered location is invalid.
                if (location == null)
                {
                    throw new ValidationException("Invalid Location " + locationId);
                }
                //TC5: Scanned a carton location.
                if (location.StoresWhat != "CTN")
                {
                    throw new ValidationException(string.Format("Location {0} in area {1} is not a carton location", locationId, location.AreaShortName));
                }
                //TC6: Unavailable location is scanned.
                if (location.UnavailableFlag)
                {
                    throw new ValidationException(string.Format("Location {0} in area {1} is not available for locating", locationId, location.AreaShortName));
                }
                model.DestAreaId = location.AreaId;
                model.DestBuildingId = location.BuildingId;
                model.CountCartonsAtLocation = location.CountCartons;
                model.LocationTravelSequence = location.TravelSequence;
                model.MaxCartonsAtLocation = location.MaxCartons;
                model.CurrentLocationId = locationId;
                model.AreaShortName = location.AreaShortName;
                if (model.MaxCartonsAtLocation.HasValue && model.CountCartonsAtLocation.HasValue && model.MaxCartonsAtLocation.Value <= model.CountCartonsAtLocation.Value)
                {
                    AddStatusMessage(string.Format("Warning: Max {0} cartons are allowed at location", model.MaxCartonsAtLocation));
                    model.Sound = (char)Sound.Warning;
                }

                // Were any cartons scanned?
                if (scans.Count > 0)
                {
                    IList<string> cartons = _service.LocateCartons(scans, location.BuildingId,
                                                                                            location.AreaId, model.CurrentLocationId,
                                                                                            location.TravelSequence, model.PalletId);
                    // User has scanned some cartons before scaning this location

                    model.CountCartonsAtLocation = _service.GetLocation(model.CurrentLocationId).CountCartons;
                    if (!string.IsNullOrWhiteSpace(model.PalletId))
                    {
                        var cartonsOfPallet = _service.GetCartonsOfPallet(model.PalletId);
                        model.CartonsOnPallet = cartonsOfPallet.Count;
                    }
                    // Check if the scanned carton is mark for rework.
                    var reworkCartons = _service.GetCartonsDetails(scans.ToArray()).Where(p => p.RemarkWorkNeeded == true).Select(q => q.CartonId);

                    if (reworkCartons.Count() > 0)
                    {
                        AddStatusMessage(string.Format("{0} Cartons are mark for rework.", string.Join(",", reworkCartons)));
                        model.Sound = (char)Sound.Warning;
                    }
                    if (cartons.Count > 0)
                    {
                        // Some cartons were located
                        var locatedCartons = string.Join(",", cartons);
                        AddStatusMessage(string.Format(" {0} Cartons located at Location {1}.", locatedCartons,
                            model.CurrentLocationId));
                    }
                    if (cartons.Count < scans.Count)
                    {
                        // Some cartons were invalid
                        var query = scans.Select((p, i) => new
                        {
                            Index = i + 1,
                            ScanText = p
                        }).Where(p => !cartons.Contains(p.ScanText)).Select(p => string.Format("({0}) {1}", p.Index, p.ScanText));
                        ModelState.AddModelError("", string.Format("Invalid Cartons {0}.", string.Join("; ", query)));
                    }
                }
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            catch (OracleDataStoreException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            return View(Views.LocationCarton, model);
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
                result = RedirectToActionPermanent(MVC_CartonManager.CartonManager.Home.Index());
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
                        result = RedirectToAction(this.Actions.Pallet(Sound.Error));
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
