using System.Data.Common;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using DcmsMobile.CartonAreas.Models;
using DcmsMobile.CartonAreas.Repository;
using DcmsMobile.CartonAreas.ViewModels;
using EclipseLibrary.Mvc.Controllers;

namespace DcmsMobile.CartonAreas.Areas.CartonAreas.Controllers
{
    [AuthorizeEx("CAM requires Role {0}", Roles = "SRC_CAM_MGR")]
    public partial class HomeController : EclipseController
    {
        private CartonAreaModel Map(CartonArea src)
        {
            return new CartonAreaModel()
                {
                    AreaId = src.AreaId,
                    Description = src.Description,
                    ShortName = src.ShortName,
                    BuildingId = src.BuildingId,
                    TotalLocations = src.TotalLocations,
                    CountAssignedLocations = src.CountAssignedLocations,
                    CountEmptyAssignedLocations = src.CountEmptyAssignedLocations,
                    CountEmptyUnassignedLocations = src.CountEmptyUnassignedLocations,
                    CountEmptyLocations = src.CountEmptyLocations,
                    CountNonemptyAssignedLocations = src.CountNonemptyAssignedLocations,
                    CountNonemptyUnassignedLocations = src.CountNonemptyUnassignedLocations,
                    CountUnassignedLocations = src.CountUnassignedLocations,
                    CountNonemptyLocations = src.CountNonemptyLocations,
                    LocationNumberingFlag = src.LocationNumberingFlag,
                    IsPalletRequired = src.IsPalletRequired,
                    UnusableInventory = src.UnusableInventory
                };
        }

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
            }
            base.Dispose(disposing);
        }

        #endregion

        //public virtual ActionResult Buildings()
        //{
        //    return View(Views.Buildings);
        //}
        /// <summary>
        /// Displays home page which shows carton area list
        /// </summary>
        /// <returns></returns>
        public virtual ActionResult Index()
        {

            var model = new IndexViewModel
           {
               Building = (from item in _service.GetBuilding()
                           select new BuildingModel
               {
                   BuildingId = item.BuildingId,
                   Address1 = item.Address1,
                   Address2 = item.Address2,
                   Address3 = item.Address3,
                   Address4 = item.Address4,
                   City = item.City,
                   CountArea = item.CountArea,
                   CountLocation = item.CountLocation,
                   CountNumberedArea = item.CountNumberedArea,
                   Description = item.Description,
                   InsertDate = item.InsertDate,
                   InsertedBy = item.InsertedBy,
                   ReceivingPalletLimit = item.ReceivingPalletLimit,
                   State = item.State,
                   ZipCode = item.ZipCode
               }).ToList()
           };
            return View(Views.Index, model);
        }

        public virtual ActionResult CartonArea()
        {
            var model = new CartonAreaViewModel
            {
                CartonAreaList = _service.GetCartonAreas().Select(p => Map(p))
            };
            model.CurrentArea = new CartonAreaModel();
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
            try
            {
                var model = new CartonArea
                    {
                        AreaId = cam.AreaId,
                        LocationNumberingFlag = cam.LocationNumberingFlag,
                        IsPalletRequired = cam.IsPalletRequired,
                        UnusableInventory = cam.UnusableInventory,
                        Description = cam.Description
                    };
                _service.UpdateArea(model);
                AddStatusMessage(string.Format("Area {0} successfully updated", model.ShortName));
            }
            catch (DbException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            return RedirectToAction(MVC_CartonAreas.CartonAreas.Home.Index());
        }

        /// <summary>
        /// Called when areaId is passed via query string
        /// </summary>
        /// <param name="areaId"></param>
        /// <param name="assigned"></param>
        /// <param name="emptyLocations"></param>
        /// <returns></returns>
        public virtual ActionResult ManageCartonArea(string areaId, bool? assigned = null, bool? emptyLocations = null)
        {
            if (string.IsNullOrEmpty(areaId))
            {
                return Index();
            }
            return DoManageCartonArea(areaId, assigned, emptyLocations, null, null, null);
        }

        /// <summary>
        /// Called when the user enters the assigned SKU to search for
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <remarks>
        /// We clear the model state since we do not want server side validation. Server validation causes problems because
        /// only partial model is posted.
        /// </remarks>
        public virtual ActionResult ApplyAssignedSkuFilter(ManageCartonAreaViewModel model)
        {
            ModelState.Clear();
            return DoManageCartonArea(model.CurrentArea.AreaId, null, null, model.AssignedSkuId, model.AssignedSkuText, null);
        }

        /// <summary>
        /// Called when the user enters any location to search for
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public virtual ActionResult ApplyLocationIdFilter(ManageCartonAreaViewModel model)
        {
            ModelState.Clear();
            return DoManageCartonArea(model.CurrentArea.AreaId, null, null, null, null, model.LocationId);
        }

        /// <summary>
        /// Update location assignment.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public virtual ActionResult UpdateLocation(AssignSkuViewModel model)
        {
            _service.AssignSkuToLocation(model.LocationId, model.SkuId, model.MaxAssignedCarton,
                                             model.AssignedVwhId);
            Response.StatusCode = 200;

            // Update carton area info in areaInfo table.
            var cartonarea = Map(_service.GetCartonAreaInfo(model.AreaId));
            return PartialView(MVC_CartonAreas.CartonAreas.Home.Views._areaInfoPartial, cartonarea);
        }

        /// <summary>
        /// Unassign SKU and VWh from location
        /// </summary>
        /// <param name="locationId"></param>
        /// <param name="areaId"></param>
        /// <returns></returns>
        [HttpPost]
        public virtual ActionResult UnassignLocation(string locationId, string areaId)
        {
            _service.UnassignSkuFromlocation(locationId);
            Response.StatusCode = 200;

            // Update carton area info in areaInfo table.
            var cartonarea = Map(_service.GetCartonAreaInfo(areaId));
            return PartialView(MVC_CartonAreas.CartonAreas.Home.Views._areaInfoPartial, cartonarea);
        }

        /// <summary>
        /// Creates and populates ManageCartonAreaViewModel
        /// </summary>
        /// <param name="areaId"></param>
        /// <param name="assigned"></param>
        /// <param name="emptyLocations"></param>
        /// <param name="assignedSkuId"></param>
        /// <param name="skuEntry"></param>
        /// <param name="locationId"></param>
        /// <returns></returns>
        private ActionResult DoManageCartonArea(string areaId, bool? assigned, bool? emptyLocations, int? assignedSkuId, string skuEntry, string locationId)
        {
            var area = _service.GetCartonAreaInfo(areaId);
            if (string.IsNullOrEmpty(areaId))
            {
                this.AddStatusMessage(string.Format("Invalid carton area {0}", areaId));
                return RedirectToAction(Actions.Index());
            }
            var model = new ManageCartonAreaViewModel
            {
                CurrentArea = Map(area),
                AssignedSkuId = assignedSkuId,
                AssignedSkuText = skuEntry,
                LocationId = locationId,
            };
            model.CurrentArea.AssignedLocationsFlag = assigned;
            model.CurrentArea.EmptyLocationsFlag = emptyLocations;
            if (ModelState.IsValid)
            {
                var filterModel = new LocationFilter
                {
                    CartonAreaId = areaId,
                    AssignedLocations = assigned,
                    EmptyLocations = emptyLocations,
                    SkuId = assignedSkuId,
                    SkuEntry = skuEntry,
                    LocationId = locationId
                };
                var locations = _service.GetLocations(filterModel);
                if (locations.Count() == 0)
                {
                    AddStatusMessage("No location found");
                }
                model.Locations = locations.Select(p => new LocationViewModel()
                    {
                        AssignedSku = p.AssignedSku == null ? null : new SkuModel
                            {
                                Style = p.AssignedSku.Style,
                                Color = p.AssignedSku.Color,
                                Dimension = p.AssignedSku.Dimension,
                                SkuSize = p.AssignedSku.SkuSize,
                                SkuId = p.AssignedSku.SkuId,
                                UpcCode = p.AssignedSku.UpcCode
                            },
                        CartonSku = p.CartonSku == null ? null : new SkuModel
                                                        {
                                                            Style = p.CartonSku.Style,
                                                            Color = p.CartonSku.Color,
                                                            Dimension = p.CartonSku.Dimension,
                                                            SkuSize = p.CartonSku.SkuSize,
                                                            SkuId = p.CartonSku.SkuId,
                                                            UpcCode = p.CartonSku.UpcCode
                                                        },
                        CartonCount = p.CartonCount,
                        PalletCount = p.PalletCount,
                        AssignedVwhId = p.AssignedVwhId,
                        TotalPieces = p.TotalPieces,
                        LocationId = p.LocationId,
                        MaxAssignedCartons = p.MaxAssignedCarton,
                        CartonSkuCount = p.CartonSkuCount
                    });
            }
            model.AssignedSku = new AssignSkuViewModel
            {
                VwhList = _service.GetVwhList().Select(p => new SelectListItem
                {
                    Text = (string.IsNullOrEmpty(p.Code) ? "NULL" : p.Code) + " : " + p.Description,
                    Value = p.Code
                })
            };
            return View(Views.ManageCartonArea, model);
        }

        public virtual ActionResult Tutorial()
        {
            return View(Views.Tutorial);
        }
    }
}



//$Id$ 
//$Revision$
//$URL$
//$Header$
//$Author$
//$Date$
