using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace DcmsMobile.PickWaves.ViewModels.ManageWaves
{
    public class WavePickslipsViewModel: ViewModelBase
    {
        public BucketModel Bucket { get; set; }

        public IList<PickslipModel> PickslipList { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int TotalOrderedPieces
        {
            get
            {
                return PickslipList == null ? 0 : this.PickslipList.Sum(p => p.OrderedPieces);
            }
        }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int TotalPickedPieces
        {
            get
            {
                return PickslipList == null ? 0 : this.PickslipList.Sum(p => p.CurrentPieces);
            }
        }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int TotalUnpickedPieces
        {
            get
            {
                return PickslipList == null ? 0 : this.PickslipList.Sum(p => p.UnPickedPieces);
            }
        }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int TotalPercentPickedPieces
        {
            get
            {
                if (TotalPickedPieces == 0 || TotalOrderedPieces == 0)
                {
                    return 0;
                }
                return (int)Math.Round((decimal)this.TotalPickedPieces * 100 / (decimal)this.TotalOrderedPieces);
            }
        }
    }
}