using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using DcmsMobile.Shipping.Helpers;
using DcmsMobile.Shipping.Repository;
using DcmsMobile.Shipping.ViewModels;
using EclipseLibrary.Mvc.Controllers;
using EclipseLibrary.Mvc.Helpers;

namespace DcmsMobile.Shipping.Areas.Shipping.Controllers
{
    // Reviewed By: Deepak Bhatt, Ritesh Verma and Rajesh Kandari on 12-4-2012
    public partial class HomeController : EclipseController
    {

        #region Intialization
        protected const string ROLE_SHIPPING = "DCMS8_TMSMGR";
        /// <summary>
        /// Required by T4MVC
        /// </summary>
        public HomeController()
        {

        }

        private ShippingService _service;

        public ShippingService Service
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
                    // _layOutPull.cshtml also expects that fake user name will be stored in this.Session["user"]
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
                _service = new ShippingService(requestContext.HttpContext.Trace, connectString, userName, clientInfo);
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
        /// <summary>
        /// Populates properties for the LayoutViewModel
        /// </summary>
        /// <param name="filterContext"></param>
        protected override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            base.OnActionExecuted(filterContext);
            var vr = filterContext.Result as ViewResult;
            if (vr != null)
            {
                var model = vr.Model as LayoutTabsViewModel;
                if (model != null)
                {
                    if (!string.IsNullOrWhiteSpace(model.PostedCustomerId))
                    {
                        var customer = _service.GetCustomerOrderSummary(model.PostedCustomerId);
                        if (customer == null)
                        {
                            // TODO: Somehow get the customer name as well
                            model.Summary.CustomerId = model.PostedCustomerId;
                        }
                        else
                        {
                            model.Summary = new CustomerSummaryModel(customer);
                        }
                    }
                    switch (model.SelectedIndex)
                    {
                        case LayoutTabPage.Summary:
                            model.CustomerFormUrl = Url.Action(this.Actions.RoutingSummary());
                            break;

                        case LayoutTabPage.Unrouted:
                            model.CustomerFormUrl = Url.Action(this.Actions.Unrouted());
                            break;

                        case LayoutTabPage.Routing:
                            model.CustomerFormUrl = Url.Action(this.Actions.Routing());
                            break;

                        case LayoutTabPage.Routed:
                            model.CustomerFormUrl = Url.Action(this.Actions.Routed());
                            break;

                        case LayoutTabPage.Bol:
                            model.CustomerFormUrl = Url.Action(this.Actions.Bol());
                            break;

                        case LayoutTabPage.Appointment:
                            model.CustomerFormUrl = Url.Action(this.Actions.AllAppointments());
                            break;
                        case LayoutTabPage.PoSearchResults:
                            model.CustomerFormUrl = Url.Action(this.Actions.PoSearch());
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                }
            }
        }

        #endregion

        #region Mapping

        private SelectListItem Map(CodeDescriptionModel src)
        {
            return new SelectListItem
            {
                Text = string.Format("{0} : {1}", src.Code, src.Description),
                Value = src.Code
            };
        }

        //Mapping used by GetorderSummary() to filter the customers retrieved.
        private RoutingStatus Map(RoutingSummaryFilter src)
        {
            if (src == RoutingSummaryFilter.UnroutedOnly)
            {
                return RoutingStatus.Unrouted;
            }
            if (src == RoutingSummaryFilter.RoutingOnly)
            {
                return RoutingStatus.Routing;
            }
            if (src == RoutingSummaryFilter.RoutedOnly)
            {
                return RoutingStatus.Routed;
            }
            if (src == RoutingSummaryFilter.BolOnly)
            {
                return RoutingStatus.InBol;
            }
            return RoutingStatus.Notset;
        }
        #endregion

        #region Index

        public virtual ActionResult Index()
        {
            return View(this.Views.Index);
        }

        #endregion

        #region CustomerSummary
        /// <summary>
        /// Returns routing details against passed filters
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public virtual ActionResult RoutingSummaryAll(RoutingSummaryViewModel model)
        {
           
           // Ignore Model Errors if any
            ModelState.Clear();
            IEnumerable<CustomerOrderSummary> customers;
            model.PostedCustomerId = string.Empty;
            //TC 4 : When we click on Routing summary link from any page
            customers = _service.GetOrderSummary(Map(model.RoutingFilter));
            model.CustomerRoutingDetails = from customer in customers
                                           orderby customer.MaxDcCancelDate 
                                           select new CustomerSummaryModel(customer);
            return View(Views.RoutingSummary, model);
        }
        /// <summary>
        ///  Returns routing details per customer. 
        /// </summary>
        /// <param name="model"></param>
        /// <returns>View RoutingSummary.cshtml</returns>
        /// <remarks>
        /// Can post PostedCustomerId,Routing Filters.
        /// </remarks>
        public virtual ActionResult RoutingSummary(RoutingSummaryViewModel model)
        {
            //This is a safe code to handle situations that will not come under normal user practice
            if (!ModelState.IsValid)
            {
                return View(Views.RoutingSummary, model);
            }
            IEnumerable<CustomerOrderSummary> customers;
            //TC 1: When we not pass customerId from Routing summary page.
            if (string.IsNullOrEmpty(model.PostedCustomerId))
            {
                model.PostedCustomerId = string.Empty;
                customers = _service.GetOrderSummary(Map(model.RoutingFilter));
            }
            //TC 2 : Pass customer Id on Routing summary page.
            else
            {
                var summary = _service.GetCustomerOrderSummary(model.PostedCustomerId);
                //TC 3 : Pass a customer on Routing summary page that does not have any open order.
                if (summary == null)
                {
                    ModelState.AddModelError("", string.Format("No open orders found for customer {0}", model.PostedCustomerId));
                    customers = Enumerable.Empty<CustomerOrderSummary>();
                }
                else
                {
                    customers = Enumerable.Repeat(summary, 1);
                }
            }
            model.CustomerRoutingDetails = from customer in customers
                                           orderby customer.MaxDcCancelDate 
                                           select new CustomerSummaryModel(customer);
            return View(Views.RoutingSummary, model);
        }
        #endregion

