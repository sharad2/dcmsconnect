using System;
using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.Shipping.Repository
{
    public class RoutablePo
    {
        [Key]
        internal RoutingKey RoutingKey { get; set; }

        [Key]
        public DateTime? AtsDate { get; set; }

        public int[] EdiIdList { get; set; }

        public string LoadId { get; set; }

        public decimal? Weight { get; set; }   

        public decimal? Volume { get; set; }

        public DateTime? PickUpDate { get; set; }

        public string CarrierId { get; set; }

        public string CarrierDescription { get; set; }

        public string CustomerDcId { get; set; }

        public int? Pieces { get; set; }

        public int? CountBoxes { get; set; }

        public int? LoadCount { get; set; }

        public int? DoorCount { get; set; }

        public string CarrierList
        {
            get;
            set;
        }
        public string LoadList
        {
            get;
            set;
        }

        public string PickupDateList
        {
            get;
            set;
        }

        public string DoorList
        {
            get;
            set;
        }
        public int? CarrierCount { get; set; }

        public string DoorId { get; set; }

        public int? PickUpDateCount { get; set; }

        public string BuildingId { get; set; }

        public int PoIterationCount { get; set; }

        /// <summary>
        /// The original carrier which was downloaded from ERP.
        /// </summary>
        public string OriginalCarrierId { get; set; }

        /// <summary>
        /// The original DC which was downloaded from ERP.
        /// </summary>
        public string OriginalDCId { get; set; }

        public string OriginalCarrierDescription { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? DcCancelDate { get; set; }

        public decimal? TotalDollars { get; set; }

        /// <summary>
        /// Count of POs in an EDI having either Load or PickUpdate
        /// </summary>
        public int? EdiRoutedPoCount { get; set; }

        /// <summary>
        /// Count of POs in an EDI neither having Load nor Pickupdate
        /// </summary>
        public int? EdiRoutablePoCount { get; set; }

        /// <summary>
        /// ASN flag set for customer
        /// </summary>
        public string CustAsnFlag { get; set; }

        /// <summary>
        /// List of Distinct Building with in PO
        /// </summary>
        public string BuildingList { get; set; }
    }
}