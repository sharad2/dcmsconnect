using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using DcmsMobile.Shipping.Repository;

namespace DcmsMobile.Shipping.ViewModels
{
    /// <summary>
    /// PO with routing information
    /// </summary>
    public class RoutablePoModel
    {
        public RoutablePoModel()
        {

        }

        public RoutablePoModel(string key)
        {
            this.Key = key;
        }

        public RoutablePoModel(RoutablePo entity)
        {
            this.RoutingKey = entity.RoutingKey;
            this.LoadId = entity.LoadId;
            this.Weight = entity.Weight;
            this.Volume = entity.Volume;
            this.AtsDate = entity.AtsDate;
            this.CarrierId = entity.CarrierId;
            this.OriginalCarrierId = entity.OriginalCarrierId;
            this.OriginalCarrierDescription = entity.OriginalCarrierDescription;
            this.OriginalDCId = entity.OriginalDCId;
            this.CustomerDcId = entity.CustomerDcId;
            this.Pieces = entity.Pieces;
            this.CountBoxes = entity.CountBoxes;
            this.PickUpDate = entity.PickUpDate;
            this.LoadCount = entity.LoadCount;
            this.CarrierCount = entity.CarrierCount;
            this.PickUpDateCount = entity.PickUpDateCount;
            this.CarrierDescription = entity.CarrierDescription;
            this.LoadList = entity.LoadList;
            this.CarrierList = entity.CarrierList;
            this.PickupDateList = entity.PickupDateList;
            this.BuildingId = entity.BuildingId;
            this.PoIterationCount = entity.PoIterationCount;
            this.DoorId = entity.DoorId;
            this.StartDate = entity.StartDate;
            this.DcCancelDate = entity.DcCancelDate;
            this.TotalDollars = entity.TotalDollars;
            this.DoorList = entity.DoorList;
            this.DoorCount = entity.DoorCount;
            this.EdiRoutablePoCount = entity.EdiRoutablePoCount;
            this.EdiList = entity.EdiIdList;
            this.IsAsnCustomer = entity.CustAsnFlag == "Y" ? true : false;
            this.BuildingList = entity.BuildingList;
        }

        [Key]
        internal RoutingKey RoutingKey { get; set; }

        public int[] EdiList { get; set; }

        [Display(Name = "Load")]
        public string LoadId { get; set; }

        public string PoId
        {
            get
            {
                if (this.RoutingKey == null)
                {
                    return string.Empty;
                }
                return this.RoutingKey.PoId;
            }
        }


