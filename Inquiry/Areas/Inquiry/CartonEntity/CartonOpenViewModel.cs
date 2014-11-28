using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.Inquiry.Areas.Inquiry.CartonEntity
{
    public class CartonOpenViewModel: ICartonProcessViewModel
    {
        public CartonOpenViewModel()
        {

        }

        internal CartonOpenViewModel(OpenCarton entity)
        {
            this.CartonId = entity.CartonId;
            this.DamageCode = entity.DamageCode;
            this.LocationId = entity.LocationId;
            this.PalletId = entity.PalletId;
            this.PriceSeasonCode = entity.PriceSeasonCode;
            this.ReservedUccID = entity.ReservedUccID;
            if (!string.IsNullOrEmpty(entity.SewingPlantCode))
            {
                this.DisplaySewingPlant = string.Format("{0}: {1}", entity.SewingPlantCode, entity.SewingPlantName);
            }
            this.ShipmentId = entity.ShipmentId;
            this.ShipmentDate = entity.ShipmentDate;         
            VwhId = entity.VwhId;

            if (entity.SkuId != null)
            {
                this.DisplaySku = string.Format("{0}, {1}, {2}, {3}", entity.Style, entity.Color, entity.Dimension, entity.SkuSize);
            }
           // this.UpcCode = entity.Upc;
            this.SkuId = entity.SkuId;
            this.Pieces = entity.Pieces;
            this.QualityCode = entity.QualityCode;
            this.QualityDescription = entity.QualityDescription;
            this.ShortName = entity.ShortName;
        }
        [Display(Name = "Carton")]
        [DisplayFormat(NullDisplayText = "None")]
        public string CartonId { get; set; }

        /// <summary>
        /// Returns the SKU, Pieces and Vwh in readable format
        /// </summary>
        [DisplayFormat(NullDisplayText = "Empty")]
        public string DisplaySku
        {
            get;
            set;
        }

        [Display(Name = "Location")]
        [DisplayFormat(NullDisplayText = "None")]
        public string LocationId { get; set; }

        [Display(Name = "Damage Code")]
        [DisplayFormat(NullDisplayText = "NA")]
        public string DamageCode { get; set; }


        [Display(Name = "Pallet")]
        [DisplayFormat(NullDisplayText = "None")]
        public string PalletId { get; set; }


        [Display(Name = "Area")]
        [DisplayFormat(NullDisplayText = "None")]
        public string ShortName { get; set; }

        [DisplayFormat(NullDisplayText="None")]
        public int? Pieces
        {
            get;
            set;
        }



        [Display(Name = "Price Season Code")]
        [DisplayFormat(NullDisplayText = "NA")]
        public string PriceSeasonCode { get; set; }
        

        public string QualityCode
        {
            get;
            set;
        }
        public string QualityDescription;
        /// <summary>
        /// Url of Report 40.23: This report display pallet history.
        /// </summary>
        public string PalletHistory
        {
            get
            {

                return System.Configuration.ConfigurationManager.AppSettings["DcmsLiveBaseUrl"] + "Reports/Category_040/R40_23.aspx";
            }
        }

        public string ReservedUccID { get; set; }

        public string DisplaySewingPlant { get; set; }

        [Display(Name = "Shipment ID")]
        [DataType("Alert")]
        [DisplayFormat(NullDisplayText = "None")]
        public string ShipmentId { get; set; }

        [Display(Name = "Shipment Date")]
        [DataType(DataType.Date)]
        public DateTime? ShipmentDate { get; set; }

        public string VwhId { get; set; }

        public int? SkuId { get; set; }


        /// <summary>
        /// Property to keep track on all processes of carton
        /// </summary>
        public IList<CartonProcessModel> ProcessList { get; set; }
    }
}