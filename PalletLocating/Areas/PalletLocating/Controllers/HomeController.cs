using System;
using System.Collections.Generic;
using System.Configuration.Provider;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using DcmsMobile.PalletLocating.Models;
using DcmsMobile.PalletLocating.Repository;
using DcmsMobile.PalletLocating.ViewModels;
using EclipseLibrary.Mvc.Controllers;


namespace DcmsMobile.PalletLocating.Areas.PalletLocating.Controllers
{
    [AuthorizeEx("Pallet Locating requires Roles {0}", Roles = "SRC_LOCATING")]
    public partial class HomeController : EclipseController
    {
        #region Intialization

        /// <summary>
        /// Required by T4MVC
        /// </summary>
        public HomeController()
        {

        }

        private PalletLocatingService _service;
        // private const string ORPHAN_CARTON_KEY = "ORPHAN_CARTON_KEY";

        protected override void Initialize(RequestContext requestContext)
        {
            if (_service == null)
            {
                _service = new PalletLocatingService(requestContext);
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

        /// <summary>
        /// Set query count in the model
        /// </summary>
        /// <param name="viewName"></param>
        /// <param name="masterName"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        protected override ViewResult View(string viewName, string masterName, object model)
        {
            // All view models must be derived from ViewModelBase, else cast error here
            var vmb = model as ViewModelBase;
            if (vmb != null)
            {
                // Tutorial page does not use a derived model
                vmb.QueryCount = _service.QueryCount;
            }
            return base.View(viewName, masterName, model);

        }

        #endregion

        /// <summary>
        /// Renders the Building view
        /// </summary>
        /// <returns></returns>
        public virtual ActionResult Building()
        {
            //if (mobileView.HasValue)
            //{
            //    MobileEmulation.EmulateMobile(this.ControllerContext, mobileView.Value);
            //    return RedirectToAction(Building());
            //}

            var model = new BuildingViewModel();

            model.AreaChoicesRequested += new EventHandler<EventArgs>(BuildingViewModel_AreaChoicesRequested);
            return View(Views.Building, model);
        }

        private void BuildingViewModel_AreaChoicesRequested(object sender, EventArgs e)
        {
            var model = (BuildingViewModel)sender;
            model.AreaChoices = _service.GetCartonAreas(null).Select(p => new AreaModel(p));
            //null means all buildings
        }

        /// <summary>
        /// Displays the Pallet or the Area view. Building mast be passed. If the building is not passed, then this is an error situation and the Building View is displayed.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public virtual ActionResult PalletOrArea(BuildingViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Building not passed. Ask for building.
                return Building();
            }

            if (!_service.ValidateBuilding(model.BuildingId))
            {
                ModelState.AddModelError("", string.Format("Building {0} does not exist", model.BuildingId));
                return Building();
            }

            // Interested in building specific areas only
            var areas = _service.GetCartonAreas(model.BuildingId).Where(p => p.BuildingId == model.BuildingId);

            // Counting only building specific areas
            switch (areas.Count())
            {
                case 0:
                    // No building specific area. So ask for area
                    return View(Views.Area, PopulateAreaViewModel(model.BuildingId, null));

                case 1:
                    // Ask for pallet
                    var avm = PopulateAreaViewModel(model.BuildingId, areas.First().AreaId);
                    return Pallet(avm);

                case 2:
                    //Common case. One area which needs replenishment, and another area which provides replenishment.
                    //In this case destination is the area which needs replenishment.
                    try
                    {
                        var destArea = areas.Single(p => !string.IsNullOrEmpty(p.ReplenishAreaId));
                        //This code will never be executed.
                        var palletModel = new PalletViewModel(destArea)
                            {
                                SuggestedLocations =
                                    _service.GetReplenishmentSuggestions(destArea.BuildingId, destArea.AreaId, destArea.ReplenishAreaId, 300, true)
                                            .Select(p => new ReplenishmentSuggestionModel(p))
                            };
                        return View(Views.Pallet, palletModel);
                    }
                    catch (InvalidOperationException)
                    {
                        // Perhaps both areas do not need replenishment. Ask for area.
                        return View(Views.Area, PopulateAreaViewModel(model.BuildingId, null));
                    }

                default:
                    // Multiple areas. Ask for area.
                    return View(Views.Area, PopulateAreaViewModel(model.BuildingId, null));
            }
        }

