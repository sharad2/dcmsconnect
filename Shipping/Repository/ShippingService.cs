
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Caching;
using System.Web;

namespace DcmsMobile.Shipping.Repository
{
    public class ShippingService : IDisposable
    {

        #region Intialization

        private readonly ShippingRepository _repos;
        private readonly string _userName;

        public ShippingService(TraceContext ctx, string connectString, string userName, string clientInfo)
        {
            _userName = userName;
#if DEBUG
            if (userName.StartsWith("_"))
            {
                // This is a dummy user. Don't let the repository know about this
                userName = "";
            }
#endif
            _repos = new ShippingRepository(ctx, connectString, userName, clientInfo);
        }

        public void Dispose()
        {
            _repos.Dispose();
        }


        #endregion
        /// <summary>
        /// Gets unrouted orders based on the passed filters. If no filter is passed we bring all unrouted orders. 
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="dcCancelDate"></param>
        /// <param name="startDate"></param>
        /// <param name="buildingId"></param>
        /// <returns></returns>
        public IEnumerable<Po> GetUnRoutedOrders(string customerId, string buildingId, bool orderType)
        {
            return _repos.GetUnroutedOrders(customerId, buildingId, orderType);
        }

        /// <summary>
        /// unroute selected orders
        /// </summary>
        /// <param name="poList"></param>
        /// <returns></returns>
        internal void UndoRoute(RoutingKey key)
        {
            _repos.UndoRouting(key);
            CustomerOrderSummaryCache.Remove(key.CustomerId);

        }

        /// <summary>
        /// Creates EDI753 for the passed POs on the passed ATS date.
        /// </summary>
        /// <param name="poList"></param>
        /// <param name="atsDate"></param>
        /// <returns></returns>
        /// <remarks>POs are added to existing EDI, if ATS date already exists for the customer</remarks>
        internal void AddPoToEdi(string customerId, ICollection<Tuple<string, int, string>> poList, DateTime atsDate,bool isElectronicEdi)
        {
            // Get list of ATS date already exists for the customer.
            var existingEdiId = _repos.GetExistingAtsDates(customerId, atsDate).Select(p => p.EdiId).FirstOrDefault();

            // Create new edi for POs if ATS date not exists for the customer.
            if (existingEdiId == null)
            {
                var ediId = _repos.CreateEDIforCustomer(customerId);
                _repos.AddPoToEdi(customerId, poList, atsDate, ediId, isElectronicEdi);
            }
            // POs are added to existing EDI, if ATS date already exists for the customer.
            else
            {
                _repos.AddPoToEdi(customerId, poList, atsDate, existingEdiId.Value,isElectronicEdi);
            }

            // Since counts for this customer have changed, remove them from the cache
            CustomerOrderSummaryCache.Remove(customerId);
            
        }
        /// <summary>
        /// Get list of all routing/routed POs.
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="routed">null return all POs, true return routed POs(POs eikter have load or PickUpdate), false:return routing POs</param>
        /// <returns></returns>

        internal IEnumerable<RoutablePo> GetRoutablePos(string customerId, bool? routed, DateTime? startDate, DateTime? dcCancelDate, string buildingId)
        {
            return _repos.GetRoutablePos(customerId, routed, startDate, dcCancelDate, buildingId);
        }

        /// <summary>
        /// returns routing information
        /// </summary>
        /// <returns></returns>
        internal IList<CustomerOrderSummary> GetOrderSummary(RoutingStatus status)
        {
            return _repos.GetOrderSummary(null, status);
        }

