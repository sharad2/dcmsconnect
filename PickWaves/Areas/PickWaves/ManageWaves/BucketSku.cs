using System.Collections.Generic;

namespace DcmsMobile.PickWaves.Repository.ManageWaves
{
    /// <summary>
    /// Represents one of the SKUs in a bucket
    /// </summary>
    public class BucketSku
    {
        public Sku Sku { get; set; }

        public int QuantityOrdered { get; set; }

        /// <summary>
        /// This property is use only for showing SKU is assigned in any location or not.
        /// </summary>
        public bool IsPitchingBucket { get; set; }

        public IEnumerable<CartonAreaInventory> BucketSkuInAreas { get; set; }

        private BucketActivityCollection _activities;

        /// <summary>
        /// Make sure that this never returns null
        /// </summary>
        public BucketActivityCollection Activities
        {
            get
            {
                return _activities ?? (_activities = new BucketActivityCollection());
            }
            set
            {
                _activities = value;
            }
        }       
    }

    /// <summary>
    /// Inventory in the carton area of a specific SKU
    /// </summary>    
    public class CartonAreaInventory
    {
        public InventoryArea InventoryArea { get; set; }

        /// <summary>
        /// The number of pieces available in the area
        /// </summary>
        public int InventoryPieces { get; set; }

        /// <summary>
        /// The number of pieces of the reference SKU available in the smallest carton
        /// </summary>
        public int PiecesInSmallestCarton { get; set; }
    }
}