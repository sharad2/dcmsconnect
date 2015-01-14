using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration.Provider;
using System.Linq;
using System.Runtime.Caching;
using System.Web;
using System.Web.Routing;


namespace DcmsMobile.Receiving.Areas.Receiving.Home.Repository
{

    internal class ReceivingService : IDisposable
    {
        private readonly ReceivingRepository _repos;

        /// <summary>
        /// This is an always increasing cache which expires every 60 min.
        /// This cache also stores a list of SKUs for which e-mail has been sent.
        /// </summary>
        private const string APPKEY_PROCESSINFO = "ReceivingService_ProcessInfo";

        /// <summary>
        /// Stores the destination area for each intransit carton. The destination will either be the receiving area or the spot check area.
        /// By storing the destination, we ensure that the destination does not change if the carton is scanned again. The entry is removed after the carton is received.
        /// </summary>
        private const string APPKEY_INTRANSIT = "ReceivingService_IntransitDestinationAreas";

        /// <summary>
        /// Stores disposition of scanned pallet.
        /// </summary>
        private const string APPKEY_PALLETDISPOS = "ReceivingService_PalletDisposition";

        private TraceContext _trace;

        /// <summary>
        /// For unit tests. 
        /// </summary>
        public ReceivingService(ReceivingRepository repos)
        {
            _repos = repos;
        }


        /// <summary>
        /// Used to store destination area of intransit cartons until they are received
        /// </summary>
        //private readonly HttpSessionStateBase _session;
        public ReceivingService(RequestContext ctx)
        {
            _repos = new ReceivingRepository(ctx);
            _trace = ctx.HttpContext.Trace;
        }

        public void Dispose()
        {
            var dis = _repos as IDisposable;
            if (dis != null)
            {
                dis.Dispose();
            }
        }

        const int PALLET_LIMIT = 50;    // Factory default

        private class Disposition
        {
            public Disposition(ReceivedCarton ctn)
            {
                this.AreaId = ctn.DestinationArea;
                this.VwhId = ctn.VwhId;
            }
            public string AreaId { get; set; }

            public string VwhId { get; set; }
        }

        /// <summary>
        /// Cache Pallet Dispostion
        /// Key is pallet_id, value is disposition
        /// </summary>
        private ConcurrentDictionary<string, Disposition> CachedPalletDisposition
        {
            get
            {
                var palletDispos = MemoryCache.Default[APPKEY_PALLETDISPOS] as ConcurrentDictionary<string, Disposition>;
                if (palletDispos == null)
                {
                    palletDispos = new ConcurrentDictionary<string, Disposition>();
                    MemoryCache.Default.Add(APPKEY_PALLETDISPOS, palletDispos, new CacheItemPolicy
                    {
                        AbsoluteExpiration = DateTime.Now.AddMinutes(30)
                    });
                }
                return palletDispos;
            }
        }

        private class ProcessModelCollection : KeyedCollection<int, ReceivingProcess>
        {
            protected override int GetKeyForItem(ReceivingProcess item)
            {
                return item.ProcessId;
            }
        }

        private ProcessModelCollection ProcessCache
        {
            get
            {
                var set = MemoryCache.Default[APPKEY_PROCESSINFO] as ProcessModelCollection;
                if (set == null)
                {
                    set = new ProcessModelCollection();
                    MemoryCache.Default.Add(APPKEY_PROCESSINFO, set, new CacheItemPolicy
                    {
                        AbsoluteExpiration = DateTime.Now.AddMinutes(60)
                    });
                }
                return set;
            }
        }


        private string GetDestinationArea(int processId, IntransitCarton ctn)
        {
            var rand = new Random();
            var process = GetProcessInfo(processId);
            if (ctn.IsSpotCheckEnabled && ctn.SpotCheckPercent.HasValue && rand.Next(100 * 100 - 1) < (Convert.ToInt32(ctn.SpotCheckPercent.Value * 100)))
            {
                // Needs spot check                 
                return process.SpotCheckAreaId;
            }

            var areaId = _repos.GetCartonDestination(ctn.CartonId);
            return string.IsNullOrEmpty(areaId) ? process.ReceivingAreaId : areaId;

        }


