using DcmsMobile.Receiving.Helpers;
using DcmsMobile.Receiving.Models;
using DcmsMobile.Receiving.Repository;
using DcmsMobile.Receiving.ViewModels;
using DcmsMobile.Receiving.ViewModels.Home;
using EclipseLibrary.Mvc.Controllers;
using EclipseLibrary.Mvc.Html;
using System;
using System.Collections.Generic;
using System.Configuration.Provider;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace DcmsMobile.Receiving.Areas.Receiving.Controllers
{
    [AuthorizeEx("Receiving requires Role {0}", Roles = "SRC_RECEIVING")]
    [RouteArea("Receving")]
    [RoutePrefix(HomeController.NameConst)]
    public partial class HomeController : EclipseController
    {
        private GroupSelectListItem Map(CartonArea src)
        {
            return new GroupSelectListItem
                {
                    GroupText = string.IsNullOrEmpty(src.BuildingId) ? "Multiple Bldg" : src.BuildingId,
                    Value = src.AreaId,
                    Text = string.Format("{0}: {1}", src.ShortName, src.Description)
                };
        }

        private PalletViewModel Map(Pallet src)
        {
            return new PalletViewModel
            {
                Cartons = src.Cartons,
                PalletId = src.PalletId,
                PalletLimit = src.PalletLimit,
                ProcessId = src.ProcessId
            };
        }

        private ReceivingProcessModel Map(ReceivingProcess src)
        {
            return new ReceivingProcessModel
            {
                ProDate = src.ProDate,
                ProNumber = src.ProNumber,
                CarrierId = src.Carrier.CarrierId,
                CarrierDescription = src.Carrier.Description,
                OperatorName = src.OperatorName,
                ReceivingStartDate = src.StartDate,
                ReceivingEndDate = src.ReceivingEndDate,
                CartonCount = src.CartonCount,
                PalletCount = src.PalletCount,
                ReceivingAreaId = src.ReceivingAreaId,
                ProcessId = src.ProcessId,
                ExpectedCartons = src.ExpectedCartons,
                PalletLimit = src.PalletLimit,
                PriceSeasonCode = src.PriceSeasonCode,
                SpotCheckAreaId = src.SpotCheckAreaId
            };
        }

        /// <summary>
        /// Required by T4MVC
        /// </summary>
        public HomeController()
        {

        }


        private ReceivingService _service;

        /// <summary>
        /// For service injection through unit tests
        /// </summary>
        public ReceivingService Service
        {
            // ReSharper disable UnusedMember.Global
            get { return _service; }
            // ReSharper restore UnusedMember.Global
            set { _service = value; }
        }


        protected override void Initialize(RequestContext requestContext)
        {
            if (_service == null)
            {
                _service = new ReceivingService(requestContext);
            }
            base.Initialize(requestContext);
        }

        protected override void Dispose(bool disposing)
        {
            var dis = _service as IDisposable;
            if (dis != null)
            {
                dis.Dispose();
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
            return base.View(viewName, masterName, model);
        }

        #region Index

        /// <summary>
        /// The main receiving page. Asks for process id
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// Input Expectation: None.
        /// </para>
        /// <para>
        /// Output Expectation: Recent processes are queried and returned in the model.
        /// </para>
        /// </remarks>
        [ActionName("Index")]
        [HttpGet]
        [Route(HomeController.ActionNameConstants.Index, Name = DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_Receving)]
        public virtual ActionResult Index()
        {

            var ivm = new IndexViewModel
             {
                 RecentProcesses = _service.GetRecentProcesses().Select(p => Map(p)).ToArray()
             };
            return View(Views.Index, ivm);
        }


        /// <summary>
        /// Name of the cookie which stores alert info
        /// </summary>
        private const string COOKIE_ALERT = "Receiving_Alert";

        private const string COOKIE_ALERT_AREA = "areaid";

        //private const string COOKIE_ALERT_FLAG = "flag";

        private const string COOKIE_ALERT_RECEIVING_AREA = "recAreaId";

        private const string COOKIE_ALERT_SPOT_CHECK_AREA = "spotCheckAreaId";

        private const string KEY_PRICE_SEASON_CODE = "priceSeasonCode";

        private const string KEY_PALLET_LIMIT = "palletLimit";
        /// <summary>
        /// This is the GET Method for Create new Process 
        /// </summary>
        /// <returns></returns>
        /// <summary>
        /// TODO: Create seperate action method Edit Process

        [HttpGet]
        public virtual ActionResult CreateProcess(int? processId)
        {
            var model = new ReceivingProcessModel();

            if (processId != null)
            {
                // Getting process info for editing case
                var info = _service.GetProcessInfo(processId.Value);
                model = Map(info);
                model.ProcessId = info.ProcessId;
            }
            else
            {
                if (Request.Cookies[COOKIE_ALERT] != null)
                {
                    model.ReceivingAreaId = Request.Cookies[COOKIE_ALERT][COOKIE_ALERT_RECEIVING_AREA];
                    model.SpotCheckAreaId = Request.Cookies[COOKIE_ALERT][COOKIE_ALERT_SPOT_CHECK_AREA];
                    model.PriceSeasonCode = Request.Cookies[COOKIE_ALERT][KEY_PRICE_SEASON_CODE];
                    if (!string.IsNullOrEmpty(Request.Cookies[COOKIE_ALERT][KEY_PALLET_LIMIT]))
                    {
                        model.PalletLimit = Convert.ToInt32(Request.Cookies[COOKIE_ALERT][KEY_PALLET_LIMIT]);
                    }
                }

            }
            PopulateIndexViewModel(model);
            return View(Views.CreateProcess, model);
        }


        /// Creates a new process 
        /// Edits and existing process
        /// <param name="model"></param>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// Input: ReceivingProcessModel.
        /// </para>
        /// <para>
        /// Output: If input valid, redirect to receiving with the created process id.
        /// If input invalid, redisplay the CreateProcess view after repopulating recent processes.
        /// </para>
        /// </remarks>
        /// TODO: Change method name to a more appropiate one. This is actually CreateOrEditProcess
        [HttpPost]
        public virtual ActionResult EditProcess(ReceivingProcessModel model)
        {
            if (!ModelState.IsValid)
            {
                PopulateIndexViewModel(model);
                return View(Views.CreateProcess, model);
            }
            var regexItem = new Regex(":");
            if (regexItem.IsMatch(model.CarrierId))
            {
                model.CarrierId = model.CarrierId.Substring(0, model.CarrierId.IndexOf(":"));
            }

            var carrier = _service.GetCarrier(model.CarrierId);


            if (carrier == null)
            {
                ModelState.AddModelError("", string.Format("{0} is invalid Carrier", model.CarrierId));
                PopulateIndexViewModel(model);
                return View(Views.CreateProcess, model);
            }
            var processModel = new ReceivingProcess
            {
                ProDate = model.ProDate,
                ProNumber = model.ProNumber,
                Carrier = new Carrier
                {
                    CarrierId = model.CarrierId,
                    Description = model.CarrierDescription
                },
                ReceivingAreaId = model.ReceivingAreaId,
                SpotCheckAreaId = model.SpotCheckAreaId,
                PalletLimit = model.PalletLimit,
                CartonCount = model.CartonCount,
                ExpectedCartons = model.ExpectedCartons ?? 0,
                PalletCount = model.PalletCount,
                ProcessId = model.ProcessId ?? 0,
                PriceSeasonCode = model.PriceSeasonCode
            };

            var cookie = new HttpCookie(COOKIE_ALERT);
            if (model.ProcessId == null)
            {
                //Creating New Process
                try
                {
                    _service.InsertProcess(processModel);

                    //Adding the values to cookie

                    if (!string.IsNullOrEmpty(model.ReceivingAreaId)
                         || !string.IsNullOrEmpty(model.SpotCheckAreaId) || !string.IsNullOrEmpty(model.PriceSeasonCode) || model.PalletLimit != null)
                    {
                        cookie.Values.Add(COOKIE_ALERT_RECEIVING_AREA, model.ReceivingAreaId);
                        cookie.Values.Add(COOKIE_ALERT_SPOT_CHECK_AREA, model.SpotCheckAreaId);
                        cookie.Values.Add(KEY_PRICE_SEASON_CODE, model.PriceSeasonCode);
                        cookie.Values.Add(KEY_PALLET_LIMIT, model.PalletLimit.ToString());
                        cookie.Expires = DateTime.Now.AddDays(15);
                        this.HttpContext.Response.Cookies.Add(cookie);
                    }
                }
                catch (ProviderException ex)
                {
                    // Exception happened but we still need to populate all the area lists.
                    PopulateIndexViewModel(model);
                    ModelState.AddModelError("", ex.Message);
                    return View(Views.CreateProcess, model);
                }
            }
            else
            {
                //updating existing Process
                try
                {
                    _service.UpdateProcess(processModel);
                }
                catch (ProviderException ex)
                {
                    // Exception happened but we still need to populate all the area lists.
                    PopulateIndexViewModel(model);
                    ModelState.AddModelError("", ex.Message);
                    return View(Views.CreateProcess, model);
                }
            }


            return RedirectToAction(MVC_Receiving.Receiving.Home.Receiving(processModel.ProcessId));
        }



        /// <summary>
        /// Selects the passed process id and initiates receiving
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// </para>
        /// <para>
        /// Output: If passed process id valid, then intiate receiving. Else redisplay index view after populating recent processes.
        /// </para>
        /// </remarks>
        //[HttpPost]
        //[Obsolete]
        //public virtual ActionResult SelectProcess([Bind(Prefix = "SelectProcess")] SelectProcessModel info)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        return RedirectToAction(MVC_Receiving.Receiving.Home.Receiving(info.ProcessId));
        //    }
        //    var ivm = new IndexViewModel
        //                  {
        //                      SelectProcess = info,
        //                      RecentProcesses = _service.GetRecentProcesses().Select(p => Map(p)).ToArray()
        //                  };

        //    return View(Views.Index, ivm);

        //}

        /// <summary>
        /// Displays the receiving page for the passed process id.
        /// </summary>
        /// <param name="processId"></param>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// Input: Process Id
        /// </para>
        /// <para>
        /// Output: Process id is queried for validity. If valid process id, receiving view is displayed. All pallets and cartons of the process are selected
        /// so that they can be displayed in the view.
        /// Otherwise, redirect to index view.        
        /// 
        /// </para>
        /// </remarks>
        [HttpGet]
        public virtual ActionResult Receiving(int? processId)
        {
            if (processId == null)
            {
                this.AddStatusMessage("Please enter the valid Process ID");
                return RedirectToAction(MVC_Receiving.Receiving.Home.Index());
            }
            // Clearing the Cache every time of GetProcessInfo() because cache info might stale.
            var pm = _service.GetProcessInfo(processId.Value, true);
            if (pm == null)
            {
                AddStatusMessage(string.Format("Invalid Process ID {0}", processId));
                return RedirectToAction(MVC_Receiving.Receiving.Home.Index());
            }
            // Convention based mapping.
            var rvm = new ReceivingViewModel
            {
                CarrierId = pm.Carrier.CarrierId,
                ProDate = pm.ProDate,
                ProNumber = pm.ProNumber,
                OperatorName = pm.OperatorName,
                ReceivingAreaId = pm.ReceivingAreaId,
                SpotCheckAreaId = pm.SpotCheckAreaId,
                ProcessId = pm.ProcessId,
                CartonCount = pm.CartonCount,
                ExpectedCartons = pm.ExpectedCartons,
                PalletLimit = pm.PalletLimit,
                PriceSeasonCode = pm.PriceSeasonCode,
                //PrinterList = _service.GetPrinters().Select(p => new SelectListItem
                //{
                //    Text = string.Format("{0}-{1}", p.Item1, p.Item2),
                //    Value = p.Item1
                //}).ToList()

            };
            //var cookie = this.Request.Cookies[KEY_SELECTED_PRINTER];
            //if (cookie != null && !string.IsNullOrEmpty(cookie.Value))
            //{
            //    rvm.PrinterId = cookie.Value;

            //}
            rvm.ProcessId = processId.Value;
            rvm.UserMismatch = ControllerContext.HttpContext.User.Identity.IsAuthenticated &&
                !string.IsNullOrEmpty(rvm.OperatorName) &&
                string.Compare(this.ControllerContext.HttpContext.User.Identity.Name, rvm.OperatorName, true) != 0;
            var pallets = _service.GetPalletsOfProcess(processId.Value);

            rvm.Pallets = pallets.Select(p => Map(p)).ToArray();
            if (rvm.Pallets.Count > 0)
            {
                // Make first pallet the active pallet
                rvm.ScanModel.PalletId = rvm.Pallets[0].PalletId;
                rvm.ScanModel.PalletDispos = rvm.Pallets[0].Cartons[0].DispositionId;
                foreach (var pallet in rvm.Pallets)
                {
                    pallet.PalletLimit = pallet.PalletLimit;
                }
            }
            rvm.cartonsOnPallet = _service.GetCartonsOfProcess(processId);


            return View(Views.Receiving, rvm);
        }

        #endregion

        /// <summary>
        /// Passing process id so that it can get back to the interrupted receiving session
        /// </summary>
        /// <param name="processId"></param>
        /// <returns></returns>
        [HttpGet]
        public virtual ActionResult Tutorial(int? processId)
        {
            return View(Views.Tutorial, processId);
        }

        #region Scan handling

        [HttpPost]
        public virtual ActionResult HandlePalletScan(ScanViewModel model)
        {
            if (model == null) throw new ArgumentNullException("model");
            if (!ModelState.IsValid)
            {
                return ValidationErrorResult();
            }

            //var scan = model.ScanModel.ScanText.ToUpper();
            //if (scan == ReceivingViewModel.SCAN_NEWPALLET)
            //{
            //    scan = _service.CreateNewPalletId();
            //}

            Debug.Assert(model.ProcessId != null, "model.ProcessId != null");
            var ctx = new ScanContext
            {
                PalletId = model.PalletId,
                DispositionId = model.PalletDispos,
                ProcessId = model.ProcessId.Value
            };
            var pallet = _service.HandlePalletScan(model.ScanText, ctx);
            Debug.Assert(pallet != null, "pallet != null");
            var pvm = new PalletViewModel
            {
                Cartons = pallet.Cartons,
                PalletLimit = pallet.PalletLimit,
                PalletId = ctx.PalletId,
                QueryCount = _service.QueryCount
            };


            //// Pallet was scanned
            //// We check for null before adding header Otherwise the code breaks in IIS 6
            //if (!string.IsNullOrEmpty(pvm.PalletId))
            //{
            //    this.Response.AppendHeader("PalletId", pvm.PalletId);
            //}
            //if (!string.IsNullOrEmpty(pvm.DispositionId))
            //{
            //    // Sharad 17 Oct 2014: bootstrap javscript no longer relies on this header. It can be removed.
            //    this.Response.AppendHeader("Disposition", pvm.DispositionId);
            //}
            //this.Response.StatusCode = 202;

            return PartialView(Views._palletPartial, pvm);


        }
        /// <summary>
        /// <para>
        /// Determines what was scanned and takes appropriate action.
        /// </para>
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// The outcome is indicated by an appropriate status code within the 200 range
        /// 201 (Created): Carton Received. Pallet Html is provided as data. CartonId and Disposition in header. 
        /// 202 (Accepted): Pallet scan. Pallet HTML is provided as data. PalletId and pallet disposition provided as header.
        /// 250 (custom): Carton not received due to disposition mismatch. Disposition is the data. Customized error message will be provided as ErrorMsg in header
        /// 251 (custom): Carton has already been received, Pallet ID on which carton was put is data, and error message will be provided in header against ErrorMsg
        /// </para>
        /// <para>
        /// HandleAjaxError attribute catches exceptions and returns Status code 203 ((Non authoritative)) with error message as data.
        /// </para>
        /// </remarks>
        [HttpPost]
        [ActionName("HandleScan")]
        [HttpAjax(true)]
        [Obsolete("Use HandlePalletScan or HandleCartonScan")]
        public virtual ActionResult HandleScan(ReceivingViewModel model)
        {
            if (model == null) throw new ArgumentNullException("model");
            if (!ModelState.IsValid || string.IsNullOrEmpty(model.ScanModel.ScanText))
            {
                return ValidationErrorResult();
            }

            var scan = model.ScanModel.ScanText.ToUpper();
            if (scan == ReceivingViewModel.SCAN_NEWPALLET)
            {
                scan = _service.CreateNewPalletId();
            }
            try
            {
                Debug.Assert(model.ProcessId != null, "model.ProcessId != null");
                Debug.Assert(model.ScanModel.ProcessId != null, "model.ScanModel.ProcessId != null");
                var ctx = new ScanContext
                {
                    PalletId = model.ScanModel.PalletId,
                    DispositionId = model.ScanModel.PalletDispos,
                    ProcessId = model.ScanModel.ProcessId.Value
                };
                var pallet = _service.HandleScan(scan, ctx);
                Debug.Assert(pallet != null, "pallet != null");
                var pvm = new PalletViewModel
                {
                    Cartons = pallet.Cartons,
                    PalletLimit = pallet.PalletLimit,
                    PalletId = ctx.PalletId,
                    QueryCount = _service.QueryCount
                };

                switch (ctx.Result)
                {
                    case ScanResult.PalletScan:
                        // Pallet was scanned
                        // We check for null before adding header Otherwise the code breaks in IIS 6
                        if (!string.IsNullOrEmpty(pvm.PalletId))
                        {
                            this.Response.AppendHeader("PalletId", pvm.PalletId);
                        }
                        if (!string.IsNullOrEmpty(pvm.DispositionId))
                        {
                            // Sharad 17 Oct 2014: bootstrap javscript no longer relies on this header. It can be removed.
                            this.Response.AppendHeader("Disposition", pvm.DispositionId);
                        }
                        this.Response.StatusCode = 202;
                        break;

                    case ScanResult.CartonReceived:
                        this.Response.StatusCode = 201;
                        pvm.LastCartonId = scan;
                        if (!string.IsNullOrEmpty(scan))
                        {
                            this.Response.AppendHeader("CartonId", scan);
                        }
                        if (!string.IsNullOrEmpty(pvm.DispositionId))
                        {
                            this.Response.AppendHeader("Disposition", pvm.DispositionId);
                        }

                        this.Response.AppendHeader("ReceivedCartonCount", ctx.CartonsOnPallet.ToString());
                        this.Response.AppendHeader("ExpectedCartonCount", ctx.ExpectedCartons.ToString());
                        break;

                    default:
                        throw new NotImplementedException();
                }
                return PartialView(Views._palletPartial, pvm);
            }
            catch (DispositionMismatchException ex)
            {
                this.Response.StatusCode = 250;
                this.Response.AppendHeader("ErrorMsg", string.Format("Carton {0} not put on pallet due to disposition mismatch, Scan a pallet for area:{1}, vwh:{2}", model.ScanModel.ScanText, ex.AreaId, ex.VwhId));

                // We use the format for disposition C15REC i.e first part is VWh_id and second part is Destination Area.
                return Content(string.Format("{0}{1}", ex.VwhId, ex.AreaId));
            }
            catch (AlreadyReceivedCartonException ex)
            {
                //carton has already been received, send the specific status code 251 with Pallet ID as data, and error message in header against ErrorMsg
                this.Response.StatusCode = 251;
                this.Response.AppendHeader("ErrorMsg", string.Format("Carton {0} has already been received on Pallet {1} ", scan, ex.PalletId));
                return Content(ex.PalletId);
            }
            catch (Exception ex)
            {
                // Simulate the behavior of the obsolete HandleAjaxError attribute
                this.Response.StatusCode = 203;
                return Content(ex.Message);
            }
        }

        /// <summary>
        /// Handles Mobile scan
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("HandleScan")]
        [HttpAjax(false)]
        public virtual ActionResult HandleMobileScan(ReceivingViewModel model)
        {
            Pallet pallet = null;
            var lastCartonId = string.Empty;
            if (!ModelState.IsValid)
            {
                return RedirectToAction(MVC_Receiving.Receiving.Home.Receiving(model.ScanModel.ProcessId));
            }

            var scan = model.ScanModel.ScanText.ToUpper();
            try
            {
                Debug.Assert(model.ProcessId != null, "model.ProcessId != null");
                Debug.Assert(model.ScanModel.ProcessId != null, "model.ScanModel.ProcessId != null");
                var ctx = new ScanContext
                {
                    PalletId = model.ScanModel.PalletId,
                    DispositionId = model.ScanModel.PalletDispos,
                    ProcessId = model.ScanModel.ProcessId.Value
                };
                pallet = _service.HandleScan(scan, ctx);

                switch (ctx.Result)
                {
                    case ScanResult.PalletScan:
                        model.Pallets = new[] { Map(pallet) };
                        // Pallet was scanned
                        break;

                    case ScanResult.CartonReceived:
                        lastCartonId = scan;
                        break;

                    default:
                        throw new NotImplementedException();
                }
            }
            catch (DispositionMismatchException ex)
            {
                ModelState.AddModelError("", "Scan a pallet for area:" + ex.AreaId + "and vwh:" + ex.VwhId);
            }
            catch (DbException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            catch (ProviderException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            catch (AlreadyReceivedCartonException ex)
            {
                ModelState.AddModelError("", string.Format("Carton {0} has already been received on Pallet {1} ", scan, ex.PalletId));

            }

            if (pallet == null)
            {
                // This happens when an exception is caught or model state is invalid
                if (string.IsNullOrEmpty(model.ScanModel.PalletId))
                {
                    // No active pallet
                    pallet = new Pallet();
                }
                else
                {
                    if (model.ScanModel.ProcessId != null)
                    {
                        pallet = _service.GetPallet(model.ScanModel.PalletId, model.ScanModel.ProcessId.Value);

                    }

                }
            }
            Debug.Assert(pallet != null, "pallet != null");
            // Handling Code
            var pm = _service.GetProcessInfo(model.ScanModel.ProcessId.Value, true);
            model.CarrierId = pm.Carrier.CarrierId;
            model.ProDate = pm.ProDate;
            model.ProNumber = pm.ProNumber;
            model.OperatorName = pm.OperatorName;
            model.ReceivingAreaId = pm.ReceivingAreaId;
            model.SpotCheckAreaId = pm.SpotCheckAreaId;
            model.ProcessId = pm.ProcessId;
            model.CartonCount = pm.CartonCount;
            model.ExpectedCartons = pm.ExpectedCartons;
            model.PalletLimit = pm.PalletLimit;
            model.PriceSeasonCode = pm.PriceSeasonCode;

            var allPallets = _service.GetPalletsOfProcess(model.ScanModel.ProcessId.Value);
            model.Pallets = model.Pallets.Concat(allPallets.Select(p => Map(p))).ToArray();
            //model.Pallets = allPallets.Select(p => Map(p)).ToArray();
            model.cartonsOnPallet = _service.GetCartonsOfProcess(model.ScanModel.ProcessId.Value);


            // If the model state is not valid, we would like the hidden fields in the view to retain their old values.
            // We do not change the state of the view in case of any validation problem.
            if (ModelState.IsValid)
            {
                ModelState.Clear();  // Necessary to ensure that hidden fields will show the values we are setting below
                model.Pallets[0].LastCartonId = lastCartonId;
                model.ScanModel.PalletId = model.Pallets[0].PalletId;
                model.ScanModel.PalletDispos = model.Pallets[0].DispositionId;
            }
            return View(Views.Receiving, model);

        }

        #endregion

        /// <summary>
        /// This is a private method which populates various list of areas acoording to passed flags.
        /// </summary>
        /// <param name="model"></param>
        private void PopulateIndexViewModel(ReceivingProcessModel model)
        {
            var areas = _service.GetCartonAreas().ToList();
            if (areas.Any(p => p.IsReceivingArea))
            {
                model.ReceivingAreasList = areas.Where(p => !p.IsNumberedArea && p.IsReceivingArea).Select(p => Map(p)).ToArray();
            }
            else
            {
                model.ReceivingAreasList = areas.Where(p => !p.IsNumberedArea).Select(p => Map(p)).ToArray();
            }
            if (areas.Any(p => p.IsSpotCheckArea))
            {
                model.SpotCheckAreasList = areas.Where(p => !p.IsNumberedArea && p.IsSpotCheckArea).Select(p => Map(p)).ToArray();
            }
            else
            {
                model.SpotCheckAreasList = areas.Where(p => !p.IsNumberedArea).Select(p => Map(p)).ToArray();
            }
            model.PriceSeasonCodeList = _service.GetPriceSeasonCode().Select(p => new SelectListItem
                {
                    Text = p.Code + ":" + p.Description,
                    Value = p.Code
                });

        }

        /// <summary>
        /// Unreceives the passed carton only if it belongs to the passed process id.
        /// </summary>
        /// <param name="cartonId"></param>
        /// <param name="processId"></param>
        /// <returns></returns>
        /// <remarks>
        /// 200 (Success): Pallet HTML is provided as data. PalletDisposition provided in header
        /// </remarks>
        [HttpPost]
        //[HandleAjaxError]
        public virtual ActionResult UnPalletizeCarton(string cartonId, int processId)
        {
            //throw new Exception("Sharad");
            var pvm = new PalletViewModel();
            string palletId;
            var pallet = _service.RemoveFromPallet(cartonId, processId, out palletId);
            pvm.Cartons = pallet.Cartons;
            pvm.PalletId = pallet.PalletId;
            pvm.PalletLimit = pallet.PalletLimit;

            this.Response.AppendHeader("Disposition", pvm.DispositionId);

            pvm.StatusMessage = string.Format("Carton {0} removed from Pallet {1}", cartonId, palletId);
            return PartialView(Views._palletPartial, pvm);

        }


        /// <summary>
        /// Prints carton ticket. 
        /// If the call is sucessfull we return sucess message.
        /// </summary>
        /// <param name="cartonId"></param>
        /// <param name="printer"></param>
        /// <returns></returns>
        /// <remarks>
        /// 200 for succes. data will be message. Both carton id and printer must be non null.
        /// 203 for error. data will be error
        /// </remarks>
        //[HandleAjaxError]
        public virtual ActionResult PrintCarton(string cartonId, string printer)
        {
            //throw new Exception("Sharad");
            var errors = new List<string>();
            if (string.IsNullOrEmpty(printer))
            {
                errors.Add("Printer is required");
            }
            if (string.IsNullOrEmpty(cartonId))
            {
                errors.Add("Carton is required");
            }
            if (errors.Count > 0)
            {
                this.Response.StatusCode = 203;
                return Content(string.Join("; ", errors));
            }
            //var cookie = new HttpCookie(KEY_SELECTED_PRINTER, printer) { Expires = DateTime.Now.AddDays(1) };
            //// Remember for 1 day sliding
            //this.Response.Cookies.Add(cookie);

            _service.PrintCarton(cartonId, printer);

            return Content(string.Format("Carton ticket Printed on {0} at {1}", printer, DateTime.Now));

        }






        private const string KEY_SELECTED_PRINTER = "SelectedCartonTicketPrinter";
        /// <summary>
        /// Returns an array of Printer objects. The id of the selected printer is passed in the "Selected" header.
        /// The selected printer is read from a cookie. The cookie is set when a carton is printed.
        /// </summary>
        /// <returns></returns>  
        public virtual JsonResult GetPrinters()
        {
            var cookie = this.Request.Cookies[KEY_SELECTED_PRINTER];
            if (cookie != null && !string.IsNullOrEmpty(cookie.Value))
            {
                this.Response.AppendHeader("Selected", cookie.Value);
            }
            var results = _service.GetPrinters();
            return Json(from result in results
                        select new
                        {
                            Name = result.Item1,
                            Description = result.Item2
                        }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// This function used for get the list of unpalletize cartons against the passed ProcessId.
        /// </summary>
        /// <returns>
        /// It will return the list of unpalletize cartons under a particular process.
        /// </returns>        
        [HttpGet]
        public virtual ActionResult NonpalletizedCartons(int? processId)
        {
            try
            {
                var model = new ReceivingViewModel();
                model.NonPalletizeCartonList =
                    _service.GetUnpalletizedCartons(processId).Select(p => new CartonNotOnPalletModel
                        {
                            AreaId = p.DestinationArea,
                            CartonId = p.CartonId,
                            VWHId = p.VwhId
                        });
                Response.StatusCode = 200;
                return PartialView(MVC_Receiving.Receiving.Home.Views._cartonNotOnPalletPartial, model);
            }
            catch (Exception ex)
            {
                // Simulate the behavior of the obsolete HandleAjaxError attribute
                this.Response.StatusCode = 203;
                return Content(ex.Message);
            }
        }

        ///// <summary>
        ///// Populate SpotCheck area drop down based on selection in Receiving area drop down.
        ///// </summary>
        ///// <param name="areaId">Selected receiving area</param>
        ///// <returns></returns>
        //public virtual JsonResult SpotCheckAreaList(string areaId)
        //{
        //    IEnumerable<CartonArea> selectedAreas;
        //    var cartonAreas = _service.GetCartonAreas().Where(p => !p.IsNumberedArea).ToArray();
        //    // There's no spot check area defined ,show all areas
        //    if (cartonAreas.All(p => !p.IsSpotCheckArea))
        //    {
        //        var data = cartonAreas.Select(Map);
        //        return Json(data, JsonRequestBehavior.AllowGet);
        //    }
        //    var building = cartonAreas.Where(p => p.AreaId == areaId).Select(p => p.BuildingId).FirstOrDefault();

        //    // Selected receiving area has building
        //    if (!string.IsNullOrWhiteSpace(building))
        //    {
        //        // Get spotcheck area of building
        //        selectedAreas = cartonAreas.Where(p => p.BuildingId == building && p.IsSpotCheckArea).ToList();
        //        // if building has no spotCheck area defined, show all areas
        //        if (!selectedAreas.Any())
        //        {

        //            selectedAreas = cartonAreas;
        //        }
        //    }
        //    //if selected receiving area has no building ,show all areas
        //    else
        //    {
        //        selectedAreas = cartonAreas;
        //    }
        //    var data1 = selectedAreas.Select(Map);
        //    return Json(data1, JsonRequestBehavior.AllowGet);
        //}

        /// <summary>
        /// Get the shipment list
        /// </summary>
        /// <returns></returns>
        public virtual ActionResult ShipmentList()
        {
            var model = new ShipmentListViewModel
            {
                ShipmentList = (from item in _service.GetShipmentList()
                                select new ShipmentListModel
                                {
                                    PoNumber = item.PoNumber,
                                    IntransitType = item.IntransitType,
                                    MaxReceiveDate = item.MaxReceiveDate,
                                    ShipmentId = item.ShipmentId,
                                    ReceivedQuantity = item.ReceivedQuantity,
                                    ExpectedQuantity = item.ExpectedQuantity,
                                    ErpType = item.ErpType,
                                    CartonCount = item.CartonCount,
                                    CartonReceived = item.CartonReceived,
                                    ProcessNumber = item.ProcessNumber,
                                    ShipmentDate = item.ShipmentDate
                                }).ToArray()
            };
            return View(Views.ShipmentList, model);
        }

        /// <summary>
        /// Close passed shipment
        /// </summary>
        /// <param name="shipmentId"></param>
        /// <param name="poId"></param>
        /// <returns></returns>
        public virtual ActionResult CloseShipment(string shipmentId, long? poId)
        {
            try
            {
                _service.CloseShipment(shipmentId, poId);
            }
            catch (DbException exception)
            {
                this.Response.StatusCode = 203;
                return Content(exception.Message);
            }
            return Content(string.Format("Shipment {0} closed successfully.", shipmentId));
        }


        /// <summary>
        /// Reopen passed shipment
        /// </summary>
        /// <param name="shipmentId"></param>
        /// <param name="poId"></param>
        /// <returns></returns>
        public virtual ActionResult ReOpenShipment(string shipmentId, long? poId)
        {
            try
            {
                if (_service.ReOpenShipment(shipmentId, poId))
                {
                    return Content(string.Format("Shipment {0} Re-opened .", shipmentId));
                }
                else
                {
                    return Content(string.Format("Shipment {0} can not be Re-opened .", shipmentId));
                }
            }
            catch (DbException exception)
            {
                this.Response.StatusCode = 203;
                return Content(exception.Message);
            }

        }

    }
}




//$Id$