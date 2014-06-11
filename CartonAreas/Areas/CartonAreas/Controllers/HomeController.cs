using DcmsMobile.CartonAreas.Repository;
using DcmsMobile.CartonAreas.ViewModels;
using EclipseLibrary.Mvc.Controllers;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;

namespace DcmsMobile.CartonAreas.Areas.CartonAreas.Controllers
{
    [AuthorizeEx("CAM requires Role {0}", Roles = "SRC_CAM_MGR")]
    public partial class HomeController : EclipseController
    {
        #region Intialization

        /// <summary>
        /// Required by T4MVC
        /// </summary>
        public HomeController()
        {

        }

        private CartonAreasService _service;

        protected override void Initialize(RequestContext requestContext)
        {
            if (_service == null)
            {
                _service = new CartonAreasService(requestContext);
            }
            base.Initialize(requestContext);
        }

        protected override void Dispose(bool disposing)
        {
            if (_service != null)
            {
                _service.Dispose();
                _service = null;
            }
            base.Dispose(disposing);
        }

        #endregion

        /// <summary>
        /// Displays home page which shows buildings
        /// </summary>
        /// <returns></returns>
        public virtual ActionResult Index()
        {
            var model = new IndexViewModel
                        {
                            Buildings = (from item in _service.GetBuildings()
                                         select new BuildingModel
                                         {
                                             BuildingId = item.BuildingId,
                                             Address1 = item.Address.Address1,
                                             Address2 = item.Address.Address2,
                                             Address3 = item.Address.Address3,
                                             Address4 = item.Address.Address4,
                                             City = item.Address.City,
                                             CountCartonArea = item.CountCartonAreas,
                                             CountPickingAreas = item.CountPickingAreas,
                                             CountLocation = item.CountLocations,
                                             CountNumberedArea = item.CountNumberedAreas,
                                             Description = item.Description,
                                             InsertDate = item.InsertDate,
                                             InsertedBy = item.InsertedBy,
                                             ReceivingPalletLimit = item.ReceivingPalletLimit,
                                             State = item.Address.State,
                                             ZipCode = item.Address.ZipCode,
                                             CountryCode = item.Address.CountryCode
                                         }).ToList()
                        };
            return View(Views.Index, model);
        }

        /// <summary>
        /// Edit pallet limit of any building.
        /// </summary>
        /// <param name="buildingId"></param>
        /// <param name="palletLimit"></param>
        /// <returns></returns>
        [HttpPost]
        public virtual ActionResult EditPalletLimit(string buildingId, int? palletLimit)
        {
            _service.UpdatePalletLimit(buildingId, palletLimit);
            return RedirectToAction(this.Actions.Index());
        }

        /// <summary>
        /// Edit address of passed building
        /// </summary>
        /// <param name="buildingId"></param>
        /// <returns></returns>
        [HttpGet]
        public virtual ActionResult EditAddressOfBuilding(string buildingId)
        {
            var building = _service.GetBuilding(buildingId);
            if (building == null)
            {
                ModelState.AddModelError("", string.Format("Invalid building {0}", buildingId));
                return RedirectToAction(this.Actions.Index());
            }
            var model = new EditAddressOfBuildingViewModel
            {
                BuildingId = buildingId,
                Description = building.Description,
                Address1 = building.Address.Address1,
                Address2 = building.Address.Address2,
                Address3 = building.Address.Address3,
                Address4 = building.Address.Address4,
                City = building.Address.City,
                State = building.Address.State,
                ZipCode = building.Address.ZipCode,
                CountryCode = building.Address.CountryCode,
                CountryCodeList = (from item in _service.GetCountryList()
                                   select new SelectListItem
                                   {
                                       Text = item.Code + " : " + item.Description,
                                       Value = item.Code,
                                       Selected = item.Code == building.Address.CountryCode
                                   }).ToArray()
            };
            return View(Views.EditBuilding, model);
        }