        /// <summary>
        /// Returns a list of cartons
        /// </summary>
        /// <param name="palletId">Cartons on this pallet</param>
        /// <param name="processId">Cartons of this process</param>
        /// <returns></returns>
        public IList<ReceivedCarton> GetUnpalletizedCartons(int processId)
        {
            return _repos.GetUnpalletizedCartons(processId);
        }

        public IList<string> GetPalletsOfProcess(int processId)
        {
            return _repos.GetPalletsOfProcess(processId);
        }

        public IList<ReceivedCarton> GetCartonsOfPallet(string palletId)
        {
            if (string.IsNullOrWhiteSpace(palletId)) throw new ArgumentNullException("palletId");

            return _repos.GetReceivedCartons2(palletId, null);
        }



        public IEnumerable<ReceivingProcess> GetRecentProcesses()
        {
            return _repos.GetProcesses(null);
        }


        /// <summary>
        /// 
        /// We cache the process info since it is heavily used
        /// </summary>
        /// <param name="processId"></param>
        /// <param name="clearCache">If true <paramref name="processId"/> will be removed from cache, and fresh info will be get through repos.</param>
        /// <returns></returns>
        public ReceivingProcess GetProcessInfo(int processId, bool clearCache = false)
        {
            //Removing the process info from cache because it has become stale.
            if (clearCache)
            {
                ProcessCache.Remove(processId);
            }


            if (ProcessCache.Contains(processId))
            {
                _trace.Warn(string.Format("Process Information for Process {0} retrieved from application cache", processId));
                return ProcessCache[processId];
            }
            var process = _repos.GetProcesses(processId).FirstOrDefault();
            if (process != null)
            {
                ProcessCache.Add(process);
            }
            return process;
        }

        public int InsertProcess(ReceivingProcess processModel)
        {
            if (processModel.ProcessId != 0)
            {
                throw new ArgumentOutOfRangeException("processModel.ProcessId", "Process Id cannot be specified");
            }
            var processId = _repos.InsertProcess(processModel);

            processModel.ProcessId = processId;
            return processId;
        }

        /// <summary>
        /// Pass new values in processModel
        /// </summary>
        public void UpdateProcess(ReceivingProcess processModel)
        {


            if (processModel.ProcessId <= 0)
            {
                throw new ArgumentOutOfRangeException("processModel.ProcessId", "Process Id must be positive");
            }
            _repos.UpdateProcess(processModel);
            ProcessCache.Remove(processModel.ProcessId);

        }

        /// <summary>
        /// Returns palletId
        /// </summary>
        /// <param name="cartonId"></param>
        /// <param name="processId"></param>
        /// <param name="palletId"></param>
        /// <returns></returns>
        public void RemoveFromPallet(string cartonId, int processId, out string palletId)
        {
            palletId = _repos.RemoveFromPallet(cartonId);
            Disposition dispos;
            CachedPalletDisposition.TryRemove(palletId, out dispos);

        }

        public void PrintCarton(string cartonId, string printer)
        {
            _repos.PrintCarton(cartonId, printer);
        }

        public IList<Tuple<string, string>> GetPrinters()
        {
            return _repos.GetPrinters();
        }


        public int GetPalletLimit(int processId)
        {

            int? limit = null;

            if (ProcessCache.Contains(processId))
            {
                limit = ProcessCache[processId].PalletLimit;
            }


            return limit.HasValue ? limit.Value : PALLET_LIMIT;

        }

