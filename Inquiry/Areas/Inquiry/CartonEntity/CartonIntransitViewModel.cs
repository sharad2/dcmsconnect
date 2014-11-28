using System;
using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.Inquiry.Areas.Inquiry.CartonEntity
{
    public class CartonIntransitViewModel
    {
        public CartonIntransitViewModel()
        {

        }
        public string UrlReceiving { get; set; }

        internal CartonIntransitViewModel(IntransitCarton entity)
        {
            this.CartonId = entity.CartonId;
            //this._firstSku = entity.FirstSku;
            //this.ModelTitle = entity.ModelTitle;
            this.PriceSeasonCode = entity.PriceSeasonCode;
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
           this.SkuId = entity.SkuId;
            this.Pieces = entity.Pieces;
            this.QualityCode = entity.QualityCode;
            this.SourceOrderedID = entity.SourceOrderedID;
            this.SourceOrderPrefix = entity.SourceOrderPrefix;
            this.SourceOrderLineNumber = entity.SourceOrderLineNumber;
           
            
        }

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

        public int? Pieces
        {
            get;
            set;
        }


        [Display(Name = "Price Season Code")]
        public string PriceSeasonCode { get; set; }

        public string QualityCode
        {
            get;
            set;
        }   
        public string DisplaySewingPlant { get; set; }
        public int? SourceOrderedID { get; set; }
        public string SourceOrderPrefix { get; set; }
        public int? SourceOrderLineNumber { get; set; }

        [Display(Name = "Shipment ID")]
        [DataType("Alert")]
        [DisplayFormat(NullDisplayText = "None")]
        public string ShipmentId { get; set; }

        [Display(Name = "Shipment Date")]
        [DataType(DataType.Date)]
        public DateTime? ShipmentDate { get; set; }

        public int? SkuId { get; set; }
        
        public string VwhId { get; set; }
    }
}