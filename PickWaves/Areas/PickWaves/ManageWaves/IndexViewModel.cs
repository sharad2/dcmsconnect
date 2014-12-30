using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System.Web.Routing;
using DcmsMobile.PickWaves.Helpers;
using EclipseLibrary.Mvc.Helpers;
using DcmsMobile.PickWaves.ViewModels;

namespace DcmsMobile.PickWaves.Areas.PickWaves.ManageWaves
{
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
            this.Buckets = new BucketModel[0];
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
            this.Buckets = new BucketModel[0];
            this.BucketState = state;
            this.UserName = userName;
        }

        /// <summary>
        /// Title of the browser windows
        /// </summary>
        public string PickWaveState
        {
            get
            {
                var state = PickWaveHelpers.GetEnumMemberAttributes<ProgressStage, DisplayAttribute>()[this.BucketState].Name;
                return state;
            }
        }

        /// <summary>
        /// The setter pre calculates totals
        /// </summary>
        [ReadOnly(true)]
        public IList<BucketModel> Buckets { get; set; }

        [DisplayFormat(DataFormatString="{0:N0}")]
        public int BucketCount
        {
            get
            {
                if (this.Buckets == null)
                {
                    return 0;
                }
                return Buckets.Count;
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