        #region Unrouted

        /// <summary>
        /// returns list of unrouted orders of the selected customer
        /// </summary>
        /// <param name="model"></param>
        /// <returns>Unrouted.cshtml</returns>
        /// <remarks>
        /// DC Cancel Date, Start Date and Building can be optionally posted, in which case they will be used to filter the PO list.
        /// </remarks>
        [AuthorizeEx("Shipping requires Role {0}", Roles = "DCMS8_TMSMGR", Purpose = "Enables you to choose orders to route and assign ATS date")]
        public virtual ActionResult Unrouted(UnroutedViewModel model)
        {

            //TC 5 : Click on Unrouted page link without selecting any customer.
            if (!ModelState.IsValid)
            {
                // Customer is required
                return View(Views.Unrouted, model);
            }
            // Add Posted customer to cookie.
            if (!string.IsNullOrEmpty(model.PostedCustomerId))
            {
                var cookieCustomer = new HttpCookie(ShippingAreaRegistration.COOKIE_PREFIX + ReflectionHelpers.NameFor((LayoutTabsViewModel m) => m.PostedCustomerId));
                cookieCustomer.Value = model.PostedCustomerId;
                cookieCustomer.Expires = DateTime.Now.AddDays(15);
                this.HttpContext.Response.Cookies.Add(cookieCustomer);
            }
            // TC 6 : When we click Unrouted page link with customerId.
            var orders = _service.GetUnRoutedOrders(model.PostedCustomerId, model.BuildingId, model.ShowUnavailableBucket);

            var groupedPoList = from po in orders
                                //orderby po.BuildingId, po.MinDcCancelDate, po.StartDate,
                                //po.CustomerDcId,po.PoId
                                group po by new UnroutedPoGroup(po.BuildingId, po.MinDcCancelDate) into g
                                select g;
            foreach (var item in groupedPoList)
            {
                var list = (from order in item
                            orderby order.CustomerDcId, order.PoId
                            select new UnroutedPoModel
                            {
                                PickedPieces = order.PickedPieces ?? 0,
                                MinDcCancelDate = order.MinDcCancelDate,
                                NumberOfBoxes = order.NumberOfBoxes,
                                PiecesOrdered = order.PiecesOrdered ?? 0,
                                PoId = order.PoId,
                                BuildingId = order.BuildingId,
                                StartDate = order.StartDate,
                                Volume = order.Volume,
                                Weight = order.Weight,
                                Iteration = order.Iteration,
                                PoIterationCount = order.PoIterationCount,
                                CustomerDcId = order.CustomerDcId,
                                BucketId = order.BucketId,
                                BuildingCount=order.BuidlingCount,
                                IsEdiCustomer=order.IsEdiCustomer
                            }).ToList();
                item.Key.UpdateStats(list);
                model.GroupedPoList.Add(item.Key, list);
            }
            var query = from pogroup in model.GroupedPoList.Keys
                        //orderby pogroup.DcCancelDate ?? DateTime.MaxValue
                        group pogroup by pogroup.BuildingId into g
                        orderby g.Key
                        select new
                        {
                            BuildingId = g.Key,
                            UnRoutedPoGroup = g.OrderBy(p => p).ToList()
                        };

            foreach (var item in query)
            {
                model.DcCancelDatesByBuilding.Add(item.BuildingId, item.UnRoutedPoGroup);
            }
            model.AtsDateList = _service.GetExistingAtsDates(model.PostedCustomerId).Select(p => Tuple.Create(p.AtsDate, p.EdiId, p.PoCount));

            return View(Views.Unrouted, model);
        }
        /// <summary>
        /// Creates EDI for selected orders
        /// </summary>
        /// <param name="UnroutedViewModel"></param>
        /// <returns>View Index.cshtml</returns>
        [HttpPost]
        public virtual ActionResult PrepareToRoute(UnroutedViewModel model)
        {
            //TC 7 : Do not select any PO or ATS date and click Assign button on unrouted.
            if (model.SelectedKeys == null || model.AtsDate == null)
            {
                ModelState.AddModelError("", "Please select at least one order to route and provide ATS date.");
            }
            //TC 8 : click Assign button on unrouted page with POs and ATS date.
            else
            {
                try
                {
                    var tuples = from key in model.SelectedKeys
                                 let upm = new UnroutedPoModel
                                 {
                                     Key = key
                                 }
                                 select new
                                 {
                                     Building = upm.BuildingId,
                                     PoKey = Tuple.Create(upm.PoId, upm.Iteration, upm.CustomerDcId)
                                 };
                    var pokeys = tuples.Select(p => p.PoKey).ToArray();
                    _service.AddPoToEdi(model.PostedCustomerId, pokeys, model.AtsDate.Value,model.IsAutomaticEdi);
                    model.RecentlyAssignedPoCount = pokeys.Length;
                    model.RecentlyAssignedGroup = new RoutingPoGroup(tuples.Select(p => p.Building).First(), model.AtsDate.Value);
                }
                catch (DbException ex)
                {
                    ModelState.AddModelError("", ex.InnerException.Message);
                }
            }
            return RedirectToAction(Actions.Unrouted(model));          
        }
        #endregion

        #region Routing

