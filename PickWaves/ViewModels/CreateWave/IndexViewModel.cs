using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Routing;
using DcmsMobile.PickWaves.Repository.CreateWave;
using EclipseLibrary.Mvc.Helpers;

namespace DcmsMobile.PickWaves.ViewModels.CreateWave
{
    public class CreateWaveAreaModel : InventoryAreaModel
    {
        public CreateWaveAreaModel(CreateWaveArea entity) : base(entity)
        {
            this.CountSku = entity.CountSku;
            if (this.CountSku == 0)
            {
                // Treat as null
                this.CountSku = null;
            }
            this.CountOrderedSku = entity.CountOrderedSku;
        }

        public int? CountSku { get; set; }

        public int? CountOrderedSku { get; set; }

        public int PercentSkusInArea
        {
            get
            {
                if (this.CountOrderedSku == 0 || this.CountSku == null || this.CountSku == 0)
                {
                    return 0;
                }
                return (int)Math.Round((decimal)this.CountSku * 100 / (decimal)this.CountOrderedSku);
            }
        }

    }

    /// <summary>
    /// The unbinder is capable of handling many properties.
    /// </summary>
    public class IndexViewModel : PickslipMatrixPartialViewModel
    {
        public IndexViewModel()
        {
        }

        public IndexViewModel(string customerId)
        {
            CustomerId = customerId;
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
        
        #region Posted Values

        [Display(Name="Pulling")]
        public string PullAreaId { get; set; }

        [Display(Name = "Pitching")]
        public string PitchAreaId { get; set; }

        [Display(Name="Require Box Expediting")]
        public bool RequireBoxExpediting { get; set; }

        /// <summary>
        /// Whether user wants to allow pulling. Note that the PullAreaId should ignored if this is posted as false
        /// </summary>
        public bool AllowPulling { get; set; }

        /// <summary>
        /// Whether user wants to allow pitching. Note that the PitchAreaId should ignored if this is posted as false
        /// </summary>
        public bool AllowPitching { get; set; }
        #endregion

        public IList<CreateWaveAreaModel> PullAreaList { get; set; }

        public int CountVisiblePullAreas
        {
            get
            {
                if (PullAreaList == null)
                {
                    return 0;
                }
                return this.PullAreaList.Count(p => p.PercentSkusInArea >= 40);
            }
        }

        public IList<CreateWaveAreaModel> PitchAreaList { get; set; }

        public int CountVisiblePitchAreas
        {
            get
            {
                if (PitchAreaList == null)
                {
                    return 0;
                }
                return this.PitchAreaList.Count(p => p.PercentSkusInArea >= 40);
            }
        }
    }

    internal class IndexViewModelUnbinder : PickslipMatrixPartialViewModelUnbinder
    {
        public override void UnbindModel(RouteValueDictionary routeValueDictionary, string routeName, object value)
        {
            base.UnbindModel(routeValueDictionary, routeName, value);
            var model = value as IndexViewModel;

            if (model.LastBucketId.HasValue)
            {
                routeValueDictionary.Add(model.NameFor(m => m.LastBucketId), model.LastBucketId);
            }

            // After a bucket has been created, show these settings as default
            if (!string.IsNullOrWhiteSpace(model.PitchAreaId))
            {
                routeValueDictionary.Add(model.NameFor(m => m.PitchAreaId), model.PitchAreaId);
            }
            if (!string.IsNullOrWhiteSpace(model.PitchAreaId))
            {
                routeValueDictionary.Add(model.NameFor(m => m.PullAreaId), model.PullAreaId);
            }
            if (model.RequireBoxExpediting)
            {
                routeValueDictionary.Add(model.NameFor(m => m.RequireBoxExpediting), model.RequireBoxExpediting);
            }
        }
    }
}