using DcmsMobile.BoxPick.Models;
using DcmsMobile.BoxPick.ViewModels;
using EclipseLibrary.Mvc.Helpers;
using System;
using System.Data.Common;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Web.Mvc;

namespace DcmsMobile.BoxPick.Areas.BoxPick.Controllers
{
    /// <summary>
    /// Controller for the BoxPick Application
    /// </summary>
    /// <remarks>
    /// <list type="table">
    /// <item>
    /// <term>
    /// <see cref="AcceptCarton" />
    /// </term>
    /// <description>
    /// No validations are performed. The screen requesting UCC is displayed.
    /// </description>
    /// </item>
    /// </list>
    /// <code lang="sql">
    /// alter user boxpick grant connect through dcms8
    /// </code>
    /// <para>
    /// Productivity: We track the time between when the carton was proposed and when it was picked. This duration is inserted as
    /// part of productivity. All times are database server times. Any successful skipping activity resets the time.
    /// </para>
    /// </remarks>
    [Route("{action}")]
    public partial class HomeController : BoxPickControllerBase
    {
        #region Building

        /// <summary>
        /// Accepts building.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// Tests:
        /// Input environment: Valid building is 1 to 5 characters.
        /// Action: For valid building, perform query to check in database.
        /// Output: Model contains entered building.
        /// Success Output: Pallet view is displayed.
        /// Failure output: Model errors exist for building field. Building view is redisplayed.
        /// </para>
        /// <para>
        /// Sharad 14 mar 2012: Fixed bug. The area was forgotten if the user did not confirm pallet scan.
        /// </para>
        /// </remarks>
        [ActionName("Index")]
        [Route(Name = DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_BoxPick)]
        public virtual ActionResult AcceptBuilding(BuildingViewModel model)
        {
            string currentBuildingId;
            string currentSaurceArea;
            string currentSaurceAreaShortName;

            //TC1 : Press enter in textbox on building view.
            if (string.IsNullOrEmpty(model.ScannedBuildingOrArea))
            {
                //TC2 : Press enter in textbox on building area view or pallet view..
                if (!string.IsNullOrEmpty(model.CurrentBuildingId))
                {
                    model.CurrentBuildingId = model.CurrentBuildingId.ToUpper();
                    // If building is set, just proceed because area is optional
                    currentBuildingId = model.CurrentBuildingId;
                    currentSaurceArea = model.CurrentSourceArea;
                    currentSaurceAreaShortName = model.CurrentSourceAreaShortName;
                    MasterModel.ClearSessionValues(this.Session);
                    model.CurrentBuildingId = currentBuildingId;
                    model.CurrentSourceArea = currentSaurceArea;
                    model.CurrentSourceAreaShortName = currentSaurceAreaShortName;
                    return View(Views.Pallet, new PalletViewModel(this.Session));
                }
                // Building is not set. Start over.
                MasterModel.ClearSessionValues(this.Session);
                return View(Views.Building, model);
            }

            var fieldName = model.NameFor(m => m.ScannedBuildingOrArea);
            try
            {
                PickContext pickContext = _repos.Value.GetPickContext(model.ScannedBuildingOrArea);
                //TC3 : Scan invlaid building or area.
                if (pickContext == null)
                {
                    // Bad scan. Preserve current building and area
                    ModelState.AddModelError(fieldName, string.Format("{0} is not a valid Building or Area", model.ScannedBuildingOrArea));
                    currentBuildingId = model.CurrentBuildingId;
                    currentSaurceArea = model.CurrentSourceArea;
                    currentSaurceAreaShortName = model.CurrentSourceAreaShortName;
                }
                else
                {
                    //TC4 : On first scan of building we reach here
                    if (IsInitialScan(model))
                    {
                        // Both values are
                        currentBuildingId = pickContext.BuildingId;
                        currentSaurceArea = pickContext.SourceArea;
                        currentSaurceAreaShortName = pickContext.SourceAreaShortName;
                    }
                    else if (pickContext.IsBuilding)
                    {
                        // Accept the building only
                        currentBuildingId = pickContext.BuildingId;
                        currentSaurceAreaShortName = model.CurrentSourceAreaShortName;
                        currentSaurceArea = model.CurrentSourceArea;
                    }
                    else if (pickContext.IsMultiBuildingArea)
                    {
                        currentBuildingId = model.CurrentBuildingId;
                        currentSaurceArea = pickContext.SourceArea;
                        currentSaurceAreaShortName = pickContext.SourceAreaShortName;
                    }
                    else if (pickContext.IsSingleBuildingArea)
                    {
                        // Ignore the area if it belongs to a different building
                        if (string.IsNullOrEmpty(model.CurrentBuildingId) || model.CurrentBuildingId == pickContext.BuildingId)
                        {
                            currentBuildingId = pickContext.BuildingId;
                            currentSaurceArea = pickContext.SourceArea;
                            currentSaurceAreaShortName = pickContext.SourceAreaShortName;
                        }
                        else
                        {
                            ModelState.AddModelError("", string.Format("Area {0} doesn't belong to building {1}", pickContext.SourceArea, model.CurrentBuildingId));
                            currentBuildingId = model.CurrentBuildingId;
                            currentSaurceAreaShortName = string.Empty;
                            currentSaurceArea = string.Empty;
                        }
                    }
                    else
                    {
                        throw new Exception("We never expect to get here");
                    }
                }
            }
            catch (DbException ex)
            {
                ModelState.AddModelError(fieldName, ex.Message);
                currentBuildingId = model.CurrentBuildingId;
                currentSaurceArea = model.CurrentSourceArea;
                currentSaurceAreaShortName = model.CurrentSourceAreaShortName;
            }
            MasterModel.ClearSessionValues(this.Session);
            model.CurrentBuildingId = currentBuildingId;
            model.CurrentSourceArea = currentSaurceArea;
            model.CurrentSourceAreaShortName = currentSaurceAreaShortName;
            if (!ModelState.IsValid || string.IsNullOrEmpty(model.CurrentBuildingId) || string.IsNullOrEmpty(model.CurrentSourceArea))
            {
                return View(Views.Building, model);
            }

            return View(Views.Pallet, new PalletViewModel(this.Session));
        }