        /// <summary>
        /// Returns list of orders for whom EDI 753 has been created.
        /// </summary>
        /// <param name="model"></param>
        /// <returns>View Routing.cshtml</returns>
        [AuthorizeEx("Shipping requires Role {0}", Roles = "DCMS8_TMSMGR", Purpose = "Enables you to provide routing information to the orders")]
        public virtual ActionResult Routing(RoutingViewModel model)
        {
            //TC 9 : Click on Routing page link without selecting any customer.
            if (!ModelState.IsValid)
            {
                // Customer is required
                return View(this.Views.Routing, model);
            }

            // Add Posted customer to cookie.
            if (!string.IsNullOrEmpty(model.PostedCustomerId))
            {
                var cookieCustomer = new HttpCookie(ShippingAreaRegistration.COOKIE_PREFIX + ReflectionHelpers.NameFor((LayoutTabsViewModel m) => m.PostedCustomerId));
                cookieCustomer.Value = model.PostedCustomerId;
                cookieCustomer.Expires = DateTime.Now.AddDays(15);
                this.HttpContext.Response.Cookies.Add(cookieCustomer);
            }
           
            //Get list of RoutablePos.
            var list = _service.GetRoutablePos(model.PostedCustomerId, model.ShowRoutedOrders ? (bool?)null : false, model.StartDate, model.DcCancelDate, model.BuildingId);
            var groupedPoList = from po in list
                                where po.AtsDate.HasValue
                                orderby po.AtsDate, po.CustomerDcId, po.RoutingKey.PoId, po.CarrierId
                                group po by new RoutingPoGroup(po.BuildingId, po.AtsDate.Value) into g
                                select g;

            //Get sum of Weight ,volume,pieces and dollars
            foreach (var item in groupedPoList)
            {
                var polist = item.Select(p => new RoutablePoModel(p)).ToList();
                item.Key.PoCount = polist.Count;
                item.Key.PoCancelToday = polist.Any(p => (p.AtsDate.Value.Date == DateTime.Today.Date || p.AtsDate.Value.Date < DateTime.Today.Date) && string.IsNullOrWhiteSpace(p.LoadId) && p.PickUpDate == null);
                polist.Aggregate(item.Key, (sum, next) =>
                {
                    if (next.Weight.HasValue)
                    {
                        sum.TotalWeight = (sum.TotalWeight ?? 0) + next.Weight;
                    }
                    if (next.Pieces.HasValue)
                    {
                        sum.TotalPieces = (sum.TotalPieces ?? 0) + next.Pieces;
                    }
                    if (next.Volume.HasValue)
                    {
                        sum.TotalVolume = (sum.TotalVolume ?? 0) + next.Volume;
                    }
                    if (next.CountBoxes.HasValue)
                    {
                        sum.TotalCountBoxes = (sum.TotalCountBoxes ?? 0) + next.CountBoxes;
                    }
                    if (next.TotalDollars.HasValue)
                    {
                        sum.TotalDollarsOrdered = (sum.TotalDollarsOrdered ?? 0) + next.TotalDollars;
                    }
                    return sum;
                });
                model.GroupedPoList.Add(item.Key, polist);
            }

            var query = from pogroup in model.GroupedPoList.Keys
                        group pogroup by pogroup.BuildingId into g
                        select new
                        {
                            BuildingId = g.Key,
                            RoutingPoGroups = g.ToList()
                        };

            foreach (var item in query)
            {
                model.AtsDatesByBuilding.Add(item.BuildingId, item.RoutingPoGroups);
            }
            return View(this.Views.Routing, model);
        }

        /// <summary>
        /// Unroutes the passed PO. 
        /// </summary>
        /// <param name="model"></param>
        /// <remarks>
        /// Deletes the PO and puts back the original order info (dc,carrier,address) .
        /// 203:Error code for ajax call.
        /// </remarks>
        [HttpPost]
        public virtual ActionResult UndoRouting(string key)
        {
            //This is a safe code to handle situation key is empty.
            if (string.IsNullOrEmpty(key))
            {
                this.Response.StatusCode = 203;
                return Content("Inappropriate data can't perform delete operation");
            }
            var rkey = new RoutingKey(key);
            _service.UndoRoute(rkey);
            return Content(string.Format("PO {0} will now be visible on the Unrouted page", rkey.PoId));

        }
        /// <summary>
        /// Update routing information for the selected POs
        /// </summary>
        /// <param name="model"></param>
        /// <returns>View Routing.cshtml</returns>
        [HttpPost]
        public virtual ActionResult UpdateRouting(RoutingViewModel model)
        {
            //This is a safe code to handle situations that will not come under normal user practice
            if (!ModelState.IsValid)
            {
                return RedirectToAction(Actions.Routing(model));
            }
            var routingKeys = (from key in model.SelectedKeys
                               select new RoutingKey(key)
                                        ).ToArray();
            //TC 10 : Select some orders and click Routing Editor button from Routing View then provide values to updating routing information dialog and click Apply 
            if (model.UpdateCarrier || model.UpdateLoad || model.UpdateDc || model.UpdatePickUpDate)
            {
                try
                {
                    var count = _service.UpdateRouting(new RoutingUpdater
                    {
                        RoutingKeys = routingKeys,
                        CarrierId = model.RoutingInfo.CarrierId,
                        UpdateCarrierId = model.UpdateCarrier,
                        LoadId = model.RoutingInfo.LoadId,
                        UpdateLoadId = model.UpdateLoad,
                        CustomerDcId = model.RoutingInfo.CustomerDcId,
                        UpdateCustomerDcId = model.UpdateDc,
                        PickUpDate = model.RoutingInfo.PickUpDate,
                        UpdatePickupDate = model.UpdatePickUpDate
                    });

                    if (count > 0)
                    {
                        AddStatusMessage(string.Format("Changes applied on {0} POs",
                          count));
                    }
                }
                catch (DbException ex)
                {
                    ModelState.AddModelError("", ex.InnerException.Message);
                }
            }
            return RedirectToAction(Actions.Routing(model));
        }

        /// <summary>
        /// This method is used to check the scanned DC is valid or not.
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="customerDC"></param>
        ///  <remarks>
        /// 203:Error code for ajax call.
        /// </remarks>
        /// <returns>
        ///  If DC validated then nothing else status code 203 with prorper massage.
        /// </returns>

        [HttpPost]
        public virtual ActionResult ValidateDC(string customerId, string customerDC)
        {
            var dc = _service.GetDC(customerId, customerDC);

            //TC12: If the passed DC doesn't belong to existing DCs of customer.
            if (dc == null)
            {
                Response.StatusCode = 203;
                var msg = string.Format("Passed DC {0} does not belongs to existing DCs of customer: {1}.You can still continue with passed DC.'", customerDC, customerId);
                return ModelState.IsValid ? Content(msg) : ValidationErrorResult();
            }
            return Content("");
        }
        #endregion

