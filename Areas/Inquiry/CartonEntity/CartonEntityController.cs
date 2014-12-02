using DcmsMobile.Inquiry.Areas.Inquiry.SharedViews;
using DcmsMobile.Inquiry.Helpers;
using System;
using System.Data.Common;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DcmsMobile.Inquiry.Areas.Inquiry.CartonEntity
{
    [RoutePrefix("ctn")]
    public partial class CartonEntityController : InquiryControllerBase
    {
        private Lazy<CartonEntityRepository> _repos;
        protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        {
            base.Initialize(requestContext);
            _repos = new Lazy<CartonEntityRepository>(() => new CartonEntityRepository(requestContext.HttpContext.User.Identity.Name,
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


        /// <summary>
        /// Active carton
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Route("{id}", Name = DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_SearchCarton1)]
        [SearchQuery(@"SELECT {0}, carton_id, 'Carton received on ' || TO_CHAR(insert_date), NULL, NULL FROM <proxy />src_carton WHERE carton_id = :search_text", Group = "ctn", Rating = 30)]
        public virtual ActionResult Carton(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return RedirectToAction(MVC_Inquiry.Inquiry.Home.Index());
            }
            var model = new CartonViewModel();
            var ctn = _repos.Value.GetActiveCarton(id);
            if (ctn != null)
            {
                model = new CartonViewModel(ctn)
                {
                    AllowPrinting = true,
                    PrinterList = _repos.Value.GetPrinters().Select(p => new SelectListItem
                    {
                        Text = string.Format("{0}: {1}", p.Item1, p.Item2),
                        Value = p.Item1
                    })
                };
                var cookie = this.Request.Cookies[GlobalConstants.COOKIE_UCC_CCL_PRINTER];
                if (cookie != null)
                {
                    model.PrinterId = cookie.Value;
                }
            }
            else
            {
                AddStatusMessage(string.Format("Active Carton {0} does not exist", id));
                return RedirectToAction(MVC_Inquiry.Inquiry.Home.Index());
            }

            model.ProcessList = _repos.Value.GetCartonHistory(id).Select(p => new CartonProcessModel(p)).ToArray();
            var route = Url.RouteCollection[DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_EditCarton1];
            model.UrlEditCarton = route == null ? string.Empty : Url.RouteUrl(DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_EditCarton1, new
            {
                id = model.CartonId
            });

            route = Url.RouteCollection[DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_AbandonRework];
            model.UrlAbondonRework = route == null ? string.Empty : Url.RouteUrl(DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_AbandonRework);


            route = Url.RouteCollection[DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_Restock1];
            model.UrlRestock = route == null ? string.Empty : Url.RouteUrl(DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_Restock1, new
            {
                id = model.CartonId
            });

            route = Url.RouteCollection[DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_MarkReworkComplete];
            model.UrlMarkReworkComplete = route == null ? string.Empty : Url.RouteUrl(DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_MarkReworkComplete);

            route = Url.RouteCollection[DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_CartonToPallet];
            model.UrlCartonToPallet = route == null ? string.Empty : Url.RouteUrl(DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_CartonToPallet);

            route = Url.RouteCollection[DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_BulkUpdateCarton];
            model.UrlBulkUpdateCarton = route == null ? string.Empty : Url.RouteUrl(DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_BulkUpdateCarton);

            route = Url.RouteCollection[DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_CartonLocating];
            model.UrlCartonLocating = route == null ? string.Empty : Url.RouteUrl(DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_CartonLocating);

            return View(Views.Carton, model);
        }

        [HttpPost]
        [Route("print")]
        public virtual ActionResult PrintCartonTicket(string cartonId, string printerId)
        {

            try
            {
                _repos.Value.PrintCarton(cartonId, printerId);
                this.AddStatusMessage(string.Format("Carton Ticket for carton id {0} is printed on printer {1}.", cartonId, printerId));
                // Save the printer choice in a cookie
                var cookie = new HttpCookie(GlobalConstants.COOKIE_UCC_CCL_PRINTER, printerId)
                {
                    Expires = DateTime.Now.AddMonths(1)
                };
                this.Response.Cookies.Add(cookie);
            }
            catch (DbException ex)
            {
                ModelState.AddModelError("PrintException", ex.InnerException);
            }
            return RedirectToAction(Actions.Carton(cartonId));
        }

        [Route("excel/{id}")]
        public virtual ActionResult CartonExcel(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException("id");
            }
            var result = new ExcelResult("Carton_" + id);
            var processList = _repos.Value.GetCartonHistory(id).Select(p => new CartonProcessModel(p)).ToArray();
            result.AddWorkSheet(processList, "Carton History", "History of Carton " + id);
            return result;
        }


        /// <summary>
        /// Carton Open
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Route("open/{id}")]
        [SearchQuery(@"SELECT {0}, carton_id, 'Open Carton opened on ' || TO_CHAR(insert_date), NULL, NULL FROM <proxy />src_open_carton WHERE carton_id = :search_text",
            Group = "ctn", Rating = 20)]
        public virtual ActionResult CartonOpen(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return RedirectToAction(MVC_Inquiry.Inquiry.Home.Index());
            }
            var ctn = _repos.Value.GetOpenCarton(id);
            if (ctn == null)
            {
                AddStatusMessage(string.Format("Open Carton {0} does not exist", id));
                return RedirectToAction(MVC_Inquiry.Inquiry.Home.Index());
            }
            var model = new CartonOpenViewModel(ctn);
            //model.ModelTitle = string.Format("Opened Carton {0}", model.CartonId);
            model.ProcessList = _repos.Value.GetCartonHistory(id).Select(p => new CartonProcessModel(p)).ToArray();
            return View(Views.CartonOpen, model);
        }



        /// <summary>
        /// Carton intransit
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Route("itr/{id}")]
        [SearchQuery(@"SELECT {0}, carton_id, 'Intransit Carton of Shipment ' || shipment_id || ' dated ' || TO_CHAR(shipment_date), NULL, NULL FROM <proxy />src_carton_intransit WHERE carton_id = :search_text",
            Group = "ctn", Rating = 20)]
        public virtual ActionResult CartonIntransit(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return RedirectToAction(MVC_Inquiry.Inquiry.Home.Index());
            }
            var ctn = _repos.Value.GetIntransitCartonInfo(id);
            if (ctn == null)
            {
                AddStatusMessage(string.Format("Intransit Carton {0} does not exist", id));
                return RedirectToAction(MVC_Inquiry.Inquiry.Home.Index());
            }
            var model = new CartonIntransitViewModel(ctn);
            return View(Views.CartonIntransit, model);
        }

        [Route("pallet/{id}", Name = DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_SearchCartonPallet1)]
        [SearchQuery(@"SELECT {0}, pallet_id, 'Carton Pallet ' || pallet_id, NULL, NULL FROM <proxy />src_carton WHERE pallet_id= :search_text and rownum &lt; 2")]
        public virtual ActionResult CartonPallet(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return RedirectToAction(MVC_Inquiry.Inquiry.Home.Index());
            }
            var cartons = _repos.Value.GetCartonsOfPallet(id);
            if (cartons == null || cartons.Count == 0)
            {
                this.AddStatusMessage(string.Format("No info found for {0}", id));
                return RedirectToAction(MVC_Inquiry.Inquiry.Home.Index());
            }
            var model = new CartonPalletViewModel
            {
                PalletId = id,
                AllCartons = cartons.Select(p => new CartonHeadlineModel(p)).ToList(),
                TotalCartons = cartons.Count,
                TotalPieces = cartons.Sum(p => p.Pieces) ?? 0


            };
            var route = Url.RouteCollection[DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_BulkUpdateCarton];
            if (route != null)
            {
                model.UrlBulkUpdateCarton = Url.RouteUrl(DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_BulkUpdateCarton);
            }
            //model.DcmsLinks.Add(new DcmsLinkModel
            //{
            //    ShortDescription = "Bulk Update Carton",
            //    Url = route == null ? string.Empty : Url.RouteUrl(DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_BulkUpdateCarton)
            //});

            route = Url.RouteCollection[DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_CartonLocating1];
            if (route != null)
            {

                model.UrlCartonLocating = Url.RouteUrl(DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_CartonLocating1, new
                    {
                        id = model.PalletId
                    });
            }
            //model.DcmsLinks.Add(new DcmsLinkModel
            //{
            //    ShortDescription = "Carton Locating",
            //    Url = route == null ? string.Empty : Url.RouteUrl(DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_CartonLocating1, new
            //    {
            //        id = model.PalletId
            //    })
            //});

            route = Url.RouteCollection[DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_CartonToPallet];
            if (route != null)
            {
                model.UrlCartonToPallet = Url.RouteUrl(DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_CartonToPallet);
            }
            //model.DcmsLinks.Add(new DcmsLinkModel
            //{
            //    ShortDescription = "Carton To Pallet",
            //    Url = route == null ? string.Empty : Url.RouteUrl(DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_CartonToPallet)
            //});

            return View(Views.CartonPallet, model);
        }

        [Route("pallet/excel/{id}")]
        public virtual ActionResult CartonPalletExcel(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException("id");
            }
            var allCartons = _repos.Value.GetCartonsOfPallet(id).Select(p => new CartonHeadlineModel(p)).ToList();
            var result = new ExcelResult("CartonPallet_" + id);
            result.AddWorkSheet(allCartons, "Cartons", "List of Cartons on Pallet " + id);
            return result;
        }


        [Route("list")]
        public virtual ActionResult CartonList()
        {
            var cartons = _repos.Value.GetCartonsOfPallet(string.Empty, 200);

            var model = new CartonListViewModel
            {
                AllCartons = cartons.Select(p => new CartonHeadlineModel(p)).ToList()
            };
            return View(Views.CartonList, model);
        }

        [Route("pallet/list")]
        public virtual ActionResult PalletList()
        {
            var List = _repos.Value.GetPalletList(200);
            var model = new CartonPalletListViewModel
            {
                PalletList = List.Select(p => new PalletHeadLineModel
                {
                    PalletId = p.PalletId,
                    CartonAreaCount = p.CartonAreaCount,
                    AreaShortName = p.AreaShortName,
                    TotalCarton = p.TotalCarton,
                    MaxAreaChangeDate = p.MaxAreaChangeDate,
                    MinAreaChangeDate = p.MinAreaChangeDate,
                    WarehouseLocationId = p.WarehouseLocationId
                }).ToList()
            };
            return View(Views.CartonPalletList, model);
        }

    }
}