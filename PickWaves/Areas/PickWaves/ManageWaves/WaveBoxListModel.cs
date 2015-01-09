using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using DcmsMobile.PickWaves.Helpers;
using System;
using DcmsMobile.PickWaves.ViewModels;

namespace DcmsMobile.PickWaves.Areas.PickWaves.ManageWaves
{
    /// <summary>
    /// Boxes list of passed bucket
    /// </summary>
    public class WaveBoxListModel: ViewModelBase
    {
        [Obsolete("use bucketmodel")]
        public int BucketId { get; set; }

        public BucketModel Bucket { get; set; }

        public BucketActivityType ActivityFilter { get; set; }

        public IList<BoxModel> BoxesList { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int TotalOrderedPieces
        {
            get
            {
                return this.BoxesList.Sum(p => p.ExpectedPieces ?? 0);
            }
        }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int TotalPiecesInBox
        {
            get
            {
                return this.BoxesList.Sum(p => p.CurrentPieces ?? 0);
            }
        }

    }
}