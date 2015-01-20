using DcmsMobile.PickWaves.Repository;
using System;
using System.Collections.Generic;

namespace DcmsMobile.PickWaves.Areas.PickWaves.ManageWaves
{
    /// <summary>
    /// Represents one of the SKUs in a bucket
    /// </summary>
    internal class BucketSku
    {
        public Sku Sku { get; set; }

        public int? QuantityOrdered { get; set; }

        public IList<CartonAreaInventory> BucketSkuInAreas { get; set; }

        //public int? TotalPitchablePieces { get; set; }

        //public int? TotalPiecesPulled { get; set; }

        //public int? TotalPiecesPitched { get; set; }

        //public int? TotalPullablePieces { get; set; }

        public int? PiecesCompletePulling { get; set; }

        public int? PiecesBoxesCreatedPulling { get; set; }

        public int? BoxesRemainingPulling { get; set; }

        public int? PiecesCompletePitching { get; set; }

        public int? BoxesRemainingPitching { get; set; }

        public int? PiecesBoxesCreatedPitching { get; set; }
    }

    /// <summary>
    /// Inventory in the carton area of a specific SKU
    /// </summary>    
    internal class CartonAreaInventory
    {
        public InventoryArea InventoryArea { get; set; }

        /// <summary>
        /// The number of pieces available in the area
        /// </summary>
        public int InventoryPieces { get; set; }

        ///// <summary>
        ///// The number of pieces of the reference SKU available in the smallest carton
        ///// </summary>
        //public int PiecesInSmallestCarton { get; set; }

        /// <summary>
        /// Location in the area where most pieces are available
        /// </summary>
        public string BestLocationId { get; set; }

        public int PiecesAtBestLocation { get; set; }
    }
}