using System;
using System.Collections.Generic;
using System.Configuration.Provider;
using System.Data.Common;
using System.Linq;
using System.Runtime.Caching;
using System.Web.Routing;
using DcmsMobile.CartonManager.Models;
using DcmsMobile.CartonManager.ViewModels;

namespace DcmsMobile.CartonManager.Repository
{
    public class CartonManagerService : IDisposable
    {
        #region Intialization

        private readonly CartonManagerRepository _repos;

        public CartonManagerService(RequestContext ctx)
        {
            string module = "CartonEditor";
            var clientInfo = string.IsNullOrEmpty(ctx.HttpContext.Request.UserHostName) ?
                             ctx.HttpContext.Request.UserHostAddress : ctx.HttpContext.Request.UserHostName;

            _repos = new CartonManagerRepository(ctx.HttpContext.User.Identity.Name, module, clientInfo, ctx.HttpContext.Trace);
        }

        public void Dispose()
        {
            _repos.Dispose();
        }

        #endregion

        /// <summary>
        /// Gets all carton areas if you do not pass any parameter.
        /// If you pass a flag gets area according to passed flag. 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<CartonArea> GetCartonAreas(string areaShortName, string buildingId)
        {
            return _repos.GetCartonAreas(null, areaShortName, buildingId);
        }

        public CartonArea GetCartonArea(string areaId)
        {
            return _repos.GetCartonAreas(areaId, null, null).FirstOrDefault();
        }

        public IEnumerable<CodeDescriptionModel> GetVwhList()
        {
            return _repos.GetVwhList();
        }

        public IEnumerable<CodeDescriptionModel> GetQualityCodes()
        {
            return _repos.GetQualityCodes();
        }

        public IEnumerable<CodeDescriptionModel> GetZebraPrinters()
        {
            return _repos.GetZebraPrinters();
        }

        public IEnumerable<CodeDescriptionModel> GetReasonCodes()
        {
            return _repos.GetRichterReasonCodes();
        }

        /// <summary>
        /// Pass in what you want update. Get back what actually needs updating. The returned set of flags will alwyays be
        /// a subset of the passed flags.
        /// </summary>
        /// <param name="currentCarton"></param>
        /// <param name="modifiedCarton"></param>
        /// <param name="updateFlags"></param>
        /// <returns></returns>
        /// <remarks>
        /// modifiedCarton.CartonId is not looked at. It is assumed to be the same as currentCarton.CartonId
        /// </remarks>
        private CartonUpdateFlags GetModifications(Carton currentCarton, Carton modifiedCarton, CartonUpdateFlags updateFlags)
        {
            if (modifiedCarton == null)
            {
                throw new ArgumentNullException("modifiedCarton");
            }

            if (currentCarton == null || string.IsNullOrWhiteSpace(currentCarton.CartonId))
            {
                throw new ArgumentNullException("currentCarton", "Current carton or its id cannot be null");
            }

            if (!string.IsNullOrEmpty(modifiedCarton.CartonArea.AreaId) && GetCartonArea(modifiedCarton.CartonArea.AreaId).IsNumberedLocationArea &&
                string.IsNullOrWhiteSpace(modifiedCarton.LocationId))
            {
                throw new ProviderException("Location is required for numbered areas. ");
            }
            if (updateFlags.HasFlag(CartonUpdateFlags.MarkReworkComplete) && updateFlags.HasFlag(CartonUpdateFlags.AbandonRework))
            {
                throw new ProviderException("Mark rework complete and abandon rework can not be performed on same carton. ");
            }

            var returnFlags = CartonUpdateFlags.None;
            if (updateFlags.HasFlag(CartonUpdateFlags.Sku) && currentCarton.SkuInCarton.SkuId != modifiedCarton.SkuInCarton.SkuId)
            {
                returnFlags |= CartonUpdateFlags.Sku;
            }
            if (updateFlags.HasFlag(CartonUpdateFlags.Quality) && currentCarton.QualityCode != modifiedCarton.QualityCode)
            {
                returnFlags |= CartonUpdateFlags.Quality;
            }
            if (updateFlags.HasFlag(CartonUpdateFlags.Pieces) && currentCarton.Pieces != modifiedCarton.Pieces)
            {
                returnFlags |= CartonUpdateFlags.Pieces;
            }
            if (updateFlags.HasFlag(CartonUpdateFlags.Vwh) && currentCarton.VwhId != modifiedCarton.VwhId)
            {
                returnFlags |= CartonUpdateFlags.Vwh;
            }
            if (updateFlags.HasFlag(CartonUpdateFlags.Pallet) && currentCarton.PalletId != modifiedCarton.PalletId)
            {
                returnFlags |= CartonUpdateFlags.Pallet;
            }
            if (updateFlags.HasFlag(CartonUpdateFlags.Area) && currentCarton.CartonArea.AreaId != modifiedCarton.CartonArea.AreaId)
            {
                returnFlags |= CartonUpdateFlags.Area;
            }
            if (updateFlags.HasFlag(CartonUpdateFlags.Location) && currentCarton.LocationId != modifiedCarton.LocationId)
            {
                returnFlags |= CartonUpdateFlags.Location;
            }
            if (updateFlags.HasFlag(CartonUpdateFlags.PriceSeasonCode) && currentCarton.PriceSeasonCode != modifiedCarton.PriceSeasonCode)
            {
                returnFlags |= CartonUpdateFlags.PriceSeasonCode;
            }
            if (updateFlags.HasFlag(CartonUpdateFlags.MarkReworkComplete) && currentCarton.RemarkWorkNeeded == true)
            {
                returnFlags |= CartonUpdateFlags.MarkReworkComplete;
            }
            if (updateFlags.HasFlag(CartonUpdateFlags.AbandonRework) && currentCarton.RemarkWorkNeeded == true)
            {
                returnFlags |= CartonUpdateFlags.AbandonRework;
            }
            if (updateFlags.HasFlag(CartonUpdateFlags.RemovePallet) && !string.IsNullOrEmpty(currentCarton.PalletId))
            {
                returnFlags |= CartonUpdateFlags.RemovePallet;
            }
            return returnFlags;
        }

