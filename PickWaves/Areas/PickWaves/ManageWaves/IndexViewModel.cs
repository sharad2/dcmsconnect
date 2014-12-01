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
        public string PageTitle
        {
            get
            {
                var state = PickWaveHelpers.GetEnumMemberAttributes<ProgressStage, DisplayAttribute>()[this.BucketState].Name;
                return string.Format("{0} waves of {1}: {2}", state, this.CustomerId,this.CustomerName);
            }
        }

        /// <summary>
        /// The setter pre calculates totals
        /// </summary>
        [ReadOnly(true)]
        public IList<BucketModel> Buckets { get; set; }

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

    internal class IndexViewModelUnbinder : IModelUnbinder<IndexViewModel>
    {
        public void UnbindModel(RouteValueDictionary routeValueDictionary, string routeName, IndexViewModel model)
        {
            if (model == null)
            {
                return;
            }
            if (!string.IsNullOrWhiteSpace(model.CustomerId))
            {
                routeValueDictionary.Add(model.NameFor(m => m.CustomerId), model.CustomerId);
            }
            if (model.BucketState != ProgressStage.InProgress)
            {
                // Don't bother to encode the default state
                routeValueDictionary.Add(model.NameFor(m => m.BucketState), (int)model.BucketState);
            }
            if (!string.IsNullOrWhiteSpace(model.UserName))
            {
                routeValueDictionary.Add(model.NameFor(m => m.UserName), model.UserName);
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

