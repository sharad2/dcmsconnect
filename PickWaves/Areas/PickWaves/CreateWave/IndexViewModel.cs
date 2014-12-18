using EclipseLibrary.Mvc.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System.Web.Routing;

namespace DcmsMobile.PickWaves.Areas.PickWaves.CreateWave
{
    /// <summary>
    /// The unbinder is capable of handling many properties.
    /// </summary>
    public class IndexViewModel : PickslipMatrixPartialViewModel
    {
        public IndexViewModel()
        {
        }

        //public IndexViewModel(string customerId)
        //{
        //    CustomerId = customerId;
        //}

        public IndexViewModel(string customerId, int? bucketId = null)
        {
            CustomerId = customerId;
            LastBucketId = bucketId;
        }

        internal IndexViewModel(string customerId, int rowDimIndex, int colDimIndex, string vwhId, string pullAreaId, string pitchAreaId, int? lastBucketId)
        {
            CustomerId = customerId;
            RowDimIndex = rowDimIndex;
            ColDimIndex = colDimIndex;
            VwhId = vwhId;
            PullAreaId = pullAreaId;
            PitchAreaId = pitchAreaId;
            LastBucketId = lastBucketId;
        }

        public string CustomerName { get; set; }

        /// <summary>
        /// Value of dimension
        /// </summary>
        public string DimensionDisplayValue { get; set; }

        /// <summary>
        /// Imported order, customer dc, priority, purchase order etc..
        /// </summary>
        public string DimensionDisplayName { get; set; }

        /// <summary>
        /// Return the bucket Id when bucket is created.
        /// </summary>
        public int? LastBucketId { get; set; }

        #region Posted Values while creating new pick wave

        [Display(Name = "Pulling")]
        public string PullAreaId { get; set; }

        [Display(Name = "Pitching")]
        public string PitchAreaId { get; set; }

        [Display(Name = "Require Box Expediting")]
        public bool RequiredBoxExpediting { get; set; }

        [Display(Name = "Quick Pitch")]
        public bool QuickPitch { get; set; }
        #endregion

        public IList<SelectListItem> PullAreas { get; set; }

        public IList<SelectListItem> PitchAreas { get; set; }

        #region Only for display

        [DisplayFormat(NullDisplayText = "Undecided")]
        public string PullAreaShortName { get; set; }

        [DisplayFormat(NullDisplayText = "Undecided")]
        public string PitchAreaShortName { get; set; }

        public int PickslipCount { get; set; }

        #endregion



        public static string OrderSummaryReportUrl
        {
            get
            {
                return System.Configuration.ConfigurationManager.AppSettings["DcmsLiveBaseUrl"] + "Reports/Category_110/R110_08.aspx";
            }
        }

    }

    //[Obsolete]
    //internal class IndexViewModelUnbinder : PickslipMatrixPartialViewModelUnbinder
    //{
    //    public override void UnbindModel(RouteValueDictionary routeValueDictionary, string routeName, object value)
    //    {
    //        base.UnbindModel(routeValueDictionary, routeName, value);
    //        var model = value as IndexViewModel;

    //        if (model.LastBucketId.HasValue)
    //        {
    //            routeValueDictionary.Add(model.NameFor(m => m.LastBucketId), model.LastBucketId);
    //        }

    //        // After a bucket has been created, show these settings as default
    //        if (!string.IsNullOrWhiteSpace(model.PitchAreaId))
    //        {
    //            routeValueDictionary.Add(model.NameFor(m => m.PitchAreaId), model.PitchAreaId);
    //        }
    //        if (!string.IsNullOrWhiteSpace(model.PullAreaId))
    //        {
    //            routeValueDictionary.Add(model.NameFor(m => m.PullAreaId), model.PullAreaId);
    //        }
    //    }
    //}
}