using System;
using System.Collections.Generic;
using System.Configuration.Provider;
using System.Linq;
using System.Web.Routing;

namespace DcmsMobile.CartonAreas.Repository
{

    internal class CartonAreasService : IDisposable
    {

        #region Intialization
        private readonly CartonAreasRepository _repos;

        public CartonAreasService(RequestContext ctx)
        {
            string module = ctx.HttpContext.Request.Url == null ? "CartonAreas" : ctx.HttpContext.Request.Url.AbsoluteUri;
            var clientInfo = string.IsNullOrEmpty(ctx.HttpContext.Request.UserHostName) ?
                             ctx.HttpContext.Request.UserHostAddress : ctx.HttpContext.Request.UserHostName;

            _repos = new CartonAreasRepository(ctx.HttpContext.User.Identity.Name, module, clientInfo, ctx.HttpContext.Trace);
        }

        public void Dispose()
        {
            _repos.Dispose();
        }

        #endregion

        /// <summary>
        /// This method is used for update location.
        /// </summary>
        /// <param name="locationId"></param>
        /// <param name="skuId"></param>
        /// <param name="maxCartons"></param>
        /// <param name="vwhId"></param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public string AssignSkuToLocation(string locationId, int? skuId, int? maxCartons, string vwhId)
        {
            return _repos.AssignSkuToLocation(locationId, skuId, maxCartons, vwhId);
        }

        /// <summary>
        /// Unassign SKU and VWh for location.
        /// </summary>
        /// <param name="locationId"></param>
        public string UnassignSkuFromlocation(string locationId)
        {
            return _repos.AssignSkuToLocation(locationId, null, null, null);
        }

        /// <summary>
        /// Give the list of carton area.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<CartonArea> GetCartonAreas(string buildingId)
        {
            return _repos.GetCartonAreas(null, buildingId);
        }

        /// <summary>
        /// This method is used for find the all information of any one Area.
        /// </summary>
        /// <param name="areaId"></param>
        /// <returns></returns>
        public CartonArea GetCartonAreaInfo(string areaId)
        {
            return _repos.GetCartonAreas(areaId, null).FirstOrDefault();
        }

        ///// <summary>
        ///// Find the list of location in any one area.
        ///// </summary>
        ///// <param name="filters"></param>
        ///// <returns></returns>
        //[Obsolete]
        //public IEnumerable<Location> GetLocations(LocationFilter filters)
        //{
        //    throw new NotImplementedException();
        //    //return _repos.GetLocations(filters);
        //}

        public IList<Location> GetLocationsMatchingPattern(string locationPattern)
        {
            if (string.IsNullOrWhiteSpace(locationPattern))
            {
                throw new ArgumentNullException("locationPattern");
            }
            return _repos.GetLocations(locationPattern.Replace('*', '%'), null, null, null, null);
        }

        public IList<Location> GetLocationsAssignedToSku(string cartonAreaId, int skuId)
        {
            return _repos.GetLocations(null, skuId, null, null, cartonAreaId);
        }

        public IList<Location> GetLocations(string cartonAreaId, bool? assignedLocations, bool? emptyLocation)
        {
            return _repos.GetLocations(null, null, assignedLocations, emptyLocation, cartonAreaId);
        }

        /// <summary>
        ///  This method give the list of VWhId
        /// </summary>
        /// <returns></returns>
        public IEnumerable<CodeDescriptionModel> GetVwhList()
        {
            return _repos.GetVwhList();
        }

        /// <summary>
        /// This method is used to update area information.
        /// </summary>
        /// <param name="model"></param>
        public void UpdateArea(CartonArea model)
        {
            _repos.UpdateArea(model);
        }

        internal IList<Building> GetBuildings()
        {
            return _repos.GetBuildings(null);
        }

        internal Building GetBuilding(string buildingId)
        {
            var building = _repos.GetBuildings(buildingId);
            return building.Count == 0 ? null : building.First();
        }

        public void UpdatePalletLimit(string buildingId, int? palletLimit)
        {
            _repos.UpdatePalletLimit(buildingId, palletLimit);
        }

        public void UpdateAddress(string buildingId, string description, Address address)
        {
            _repos.UpdateAddress(buildingId, description, address);
        }

        public void AddBuilding(Building building)
        {
            _repos.AddBuilding(building);
        }

        internal IList<CodeDescriptionModel> GetCountryList()
        {
            return _repos.GetCountryList();
        }

        internal IList<PickingArea> GetPickingAreas(string buildingId)
        {
            return _repos.GetPickingAreas(buildingId);
        }


        public void UpdatePickingArea(PickingArea model)
        {
            _repos.UpdatePickingArea(model);
        }
    }
}
//$Id$