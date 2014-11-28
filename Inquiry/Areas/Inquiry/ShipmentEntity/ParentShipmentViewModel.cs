using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System.Linq;

namespace DcmsMobile.Inquiry.Areas.Inquiry.ShipmentEntity
{
    public class ParentShipmentContentModel
    {
        public ParentShipmentContentModel()
        {

        }


        internal ParentShipmentContentModel(ParentShipmentContent entity)
        {
            this.CurrentPieces = entity.CurrentPieces;
            this.ExpectedPieces = entity.ExpectedPieces;
            this.CountFrozenBuckets = entity.CountFrozenBuckets;
            this.IncompletePickslips = entity.IncompletePickslips;
            this.MaxBoxSuggestion = entity.MaxBoxSuggestion;
            this.MaxBucketSuggestion = entity.MaxBucketSuggestion;
            this.MaxPickslipSuggestion = entity.MaxPickslipSuggestion;
            this.MinBoxSuggestion = entity.MinBoxSuggestion;
            this.MinBucketSuggestion = entity.MinBucketSuggestion;
            this.MinPickslipSuggestion = entity.MinPickslipSuggestion;
            this.ShippableBoxes = entity.ShippableBoxes;
            this.ShippingId = entity.ShippingId;
            this.TotalBoxes = entity.TotalBoxes;
            this.TotalBuckets = entity.TotalBuckets;
            this.TotalPickslips = entity.TotalPickslips;
            //this.TotalTransferedBoxes = entity.TotalTransferedBoxes;
            //this.TransferedCurrentPieces = entity.TransferedCurrentPieces;
            this.Weight = entity.Weight;
        }

        [Key]
        [Display(ShortName = "Shipment", Order = 1)]
        public string ShippingId { get; set; }