        /// <summary>
        /// This method is called from action link
        /// </summary>
        /// <param name="areaId"></param>
        /// <param name="useCache">Whether it is permissible to retrieve replenishment suggestions from cache</param>
        /// <returns></returns>
        public virtual ActionResult PalletLink(string areaId, bool useCache)
        {
            var model = new AreaViewModel
            {
                AreaId = areaId,
                UseSuggestionCache = useCache
            };
            return Pallet(model);
        }

        /// <summary>
        /// Displays Pallet view. Must pass and AreaShortName.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <remarks>
        /// If building and/or area is not passed, the building screen is displayed.
        /// </remarks>
        public virtual ActionResult Pallet(AreaViewModel model)
        {
            if (string.IsNullOrEmpty(model.AreaId) && string.IsNullOrEmpty(model.AreaShortName))
            {
                // Ask for building again
                ModelState.Clear();
                return Building();
            }
            Area area = null;
            if (!string.IsNullOrEmpty(model.AreaId))
            {
                area = _service.GetCartonArea(model.AreaId);
            }
            if (!string.IsNullOrEmpty(model.AreaShortName))
            {
                area = _service.GetCartonArea(model.BuildingId, model.AreaShortName);
            }
            if (area == null)
            {
                //Get area of location, if location was scanned.
                area = _service.GetAreaFromLocationId(model.AreaShortName);
            }
            if (area == null)
            {
                ModelState.AddModelError("", string.Format("Area/Location {0} is not recognized for building {1}", model.AreaShortName, model.BuildingId));
                return View(Views.Area, PopulateAreaViewModel(model.BuildingId, null));
            }
            var palletModel = new PalletViewModel(area)
                {
                    SuggestedLocations = _service.GetReplenishmentSuggestions(area.BuildingId, area.AreaId, area.ReplenishAreaId, 300, model.UseSuggestionCache)
                                                .Select(p => new ReplenishmentSuggestionModel(p))
                };
            return View(Views.Pallet, palletModel);
        }

