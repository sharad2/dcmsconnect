using System;
using System.ComponentModel.DataAnnotations;
using DcmsMobile.PickWaves.Helpers;
using DcmsMobile.PickWaves.Repository;
using System.Linq;

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

            PiecesInProgressExpected = entity.Stats[PiecesKind.Expected, BoxState.InProgress];
            PiecesInProgressCurrent = entity.Stats[PiecesKind.Current, BoxState.InProgress];

            PiecesComplete = entity.Stats[PiecesKind.Current, BoxState.Completed];

            PiecesNotStarted = entity.Stats[PiecesKind.Expected, BoxState.NotStarted];

            PiecesRemaining = new[] {
                entity.Stats[PiecesKind.Expected, BoxState.InProgress],
                entity.Stats[PiecesKind.Expected, BoxState.NotStarted],
                -entity.Stats[PiecesKind.Current, BoxState.InProgress],
                -entity.Stats[PiecesKind.Current, BoxState.NotStarted]

            }.Sum();

         
      
            PiecesCancelled = new[] {
                entity.Stats[PiecesKind.Expected, BoxState.Cancelled],
                entity.Stats[PiecesKind.Expected, BoxState.Completed],
                -entity.Stats[PiecesKind.Current, BoxState.Completed],
            }.Sum();

          
            PiecesBoxesCreated = new[] {
                entity.Stats[PiecesKind.Expected, BoxState.InProgress],
                entity.Stats[PiecesKind.Expected, BoxState.Completed],
                entity.Stats[PiecesKind.Expected, BoxState.NotStarted],
                 entity.Stats[PiecesKind.Expected, BoxState.Cancelled]
            }.Sum();

            BoxesInprogress = entity.Stats.GetBoxCounts(new[] { BoxState.InProgress });

            BoxesCancelled = entity.Stats.GetBoxCounts(new[] { BoxState.Cancelled });

            BoxesNotStarted = entity.Stats.GetBoxCounts(new[] { BoxState.NotStarted });

            BoxesComplete = entity.Stats.GetBoxCounts(new[] { BoxState.Completed}) ?? 0;

            BoxesRemaining = (entity.Stats.GetBoxCounts(new[] { BoxState.InProgress, BoxState.NotStarted }) ?? 0);

        }

        #region Box Counts

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? BoxesRemaining { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? BoxesInprogress { get; set; }



        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? BoxesNotStarted { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? BoxesComplete { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? BoxesCancelled { get; set; }

        #endregion

        #region Pieces
        /// <summary>
        /// The number of pieces which work has been completed. This does not include cancelled pieces. After a box is validated,
        /// underpicked pieces are considered to be complete. PieceComplete is the number of pieces in the box
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? PiecesComplete { get; set; }


        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? PiecesNotStarted { get; set; }

        /// <summary>
        /// Number of Expected pieces in boxes which have started but not yet completed
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? PiecesInProgressExpected { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? PiecesInProgressCurrent { get; set; }

        /// <summary>
        /// The number of pieces for which pulling or picking needs to be performed.
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:N0}")]
        [Obsolete]
        public int? PiecesRemaining { get; set; }


    

        /// <summary>
        /// Sum of expected pieces in cancelled boxes plus under picked pieces in completed boxes
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? PiecesCancelled { get; set; }


        /// <summary>
        /// Sum of expected pieces in all created boxes. If all boxes have been created, which should be the case for unfrozen buckets, then this should be the same as
        /// pieces ordered.
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? PiecesBoxesCreated { get; private set; }
        #endregion

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