        private static bool IsInitialScan(BuildingViewModel model)
        {
            return string.IsNullOrEmpty(model.CurrentBuildingId) && string.IsNullOrEmpty(model.CurrentSourceArea);
        }


        #endregion

        #region Pallet
        /// <summary>
        /// Step 2: Receiving Pallet. Will ask for Carton
        /// </summary>
        /// <param name="mm"></param>
        /// <returns></returns>
        /// <remarks>
        /// Input Env: ScannedPalletId is posted with valid pattern.
        /// Action: Query the pallet. Failure is pallet does not exist in database or is not pickable.
        /// Success Output: Display carton view. Session contains pallet.
        /// Failure Output: Redisplay Pallet View. ScannedPalletId field is invalid. Session does not contain any pallet.
        /// </remarks>
        [ActionName("Pallet")]
        public virtual ActionResult AcceptPallet(PalletViewModel mm)
        {
            //TC5 : Scan null or empty in textbox on Pallet page.
            if (string.IsNullOrEmpty(mm.ScannedPalletId))
            {
                return RedirectToAction(Actions.AcceptBuilding());
            }
            mm.ScannedPalletId = mm.ScannedPalletId.ToUpper();
            //TC6 : Scan a invalid pallet i.e. a pallet that do not starts with P 
            if (!ModelState.IsValid)
            {
                return View(Views.Pallet, mm);
            }

            Pallet pallet = null;
            var fieldScannedPalletId = mm.NameFor(m => m.ScannedPalletId);
            try
            {
                pallet = _repos.Value.RetrievePalletInfo(mm.ScannedPalletId);
                //TC7 : Scan a new pallet to create ADR pallet.
                if (pallet == null)
                {
                    return RedirectToAction(MVC_BoxPick.BoxPick.Confirm.Actions.ADRPallet(mm.ScannedPalletId));

                }
                //TC8 : Scan a pallet for which TotalBoxes are equal to PickedBoxes
                else if (pallet.IsFull)
                {
                    this.AddStatusMessage(string.Format("Pallet {0} has been already picked.", mm.ScannedPalletId));
                    return RedirectToAction(MVC_BoxPick.BoxPick.Confirm.Print(mm.ScannedPalletId));
                }
                //TC9 : Scan a pallet that doesnot belongs to current scanned building
                else if (pallet.BuildingId != mm.CurrentBuildingId)
                {
                    ModelState.AddModelError(fieldScannedPalletId, string.Format("Pallet {0} is for building {1} and not for current building {2}, Please scan new pallet",
                        mm.ScannedPalletId, pallet.BuildingId, mm.CurrentBuildingId));
                }
                //TC10 : If we don't enter any area then scanned pallet area will be treated as current source area.
                else if (string.IsNullOrEmpty(mm.CurrentSourceArea))
                {
                    // We have chosen the area on user's behalf
                    mm.CurrentSourceArea = pallet.CartonSourceArea;
                    mm.CurrentSourceAreaShortName = pallet.SourceAreaShortName;
                }
                //TC11 : If scanned palletArea is not same as currten pallet area
                else if (pallet.CartonSourceArea != mm.CurrentSourceArea)
                {
                    // Pallet must be of the correct destination area
                    ModelState.AddModelError(fieldScannedPalletId, string.Format("Pallet {0} is for area {1} and not for current area {2}, Please scan new pallet",
                            mm.ScannedPalletId, pallet.SourceAreaShortName, mm.CurrentSourceAreaShortName));
                }
                //Showing cartons destination area in where the carton to picked
                mm.CurrentDestinationArea = pallet.DestinationArea;
                mm.CurrentDestAreaShortName = pallet.DestAreaShortName;
            }
            catch (DbException ex)
            {
                ModelState.AddModelError(fieldScannedPalletId, ex.Message);
            }

            var model = new MasterModelWithPallet(this.HttpContext.Session);
            if (ModelState.IsValid)
            {
                model.Map(pallet);
                TryValidateModel(model, fieldScannedPalletId);
            }

            if (!ModelState.IsValid || pallet.IsFull)
            {
                model.Map(null);
                return View(Views.Pallet, mm);
            }

            return View(Views.Carton, new CartonViewModel(this.HttpContext.Session));
        }

