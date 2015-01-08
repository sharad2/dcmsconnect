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

        internal IndexBucketModel(BucketWithActivities entity)
        {
            this.BucketId = entity.BucketId;
            this.OrderedPieces = entity.OrderedPieces;
            this.Priority = entity.PriorityId;
            this.PickslipCount = entity.CountPickslips;
            this.PoCount = entity.CountPurchaseOrder;

            //TODO: Other properties available in Bucketbase. DC Cancel.
        }

        [Display(Name = "Pick Wave")]
        public int BucketId { get; set; }

        [Display(Name = "Ordered Pieces")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int OrderedPieces { get; set; }


        public int Priority { get; set; }

        public int PickslipCount { get; set; }

        public int PoCount { get; set; }


    }
    /// <summary>
    /// ReadOnly(false) attribute explicitly clarifies what we expect to see posted. MVC's DefaultModelBinder respects this.
    /// </summary>
    public class IndexViewModel : ViewModelBase
    {
        /// <summary>
        /// Needed for model binder
        /// </summary>
        public IndexViewModel()
        {
            this.Buckets_O = new BucketModel[0];
            this.BucketState = ProgressStage.InProgress;
        }

        /// <summary>
        /// Displays a list of buckets for the passed customer and bucket status
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="stateIndex"></param>
        public IndexViewModel(string customerId, ProgressStage state, string userName)
        {
            this.CustomerId = customerId;
            this.Buckets_O = new BucketModel[0];
            this.BucketState = state;
            this.UserName = userName;
        }

        /// <summary>
        /// Title of the browser windows
        /// </summary>
        public string BucketStateDisplayName
        {
            get
            {
                var state = PickWaveHelpers.GetEnumMemberAttributes<ProgressStage, DisplayAttribute>()[this.BucketState].Name;
                return state;
            }
        }

        public IList<IndexBucketModel> Buckets { get; set; }

        [Obsolete]
        public IList<BucketModel> Buckets_O { get; set; }

        [DisplayFormat(DataFormatString="{0:N0}")]
        public int BucketCount
        {
            get
            {
                if (this.Buckets_O == null)
                {
                    return 0;
                }
                return Buckets_O.Count;
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

        public ProgressStage BucketState { get; set; }
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

