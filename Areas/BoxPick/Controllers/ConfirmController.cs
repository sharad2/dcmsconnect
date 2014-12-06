using System;
using System.Data.Common;
using System.Diagnostics.Contracts;
using System.Web.Mvc;
using DcmsMobile.BoxPick.Models;
using DcmsMobile.BoxPick.ViewModels;
using EclipseLibrary.Mvc.Helpers;

namespace DcmsMobile.BoxPick.Areas.BoxPick.Controllers
{
    [Route("confirm/{action}")]
    public partial class ConfirmController : BoxPickControllerBase
    {
        #region PartialPickPallet

        /// <summary>
        /// Prompts for confirming pallet.
        /// </summary>
        /// <remarks>
        /// Input Env: None
        /// Action: None
        /// Success Output: Display PartialPickPallet view. Session contains pallet.
        /// </remarks>
        [ActionName("PartialPickPallet")]
        public virtual ActionResult PartialPickPallet(MasterModelWithPallet model)
        {
            Contract.Requires(this.ModelState.IsValid, "Attributes should prevent invalid model from invoking this action");

            PartialPickPalletViewModel cpvm = new PartialPickPalletViewModel(this.Session);
            cpvm.Sound = 'W';
            return View(cpvm);
        }

        /// <summary>
        /// Remove remaining boxes from pallet if the confirmation scan matches.
        /// On success redirect to pallet page. On failure, redirect to carton page.
        /// </summary>
        /// <param name="model"></param>
        /// <returns>Pallet view on success. Carton view on failure</returns>
        /// <remarks>
        ///  Input Env: ConfirmPalletId is posted.
        /// Action: Remove remaining boxes from pallet  and redirects to pallet view
        /// Success Output:
        /// 1. If ConfirmPalletId is Empty, redirect with skipped status message to carton.
        /// 2. If ConfirmPalletId matches CurrentPalletId remove boxes and redirects to pallet view
        /// Failure Output:
        /// 1) ConfirmPalletId does not match CurrentPalletId. redirect to Carton.
        /// 2) if any other error occurs while removing boxes redirect to carton.
        /// </remarks>
        [HttpPost]
        [ActionName("PartialPickPallet")]
        public virtual ActionResult PartialPickPalletConfirm(PartialPickPalletViewModel model)
        {
            Contract.Requires(this.ModelState.IsValid, "Attributes should prevent invalid model from invoking this action");
            //TC 25 : Enter empty on confirm Pallet Screen
            if (string.IsNullOrWhiteSpace(model.ConfirmPalletId))
            {
                this.AddStatusMessage("Pallet skipping cancelled");
                return RedirectToAction(MVC_BoxPick.BoxPick.Home.AcceptCarton());
            }
            //TC 26 : Enter a different pallet Id on confirm Pallet screen
            if (!model.CurrentPalletId.Equals(model.ConfirmPalletId))
            {
                ModelState.AddModelError("", "The confirmation pallet did not match the original pallet.");
            }

            if (!ModelState.IsValid)
            {
                // Rescan does not match original pallet
                return RedirectToAction(MVC_BoxPick.BoxPick.Home.AcceptCarton());
            }
            try
            {
                var removed = _repos.Value.RemoveRemainingBoxesFromPallet(model.ConfirmPalletId);
                this.AddStatusMessage(string.Format("{0} boxes removed from Pallet {1}. Scan new Pallet.",
                    removed, model.ConfirmPalletId));

                switch (model.PickMode)
                {
                    case PickModeType.ADR:
                        model.Map(null);
                        return RedirectToAction(this.Actions.Print(model.ConfirmPalletId));

                    case PickModeType.ADREPPWSS:
                        return RedirectToAction(MVC_BoxPick.BoxPick.Home.AcceptPallet());

                    default:
                        throw new NotImplementedException();
                }
            }
            catch (DbException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            return RedirectToAction(MVC_BoxPick.BoxPick.Home.AcceptCarton());
        }

        #endregion

        #region Skip UCC

        /// <summary>
        /// If passed UCC does not match pattern, redirect to Carton view else prompt for confirmation
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <remarks>
        ///<para>
        /// Input Env:  
        /// Action: sets last uccId to Warning sound and validates it
        /// Success Output: Display SkipUcc view. Session contains pallet.
        /// Failure Output: Display Carton View. with errors  
        /// </para> 
        /// </remarks>
        [ActionName("SkipUcc")]
        public virtual ActionResult StartSkipUcc(MasterModelWithPallet model)
        {
            Contract.Requires(this.ModelState.IsValid, "Attributes should prevent invalid model from invoking this action");

            SkipUccViewModel suvm = new SkipUccViewModel(this.HttpContext.Session);
            suvm.Sound = 'W';
            return View(suvm);
        }

        /// <summary>
        /// Verifies that the passed scan matches the current scan in the model and then removes the box from the pallet.
        /// Always redirects to carton action.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <remarks>
        /// Input Env: ConfirmScan is posted.
        /// Action: Pick box and requery pallet.
        /// Success Output:
        /// 1. If ConfirmScan is Empty, redirect with skipped status message to carton. 
        /// 2. If ConfirmScan matches UccIdToPick remove box and requeries pallet
        ///     A.) If pallet full, redirect to pallet.
        ///     B.) If pallet still has boxes to pick, redirect to carton.
        /// Failure Output:
        /// 1) ConfirmScan does not match UccIdToPick. Reprompt for Carton.
        /// </remarks>
        [HttpPost]
        [ActionName("SkipUcc")]
        public virtual ActionResult SkipUcc(SkipUccViewModel model)
        {
            Contract.Requires(this.ModelState.IsValid, "Attributes should prevent invalid model from invoking this action");
            //TC 27 : Enter empty on confirm UCC screen
            if (string.IsNullOrEmpty(model.ConfirmScan))
            {
                this.AddStatusMessage("UCC skipping cancelled");
                //return RedirectToAction("Carton", "Home");
                return RedirectToAction(MVC_BoxPick.BoxPick.Home.AcceptCarton());
            }
            //TC 28 : Enter S on confirm UCC Skip screen or
            if (model.ConfirmScan == "S" || model.ConfirmScan == "s")
            {
                model.ConfirmScan = model.UccIdToPick;
            }

            //var fieldName = ReflectionHelpers.FieldNameFor((SkipUccViewModel m) => m.ConfirmScan);
            var fieldName = model.NameFor(m => m.ConfirmScan);

            //TC 29 : Scan anything different from UCC To Pick from pallet
            if (!model.ConfirmScan.Equals(model.UccIdToPick))
            {
                ModelState.AddModelError(fieldName, "The confirmation UCC scan did not match the original scan");
            }

            if (!ModelState.IsValidField(fieldName))
            {
                // Confirm scan does not match original
                return RedirectToAction(MVC_BoxPick.BoxPick.Home.AcceptCarton());
            }

            try
            {
                _repos.Value.RemoveBoxFromPallet(model.ConfirmScan, model.CurrentPalletId);
                this.AddStatusMessage(string.Format("UCC {0} removed from Pallet {1}. Please place a red dot on the UCC label.",
                    model.ConfirmScan, model.CurrentPalletId));
            }
            catch (DbException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            if (!ModelState.IsValid)
            {
                return RedirectToAction(MVC_BoxPick.BoxPick.Home.AcceptCarton());
            }

            Contract.Assert(this.ModelState.IsValid, "We have already handled the invalid cases");

            //Requery pallet
            Pallet pallet = null;
            try
            {
                pallet = _repos.Value.RetrievePalletInfo(model.CurrentPalletId);
                //TODO : Needs to define
                if (pallet == null)
                {
                    ModelState.AddModelError("", string.Format("Pallet {0} is not available for picking anymore. Please start over.", model.CurrentPalletId));
                }
                else if (pallet.IsFull)
                {
                    this.AddStatusMessage(string.Format("Pallet {0} has completed. Please scan new pallet.", model.CurrentPalletId));
                }
                else
                {
                    model.Map(pallet);
                    if (!TryValidateModel(model))
                    {
                        ModelState.AddModelError("", string.Format("Pallet {0} is invalid. Please contact System Administrator", model.CurrentPalletId));
                    }
                }
            }
            catch (DbException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            if (!ModelState.IsValid)
            {
                model.Map(null);
                return RedirectToAction(MVC_BoxPick.BoxPick.Home.AcceptPallet());
            }

            if (pallet.IsFull)
            {
                switch (model.PickMode)
                {
                    case PickModeType.ADR:
                        string palletId = model.CurrentPalletId;
                        model.Map(null);
                        return RedirectToAction(this.Actions.Print(palletId));

                    case PickModeType.ADREPPWSS:
                        model.Map(null);
                        return RedirectToAction(MVC_BoxPick.BoxPick.Home.AcceptPallet());

                    default:
                        throw new NotImplementedException();
                }
            }
            else
            {
                return RedirectToAction(MVC_BoxPick.BoxPick.Home.AcceptCarton());
            }
        }
        #endregion

        #region ConfirmADRPallet

        /// <summary>
        /// Scan new pallet
        /// </summary>
        /// <param name="palletId"></param>
        /// <returns></returns>
        [HttpGet]
        [ActionName("ADRPallet")]
        public virtual ActionResult ADRPallet(string palletId)
        {
            var model = new ADRPalletViewModel(this.Session)
            {
                ConfirmPalletId = palletId
            };
            return View(Views.ADRPallet, model);
        }

        /// <summary>
        /// Confirm to create new ADR pallet
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("ADRPallet")]
        public virtual ActionResult ConfirmADRPallet(ADRPalletViewModel model)
        {
            //Sent to pallet scan 
            //TC 30 : Scan null in confirm screen to create new ADR pallet
            if (string.IsNullOrEmpty(model.Scan))
            {
                return RedirectToAction(MVC_BoxPick.BoxPick.Home.AcceptPallet());
            }

            //TC 31 : Enter different pallet Id in confirm screen to create new pallet
            if (model.ConfirmPalletId != model.Scan)
            {
                this.AddStatusMessage("The confirmation pallet did not match the original pallet.");
                return RedirectToAction(MVC_BoxPick.BoxPick.Home.AcceptPallet());
            }

            //Create pallet for ADR
            try
            {
                int rowsAffected = _repos.Value.CreateADRPallet(model.ConfirmPalletId, model.CurrentBuildingId, model.CurrentDestinationArea);
                if (rowsAffected == 0)
                {
                    this.AddStatusMessage(string.Format("No cartons to pick for building {0}{1}", model.CurrentBuildingId,
                        !string.IsNullOrEmpty(model.CurrentDestAreaShortName) ? string.Format(", area {0}", model.CurrentDestAreaShortName) : string.Empty));
                    return RedirectToAction(MVC_BoxPick.BoxPick.Home.AcceptPallet());
                }
                this.AddStatusMessage(string.Format("{0:d} carton to be picked", rowsAffected));
            }
            catch (DbException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            //Sent to pallet scan, CreateADRPallet should return exception when not able to create pallet
            if (!ModelState.IsValid)
            {
                return RedirectToAction(MVC_BoxPick.BoxPick.Home.AcceptPallet());
            }

            Pallet pallet = null;
            try
            {
                pallet = _repos.Value.RetrievePalletInfo(model.ConfirmPalletId);
                if (pallet == null)
                {
                    ModelState.AddModelError("", "No cartons to pick");
                }
                else if (pallet.IsFull)
                {
                    //This is defensive check
                    this.AddStatusMessage(string.Format("Pallet {0} has been already picked. Please scan new pallet.", model.ConfirmPalletId));
                }
                else if (pallet.BuildingId != model.CurrentBuildingId)
                {
                    ModelState.AddModelError("", string.Format("Pallet {0} is for building {1} and not for current building {2}, Please scan new pallet",
                        model.ConfirmPalletId, pallet.BuildingId, model.CurrentBuildingId));
                }
                else if (string.IsNullOrEmpty(model.CurrentSourceArea))
                {
                    // We have chosen the area on user's behalf
                    model.CurrentSourceArea = pallet.CartonSourceArea;
                    model.CurrentSourceAreaShortName = pallet.SourceAreaShortName;
                }
                else if (pallet.CartonSourceArea != model.CurrentSourceArea)
                {
                    // Pallet must be of the correct source area
                    ModelState.AddModelError("", string.Format("Pallet {0} is for area {1} and not for current area {2}, Please scan new pallet",
                            model.ConfirmPalletId, pallet.SourceAreaShortName, model.CurrentSourceAreaShortName));
                }
                //Showing cartons destination area in where the carton to picked
                model.CurrentDestinationArea = pallet.DestinationArea;
                model.CurrentDestAreaShortName = pallet.DestAreaShortName;
            }
            catch (DbException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            MasterModelWithPallet mm = new MasterModelWithPallet(this.HttpContext.Session);
            if (ModelState.IsValid)
            {
                mm.Map(pallet);
                TryValidateModel(mm);
            }

            if (!ModelState.IsValid || pallet.IsFull)
            {
                mm.Map(null);
                return RedirectToAction(MVC_BoxPick.BoxPick.Home.AcceptPallet());
            }
            return RedirectToAction(MVC_BoxPick.BoxPick.Home.AcceptCarton());
        }
        #endregion

        #region Confirm Suspense Carton

        /// <summary>
        /// GET method to populate the view of Suspense Carton confirmation
        /// </summary>
        /// <param name="scannedCartonId"></param>
        /// <returns></returns>
        [HttpGet]
        [ActionName("ConfirmSuspenseCarton")]
        public virtual ActionResult SuspenseCarton(string scannedCartonId)
        {
            var cvm = new CartonViewModel(this.HttpContext.Session);
            cvm.ScannedCartonId = scannedCartonId;
            cvm.Sound = 'W';
            return View(Views.ConfirmSuspenseCarton, cvm);
        }


        /// <summary>
        /// This is a POST method which will redirect to Home/AcceptCarton after putting the carton in suspense if confirmed
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("ConfirmSuspenseCarton")]
        public virtual ActionResult ConfirmSuspenseCarton(CartonViewModel model)
        {
            //TC 32 : Scan ! on confir suspense screen
            if (model.ConfirmCartonId == "1")
            {
                this.AddStatusMessage(string.Format("Carton {0} put in suspense.", model.CartonIdToPick));
                _repos.Value.MarkCartonInSuspense(model.CartonIdToPick);
            }
            else
            {
                this.AddStatusMessage(string.Format("The carton {0} was not put in suspense.", model.CartonIdToPick));
            }
            model.SuspenseFlag = true;
            return RedirectToAction(MVC_BoxPick.BoxPick.Home.AcceptCarton()
                .AddRouteValue(model.NameFor(m => m.ScannedCartonId), model.ScannedCartonId)
                .AddRouteValue(model.NameFor(m => m.SuspenseFlag), model.SuspenseFlag));
        }
        #endregion


        #region Print


        [ActionName("Print")]
        [HttpGet]
        public virtual ActionResult Print(string palletId)
        {
            var model = new PrinterViewModel(this.Session)
            {
                PalletToPrint = palletId
            };
            if (!TryValidateModel(model))
            {
                return RedirectToAction(MVC_BoxPick.BoxPick.Home.AcceptPallet());
            }

            model.Sound = 'W';
            return View(Views.Print, model);
        }

        [ActionName("Print")]
        [HttpPost]
        public virtual ActionResult PrintPallet(PrinterViewModel model)
        {
            //TC 33 : Enter empty on print pallet screen
            if (string.IsNullOrEmpty(model.Scan))
            {
                return RedirectToAction(MVC_BoxPick.BoxPick.Home.AcceptPallet());
            }
            //TC 34 : Enter invalid string on Print Pallet screen
            if (!model.PalletToPrint.Equals(model.Scan))
            {
                this.AddStatusMessage("The confirmation pallet did not match the original pallet.");
            }

            //Sent to pallet scan 
            if (!ModelState.IsValid)
            {
                return RedirectToAction(MVC_BoxPick.BoxPick.Home.AcceptPallet());
            }

            try
            {
                string labelPrinter = string.Empty;
                string printers = _repos.Value.GetPrinter(model.PalletToPrint);
                //TC 35 : When no printer found
                if (string.IsNullOrWhiteSpace(printers))
                {
                    ModelState.AddModelError("", "No printer found, Please contact your system administrator");
                }
                else
                {
                    string[] printer = printers.Trim().Split(',');
                    if (string.IsNullOrEmpty(printer[0]))
                    {
                        ModelState.AddModelError("", "Label printer not found, Please contact your system administrator");
                    }
                    else
                    {
                        labelPrinter = printer[0];
                    }
                }

                if (ModelState.IsValid)
                {
                    _repos.Value.Print(model.PalletToPrint, labelPrinter);
                    this.AddStatusMessage(string.Format("Pallet {0} printed on {1} printer.", model.PalletToPrint, labelPrinter));
                }
            }
            catch (DbException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            return RedirectToAction(MVC_BoxPick.BoxPick.Home.AcceptPallet());
        }

        #endregion
    }
}



//$Id$