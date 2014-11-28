using System;
using System.Collections.Generic;
using System.Web;

namespace DcmsMobile.Inquiry.Areas.Inquiry.IntransitEntity
{
    internal class IntransitEntityService : IDisposable
    {
        #region Intialization

        private readonly IntransityEntityRepository _repos;

        public IntransitEntityService(TraceContext ctx, string connectString, string userName, string clientInfo)
        {
            _repos = new IntransityEntityRepository(ctx, connectString, userName, clientInfo);
        }

        public void Dispose()
        {
            _repos.Dispose();
        }


        #endregion

        /// <summary>
        /// Shipments list
        /// </summary>
        /// <param name="maxCloseDate"></param>
        /// <param name="minCloseDate"></param>
        /// <param name="statusFilter"></param>
        /// <remarks>
        /// If both date minCloseDate and maxCloseDate are same,than we shows shipments of minCloseDate.
        /// </remarks>
        /// <returns></returns>
        public IList<IntransitShipment> GetInboundShipmentSummary(DateTime? minCloseDate, DateTime? maxCloseDate, ShipmentFilters statusFilter, string sewingPlant, int nRowsToShow)
        {
            // If both min and max are specified and min > max, then swap the dates
            if (minCloseDate.HasValue && maxCloseDate.HasValue && minCloseDate > maxCloseDate)
            {
                var temp = minCloseDate;
                minCloseDate = maxCloseDate;
                maxCloseDate = temp;
            }
            return _repos.GetInboundShipmentSummary(minCloseDate == null ? (DateTime?)null : minCloseDate.Value.Date,
                maxCloseDate == null ? (DateTime?)null : maxCloseDate.Value.Date.AddDays(1).AddSeconds(-1),
                statusFilter, sewingPlant, nRowsToShow);
        }


        internal IList<IntransitShipmentSkuSummary> GetInboundShipmentSkuDetail(ShipmentSkuFilters statusFilter, DateTime? minCloseDate, DateTime? maxCloseDate, string sewingPlant, int nRowsToShow)
        {
            // If both min and max are specified and min > max, then swap the dates
            if (minCloseDate.HasValue && maxCloseDate.HasValue && minCloseDate > maxCloseDate)
            {
                var temp = minCloseDate;
                minCloseDate = maxCloseDate;
                maxCloseDate = temp;
            }

            return _repos.GetInboundShipmentSkuDetail(statusFilter, minCloseDate == null ? (DateTime?)null : minCloseDate.Value.Date,
             maxCloseDate == null ? (DateTime?)null : maxCloseDate.Value.Date.AddDays(1).AddSeconds(-1),
              sewingPlant, nRowsToShow);
        }

        internal IList<SewingPlant> GetSewingPlantList()
        {
            return _repos.GetSewingPlantList();
        }

        internal IList<IntransitShipmentSku> GetInboundShipmentInfo(string id)
        {
            return _repos.GetInboundShipmentInfo(id);
        }

    }
}