        #endregion

        #region Carton
        /// <summary>
        /// Unconstrained action. Step 3: Receiving carton. Asking for UCC
        /// </summary>
        /// <param name="model">Only PalletId and CartonId will be populated</param>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// Prompts the user to enter UCC.
        /// </para>
        /// <para>
        /// 
        /// Input Env: ScannedCartonId is posted.
        /// Action: Query the posted carton. Failure if carton does not exist in database or does not qualify for the current box to pick.
        /// Success Output: Display UCC view. Carton is available in MasterModel.LastCartonId and is stored in session.
        /// Failure Output: Redisplay Carton View. Last carton does not change. Empty carton id redisplays view but does not make model invalid.
        /// </para>
        /// </remarks>
        [ActionName("Carton")]
        public virtual ActionResult AcceptCarton(CartonViewModel model)
        {
            Contract.Requires(this.ModelState.IsValid, "Attributes should prevent invalid model from invoking this action");
            //TC12 : Scan null Or empty enter in Carton Text box on carton page.
            if (string.IsNullOrWhiteSpace(model.ScannedCartonId))
            {
                // Clear existing carton if any
                return View(Views.Carton, model);
            }
            //TC13 : Enter S on textbox on carton page.
            if (model.ScannedCartonId == "S" || model.ScannedCartonId == "s")
            {
                return RedirectToAction(MVC_BoxPick.BoxPick.Confirm.SkipUcc());
            }
            model.ScannedCartonId = model.ScannedCartonId.ToUpper();
            // Validate carton. If box is returned as null then carton was not pickable.

            var box = _repos.Value.GetBoxForCarton(model.ScannedCartonId, model.CurrentPalletId, model.UccIdToPick);

            //TC14 : If no information of scanned carton found
            //  Check whether the scanned carton can be placed in current pallet.  
            if (box == null)
                ModelState.AddModelError(model.NameFor(m => m.ScannedCartonId), "Carton cannot be picked for this pallet");
            if (!ModelState.IsValid)
            {
                //Bad carton
                return View(Views.Carton, model);
            }

            //TODO : Needs to define
            if (!model.SuspenseFlag && model.CurrentLocationId != box.AssociatedCarton.LocationId && !string.IsNullOrEmpty(model.CartonIdToPick))
            {
                // Now carton is being picked from another location. Ask to mark it in suspense.
                return RedirectToAction(MVC_BoxPick.BoxPick.Confirm.Actions.SuspenseCarton(model.ScannedCartonId));
            }
            var uvm = new UccViewModel(this.HttpContext.Session);
            uvm.SetLastCartonAndLocation(box.AssociatedCarton.CartonId, box.AssociatedCarton.LocationId);

            switch (model.PickMode)
            {
                //TC16 : When Pickmode of pallet is ADR
                case PickModeType.ADR:
                    uvm.ScannedUccId = box.UccId;
                    return AcceptUcc(uvm);
                //TC17 : When Pickmode of pallet is ADREPPWSS
                case PickModeType.ADREPPWSS:
                    return View(Views.Ucc, uvm);

                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// We get here when a UCC is scanned when a carton is expected.
        /// We ask for a rescan and then put a red dot on the UCC.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// Input Env: ScannedCartonId is posted but it actually contains a UCC.
        /// Action: None.
        /// Success Output: ScannedCartonId matches MasterModelWithPallet.UccIdToPick. Redirect to SkippUcc in ConfirmController.
        /// Failure Output: Redisplay Carton View.
        /// </para>
        /// </remarks>
        [DcmsMobile.BoxPick.Helpers.FormActionSelector(typeof(CartonViewModel), "scan", Box.REGEX_UCC)]
        [ActionName("Carton")]
        public virtual ActionResult AcceptUccInCarton(CartonViewModel model)
        {
            Contract.Requires(this.ModelState.IsValid, "Attributes should prevent invalid model from invoking this action");
            var fieldName = model.NameFor(m => m.ScannedCartonId);
            //TC18 : Scan a UCC to skip that is not expected to get picked.
            if (model.UccIdToPick != model.ScannedCartonId)
            {
                ModelState.AddModelError(fieldName, string.Format("Can only skip the current UCC {0}.", model.UccIdToPick));
                return View(Views.Carton, model);
            }
            return RedirectToAction(MVC_BoxPick.BoxPick.Confirm.SkipUcc());
        }

        /// <summary>
        /// We get here if a pallet was scanned when a carton was expected. If the scanned pallet does not match the
        /// current pallet, the scan is ignored with an error. Otherwise we navigate to confirm pallet screen.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// Input Env: ScannedCartonId is posted but it actually contains a Pallet.
        /// Action: None.
        /// Success Output: ScannedCartonId matches MasterModelWithPallet.CurrentPalletId. Redirect to PartialPickPallet in ConfirmController.
        /// Failure Output: Redisplay Carton View.
        /// </para>
        /// </remarks>
        [DcmsMobile.BoxPick.Helpers.FormActionSelector(typeof(CartonViewModel), "scan", PalletViewModel.REGEX_PALLET)]
        [ActionName("Carton")]
        public virtual ActionResult AcceptPalletInCarton(CartonViewModel model)
        {
            Contract.Requires(this.ModelState.IsValid, "Attributes should prevent invalid model from invoking this action");

            var fieldName = model.NameFor(m => m.ScannedCartonId);
            return BeginPartialPickPallet(model, fieldName, Views.Carton);
        }

        #endregion

        #region Ucc

        /// <summary>
        /// Step 4: Receiving UCC. At this point the database will be updated to indicate that the box has been picked.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// Input Env: ScannedUccId is posted.
        /// Action: Pick box and requery pallet.
        /// Success Output:
        /// 1. If pallet full, redirect to pallet.
        /// 2. If pallet still has boxes to pick, redirect to carton.
        /// Failure Output:
        /// 1) ScannedUccId does not match UccIdToPick. Reprompt for UCC.
        /// 2) ScannedUccId not suitable for carton. Reprompt for carton.
        /// </para>
        /// </remarks>
        [ActionName("Ucc")]
        [DcmsMobile.BoxPick.Helpers.FormActionSelector(typeof(UccViewModel), "scan", Box.REGEX_UCC)]
        public virtual ActionResult AcceptUcc(UccViewModel model)
        {
            model.ScannedUccId = model.ScannedUccId.ToUpper();
            var fieldName = model.NameFor(m => m.ScannedUccId);
            //TC19 : Needs to validate that only proposed Ucc can be removed from pallet
            if (model.ScannedUccId != model.UccIdToPick && model.PickMode == PickModeType.ADREPPWSS)
            {
                ModelState.AddModelError(fieldName, "Please scan the UCC being proposed.");
            }

            if (!ModelState.IsValid)
            {
                return View(Views.Ucc, model);
            }

            try
            {
                // Pick box now
                _repos.Value.PickCarton(model.ScannedUccId, model.LastCartonId, model.ProductivityStartTime.Value);
                if (model.CountRequiredVAS > 0)
                {
                    this.AddStatusMessage(string.Format("Carton {0} associated with Box {1} picked.VAS required on this box please take this box to VAS area", model.LastCartonId, model.ScannedUccId));
                }
                else
                {
                    this.AddStatusMessage(string.Format("Carton {0} associated with Box {1} picked.", model.LastCartonId, model.ScannedUccId));
                }
                model.SetLastUccPicked(model.ScannedUccId);
            }
            catch (DbException ex)
            {
                // Carton not suitable for box
                ModelState.AddModelError(fieldName, ex.Message);
            }

            if (!ModelState.IsValid)
            {
                return View(Views.Carton, new CartonViewModel(this.Session));
            }

            Contract.Assert(this.ModelState.IsValid, "We have already handled the invalid cases");

            // Requery pallet
            Pallet pallet = null;
            var fieldCurrentPallet = model.NameFor(m => m.CurrentPalletId);
            try
            {
                pallet = _repos.Value.RetrievePalletInfo(model.CurrentPalletId);
                //TC20 : Pick a pallet that is available for only one box.
                if (pallet == null)
                {
                    ModelState.AddModelError("", string.Format("Pallet {0} does not available for picking anymore, Please start over", model.CurrentPalletId));
                }
                //TC21 : Pick a pallet whose last box is remain to get picked.
                else if (pallet.IsFull)
                {
                    this.AddStatusMessage(string.Format("Pallet {0} has completed. Please scan new pallet.", model.CurrentPalletId));
                }
                else
                {
                    model.Map(pallet);
                    if (!TryValidateModel(model))
                    {
                        ModelState.AddModelError(fieldCurrentPallet, string.Format("Pallet {0} is invalid. Please contact System Administrator", model.CurrentPalletId));
                    }
                }
            }
            catch (DbException ex)
            {
                ModelState.AddModelError(fieldCurrentPallet, ex.Message);
            }

            if (!ModelState.IsValid)
            {
                // Get rid of this invalid pallet
                model.Map(null);
                return RedirectToAction(Actions.AcceptPallet());
            }

            if (pallet.IsFull)
            {
                switch (model.PickMode)
                {
                    case PickModeType.ADR:
                        string palletId = model.CurrentPalletId;
                        model.Map(null);
                        return RedirectToAction(MVC_BoxPick.BoxPick.Confirm.Print(palletId));

                    case PickModeType.ADREPPWSS:
                        model.Map(null);
                        return RedirectToAction(Actions.AcceptPallet());

                    default:
                        throw new NotImplementedException();
                }
            }
            return View(Views.Carton, new CartonViewModel(this.Session));
        }

        /// <summary>
        /// Unconstrained. If a carton is scanned when UCC is expected, we make it the current carton.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// BUG: Any non UCC and non Pallet scan is treated as a carton
        /// </para>
        /// <para>
        /// Input Env: ScannedUccId is actually a carton.
        /// Action: Provide the exact same behavior which would have occurred if carton scanned on carton screen.
        /// Sound: Warning
        /// </para>
        /// </remarks>
        [ActionName("Ucc")]
        public virtual ActionResult AcceptCartonInUcc(UccViewModel uvm)
        {
            //TC22 : Scan nothing or empty enter on UCC page.
            if (string.IsNullOrEmpty(uvm.ScannedUccId))
            {
                return View(Views.Ucc, uvm);
            }

            return RedirectToAction(Actions.AcceptUccInCarton()
                .AddRouteValue(ReflectionHelpers.NameFor((CartonViewModel m) => m.ScannedCartonId), uvm.ScannedUccId));

        }

        /// <summary>
        /// A pallet has been scanned when a UCC was expected. Navigate to pallet confirmation screen.
        /// If the scanned pallet is not the current pallet, it is ignored with error.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Tests: Similar to tests for AcceptPalletInCarton
        /// </remarks>
        [DcmsMobile.BoxPick.Helpers.FormActionSelector(typeof(UccViewModel), "scan", PalletViewModel.REGEX_PALLET)]
        [ActionName("Ucc")]
        public virtual ActionResult AcceptPalletInUcc(UccViewModel model)
        {
            Contract.Requires(this.ModelState.IsValid, "Attributes should prevent invalid model from invoking this action");
            var fieldName = model.NameFor(m => m.ScannedUccId);
            return BeginPartialPickPallet(model, fieldName, Views.Ucc);
        }

        #endregion

        #region Common functions


        private ActionResult BeginPartialPickPallet(MasterModelWithPallet model, string fieldName, string viewName)
        {
            //Retrieve value from the model rather than from form based on fieldName
            PropertyInfo property = model.GetType().GetProperty(fieldName);
            string palletId = property.GetValue(model, null).ToString();
            //TC23 : Scan a new pallet on Carton or UCC view.
            if (palletId == null || model.CurrentPalletId != palletId)
            {
                ModelState.AddModelError(fieldName, "Cannot start a new pallet until the current pallet has been completed.");
                return View(viewName, model);
            }
            return RedirectToAction(MVC_BoxPick.BoxPick.Confirm.PartialPickPallet());
        }

        #endregion

    }

}



//$Id$