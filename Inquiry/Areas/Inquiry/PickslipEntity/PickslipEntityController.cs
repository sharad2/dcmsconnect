using DcmsMobile.Inquiry.Areas.Inquiry.SharedViews;
using DcmsMobile.Inquiry.Helpers;
using System;
using System.Data.Common;
using System.Linq;
using System.Web;
using System.Web.Mvc;
namespace DcmsMobile.Inquiry.Areas.Inquiry.PickslipEntity
{
    public partial class PickslipEntityController : InquiryControllerBase
    {
        private Lazy<PickslipEntityRepository> _repos;
        protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        {
            base.Initialize(requestContext);
            _repos = new Lazy<PickslipEntityRepository>(() => new PickslipEntityRepository(requestContext.HttpContext.User.Identity.Name,
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

        [Route("ps/{id:long}", Name = DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_SearchPickslip1)]
        [SearchQuery(@"SELECT {0}, TO_CHAR(ps.pickslip_id), 'Pickslip', NULL, NULL FROM <proxy />ps WHERE ps.pickslip_id = :int_value", Group = "ps", Rating = 10)]
        public virtual ActionResult Pickslip(long? id)
        {
            if (id == null)
            {
                return RedirectToAction(MVC_Inquiry.Inquiry.Home.Index());
            }
            int MAX_BOXES = 1000;
            var pickslip = _repos.Value.GetActivePickslip(id.Value);
            if (pickslip == null)
            {
                this.AddStatusMessage(string.Format("No info found for pickslip {0}", id));
                return RedirectToAction(MVC_Inquiry.Inquiry.Home.Index());
            }
            var boxes = _repos.Value.GetBoxes(id.Value, MAX_BOXES);
            var model = new PickslipViewModel(pickslip)
            {
                AllSku = _repos.Value.GetPickslipSku(id.Value).Select(p => new PickslipSkuModel(p)).ToArray(),
                AllBoxes = boxes.Select(p => new BoxHeadlineModel(p)).ToList(),
                PrinterList = _repos.Value.GetPackingSlipPrinters().Select(p => new SelectListItem
                {
                    Text = string.Format("{0} : {1}", p.Item1, p.Item2),
                    Value = p.Item1
                })
            };
            var box = boxes.FirstOrDefault();
            if (box != null)
            {
                model.TotalBoxes = box.TotalBoxes;
            }
            model.ModelTitle = string.Format("Pickslip {0}", id);
            var cookie = this.Request.Cookies[GlobalConstants.COOKIE_PACKING_PRINTER];
            if (cookie != null)
            {
                model.PrinterId = cookie.Value;
            }
            return View(Views.Pickslip, model);
        }

        [HttpPost]
        [Route("print/ps")]
        public virtual ActionResult PrintPackingSlip(long pickslipId, string printerid, int numberOfCopies, bool printMasterPackingslip = false, bool printPackingSlip = false)
        {
            if (printPackingSlip == false && printMasterPackingslip == false)
            {
                this.ModelState.AddModelError("", "Please choose something to print");
            }
            if (numberOfCopies > 99)
            {
                this.ModelState.AddModelError("", "Maximum 99 copies can be printed");
            }
            try
            {
                _repos.Value.PrintPackingSlip(pickslipId, printMasterPackingslip, printPackingSlip, true, printerid, numberOfCopies);
                this.AddStatusMessage(string.Format("Printing has been done successfully."));
                var cookie = new HttpCookie(GlobalConstants.COOKIE_PACKING_PRINTER, printerid)
                {
                    Expires = DateTime.Now.AddMonths(1)
                };
                this.Response.Cookies.Add(cookie);
            }
            catch (DbException ex)
            {
                this.ModelState.AddModelError("PrintException", ex.InnerException);
            }
            return RedirectToAction(Actions.Pickslip(pickslipId));
        }
        [Route("ps/excel/{id}")]
        public virtual ActionResult PickslipExcel(long id)
        {
            var result = new ExcelResult("Pickslip_" + id);
            result.AddWorkSheet(_repos.Value.GetBoxes(id, GlobalConstants.MAX_EXCEL_ROWS).Select(p => new BoxHeadlineModel(p)).ToList(), "Boxes", "List of boxes of Pickslip " + id);
            result.AddWorkSheet(_repos.Value.GetPickslipSku(id).Select(p => new PickslipSkuModel(p)).ToArray(), "SKUs", "List of SKUs in Pickslip " + id);
            return result;
        }

        [Route("psimp/{id:long}")]
        [SearchQuery(@"SELECT {0}, TO_CHAR(p.pickslip_id), 'In Order Bucket Pickslip', NULL, NULL FROM <proxy />dem_pickslip p WHERE p.pickslip_id = :int_value AND p.ps_status_id = 1",
            Group = "ps", Rating = 2)]
        public virtual ActionResult PickslipImported(long? id)
        {
            if (id == null)
            {
                return RedirectToAction(MVC_Inquiry.Inquiry.Home.Index());
            }
            // Status 1 in dem_pickslip.
            var pickslip = _repos.Value.GetInOrderBucketPickslip(id.Value);
            if (pickslip == null)
            {
                this.AddStatusMessage(string.Format("No info found for In_Order bucket pickslip {0}. If you worked on this pickslip after the last scan, please scan again.", id));
                return RedirectToAction(MVC_Inquiry.Inquiry.Home.Index());
            }
            var model = new PickslipViewModel(pickslip)
            {
                AllSku = _repos.Value.GetSkuOfTransferPickslip(id.Value).Select(p => new PickslipSkuModel(p)).ToArray(),
                AllBoxes = new BoxHeadlineModel[0]
            };
            model.ModelTitle = string.Format("In Order Bucket Pickslip {0}", id);
            return View(Views.PickslipImported, model);
        }

        [Route("poimp/excel/{id}/{pk1}")]
        public virtual ActionResult PoImportedExcel(string id, string pk1)
        {
            var result = new ExcelResult("PO_" + id);
            result.AddWorkSheet(_repos.Value.GetInOrderBucketPickslipsOfPo(id, pk1).Select(p => new PickslipHeadlineModel(p)).ToList(), "Pickslips", "List of Pickslips in PO " + id);
            return result;
        }

        [Route("psimp/excel/{id:long}")]
        public virtual ActionResult PickslipImportedExcel(long id)
        {
            var result = new ExcelResult("Pickslip_" + id);
            result.AddWorkSheet(_repos.Value.GetSkuOfTransferPickslip(id).Select(p => new PickslipSkuModel(p)).ToArray(), "SKUs", "List od SKUs in Pickslip " + id);
            return result;
        }

        /// <summary>
        /// This function looks up dem_pickslip for the PO
        /// </summary>
        /// <param name="id"></param>
        /// <param name="pk1"></param>
        /// <returns></returns>
        [Route("po/{id}/{pk1}")]
        [SearchQuery(@"
SELECT {0}, t.customer_order_id,
                           'In Order Bucket PO starting ' || TO_CHAR(MAX(t.DELIVERY_DATE)) ||
                           ' for Customer ' || t.customer_id,
                           t.customer_id, NULL
                      FROM <proxy />dem_pickslip t
                     WHERE t.customer_order_id = :search_text AND t.ps_status_id = 1
                     group by t.customer_order_id,t.customer_id
", Group = "po", Rating = 1)]
        public virtual ActionResult ImportedPo(string id, string pk1)
        {
            const int MAX_PICKSLIPS = 1000;
            var pickslips = _repos.Value.GetInOrderBucketPickslipsOfPo(id, pk1);
            if (pickslips.Count == 0)
            {
                this.AddStatusMessage(string.Format("No info found for PO: {0}; Customer: {1}", id, pk1));
                return RedirectToAction(MVC_Inquiry.Inquiry.Home.Index());
            }
            var po = (from ps in pickslips
                      select new PurchaseOrder
                      {
                          CancelDate = ps.CancelDate,
                          CustomerId = ps.CustomerId,
                          CustomerName = ps.CustomerName,
                          DcCancelDate = ps.DcCancelDate,
                          PoId = ps.PoId,
                          StartDate = ps.StartDate,
                      }).First();
            var model = new PoViewModel(po)
            {
                AllPickslips = pickslips.Select(p => new PickslipHeadlineModel(p)).ToList(),
                ModelTitle = string.Format("In Order Bucket PO {0}", id)
            };
            model.PoId = id;
            if (model.AllPickslips.Count == MAX_PICKSLIPS)
            {
                model.PickslipLimit = MAX_PICKSLIPS;
            }
            //model.ShowInventoryStatus = false;
            return View(Views.POImported, model);
        }

        [Route("po/{id}/{pk1}/{pk2:int}", Name = DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_SearchPo3)]
        [SearchQuery(@"
SELECT {0}, t.po_id,
        'PO Iteration ' || to_char(t.iteration) || ' started ' || TO_CHAR(t.start_date) ||' for Customer '|| t.customer_id,
        t.customer_id, to_char(t.iteration)
FROM <proxy />po t 
WHERE t.po_id= :search_text
", Group = "po", Rating = 5)]
        public virtual ActionResult Po(string id, string pk1, int pk2)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return RedirectToAction(MVC_Inquiry.Inquiry.Home.Index());
            }
            const int MAX_PICKSLIPS = 1000;
            var po = _repos.Value.GetPo(id, pk1, pk2);
            if (po == null)
            {
                this.AddStatusMessage("No info found for PO " + id + " Customer " + pk1 + " iteration " + pk2);
                return RedirectToAction(MVC_Inquiry.Inquiry.Home.Index());
            }
            var model = new PoViewModel(_repos.Value.GetPo(id, pk1, pk2))
            {
                AllPickslips = _repos.Value.GetPickslips(id, pk1, pk2, MAX_PICKSLIPS).Select(p => new PickslipHeadlineModel(p)).ToList(),
                ModelTitle = string.Format("PO {0}", id)
            };
            //if (model.AllPickslips.Count == 0)
            //{
            //    this.AddStatusMessage(string.Format("No info found for PO: {0}; Customer: {1}; Iteration: {2}", id, pk1, pk2));
            //    return RedirectToAction(MVC_Inquiry.Inquiry.Home.Index());
            //}
            if (model.AllPickslips.Count == MAX_PICKSLIPS)
            {
                model.PickslipLimit = MAX_PICKSLIPS;
            }
            //model.ShowInventoryStatus = true;
            return View(Views.PO, model);
        }

        [Route("po/excel/{id}/{pk1}/{pk2:int}")]
        public virtual ActionResult PoExcel(string id, string pk1, int? pk2)
        {
            var result = new ExcelResult("PO_" + id);
            result.AddWorkSheet(_repos.Value.GetPickslips(id, pk1, pk2, GlobalConstants.MAX_EXCEL_ROWS).Select(p => new PickslipHeadlineModel(p)).ToList(), "Pickslips", "List of Pickslips in PO " + id);
            return result;
        }

        [Route("wave/{id:int}", Name = DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_SearchWave1)]
        [SearchQuery(@"Select {0}, TO_CHAR(buc.bucket_id), 'Pick Wave ' || TO_CHAR(buc.bucket_id), NULL, NULL FROM <proxy />bucket buc WHERE buc.bucket_id = :int_value")]
        public virtual ActionResult Wave(int? id)
        {
            if (id == null)
            {
                return RedirectToAction(MVC_Inquiry.Inquiry.Home.Index());
            }
            var wave = _repos.Value.GetWaveInfo(id.Value);
            if (wave == null)
            {
                this.AddStatusMessage(string.Format("No info found for {0}", id));
                return RedirectToAction(MVC_Inquiry.Inquiry.Home.Index());
            }
            var model = new WaveViewModel
            {
                AssignedSkuCount = wave.AssignedSkuCount,
                AvailableForPitching = wave.AvailableForPitching,
                BucketId = wave.BucketId,
                BucketName = wave.BucketName,
                BuildingPullFrom = wave.BuildingPullFrom,
                CreatedBy = wave.CreatedBy,
                CustomerId = wave.CustomerId,
                CustomerName = wave.CustomerName,
                DateCreated = wave.DateCreated,
                DcCancelDate = wave.DcCancelDate,
                ExportFlag = wave.ExportFlag,
                Freeze = wave.Freeze,
                PickedPieces = wave.PickedPieces,
                PickMode = wave.PickMode,
                PickslipCount = wave.PickslipCount,
                PitchArea = wave.PitchArea,
                PitchAreaDescription = wave.PitchAreaDescription,
                PitchBuilding = wave.PitchBuilding,
                PoCount = wave.PoCount,
                Priority = wave.Priority,
                PullArea = wave.PullArea,
                PullAreaDescription = wave.PullAreaDescription,
                PullToDock = wave.PullToDock,
                TotalBoxes = wave.TotalBoxes,
                TotalQuantityOrdered = wave.TotalQuantityOrdered,
                TotalSkuCount = wave.TotalSkuCount,
                UnavailableBoxCount = wave.UnavailableBoxCount,
                VerifiedBoxes = wave.VerifiedBoxes,
                BucketStatus = wave.BucketStatus
            };
            var route = Url.RouteCollection[DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_BoxPick];
            if (route != null)
            {
                model.UrlPullBoxes = Url.RouteUrl(DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_BoxPick);
            }
            route = Url.RouteCollection[DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_ManagePickWave1];
            if (route != null)
            {
                model.UrlManagePickwave = Url.RouteUrl(DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_ManagePickWave1, new
                {
                    id = id
                });
            }
            return View(Views.Wave, model);
        }

        [Route("pslist")]
        public virtual ActionResult PickslipList()
        {
            var pickslipList = new PickslipListViewModel
            {
                AllPickslips = _repos.Value.GetPickslips(string.Empty, string.Empty, null, 200).Select(p => new PickslipHeadlineModel(p)).ToList()
            };
            //pickslipList.ShowInventoryStatus = true;
            return View(Views.PickslipList, pickslipList);
        }

        [Route("wavelist")]
        public virtual ActionResult WaveList()
        {
            var list = _repos.Value.GetWaveList(200);
            var model = new WaveListViewModel
            {
                WaveList = list.Select(p => new WaveHeadlineModel
                {
                    PitchBuilding=p.PitchBuilding,
                    PitchArea=p.PitchArea,
                    PitchAreaDescription=p.PitchAreaDescription,
                    BuildingPullFrom=p.BuildingPullFrom,
                    PullArea=p.PullArea,
                    PullAreaDescription=p.PullAreaDescription,
                    BucketName = p.BucketName,
                    BucketId = p.BucketId,
                    Priority = p.Priority,
                    BucketStatus = p.BucketStatus,
                    CustomerId = p.CustomerId,
                    CustomerName = p.CustomerName,
                    DateCreated = p.DateCreated,
                    Freeze = p.Freeze,
                    PickslipCount = p.PickslipCount,
                    PoCount = p.PoCount
                }).ToList()
            };
            return View(Views.WaveList, model);
        }

        [Route("polist")]
        public virtual ActionResult PoList()
        {
            //var orders = new RecentPoListViewModel
            //{
            //    PoList = _repos.Value.GetRecentOrders(200).Select(p => new RecentPoModel(p)).ToArray()
            //};
            return View(Views.PoList);
        }

        /// <summary>
        /// This is a child action called by PoList View. ChildActionOnly attribute guarantees that only views will be able call this action
        /// </summary>
        /// <returns></returns>
        [Route("orders")]
        [ChildActionOnly]
        public virtual ActionResult GetRecentOrders()
        {
            var orders = new RecentPoListViewModel
            {
                PoList = _repos.Value.GetRecentOrders(200).Select(p => new RecentPoModel(p)).ToList(),
               
            };
            orders.ShowCustomerFlag = true;
            return PartialView(MVC_Inquiry.Inquiry.SharedViews.Views._recentPoListPartial, orders);
        }
    }
}