using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using DcmsMobile.PickWaves.Helpers;

namespace DcmsMobile.PickWaves.ViewModels.ManageWaves
{
    public class WaveSkuListModel
    {
        public int BucketId { get; set; }

        /// <summary>
        /// List of all SKU and their availability in Areas of this Wave
        /// </summary>
        public IList<BucketSkuModel> BucketSkuList { get; set; }

        /// <summary>
        /// List all distinct areas where SKUs of reference wave is available
        /// </summary>
        public IList<InventoryAreaModel> AllAreas { get; set; }

        /// <summary>
        /// Total number of picked pieces for all SKUs of this wave
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int TotalPiecesPicked
        {
            get
            {
                return BucketSkuList.Sum(p => p.Activities.Sum(q => q.PiecesComplete));
            }
        }

        /// <summary>
        /// Total remaining pieces for all SKUs of this wave
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int RemainingPiecesToPick
        {
            get
            {
                return BucketSkuList.Sum(p => p.Activities.Sum(q => q.PiecesIncomplete));
            }
        }

        /// <summary>
        /// Total ordered pieces for all SKUs of this wave
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int TotalOrderedPieces
        {
            get
            {
                return BucketSkuList.Sum(p => p.OrderedPieces);
            }
        }

        /// <summary>
        /// Total Percent picked for all SKUs of this wave
        /// </summary>
        public decimal PercentPiecesComplete
        {
            get
            {
                if (TotalPiecesPicked == 0 || TotalOrderedPieces == 0)
                {
                    return 0;
                }
                return (decimal)TotalPiecesPicked * 100 / (decimal)(TotalOrderedPieces);
            }
        }

        public BoxState StateFilter { get; set; }

        public BucketActivityType ActivityFilter { get; set; }

        public string StateFilterDisplay
        {
            get
            {
                return PickWaveHelpers.GetEnumMemberAttributes<BoxState, DisplayAttribute>()[this.StateFilter].Name;
            }
        }

        public string ActivityFilterDisplay
        {
            get
            {
                return PickWaveHelpers.GetEnumMemberAttributes<BucketActivityType, DisplayAttribute>()[this.ActivityFilter].Name;
            }
        }

        /// <summary>
        /// Total Weight of all SKUs of this wave
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:n2}")]
        public decimal TotalWeight
        {
            get
            {
                return BucketSkuList.Sum(p => p.WeightPerDozen);
            }
        }

        /// <summary>
        /// Total Volume of all SKUs of this wave
        /// </summary>     
        [DisplayFormat(DataFormatString = "{0:n2}")]
        public decimal TotalVolume
        {
            get
            {
                return BucketSkuList.Sum(p => p.VolumePerDozen);                
            }
        }
    }
}