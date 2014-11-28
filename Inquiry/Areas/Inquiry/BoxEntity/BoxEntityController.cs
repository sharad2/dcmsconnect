using DcmsMobile.Inquiry.Areas.Inquiry.SharedViews;
using DcmsMobile.Inquiry.Helpers;
using EclipseLibrary.Mvc.Controllers;
using System;
using System.Data.Common;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DcmsMobile.Inquiry.Areas.Inquiry.BoxEntity
{
    [RoutePrefix("box")]
    public partial class BoxEntityController : InquiryControllerBase
    {
        private Lazy<BoxEntityRepository> _repos;
        protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        {
            base.Initialize(requestContext);
            _repos = new Lazy<BoxEntityRepository>(() => new BoxEntityRepository(requestContext.HttpContext.User.Identity.Name,
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

        #region box



        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="showPrintDialog">Pass showPrintDialog = true in query string to auto open the print dialog. BoxManager passes this parameter</param>
        /// <returns></returns>
        [Route("{id}", Name = DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_SearchUcc1)]
        [SearchQuery(@"SELECT {0}, UCC128_ID, 'Box ' || UCC128_ID, NULL, NULL FROM <proxy />box WHERE (UCC128_ID = :search_text or pro_number = :search_text) and rownum &lt; 2")]
        [SearchQuery(@"
select {0}, bd.ucc128_id, 'Electronic Product Code ' || bepc.epc, NULL, NULL
  from <proxy />boxdet_epc bepc
 inner join <proxy />boxdet bd
    on bd.boxdet_id = bepc.boxdet_id
WHERE bepc.epc = :search_text
")]
        public virtual ActionResult Box(string id, bool showPrintDialog = false)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return RedirectToAction(MVC_Inquiry.Inquiry.Home.Index());
            }
            var box = _repos.Value.GetBoxOfUcc(id);
            if (box == null)
            {
                this.AddStatusMessage(string.Format("No box found for UCC Id {0}", id));
                return RedirectToAction(MVC_Inquiry.Inquiry.Home.Index());
            }
            //var boxVas = _repos.Value.GetVasOnBox(id);
            var model = new BoxViewModel(box)
            {
                ShowPrintDialog = showPrintDialog,
                AuditList = _repos.Value.GetBoxProcesssHistory(id).Select(p => new BoxAuditModel(p)).ToArray(),
                PrinterList = _repos.Value.GetPrinters().Select(p => new SelectListItem
                {
                    Text = string.Format("{0}: {1}", p.Item1, p.Item2),
                    Value = p.Item1
                }).ToArray(),
                //ListOfCompleteVas = boxVas.ListOfCompleteVas,
                //ListOfIncompleteVas = boxVas.ListOfIncompleteVas,
                CanCancelBox = AuthorizeExAttribute.IsSuperUser(this.HttpContext) || this.HttpContext.User.IsInRole(GlobalConstants.ROLE_MANAGER),
                ManagerRoleName = GlobalConstants.ROLE_MANAGER
            };
            var route = Url.RouteCollection[DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_ApplyVasToBox1];
            if (route != null)
            {
                model.UrlManageVas = Url.RouteUrl(DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_ApplyVasToBox1, new
                    {
                        id = model.Ucc128Id
                    });
            }

            route = Url.RouteCollection[DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_Logon];
            if (route != null)
            {
                model.UrlLogin = Url.RouteUrl(DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_Logon, new
                    {
                        returnUrl = Url.Action(Actions.Box(id))
                    });
            }
            route = Url.RouteCollection[DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_ScanToPallet1];
            if (route != null)
            {
                model.UrlScanToPallet = Url.RouteUrl(DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_ScanToPallet1, new
                    {
                        id = model.Ucc128Id
                    });
            }

            var listBoxSku = _repos.Value.GetBoxSku(id, null);
            model.SkuWithEpc = listBoxSku.Select(p => new BoxSkuModel(p)).ToArray();
            var boxVasList = _repos.Value.GetVasOnBox(id);
            model.VasStatusList = boxVasList.Select(p => new BoxVasModel(p)).ToList();

            var pickers = listBoxSku.Select(p => p.MinPicker).Distinct();
            model.PickerNames = string.Join(", ", pickers);

            if (model.SkuWithEpc != null)
            {
                var allepc = _repos.Value.GetBoxEpc(id);
                foreach (var sku in model.SkuWithEpc)
                {
                    sku.AllEpc = allepc.Where(p => p.SkuId == sku.SkuId).Select(p => p.EpcCode).ToArray();
                }
            }

            var cookie = this.Request.Cookies[GlobalConstants.COOKIE_UCC_CCL_PRINTER];
            if (cookie != null)
            {
                model.PrinterId = cookie.Value;
            }
            return View(Views.ViewNames.Box, model);

        }

        [Route("excel/{id}")]
        public virtual ActionResult BoxExcel(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException("id");
            }
            var result = new ExcelResult("Box_" + id);
            result.AddWorkSheet(_repos.Value.GetBoxSku(id, null).Select(p => new BoxSkuModel(p)).ToArray(),
                "SKU", "List of SKU in Box " + id);

            result.AddWorkSheet(_repos.Value.GetBoxProcesssHistory(id).Select(p => new BoxAuditModel(p)).ToArray(),
                "Audit", "Audit of Box " + id);
            return result;
        }

        [HttpPost]
        [Route("print/lbl")]
        public virtual ActionResult PrintBoxUccOrCcl(string ucc128Id, string printerId, bool printCcl = false, bool printUcc = false, bool printCatalog = false)
        {
            if (string.IsNullOrWhiteSpace(ucc128Id))
            {
                // Defensive check
                ModelState.AddModelError("", "Unexpected error. Please try again.");
                return RedirectToAction(MVC_Inquiry.Inquiry.Home.Index());
            }
            if (!ModelState.IsValid)
            {
                return RedirectToAction(Actions.Box(ucc128Id));
            }
            //To Print Labels
            try
            {
                if (printCcl)
                {
                    _repos.Value.PrintCCL(ucc128Id, printerId);
                    AddStatusMessage(string.Format("CCL label is printed for box {0} ", ucc128Id));
                }
                if (printUcc)
                {
                    _repos.Value.PrintUCC(ucc128Id, printerId);
                    AddStatusMessage(string.Format("UCC label is printed for box {0}  ", ucc128Id));
                }
                //To Print Label for Catalog.
                if (printCatalog)
                {
                    var noOfSku = _repos.Value.PrintCatalog(ucc128Id, printerId);
                    AddStatusMessage(string.Format("{0} Catalog Labels for box {1} Printed.", noOfSku, ucc128Id));
                }
                // Save the printer choice in a cookie
                var cookie = new HttpCookie(GlobalConstants.COOKIE_UCC_CCL_PRINTER, printerId)
                {
                    Expires = DateTime.Now.AddMonths(1)
                };

                this.Response.Cookies.Add(cookie);
            }
            catch (DbException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            return RedirectToAction(Actions.Box(ucc128Id));
        }

        [HttpPost]
        [AuthorizeEx("Cancelling a box requires Role {0}", Roles = GlobalConstants.ROLE_MANAGER)]
        [Route("Cancel/{id}")]
        public virtual ActionResult CancelBox(string id)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(Actions.Box(id));
            }
            try
            {
                _repos.Value.CancelBox(id);
            }
            catch (DbException exception)
            {
                this.ModelState.AddModelError("", exception.InnerException);
            }
            return RedirectToAction(Actions.Box(id));
        }

        #endregion

        #region BoxPallet
        [Route("pallet/{id}", Name = DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_SearchBoxPallet1)]
        [SearchQuery(@"SELECT {0}, pallet_id, 'Box Pallet' || pallet_id, NULL, NULL FROM <proxy />box WHERE pallet_id= :search_text and rownum &lt; 2")]
        public virtual ActionResult BoxPallet(string id, bool showPrintDialog = false)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return RedirectToAction(MVC_Inquiry.Inquiry.Home.Index());
            }
            int MAX_BOXES = 200;
            var boxes = _repos.Value.GetBoxesOfPallet(id, MAX_BOXES);
            if (boxes.Count == 0)
            {
                this.AddStatusMessage("No information found for pallet");
                return RedirectToAction(MVC_Inquiry.Inquiry.Home.Index());
            }
            var box = boxes.First();

            var model = new BoxPalletViewModel
            {
                CclPrintedBoxes = boxes.Count(p => p.LastCclPrintedDate.HasValue),
                //TotalBoxes = boxes.Count(),
                PickedBoxes = boxes.Count(p => p.PitchingEndDate.HasValue),
                Area = box.IaId,
                ShortName = box.ShortName,

                //CustomerId = box.CustomerId,
                //CustomerName = box.CustomerName,
                PalletId = box.PalletId,
                UccPrintedBoxes = boxes.Count(p => p.LastUccPrintedDate.HasValue),
                ShowPrintDialog = showPrintDialog,
                PalletHistory = _repos.Value.GetBoxPalletHistory(id).Select(p => new BoxPalletHistoryModel(p)).ToArray(),
                AllBoxes = boxes.Select(p => new BoxHeadlineModel(p)).ToList(),
                PrinterList = _repos.Value.GetPrinters().Select(p => new SelectListItem
                {
                    Text = string.Format("{0}: {1}", p.Item1, p.Item2),
                    Value = p.Item1
                }),
                AllSku = _repos.Value.GetBoxSku(null, id).Select(p => new BoxSkuModel(p))
                    .OrderBy(p => p.Style)
                    .ThenBy(p => p.Color)
                    .ThenBy(p => p.Dimension)
                    .ThenBy(p => p.SkuSize)
                    .ToList(),
                Building = box.Building
            };
            if (box != null && box.TotalBoxes != model.AllBoxes.Count)
            {
                AddStatusMessage(string.Format("Pallet contains {0:N0} boxes, but only {1:N0} are being displayed", box.TotalBoxes, model.TotalBoxes));
            }
            var cookie = this.Request.Cookies[GlobalConstants.COOKIE_UCC_CCL_PRINTER];
            if (cookie != null)
            {
                model.PrinterId = cookie.Value;
            }
            var route = Url.RouteCollection[DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_PalletLocating];

            model.DcmsLinks.Add(new DcmsLinkModel
            {
                ShortDescription = "Locate Pallet",
                Url = route == null ? string.Empty : Url.RouteUrl(DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_PalletLocating)
            });


            route = Url.RouteCollection[DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_MoveBoxPallet1];
            model.DcmsLinks.Add(new DcmsLinkModel
            {
                ShortDescription = "Move Pallet",
                Url = route == null ? string.Empty : Url.RouteUrl(DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_MoveBoxPallet1, new
                {
                    id = model.PalletId
                })
            });
            return View(Views.BoxPallet, model);
        }

        [HttpPost]
        [Route("pallet/print")]
        public virtual ActionResult PrintBoxesOfPallet(string palletId, string printerId, bool palletSummary = false, bool printedBoxes = false, bool unprintedBoxes = false)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(Actions.BoxPallet(palletId));
            }
            if (!palletSummary && !printedBoxes && !unprintedBoxes)
            {
                AddStatusMessage("Please select label type to print.");
                return RedirectToAction(Actions.BoxPallet(palletId));
            }
            //if (printBoxes)
            //{
            //    if (printAllBoxes == false && totalBoxes == uccPrintedBoxes && totalBoxes == cclPrintedBoxes)
            //    {
            //        AddStatusMessage("Pallet does not have any non printed box.");
            //        return RedirectToAction(Actions.BoxPallet(palletToPrint));
            //    }
            //}
            try
            {
                _repos.Value.PrintBoxesOfPallet(palletId, printerId, 1, printedBoxes && unprintedBoxes,
                                          palletSummary, printedBoxes || unprintedBoxes);
                AddStatusMessage(string.Format("Pallet {0} printed on printer {1}", palletId, printerId));
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
            return RedirectToAction(Actions.BoxPallet(palletId));
        }

        [Route("pallet/excel/{id}")]
        public virtual ActionResult BoxPalletExcel(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException("id");
            }
            var result = new ExcelResult("BoxPallet_" + id);
            var boxes = _repos.Value.GetBoxesOfPallet(id, GlobalConstants.MAX_EXCEL_ROWS);
            result.AddWorkSheet(boxes.Select(p => new BoxHeadlineModel(p)).ToList(), "Boxes", "List of Boxes on Pallet " + id);
            var allSku = _repos.Value.GetBoxSku(null, id).Select(p => new BoxSkuModel(p))
                        .OrderBy(p => p.Style)
                        .ThenBy(p => p.Color)
                        .ThenBy(p => p.Dimension)
                        .ThenBy(p => p.SkuSize)
                        .ToArray();
            result.AddWorkSheet(allSku, "SKU", "List of SKUs on Pallet " + id);
            var history = _repos.Value.GetBoxPalletHistory(id).Select(p => new BoxPalletHistoryModel(p)).ToArray();
            result.AddWorkSheet(history, "History", "Audit Entries for Pallet " + id);
            return result;
        }

        #endregion

        [Route("boxlist")]
        public virtual ActionResult BoxList()
        {

            var boxes = _repos.Value.GetRecentPitchedBoxList(200);
            var model = new BoxListViewModel()
            {
                AllBoxes = boxes.Select(p => new BoxHeadlineModel(p)).ToList()

            };

            return View(Views.BoxList, model);
        }
        [Route("pallet/list")]
        public virtual ActionResult BoxPalletList()
        {
            var List = _repos.Value.GetBoxPalletList(200);
            var model = new BoxPalletListViewModel
            {
                BoxPalletList = List.Select(p => new BoxPalletHeadLineModel
                              {

                                  PalletId = p.PalletId,
                                  CustomerId=p.CustomerId,
                                  CustomerName=p.CustomerName,
                                  AreaShortName=p.AreaShortName,
                                  BoxCount=p.BoxCount,
                                  BoxAreaCount=p.BoxAreaCount,
                                  WarehouseLocationId=p.WarehouseLocationId
                              }).ToList()
            };
            return View(Views.BoxPalletList,model);
        }
    }
}