using System;
using System.ComponentModel.DataAnnotations;
using DcmsMobile.PickWaves.Helpers;
using DcmsMobile.PickWaves.Repository;

namespace DcmsMobile.PickWaves.ViewModels
{
    /// <summary>
    /// Represents an activity which can be performed on a bucket to move it closer to completion,
    /// e.g. Checking, Pitching, Pulling
    /// </summary>
    public class BucketActivityModel
    {
        /// <summary>
        /// Void constructor needed because of BucketModelBase template class
        /// </summary>
        public BucketActivityModel()
        {

        }

        internal BucketActivityModel(BucketActivity entity)
        {
            ActivityType = entity.ActivityType;
            AreaId = entity.Area.AreaId;
            AreaShortName = string.IsNullOrWhiteSpace(entity.Area.ShortName) ? entity.Area.AreaId : entity.Area.ShortName;
            AreaDescription = entity.Area.Description;
            BuildingId = entity.Area.BuildingId;
            ReplenishAreaId = entity.Area.ReplenishAreaId;
            if (entity.MinEndDate.HasValue || entity.MaxEndDate.HasValue)
            {
                PickingDateRange = new DateRange
                {
                    From = entity.MinEndDate,
                    To = entity.MaxEndDate,
                    ShowTime = true
                };
            }

            PiecesComplete = entity.Stats[PiecesKind.Current, new[] {BoxState.Completed, BoxState.InProgress}] ?? 0;
            PiecesIncomplete = (entity.Stats[PiecesKind.Expected, BoxState.InProgress] ?? 0) - (entity.Stats[PiecesKind.Current, BoxState.InProgress] ?? 0);
            PiecesBoxesCreated = entity.Stats[PiecesKind.Expected, new[] {BoxState.Cancelled, BoxState.InProgress, BoxState.Completed}] ?? 0;

            var pcs = (entity.Stats[PiecesKind.Expected, BoxState.Completed] ?? 0) - (entity.Stats[PiecesKind.Current, BoxState.Completed] ?? 0);
            if (pcs > 0)
            {
                UnderPickedPieces = pcs;
            }
            pcs = entity.Stats[PiecesKind.Expected, BoxState.Cancelled] ?? 0;
            if (pcs > 0)
            {
                CancelledPieces = pcs;
            }
            CountBoxesCreated = entity.Stats[new[] {BoxState.Completed, BoxState.InProgress, BoxState.NotStarted}];

            CountBoxesIncomplete = entity.Stats[new[] {BoxState.InProgress, BoxState.NotStarted}];
        }

        public BucketActivityType ActivityType { get; set; }

        /// <summary>
        /// Name of this activity
        /// </summary>
        public string DisplayName
        {
            get
            {
                switch (this.ActivityType)
                {
                    case BucketActivityType.Pitching:
                        return "Pitching";

                    case BucketActivityType.Pulling:
                        return "Pulling";

                    default:
                        return this.ActivityType.ToString();
                }

            }
        }

        public string AreaShortName { get; set; }

        public string AreaDescription { get; set; }

        public string BuildingId { get; set; }

        /// <summary>
        /// Displays the short name with Building
        /// </summary>
        [DisplayFormat(NullDisplayText = "<span class='ui-state-error'>Undecided</span>", HtmlEncode = false)]
        public string AreaShortNameDisplay
        {
            get
            {
                if (string.IsNullOrEmpty(this.AreaId))
                {
                    return null;
                }
                return string.Format("{0}-{1}", this.BuildingId, this.AreaShortName);
            }
        }

        /// <summary>
        /// The number of pieces which have been pulled or picked
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int PiecesComplete { get; set; }

        /// <summary>
        /// Read only.
        /// The number of pieces for which pulling or picking have not yet been performed.
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int PiecesIncomplete { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? CountBoxesIncomplete { get; set; }

        /// <summary>
        /// Sum of complete and incomplete
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int PiecesToShip
        {
            get
            {
                return PiecesIncomplete + PiecesComplete;
            }
        }

        /// <summary>
        /// Number of pieces in under picked verified boxes
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? UnderPickedPieces { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? CancelledPieces { get; set; }

        public int PercentPiecesComplete
        {
            get
            {
                //return 10;
                if (PiecesComplete + PiecesIncomplete == 0)
                {
                    return 0;
                }

                return (int)Math.Round(PiecesComplete * 100.0 / (PiecesComplete + PiecesIncomplete));
            }
        }

        public int PercentPiecesIncomplete
        {
            get
            {
                //return 10;
                if (PiecesComplete + PiecesIncomplete == 0)
                {
                    return 0;
                }

                return (int)Math.Round(PiecesIncomplete * 100.0 / (PiecesComplete + PiecesIncomplete));
            }
        }

        /// <summary>
        /// Total number of pieces which will be pulled or picked for this bucket
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int PiecesBoxesCreated { get; private set; }

        /// <summary>
        /// Total boxes for this activity
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? CountBoxesCreated { get; set; }

        public string AreaId { get; set; }

        /// <summary>
        /// Carton area from which this pick area is replenished
        /// </summary>
        public string ReplenishAreaId { get; set; }

        [DataType(DataType.Text)]
        [DisplayFormat(NullDisplayText = "Not Started")]
        public DateRange PickingDateRange { get; set; }




    }
}