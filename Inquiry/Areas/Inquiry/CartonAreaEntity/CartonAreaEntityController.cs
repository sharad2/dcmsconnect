using DcmsMobile.Inquiry.Areas.Inquiry.SharedViews;
using DcmsMobile.Inquiry.Helpers;
using System;
using System.Linq;
using System.Web.Mvc;

namespace DcmsMobile.Inquiry.Areas.Inquiry.CartonAreaEntity
{
    [RoutePrefix("ctnarea")]
    public partial class CartonAreaEntityController : InquiryControllerBase
    {
        private Lazy<CartonAreaEntityRepository> _repos;
        protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        {
            base.Initialize(requestContext);
            _repos = new Lazy<CartonAreaEntityRepository>(() => new CartonAreaEntityRepository(requestContext.HttpContext.User.Identity.Name,
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


        [Route("loc/{id}", Name = DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_SearchCartonLocation1)]
        [SearchQuery(@"Select {0}, msl.location_id, 'Carton Location ' || msl.location_id, NULL, NULL FROM <proxy />master_storage_location msl WHERE msl.location_id = :search_text")]
        public virtual ActionResult CartonLocation(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return RedirectToAction(MVC_Inquiry.Inquiry.Home.Index());
            }
            int MAXNONPALLETCARTONS = 100;
            var location = _repos.Value.GetCartonLocationInfo(id);
            if (location == null)
            {
                this.AddStatusMessage(string.Format("No info found for {0}", id));
                return RedirectToAction(MVC_Inquiry.Inquiry.Home.Index());
            }
            var model = new CartonLocationViewModel
            {
                //PalletCartons = _repos.Value.GetCartonsOfLocationOnPallet(id).Select(p => new CartonLocationPalletModel
                //{
                //    CartonCount = p.CartonCount,
                //    DistinctSKUs = p.DistinctSKUs,
                //    PalletId = p.PalletId,
                //    SKUQuantity = p.SKUQuantity
                //}).ToArray(),
                Area = location.Area,
                AssignedColor = location.AssignedSku.Color,
                AssignedDimension = location.AssignedSku.Dimension,
                AssignedSkuSize = location.AssignedSku.SkuSize,
                AssignedStyle = location.AssignedSku.Style,
                //AssignedUpc = location.AssignedSku.Upc,
                Capacity = location.Capacity,
                LocationId = location.LocationId,
                ShortName = location.ShortName,
                WhId = location.WhId
            };

            var nonPalletCartons = _repos.Value.GetCartonsAtLocation(id);
            model.TotalCarton = nonPalletCartons.Count;
            model.Cartons = nonPalletCartons.Take(MAXNONPALLETCARTONS).Select(p => new CartonAtLocationModel
            {
                CartonId = p.CartonId,
                SKUQuantity = p.SKUQuantity,
                PalletId = p.PalletId
            }).OrderBy(p => string.IsNullOrWhiteSpace(p.PalletId) ? "Z" : p.PalletId).ThenBy(p => p.CartonId).ToArray();

            return View(Views.CartonLocation, model);
        }

        [Route("loc/excel/{id}")]
        public virtual ActionResult CartonLocationExcel(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException("id");
            }
            //var palletCartons = _repos.Value.GetCartonsOfLocationOnPallet(id).Select(p => new CartonLocationPalletModel
            //{
            //    CartonCount = p.CartonCount,
            //    DistinctSKUs = p.DistinctSKUs,
            //    PalletId = p.PalletId,
            //    SKUQuantity = p.SKUQuantity
            //}).ToArray();

            var nonPalletCartons = _repos.Value.GetCartonsAtLocation(id).Take(GlobalConstants.MAX_EXCEL_ROWS).Select(p => new CartonAtLocationModel
            {
                CartonId = p.CartonId,
                //DistinctSKUs = p.DistinctSKUs,
                SKUQuantity = p.SKUQuantity
            }).ToArray();
            var result = new ExcelResult("CartonLocation_" + id);
            //result.AddWorkSheet(palletCartons, "Pallets on Location", "List on Pallets on Location " + id);
            result.AddWorkSheet(nonPalletCartons, "Cartons on Location", "List of Non Pallet Cartons on location " + id);
            return result;
        }

        [Route("area{id}", Name = DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_SearchCartonArea1)]
        [SearchQuery(@"SELECT {0}, inventory_storage_area, 'Carton Area ' || max(short_name) || ' for ' || warehouse_location_id, inventory_storage_area, NULL FROM <proxy />tab_inventory_area
        WHERE (inventory_storage_area= :search_text or short_name= :search_text) and stores_what = 'CTN' group by inventory_storage_area,warehouse_location_id")]
        public virtual ActionResult CartonArea(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return RedirectToAction(MVC_Inquiry.Inquiry.Home.Index());
            }
            var area = _repos.Value.GetCartonAreaInfo(id);
            if (area == null)
            {
                this.AddStatusMessage(string.Format("No Info Found For {0}", id));
                return RedirectToAction(MVC_Inquiry.Inquiry.Home.Index());
            }
            var route = Url.RouteCollection[DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_ManageCartonArea1];
            var model = new CartonAreaViewModel
            {
                AssignedLocations = area.AssignedLocations ?? 0,
                Description = area.Description,
                PalletRequired = area.IsPalletRequired,
                RepackArea = area.IsRepackArea,
                LocationNumberingFlag = area.LocationNumberingFlag,
                NonEmptyLocations = area.NonEmptyLocations ?? 0,
                OverdraftAllowed = area.OverdraftAllowed,
                ShortName = area.ShortName,
                TotalLocations = area.TotalLocations ?? 0,
                //ShipableInventory = area.ShipableInventory,
                WhID = area.WhID,
                AreaId = area.CartonStorageArea,
                UrlManageArea = route == null ? string.Empty : Url.RouteUrl(DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_ManageCartonArea1, new
                {
                    id = area.CartonStorageArea
                })
            };
            model.AreaInventory = _repos.Value.GetCartonAreaInventory(id).Select(p => new CartonAreaInventoryModel(p)).ToArray();
            //DeepValidateModel(model);
            //var route = Url.RouteCollection[DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_ManageCartonArea1];
            //model.DcmsLinks.Add(new DcmsLinkModel
            //{
            //    ShortDescription = "Manage Area",
            //    Url = route == null ? string.Empty : Url.RouteUrl(DcmsLibrary.Mvc.PublicRoutes.DcmsConnect_ManageCartonArea1, new
            //    {
            //        id = model.AreaId
            //    })
            //});

            return View(Views.CartonArea, model);
        }

        [Route("excel/{id}")]
        public virtual ActionResult CartonAreaExcel(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException("id");
            }
            var areaInventory = _repos.Value.GetCartonAreaInventory(id).Select(p => new CartonAreaInventoryModel(p)).ToArray();
            var result = new ExcelResult("CartonArea_" + id);
            result.AddWorkSheet(areaInventory, "Inventory", "Inventory in Area " + id);
            return result;
        }

        [Route("list")]
        public virtual ActionResult CartonAreaList()
        {
            var areaList = _repos.Value.GetCartonAreaList();
            var model = new CartonAreaListViewModel
            {
                AllAreas = areaList.Select(p => new CartonAreaModel
                {
                    CartonCount = p.CartonCount,
                    Area = p.Area,
                    AreaShortName = p.AreaShortName,
                    Description = p.Description,
                    DistinctSKUs = p.DistinctSKUs,
                    Quantity = p.Quantity,
                    TotalLocations = p.TotalLocations,
                    UsedLocations = p.UsedLocations,
                    BuildingId = p.Building,
                    BuildingDescription = p.BuildingDescription
                }).OrderBy(p => string.IsNullOrWhiteSpace(p.BuildingId) ? "ZZ" : p.BuildingId).ThenByDescending(p => p.Quantity).ToList()
            };
            return View(Views.CartonAreaList, model);
        }

        [Route("location")]
        public virtual ActionResult CartonLocationList()
        {

            var cartonLocList = _repos.Value.GetCartonLocationList();
            var model = new CartonLocationListViewModel
          {
              CartonLocationList = cartonLocList.Select(p => new CartonLocationHeadlineModel
              {
                  LocationId = p.LocationId,
                  Area = p.Area,
                  ShortName = p.ShortName,
                  WhId = p.WhId,
                  Capacity = p.Capacity
              }).ToList()

          };
            return View(Views.CartonLocationList, model);
        }
    }
}