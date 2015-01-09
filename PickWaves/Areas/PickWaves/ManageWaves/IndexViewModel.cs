using DcmsMobile.PickWaves.Helpers;
using DcmsMobile.PickWaves.Repository;
using DcmsMobile.PickWaves.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.PickWaves.Areas.PickWaves.ManageWaves
{
    /// <summary>
    /// The model for the bucket displayed in the list on the Index page
    /// </summary>
    public class IndexBucketModel
    {
        public IndexBucketModel()
        {

        }

        internal IndexBucketModel(CustomerBucket entity)
        {
            this.BucketId = entity.BucketId;
            this.BucketName = entity.BucketName;
            this.BucketComment = entity.BucketComment;
            this.OrderedPieces = entity.OrderedPieces;
            this.Priority = entity.PriorityId;
            this.PickslipCount = entity.CountPickslips;
            this.PoCount = entity.CountPurchaseOrder;
            this.DcCancelDateRange = new DateRange
             {
                 From = entity.MinDcCancelDate,
                 To = entity.MaxDcCancelDate
             };

            this.CreatedBy = entity.CreatedBy;
            this.CreationDate = entity.CreationDate;
            this.IsFrozen = entity.IsFrozen;
            this.PitchLimit = entity.PitchLimit;

            this.PitchAreaBuildingId = entity.PitchAreaBuildingId;
            this.PitchAreaDescription=entity.PitchAreaDescription;
            this.PitchAreaId = entity.PitchAreaId;
            this.PitchAreaShortName = entity.PitchAreaShortName;
            this.ReplenishAreaId = entity.ReplenishAreaId;
          
            this.PullAreaBuildingId = entity.PullAreaBuildingId;
            this.PullAreaDescription = entity.PullAreaDescription;
            this.PullAreaId = entity.PullAreaId;
            this.PullAreaShortName = entity.PullAreaShortName;
          
        }

        [Display(Name = "Pick Wave")]
        public int BucketId { get; set; }

        public string BucketName { get; set; }

        /// <summary>
        /// Comment of the bucket
        /// </summary>
        public string BucketComment { get; set; }

        [Display(Name = "Ordered Pieces")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int OrderedPieces { get; set; }


        public int Priority { get; set; }

        public int PickslipCount { get; set; }

       [DisplayFormat(DataFormatString = "{0:N0}")]
        public int PoCount { get; set; }

        [Display(Name = "DC Cancel Date")]
        [DataType(DataType.Text)]
        public DateRange DcCancelDateRange { get; set; }


        public bool IsFrozen { get; set; }


        public int? PitchLimit { get; set; }       

        public DateTime CreationDate { get; set; }

        public string CreatedBy { get; set; }


        /// <summary>
        /// Pitch Area
        /// </summary>
        public string PitchAreaId { get; set; }

        public string PitchAreaShortName { get; set; }

        public string PitchAreaDescription { get; set; }

        public string PitchAreaBuildingId { get; set; }

        public string ReplenishAreaId { get; set; }


        /// <summary>
        /// Pull Area
        /// </summary>
        public string PullAreaId { get; set; }

        public string PullAreaShortName { get; set; }

        public string PullAreaDescription { get; set; }

        public string PullAreaBuildingId { get; set; }


    }
    /// <summary>
    /// ReadOnly(false) attribute explicitly clarifies what we expect to see posted. MVC's DefaultModelBinder respects this.
    /// </summary>
    public class IndexViewModel : ViewModelBase
    {
        //[Obsolete]
        //public IList<IndexBucketModel> Buckets { get; set; }

        public IList<IndexBucketModel> FrozenBuckets { get; set; }

        public IList<IndexBucketModel> InProgressBuckets { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int BucketCount
        {
            get
            {
                return FrozenBuckets.Count + InProgressBuckets.Count;
            }
        }

        [Display(Name = "Customer")]
        [ReadOnly(false)]
        public string CustomerId { get; set; }

        [ReadOnly(true)]
        public string CustomerName { get; set; }

        public string UserName { get; set; }

        public static string InventoryShortageReportUrl
        {
            get
            {
                return System.Configuration.ConfigurationManager.AppSettings["DcmsLiveBaseUrl"] + "Reports/Category_130/R130_28.aspx";
            }
        }

        public static string UnshippedboxesReportUrl
        {
            get
            {
                return System.Configuration.ConfigurationManager.AppSettings["DcmsLiveBaseUrl"] + "Reports/Category_110/R110_16.aspx";
            }
        }

    }

}

/*
    $Id$ 
    $Revision$
    $URL$
    $Header$
    $Author$
    $Date$
*/

