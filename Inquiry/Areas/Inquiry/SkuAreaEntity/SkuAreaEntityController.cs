using DcmsMobile.Inquiry.Helpers;
using System;
using System.Linq;
using System.Web.Mvc;

namespace DcmsMobile.Inquiry.Areas.Inquiry.SkuAreaEntity
{
    [RoutePrefix("skuarea")]
    public partial class SkuAreaEntityController : InquiryControllerBase
    {
        private Lazy<SkuAreaEntityRepository> _repos;
        protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        {
            base.Initialize(requestContext);
            _repos = new Lazy<SkuAreaEntityRepository>(() => new SkuAreaEntityRepository(requestContext.HttpContext.User.Identity.Name,
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

        [Route("{id}", Name = DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_SearchSkuArea1)]
        [SearchQuery(@"SELECT {0}, ia.ia_id, 'Forward area '|| max(ia.short_name) || ' for ' || ia.warehouse_location_id, ia.ia_id, NULL FROM <proxy />ia 
WHERE ia.ia_id = :search_text or ia.short_name = :search_text group by ia.ia_id , ia.warehouse_location_id")]
        public virtual ActionResult SKUArea(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return RedirectToAction(MVC_Inquiry.Inquiry.Home.Index());
            }
            var skuArea = _repos.Value.GetSkuAreaInfo(id);
            if (skuArea == null)
            {
                this.AddStatusMessage(string.Format("No Info Found For {0}", id));
                return RedirectToAction(MVC_Inquiry.Inquiry.Home.Index());
            }
            var model = new SkuAreaViewModel
            {
                Description = skuArea.Description,
                DefaultLocation = skuArea.DefaultLocation,
                WhId = skuArea.WhId,
                PickingAreaFlag = skuArea.PickingAreaFlag,
                ShipingAreaFlag = skuArea.ShipingAreaFlag,
                PullCartonLimit = skuArea.PullCartonLimit,
                NumberOfLocations = skuArea.NumberOfLocations ?? 0,
                AssignedLocations = skuArea.AssignedLocations ?? 0,
                ShortName = skuArea.ShortName
            };
            return View(Views.SKUArea, model);

        }


        [Route("loc/{id}", Name = DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_SearchSkuLocation1)]
        [SearchQuery(@"SELECT {0}, location_id, 'SKU Location ' || location_id, NULL, NULL FROM <proxy />ialoc WHERE location_id= :search_text")]
        public virtual ActionResult SkuLocation(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return RedirectToAction(MVC_Inquiry.Inquiry.Home.Index());
            }
            var loc = _repos.Value.GetSkuLocation2(id);
            if (loc == null)
            {
                this.AddStatusMessage(string.Format("No info found for {0}", id));
                return RedirectToAction(MVC_Inquiry.Inquiry.Home.Index());
            }
            var model = new SkuLocationViewModel(loc)
            {
                AllPallets = _repos.Value.GetPalletsOfSkuLocation(id).Select(p => new SkuLocationPalletModel(p)).ToArray()
                //AssignUnassignAudit = _repos.Value.GetLocAssignUnassignAudit(id).Select(p => new LocationAuditModel(p)).ToArray()
                //InventoryAudit = _repos.Value.GetLocationsInventoryAudit(id).Select(p => new LocationAuditModel(p)).ToArray()
            };

            //if (loc.AssignedSkuId.HasValue && !model.AllSku.Any(p => loc.AssignedSkuId == p.SkuId))
            //{
            //    // The assigned SKU is not part of the SKUs at location
            //    model.AllSku.Insert(0, new SkuLocationSkuModel
            //    {
            //        //Upc = loc.AssignedUpc,
            //        Style = loc.AssignedStyle,
            //        Color = loc.AssignedColor,
            //        Dimension = loc.AssignedDimension,
            //        SkuSize = loc.AssignedSkuSize,
            //        SkuId = loc.AssignedSkuId.Value,
            //        //IsAssigned = true
            //    });

            //}
            return View(Views.SkuLocation, model);
        }

        /// <summary>
        /// Returs audit entries of passed location
        /// </summary>
        /// <param name="locationId"></param>
        /// <returns></returns>
        [Route("locaudit")]
        public virtual PartialViewResult LocationAudit(string locationId)
        {
            var list = _repos.Value.GetLocationsInventoryAudit(locationId).Select(p => new LocationAuditModel(p)).ToArray();
            return PartialView(Views._locationAuditPartial, list);
        }

        [Route("skuassignmentaudit")]
        public virtual PartialViewResult SkuAssignmentAudit(string locationId)
        {
            var list = _repos.Value.GetLocAssignUnassignAudit(locationId).Select(p => new LocationAuditModel(p)).ToArray();
            return PartialView(Views._SkuAssignmentAuditPartial, list);
        }


        [Route("loc/excel/{id}")]
        public virtual ActionResult SkuLocationExcel(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException("id");
            }
            var loc = _repos.Value.GetSkuLocation2(id);
            if (loc == null)
            {
                throw new NotImplementedException("What should we do here?");
            }
            var allSku = loc.SkusAtLocation.Select(p => new SkuLocationSkuModel
            {
                //Upc = p.Upc,
                Style = p.Style,
                Color = p.Color,
                Dimension = p.Dimension,
                SkuSize = p.SkuSize,
                Pieces = p.Pieces,
                //IsAssigned = loc.AssignedSkuId == p.SkuId
            }).ToList();
            var allPallets = _repos.Value.GetPalletsOfSkuLocation(id).Select(p => new SkuLocationPalletModel(p)).ToArray();
            var assignUnassignAudit = _repos.Value.GetLocAssignUnassignAudit(id).Select(p => new LocationAuditModel(p)).ToArray();
            var inventoryAudit = _repos.Value.GetLocationsInventoryAudit(id).Select(p => new LocationAuditModel(p)).ToArray();

            var result = new ExcelResult("SkuLocation_" + id);
            result.AddWorkSheet(allSku, "Inventory", "List on content on location " + id);
            result.AddWorkSheet(allPallets, "Pallets", "List of Pallets on location " + id);
            result.AddWorkSheet(assignUnassignAudit, "Assignment Audit", "SKU assignment audot of location " + id);
            result.AddWorkSheet(inventoryAudit, "Inventory Audit", "Inventory Audit of location" + id);
            return result;
        }

        [Route("arealist")]
        public virtual ActionResult SKUAreaList()
        {

            var skuArea = _repos.Value.GetSkuAreaList();
            var model = new SkuAreaListViewModel()
            {
                SKUAreaList = skuArea.Select(p => new SKUAreaListModel
                {
                    IaId = p.IaId,
                    ShortName = p.ShortName,
                    Description = p.Description,
                    NumberOfLocations = p.NumberOfLocations
                }).ToList()
            };
            return View(Views.SKUAreaList, model);

        }

        [Route("loclist")]
        public virtual ActionResult SkuLocationList()
        {
            var List = _repos.Value.GetSkuLocationList();
            var model = new SkuLocationListViewModel()
            {
                SkuLocList = List.Select(p => new SkuLocationHeadlineModel
                {
                    LocationId = p.LocationId,
                    SkuId = p.SkuId,
                    Style = p.Style,
                    Color = p.Color,
                    Dimension = p.Dimension,
                    SkuSize = p.SkuSize,
                    IaId = p.IaId,
                    AreaShortName = p.AreaShortName,
                    MaxPieces = p.MaxPieces,
                    BuildingId = p.BuildingId
                }).ToList()
            };
            return View(Views.SkuLocationList, model);
        }

    }
}