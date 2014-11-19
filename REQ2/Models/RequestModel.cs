
using System;
using System.Collections.Generic;

namespace DcmsMobile.REQ2.Models
{
    public class RequestModel
    {
        
        public string CtnResvId { get; set; }

        /// <summary>
        /// Id of the request in src_req_detail
        /// </summary>
        [Obsolete]
        public int? ReqId { get; set; }

        // Source

        public string SourceVwhId { get; set; }

        public string SourceQuality { get; set; }

        public string BuildingId { get; set; }

        public string PriceSeasonCode { get; set; }

        public string SewingPlantCode { get; set; }
        public string SourceAreaId { get; set; }

        //target
        public string Priority { get; set; }

        public string AllowOverPulling { get; set; }
       
        public string PackagingPreferance { get; set; }
        
        public string DestinationArea { get; set; }

        public string TargetVwhId { get; set; }

        public string SaleTypeId { get; set; }

        public DateTime? CartonReceivedDate { get; set; }

        public IEnumerable<RequestSkuModel> RequestedSkus { get; set; }

        public DateTime? DateCreated { get; set; }

        public string Remarks { get; set; }

        public string RequestedBy { get; set; }

        public bool AssignedFlag { get; set; }

        public int QuantityRequested { get; set; }

        public int AssignedCartonCount { get; set; }

        public int AssignedPieces { get; set; }

        public string SourceAreaShortName { get; set; }

        public string DestinationAreaShortName { get; set; }

        //25-12-2012:For conversion request.
        public bool IsConversionRequest { get; set; }

        public string TargetQuality { get; set; }
    }
}

//$Id$