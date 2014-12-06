using DcmsMobile.Receiving.Areas.Receiving.Home.Repository;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.Receiving.Areas.Receiving.Home
{

    public class ReceivedCartonModel
    {
        public ReceivedCartonModel()
        {

        }

        internal ReceivedCartonModel(ReceivedCarton entity)
        {
            if (entity.Sku != null)
            {
                this.DisplaySku = string.Format("{0},{1},{2},{3}", entity.Sku.Style, entity.Sku.Color, entity.Sku.Dimension, entity.Sku.SkuSize);
                this.SkuPrice = entity.Sku.SkuPrice;
            }
            this.CartonId = entity.CartonId;
            this.VwhId = entity.VwhId;
            this.ReceivedDate = entity.ReceivedDate;
            this.ProcessId = entity.InShipmentId;
            this.DestinationArea = entity.DestinationArea;
        }

        [Key]
        [Required]
        [Display(ShortName = "Carton", Name = "Carton")]
        public string CartonId { get; set; }

        [Display(ShortName = "Vwh")]
        public string VwhId { get; set; }

        [Display(ShortName = "Area")]
        public string DestinationArea
        {
            get;
            set;
        }

        public string DisplaySku { get; set; }

        public decimal? SkuPrice { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd-MMM hh:mm:ss tt}")]
        [Display(Name = "Receive Date")]
        public DateTimeOffset? ReceivedDate { get; set; }

        public int? ProcessId { get; set; }

        /// <summary>
        /// Whether this carton should be highlighted in the list. It will be highlighted when it has just been received
        /// </summary>
        public bool Highlight { get; set; }
    }

    public class PalletViewModel
    {
        public PalletViewModel()
        {

        }

        public int ProcessId { get; set; }

        [Display(Name = "Pallet")]
        public string PalletId { get; set; }

        private IList<ReceivedCartonModel> _cartons;
        public IList<ReceivedCartonModel> Cartons
        {
            get { return _cartons ?? (_cartons = new ReceivedCartonModel[0]); }
            set
            {
                _cartons = value;
            }
        }

        [Display(Name = "Pallet Limit")]
        public int PalletLimit { get; set; }



        public int SkuCount
        {
            get;
            set;  //TODO
        }

        public int PalletProgress
        {
            get
            {
                if (this.PalletLimit == 0)
                {
                    return 100;
                }
                return (int)Math.Round(this.Cartons.Count * 100.0 / this.PalletLimit);
            }
        }

        public string StatusMessage { get; set; }

        public string DestinationArea
        {
            get
            {
                if (Cartons == null || Cartons.Count == 0)
                {
                    return string.Empty;
                }
                return Cartons[0].DestinationArea;
            }
        }

        public string VwhId
        {
            get
            {
                if (Cartons == null || Cartons.Count == 0)
                {
                    return string.Empty;
                }
                return Cartons[0].VwhId;
            }
        }
    }
}



//$Id$