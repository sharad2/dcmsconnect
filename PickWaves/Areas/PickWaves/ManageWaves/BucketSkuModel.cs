using DcmsMobile.PickWaves.ViewModels;
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
        [Key]
        public int SkuId { get; set; }

        public string Style { get; set; }

        public string Color { get; set; }

        public string Dimension { get; set; }

        public string SkuSize { get; set; }

        public string UpcCode { get; set; }

        public string VwhId { get; set; }

        public decimal WeightPerDozen { get; set; }

        public decimal VolumePerDozen { get; set; }

        public bool IsAssignedSku { get; set; }

        /// <summary>
        /// Number of pieces ordered for this SKU
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int OrderedPieces { get; set; }

        /// <summary>
        /// List of Areas and available SKU quantity which were ordered for this wave.
        /// Inventory for all possible areas exist in the list so that the view can render table conveniently.
        /// </summary>
        public IList<BucketSkuAreaModel> InventoryByArea { get; set; }

        public IList<BucketActivityModel> Activities { get; set; }

        internal decimal PercentCurrentPieces
        {
            get
            {
                var inbox = this.Activities.Sum(p => p.PiecesComplete);
                if (inbox == 0 || OrderedPieces == 0)
                {
                    return 0;       // Not full at all
                }
                return (decimal)inbox / (this.OrderedPieces);
            }
        }
    }

    /// <summary>
    /// Area and available SKU quantity, ordered for bucket and which can be pulled form there.
    /// </summary>
    public class BucketSkuAreaModel : InventoryAreaModel
    {
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? InventoryPieces { get; set; }

        [DisplayFormat(DataFormatString = "[{0:N0}]")]
        public int? QuantityInSmallestCarton { get; set; }
    }
}