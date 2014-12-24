using DcmsMobile.PickWaves.Repository;
using DcmsMobile.PickWaves.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.PickWaves.Areas.PickWaves.CreateWave
{

    public class CreateWavePickslipModel
    {
        internal CreateWavePickslipModel(Pickslip entity)
        {
            PickslipId = entity.PickslipId;
            PurchaseOrder = entity.PurchaseOrder;
            //VwhId = entity.VwhId;
            //CancelDate = entity.CancelDate;
            PickslipImportDate = entity.PickslipImportDate;
            StartDate = entity.StartDate;
            CustomerDcId = entity.CustomerDcId;
            CustomerStoreId = entity.CustomerStoreId;
            //OrderedPieces = entity.OrderedPieces;
            //CurrentPieces = entity.CurrentPieces;
            //CancelledBoxCount = entity.CancelledBoxCount;
            //PiecesInCancelledBoxes = entity.PiecesInCancelledBoxes;
            //BoxCount = entity.BoxCount;
            //IsFrozenWave = entity.IsFrozenWave;
            DcCancelDate=entity.DcCancelDate;

        }

        [Display(Name = "Pickslip")]
        [Key]
        public long PickslipId { get; set; }

        [Display(Name = "Purchase Order")]
        public string PurchaseOrder { get; set; }

        [Display(Name = "Customer DC ID")]
        public string CustomerDcId { get; set; }

        [Display(Name = "Pickslip Import Date")]
        [DisplayFormat(DataFormatString = "{0:d}")]
        public DateTime? PickslipImportDate { get; set; }

        [Display(Name = "Customer Store ID")]
        public string CustomerStoreId { get; set; }

        [Display(Name = "Start Date")]
        [DisplayFormat(DataFormatString = "{0:d}")]
        public DateTime? StartDate { get; set; }

        [Display(Name = "DC Cancel Date")]
        [DisplayFormat(DataFormatString = "{0:d}")]
        public DateTime? DcCancelDate { get; set; }

        public string UrlInquiryPickslip { get; set; }

        public string UrlInquiryPurchaseOrder { get; set; }
    }


    public class PickslipListViewModel :ViewModelBase
    {
        public PickslipListViewModel()
        {
        }

        public string CustomerId { get; set; }

        public PickslipDimension GroupDimIndex { get; set; }

        public PickslipDimension SubgroupDimIndex { get; set; }

        public DimensionValue SubgroupDimVal { get; set; }

        public DimensionValue GroupDimVal { get; set; }

        public string VwhId { get; set; }

        public string GroupDimDisplayName { get; set; }

        public string SubgroupDimDisplayName { get; set; }


        public IList<CreateWavePickslipModel> PickslipList { get; set; }

        public string CustomerName { get; set; }

        /// <summary>
        /// If user wants to add pickslip to specific bucket
        /// </summary>
        public int? BucketId { get; set; }

        public BucketModel Bucket { get; set; }

    }

}