        #region Routed

        /// <summary>
        /// Get detailed list of orders which are already routed.
        /// </summary>
        /// <param name="model"></param>
        /// <returns>View Routed.cshtml</returns>
        [AuthorizeEx("Shipping requires Role {0}", Roles = "DCMS8_TMSMGR", Purpose = "Enables you to Create BOL")]
        public virtual ActionResult Routed(RoutedViewModel model)
        {
            //TC 13 : Click Routed link without selecting any customer.
            if (!ModelState.IsValid)
            {
                return View(Views.Routed, model);
            }
            // Add Posted customer to cookie.
            if (!string.IsNullOrEmpty(model.PostedCustomerId))
            {
                var cookieCustomer = new HttpCookie(ShippingAreaRegistration.COOKIE_PREFIX + ReflectionHelpers.NameFor((LayoutTabsViewModel m) => m.PostedCustomerId));
                cookieCustomer.Value = model.PostedCustomerId;
                cookieCustomer.Expires = DateTime.Now.AddDays(15);
                this.HttpContext.Response.Cookies.Add(cookieCustomer);
            }

            //Get list of POs having routing information.
            var list = _service.GetRoutablePos(model.PostedCustomerId, true, null, null, null);
            var groupedPoList = from po in list
                                orderby po.BuildingId, po.AtsDate,po.CustomerDcId,po.LoadId, po.RoutingKey.PoId
                                group po by new RoutedPoGroup
                                {
                                    BuildingId = po.BuildingId,                                   
                                    AtsDate = po.AtsDate.Value.Date
                                } into g                                
                                select g;

            foreach (var item in groupedPoList)
            {
                var polist = item.Select(p => new RoutablePoModel(p)).ToArray();
                item.Key.EdiIdList = string.Join(",", polist.SelectMany(p => p.EdiList).Distinct());
                item.Key.PoCount = polist.Length;
                model.GroupedPoList.Add(item.Key, polist);              
            }
            // Deduce how many BOLs will be created basesd on values of Load,DC,Building in each row
            int curBolRowNumber = 0;
            for (var i = 0; i < model.GroupedPoList.Count; ++i)
            {
                var polist = model.GroupedPoList.Values[i];
                for (var j = 0; j < polist.Count; ++j)
                {

                    if (j == 0)
                    {
                        // For first row.
                        curBolRowNumber++;
                    }
                    else if 
                        
                    //  check if new BOL is created or not. we also compare EDI as in some scenarios PO's might have same DC, Load and Building but different EDI. 
                        (polist[j].LoadId == polist[j - 1].LoadId &&
                        (!polist[j].IsAsnCustomer || polist[j].CustomerDcId == polist[j-1].CustomerDcId) &&
                        polist[j].BuildingId == polist[j - 1].BuildingId)
                    {
                        // BOL has not changed

                    }
                    else
                    {
                        // BOL has changed.
                        curBolRowNumber++;

                    }

                    polist[j].BolRowNumber = curBolRowNumber;
                }
            }
            return View(Views.Routed, model);
        }

        /// <summary>
        /// Validates passed EDI.If valid then creates BOL for passed EDI.
        /// </summary>
        /// <param name="model"></param>
        /// <returns>View Routed.cshtml</returns>
        [HttpPost]
        public virtual ActionResult CreateBol(RoutedViewModel model)
        {
            if (string.IsNullOrEmpty(model.EdiId))
            {
                //Return in case no EDI is passed
                ModelState.AddModelError("", "BOL can not be created");
                return RedirectToAction(Actions.Routed(model));
            }
            var ediIdList=model.EdiId.Split(',').Where(p => !string.IsNullOrWhiteSpace(p)).Select(p => int.Parse(p)).ToArray();
            var polist = _service.EdiSummary(ediIdList);

            //Check if any PO in ATS don't have Routing info(Load or  PickUpDate)
            var routablePos = (from po in polist
                               where string.IsNullOrEmpty(po.Load_Id) && po.PickUp_Date == null
                               select new EdiPo
                               {                                  
                                   Po_Id = po.Po_Id
                               }).ToList();

            //TC 14 : When try to create BOL for an ATS Date that have one or more UnroutedPO.
            if (routablePos.Count > 0)
            {
                ModelState.AddModelError("", string.Format("BOL can not be created as some POs {0} of  ATS date {1:d} don't have routing info.Please go to routing page to either provide routing info or unroute PO.", string.Join(",", routablePos.Select(p => p.Po_Id)),model.BolAtsdate));
            }
            //TC 15 : When try to create BOL for an ATS Date that have more than one Load or Pickup date.
            else if (polist.Any(p => p.LoadCount > 1 || p.PickUpDateCount > 1 ))
            {
                ModelState.AddModelError("", "BOL can not be created as some POs have more than one Load or Pickup date");
            }
            //Create BOL for an ATS having routing(Load or PickUpdate) information for its all POs.
            else
            {
                var count = _service.CreateBol(ediIdList, model.PostedCustomerId);
                model.CreatedBolCount = count;
            }

            return RedirectToAction(Actions.Routed(model));
        }
        #endregion

        #region Appointments

        /// <summary>.
        /// Dislays appointment view
        /// </summary>
        /// <returns></returns>
        public virtual ActionResult AllAppointments(AppointmentViewModel model)
        {
            model.BuildingList = _service.GetBuildingList().Select(p => Map(p));
            ModelState.Clear();
            //Ignore customer cookie       
            model.PostedCustomerId = string.Empty;
            Appointment app;
            // When called from BOL page
            if (model.AppointmentId.HasValue)
            {
                app = _service.GetAppointmentById(model.AppointmentId.Value);
                if (app == null)
                {
                    this.AddStatusMessage("Appointment passed was invalid and has been ignored");                   
                }
                if (app != null)
                {
                    model.InitialDate = app.AppointmentTime;

                }
            }
            return View(Views.Appointment, model);
        }

