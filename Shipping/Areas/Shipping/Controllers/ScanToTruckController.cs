﻿using System;
using System.Configuration;
using System.Data.Common;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using System.Web.Routing;
using DcmsMobile.Shipping.Repository.ScanToTruck;
using DcmsMobile.Shipping.ViewModels;
using DcmsMobile.Shipping.ViewModels.ScanToTruck;
using EclipseLibrary.Mvc.Controllers;

namespace DcmsMobile.Shipping.Areas.Shipping.Controllers
{
    //Reviewed By: Ravneet, Binod and Deepak 15 Dec 2012
    //Rajesh Kandari 26 Dec 2012: Removed area choice for loading pallets.
    [AuthorizeEx("Scan To Truck requires Role {0}", Roles = "DCMS8_SCANTOTRUCK", Purpose = "Enables you to load boxes on truck.")]
    public partial class ScanToTruckController : EclipseController
    {
        #region Intialization

        /// <summary>
        /// Required by T4MVC
        /// </summary>
        public ScanToTruckController()
        {

        }

        private ScanToTruckService _service;

        internal ScanToTruckService Service
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
#if DEBUG
                // Generate a random user name if user is not logged in. This helps in concurrency testing.
                if (string.IsNullOrWhiteSpace(userName))
                {
                    // See whether we have already assigned a user name to this request
                    // _layoutShipping.cshtml also expects that fake user name will be stored in this.Session["user"]
                    userName = this.Session["user"] as string;
                }
                if (string.IsNullOrWhiteSpace(userName))
                {
                    // If not, generate a random name now
                    userName = "_Debug" + (DateTime.Now.Ticks % 100).ToString();
                    this.Session["user"] = userName;
                }
#endif
                var clientInfo = string.IsNullOrEmpty(requestContext.HttpContext.Request.UserHostName) ? requestContext.HttpContext.Request.UserHostAddress :
                    requestContext.HttpContext.Request.UserHostName;
                _service = new ScanToTruckService(requestContext.HttpContext.Trace, connectString, userName, clientInfo);
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

        private readonly static Regex __regexPallet = new Regex(@"^([P|p]\S{1,19}$)");

        protected override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            base.OnActionExecuted(filterContext);
            var vr = filterContext.Result as ViewResult;
            if (vr != null)
            {
                var model = vr.Model as LayoutTabsViewModel;
                if (model != null)
                {
                    // Redirect to Summary page for selected customer.
                    switch (model.SelectedIndex)
                    {
                        case LayoutTabPage.ScanToTruck:
                            model.CustomerFormUrl = Url.Action(MVC_Shipping.Shipping.Home.RoutingSummary());
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                }
            }
        }

        /// <summary>
        /// Show Index view.
        /// </summary>
        /// <returns></returns>
        public virtual ActionResult Index()
        {
            return View(Views.Index, new IndexViewModel());
        }

        /// <summary>
        /// Gets details of appointment for passed building and area.
        /// </summary>
        /// <param name="ivm">
        /// AppointmentNo will be posted
        /// </param>
        /// <returns></returns>
        public virtual ActionResult Appointment(IndexViewModel ivm)
        {
            //TC1: Appointment number must be passed.
            if (ivm.AppointmentNo == null)
            {
                ModelState.AddModelError("", "Appointment Number is required.");
                ivm.Sound = (char)Sound.Error;
                return View(Views.Index, ivm);
            }
            var appoint = _service.GetAppointmentInfo(ivm.AppointmentNo.Value);
            
            //TC2: Scanned appointment does not exist.
            if (appoint == null)
            {
                AddStatusMessage(string.Format("Appointment does not exist {0}.", ivm.AppointmentNo));
                return View(Views.Index, ivm);
            }
            var model = new PalletViewModel
            {
                AppointmentNumber = ivm.AppointmentNo.Value,
                UnPalletizeBoxCount = appoint.UnPalletizeBoxCount,
                TotalPalletCount = appoint.TotalPalletCount,
                LoadedBoxCount = appoint.LoadedBoxCount,
                TotalBoxCount = appoint.TotalBoxCount,
                LoadedPalletCount = appoint.LoadedPalletCount,
                PalletsInSuspenseCount = appoint.PalletsInSuspenseCount,
                DoorId = appoint.DoorId,
                CarrierId = appoint.CarrierId,
                AppointmentBuildingId = appoint.BuildingId,
                Sound = ivm.Sound
            };
            // We try to make suggestions.
            try
            {
                //Suggests the pallets to load on truck.
                var palletSuggestionList = _service.GetPalletSuggestion(model.AppointmentNumber);
                model.PalletSuggestionList = from pallet in palletSuggestionList
                                             select new PalletModel
                                             {
                                                 PalletId = pallet.PalletId,
                                                 IaId = pallet.IaId,
                                                 LocationId = pallet.LocationId,
                                                 BoxesCount = pallet.BoxesCount
                                             };
                model.SuggestedPallet = palletSuggestionList.Select(p => p.PalletId).FirstOrDefault();
            }
            catch (DbException ex)
            {
                ModelState.AddModelError("", ex.InnerException.Message);
                model.Sound = (char)Sound.Error;
            }
            //TC3: If there is no pallet suggestion for passed appointment.
            if (!model.PalletSuggestionList.Any())
            {
                AddStatusMessage(string.Format("No pallet suggestion found for appointment {0}", model.AppointmentNumber));
            }
            return View(Views.Pallet, model);
        }


