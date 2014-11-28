using DcmsMobile.Inquiry.Areas.Inquiry.SharedViews;
using DcmsMobile.Inquiry.Helpers;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;

namespace DcmsMobile.Inquiry.Areas.Inquiry.CustomerEntity
{
    [RoutePrefix("customer")]
    public partial class CustomerEntityController : InquiryControllerBase
    {
        private Lazy<CustomerEntityRepository> _repos;
        protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        {
            base.Initialize(requestContext);
            _repos = new Lazy<CustomerEntityRepository>(() => new CustomerEntityRepository(requestContext.HttpContext.User.Identity.Name,
                requestContext.HttpContext.Request.UserHostName ?? requestContext.HttpContext.Request.UserHostAddress));
        }

        protected override void Dispose(bool disposing)
        {
            if (_repos != null && _repos.IsValueCreated)
            {
                _repos.Value.Dispose();
                _repos = null;
            }
            base.Dispose(disposing);
        }


        [Route("list")]
        public virtual ActionResult CustomerList()
        {
            var customerList = _repos.Value.GetCustomerList();
            var model = new CustomerListViewModel()
            {
                CustomerList = customerList.Select(p => new CustomerHeadlineModel
                {
                    CustomerId = p.CustomerId,
                    CustomerName = p.CustomerName,
                    CustomerTypeDescription = p.CustomerTypeDescription,
                    PickslipImportDate = p.PickslipImportDate,
                    PoCount =p.PoCount
                }).ToList()
            };

            return View(Views.CustomerList, model);

        }



        [Route("{id?}", Name = DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_SearchCustomer1)]
        [SearchQuery(@"SELECT {0}, customer_id, 'Customer ' || customer_id, NULL, NULL FROM <proxy />cust WHERE customer_id= :search_text")]
        public virtual ActionResult Customer(string id)
        {
            //var regexItem = new Regex(":");

            //if (regexItem.IsMatch(id))
            //{
            //    id = id.Substring(0, id.IndexOf(":"));
            //}
         
            var customer = _repos.Value.GetCustomerInfo(id);
            if (customer == null)
            {
                this.AddStatusMessage(string.Format("No info found for {0}", id));
                return RedirectToAction(MVC_Inquiry.Inquiry.Home.Index());
            }
            var model = new CustomerViewModel
            {
                AccountType = customer.CustomerTypeDescription,
                AmsFlag = customer.AmsFlag,
                Asn_flag = customer.Asn_flag,
                CarrierId = customer.CarrierId,
                CarrierDescription = customer.CarrierDescription,
                Category = customer.Category,
                CclShortName = customer.CclShortName,
                CustomerId = customer.CustomerId,
                CustomerName = customer.CustomerName,
                DefaultPickMode = customer.DefaultPickMode,
                EdiFlag = customer.EdiFlag,
                MaxPiecesPerBox = customer.MaxPiecesPerBox,
                MinPiecesPerBox = customer.MinPiecesPerBox,
                MpsShortName = customer.MpsShortName,
                NumberOfCcl = customer.NumberOfCcl,
                NumberOfMps = customer.NumberOfMps,
                NumberOfPspb = customer.NumberOfPspb,
                NumberOfShlbl = customer.NumberOfShlbl,
                NumberOfUcc = customer.NumberOfUcc,
                PspbShortName = customer.PspbShortName,
                ScoFlag = customer.ScoFlag,
                ShlblShortName = customer.ShlblShortName,
                UccShortName = customer.UccShortName,
                CustVas     = customer.CustVas
            };
            var route = Url.RouteCollection[DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_ManagePickWave1];
            if (route != null)
            {
                model.UrlManageCustomerPickwave = Url.RouteUrl(DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_ManagePickWave1, new
                   {
                       id = model.CustomerId
                   });
            }
            //model.DcmsLinks.Add(new DcmsLinkModel
            //{
            //    ShortDescription = "Manage Customer Pickwave",
            //    Url = route == null ? string.Empty : Url.RouteUrl(DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_ManagePickWave1, new
            //    {
            //        id = model.CustomerId
            //    })
            //});

            route = Url.RouteCollection[DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_VASConfigration];
            if (route != null)
            {
                model.UrlCustomerVas = Url.RouteUrl(DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_VASConfigration, new
                {
                    id = model.CustomerId
                });
            }
            //model.DcmsLinks.Add(new DcmsLinkModel
            //{
            //    ShortDescription = "Configure customer",
            //    Url = route == null ? string.Empty : Url.RouteUrl(DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_VASConfigration)
            //});

            route = Url.RouteCollection[DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_CustomerRoutingSummary1];
            if (route != null)
            {
                model.UrlRouteOrder = Url.RouteUrl(DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_CustomerRoutingSummary1, new
                  {
                      id = model.CustomerId
                  });
            }
            //model.DcmsLinks.Add(new DcmsLinkModel
            //{
            //    ShortDescription = "Route Order",
            //    Url = route == null ? string.Empty : Url.RouteUrl(DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_CustomerRoutingSummary1, new
            //    {
            //        id = model.CustomerId
            //    })
            //});

            return View(Views.Customer, model);
        }

        [Route("orders")]
        public virtual ActionResult GetRecentOrders(string customer)
        {
            if (string.IsNullOrWhiteSpace(customer))
            {
                throw new ArgumentNullException("customer");
            }
            //var orders = _repos.Value.GetRecentOrders(customer, 200).Select(p => new RecentPoModel(p)).ToArray();

            var orders = new RecentPoListViewModel
            {
                PoList = _repos.Value.GetRecentOrders(customer, 201).Select(p => new RecentPoModel(p)).ToArray()
            };

            return PartialView(MVC_Inquiry.Inquiry.SharedViews.Views._recentPoListPartial, orders);
        }

        [Route("excel/{id}")]
        public virtual ActionResult CustomerExcel(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException("id");
            }
            var orders = _repos.Value.GetRecentOrders(id, GlobalConstants.MAX_EXCEL_ROWS).Select(p => new RecentPoModel(p)).ToArray();
            var result = new ExcelResult("Customer_" + id);
            result.AddWorkSheet(orders, "Recent PO", "Active POs of Customer " + id);
            return result;
        }

        /// <summary>
        /// Method for Customer Auto complete.
        /// </summary>
        /// <param name="term"></param>
        /// <returns></returns>
        [Route("ac/{term?}")]
        public virtual ActionResult CustomerAutocomplete(string term)
        {
            try
            {
                return Json(from p in _repos.Value.CustomerAutoComplete(term.ToUpper())
                            select new
                            {
                                label = string.Format("{0}:{1}", p.Item1, p.Item2),
                                value = p.Item1
                            }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(Enumerable.Repeat(new
                {
                    label = ex.Message
                }, 1), JsonRequestBehavior.AllowGet);
            }
        }
    }
}