        /// <summary>
        /// Update routing info provided by customer
        /// </summary>
        /// <param name="updater"></param>
        /// <returns></returns>
        internal int UpdateRouting(RoutingUpdater updater)
        {
            int count = 0;
            // Since counts for this customer have changed, remove them from the cache
            var cache = CustomerOrderSummaryCache;
            foreach (var customerId in updater.RoutingKeys.Select(p => p.CustomerId).Distinct())
            {
                cache.Remove(customerId);
            }
            // If null value is provided for DC or Carrier, update them with original values for each selected PO.
            if (updater.UpdateCustomerDcId == true && string.IsNullOrEmpty(updater.CustomerDcId) || updater.UpdateCarrierId == true && string.IsNullOrEmpty(updater.CarrierId))
            {
                foreach (var item in updater.RoutingKeys)
                {
                    var newupdater = new RoutingUpdater
                    {
                        RoutingKeys = updater.RoutingKeys.Where(p => p.Key == item.Key).ToArray(),
                        CustomerDcId = updater.CustomerDcId,
                        UpdateCustomerDcId = updater.UpdateCustomerDcId,
                        PickUpDate = updater.PickUpDate,
                        UpdatePickupDate = updater.UpdatePickupDate,
                        LoadId = updater.LoadId,
                        UpdateLoadId = updater.UpdateLoadId,
                        CarrierId = updater.CarrierId,
                        UpdateCarrierId = updater.UpdateCarrierId
                    };
                    var defaultRoutinginfo = _repos.GetDefaultRoutingInfo(item.CustomerId,item.PoId, item.Iteration,item.DcId);
                    if (updater.UpdateCustomerDcId == true && string.IsNullOrEmpty(updater.CustomerDcId))
                    {
                        newupdater.CustomerDcId = defaultRoutinginfo.Item1;
                    }
                    if (updater.UpdateCarrierId == true && string.IsNullOrEmpty(updater.CarrierId))
                    {
                        newupdater.CarrierId = defaultRoutinginfo.Item2;
                    }                
                    count += _repos.UpdateRouting(newupdater);
                }
            }
            else
            {
                count = _repos.UpdateRouting(updater);
            }
            return count;
        }

        /// <summary>
        /// Gets existing ATS dates for passed customer
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="ediId"></param>
        /// <returns></returns>
        internal IEnumerable<AtsDateSummary> GetExistingAtsDates(string customerId)
        {
            return _repos.GetExistingAtsDates(customerId, null);
        }

        /// <summary>
        /// Creates BOL for passed list of  EDI. 
        /// </summary>
        /// <param name="edId"></param>
        /// <remarks>       
        /// </remarks>
        internal int CreateBol(int[] ediIdList, string custId)
        {
            int count = 0;
            CustomerOrderSummaryCache.Remove(custId);
            foreach (var edIId in ediIdList)
            {
                count += _repos.CreateBol(edIId);
            }
            return count;
        }

        /// <summary>
        /// Gets the unshiiped BOLs for the passed customer.
        /// </summary>
        /// <param name="CustomerId"></param>
        /// <returns></returns>
        internal IEnumerable<Bol> GetBols(string customerId, bool showScheduled)
        {
            return _repos.GetBols(customerId, showScheduled);
        }

        /// <summary>
        /// Delete unshipped Bill of ladings.
        /// </summary>
        /// <param name="ShippingIdList"></param>
        /// <returns></returns>
        internal bool DeleteBol(string customerId,string ShippingIdList)
        {
            var shippingIdList1 = ShippingIdList.Split(',');
            try
            {
                foreach (var shippingId in shippingIdList1)
                {
                    _repos.DeleteBol(shippingId);
                }
                // Since counts for this customer have changed, remove them from the cache
                CustomerOrderSummaryCache.Remove(customerId);
                return true;
            }
            catch (Exception) { return false; }
        }

        private class CustomerOrderSummaryCollection : KeyedCollection<string, CustomerOrderSummary>
        {
            protected override string GetKeyForItem(CustomerOrderSummary item)
            {
                return item.CustomerId;
            }
        }

        private CustomerOrderSummaryCollection CustomerOrderSummaryCache
        {
            get
            {
                var CACHE_KEY = typeof(ShippingRepository).FullName;
                var custCache = MemoryCache.Default[CACHE_KEY] as CustomerOrderSummaryCollection;
                if (custCache == null)
                {
                    custCache = new CustomerOrderSummaryCollection();
                    MemoryCache.Default.Add(CACHE_KEY, custCache, DateTime.Now.AddHours(2));
                }
                return custCache;
            }
        }

        /// <summary>
        /// Gets details of open orders of the passed customer.
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        internal CustomerOrderSummary GetCustomerOrderSummary(string customerId)
        {
            if (string.IsNullOrWhiteSpace(customerId))
            {
                throw new ArgumentNullException("customerId");
            }
            var custCache = CustomerOrderSummaryCache;
            CustomerOrderSummary summary;
            if (custCache.Contains(customerId))
            {
                summary = custCache[customerId];
            }
            else
            {
                summary = _repos.GetOrderSummary(customerId, RoutingStatus.Notset).FirstOrDefault();
                if (summary != null)
                {
                    custCache.Add(summary);
                }
            }
            return summary;
        }

        /// <summary>
        /// Return appointment against passesd appointmentId
        /// </summary>
        /// <param name="appointmentId"></param>
        /// <returns></returns>
        internal Appointment GetAppointmentById(int appointmentId)
        {
            return _repos.GetAppointments(appointmentId: appointmentId).FirstOrDefault();
        }