        /// <summary>
        /// Displays the appointments view. Can post AppointmentId, CustomerId
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public virtual ActionResult Appointment(AppointmentViewModel model)
        {
            // Add Posted customer to cookie.
            if (!string.IsNullOrEmpty(model.PostedCustomerId))
            {
                var cookieCustomer = new HttpCookie(ShippingAreaRegistration.COOKIE_PREFIX + ReflectionHelpers.NameFor((LayoutTabsViewModel m) => m.PostedCustomerId));
                cookieCustomer.Value = model.PostedCustomerId;
                cookieCustomer.Expires = DateTime.Now.AddDays(15);
                this.HttpContext.Response.Cookies.Add(cookieCustomer);
            }           
            model.BuildingList = _service.GetBuildingList().Select(p => Map(p));
            Appointment app;          
            //TC 16 : Select a customer  and click Appointment link from anywhere in application or scan customerId on Appointment page.
            if (!string.IsNullOrWhiteSpace(model.PostedCustomerId))
            {
                app = _service.GetLastAppointmentOfCustomer(model.PostedCustomerId);
                //TC 17 : Scan a customer in Select customer text box on Appointment pages.
                if (app == null)
                {
                    this.AddStatusMessage("No appointments for customer found. All appointments are being displayed");
                }
            }
            //TC 18: Click on Appointment page on start of application
            else
            {
                app = null;
            }
            //TC 19: Select a customer and click Appointment link from anywhere in application or scan customerId on Appointment page.
            if (app != null)
            {
                model.InitialDate = app.AppointmentTime;

            }
            return View(Views.Appointment, model);
        }

        /// <summary> 
        /// Create new appointment or update existing one
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [AuthorizeEx("Shipping requires Role {0}", Roles = "DCMS8_TMSMGR", Purpose = "Enables you to Create/Delete/Edit Appointments")]
        public virtual ActionResult CreateUpdateAppointment(AppointmentModel model)
        {

            if (!ModelState.IsValid)
            {
                return ValidationErrorResult();
            }
            // Model validation should ensure this
            Debug.Assert(model.AppointmentDate != null);
            var app = new Appointment
            {
                BuildingId = model.BuildingId,
                CarrierId = model.CarrierId,
                Remarks = model.Remarks,
                PickUpDoor = model.PickUpDoor,
                AppointmentTime = model.AppointmentDate.Value,

            };
            //TC 20 : On Appointment page click on  '+' within dates to create new appointment.
            if (model.id == null)
            {
                _service.CreateAppointment(app);
                model.id = app.AppointmentId;
                model.AppointmentNumber = app.AppointmentNumber;
            }
            //TC 21 : On Appointment page click update icon for particular BOL within dates or scan an appointment in Find text box and then click Edit Appointment Link.
            else
            {
                app.AppointmentId = model.id.Value;
                app.RowSequence = model.RowSequence.Value;
                _service.UpdateAppointment(app);
                model.AppointmentNumber = app.AppointmentNumber;
            }
            model.AppointmentDate = app.AppointmentTime;
            return Json(model);
        }


        /// <summary>
        /// Delete appointment.
        /// </summary>
        /// <param name="appointmentId"></param>
        /// <returns></returns>
        [HttpPost]
        [AuthorizeEx("Shipping requires Role {0}", Roles = "DCMS8_TMSMGR", Purpose = "Enables you to Create/Delete/Edit Appointments")]
        public virtual ActionResult DeleteAppointment(int? appointmentId)
        {
            //This is an safe code this will not get passed in normal user practice.
            if (appointmentId == null)
            {
                return Content("Internal error. Can not delete.");
            }
            var app = _service.GetAppointmentById(appointmentId.Value);
            _service.DeleteAppointment(appointmentId.Value);
            return Content(string.Format("Appointment# {0} has been deleted.", app.AppointmentNumber));
        }

