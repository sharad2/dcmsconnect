using DcmsMobile.Inquiry.Helpers;
using System;
using System.Linq;
using System.Web.Mvc;

namespace DcmsMobile.Inquiry.Areas.Inquiry.ReturnEntity
{
    [RoutePrefix("ret")]
    public partial class ReturnEntityController : InquiryControllerBase
    {
        private Lazy<ReturnEntityRepository> _repos;
        protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        {
            base.Initialize(requestContext);
            _repos = new Lazy<ReturnEntityRepository>(() => new ReturnEntityRepository(requestContext.HttpContext.User.Identity.Name,
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


        [Route("{id}", Name = DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_SearchReturnNumber1)]
        [SearchQuery(@"select {0}, ret.returns_authorization_number, 'Return ' || ret.returns_authorization_number,Null,Null from <proxy />dem_returns ret where ret.returns_authorization_number = :search_text and ROWNUM &lt; 2")]
        public virtual ActionResult Return(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return RedirectToAction(MVC_Inquiry.Inquiry.Home.Index());
            }
            int MAX_ROWS = 500;
            var model = new ReturnViewModel
            {
                ReturnAuthorizationNumber = id,
                ReturnRecipts = _repos.Value.GetReturnInfo(id, MAX_ROWS).Select(p => new ReturnReceiptModel(p)).ToArray()
            };

            if (model.ReturnRecipts.Count == 0)
            {
                this.AddStatusMessage(string.Format("No Return found with number {0}", id));
                return RedirectToAction(MVC_Inquiry.Inquiry.Home.Index());
            }
            return View(this.Views.Return, model);
        }

        [Route("excel/{id}")]
        public virtual ActionResult ReturnExcel(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException("id");
            }
            var returnRecipts = _repos.Value.GetReturnInfo(id, GlobalConstants.MAX_EXCEL_ROWS).Select(p => new ReturnReceiptModel(p)).ToArray();
            var result = new ExcelResult("Return_Receipts " + id);
            result.AddWorkSheet(returnRecipts, "Return Receipt", "List of Return Receipt " + id);
            return result;
        }

        // select receipt_number from dem_returns
        [Route("receipt/{id}")]
        [SearchQuery(@"select {0}, receipt_number, 'Return Receipt' || receipt_number, Null, Null from <proxy />dem_returns where receipt_number = :search_text")]
        public virtual ActionResult ReturnReceipt(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return RedirectToAction(MVC_Inquiry.Inquiry.Home.Index());
            }
            var receiptInfo = _repos.Value.GetReturnReceiptInfo(id);
            if (receiptInfo == null)
            {
                this.AddStatusMessage(string.Format("No Return Receipt with number {0} found ", id));
                return RedirectToAction(MVC_Inquiry.Inquiry.Home.Index());
            }

            var receiptHeader = receiptInfo.FirstOrDefault();

            var model = new ReturnReceiptViewModel
            {
                ReturnNumber = receiptHeader.ReturnNumber,
                ActivityId = receiptHeader.ActivityId,
                CarrierDescription = receiptHeader.CarrierDescription,
                CarrierId = receiptHeader.CarrierId,
                CustomerId = receiptHeader.CustomerId,
                CustomerName = receiptHeader.CustomerName,
                DMNumber = receiptHeader.DMNumber,
                InsertedBy = receiptHeader.InsertedBy,
                InsertDate = receiptHeader.InsertDate,
                ModifiedBy = receiptHeader.ModifiedBy,
                ModifiedDate = receiptHeader.ModifiedDate,
                ReceiptNumber = receiptHeader.ReceiptNumber,
                ReceivedDate = receiptHeader.ReceivedDate,
                CustomerStoreId = receiptHeader.CustomerStoreId,
                DmDate = receiptHeader.DmDate,
                ReasonCode = receiptHeader.ReasonCode,
                ReasonDescription = receiptHeader.ReasonDescription,
                ExpectedPieces = receiptHeader.ExpectedPieces,
                ReturnSku = receiptInfo.Where(p => !string.IsNullOrWhiteSpace(p.Upc)).Select(p => new ReturnSkuModel
                {
                    Upc = p.Upc,
                    Style = p.Style,
                    Color = p.Color,
                    Dimension = p.Dimension,
                    SkuSize = p.SkuSize,
                    RetailPrice = (p.RetailPrice * p.Quantity),
                    Pieces = p.Quantity,
                    SkuId = p.SkuId ?? 0
                }).ToList()
            };
            return View(this.Views.ReturnReceipt, model);
        }

        [Route("receipt/excel/{id}")]
        public virtual ActionResult ReturnReceiptExcel(string id)
        {
            var returnSku = _repos.Value.GetReturnReceiptInfo(id).Where(p => !string.IsNullOrWhiteSpace(p.Upc)).Select(p => new ReturnSkuModel
            {
                Upc = p.Upc,
                Style = p.Style,
                Color = p.Color,
                Dimension = p.Dimension,
                SkuSize = p.SkuSize,
                RetailPrice = (p.RetailPrice * p.Quantity),
                Pieces = p.Quantity
            }).ToList();
            var result = new ExcelResult("ReturnReceipt_" + id);
            result.AddWorkSheet(returnSku, "SKU", "List of KU in Return Receipt " + id);
            return result;
        }

        [Route("list")]
        public virtual ActionResult ReturnReceiptList()
        {
            var list = _repos.Value.GetReturnList();
            var model = new ReturnReceiptListViewModel
                {
                    ReturnReceiptList = list.Select(p => new ReturnReceiptHeadlineModel
                    {
                        ReturnNumber = p.ReturnNumber,
                         CustomerId=p.CustomerId,
                         CustomerName=p.CustomerName,
                        NoOfCartons = p.NoOfCartons,
                        CustomerCount = p.CustomerCount,
                        TotalReceipts = p.TotalReceipts,
                        ReceivedDate = p.ReceivedDate,
                        ExpectedPieces = p.ExpectedPieces
                    }).ToList()

                };
            return View(Views.ReturnList, model);
        }
    }
}