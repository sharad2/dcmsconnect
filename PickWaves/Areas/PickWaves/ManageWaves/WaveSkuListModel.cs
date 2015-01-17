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
        [Obsolete]
        public int? TotalPiecesComplete
        {
            get
            {
                return BucketSkuList.Sum(p => p.Activities.Sum(q => q.PiecesComplete));
            }
        }

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
                    .Sum(p => p.PiecesRemaining + p.PiecesComplete);
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
        /// Total remaining pieces for all SKUs of this wave
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:N0}")]
        [Obsolete("Rename to TotalPieesRemaining")]
        public int? RemainingPiecesToPick
        {
            get
            {
                return BucketSkuList.Sum(p => p.Activities.Sum(q => q.PiecesRemaining));
            }
        }

        ///// <summary>
        ///// Total ordered pieces for all SKUs of this wave
        ///// </summary>
        //[DisplayFormat(DataFormatString = "{0:N0}")]   
        //[Obsolete]
        //public int? TotalOrderedPieces
        //{
        //    get
        //    {
        //        return BucketSkuList.Sum(p => p.OrderedPieces);
        //    }
        //}

        ///// <summary>
        ///// Total Percent picked for all SKUs of this wave
        ///// </summary>
        //[DisplayFormat(DataFormatString="{0:N0}%")]
        //[Obsolete]
        //public int PercentPiecesPulled
        //{
        //    get
        //    {
        //        if (TotalPiecesPulled == 0 || TotalOrderedPieces == 0)
        //        {
        //            return 0;
        //        }
        //        return (int)Math.Round((decimal)TotalPiecesPulled * 100 / (decimal)(TotalOrderedPieces));
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