        /// <summary>
        /// Return appointment against passed appointment number
        /// </summary>
        /// <param name="appointmentNumber"></param>
        /// <returns></returns>
        internal Appointment GetAppointmentByNumber(int appointmentNumber)
        {
            return _repos.GetAppointments(appointmentNumber: appointmentNumber,shipped:true).FirstOrDefault();
        }

        /// <summary>
        /// Return list of appointment against passed parameters
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="buildingIdList"></param>
        /// <param name="customerId"></param>
        /// <param name="carrierId"></param>
        /// <param name="scheduled"></param>
        /// <param name="shipped"></param>
        /// <returns></returns>
        internal IEnumerable<Appointment> GetAppointments(DateTimeOffset? startDate, DateTimeOffset? endDate, string[] buildingIdList, string customerId, string carrierId, bool? scheduled, bool? shipped)
        {
            return _repos.GetAppointments(startDate: startDate, endDate: endDate, buildingIdList: buildingIdList, customerId: customerId, carrierId: carrierId, scheduled: scheduled, shipped: shipped, maxRows: 1000);
        }

        /// <summary>
        /// create  new appointment
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        internal void CreateAppointment(Appointment app)
        {
            _repos.CreateAppointment(app);
        }

        /// <summary>
        /// update existing appointment
        /// </summary>
        /// <param name="app"></param>
        internal void UpdateAppointment(Appointment app)
        {
            _repos.UpdateAppointment(app);
        }

        /// <summary>
        /// Delete appointment
        /// </summary>
        /// <param name="app"></param>
        internal void DeleteAppointment(int appointmentId)
        {
            _repos.DeleteAppointment(appointmentId);
        }

        /// <summary>
        /// Set appointment id to passed shipping id list.
        /// </summary>
        /// <param name="shippingIdList"></param>
        /// <param name="appointmentId"></param>
        /// <returns></returns>
        internal void AssignAppointmentToBol(ICollection<string> shippingIdList, int appointmentId)
        {
            _repos.AssignAppointmentToBol(shippingIdList, appointmentId);
        }

        /// <summary>
        /// TODO:Cache building list.
        /// </summary>
        /// <returns></returns>
        internal IEnumerable<CodeDescriptionModel> GetBuildingList()
        {
            return _repos.GetBuildingList();
        }

        /// <summary>
        /// Get detailed list of BOls assigened to passesd appointment list
        /// </summary>
        /// <param name="appointmentIdlist"></param>
        /// <returns></returns>
        internal IEnumerable<AppointmentBol> GetScheduledBols(IEnumerable<int> appointmentIdlist, bool? shipped)
        {
            if (!appointmentIdlist.Any())
            {
                // Avoid the query
                return Enumerable.Empty<AppointmentBol>();
            }
            return _repos.GetScheduledAppointmentBols(appointmentIdlist, shipped);
        }

        internal IEnumerable<AppointmentBol> GetUnscheduledBols(DateTimeOffset from, DateTimeOffset to, bool? shipped)
        {
            return _repos.GetUnscheduledAppointmentBols(from, to, shipped);
        }

        /// <summary>
        /// Returns latest appointment of customer
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        internal Appointment GetLastAppointmentOfCustomer(string customerId)
        {
            return _repos.GetAppointments(customerId: customerId, maxRows: 1).FirstOrDefault();
        }

        /// <summary>
        /// updates truck arrival time
        /// </summary>
        /// <param name="appointmentId"></param>
        /// <param name="arrivalTime"></param>
        internal void UpdateArrivalTime(int appointmentId, TimeSpan? arrivalTime)
        {
            _repos.UpdateArrivalTime(appointmentId, arrivalTime);
        }

        /// <summary>
        /// Unassign appointment from passed list of BOLs
        /// </summary>
        /// <param name="selectedBols"></param>
        internal void UnAssignAppointment(ICollection<string> selectedBols)
        {
            _repos.UnAssignAppointment(selectedBols);
        }

        /// <summary>
        /// Returns list of POs associated with passed EDI
        /// </summary>
        /// <param name="ediId"></param>
        /// <returns></returns>
        internal ICollection<EdiPo> EdiSummary(int[] ediId)
        {
            return _repos.EdiSummary(ediId);
        }
   

        internal string GetDC(string customerId, string customerDC)
        {
            return _repos.GetDC(customerId, customerDC);
        }

        /// <summary>
        /// Returns Status(Unrouted,Routed) of Passed Po along with other info
        /// </summary>
        /// <param name="poPattern"></param>
        /// <returns></returns>
        public IList<PoStatus> GetPoStatus(string poPattern)
        {
            return _repos.GetPoStatus(poPattern);
        }
    }
}