        [DisplayFormat(DataFormatString = "{0:N0}")]
        [Display(Name = "Cartons", ShortName = "Cartons Shipped", Order = 6)]
        public int TotalBoxes { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        [ScaffoldColumn(false)]
        public int? ShippableBoxes { get; set; }

        //[DisplayFormat(DataFormatString = "{0:N0}")]
        //[Display(Name = "Cartons", ShortName = "Cartons Transfered", Order = 7)]
        //public int? TotalTransferedBoxes { get; set; }


        [Display(Name = "Weight", Order = 8)]
        [DisplayFormat(NullDisplayText = "None", DataFormatString = "{0:N2} lbs")]
        public decimal? Weight { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        [ScaffoldColumn(false)]
        public int? NonShippableBoxes
        {
            get
            {
                return this.TotalBoxes - this.ShippableBoxes;
            }
        }

        [ScaffoldColumn(false)]
        public string MaxBoxSuggestion { get; set; }

        [ScaffoldColumn(false)]
        public string MinBoxSuggestion { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        [ScaffoldColumn(false)]
        public int? IncompletePickslips { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        [Display(Name = "Pickslips", ShortName = "# Pickslips", Order = 2)]
        public int TotalPickslips { get; set; }

        [ScaffoldColumn(false)]
        public int? MinPickslipSuggestion { get; set; }

        [ScaffoldColumn(false)]
        public int? MaxPickslipSuggestion { get; set; }

        [ScaffoldColumn(false)]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? TotalBuckets { get; set; }

        /// <summary>
        /// Number of frozen buckets
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:N0}")]
        [ScaffoldColumn(false)]
        public int CountFrozenBuckets { get; set; }

        [ScaffoldColumn(false)]
        public int? MinBucketSuggestion { get; set; }

        [ScaffoldColumn(false)]
        public int? MaxBucketSuggestion { get; set; }

        [Display(Name = "Pcs Ordered", Order = 3)]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int ExpectedPieces { get; set; }

        [Display(Name = "Pcs Shipped", Order = 4)]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int CurrentPieces { get; set; }

        //[Display(Name = "Shipped Pieces", ShortName = "Pcs Transferred", Order = 5)]
        //[DisplayFormat(DataFormatString = "{0:N0}")]
        //[Obsolete]
        //public int TransferedCurrentPieces { get; set; }

        // [DisplayFormat(DataFormatString = "{0:P0}")]
        [ScaffoldColumn(false)]
        public int PercentComplete
        {
            get
            {
                return (this.ExpectedPieces > 0) ? ((this.CurrentPieces * 100) / this.ExpectedPieces) : 0;
            }

        }

        //[ScaffoldColumn(false)]
        //[Obsolete]
        //public int PercentShipped
        //{
        //    get
        //    {
        //        return (this.ExpectedPieces > 0) ? ((this.TransferedCurrentPieces * 100) / this.ExpectedPieces) : 0;
        //    }

        //}
    }

    public class ParentShipmentViewModel
    {
        public ParentShipmentViewModel()
        {

        }

        internal ParentShipmentViewModel(ParentShipment entity)
        {
            switch (entity.FreightChargeTerm)
            {
                case "CC":
                    this.FreightChargeTerm = "Collect";
                    break;

                case "PP":
                    FreightChargeTerm = "Prepaid";
                    break;

                case "PC":
                    FreightChargeTerm = "Third Party";
                    break;

                default:
                    FreightChargeTerm = entity.FreightChargeTerm;
                    break;
            }

            this.OnHoldFlag = entity.OnHoldFlag;
            this.AppointmentNumber = entity.AppointmentNumber;
            this.ArrivalDate = entity.ArrivalDate;
            this.CarrierId = entity.CarrierId;
            this.CarrierName = entity.CarrierName;
            this.City = entity.City;
            this.Country = entity.Country;
            this.CustomerDcId = entity.CustomerDcId;
            this.CustomerID = entity.CustomerID;
            this.CustomerName = entity.CustomerName;
            this.FromAddress = entity.FromAddress;
            this.FromCity = entity.FromCity;
            this.FromCompany = entity.FromCompany;
            this.FromCountry = entity.FromCountry;
            this.FromState = entity.FromState;
            this.FromZipCode = entity.FromZipCode;
            this.IsTransferred = entity.IsTransferred;
            this.ParentShippingId = entity.ParentShippingId;
            this.ShippingDate = entity.ShippingDate;
            this.State = entity.State;
            this.ToAddress = entity.ToAddress;
            this.ZipCode = entity.ZipCode;
            this.MBolID = entity.MBolID;

        }

        #region Printing
        [Display(Name = "Printer")]
        public string PrinterId { get; set; }

        public IEnumerable<SelectListItem> PrinterList { get; set; }
        #endregion

        public IList<ParentShipmentContentModel> ShipmentDetail { get; set; }

        #region Dates
        [Display(Name = "Shipping Date")]
        [DisplayFormat(DataFormatString = "{0:d}")]
        public DateTime? ShippingDate { get; set; }

        [Display(Name = "Arrival Date")]
        [DisplayFormat(NullDisplayText = "None", DataFormatString = "{0:d}")]
        public DateTime? ArrivalDate { get; set; }
        #endregion

        #region Status
        [Display(Name = "On Hold")]
        public bool OnHoldFlag { get; set; }

        public bool IsTransferred { get; set; }

        //public bool IsMbolShipment { get; set; }

        #endregion

        #region Addresses
        [Display(Name = "Shipping Address")]
        public string[] ToAddress
        {
            get;
            set;
        }

        [Display(Name = "Shipping Address")]
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
        #endregion

        #region Customer
        [Display(Name = "Customer")]
        [DisplayFormat(NullDisplayText = "None")]
        public string CustomerID { get; set; }

        public string CustomerName { get; set; }

        [Display(Name = "DC")]
        public string CustomerDcId { get; set; }
        #endregion

        #region Carrier
        public string CarrierId { get; set; }

        public string CarrierName { get; set; }

        [Display(Name = "Freight Charge Term")]
        public string FreightChargeTerm
        {
            get;
            private set;
        }

        [DisplayFormat(NullDisplayText = "(None)")]
        public int? AppointmentNumber { get; set; }
        #endregion

        [Display(Name = "Parent Shipping ID")]
        public string ParentShippingId { get; set; }

        [Display(Name = "Master BOL")]
        [DisplayFormat(NullDisplayText = "(None)")]
        public string MBolID { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int TotalCurrentPieces
        {
            get
            {
                if (this.ShipmentDetail == null)
                {
                    return 0;
                }
                return this.ShipmentDetail.Sum(p => p.CurrentPieces);
            }
        }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int TotalExpectedPieces
        {
            get
            {
                if (this.ShipmentDetail == null)
                {
                    return 0;
                }
                return this.ShipmentDetail.Sum(p => p.ExpectedPieces);
            }
        }

        public int TotalPercentComplete
        {
            get
            {
                return (this.TotalExpectedPieces > 0) ? ((this.TotalCurrentPieces * 100) / this.TotalExpectedPieces) : 0;
            }
        }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int TotalCartons
        {
            get
            {
                if (this.ShipmentDetail == null)
                {
                    return 0;
                }
                return this.ShipmentDetail.Sum(p => p.TotalBoxes);
            }
        }

        [DisplayFormat(NullDisplayText = "None", DataFormatString = "{0:N2} lbs")]
        public decimal? TotalWeight
        {
            get
            {
                if (this.ShipmentDetail == null)
                {
                    return null;
                }
                return this.ShipmentDetail.Sum(p => p.Weight);
            }
        }

    }
}