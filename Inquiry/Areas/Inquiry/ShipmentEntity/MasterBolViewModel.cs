using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace DcmsMobile.Inquiry.Areas.Inquiry.ShipmentEntity
{



    public class MasterBolShipmentModel
    {
        public MasterBolShipmentModel()
        {

        }

        internal MasterBolShipmentModel(MasterBolShipment entity)
        {
            this.ArrivalDate = entity.ArrivalDate;
            this.OnHold = entity.OnHold;
            this.ShippingDate = entity.ShippingDate;
            this.ShippingId = entity.ShippingId;
        }
        public string ShippingId { get; set; }

        public bool OnHold { get; set; }

        [Display(Name = "Arrival Date")]
        [DisplayFormat(NullDisplayText = "None", DataFormatString = "{0:d}")]
        public DateTime? ArrivalDate { get; set; }

        [Display(Name = "Shipping Date")]
        [DisplayFormat(DataFormatString = "{0:d}")]
        public DateTime? ShippingDate { get; set; }


    }

    public class MasterBolViewModel
    {
        public MasterBolViewModel()
        {

        }

        internal MasterBolViewModel(ParentShipment entity)
        {
            this.CarrierId = entity.CarrierId;
            this.CarrierName = entity.CarrierName;
            this.CustomerID = entity.CustomerID;
            this.CustomerName = entity.CustomerName;
            this.FromAddress = entity.FromAddress;
            this.FromCity = entity.FromCity;
            this.FromCompany = entity.FromCompany;
            this.FromCountry = entity.FromCountry;
            this.FromState = entity.FromState;
            this.FromZipCode = entity.FromZipCode;
            this.IsTransferred = entity.IsTransferred;
            this.MBolID = entity.MBolID;
        }

        #region Printing
        [Display(Name = "Printer")]
        public string PrinterId { get; set; }

        public IEnumerable<SelectListItem> PrinterList { get; set; }
        #endregion

        #region Status

        public bool IsTransferred { get; set; }

        #endregion

        #region Addresses
        [Display(Name = "Shipping Address")]
        public string[] FromAddress
        {
            get;
            set;
        }

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
        #endregion

        #region Carrier
        public string CarrierId { get; set; }

        public string CarrierName { get; set; }
        #endregion

        [Display(Name = "Master BOL")]
        [DisplayFormat(NullDisplayText = "(None)")]
        public string MBolID { get; set; }

        public IList<MasterBolShipmentModel> ShipmentList { get; set; }

        public int shipmentListCount
        {
            get
            {
                return this.ShipmentList.Count;

            }
        }
    }
}