        /// <summary>
        /// Renders the Location View. PalletId and Area id must be passed.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <remarks>BuildingId is not required due to such Areas which are in multiple buildings</remarks>
        public virtual ActionResult Location(PalletViewModel model)
        {
            if (string.IsNullOrEmpty(model.AreaId) || string.IsNullOrEmpty(model.PalletOrCartonId))
            {
                // If pallet not passed, then we must ask for area. this means user pressed a blank enter and he wishes to
                // specify a new area.
                ModelState.Clear();
                return View(Views.Area, PopulateAreaViewModel(model.BuildingId, null));
            }

            var pallet = model.PalletOrCartonId.StartsWith("P") ? _service.GetPallet(model.PalletOrCartonId) :
                _service.GetPalletFromCartonId(model.PalletOrCartonId);
            //invalid pallet passed, returns pallet view to ask again a valid pallet.
            var destArea = _service.GetCartonArea(model.AreaId);
            ////////////////////////////// Orphan carton case /////////////////////////////////////
            if (pallet == null)
            {
                // Orphan carton case. We will offer the user to put this carton on a pallet.
                var carton = _service.GetCarton(model.PalletOrCartonId);
                if (carton != null)
                {
                    //Ask for new carton or pallet
                    var palletModel = new PalletViewModel(destArea)
                        {
                            SuggestedLocations =
                                _service.GetReplenishmentSuggestions(destArea.BuildingId, destArea.AreaId, destArea.ReplenishAreaId, 300, true)
                                        .Select(p => new ReplenishmentSuggestionModel(p)),
                            //Store the carton in hidden field for C2P
                            LastScan = carton.CartonId
                        };
                    AddStatusMessage(string.Format("Scan Pallet to palletize carton {0}", carton.CartonId));
                    return View(Views.Pallet, palletModel);
                }

                //If scan was invalid then populate this view again.
                ModelState.AddModelError("", string.Format("Scan {0} is not recognized as Pallet or Carton.", model.PalletOrCartonId));
                return Pallet(PopulateAreaViewModel(model.BuildingId, model.AreaId));
            }
            if (!string.IsNullOrEmpty(model.LastScan))
            {
                //User scanned a pallet therefore try to the palletize carton.
                try
                {
                    pallet = _service.PalletizeCarton(model.LastScan, pallet);
                    AddStatusMessage(string.Format("Carton {0} successfully moved to Pallet {1}", model.LastScan, pallet.PalletId));
                }
                catch (ProviderException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            if (!ModelState.IsValid)
            {
                return Pallet(PopulateAreaViewModel(model.BuildingId, model.AreaId));
            }

            ///////////////////////////////////////////////// End orphan carton case ///////////////////////////////////////////////////////

            // Multi SKU case, Error message to clean the pallet first before locate this.
            if (pallet.SkuCount > 1)
            {
                AddStatusMessage(string.Format("Pallet {0} contains {1} cartons of {2} different SKUs.Continue only if you are sure?", model.PalletOrCartonId, pallet.CartonCount, pallet.SkuCount));
                model.Sound = 'E';
            }

            if (destArea.IsNumbered)
            {
                var lvm = PopulateLocationViewModel(model.AreaId, model.BuildingId, pallet);
                lvm.Sound = model.Sound;
                return View(Views.Location, lvm);
            }
            //Locate and merge pallet
            _service.LocateandMergePallet(null, pallet.PalletId, destArea.AreaId, null);
            AddStatusMessage(string.Format("Pallet {0} successfully moved to {1}", pallet.PalletId, destArea.ShortName));

            //If successfully updated, returns new PalletViewModel to Pallet View with previous state values like destination Area and Building
            return RedirectToAction(MVC_PalletLocating.PalletLocating.Home.ActionNames.Pallet,
                                    new AreaViewModel { AreaId = destArea.AreaId, BuildingId = model.BuildingId, Sound = 'S' });
        }
        /// <summary>
        /// This method is used to populate area view model
        /// </summary>
        /// <returns></returns>
        private AreaViewModel PopulateAreaViewModel(string buildingId, string areaId)
        {
            return new AreaViewModel
                 {
                     BuildingId = buildingId,
                     AreaList = _service.GetCartonAreas(buildingId).Select(p => new AreaModel(p)),
                     AreaId = string.IsNullOrEmpty(areaId) ? null : _service.GetCartonArea(areaId).AreaId
                 };
        }

        /// <summary>
        /// This method is used to populate the LocationViewModel
        /// Confirm View and Location view use this method to populate
        /// </summary>
        ///  <param name="areaId"></param>
        /// <param name="buildingId"></param>
        /// <param name="pallet"></param>
        /// <returns></returns>
        /// <remarks>
        /// Binod and DB: We are keeping SuggestedAreaId in LocationViewModel now. This allows us to check which area was suggested by program. 
        /// </remarks>
        private LocationViewModel PopulateLocationViewModel(string areaId, string buildingId, Pallet pallet)
        {
            //Getting suggested locations
            IEnumerable<CartonLocation> suggestedLocations = null;
            try
            {
                suggestedLocations = _service.SuggestLocationsForPallet(areaId, pallet.PalletId);
            }
            catch (Exception ex)
            {
                AddStatusMessage(ex.Message);
            }

            //Returning Location view
            var targetArea = _service.GetCartonArea(areaId);
            var lvm = new LocationViewModel
                          {
                              PalletId = pallet.PalletId,
                              PalletCartonCount = pallet.CartonCount,
                              PalletSkuCount = pallet.SkuCount,
                              CartonVwhCount = pallet.CartonVwhCount,
                              CartonVwhId = pallet.CartonVwhId,
                              SuggestedLocations = suggestedLocations == null ? null : suggestedLocations.Select(p => new CartonLocationModel
                              {
                                  AreaId = p.Area.AreaId,
                                  AreaShortName = p.Area.ShortName,
                                  LocationId = p.LocationId,
                                  CartonCount = p.CartonCount,
                                  MaxCartons = p.MaxCartons,
                                  SkuCount = p.SkuCount
                              }).ToList(),
                              PalletSku = pallet.PalletSku == null ? null : new SkuModel(pallet.PalletSku),
                              TargetAreaId = areaId,
                              SuggestedAreaShortName = suggestedLocations == null ? null : suggestedLocations.Max(p => p.Area.ShortName),
                              SuggestedAreaId = suggestedLocations == null ? null : suggestedLocations.Max(p => p.Area.AreaId),
                              TargetAreaShortName = targetArea.ShortName,
                              ReplenishAreaShortName = targetArea.ReplenishAreaShortName,
                              BuildingId = buildingId,
                              PalletAreaShortName = pallet.PalletArea.ShortName,
                              PalletLocation = pallet.LocationId
                          };
            return lvm;
        }


        /// <summary>
        /// For locating a pallet, Must be passed PalletId, LocationId where the pallet should be located, Building and Target Area.
        /// In success case we are required AreaId to redirect to Pallet view
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public virtual ActionResult LocatePallet(LocationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                //Enter pressed, must be located to Pallet view to change the pallet
                ModelState.Clear();
                return Pallet(PopulateAreaViewModel(model.BuildingId, model.TargetAreaId));
            }


            var locationId = string.IsNullOrEmpty(model.ConfirmLocationId) ? model.LocationId : model.ConfirmLocationId;

            //////////////////////////////// DB:Try Pallet Merging ///////////////////////////////////
            // Pallet is scanned by user instead of location. We propose merging the original pallet in it. 
            // If pallet to merge is not null it means that user has already confirmed merging of pallet.
            if (locationId.StartsWith("P") && string.IsNullOrEmpty(model.PalletToMerge))
            {
                var palletId = locationId;
                if (model.PalletId != palletId)
                {
                    // Need confirmation from user
                    var msg = string.Format("Do you want to merge the pallet {0} to {1}.Scan pallet {1} to merge, press enter to skip and continue again.", model.PalletId, palletId);
                    AddStatusMessage(msg);
                    var lvm = PopulateLocationViewModel(model.TargetAreaId, model.BuildingId, _service.GetPallet(model.PalletId));
                    lvm.PalletToMerge = palletId;
                    return View(Views.Location, lvm);
                }
                // Trying to merge to self?
                ModelState.AddModelError("", "You are trying to merge on same Pallet, scan another pallet.");
                return Location(new PalletViewModel
                                    {
                                        PalletOrCartonId = model.PalletId,
                                        AreaId = model.TargetAreaId,
                                        BuildingId = model.BuildingId
                                    });
            }

            if (!string.IsNullOrEmpty(model.PalletToMerge) && model.PalletToMerge == locationId)
            {
                // We need to merge the two pallets, while merging get location from PalletToMerge and pass it. This is done to take care of all rules.
                var palletInfo = _service.GetPallet(model.PalletToMerge);
                if (palletInfo == null)
                {
                    ModelState.AddModelError("", string.Format("Invalid Pallet {0} passed", model.PalletToMerge));
                    return Location(new PalletViewModel
                    {
                        PalletOrCartonId = model.PalletId,
                        AreaId = model.TargetAreaId,
                        BuildingId = model.BuildingId
                    });
                }
                locationId = palletInfo.LocationId;
                if (string.IsNullOrEmpty(locationId))
                {
                    var msg = string.Format("Pallet {0} does not exist at any location, therefore you can not merge pallet {1} into it.", model.PalletToMerge, model.PalletId);
                    ModelState.AddModelError("", msg);
                }
            }

            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            LocatingResult locResult = null;
            try
            {
                locResult = _service.QualifyLocationForPallet(model.PalletId, locationId);
                if (locResult.State.HasFlag(LocationStates.AssignmentViolation))
                {
                    var msg = string.Format("Can not locate the pallet {0} to location {1} which is assigned for SKU {2} only.", locResult.Pallet.PalletId, locResult.Location.LocationId, locResult.Location.AssignedSku);
                    ModelState.AddModelError("", msg);
                }
                if (locResult.State.HasFlag(LocationStates.VwhAssignmentViolation))
                {
                    var msg = string.Format("Can not locate the pallet {0} to location {1} which is assigned for VWh {2} only.", locResult.Pallet.PalletId, locResult.Location.LocationId, locResult.Location.AssignedVWhId);
                    ModelState.AddModelError("", msg);
                }

                // Denying if the user entered the location of another area not from suggested area.
                if (model.SuggestedAreaId != null && model.SuggestedAreaId != locResult.Location.Area.AreaId)
                {
                    var area = _service.GetCartonArea(model.SuggestedAreaId);
                    ModelState.AddModelError("", string.Format("Pallet {0} should be located in area {1}, but you are trying to locate in area {2}", model.PalletId, area.ShortName, locResult.Location.Area.ShortName));
                }
            }
            catch (ProviderException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            // Check whether location qualifies. For error states, add model errors.
            if (!ModelState.IsValid)
            {
                return Location(new PalletViewModel
                {
                    PalletOrCartonId = model.PalletId,
                    AreaId = model.TargetAreaId,
                    BuildingId = model.BuildingId
                });
            }

            //Asking for confirmation when location is Unavailable
            if (locResult.State.HasFlag(LocationStates.UnavailableLocation) && model.LocationId != model.ConfirmLocationId)
            {
                var pallet = _service.GetPallet(model.PalletId);

                //Getting suggested locations
                var lvm = PopulateLocationViewModel(model.TargetAreaId, model.BuildingId, pallet);
                lvm.LocationId = locationId;

                //Need confirmation
                var msg = string.Format("Scanned location {0} is unavailable, if you still want to locate pallet {1}, rescan the location {0}.", locationId, pallet.PalletId);
                AddStatusMessage(msg);
                lvm.Sound = 'W';
                return View(Views.ConfirmLocation, lvm);
            }

            //Asking for confirmation when capacity is being violated
            if (locResult.State.HasFlag(LocationStates.CapacityViolation) && model.LocationId != model.ConfirmLocationId)
            {
                var pallet = _service.GetPallet(model.PalletId);

                //Getting suggested locations
                var lvm = PopulateLocationViewModel(model.TargetAreaId, model.BuildingId, pallet);
                lvm.LocationId = locationId;
                //Need confirmation
                int val;
                //MaxCartons is nullable so trying to handle the null case
                var max = int.TryParse(locResult.Location.MaxCartons.ToString(), out val) ? val : 0;
                //Location Cartons count after current assignment will be done at location.
                var cartonCountAfterAssignment = Math.Abs(max - (locResult.Location.CartonCount + pallet.CartonCount));
                var msg = string.Format("You are locating <strong>{0}</strong> cartons at <strong>{1}</strong>. However, <strong>{1}</strong> already has <strong>{2}</strong> cartons and its capacity is <strong>{3}</strong> cartons. Therefore, the location will have <strong>{4}</strong> cartons too many. Scan the location again to locate anyway, or scan a different location.",
                    pallet.CartonCount, locationId, locResult.Location.CartonCount, string.IsNullOrEmpty(locResult.Location.MaxCartons.ToString()) ? "Unknown" : locResult.Location.MaxCartons.ToString(), cartonCountAfterAssignment);
                AddStatusMessage(msg);
                lvm.Sound = 'W';
                return View(Views.ConfirmLocation, lvm);
            }

            // 1.  Locate pallet
            // 2. merge pallet if PalletToMerge is not null.
            _service.LocateandMergePallet(locationId, model.PalletId, locResult.Location.Area.AreaId, model.PalletToMerge);
            if (!string.IsNullOrEmpty(model.PalletToMerge))
            {
                AddStatusMessage(string.Format("Pallet {0} successfully merged to {1} and  located at {2}.", model.PalletId, model.PalletToMerge, locationId));
            }
            else
            {
                AddStatusMessage(string.Format("Pallet {0} successfully located to {1}.", model.PalletId, locationId));
            }
            //If successfully updated, returns new PalletViewModel to Pallet View with previous state values like destination Area and Building
            return RedirectToAction(MVC_PalletLocating.PalletLocating.Home.ActionNames.Pallet,
                new AreaViewModel { AreaId = model.TargetAreaId, BuildingId = model.BuildingId, Sound = 'S' });
        }

        /// <summary>
        /// This method is use for showing the info of pallet located
        /// in last few hours.
        /// </summary>
        /// <param name="model"></param>
        /// <remarks>
        ///  This method is use for showing the info of pallet located
        /// in last few hours.
        /// </remarks>
        /// <returns></returns>
        [HttpGet]
        public virtual ActionResult LocatingActivity(PalletinfoViewModel model)
        {
            var result = _service.GetPalletMovements(model.UserName, model.InsertToDate, model.InsertFromDate);
            model.PalletInfo = result.Select(p => new PalletMovementModel
            {
                UserName = p.UserName,
                CountCarton = p.CountCarton,
                FromArea = p.FromArea,
                Pallet = p.Pallet,
                ToArea = p.ToArea,
                FromLocation = p.FromLocation,
                ToLocation = p.ToLocation,
                InsertDate = p.InsertDate
            });
            return View(Views.PalletLocatingInfo, model);
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