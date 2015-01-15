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

            PiecesComplete = ((entity.Stats.GetPieces(PiecesKind.Expected, new[] { BoxState.InProgress, BoxState.Completed, BoxState.NotStarted }) ?? 0)
                        - (entity.Stats.GetPieces(PiecesKind.Current, new[] { BoxState.Cancelled }) ?? 0))
                       - (entity.Stats.GetPieces(PiecesKind.Expected, new[] { BoxState.InProgress, BoxState.NotStarted }) ?? 0);


            PiecesRemaining = (entity.Stats.GetPieces(PiecesKind.Expected, new[] { BoxState.InProgress, BoxState.NotStarted }) ?? 0);
            //- (entity.Stats.GetPieces(PiecesKind.Current, new[] { BoxState.InProgress }) ?? 0);


            PiecesBoxesCreated = (entity.Stats.GetPieces(PiecesKind.Expected, new[] { BoxState.InProgress, BoxState.Completed, BoxState.NotStarted }) ?? 0)
                        - (entity.Stats.GetPieces(PiecesKind.Current, new[] { BoxState.Cancelled }) ?? 0);



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
            CountBoxesCreated = (entity.Stats.GetBoxCounts(new[] { BoxState.Completed, BoxState.InProgress, BoxState.NotStarted })?? 0)
                - (entity.Stats.GetBoxCounts(new[] { BoxState.Cancelled })?? 0);

            CountBoxesCancelled = (entity.Stats.GetBoxCounts(new[] { BoxState.Cancelled }));

            var box = (entity.Stats.GetBoxCounts(new[] { BoxState.Completed }) ?? 0);
            if (box > 0)
            {
                CountBoxesComplete = box  - (entity.Stats.GetBoxCounts(new[] { BoxState.Cancelled }) ?? 0);
            }
            else
            {
                CountBoxesComplete = box;
            }
          

         
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
        /// The number of pieces which work has been completed. This includes cancelled pieces. After a box is validated,
        /// underpicked pieces are considered to be complete.
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int PiecesComplete { get; set; }

        /// <summary>
        /// The number of pieces for which pulling or picking needs to be performed.
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int PiecesRemaining { get; set; }



        [DisplayFormat(DataFormatString = "{0:N0}")]
        [Obsolete]
        public int? CountBoxesIncomplete { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? CountBoxesComplete { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? CountBoxesCancelled { get; set; }

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
                if (PiecesComplete + PiecesRemaining == 0)
                {
                    return 0;
                }

                return (int)Math.Round(PiecesComplete * 100.0 / (PiecesComplete + PiecesRemaining));
            }
        }

        [Obsolete("Rename to PercentPiecesRemainig")]
        public int PercentPiecesIncomplete
        {
            get
            {
                //return 10;
                if (PiecesComplete + PiecesRemaining == 0)
                {
                    return 0;
                }

                return (int)Math.Round(PiecesRemaining * 100.0 / (PiecesComplete + PiecesRemaining));
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