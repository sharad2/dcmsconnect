using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using DcmsMobile.PickWaves.Helpers;
using DcmsMobile.PickWaves.ViewModels;
using System;

namespace DcmsMobile.PickWaves.Areas.PickWaves.ManageWaves
{
    public class WaveSkuListModel:ViewModelBase
    {
        //public int BucketId { get; set; }
        public BucketModel Bucket { get; set; }
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
        public int? TotalOrderedPieces
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
                return Math.Round((decimal)TotalPiecesPicked * 100 / (decimal)(TotalOrderedPieces));
            }
        }

        //[Obsolete]
        //public BoxState StateFilter { get; set; }

        //[Obsolete]
        //public BucketActivityType ActivityFilter { get; set; }

        //[Obsolete]
        //public string StateFilterDisplay
        //{
        //    get
        //    {
        //        return PickWaveHelpers.GetEnumMemberAttributes<BoxState, DisplayAttribute>()[this.StateFilter].Name;
        //    }
        //}

        //[Obsolete]
        //public string ActivityFilterDisplay
        //{
        //    get
        //    {
        //        return PickWaveHelpers.GetEnumMemberAttributes<BucketActivityType, DisplayAttribute>()[this.ActivityFilter].Name;
        //    }
        //}

        /// <summary>
        /// Total Weight of all SKUs of this wave
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:N4}")]
        public decimal? TotalWeight
        {
            get
            {
                return BucketSkuList.Sum(p => p.Weight);
            }
        }

        /// <summary>
        /// Total Volume of all SKUs of this wave
        /// </summary>     
        [DisplayFormat(DataFormatString = "{0:N4}")]
        public decimal? TotalVolume
        {
            get
            {
                return BucketSkuList.Sum(p => p.Volume);                
            }
        }
    }
}