using System;
using System.Collections.Generic;
using System.Configuration.Provider;
using System.Linq;
using System.Web.Routing;
using DcmsMobile.CartonAreas.Models;
using DcmsMobile.CartonAreas.ViewModels;

namespace DcmsMobile.CartonAreas.Repository
{

    public class CartonAreasService : IDisposable
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
        public void AssignSkuToLocation(string locationId, int? skuId, int? maxCartons, string vwhId)
        {
            if (string.IsNullOrEmpty(locationId))
                throw new ArgumentNullException("", "Location ID can not be null");
            if (_repos.AssignSkuToLocation(locationId, skuId, maxCartons, vwhId) <= 0)
            {
                throw new ProviderException(string.Format("Can't update location, please verify location #{0}", locationId));
            }
        }

        /// <summary>
        /// Unassign SKU and VWh for location.
        /// </summary>
        /// <param name="locationId"></param>
        public void UnassignSkuFromlocation(string locationId)
        {
            if (_repos.AssignSkuToLocation(locationId, null, null, null) <= 0)
            {
                throw new ProviderException(string.Format("Can't update location, please verify location #{0}", locationId));
            }
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

        /// <summary>
        /// Find the list of location in any one area.
        /// </summary>
        /// <param name="filters"></param>
        /// <returns></returns>
        public IEnumerable<Location> GetLocations(LocationFilter filters)
        {
            return _repos.GetLocations(filters);
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
            return _repos.GetBuildings(buildingId).First();
        }

        public void UpdatePalletLimit(string buildingId, int? palletLimit)
        {
            _repos.UpdatePalletLimit(buildingId, palletLimit);
        }


        public void UpdateAddress(UpdateAddressOfBuilding building)
        {
            _repos.UpdateAddress(building);
        }


        public void AddBuilding(Building building)
        {
            _repos.AddBuilding(building);
        }
    }
}
//$Id$