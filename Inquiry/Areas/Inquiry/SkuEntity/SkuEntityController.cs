using DcmsMobile.Inquiry.Areas.Inquiry.SharedViews;
using DcmsMobile.Inquiry.Helpers;
using System;
using System.Linq;
using System.Web.Mvc;

namespace DcmsMobile.Inquiry.Areas.Inquiry.SkuEntity
{
    [RoutePrefix("sku")]
    public partial class SkuEntityController : InquiryControllerBase
    {
        private Lazy<SkuEntityRepository> _repos;
        protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        {
            base.Initialize(requestContext);
            _repos = new Lazy<SkuEntityRepository>(() => new SkuEntityRepository(requestContext.HttpContext.User.Identity.Name,
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

        [Route("{id:int?}", Name = DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_SearchUpc1)]
        [SearchQuery(@"SELECT {0}, TO_CHAR(sku_id), 'UPC Code ' || upc_code, NULL, NULL FROM <proxy />master_sku WHERE upc_code= :search_text", Group = "sku", Rating = 10)]
        [SearchQuery(@"
select {0}, TO_CHAR(msku.sku_id),
       'Private Label Barcode of Customer ' || mcs.customer_id,
       NULL, NULL
  from <proxy />master_customer_sku mcs
 inner join <proxy />master_sku msku
    on msku.sku_id = mcs.sku_id
WHERE MCS.SCANNED_BAR_CODE = :search_text
and rownum &lt; 2
", Group = "sku", Rating = 1)]
        public virtual ActionResult Sku(int? id)
        {
            if (id == null)
            {
                return RedirectToAction(MVC_Inquiry.Inquiry.Home.Index());
            }

            var sku = _repos.Value.GetSku(id.Value);

            if (sku == null)
            {
                this.AddStatusMessage(string.Format("No information found for scanned Sku {0}", id));
                return RedirectToAction(MVC_Inquiry.Inquiry.Home.Index());
            }
            var model = new SkuViewModel
            {
                AdditionalRetailPrice = sku.AdditionalRetailPrice,
                Color = sku.Color,
                Description = sku.Description,
                Dimension = sku.Dimension,
                PiecesPerPackage = sku.PiecesPerPackage,
                RetailPrice = sku.RetailPrice,
                SkuId = sku.SkuId,
                SkuSize = sku.SkuSize,
                StandardCaseQty = sku.StandardCaseQty,
                Style = sku.Style,
                Upc = sku.Upc
            };


            var query = from item in _repos.Value.GetSkuInventoryByArea(model.SkuId)
                        orderby item.Pieces descending, item.VwhId, item.Building, item.ShortName
                        select new SkuInventoryModel(item);
            model.SkuAreaInventory = query.ToList();

            model.CustomerLabelList = _repos.Value.GetPrivateLabelBarCodesOfSku(model.SkuId).Select(p => new SkuPrivateLabelModel
            {
                CustomerId = p.CustomerId,
               // InsertedBy = p.InsertedBy,
                //InsetDate = p.InsetDate,
                //ModifiedBy = p.ModifiedBy,
                //ModifiedDate = p.ModifiedDate,
                ScannedBarCode = p.ScannedBarCode,
                CustomerName = p.CustomerName
            }).ToList();

            return View(Views.Sku, model);
        }

       
        [Route("list")]
        public virtual ActionResult SkuList()
        {
            
            var SkuList = _repos.Value.GetSkuList();
            var model = new SkuListViewModel
            {
                SkuList = SkuList.Select(p => new SkuHeadlineModel
                {
                    sku_id = p.SkuId,
                    Style = p.Style,
                    Color = p.Color,
                    Dimension =p.Dimension,
                    Sku_Size =p.SkuSize,
                    Upc =p.Upc,
                    PickslipOrderDate = p.PickslipOrderDate                    
                }).ToList()
            };
            return View(Views.SkuList, model);

        }

   
        

        [Route("excel/{id:int}")]
        public virtual ActionResult SkuExcel(int id)
        {
            var result = new ExcelResult("SKU_" + id);

            var items = _repos.Value.GetSkuInventoryByArea(id);
            var query = from row in items
                        select new SkuInventoryModel(row);

            result.AddWorkSheet(query.ToArray(), "Inventory", "Inventory of SKU " + id + " per Area/Vwh");
            result.AddWorkSheet(_repos.Value.GetRecentOrders(id, GlobalConstants.MAX_EXCEL_ROWS).Select(p => new RecentPoModel(p)).ToArray(),
                "Recent PO", "Active POs of SKU " + id);
            return result;
        }

        [Route("style/{id}", Name = DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_SearchStyle1)]
        [SearchQuery(@"SELECT {0},  ms.style, 'Style ' || ms.style, NULL, NULL FROM <proxy />master_style ms where ms.style= :search_text")]
        public virtual ActionResult Style(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return RedirectToAction(MVC_Inquiry.Inquiry.Home.Index());
            }
            var style = _repos.Value.GetStyleInfo(id);
            if (style == null)
            {
                this.AddStatusMessage(string.Format("No info found for {0}", id));
                return RedirectToAction(MVC_Inquiry.Inquiry.Home.Index());
            }
            var model = new StyleViewModel
            {
                StyleId = style.StyleId,
                LabelId = style.LabelId,
                Description = style.Description,
                CountryOfOrigins = style.CountryOfOrigins.Select(p => new CountryOfOriginModel
                {
                    CountColors = p.CountColors,
                    CountryId = p.CountryId,
                    CountryName = p.CountryName
                }).ToArray()
            };

            return View(Views.Style, model);
        }

        [Route("label/{id}", Name = DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_SearchLabel1)]
        [SearchQuery(@"SELECT {0}, label_id, 'Style Label ' || label_id, NULL, NULL FROM <proxy />tab_style_label WHERE label_id= :search_text")]
        public virtual ActionResult Label(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return RedirectToAction(MVC_Inquiry.Inquiry.Home.Index());
            }
            var lbl = _repos.Value.GetLabelInfo(id);
            if (lbl == null)
            {
                this.AddStatusMessage(string.Format("No info found for {0}", id));
                return RedirectToAction(MVC_Inquiry.Inquiry.Home.Index());
            }
            LabelViewModel model = new LabelViewModel
            {
                LabelId = lbl.Item1,
                Description = lbl.Item2
            };

            return View(Views.Label, model);
        }

        [Route("RecentOrders")]
        public virtual ActionResult GetRecentOrders(int? skuId)
        {
            if (skuId == null)
            {
                throw new ArgumentNullException("SkuId cannot be null");
            }
            var orders = new RecentPoListViewModel
            {
                PoList = _repos.Value.GetRecentOrders(skuId.Value, 200).Select(p => new RecentPoModel(p)).ToArray()
            };
            //var orders = _repos.Value.GetRecentOrders(skuId.Value, 200).Select(p => new RecentPoModel(p)).ToArray();
            return PartialView(MVC_Inquiry.Inquiry.SharedViews.Views._recentPoListPartial, orders);
        }
       
        /// <summary>
        /// method for Sku Autocomplete
        /// </summary>
        /// <param name="term"></param>
        /// <returns></returns>
        [Route("ac/{term?}")]
        public virtual ActionResult SkuAutocomplete(string term)
        {
            try
            {
                var data = from sku in _repos.Value.SkuAutoComplete((term ?? string.Empty).ToUpper())
                           select new
                           {
                               label = string.Format("{0}: {1}, {2}, {3}, {4}", sku.Upc, sku.Style, sku.Color, sku.Dimension, sku.SkuSize),
                               value = sku.SkuId
                           };
                return Json(data, JsonRequestBehavior.AllowGet);
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