        public string CustomerId
        {
            get
            {
                if (this.RoutingKey == null)
                {
                    return string.Empty;
                }
                return this.RoutingKey.CustomerId;
            }
        }
        /// <summary>
        /// This is an opaque string value which represents all keys
        /// </summary>
        public string Key
        {
            get
            {
                if (this.RoutingKey == null)
                {
                    return string.Empty;
                }
                return this.RoutingKey.Key;
            }
            set
            {
                this.RoutingKey = new RoutingKey(value);
            }
        }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal? Weight { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal? Volume { get; set; }

        [DisplayFormat(DataFormatString = "{0:d}")]
        public DateTime? AtsDate { get; set; }

        /// <summary>
        /// The view uses this as HTML element id
        /// </summary>
        public string AtsDateId
        {
            get
            {
                return string.Format("{0:yyyyMMdd}", this.AtsDate);
            }
        }

        [Display(Name = "Carrier")]
        public string CarrierId { get; set; }
        public string CarrierDescription { get; set; }
        public string CarrierDisplay
        {
            get
            {
                if (string.IsNullOrWhiteSpace(this.CarrierId))
                {
                    return string.Empty;
                }
                return string.Format("{0}: {1}", this.CarrierId, this.CarrierDescription);
            }
        }

        //For display the original carrier on UI.
        public string OriginalCarrierId { get; set; }
        public string OriginalCarrierDescription { get; set; }
        public string OriginalCarrierDisplay
        {
            get
            {
                if (string.IsNullOrWhiteSpace(this.OriginalCarrierId))
                {
                    return string.Empty;
                }
                return string.Format("{0}: {1}", this.OriginalCarrierId, this.OriginalCarrierDescription);
            }
        }

        /// <summary>
        /// The original DC which was downloaded from ERP.
        /// </summary>
        public string OriginalDCId { get; set; }

        [Display(Name = "DC")]
        public string CustomerDcId { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? Pieces { get; set; }

        public int? CountBoxes { get; set; }

        /// <summary>
        /// This property required because repository return value in sum/count formate(by default 0) 
        /// </summary>
        public string CountBoxesDisplay
        {
            get
            {
                if (this.CountBoxes == 0)
                {
                    return null;
                }
                return string.Format("{0:N0}", this.CountBoxes);
            }
        }

        [DisplayFormat(DataFormatString = "{0:ddd d MMM}")]
        public DateTime? PickUpDate { get; set; }

        public int? LoadCount { get; set; }

        public string DoorId { get; set; }

        public int? DoorCount { get; set; }

        public int? CarrierCount { get; set; }

        public int? PickUpDateCount { get; set; }

        public string CarrierList { get; set; }

        public string LoadList { get; set; }

        public string PickupDateList { get; set; }

        public string DoorList { get; set; }

        [DisplayName("Building")]
        public string BuildingId { get; set; }

        /// <summary>
        /// This property used to highlight the POs having multiple iteration.
        /// </summary>
        public int PoIterationCount { get; set; }

        [DisplayFormat(DataFormatString = "{0:d}")]
        public DateTime? StartDate { get; set; }

        [DisplayFormat(DataFormatString = "{0:d}")]
        public DateTime? DcCancelDate { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal? TotalDollars { get; set; }


        /// <summary>
        /// The property is used to show conditional title at UI.
        /// </summary>
        /// <remarks>Hence 8 is length of list</remarks>
        private ICollection<string> _poAlertMessages;
        public ICollection<string> PoAlertMessages
        {
            get
            {
                if (_poAlertMessages != null)
                {
                    return _poAlertMessages;
                }
                //Hence 8 is capacity of the list.
                _poAlertMessages = new List<string>(8);
                if (PoIterationCount > 1)
                {
                    _poAlertMessages.Add(string.Format("This PO appears {0} times in the list because it was downloaded multiple times from the Vision system", this.PoIterationCount));
                }
                if (LoadCount > 1)
                {
                    _poAlertMessages.Add(string.Format("Multiple Loads: {0}.For BOL creation it's necessary the PO has only one Load", this.LoadList));
                }

                if (PickUpDateCount > 1)
                {
                    _poAlertMessages.Add(string.Format("Multiple PickupDates: {0}.For BOL creation it's necessary the PO has only one PickupDate", this.PickupDateList));
                }
                if (DoorCount > 1)
                {
                    _poAlertMessages.Add(string.Format("PO {0} has multiple Door: {1}.it will result in seprate BOL for each door.", this.PoId, this.DoorList));
                }
                if (!string.IsNullOrEmpty(this.BuildingList) && this.BuildingList.Split('-').Where(p => !string.IsNullOrWhiteSpace(p)).ToArray().Length > 1)
                {
                    _poAlertMessages.Add(string.Format("PO {0} has multiple Buildings: {1}.It will result in separate BOL per building.", this.PoId, this.BuildingList));
                }
                return _poAlertMessages;
            }
        }

        /// <summary>
        /// Sequence number of the BOL which this row represents. Used by RoutedViewModel to display BOL sequence.
        /// </summary>
        public int BolRowNumber { get; set; }

        /// <summary>
        /// True if PO has either Load or PickUp date
        /// </summary>
        public bool IsRouted
        {
            get
            {
                return (!string.IsNullOrEmpty(this.LoadId) || this.PickUpDate != null);
            }
        }

        /// <summary>
        /// Count of POs in an EDI having neither Load nor PickUpdate
        /// </summary>
        public int? EdiRoutablePoCount { get; set; }

        public bool IsAsnCustomer { get; set; }

        public string BuildingList { get; set; }
    }

}