        /// <summary>
        /// Update address of passed building
        /// </summary>
        /// <param name="model"></param>
        /// <param name="create">If true, then the building will be created, otherwise it will be updated</param>
        /// <returns></returns>
        [HttpPost]
        public virtual ActionResult UpdateBuilding(EditAddressOfBuildingViewModel model, bool create)
        {
            if (!ModelState.IsValid)
            {
                return View(Views.EditBuilding, model);
            }
            var address = new Address
            {
                Address1 = model.Address1,
                Address2 = model.Address2,
                Address3 = model.Address3,
                Address4 = model.Address4,
                City = model.City,
                State = model.State,
                ZipCode = model.ZipCode,
                CountryCode = model.CountryCode
            };
            try
            {
                if (create)
                {
                    throw new NotImplementedException();
                }
                else
                {
                    _service.UpdateAddress(model.BuildingId, model.Description, address);
                    this.AddStatusMessage(string.Format("Building {0} updated.", model.BuildingId));
                }
            }
            catch (DbException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(Views.EditBuilding, model);
            }
            return RedirectToAction(this.Actions.Index());
        }

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <returns></returns>
        //[HttpGet]
        //[Obsolete]
        //public virtual ActionResult AddNewBuilding()
        //{
        //    var model = new AddBuildingViewModel
        //    {
        //        CountryCodeList = (from item in _service.GetCountryList()
        //                           select new SelectListItem
        //                           {
        //                               Text = item.Code + " : " + item.Description,
        //                               Value = item.Code
        //                           }).ToArray()
        //    };
        //    return View(Views.AddBuilding, model);
        //}

        //[HttpPost]
        //[Obsolete]
        //public virtual ActionResult AddBuilding(AddBuildingViewModel modal)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return View(Views.AddBuilding, modal);
        //    }
        //    modal.BuildingId = modal.BuildingId.ToUpper();
        //    try
        //    {
        //        _service.AddBuilding(new Building
        //        {
        //            BuildingId = modal.BuildingId,
        //            Description = modal.Description,
        //            Address = new Address
        //            {
        //                Address1 = modal.Address1,
        //                Address2 = modal.Address2,
        //                Address3 = modal.Address3,
        //                Address4 = modal.Address4,
        //                City = modal.City,
        //                State = modal.State,
        //                ZipCode = modal.ZipCode,
        //                CountryCode = modal.CountryCode
        //            }
        //        });
        //        this.AddStatusMessage(string.Format("New Building {0} added sucessfully.", modal.BuildingId));
        //    }
        //    catch (DbException ex)
        //    {
        //        ModelState.AddModelError("", ex.Message);
        //        return RedirectToAction(this.Actions.AddNewBuilding());
        //    }
        //    return RedirectToAction(MVC_CartonAreas.CartonAreas.Home.Index());
        //}

        public virtual ActionResult CartonArea(string buildingId)
        {
            if (string.IsNullOrWhiteSpace(buildingId))
            {
                return RedirectToAction(Actions.Index());
            }
            var model = new CartonAreaViewModel
            {
                CartonAreaList = _service.GetCartonAreas(buildingId).Select(p => new CartonAreaModel(p)).ToArray(),
                BuildingId = buildingId
            };
            return View(Views.CartonArea, model);

        }