        /// <summary>
        /// Load the passed pallet on truck.
        /// </summary>
        /// <param name="model">
        /// Model must pass ScanText(Pallet Id), AppointmentNumber
        /// </param>
        /// <returns></returns>
        [HttpPost]
        public virtual ActionResult Pallet(PalletViewModel model)
        {
            // TC4: Enter pressed, return to previous page to scan new appointment.
            if (string.IsNullOrEmpty(model.ScanText))
            {
                return RedirectToAction(this.Actions.Index());
            }
            //TC5: 'S' pressed, skip suggested pallet by system.
            if (model.ScanText == "S")
            {
                return Appointment(new IndexViewModel { AppointmentNo = model.AppointmentNumber }); ;
            }
            // TC6: If scanned text is not a pallet.
            if (!__regexPallet.IsMatch(model.ScanText))
            {
                ModelState.AddModelError("", string.Format("Scan text {0} is not recognized.", model.ScanText));
                return Appointment(new IndexViewModel { AppointmentNo = model.AppointmentNumber});
            }
            try
            {
                var boxes = _service.GetBoxesOfPallet(model.ScanText);

                // TC7: Scanned pallet belongs to other appointment.
                if (boxes.Any() && model.AppointmentNumber != boxes.First().AppointmentNumber)
                {
                    // Ensuring pallet belongs to same appointment.
                    throw new ScanToTruckServiceException(ScanToTruckServiceErrorCode.AppointmentMisMatch);
                }

                var isPalletLoaded = boxes.Any(p => p.TruckLoadDate != null);

                // TC8: If pallet was not loaded, then load the pallet on truck.
                if (!isPalletLoaded)
                {
                    _service.LoadPallet(model.ScanText);
                    AddStatusMessage(string.Format("Pallet {0} having {1} boxes loaded.", model.ScanText, boxes.Count()));
                }

                // TC9: If user wants to unload pallet.
                else
                {
                    //TC10: Ask confirmation for pallet to unload it.
                    AddStatusMessage(string.Format("Please rescan Pallet {0} to confirm unload it.", model.ScanText));
                    var upvm = new UnloadPalletViewModel
                        {
                            AppointmentNumber = model.AppointmentNumber,
                            //Context = model.Context,
                            ConfirmScanText = model.ScanText,
                            Sound = (char)Sound.Warning
                        };
                    return View(Views.UnloadPallet, upvm);
                }
            }
            catch (DbException ex)
            {
                ModelState.AddModelError("", ex.InnerException);
                model.Sound = (char)Sound.Error;
            }
            catch (ScanToTruckServiceException ex)
            {
                switch (ex.ErrorCode)
                {
                    case ScanToTruckServiceErrorCode.UnVerifiedBoxes:
                        ModelState.AddModelError("", string.Format("{0} boxes of pallet {1} are unverified .", ex.Data["Data"], model.ScanText));
                        break;

                    case ScanToTruckServiceErrorCode.StopProcess:
                        ModelState.AddModelError("", string.Format("{0} boxes of pallet {1} are cancelled.", ex.Data["Data"], model.ScanText));
                        break;

                    case ScanToTruckServiceErrorCode.AppointmentMisMatch:
                        ModelState.AddModelError("", string.Format("Pallet {0} does not belongs to {1} appointment.", model.ScanText, model.AppointmentNumber));
                        break;

                    case ScanToTruckServiceErrorCode.InvalidPallet:
                        ModelState.AddModelError("", string.Format("No boxes exist in Pallet {0}.", model.ScanText));
                        break;

                    case ScanToTruckServiceErrorCode.LoadedOnTruck:
                        ModelState.AddModelError("", string.Format("{0} boxes of {1} pallet already loaded on truck.", ex.Data["Data"], model.ScanText));
                        break;
                }
                model.Sound = (char)Sound.Error;
            }
            return Appointment(new IndexViewModel { AppointmentNo = model.AppointmentNumber});
        }

        /// <summary>
        /// Unload passed pallet.
        /// </summary>
        /// <param name="model">
        /// Model must passed ScanText,ConfirmScanText,AppointmentNumber,Context
        /// </param>
        /// <returns></returns>
        [HttpPost]
        public virtual ActionResult UnloadPallet(UnloadPalletViewModel model)
        {
            // TC11: Confirming scanned pallet to unloading.
            if (!string.IsNullOrWhiteSpace(model.ScanText) && model.ConfirmScanText != model.ScanText)
            {
                ModelState.AddModelError("", string.Format("Confirmation does not match, Pallet {0} is not unloaded.", model.ConfirmScanText));
                model.Sound = (char)Sound.Error;
            }
            // TC12: If Confirm pallet is matched with scanned pallet, Unload it.
            if (model.ConfirmScanText == model.ScanText)
            {
                _service.UnLoadPallet(model.ScanText);
                AddStatusMessage(string.Format("Pallet {0} unloaded.", model.ScanText));
            }
            //TC13: If user wants to quit unloading pallet.
            return Appointment(new IndexViewModel { AppointmentNo = model.AppointmentNumber});
        }
    }
}