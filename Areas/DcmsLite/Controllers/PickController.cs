using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DcmsMobile.DcmsLite.Repository.Pick;
using DcmsMobile.DcmsLite.ViewModels.Pick;
using EclipseLibrary.Mvc.Controllers;

namespace DcmsMobile.DcmsLite.Areas.DcmsLite.Controllers
{
    public partial class PickController : DcmsLiteControllerBase<PickService>
    {
        private BoxModel Map(Box src)
        {
            return new BoxModel
            {
                UccId = src.UccId,
                BoxId = src.BoxId,
                LastUccPrintDate = src.LastUccPrintDate
            };
        }

        private PrintBatchModel Map(PrintBatch src)
        {
            return new PrintBatchModel
                {
                    BatchId = src.BatchId,
                    LastUccPrintDate = src.LastUccPrintDate,
                    PrintedBy = src.PrintedBy,
                    TotalBoxes = src.CountBoxes,
                    BucketId = src.BucketId,
                    CountUnprintedBoxes = src.CountUnprintedBoxes
                };
        }

        public virtual ActionResult Index(string customerId)
        {
            var bucketListForPrinting = _service.GetBucketList(_buildingId, customerId);
            if (bucketListForPrinting == null || !bucketListForPrinting.Any())
            {
                this.AddStatusMessage("No bucket is available for printing.");
            }

            var query = from item in bucketListForPrinting
                        orderby item.VwhId, item.CustomerId,  //Showing unprinted buckets first
                                item.TotalBoxes == 0 ? int.MaxValue : item.TotalBoxes - item.CountPrintedBoxes descending,
                        item.CreationDate descending, item.BucketId descending, item.PoId, item.CustomerDcId
                        group item by item.VwhId into g
                        select g;

            var model = new IndexViewModel();
            foreach (var item in query)
            {
                model.BucketListForPrinting.Add(new KeyValuePair<string, BucketModel[]>(item.Key,
                    (from bucket in item
                     select new BucketModel
                     {
                         BucketId = bucket.BucketId,
                         BucketName = bucket.BucketName,
                         CountOfUccPrintedBoxes = bucket.CountPrintedBoxes,
                         TotalBoxes = bucket.TotalBoxes,
                         TotalPickslips = bucket.TotalPickslips,
                         LastPrintedBy = bucket.LastPrintedBy,
                         CreationDate = bucket.CreationDate,
                         MinPrintedOn = bucket.MinPrintedOn == null ? (DateTimeOffset?)null : bucket.MinPrintedOn.Value.Date,
                         MaxPrintedOn = bucket.MaxPrintedOn == null ? (DateTimeOffset?)null : bucket.MaxPrintedOn.Value.Date,
                         CustomerId = bucket.CustomerId,
                         Customer = string.Format("{0}: {1}", bucket.CustomerId, bucket.CustomerName),
                         VwhId = bucket.VwhId,
                         PoId = bucket.PoId,
                         IsFrozen = bucket.IsFrozen,
                         CustomerDcId = bucket.CustomerDcId,
                         CountPrintedBy = bucket.CountPrintedBy
                     }).ToArray()));
            }
            model.CustomerId = customerId;
            var cookie = Request.Cookies[COOKIE_SELECTED_VWH];
            model.VwhId = cookie != null ? cookie.Value : null;

            return View(Views.Index, model);
        }

        /// <summary>
        /// Each selected printer is stored as a sub key of this cookie. It is read by Action Wave() and written by PrintNewBatch()
        /// </summary>
        private const string COOKIE_SELECTED_PRINTERS = "DcmsLite_SelectedPrinters";
        private const string COOKIE_SELECTED_BATCH_SIZE = "DcmsLite_SelectedBatchSize";
        private const string COOKIE_SELECTED_VWH = "DcmsLite_SelectedVWh";

        public virtual ActionResult Wave(int bucketId)
        {
            var bucket = _service.GetBucket(bucketId);
            if (bucket == null)
            {
                ModelState.AddModelError("bucketId", "Unrecognized Bucket.");
                return RedirectToAction(Actions.Index());
            }

            #region Storing Selected Vwh for activating the same tab next time

            var cookie = Request.Cookies[COOKIE_SELECTED_VWH];

            if (cookie == null || cookie.Value != bucket.VwhId)
            {
                cookie = new HttpCookie(COOKIE_SELECTED_VWH, bucket.VwhId);
            }
            cookie.Expires = DateTime.Now.AddMinutes(60); // remember for one hour sliding
            this.Response.Cookies.Add(cookie);

            #endregion


            cookie = Request.Cookies[COOKIE_SELECTED_PRINTERS];

            var model = new WaveViewModel
                {
                    BucketId = bucketId,
                    IsFrozen = bucket.IsFrozen,
                    PrinterList = (from printer in _service.GetPrinterList(_buildingId)
                                   select new SelectListItem
                                       {
                                           Text = printer.Description,
                                           Value = printer.PrinterName,
                                           Selected = cookie != null && cookie.Values.AllKeys.Contains(printer.PrinterName)
                                       }).ToArray(),
                    PrintedBoxCount = bucket.CountPrintedBoxes,
                    TotalBoxes = bucket.TotalBoxes,
                    BoxesNotInBatch = bucket.BoxesNotInBatch,
                    PrintedBatchList = _service.GetBatches(bucketId).Select(Map).ToArray(),
                    BatchSize = Request.Cookies[COOKIE_SELECTED_BATCH_SIZE] == null ? (int?)null : int.Parse(Request.Cookies[COOKIE_SELECTED_BATCH_SIZE].Value)
                };
            return View(Views.Wave, model);
        }