        /// <summary>
        /// This method is used to Update all value of Carton Areas on index page.
        /// </summary>
        /// <param name="cam">
        /// This parameter contains all the flags to set against AreaId
        /// </param>
        /// <returns>
        /// This method returns the index view to update the grid.
        /// </returns>        
        [HttpPost]
        public virtual ActionResult UpdateArea(CartonAreaModel cam)
        {
            var entity = new CartonArea
                {
                    AreaId = cam.AreaId,
                    LocationNumberingFlag = cam.LocationNumberingFlag,
                    IsPalletRequired = cam.IsPalletRequired,
                    UnusableInventory = cam.UnusableInventory,
                    Description = cam.Description
                };
            try
            {
                _service.UpdateArea(entity);
                AddStatusMessage(string.Format("Area {0} successfully updated", entity.ShortName));
            }
            catch (DbException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            //TODO : pass building also
            return RedirectToAction(MVC_CartonAreas.CartonAreas.Home.CartonArea(entity.BuildingId));
        }

        /// <summary>
        /// Creates and populates ManageCartonAreaViewModel. The parameters are the filters used to retrieve the location list
        /// </summary>
        ///<param name="areaId"></param>
        /// <returns>The populated model, or null if invalid area was passed</returns>
        private ManageCartonAreaViewModel CreateManageCartonAreaViewModel(string areaId)
        {
            var area = _service.GetCartonAreaInfo(areaId);
            if (string.IsNullOrEmpty(areaId))
            {
                return null;
            }
            var model = new ManageCartonAreaViewModel
            {
                Matrix = new CartonAreaLocationCountMatrixViewModel(area),
                ShortName = area.ShortName,
                BuildingId = area.BuildingId
            };

            model.AssignedSku = new AssignSkuViewModel
            {
                VwhList = _service.GetVwhList().Select(p => new SelectListItem
                {
                    Text = (string.IsNullOrEmpty(p.Code) ? "NULL" : p.Code) + " : " + p.Description,
                    Value = p.Code
                })
            };
            return model;
        }

        /// <summary>
        /// Called when areaId is passed via query string
        /// </summary>
        /// <param name="areaId"></param>
        /// <param name="assigned"></param>
        /// <param name="emptyLocations"></param>
        /// <returns></returns>
        public virtual ActionResult ManageCartonArea(string areaId)
        {
            if (string.IsNullOrEmpty(areaId))
            {
                return Index();
            }
            var model = CreateManageCartonAreaViewModel(areaId);
            var locations = _service.GetCartonAreaLocations(areaId, 500);
            model.Locations = locations.Select(p => new LocationModel(p)).ToArray();
            model.CountTotalLocations = locations.Select(p => p.CountTotalLocations).First();
            return View(Views.ManageCartonArea, model);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="areaId"></param>
        /// <param name="assignedSkuId"></param>
        /// <param name="locationId"></param>
        /// <param name="assignedLocation"></param>
        /// <param name="emptyLocations"></param>
        /// <returns></returns>
        public virtual ActionResult ApplyCartonAreaLocationFilter(string areaId, int? assignedSkuId, string locationId, bool? assignedLocation, bool? emptyLocations)
        {
            if (string.IsNullOrWhiteSpace(areaId))
            {
                return RedirectToAction(Actions.Index());
            }
            var model = CreateManageCartonAreaViewModel(areaId);
            model.Matrix.AssignedLocationsFilter = assignedLocation;
            model.Matrix.EmptyLocationsFilter = emptyLocations;
            var locations = _service.GetCartonAreaLocationsOfFilters(areaId, assignedSkuId, locationId, assignedLocation, emptyLocations, 500);
            model.Locations = locations.Select(p => new LocationModel(p)).ToArray();
            if (locations.Count > 0)
            {
                model.CountTotalLocations = locations.Select(p => p.CountTotalLocations).First();
            }
            if (assignedSkuId != null)
            {
                var sku = _service.GetSku(assignedSkuId.Value);
                model.AssignedToSkuFilter = new SkuModel
                {
                    Style = sku.Style,
                    Color = sku.Color,
                    Dimension = sku.Dimension,
                    SkuSize = sku.SkuSize,
                    UpcCode = sku.UpcCode
                };
            }
            if (!string.IsNullOrWhiteSpace(locationId))
            {
                model.LocationPatternFilter = locationId;
            }
            return View(Views.ManageCartonArea, model);
        }

        /// <summary>
        /// Update location assignment.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public virtual ActionResult UpdateLocation(AssignSkuViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(p => p.Errors).Select(p => p.ErrorMessage);
                throw new Exception(string.Join("; ", errors));
            }
            var areaId = _service.AssignSkuToLocation(model.LocationId, model.SkuId, model.MaxAssignedCarton,
                                              model.AssignedVwhId);
            //Response.StatusCode = 200;
            // Update carton area info in areaInfo table.
            var area = _service.GetCartonAreaInfo(areaId);
            return PartialView(MVC_CartonAreas.CartonAreas.Home.Views._cartonAreaLocationCountMatrixPartial, new CartonAreaLocationCountMatrixViewModel(area));
        }

        /// <summary>
        /// Unassign SKU and VWh from location. This should be called via ajax. It will return HTML for the count matrix
        /// </summary>
        /// <param name="locationId"></param>
        /// <param name="areaId"></param>
        /// <returns></returns>
        [HttpPost]
        public virtual ActionResult UnassignLocation(string locationId)
        {
            var areaId = _service.UnassignSkuFromlocation(locationId);
            Response.StatusCode = 200;
            // Update carton area info in areaInfo table.
            var area = _service.GetCartonAreaInfo(areaId);
            return PartialView(MVC_CartonAreas.CartonAreas.Home.Views._cartonAreaLocationCountMatrixPartial, new CartonAreaLocationCountMatrixViewModel(area));
        }

