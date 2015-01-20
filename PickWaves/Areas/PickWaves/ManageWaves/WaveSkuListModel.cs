using DcmsMobile.PickWaves.Helpers;
using DcmsMobile.PickWaves.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace DcmsMobile.PickWaves.Areas.PickWaves.ManageWaves
{
    public class WaveSkuListModel:ViewModelBase
    {
        public WaveSkuListModel()
        {

        }

        public BucketModel Bucket { get; set; }
        /// <summary>
        /// List of all SKU and their availability in Areas of this Wave
        /// </summary>
        public IList<BucketSkuModel> BucketSkuList { get; set; }

        /// <summary>
        /// List all distinct areas where SKUs of reference wave is available
        /// </summary>
        public IList<InventoryAreaModel> AllAreas { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? TotalPiecesPulled
        {
            get
            {
                return BucketSkuList.SelectMany(p => p.Activities)
                    .Where(p => p.ActivityType == BucketActivityType.Pulling)
                    .Sum(p => p.PiecesComplete);
            }
        }


        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? TotalPiecesPitched
        {
            get
            {
                return BucketSkuList.SelectMany(p => p.Activities)
                    .Where(p => p.ActivityType == BucketActivityType.Pitching)
                    .Sum(p => p.PiecesComplete);
            }
        }


        /// <summary>
        /// Total number of pulling pieces in the bucket
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? TotalPullablePieces
        {
            get
            {
                return BucketSkuList.SelectMany(p => p.Activities)
                    .Where(p => p.ActivityType == BucketActivityType.Pulling)
                    .Sum(p => p.PiecesBoxesCreated);
            }
        }

        /// <summary>
        /// Total number of pitching pieces in the bucket
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? TotalPitchablePieces
        {
            get
            {
                return BucketSkuList.SelectMany(p => p.Activities)
                    .Where(p => p.ActivityType == BucketActivityType.Pitching)
                    .Sum(p => p.PiecesRemaining + p.PiecesComplete);
            }
        }

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