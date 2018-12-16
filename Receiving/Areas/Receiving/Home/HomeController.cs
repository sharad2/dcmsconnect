using DcmsMobile.Receiving.Areas.Receiving.Home.Repository;
using EclipseLibrary.Mvc.Controllers;
using EclipseLibrary.Mvc.Html;
using System;
using System.Collections.Generic;
using System.Configuration.Provider;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Script.Serialization;

namespace DcmsMobile.Receiving.Areas.Receiving.Home
{
    [AuthorizeEx("Receiving requires Role {0}", Roles = "SRC_RECEIVING")]
    [RouteArea("Receiving")]
    //[RoutePrefix("Home")]
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


        /// <summary>
        /// Required by T4MVC
        /// </summary>
        public HomeController()
        {

        }


        private Lazy<ReceivingService> _service;


        protected override void Initialize(RequestContext requestContext)
        {
            if (_service == null)
            {
                _service = new Lazy<ReceivingService>(() => new ReceivingService(requestContext));
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

        private const string KEY_TEMPDATA_CARTONS = "HighlightedCartons";

        private IList<string> HighlightedCartons
        {
            get
            {
                var x = TempData[KEY_TEMPDATA_CARTONS] as IList<string>;
                if (x == null)
                {
                    x = new List<string>();
                    TempData[KEY_TEMPDATA_CARTONS] = x;
                }
                return x;
            }
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
        [HttpGet]
        [Route(Name = DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_Receving)]
        public virtual ActionResult Index()
        {

            var ivm = new IndexViewModel
             {
                 RecentProcesses = _service.Value.GetRecentProcesses().Select(p => new ReceivingProcessModel(p)).ToArray()
             };
            return View(Views.Index, ivm);
        }

        [Route("search")]
        public virtual ActionResult Search(int? id)
        {

            if (id == null)
            {
                return RedirectToAction(MVC_Receiving.Receiving.Home.Index());

            }

            var rp = _service.Value.GetProcessInfo(id.Value);
            if (rp != null)
            {
                return RedirectToAction(MVC_Receiving.Receiving.Home.Receiving(id));
            }

            //  Redirect to Inquiry
            var route = Url.RouteCollection[DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_Search1];
            if (route != null)
            {
                var url = Url.RouteUrl(DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_Search1, new
                {
                    id = id
                });
                return Redirect(url);
            }

            // Inquiry not installed
            AddStatusMessage(string.Format("Search text {0} was not understood and is being ignored", id));
            if (Request.UrlReferrer != null)
            {
                return Redirect(Request.UrlReferrer.PathAndQuery);
            }
            else
            {
                return RedirectToAction(MVC_Receiving.Receiving.Home.Index());
            }


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
        [Route("create")]
        public virtual ActionResult CreateProcess(int? processId)
        {
            var model = new ProcessEditorViewModel();

            if (processId != null)
            {
                // Getting process info for editing case
                var src = _service.Value.GetProcessInfo(processId.Value);
                model = new ProcessEditorViewModel
                {
                    ProDate = src.ProDate,
                    ProNumber = src.ProNumber,
                    CarrierId = src.CarrierId,
                    CarrierDisplayName = string.Format("{0}: {1}", src.CarrierId, src.CarrierName),
                    PalletCount = src.PalletCount,
                    ReceivingAreaId = src.ReceivingAreaId,
                    ProcessId = src.ProcessId,
                    ExpectedCartons = src.ExpectedCartons,
                    PalletLimit = src.PalletLimit,
                    PriceSeasonCode = src.PriceSeasonCode,
                    SpotCheckAreaId = src.SpotCheckAreaId
                };
                model.ProcessId = src.ProcessId;
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
            return View(Views.ProcessEditor, model);
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
        [Route("update")]
        public virtual ActionResult CreateUpdateProcess(ProcessEditorViewModel model)
        {
            if (!ModelState.IsValid)
            {
                PopulateIndexViewModel(model);
                return View(Views.ProcessEditor, model);
            }


            var carrier = _service.Value.GetCarrier(model.CarrierId);


            if (carrier == null)
            {
                ModelState.AddModelError("", string.Format("{0} is invalid Carrier", model.CarrierId));
                PopulateIndexViewModel(model);
                return View(Views.ProcessEditor, model);
            }


            var processModel = new ReceivingProcess
            {
                ProDate = model.ProDate,
                ProNumber = model.ProNumber,
                CarrierId = model.CarrierId,
                ReceivingAreaId = model.ReceivingAreaId,
                SpotCheckAreaId = model.SpotCheckAreaId,
                PalletLimit = model.PalletLimit,
                //CartonCount = model.CartonCount,
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
                    _service.Value.InsertProcess(processModel);

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
                    return View(Views.ProcessEditor, model);
                }
            }
            else
            {
                //updating existing Process
                try
                {
                    _service.Value.UpdateProcess(processModel);
                }
                catch (ProviderException ex)
                {
                    // Exception happened but we still need to populate all the area lists.
                    PopulateIndexViewModel(model);
                    ModelState.AddModelError("", ex.Message);
                    return View(Views.ProcessEditor, model);
                }
            }


            return RedirectToAction(MVC_Receiving.Receiving.Home.Receiving(processModel.ProcessId));
        }





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
        [Route("receiving")]
        public virtual ActionResult Receiving(int? processId)
        {
            if (processId == null)
            {
                this.AddStatusMessage("Please enter the valid Process ID");
                return RedirectToAction(MVC_Receiving.Receiving.Home.Index());
            }
            // Clearing the Cache every time of GetProcessInfo() because cache info might stale.
            var pm = _service.Value.GetProcessInfo(processId.Value, true);
            if (pm == null)
            {
                AddStatusMessage(string.Format("Invalid Process ID {0}", processId));
                return RedirectToAction(MVC_Receiving.Receiving.Home.Index());
            }
            // Convention based mapping.
            var rvm = new ReceivingViewModel
            {
                CarrierId = pm.CarrierId,
                CarrierDisplayName = string.Format("{0}: {1}", pm.CarrierId, pm.CarrierName),
                ProDate = pm.ProDate,
                ProNumber = pm.ProNumber,
                //OperatorName = pm.OperatorName,
                ReceivingAreaId = pm.ReceivingAreaId,
                SpotCheckAreaId = pm.SpotCheckAreaId,
                ProcessId = pm.ProcessId,
                CartonCount = pm.CartonCount,
                ExpectedCartons = pm.ExpectedCartons,
                PalletLimit = pm.PalletLimit,
                PriceSeasonCode = pm.PriceSeasonCode,

            };

            rvm.ProcessId = processId.Value;
            var ser = new JavaScriptSerializer();
            rvm.PalletIdListJson = ser.Serialize(_service.Value.GetPalletsOfProcess(processId.Value));


            return View(Views.Receiving, rvm);
        }

        #endregion


        /// <summary>
        /// Passing process id so that it can get back to the interrupted receiving session
        /// </summary>
        /// <param name="processId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("tutorial")]
        public virtual ActionResult Tutorial(int? processId)
        {
            return View(Views.Tutorial, processId);
        }

        #region Scan handling


        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <remarks>
        /// It receives each carton in the passed array. If receiving of any carton fails, then the cartons before that have already been received.
        /// If any carton is received, then this function will never throw an exception.
        /// </remarks>
        [HttpPost]
        [Route("handlescan")]
        public virtual ActionResult HandleCartonScan(string scanText, string palletId, int processId)
        {
            //Thread.Sleep(5000);
            if (string.IsNullOrWhiteSpace(scanText))
            {
                throw new Exception("Nothing was scanned");
            }

            var list = new List<Tuple<string, string>>();
            foreach (var cartonId in scanText.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim().ToUpper()))
            {
                try
                {
                    _service.Value.ReceiveCarton(cartonId, palletId, processId);
                    HighlightedCartons.Add(cartonId);
                }
                catch (Exception ex)
                {
                    list.Add(Tuple.Create(cartonId, ex.Message));
                }
            }

            return Json(from item in list
                        select new
                        {
                            cartonId = item.Item1,
                            message = item.Item2
                        });

        }

        /// <summary>
        /// Returns the List of cartons in the pallet in HTL format
        /// </summary>
        /// <param name="palletId"></param>
        /// <param name="processId"></param>
        /// <returns></returns>
        [Route("pallethtml")]
        public virtual ActionResult GetPalletHtml(string palletId, int processId)
        {
            //if (string.IsNullOrWhiteSpace(palletId))
            //{
            //    return Content("Internal Error: Pallet Id was not passed");
            //}
            //Thread.Sleep(3000);  // For debugging

            IList<ReceivedCarton> cartons;
            if (string.IsNullOrWhiteSpace(palletId))
            {
                cartons = _service.Value.GetUnpalletizedCartons(processId);
            }
            else
            {
                cartons = _service.Value.GetCartonsOfPallet(palletId);
            }

            // This array is populated when carton is received
            var list = HighlightedCartons;
            var pvm = new PalletViewModel
            {
                Cartons = cartons.Select(p => new ReceivedCartonModel(p)
                {
                    Highlight = list.Contains(p.CartonId)
                }).ToList(),
                PalletLimit = _service.Value.GetPalletLimit(processId),
                PalletId = palletId
            };
            return PartialView(Views._palletPartial, pvm);
        }


        #endregion

        /// <summary>
        /// This is a private method which populates various list of areas acoording to passed flags.
        /// </summary>
        /// <param name="model"></param>
        private void PopulateIndexViewModel(ProcessEditorViewModel model)
        {
            var areas = _service.Value.GetCartonAreas().ToList();
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
            model.PriceSeasonCodeList = _service.Value.GetPriceSeasonCodes().Select(p => new SelectListItem
                {
                    Text = p.Item1 + ":" + p.Item2,
                    Value = p.Item1
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
        [Route("unpalletize/carton")]
        public virtual ActionResult UnPalletizeCarton(string cartonId, int processId)
        {
            //throw new Exception("Sharad");
            var pvm = new PalletViewModel();
            string palletId;
            _service.Value.RemoveFromPallet(cartonId, processId, out palletId);
            pvm.Cartons = _service.Value.GetCartonsOfPallet(palletId).Select(p => new ReceivedCartonModel(p)).ToList();
            pvm.PalletId = palletId;
            pvm.PalletLimit = _service.Value.GetPalletLimit(processId);

            //this.Response.AppendHeader("Disposition", pvm.DispositionId);

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
        [Route("print/crtn")]
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

            _service.Value.PrintCarton(cartonId, printer);

            return Content(string.Format("Ticket for Carton {0} printed on {1} at {2}", cartonId, printer, DateTime.Now));

        }






        private const string KEY_SELECTED_PRINTER = "SelectedCartonTicketPrinter";
        /// <summary>
        /// Returns an array of Printer objects. The id of the selected printer is passed in the "Selected" header.
        /// The selected printer is read from a cookie. The cookie is set when a carton is printed.
        /// </summary>
        /// <returns></returns>  
        [Route("printers")]
        public virtual JsonResult GetPrinters()
        {
            var cookie = this.Request.Cookies[KEY_SELECTED_PRINTER];
            if (cookie != null && !string.IsNullOrEmpty(cookie.Value))
            {
                this.Response.AppendHeader("Selected", cookie.Value);
            }
            var results = _service.Value.GetPrinters();
            return Json(from result in results
                        select new
                        {
                            Name = result.Item1,
                            Description = result.Item2
                        }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Get the shipment list
        /// </summary>
        /// <returns></returns>
        [Route("shipment/list")]
        public virtual ActionResult ShipmentList()
        {
            var model = new ShipmentListViewModel
            {
                ShipmentList = (from item in _service.Value.GetShipmentList()
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
                                    ReceivingProcessId = item.ReceivingProcessId,
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
        [HttpPost]
        [Route("close/shipment")]
        public virtual ActionResult CloseShipment(string shipmentId, long? poId)
        {
            //Thread.Sleep(5000);
            //throw new Exception("Sharad");
            _service.Value.CloseShipment(shipmentId, poId);

            return Content(string.Format("Shipment {0} has been closed", shipmentId));
        }


        /// <summary>
        /// Reopen passed shipment
        /// </summary>
        /// <param name="shipmentId"></param>
        /// <param name="poId"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("reopen/shipment")]
        public virtual ActionResult ReOpenShipment(string shipmentId, long? poId)
        {
            //Thread.Sleep(5000);
            if (_service.Value.ReOpenShipment(shipmentId, poId))
            {
                return Content(string.Format("Shipment {0} Re-opened .", shipmentId));
            }
            else
            {
                return Content(string.Format("Shipment {0} can not be Re-opened .", shipmentId));
            }
        }

        /// <summary>
        /// Get matching carriers
        /// </summary>
        /// <param name="term"></param>
        /// <returns></returns>
        [Route("carriers")]
        public virtual JsonResult GetCarriers(string term)
        {
            // Change null to empty string
            term = term ?? string.Empty;

            var tokens = term.Split(new[] { ":" }, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim())
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .ToList();

            string searchId;
            string searchDescription;

            switch (tokens.Count)
            {
                case 0:
                    // All carriers
                    searchId = searchDescription = string.Empty;
                    break;

                case 1:
                    // Try to match term with either id or description
                    searchId = searchDescription = tokens[0];
                    break;

                case 2:
                    // Try to match first token with id and second with description
                    searchId = tokens[0];
                    searchDescription = tokens[1];
                    break;

                default:
                    // For now, ignore everything after the second :
                    searchId = tokens[0];
                    searchDescription = tokens[1];
                    break;


            }

            var data = _service.Value.GetCarriers(searchId, searchDescription).Select(p => new
            {
                label = string.Format("{0}: {1}", p.Item1, p.Item2),
                value = p.Item1
            }); ;
            return Json(data, JsonRequestBehavior.AllowGet);
        }

    }


}




//$Id$