        /// <summary>
        /// Called via AJAX to get a list of events to display
        /// </summary>
        /// <param name="start">This parameter name must not be changed. AJAX call hardwires this.</param>
        /// <param name="end">This parameter name must not be changed. AJAX call hardwires this.</param>
        /// <param name="buildingIdList">Used as a filter</param>
        /// <param name="carrierId">Used as Filter</param>
        /// <param name="viewName">Name of the view being displayed. month|basicDay|week</param>
        /// <returns></returns>
        /// <remarks>
        /// </remarks>
        public virtual ActionResult GetAppointments(
            [ModelBinderAttribute(typeof(UnixTimestampBinder))]
            DateTimeOffset start,
            [ModelBinderAttribute(typeof(UnixTimestampBinder))]
            DateTimeOffset end,
            string[] buildingIdList, string customerId, string carrierId,
            CalendarViewName? viewName, bool? scheduled, bool? shipped)
        {
            var list = _service.GetAppointments(start, end, buildingIdList, customerId, carrierId, scheduled, shipped).Select(p => new AppointmentModel(p)).ToList();
            string partialName;

            switch (viewName)
            {
                case CalendarViewName.month:
                case CalendarViewName.basicWeek:
                    partialName = this.Views._monthHtmlPartial;
                    break;

                case CalendarViewName.basicDay:
                    partialName = this.Views._dayHtmlPartial;
                    UpdateListForDayView(list, start, end, shipped);
                    foreach (var app in list.OrderBy(p => p.AppointmentDate).Where((p, i) => i % 2 == 0))
                    {
                        app.AddClassName("appA");
                    }
                    break;

                default:
                    partialName = string.Empty;
                    break;
            }
            if (!string.IsNullOrEmpty(partialName))
            {
                list.ForEach(p =>
                {
                   
                    p.appointmentHtml = RenderPartialViewToString(partialName, p);
                    if (p.id == null)
                    {                       
                        p.AddClassName("app-unscheduled");
                    }
                    else
                    {
                        p.AddClassName("app-scheduled");
                    }
                });
            }
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Show all information of passed appointment id per shipping id in Day view.
        /// </summary>
        /// <param name="list"></param>
        private void UpdateListForDayView(List<AppointmentModel> list, DateTimeOffset start, DateTimeOffset end, bool? shipped)
        {
            //TC 22 : Click on any particular date or click on day button on appointment page.
            var bols = _service.GetScheduledBols(list.Where(p => p.id.HasValue).Select(p => p.id.Value), shipped);
            foreach (var item in list.Where(p => p.id.HasValue))
            {
                //TC 23 : Add class on alternate appointments so as to distinguish them from one another
                item.AppointmentBols = from bol in bols.Where(p => p.AppointmentId == item.id)
                                       select new AppointmentBolModel
                                       {
                                           AppointmentId = bol.AppointmentId,
                                           AppointmentDateTime = bol.AppointmentDateTime,
                                           AppointmentNumber = bol.AppointmentNumber,
                                           CustomerId = bol.CustomerId,
                                           CustomerName = bol.CustomerName,
                                           EndTime = bol.EndTime,
                                           ShippingId = bol.ShippingId,
                                           BoxesLoadedCount = bol.LoadedBoxCount,
                                           LoadedPalletCount = bol.LoadedPalletCount,
                                           StartTime = bol.StartTime,
                                           TotalPalletCount = bol.TotalPalletCount,
                                           BoxesUnverifiedCount = bol.UnverifiedBoxCount,
                                           BoxesAtDockCount = bol.AtDockBoxCount,
                                           BoxesUnpalletizeCount = bol.UnpalletizeBoxCount,
                                           NoBolPoCount = bol.NoBolPoCount,
                                           BolPoCount = bol.BolPoCount
                                       };
            }

            // Handle unscheduled appointments
            bols = _service.GetUnscheduledBols(start, end, shipped);
            foreach (var item in list.Where(p => !p.id.HasValue))
            {
                item.AppointmentBols = from bol in bols.Where(p => p.AppointmentDateTime == item.AppointmentDate && p.BuildingId == item.BuildingId)
                                       select new AppointmentBolModel
                                       {
                                           AppointmentId = bol.AppointmentId,
                                           AppointmentDateTime = bol.AppointmentDateTime,
                                           AppointmentNumber = bol.AppointmentNumber,
                                           CustomerId = bol.CustomerId,
                                           CustomerName = bol.CustomerName,
                                           EndTime = bol.EndTime,
                                           ShippingId = bol.ShippingId,
                                           BoxesLoadedCount = bol.LoadedBoxCount,
                                           LoadedPalletCount = bol.LoadedPalletCount,
                                           StartTime = bol.StartTime,
                                           TotalPalletCount = bol.TotalPalletCount,
                                           BoxesUnverifiedCount = bol.UnverifiedBoxCount,
                                           BoxesAtDockCount = bol.AtDockBoxCount,
                                           BoxesUnpalletizeCount = bol.UnpalletizeBoxCount,
                                           NoBolPoCount = bol.NoBolPoCount,
                                           BolPoCount = bol.BolPoCount
                                       };
            }
        }

        private string RenderPartialViewToString(string viewName, object model)
        {
            //This is a safe code never going to get called with in normal user practice.
            if (string.IsNullOrEmpty(viewName))
                viewName = ControllerContext.RouteData.GetRequiredString("action");

            ViewData.Model = model;


            using (StringWriter sw = new StringWriter())
            {
                ViewEngineResult viewResult = ViewEngines.Engines.FindPartialView(ControllerContext, viewName);
                ViewContext viewContext = new ViewContext(ControllerContext, viewResult.View, ViewData, TempData, sw);
                viewResult.View.Render(viewContext, sw);

                return sw.GetStringBuilder().ToString();
            }
        }

        /// <summary>
        /// Returns the passed appointment via AJAX.
        /// </summary>
        /// <param name="appointmentNumber"></param>
        /// <returns></returns>
        public virtual ActionResult GetAppointmentByNumber(int appointmentNumber)
        {
            var app = _service.GetAppointmentByNumber(appointmentNumber);
            //TC 24 : Input an non existing appointment number in Appointment number text box on Appointment page and click GO.
            if (app == null)
            {
                Response.StatusCode = 203;
                return Content("Appointment not found");
            }
            return Json(new AppointmentModel(app), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Update truck arrival delay time.
        /// </summary>
        /// <param name="Id">Id of appointment</param>
        /// <param name="truckArrivalTime">Time at which truck actually arrived</param>
        /// <param name="appointmentTime">Appointment time</param>
        /// <returns></returns>
        [HttpPost]
        [AuthorizeEx("Shipping requires Role {0}", Roles = "DCMS8_TMSMGR", Purpose = "Enables you to Create/Delete/Edit Appointments")]
        public virtual ActionResult UpdateTruckArrival(int id,
            [ModelBinderAttribute(typeof(UnixTimestampBinder))]
            DateTimeOffset? truckArrivalTime,
            [ModelBinderAttribute(typeof(UnixTimestampBinder))]
            DateTimeOffset appointmentTime)
        {
            TimeSpan? delay;
            //TC 25 : Update Truck Arrival date from any day view on Appointment page for any appointment.
            if (truckArrivalTime.HasValue)
            {
                // TODO: What happens if arrival time is past midnight
                delay = TimeSpan.FromMinutes(truckArrivalTime.Value.TimeOfDay.TotalMinutes - appointmentTime.TimeOfDay.TotalMinutes);
            }
            else
            {

                delay = null;
            }
            _service.UpdateArrivalTime(id, delay);
            return Content("Arrival Time Updated");
        }
        #endregion

        #region Bol

        /// <summary>
        /// Returns list of appointments against passed parameters
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="carrierIdList"></param>
        /// <returns></returns>
        public virtual ActionResult GetAppointmentsForBol(
           [ModelBinderAttribute(typeof(UnixTimestampBinder))]
            DateTimeOffset start,
           [ModelBinderAttribute(typeof(UnixTimestampBinder))]
            DateTimeOffset end)
        {
            var list = _service.GetAppointments(start, end, null, null, null, true, null).Select(p => new AppointmentModel(p)).ToList();
            list.ForEach(p =>
            {
                p.appointmentHtml = RenderPartialViewToString(Views._bolAppointmentHtmlPartial, p);

            });
            return Json(list, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// Assign/unassigned appointment to/from BOLs based upon flag Assign Flag. if flag is true assign appointment, false-> unassign       
        /// </summary>
        /// <param name="model">SelectedBols,AppointmentId</param>
        /// <returns></returns>
        [HttpPost]
        public virtual ActionResult AssignAppointmentToBol(BolViewModel model)
        {
            //This is a safe code to handle situations that will not appear in normal user practice.
            if (model.AssignFlag == null)
            {
                ModelState.AddModelError("", "Internal error:Can't perform operation");
            }
            //TC 26 : Select BOLs and Appointment and click Assign BOL in Pick Appointment dialog on Bol Page.
            if (model.AssignFlag == true)
            {
                //This is a safe code.
                if (model.SelectedBols == null || model.InitialAppointment == null || model.InitialAppointment.id == null)
                {
                    ModelState.AddModelError("", "Please select at least one BOL to set appointment");
                }
                else
                {
                    var app = _service.GetAppointmentById(model.InitialAppointment.id.Value);
                    //Safe code.
                    if (app == null)
                    {
                        ModelState.AddModelError("", "Selected appointment could not be found");
                    }
                    else
                    {
                        _service.AssignAppointmentToBol(model.SelectedBols, model.InitialAppointment.id.Value);
                        model.AssignedAppointmentId = app.AppointmentId;
                        model.AssignedBolCount = model.SelectedBols.Count;
                        model.AssignedAppointmentNumber = app.AppointmentNumber;
                    }

                }
            }
            //TC 27 : Select BOLs of Appointment and Click on Unassigned Bol button in Pick Appointment dialog
            if (model.AssignFlag == false)
            {
                //This is a safe code.If only comes if jscript allow to call this function without selection.
                if (model.SelectedBols == null)
                {
                    ModelState.AddModelError("", "Please select at least one BOL to unassigned");
                }
                else
                {
                    _service.UnAssignAppointment(model.SelectedBols);
                    AddStatusMessage(string.Format("Appointment unassigned from {0} BOLs", model.SelectedBols.Count()));
                }
            }
            model.InitialAppointment = null;
            return RedirectToAction(Actions.Bol(model));
        }
        /// <summary>
        /// Get detailed list of unshipped BOLs for the passed customer
        /// </summary>
        /// <param name="model"></param>
        /// <returns>List of BOLs</returns>       
        [AuthorizeEx("Shipping requires Role {0}", Roles = "DCMS8_TMSMGR", Purpose = "Enables you to  Assign/Unassigned appointments to BOL")]
        public virtual ActionResult Bol(BolViewModel model)
        {
            //TC 28: Do not select any customer and click Start Over Link in BOL page or click BOL link from any where in application
            if (!ModelState.IsValid)
            {
                return View(Views.Bol, model);
            }
            // Add Posted customer to cookie.
            if (!string.IsNullOrEmpty(model.PostedCustomerId))
            {
                var cookieCustomer = new HttpCookie(ShippingAreaRegistration.COOKIE_PREFIX + ReflectionHelpers.NameFor((LayoutTabsViewModel m) => m.PostedCustomerId));
                cookieCustomer.Value = model.PostedCustomerId;
                cookieCustomer.Expires = DateTime.Now.AddDays(15);
                this.HttpContext.Response.Cookies.Add(cookieCustomer);
            }
            // When Initial appointment is passed from dayView of appointment page ,show selected bols by default
            if (model.InitialAppointment != null && model.InitialAppointment.id.HasValue)
            {
                var app = _service.GetAppointmentById(model.InitialAppointment.id.Value);
                if (app == null)
                {
                    this.AddStatusMessage("Appointment passed was invalid and has been ignored");
                }
                else
                {
                    model.InitialAppointment = new AppointmentModel(app);
                    model.ShowScheduledAlso = true;
                }
            }
            var bolList = _service.GetBols(model.PostedCustomerId, model.ShowScheduledAlso);
            //TC 29 : Select a customer that have unshipped BOLs and then click Bill Of Lading Link from any where or click Start Over Link from BOL page.
            if (bolList.Any())
            {
                model.Bols = (from bol in bolList
                              orderby bol.AppointmentDateTime, bol.AppointmentId, bol.ShippingId
                              select new BolModel
                              {
                                  BolCreatedBy = bol.BolCreatedBy,
                                  BolCreatedOn = bol.BolCreatedOn,
                                  ShipDate = bol.ShipDate,
                                  ShippingId = bol.ShippingId,
                                  PickUpDate = !string.IsNullOrEmpty(bol.PickupDateList.Max().ToString()) ? bol.PickupDateList.Max() : null,
                                  StartDate = bol.StartDate,
                                  DcCancelDate = bol.DcCancelDate,
                                  CancelDate = bol.CancelDate,
                                  CustomerDcId = bol.CustomerDcId,
                                  PoCount = bol.PoCount,
                                  EdiId = bol.EdiId,
                                  PickupDateList = bol.PickupDateList,
                                  NumberOfPickupDates = bol.PickupDateList.Count(),
                                  Appointment = new AppointmentModel
                                  {
                                      id = bol.AppointmentId,
                                      AppointmentNumber = bol.AppointmentNumber,
                                      // In case of no appointment date, Pickupdate or Ats date is considered as appointment date   
                                      AppointmentDate = bol.AppointmentDateTime.HasValue ? bol.AppointmentDateTime.Value : bol.PickupDateList.Max() ?? bol.AtsDate,
                                      CarrierId = bol.CarrierId,
                                      CarrierName = bol.CarrierDescription,
                                      BuildingId = bol.ShipBuilding,
                                      title = string.Format("Pickup on {2:d} by {0}: {1}", bol.CarrierId, bol.CarrierDescription, bol.PickupDateList.Max() ?? bol.AtsDate)
                                  },
                                  AtsDate = bol.AtsDate
                              }).ToArray();
            }
            else
            {
                AddStatusMessage("No BOL exists for selected customer");
            }
            return View(Views.Bol, model);
        }

        /// <summary>
        /// Deletes the unshipped Bill of ladings.
        /// </summary>
        /// <param name="model"></param>
        /// <returns>Action Bol which in turn returns view Bol.cshtml</returns>
        /// Returns message. In case of error, status code 203 is returned with proper message
        public virtual ActionResult DeleteBol(string customerId,string shippingIdList)
        {
            var msg = string.Empty;
            //This is a safe code to handle situation that will not come under normal user practice
            if (string.IsNullOrWhiteSpace(shippingIdList) || string.IsNullOrWhiteSpace(customerId))
            {
                this.Response.StatusCode = 203;
                return Content("Internal error:Can't  delete.");
            }
            ////This is an safe code to handle situation that will not come under normal user practice
            if (_service.DeleteBol(customerId,shippingIdList))
            {
                msg = string.Format(" Bol {0} has been successfully deleted", shippingIdList);
            }
            //This is an safe code to handle situation that will not come under normal user practice
            else
            {
                Response.StatusCode = 203;
                msg = string.Format("Internal error:Can't  delete.");
            }

            return Content(msg);
        }

        #endregion

        #region PoSearch

        /// <summary>
        /// Search for PO and redirect to appropriate UI based on PO status(Unrouted,Routed etc)
        /// </summary>
        /// <param name="poId">Partial POs are acceptable. we check for %PO%</param>
        /// <returns></returns>
        /// <remarks>
        /// This should be called via AJAX with POID passed. On success, returns the url to redirect to.
        /// If PO is not found, returns null.
        /// </remarks>
        public virtual ActionResult PoSearch(string poId)
        {
            //TC 30: When no PO is provided to search
            if (string.IsNullOrWhiteSpace(poId))
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
            var list = _service.GetPoStatus(poId);
            // TC 31: When PO entered is invalid
            if (list.Count == 0)
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
            if (list.Count > 1)
            {
                return Json(Url.Action(this.Actions.PoSearchResults(poId)), JsonRequestBehavior.AllowGet);
            }
            string url;
            var item = list.Select(p => new PoStatusModel(p)).Single();
            switch (item.Status)
            {
                case RoutingStatus.Unrouted:
                    var UnRoutedPoGroup = new UnroutedPoGroup(item.BuildingId, item.DcCancelDate.HasValue ? item.DcCancelDate.Value : (DateTime?)null);
                    url = Url.Action(MVC_Shipping.Shipping.Home.Unrouted(new UnroutedViewModel(item.CustomerId, UnRoutedPoGroup)));
                    break;

                case RoutingStatus.Routing:
                    if (item.AtsDate.HasValue)
                    {
                        url = Url.Action(MVC_Shipping.Shipping.Home.Routing(new RoutingViewModel(item.CustomerId, false, new RoutingPoGroup(item.BuildingId, item.AtsDate.Value))));
                    }
                    else
                    {
                        throw new NotImplementedException("No ATS Date for Routing");
                    }
                    break;

                case RoutingStatus.Routed:
                    if (item.AtsDate.HasValue)
                    {
                        url = Url.Action(MVC_Shipping.Shipping.Home.Routed(new RoutedViewModel(item.CustomerId, new RoutedPoGroup(item.AtsDate.Value))));
                    }
                    else
                    {
                        throw new NotImplementedException("No ATS Date for Routed");
                    }
                    break;

                case RoutingStatus.InBol:
                    // BOL Has been created. Redirect to report.
                    url = string.Format("{0}?shipping_id={1}", item.BolDetailUrl, item.ShippingId);
                    break;

                default:
                    // Should never happen
                    throw new NotImplementedException();                   
            }       
            return Json(url, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Called when PO has multiple status
        /// </summary>
        /// <param name="poPattern"></param>
        /// <returns></returns>
        public virtual ActionResult PoSearchResults(string poPattern)
        {
            var list = _service.GetPoStatus(poPattern);
            var model = new PoSearchResultsViewModel();
            model.PoList = list.Select(p => new PoStatusModel(p)).ToList();

            foreach (var item in model.PoList)
            {
                switch (item.Status)
                {
                    case RoutingStatus.Unrouted:
                        var UnRoutedPoGroup = new UnroutedPoGroup(item.BuildingId, item.DcCancelDate.HasValue ? item.DcCancelDate.Value : (DateTime?)null);
                        item.CustomUrl = Url.Action(MVC_Shipping.Shipping.Home.Unrouted(new UnroutedViewModel(item.CustomerId, UnRoutedPoGroup)));
                        break;

                    case RoutingStatus.Routing:
                        if (item.AtsDate.HasValue)
                        {
                            item.CustomUrl = Url.Action(MVC_Shipping.Shipping.Home.Routing(new RoutingViewModel(item.CustomerId, false, new RoutingPoGroup(item.BuildingId, item.AtsDate.Value))));
                        }
                        else
                        {
                            throw new NotImplementedException("No ATS Date for Routing");
                        }
                        break;

                    case RoutingStatus.Routed:
                        if (item.AtsDate.HasValue)
                        {
                            item.CustomUrl = Url.Action(MVC_Shipping.Shipping.Home.Routed(new RoutedViewModel(item.CustomerId, new RoutedPoGroup(item.AtsDate.Value))));
                        }
                        else
                        {
                            throw new NotImplementedException("No ATS Date for Routed");
                        }
                        break;

                    case RoutingStatus.InBol:
                        // BOL Has been created. Redirect to report.
                        item.CustomUrl = string.Format("{0}?shipping_id={1}", item.BolDetailUrl, item.ShippingId);
                        break;

                    default:
                        // Should never happen
                        throw new NotImplementedException();                    
                }
            }
            return View(this.Views.PoSearchResults, model);           
        }
        #endregion
    }
}
