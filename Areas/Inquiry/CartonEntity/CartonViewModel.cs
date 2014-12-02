using DcmsMobile.Inquiry.Areas.Inquiry.SharedViews;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace DcmsMobile.Inquiry.Areas.Inquiry.CartonEntity
{
    public class CartonViewModel : ICartonProcessViewModel
    {
        //[Obsolete]
        //private SkuInventoryItem _firstSku;

        public CartonViewModel()
        {

        }

        internal CartonViewModel(ActiveCarton entity)
        {

            this.Building = entity.Building;
            this.CartonId = entity.CartonId;
            this.CartonStorageArea = entity.CartonAreaId;
            this.DamageCode = entity.DamageCode;
            //this._firstSku = entity.FirstSku;
            this.LastPulledDate = entity.LastPulledDate;
            this.LocationId = entity.LocationId;
            //this.ModelTitle = entity.ModelTitle;
            this.PalletId = entity.PalletId;
            this.PriceSeasonCode = entity.PriceSeasonCode;
            this.ReqProcessId = entity.ReqProcessId;
            this.ReservedUccID = entity.ReservedUccID;
            if (!string.IsNullOrEmpty(entity.SewingPlantCode)) {
                this.DisplaySewingPlant = string.Format("{0}: {1}", entity.SewingPlantCode, entity.SewingPlantName);
            }
            this.ShipmentId = entity.ShipmentId;
            this.ShipmentDate = entity.ShipmentDate;
            this.SuspenseDate = entity.SuspenseDate;
            this.UnmatchComment = entity.UnmatchComment;
            this.UnmatchReason = entity.UnmatchReason;
            this.IsCartonMarkedForWork = entity.IsCartonMarkedForWork;
            this.ShortName = entity.AreaShortName;
            this.AreaDescription = entity.AreaDescription;

            this.CartonSkuRestockAisleAreaId = entity.BestRestockAreaShortName;
            this.CartonSkuRestockAisleId = entity.BestRestockAisleId;
            this.CartonSkuRestockAisleWhId = entity.BestRestockBuildingId;
            this.CartonSkuRestockLocationId = entity.BestRestockLocationId;
            this.SkuId = entity.SkuId;
            VwhId = entity.VwhId;

            if (entity.SkuId != null)
            {
                this.DisplaySku = string.Format("{0}, {1}, {2}, {3}", entity.Style, entity.Color, entity.Dimension, entity.SkuSize);
            }
           // this.UpcCode = entity.Upc;
            this.Pieces = entity.Pieces;
            this.QualityCode = entity.QualityCode;
            this.QualityDescription = entity.QualityDescription;            
            
        }

        [DisplayFormat(NullDisplayText="NA")]
        public string VwhId { get; set; }

        [Display(Name = "Carton")]
        [DisplayFormat(NullDisplayText = "None")]
        public string CartonId { get; set; }

        [Display(Name = "Location")]
        [DisplayFormat(NullDisplayText = "None")]
        public string LocationId { get; set; }

        public string CartonStorageArea { get; set; }

        [Display(Name = "Area")]
        [DisplayFormat(NullDisplayText = "None")]
        public string ShortName { get; set; }

        public string AreaDescription { get; set; }

        /// <summary>
        /// Returns the SKU, Pieces and Vwh in readable format
        /// </summary>
        [DisplayFormat(NullDisplayText = "Empty")]
        public string DisplaySku
        {
            get;
            set;
        }

        public int? SkuId { get; set; }

        public int? Pieces
        {
            get;
            set;
        }

        public string QualityCode
        {
            get;
            set;
        }

        public string QualityDescription;

        [Display(Name = "Request")]
        [DisplayFormat(NullDisplayText = "None")]
        public int? ReqProcessId { get; set; }

        [Display(Name = "Last Pulled Date")]
        [DisplayFormat(DataFormatString = "{0:d}")]
        public DateTime? LastPulledDate { get; set; }


        public string ReservedUccID { get; set; }


        [Display(Name = "Pallet")]
        [DisplayFormat(NullDisplayText = "None")]
        public string PalletId { get; set; }


        [Display(Name = "Price Season Code")]
        [DisplayFormat(NullDisplayText = "None")]
        public string PriceSeasonCode { get; set; }


        [DisplayFormat(NullDisplayText = "NA")]
        public string DisplaySewingPlant { get; set; }

        [Display(Name = "Building")]
        [DisplayFormat(NullDisplayText = "Unknown")]
        public string Building { get; set; }

        [Display(Name = "Damage Code")]
        [DisplayFormat(NullDisplayText = "None")]
        public string DamageCode { get; set; }

        [Display(Name = "Suspense Date")]
        [DisplayFormat(DataFormatString = "{0:d}", NullDisplayText = "Not in suspense")]
        public DateTime? SuspenseDate { get; set; }

        [Display(Name = "Unmatched Reason")]
        [DisplayFormat(NullDisplayText = "None")]
        public string UnmatchReason { get; set; }

        [Display(Name = "Unmatched Comment")]
        [DisplayFormat(NullDisplayText = "None")]
        public string UnmatchComment { get; set; }



        /// <summary>
        /// Property to keep track on all processes of carton
        /// </summary>
        public IList<CartonProcessModel> ProcessList { get; set; }


        [Display(Name = "Shipment ID")]
        [DataType("Alert")]
        [DisplayFormat(NullDisplayText = "None")]
        public string ShipmentId { get; set; }

        [Display(Name = "Shipment Date")]
        [DataType(DataType.Date)]
        public DateTime? ShipmentDate { get; set; }


        /// <summary>
        /// Url of Report 40.23: This report display pallet history.
        /// Url of Report 40.09: Carton on Pallet
        /// </summary>
        public string PalletHistory
        {
            get
            {

                return System.Configuration.ConfigurationManager.AppSettings["DcmsLiveBaseUrl"] + "Reports/Category_040/R40_23.aspx";
            }
        }
        public string CartonOnPalletReport
        {
            get
            {

                return System.Configuration.ConfigurationManager.AppSettings["DcmsLiveBaseUrl"] + "Reports/Category_040/R40_09.aspx";
            }
        }


        public bool AllowPrinting { get; set; }

        [Display(Name = "Printer")]
        public string PrinterId { get; set; }

        public IEnumerable<SelectListItem> PrinterList { get; set; }

        public bool IsCartonMarkedForWork { get; set; }

        //[Obsolete]
        //public SkuInventoryItemModel PreConversionSku { get; set; }

        //this property ios added to show Assigned restock aisle for carton's SKU on carton scan
        //public CartonLocation AssignedRestockAisle { get; set; }
        public string CartonSkuRestockAisleId { get; set; }

        public string CartonSkuRestockAisleWhId { get; set; }

        public string CartonSkuRestockAisleAreaId { get; set; }

        public string CartonSkuRestockLocationId { get; set; }

        /// <summary>
        /// OK
        /// </summary>
        public string UrlAbondonRework { get; set; }

        /// <summary>
        /// OK
        /// </summary>
        public string UrlEditCarton { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string UrlRestock { get; set; }

        /// <summary>
        /// OK
        /// </summary>
        public string UrlCartonToPallet { get; set; }


        public string UrlBulkUpdateCarton{ get; set; }

        /// <summary>
        /// Ok
        /// </summary>
        public string UrlCartonLocating { get; set; }

        /// <summary>
        /// Ok
        /// </summary>
        public string UrlMarkReworkComplete { get; set; }

    }
}