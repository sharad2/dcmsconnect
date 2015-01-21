using DcmsMobile.PickWaves.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace DcmsMobile.PickWaves.Areas.PickWaves.ManageWaves
{
    /// <summary>
    /// Sku ordered in wave
    /// </summary>
    public class BucketSkuModel
    {
        internal BucketSkuModel(BucketSku entity)
        {
            Style = entity.Sku.Style;
            Color = entity.Sku.Color;
            Dimension = entity.Sku.Dimension;
            SkuSize = entity.Sku.SkuSize;
            UpcCode = entity.Sku.UpcCode;
            SkuId = entity.Sku.SkuId;
            VwhId = entity.Sku.VwhId;
            Volume = entity.Sku.VolumePerDozen / 12;
            Weight = entity.Sku.WeightPerDozen / 12;
            OrderedPieces = entity.QuantityOrdered;
            PiecesCompletePulling = entity.PiecesCompletePulling;
            PiecesBoxesCreatedPulling = entity.PiecesBoxesCreatedPulling;
            BoxesRemainingPulling = entity.BoxesRemainingPulling;

            PiecesCompletePitching = entity.PiecesCompletePitching;
            PiecesBoxesCreatedPitching = entity.PiecesBoxesCreatedPitching;
            BoxesRemainingPitching = entity.BoxesRemainingPitching;
        }

        [Key]
        public int SkuId { get; set; }

        public string Style { get; set; }

        public string Color { get; set; }

        public string Dimension { get; set; }

        public string SkuSize { get; set; }

        public string DisplaySku
        {
            get
            {
                return string.Format("{0},{1},{2},{3}", this.Style, this.Color, this.Dimension, this.SkuSize);
            }
        }

        public string UpcCode { get; set; }

        public string VwhId { get; set; }

        [DisplayFormat(DataFormatString = "{0:N4}")]
        public decimal? Weight { get; set; }

        [DisplayFormat(DataFormatString = "{0:N4}")]
        public decimal? Volume { get; set; }


        /// <summary>
        /// Number of pieces ordered for this SKU
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? OrderedPieces { get; set; }

        /// <summary>
        /// List of Areas and available SKU quantity which were ordered for this wave.
        /// Inventory for all possible areas exist in the list so that the view can render table conveniently.
        /// </summary>
        public IList<BucketSkuAreaModel> InventoryByArea { get; set; }


        public int? PiecesCompletePulling { get; set; }

        public int? PiecesBoxesCreatedPulling { get; set; }

        public int? BoxesRemainingPulling { get; set; }

        public int? PiecesCompletePitching { get; set; }

        public int? PiecesBoxesCreatedPitching { get; set; }

        public int? BoxesRemainingPitching { get; set; }
    }

    /// <summary>
    /// Area and available SKU quantity, ordered for bucket and which can be pulled form there.
    /// </summary>
    public class BucketSkuAreaModel
    {
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? InventoryPieces { get; set; }

        public int? PiecesAtBestLocation { get; set; }

        [DisplayFormat(DataFormatString = "@ {0}")]
        public string BestLocationId { get; set; }

        public string BuildingId { get; set; }

        public string Description { get; set; }

        public string ShortName { get; set; }

        public string AreaId { get; set; }
    }
}