        public virtual ActionResult PickingArea(string buildingId)
        {
            var model = new PickingAreaViewModel
            {
                BuildingId = buildingId,
                PickingAreaList = (from area in _service.GetPickingAreas(buildingId)
                                   select new PickingAreaModel
                                   {
                                       AreaId = area.AreaId,
                                       LocationNumberingFlag = area.LocationNumberingFlag,
                                       Description = area.Description,
                                       ShortName = area.ShortName,
                                       IsPickingArea = area.IsPickingArea,
                                       IsRestockArea = area.IsRestockArea,
                                       IsShippingArea = area.IsShippingArea,
                                       LocationCount = area.LocationCount
                                   }).ToArray()
            };
            return View(Views.PickingArea, model);

        }

        public virtual ActionResult Tutorial()
        {
            return View(Views.Tutorial);
        }

        public virtual ActionResult UpdatePickingArea(PickingAreaViewModel model)
        {
            try
            {
                var updatePickingAreas = new PickingArea
                {
                    AreaId = model.AreaId,
                    Description = model.Description,
                    IsPickingArea = model.IsPickingArea,
                    IsRestockArea = model.IsRestockArea,
                    IsShippingArea = model.IsShippingArea,
                    LocationNumberingFlag = model.LocationNumberingFlag //TODO:How to update,not implemented in repository yet.                   
                };
                _service.UpdatePickingArea(updatePickingAreas);
                AddStatusMessage(string.Format("Picking Area {0} successfully updated", updatePickingAreas.ShortName));
            }
            catch (DbException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            return RedirectToAction(MVC_CartonAreas.CartonAreas.Home.PickingArea(model.BuildingId));
        }

        public virtual ActionResult ManagePickingArea(string areaId)
        {
            if (string.IsNullOrEmpty(areaId))
            {
                return Index();
            }
            var area = _service.GetPickingArea(areaId);
            if (area == null)
            {
                ModelState.AddModelError("", string.Format("Area {0} not found.", areaId));
                return Index();
            }
            var locations = _service.GetPickingAreaLocations(areaId, 500);
            if (locations.Count == 0)
            {
                ModelState.AddModelError("", string.Format("No location in area {0}.", areaId));
                return RedirectToAction(this.Actions.PickingArea(area.BuildingId));
            }
            var model = new ManagePickingAreaViewModel
            {
                Matrix = new PickingAreaLocationCountMatrixViewModel(area),
                AreaId = areaId,
                ShortName = area.ShortName,
                BuildingId = area.BuildingId,
                PickingLocations = (from item in locations
                                    select new PickingLocationModel(item)).ToArray(),
                CountTotalLocations = locations.Select(p => p.CountTotalLocations).First()
            };
            return View(Views.ManagePickingArea, model);
        }

        public virtual ActionResult ApplyPickingAreaLocationFilter(string areaId, bool? assignedLocation, bool? emptyLocations, int? assignedSkuId, string locationId)
        {
            if (string.IsNullOrWhiteSpace(areaId))
            {
                return RedirectToAction(Actions.Index());
            }
            var area = _service.GetPickingArea(areaId);
            if (area == null)
            {
                ModelState.AddModelError("", string.Format("Area {0} not found.", areaId));
                return Index();
            }
            var model = new ManagePickingAreaViewModel
                        {
                            Matrix = new PickingAreaLocationCountMatrixViewModel(area),
                            AreaId = areaId,
                            ShortName = area.ShortName,
                            BuildingId = area.BuildingId
                        };
            model.Matrix.AssignedLocationsFilter = assignedLocation;
            model.Matrix.EmptyLocationsFilter = emptyLocations;
            var locations = _service.GetPickingAreaLocationsOfFilters(areaId, assignedLocation, emptyLocations, assignedSkuId, locationId, 500);
            model.PickingLocations = (from item in locations
                                      select new PickingLocationModel(item)).ToArray();
            if (locations.Count > 0)
            {
                model.CountTotalLocations = locations.Select(p => p.CountTotalLocations).First();
            }
            if (assignedSkuId != null)
            {
                var sku = _service.GetSku(assignedSkuId.Value);
                model.AssignedToSkuFilter = new PickingAreaSkuModel
                {
                    Style = sku.Style,
                    Color = sku.Color,
                    Dimension = sku.Dimension,
                    SkuSize = sku.SkuSize,
                    UpcCode = sku.UpcCode
                };
            }
            if (!string.IsNullOrWhiteSpace(locationId))
            {
                model.LocationPatternFilter = locationId;
            }
            return View(Views.ManagePickingArea, model);
        }
    }
}



//$Id$ 
//$Revision$
//$URL$
//$Header$
//$Author$
//$Date$
