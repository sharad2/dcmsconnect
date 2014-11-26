using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using DcmsMobile.PickWaves.Helpers;
using EclipseLibrary.Mvc.Helpers;
using EclipseLibrary.Mvc.Html;
using DcmsMobile.PickWaves.ViewModels;

namespace DcmsMobile.PickWaves.Areas.PickWaves.ManageWaves
{
    /// <summary>
    /// What should the wave details page propose to the user
    /// </summary>
    [Flags]
    public enum SuggestedNextActionType
    {
        NotSet = 0x0,
        BackTo = 0x1,
        CancelEditing = 0x2,
        SearchAgain = 0x4,

        /// <summary>
        /// Just after a wave has been successfully edited
        /// </summary>
        UnfreezeMe = 0x8,
        EditMe = 0x10,

        /// <summary>
        /// Link which will help in freezing other waves
        /// </summary>
        FreezeOthers = 0x20,

        UnfreezeOthers = 0x40
    }

    public class WaveViewModel : ViewModelBase
    {
        public WaveViewModel()
        {

        }
        public WaveViewModel(int bucketId, SuggestedNextActionType actions = SuggestedNextActionType.NotSet, BucketActivityType? activityTypeFilter = null)
        {
            Bucket = new BucketModel
            {
                BucketId = bucketId
            };
            this.HighlightedActions = actions;
            ActivityTypeFilter = activityTypeFilter;
        }

        public BucketModel Bucket { get; set; }


        public SuggestedNextActionType HighlightedActions { get; set; }

        /// <summary>
        /// If this is non null, box, pickslip and SKU details should be shown for this activity type only
        /// </summary>
        public BucketActivityType? ActivityTypeFilter { get; set; }

        public static string BucketSummaryReportUrl
        {
            get
            {
                return System.Configuration.ConfigurationManager.AppSettings["DcmsLiveBaseUrl"] + "Reports/Category_140/R140_02.aspx";
            }
        }

        #region Editing

        /// <summary>
        /// If true, the properties of the pick wave are editable
        /// </summary>
        public bool DisplayEditableWave { get; set; }

        /// <summary>
        /// This value will be posted after editing. We will update the database only if the current value is different from this value
        /// </summary>
        public string BucketNameOriginal { get; set; }

        public int PriorityIdOriginal { get; set; }

        public string PullAreaOriginal { get; set; }

        public string PitchAreaOriginal { get; set; }

        public int? PitchLimitOriginal { get; set; }

        public string BucketCommentOriginal { get; set; }

        public bool RequiredBoxExpeditingOriginal { get; set; }

        public bool QuickPitchOriginal { get; set; }        

        public IDictionary<BucketActivityType, IList<SelectListItem>> BucketAreaLists { get; set; }

        /// <summary>
        /// If user set it true, bucket will be unfrozen after save
        /// </summary>
        [Display(Name = "Unfreeze current pick wave after save.")]
        public bool UnfreezeWaveAfterSave { get; set; }

        #endregion
        
        /// <summary>
        /// If user wants to edit the Wave must show the SKU tab for its inventory availability
        /// </summary>
        public int ActiveTab
        {
            get
            {
                return DisplayEditableWave ? 2 : 0;
            }
        }

     
    }

    internal class WaveViewModelUnbinder : IModelUnbinder<WaveViewModel>
    {

        public void UnbindModel(System.Web.Routing.RouteValueDictionary routeValueDictionary, string routeName, WaveViewModel model)
        {
            if (model.Bucket != null && model.Bucket.BucketId > 0)
            {
                routeValueDictionary.Add(model.NameFor(m => m.Bucket.BucketId), model.Bucket.BucketId);
            }

            if (model.ActivityTypeFilter != null)
            {
                routeValueDictionary.Add(model.NameFor(m => m.ActivityTypeFilter), model.ActivityTypeFilter);
            }

            if (model.HighlightedActions != SuggestedNextActionType.NotSet)
            {
                routeValueDictionary.Add(model.NameFor(m => m.HighlightedActions), model.HighlightedActions);
            }
        }
    }

}