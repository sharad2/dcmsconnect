using System;
using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.Inquiry.Areas.Inquiry.ShipmentEntity
{


    internal class ParentShipmentHeadline
    {

        public string MBolID { get; set; }

        public string ParentShippingId { get; set; }

        public DateTime? ShippingDate { get; set; }

        public string CarrierId { get; set; }

        public string CarrierName { get; set; }

        public string CustomerID { get; set; }

        public string CustomerName { get; set; }

        public int? BoxCount { get; set; }

        public DateTime? StatusShippedDate  { get; set; }

    }

    internal class ParentShipment
    {
        public DateTime? ShippingDate { get; set; }

        public int? AppointmentNumber { get; set; }

        public string CarrierId { get; set; }

        public string CarrierName { get; set; }

        public string ShippingType { get; set; }

        public DateTime? ArrivalDate { get; set; }

        public string CustomerID { get; set; }

        public string CustomerName { get; set; }

        public string CustomerDcId { get; set; }

        public bool OnHoldFlag { get; set; }

        public bool IsTransferred { get; set; }

        public string MBolID { get; set; }

        public string ParentShippingId { get; set; }

        public string[] ToAddress
        {
            get;
            set;
        }

        public string[] FromAddress
        {
            get;
            set;
        }

        public string City { get; set; }

        public string State { get; set; }

        public string Country { get; set; }

        public string ZipCode { get; set; }

        public string FromZipCode { get; set; }

        public string FromCity { get; set; }

        public string FromState { get; set; }

        public string FromCountry { get; set; }

        public string FromCompany { get; set; }

        public string FreightChargeTerm
        {
            get;
            set;
        }
    }

    internal class ParentShipmentContent
    {
        [Key]
        public string ShippingId { get; set; }

        public int TotalBoxes { get; set; }

        public int? ShippableBoxes { get; set; }

        //public int? TotalTransferedBoxes { get; set; }
        public DateTime? ArrivalDate { get; set; }

        public decimal? Weight { get; set; }

        public string MaxBoxSuggestion { get; set; }

        public string MinBoxSuggestion { get; set; }

        public int? IncompletePickslips { get; set; }

        public int TotalPickslips { get; set; }

        public int? MinPickslipSuggestion { get; set; }

        public int? MaxPickslipSuggestion { get; set; }

        public int? TotalBuckets { get; set; }

        public int CountFrozenBuckets { get; set; }

        public int? MinBucketSuggestion { get; set; }

        public int? MaxBucketSuggestion { get; set; }

        public int ExpectedPieces { get; set; }

        public int CurrentPieces { get; set; }

        //public int TransferedCurrentPieces { get; set; }

    }

}