        /// <summary>
        /// Prints a new batch for the passed bucket
        /// </summary>
        /// <param name="bucketId"></param>
        /// <param name="printer"></param>
        /// <param name="batchSize"></param>
        /// <param name="clearCookie">If true, then the current printer cookie is cleared. The passed printer is always added to the cookie</param>
        /// <returns></returns>
        [HttpPost]
        [AuthorizeEx("Print batch requires role {0}", Roles = ROLE_DCMS_LITE)]
        public virtual ActionResult PrintNewBatch(int? bucketId, string printer, int? batchSize, bool clearCookie = false)
        {
            if (bucketId == null)
            {
                throw new ArgumentNullException("bucketId", "Bucket cannot be null");
            }
            if (batchSize == null)
            {
                throw new ArgumentNullException("batchSize", "Batch Size cannot be null");
            }

            if (string.IsNullOrWhiteSpace(printer))
            {
                throw new ArgumentNullException("printer", "Printer must be specified");
            }

            var batchId = _service.PrintBucket(bucketId.Value, batchSize.Value, printer);

            #region Handling Cookies

            var cookie = clearCookie ? null : Request.Cookies[COOKIE_SELECTED_PRINTERS];

            if (cookie == null)
            {
                cookie = new HttpCookie(COOKIE_SELECTED_PRINTERS);
            }
            cookie.Values.Add(printer, printer);
            cookie.Expires = DateTime.Now.AddMinutes(60); // remember for one hour sliding
            this.Response.Cookies.Add(cookie);

            cookie = Request.Cookies[COOKIE_SELECTED_BATCH_SIZE];
            if (cookie == null || cookie.Value != batchSize.ToString())
            {
                cookie = new HttpCookie(COOKIE_SELECTED_BATCH_SIZE, batchSize.ToString());
            }
            cookie.Expires = DateTime.Now.AddDays(7); // remember for one hour sliding
            this.Response.Cookies.Add(cookie);

            #endregion

            return Content(batchId);
        }

        /// <summary>
        /// The passed printer is added to the cookie
        /// </summary>
        /// <param name="batchId"></param>
        /// <param name="printer"></param>
        /// <returns></returns>
        [HttpPost]
        [AuthorizeEx("Print batch requires role {0}", Roles = ROLE_DCMS_LITE)]
        public virtual ActionResult ReprintBatch(string batchId, string printer)
        {
            if (string.IsNullOrWhiteSpace(batchId))
            {
                ModelState.AddModelError("batchId", "Batch ID cannot be null.");
                return RedirectToAction(Actions.Batch(batchId));
            }
            if (string.IsNullOrWhiteSpace(printer))
            {
                ModelState.AddModelError("Printer", "Printer name is required.");
                return RedirectToAction(Actions.Batch(batchId));
            }
            //Calling service method to reprint the batch.
            _service.ReprintBatch(batchId, printer);
            AddStatusMessage(string.Format("Batch {0} printed on {1} successfully.", batchId, printer));

            var cookie = Request.Cookies[COOKIE_SELECTED_PRINTERS];

            if (cookie == null)
            {
                cookie = new HttpCookie(COOKIE_SELECTED_PRINTERS);
            }
            foreach (var key in cookie.Values.AllKeys)
            {
                // The value of each key has been emptied
                cookie.Values[key] = "";
            }
            // The selected printer gets a value of Y
            cookie.Values[printer] = "Y";
            cookie.Expires = DateTime.Now.AddMinutes(60);       // remember for one hour sliding
            this.Response.Cookies.Add(cookie);

            return RedirectToAction(Actions.Batch(batchId));
        }

        public virtual ActionResult Batch(string batchId)
        {
            if (string.IsNullOrWhiteSpace(batchId))
            {
                ModelState.AddModelError("batchId", "Batch is required");
                return RedirectToAction(Actions.Index());
            }
            var batch = _service.GetBatch(batchId);
            if (batch == null)
            {
                ModelState.AddModelError("batchId", "Batch does not exist.");
                return RedirectToAction(Actions.Index());
            }
            var cookie = Request.Cookies[COOKIE_SELECTED_PRINTERS];
            var bucket = _service.GetBucket(batch.BucketId.Value);
            var model = new BatchViewModel
                {
                    BucketId = bucket.BucketId,
                    IsFrozenBucket = bucket.IsFrozen,
                    PrintedBatchList = _service.GetBatches(batch.BucketId.Value).Select(Map).ToArray(),
                    PrinterList = (from printer in _service.GetPrinterList(_buildingId)
                                   select new SelectListItem
                                   {
                                       Text = printer.Description,
                                       Value = printer.PrinterName,
                                       Selected = cookie != null && cookie.Values[printer.PrinterName] == "Y"
                                   }).ToArray(),
                    BoxList = _service.GetBoxesOfBatch(_buildingId, batchId).Select(Map).ToArray()
                };
            model.CurrentPrintBatch = model.PrintedBatchList.FirstOrDefault(p => p.BatchId == batchId);
            return View(Views.Batch, model);
        }

        [HttpPost]
        [AuthorizeEx("Printing Label requires role {0}", Roles = ROLE_DCMS_LITE)]
        public virtual ActionResult PrintBoxLabel(string uccId, string printer)
        {
            if (string.IsNullOrWhiteSpace(uccId))
            {
                throw new ArgumentNullException("uccId", "Batch ID cannot be null.");
            }
            if (string.IsNullOrWhiteSpace(printer))
            {
                throw new ArgumentNullException("printer", "Printer name is required.");
            }
            //Calling here service method to print the ucc label.
            _service.PrintBoxLabel(uccId, printer);
            return Content(string.Format("Label of Box {0} printed successfully.", uccId));
        }
    }
}
