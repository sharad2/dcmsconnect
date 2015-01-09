using DcmsMobile.PickWaves.Repository;
using DcmsMobile.PickWaves.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace DcmsMobile.PickWaves.Areas.PickWaves.ManageWaves
{
    public class ManageWavesPickslipModel
    {
        public ManageWavesPickslipModel()
        {

        }

        internal ManageWavesPickslipModel(Pickslip entity)
        {
            PickslipId = entity.PickslipId;
            PurchaseOrder = entity.PurchaseOrder;
            VwhId = entity.VwhId;
            //CancelDate = entity.CancelDate;
            //PickslipImportDate = entity.PickslipImportDate;
            //StartDate = entity.StartDate;
            CustomerDcId = entity.CustomerDcId;
            CustomerStoreId = entity.CustomerStoreId;
            OrderedPieces = entity.OrderedPieces;
            CurrentPieces = entity.CurrentPieces;
            CancelledBoxCount = entity.CancelledBoxCount;
            BoxCount = entity.BoxCount;


        }
        [Display(Name = "Pickslip")]
        public long PickslipId { get; set; }

        [Display(Name = "Purchase Order")]
        public string PurchaseOrder { get; set; }

        [Display(Name = "Customer DC ID")]
        public string CustomerDcId { get; set; }

        [Display(Name = "Customer Store ID")]
        public string CustomerStoreId { get; set; }

        [Display(Name = "VWH")]
        public string VwhId { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int OrderedPieces { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int CurrentPieces { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int UnPickedPieces
        {
            get
            {
                return this.OrderedPieces - this.CurrentPieces;
            }
        }

        public int PercentCurrentPieces
        {
            get
            {
                if (CurrentPieces == 0 || OrderedPieces == 0)
                {
                    return 0;
                }
                return (int)Math.Round((decimal)this.CurrentPieces * 100 / (decimal)this.OrderedPieces);
            }
        }

        /// <summary>
        /// Total number of cancelled boxes in this pickslip
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int CancelledBoxCount { get; set; }

        public int BoxCount { get; set; }

        public bool IsFrozenWave { get; set; }

        public string UrlInquiryPickslip { get; set; }

        public string UrlInquiryPurchaseOrder { get; set; }


    }


    public class WavePickslipsViewModel: ViewModelBase
    {
        public BucketModel Bucket { get; set; }

        [Obsolete("Use BucketId in BucketModel")]
        public int BucketId { get; set; }

        [Obsolete("Use IsFrozenBucket in BucketModel")]
        public bool IsFrozenBucket { get; set; }

        [Obsolete]
        public string CustomerId { get; set; }

        public IList<ManageWavesPickslipModel> PickslipList { get; set; }

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