        /// <summary>
        /// Updates carton properties only if the modified values are different from current values
        /// </summary>
        /// <param name="currentCarton">.</param>
        /// <param name="modifiedCarton"></param>
        /// <param name="updateFlags"></param>
        /// <param name="reasonCode"></param>
        /// <returns></returns>
        /// <remarks>
        /// Used for optimization. If the modified value is same as current value, the update query can be avoided
        /// </remarks>
        public bool UpdateMoveCarton(Carton currentCarton, Carton modifiedCarton, CartonUpdateFlags updateFlags, string reasonCode)
        {
            var modifications = this.GetModifications(currentCarton, modifiedCarton, updateFlags);
            if (modifications == CartonUpdateFlags.None)
            {
                // No property of the carton needs to be modified. Bypass checking of qualification rules.
                return false;
            }
            var updated = false;
            if ((modifications & CartonUpdateFlags.UpdateTasks) != CartonUpdateFlags.None)
            {

                modifiedCarton.CartonId = currentCarton.CartonId;
                _repos.UpdateCarton(modifiedCarton, modifications, reasonCode);
                updated = true;
            }
            if ((modifications & CartonUpdateFlags.MoveTasks) != CartonUpdateFlags.None)
            {

                modifiedCarton.CartonId = currentCarton.CartonId;
                _repos.MoveCarton(modifiedCarton, modifications);
                updated = true;
            }
            return updated;
        }

        public void PrintCartonTicket(string scanText, string printerId)
        {
            //If printer name is passed print the carton ticket
            if (string.IsNullOrEmpty(printerId))
            {
                throw new ArgumentNullException("printerId", "Printer Id can't be NULL");
            }
            _repos.PrintCartonTicket(scanText, printerId);
        }

        public Carton GetCarton(string cartonId)
        {
            return _repos.GetCartons(cartonId, null).FirstOrDefault();
        }

        /// <summary>
        /// Cache the SKU since we expect that the same SKU will be queried multiple times
        /// </summary>
        /// <param name="barCode"></param>
        /// <returns></returns>
        public Sku GetSku(string barCode)
        {
            if (string.IsNullOrWhiteSpace(barCode))
            {
                throw new ArgumentNullException("barCode", "Scanned barcode can't be NULL");
            }
            var CACHE_KEY = typeof(CartonManagerRepository).FullName;
            var dictCache = MemoryCache.Default[CACHE_KEY] as IDictionary<string, Sku>;
            if (dictCache == null)
            {
                dictCache = new Dictionary<string, Sku>();
                MemoryCache.Default.Add(CACHE_KEY, dictCache, DateTime.Now.AddHours(2));
            }
            Sku sku;
            if (!dictCache.TryGetValue(barCode, out sku))
            {
                sku = _repos.GetSku(barCode);
                dictCache.Add(barCode, sku);
            }
            return sku;
        }

        public bool IsPallet(string scanText)
        {
            return scanText.StartsWith("P");
        }


        public IEnumerable<Carton> GetCartonsOnPallet(string palletId)
        {
            return _repos.GetCartons(null, palletId);
        }


        public DbTransaction BeginTransaction()
        {
            return _repos.BeginTransaction();
        }

        /// <summary>
        /// Get the List of PriceSeasson Code to view in Advanced screen.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<CodeDescriptionModel> GetPriceSeasonCodes()
        {
            return _repos.GetPriceSeasonCode();
        }

        public Pallet GetPallet(string palletId)
        {
            return _repos.GetPallet(palletId);
        }

        public void RemoveIrregularSamples(string cartonId, string bundleId, string destinationArea, int? pieces, string reasonCode)
        {
            _repos.RemoveIrregularSamples(cartonId, bundleId, destinationArea, pieces, reasonCode);

        }

        public IEnumerable<SkuArea> GetTransferAreas(PiecesRemoveFlag pieceFlag)
        {
            return _repos.GetTransferAreas(pieceFlag);
        }

        public string GetSampleReasonCode()
        {
            return _repos.GetSampleReasonCode();
        }

        public void DeleteEmptyCarton(string cartonId)
        {
            _repos.DeleteEmptyCarton(cartonId);
        }
    }
}



//$Id$