        public void ReceiveCarton(string scan, string palletId, int processId)
        {
            if (string.IsNullOrWhiteSpace(scan))
            {
                // Nothing to do
                return;
            }
            if (string.IsNullOrWhiteSpace(palletId))
            {
                throw new ArgumentNullException("palletId");
            }

            var cartonToReceive = _repos.GetIntransitCarton2(scan);

            if (cartonToReceive != null)
            {
                // Normal case
                if (cartonToReceive.IsShipmentClosed)
                {
                    // This carton is from an already closed shipment. check whether we can accept it.                 
                    if (!_repos.AcceptCloseShipmentCtn(scan, processId))
                    {
                        throw new ProviderException(string.Format("Carton {0} belongs to a closed shipment.Scan after carton for open shipment or use blind receiving.", scan));
                    }
                }
                var areaId = GetDestinationArea(processId, cartonToReceive);
                HandleDisposition(palletId, areaId, cartonToReceive.VwhId);
                // Unreceived carton.
                _repos.ReceiveCarton(palletId, cartonToReceive.CartonId, areaId, processId);
                return;
            }


            // See whether this carton has already been received
            var carton = _repos.GetReceivedCartons2(null, scan).FirstOrDefault();

            if (carton == null)
            {
                throw new ProviderException(string.Format("Carton {0} not part of ASN", scan));
            }
            //if (carton.InShipmentId != processId)
            //{
            //    //carton has already been received throw exception with pallet Id on which carton was put.
            //    throw new Exception(string.Format("Carton {0} has already been received by Receiving Process {1}", carton.CartonId, carton.InShipmentId));
            //}
            // Put carton on pallet and check disposition. 
            HandleDisposition(palletId, carton.DestinationArea, carton.VwhId);
            // If we get here, Dipos ok put carton on pallet
            _repos.PutCartonOnPallet(palletId, scan);
        }





        // PalletId, ProcessId, destArea, cartonId, 
        private void HandleDisposition(string palletId, string areaId, string vwhId)
        {

            if (string.IsNullOrEmpty(palletId))
            {
                throw new ArgumentNullException("palletId");

            }

            //var palletDisposition = GetPalletDisposition(palletId);

            Disposition palletDispos;

            if (!CachedPalletDisposition.TryGetValue(palletId, out palletDispos))
            {
                var cartons = _repos.GetReceivedCartons2(palletId, null);
                palletDispos = cartons.Select(p => new Disposition(p)).FirstOrDefault();
                if (palletDispos != null)
                {
                    CachedPalletDisposition.TryAdd(palletId, palletDispos);
                }
            }

            if (palletDispos == null)
            {
                return;
            }
            if (palletDispos.AreaId != areaId)
            {
                throw new Exception(string.Format("Carton for area {0} cannot be placed on pallet for area {1}", areaId, palletDispos.AreaId));
            }
            if (palletDispos.VwhId != vwhId)
            {
                throw new Exception(string.Format("Carton of Vwh {0} cannot be placed on pallet of Vwh {1}", vwhId, palletDispos.VwhId));
            }

        }



        public IEnumerable<CartonArea> GetCartonAreas()
        {
            return _repos.GetCartonAreas();
        }


        /// <summary>
        /// Get the List of PriceSeasson Code.
        /// </summary>
        /// <returns></returns>
        public IList<Tuple<string, string>> GetPriceSeasonCodes()
        {
            return _repos.GetPriceSeasonCodes();
        }

        public IList<ShipmentList> GetShipmentList()
        {
            return _repos.GetShipmentList();
        }

        /// <summary>
        /// Close passed Shipment
        /// </summary>
        /// <param name="shipmentId"></param>
        /// <param name="poId"></param>
        public void CloseShipment(string shipmentId, long? poId)
        {
            _repos.CloseShipment(shipmentId, poId);
        }

        /// <summary>
        /// Reopen passed shipment
        /// </summary>
        /// <param name="shipmentId"></param>
        /// <param name="poId"></param>
        public bool ReOpenShipment(string shipmentId, long? poId)
        {

            return _repos.ReOpenShipment(shipmentId, poId);
        }

        /// <summary>
        /// To validate carrier.
        /// </summary>
        /// <param name="carrierId"></param>
        /// <returns></returns>
        public Tuple<string, string> GetCarrier(string carrierId)
        {
            return _repos.GetCarrier(carrierId);
        }


        internal IList<Tuple<string, string>> GetCarriers(string searchId, string searchDescription)
        {
            //throw new Exception(searchId + "*" + searchDescription);
            return _repos.GetCarriers(searchId, searchDescription);
        }
    }
}


//$Id$