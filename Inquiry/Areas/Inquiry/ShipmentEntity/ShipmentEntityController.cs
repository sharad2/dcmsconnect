using DcmsMobile.Inquiry.Areas.Inquiry.SharedViews;
using DcmsMobile.Inquiry.Helpers;
using System;
using System.Data.Common;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DcmsMobile.Inquiry.Areas.Inquiry.ShipmentEntity
{
    // [RoutePrefix("shipment")]
    public partial class ShipmentEntityController : InquiryControllerBase
    {
        private Lazy<ShipmentEntityRepository> _repos;
        protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        {
            base.Initialize(requestContext);
            _repos = new Lazy<ShipmentEntityRepository>(() => new ShipmentEntityRepository(requestContext.HttpContext.User.Identity.Name,
                requestContext.HttpContext.Request.UserHostName ?? requestContext.HttpContext.Request.UserHostAddress));
        }

        protected override void Dispose(bool disposing)
        {
            if (_repos != null && _repos.IsValueCreated)
            {
                _repos.Value.Dispose();
            }
            base.Dispose(disposing);
        }


        [Route("bol/{id}", Name = DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_SearchOutboundShipment1)]
        [SearchQuery(@"Select {0}, S.SHIPPING_ID, 'Outbound Shipment ' || S.SHIPPING_ID, NULL, NULL FROM <proxy />SHIP S 
         WHERE (S.PARENT_SHIPPING_ID = :search_text or S.SHIPPING_ID = :search_text) AND rownum &lt; 2")]
        public virtual ActionResult OutboundShipment(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return RedirectToAction(MVC_Inquiry.Inquiry.Home.Index());
            }
            var shipment = _repos.Value.GetOutboundShipment(id, null);
            if (shipment == null)
            {
                AddStatusMessage(string.Format("No info found for Outbund Shipment {0}", id));
                return RedirectToAction(MVC_Inquiry.Inquiry.Home.Index());
            }
            var model = new ParentShipmentViewModel(shipment);
            model.ShipmentDetail = _repos.Value.GetDetailsOfOutboundShipment(shipment.ParentShippingId, null).Select(p => new ParentShipmentContentModel(p)).ToArray();
            //model.Title = string.Format("BOL {0}", shipment.ParentShippingId);
            //model.ScannedBOL = model.ShipmentDetail.Count > 1 ? id : null;
            model.PrinterList = _repos.Value.GetBolPrinters().Select(p => new SelectListItem
                {
                    Text = string.Format("{0} : {1}", p.Item1, p.Item2),
                    Value = p.Item1
                });
            var cookie = this.Request.Cookies[GlobalConstants.COOKIE_BOL_PRINTER];
            if (cookie != null)
            {
                model.PrinterId = cookie.Value;
            }
            return View(Views.ParentShipment, model);
        }

        [HttpPost]
        [Route("bol/print")]
        public virtual ActionResult PrintBol(string parentShippingId, string printerId)
        {
            if (string.IsNullOrWhiteSpace(printerId))
            {

                this.AddStatusMessage(string.Format("Please choose a printer"));
                return RedirectToAction(Actions.OutboundShipment(parentShippingId));
            }
            try
            {
                _repos.Value.PrintBol(parentShippingId, printerId, 1);
                this.AddStatusMessage(string.Format("Printing has been done successfully."));
                var cookie = new HttpCookie(GlobalConstants.COOKIE_BOL_PRINTER, printerId)
                {
                    Expires = DateTime.Now.AddMonths(1)
                };
                this.Response.Cookies.Add(cookie);
            }
            catch (DbException ex)
            {
                ModelState.AddModelError("PrintException", ex.InnerException);
            }
            return RedirectToAction(Actions.OutboundShipment(parentShippingId));
        }

        [Route("mbol/{id}", Name = DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_SearchMasterBol1)]
        [SearchQuery(@"select {0}, S.MBOL_ID, 'Master BOL ' || S.MBOL_ID,Null,Null from <proxy />SHIP S WHERE S.MBOL_ID = :search_text and ROWNUM &lt; 2")]
        public virtual ActionResult MasterBol(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return RedirectToAction(MVC_Inquiry.Inquiry.Home.Index());
            }
            var shipment = _repos.Value.GetOutboundShipment(null, id);
            if (shipment == null)
            {
                AddStatusMessage(string.Format("No info found for Master BOL {0}", id));
                return RedirectToAction(MVC_Inquiry.Inquiry.Home.Index());
            }
            var model = new MasterBolViewModel(shipment);

            model.ShipmentList = _repos.Value.GetMasterBolShipments(id).Select(p => new MasterBolShipmentModel(p)).ToList();

            model.PrinterList = _repos.Value.GetBolPrinters().Select(p => new SelectListItem
            {
                Text = string.Format("{0} : {1}", p.Item1, p.Item2),
                Value = p.Item1
            });
            //model.IsMbolShipment = true;
            var cookie = this.Request.Cookies[GlobalConstants.COOKIE_BOL_PRINTER];
            if (cookie != null)
            {
                model.PrinterId = cookie.Value;
            }
            return View(Views.MasterBol, model);
        }



        [Route("mbol/excel/{id}")]
        public virtual ActionResult MasterBolExcel(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException("id");
            }
            var ShipmentList = _repos.Value.GetMasterBolShipments(id).Select(p => new MasterBolShipmentModel(p)).ToList();
            var result = new ExcelResult("MasterBOL_" + id);
            result.AddWorkSheet(ShipmentList, "Master BOL Shipment List", "Shipment in Master BOL " + id);
            return result;
        }

        [Route("mbol/print")]
        public virtual ActionResult PrintMasterBol(string address, string city, string state, string zipcode, string country, string mBolId, string printerId)
        {
            var addressLines = new string[4];
            var add = address.Split(new string[] { "\r\n" }, StringSplitOptions.None);
            for (int i = 0; i < 4; i++)
            {
                if (i < add.Count())
                {
                    addressLines[i] = add[i];
                }
                else
                {
                    addressLines[i] = string.Empty;
                }
            }

            try
            {
                _repos.Value.PrintMasterBol(addressLines, city, state, zipcode, country, mBolId, printerId);
                this.AddStatusMessage(string.Format("Printing has been done successfully."));
                var cookie = new HttpCookie(GlobalConstants.COOKIE_BOL_PRINTER, printerId)
                {
                    Expires = DateTime.Now.AddMonths(1)
                };
                this.Response.Cookies.Add(cookie);
            }
            catch (DbException ex)
            {
                ModelState.AddModelError("PrintException", ex.InnerException);
            }
            //var add = Regex.Split(address, "\r\n");
            //string[] stringSeperator = new string[] { "\r\n" };
            //var a = address.Split(new string[] { "\r\n" }, StringSplitOptions.None).Select(p => p);            
            return RedirectToAction(Actions.MasterBol(mBolId));
        }

        [Route("bol/excel/{id}")]
        public virtual ActionResult OutboundShipmentExcel(string id)
        {
            var result = new ExcelResult("OutboundShipment_" + id);
            result.AddWorkSheet(_repos.Value.GetDetailsOfOutboundShipment(id, null).Select(p => new ParentShipmentContentModel(p)).ToArray(), "Shipment Details", "Details of Shipment " + id);
            return result;
        }

        [SearchQuery(@"select {0}, TO_CHAR(AP.APPOINTMENT_ID), 'Appointment Date ' || TO_CHAR(AP.APPOINTMENT_DATE), NULL, NULL
FROm <proxy />APPOINTMENT AP WHERE AP.APPOINTMENT_NUMBER = :int_value")]
        [Route("app/{id:int}", Name = DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_SearchAppointmentNumber1)]
        public virtual ActionResult Appointment(int id)
        {
            var model = _repos.Value.GetAppointmentDetails(id);
            if (model == null)
            {
                this.AddStatusMessage(string.Format("No Appointment find for Appointment Number {0}", id));
                return RedirectToAction(MVC_Inquiry.Inquiry.Home.Index());
            }
            var appVeiwModel = new AppointmentViewModel(model);
            var route = Url.RouteCollection[DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_Appointment];
            if (route != null)
            {
                appVeiwModel.UrlManageAppointmentLink = Url.RouteUrl(DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_Appointment);
            }
            //appVeiwModel.DcmsLinks.Add(new DcmsLinkModel
            //{
            //    ShortDescription = "Manage Appointment",
            //    Url = route == null ? string.Empty : Url.RouteUrl(DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_Appointment)
            //});

            route = Url.RouteCollection[DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_ScanToTruck1];
            if (route != null)
            {
                appVeiwModel.UrlLoadTruck = Url.RouteUrl(DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_ScanToTruck1, new
                {
                    id = model.AppointmentNumber
                });
            }
            //appVeiwModel.DcmsLinks.Add(new DcmsLinkModel
            //{
            //    ShortDescription = "Load Truck",
            //    Url = route == null ? string.Empty : Url.RouteUrl(DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_ScanToTruck1, new
            //    {
            //        id = model.AppointmentNumber
            //    })
            //});

            return View(Views.Appointment, appVeiwModel);
        }


        [Route("bol/list")]
        public virtual ActionResult ParentShipmentList()
        {
            var list = _repos.Value.GetParentShipmentList();
            var model = new ParentShipmentListViewModel
            {
                ParentShipmentList = list.Select(p => new ParentShipmentHeadlineModel
                {
                    ParentShippingId = p.ParentShippingId,
                    MBolID = p.MBolID,
                   // ArrivalDate = p.ArrivalDate,
                    ShippingDate = p.ShippingDate,
                    StatusShippedDate = p.StatusShippedDate,
                    BoxCount = p.BoxCount,
                    CarrierId = p.CustomerID,
                    CarrierName = p.CarrierName,
                    CustomerID = p.CustomerID,
                    CustomerName = p.CustomerName
                }).ToList()
            };
            return View(Views.ParentShipmentList,model);
        }
    }
}