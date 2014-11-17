using DcmsMobile.Receiving.Areas.Receiving.Home.Repository;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

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
            //this.DispositionId = entity.DispositionId;
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
    }

    public class PalletViewModel
    {
        public PalletViewModel()
        {

        }

        public int ProcessId { get; set; }

        [Display(Name = "Pallet")]
        public string PalletId { get; set; }

        ///// <summary>
        ///// The disposition of a pallet is the same as the disposition of the first carton, or null if there are no cartons.
        ///// </summary>
        //public string DispositionId
        //{
        //    get
        //    {
        //        if (this.Cartons.Count == 0)
        //        {
        //            return string.Empty;
        //        }
        //        return this.Cartons.First().DispositionId;
        //    }
        //}

        private IList<ReceivedCartonModel> _cartons;
        public IList<ReceivedCartonModel> Cartons
        {
            get { return _cartons ?? (_cartons = new ReceivedCartonModel[0]); }
            set
            {
                _cartons = value;
            }
        }

        /// <summary>
        /// The carton which was last put on the pallet
        /// </summary>
        /// <remarks>
        /// After a carton is received, it becomes the last carton. At all other times this is null.
        /// </remarks>
        // ReSharper disable MemberCanBePrivate.Global
        public string LastCartonId { get; set; }
        // ReSharper restore MemberCanBePrivate.Global

        public int SelectedCartonIndex
        {
            get
            {
                return this.Cartons.Select((p, i) => p.CartonId == this.LastCartonId ? i + 1 : 0).FirstOrDefault(p => p > 0) - 1;
            }
        }

        [Display(Name = "Pallet Limit")]
        public int PalletLimit { get; set; }

        //public int QueryCount { get; set; }



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