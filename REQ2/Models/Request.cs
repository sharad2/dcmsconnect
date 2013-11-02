
using System;
using System.Collections.Generic;

namespace DcmsMobile.REQ2.Models
{
    public class Request
    {

        public string CtnResvId { get; set; }

        // Source

        public string SourceVwhId { get; set; }

        public string SourceQuality { get; set; }

        public string BuildingId { get; set; }

        public string PriceSeasonCode { get; set; }

        public string SewingPlantCode { get; set; }

        public string SourceAreaId { get; set; }

        public string Priority { get; set; }

        public string DestinationArea { get; set; }

        public string TargetVwhId { get; set; }

        public DateTime? CartonReceivedDate { get; set; }

        public IEnumerable<RequestSku> RequestedSkus { get; set; }

        public DateTime? DateCreated { get; set; }

        public string Remarks { get; set; }

        public string RequestedBy { get; set; }

        /// <summary>
        /// Date on which cartons were last assigned to this request
        /// </summary>
        public DateTime? AssignDate { get; set; }

        public int QuantityRequested { get; set; }

        public int AssignedCartonCount { get; set; }

        public int AssignedPieces { get; set; }

        public string SourceAreaShortName { get; set; }

        public string DestinationAreaShortName { get; set; }

        /// <summary>
        /// Y is set against IsConversionRequest flag if user create request for conversion.
        /// </summary>
        public bool IsConversionRequest { get; set; }

        public string TargetQuality { get; set; }

        public int? RequestedSkuCount { get; set; }

        public int? PulledCartons { get; set; }

        /// <summary>
        /// Row sequence of a request. Needed when we update a request.
        /// </summary>
        public decimal? RowSequence { get; set; }

        public int? ReworkCartonCount { get; set; }
    }
}

//$Id$