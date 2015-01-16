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

            PiecesComplete = entity.Stats[PiecesKind.Expected, BoxState.Completed];

            //PiecesRemaining = (entity.Stats.GetPieces(PiecesKind.Expected, new[] { BoxState.InProgress, BoxState.NotStarted }) ?? 0)
            //    - (entity.Stats.GetPieces(PiecesKind.Current, new[] { BoxState.InProgress, BoxState.NotStarted }) ?? 0);

            PiecesRemaining = new[] {
                entity.Stats[PiecesKind.Expected, BoxState.InProgress],
                entity.Stats[PiecesKind.Expected, BoxState.NotStarted],
                -entity.Stats[PiecesKind.Current, BoxState.InProgress],
                -entity.Stats[PiecesKind.Current, BoxState.NotStarted]

            }.Sum();

            PiecesCancelled = entity.Stats.GetPieces(PiecesKind.Expected, new[] { BoxState.Cancelled, BoxState.Completed }) - (entity.Stats[PiecesKind.Current, BoxState.Completed] ?? 0);

            PiecesCancelled = new[] {
                entity.Stats[PiecesKind.Expected, BoxState.Cancelled],
                entity.Stats[PiecesKind.Expected, BoxState.Completed],
                -entity.Stats[PiecesKind.Current, BoxState.Completed],
            }.Sum();

            PiecesBoxesCreated = (entity.Stats.GetPieces(PiecesKind.Expected, new[] { BoxState.InProgress, BoxState.Completed, BoxState.NotStarted, BoxState.Cancelled }) ?? 0);



            //var pcs = (entity.Stats[PiecesKind.Expected, BoxState.Completed] ?? 0) - (entity.Stats[PiecesKind.Current, BoxState.Completed] ?? 0);
            //if (pcs > 0)
            //{
            //    UnderPickedPieces = pcs;
            //}

            CountBoxesIncomplete = (entity.Stats.GetBoxCounts(new[] { BoxState.InProgress, BoxState.NotStarted }) ?? 0);

            CountBoxesCancelled = (entity.Stats.GetBoxCounts(new[] { BoxState.Cancelled }));


            CountBoxesComplete = (entity.Stats.GetBoxCounts(new[] { BoxState.Completed, BoxState.InProgress, BoxState.NotStarted }) ?? 0);

        }

        #region Box Counts

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? CountBoxesIncomplete { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? CountBoxesComplete { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? CountBoxesCancelled { get; set; }

        #endregion

        #region Pieces
        /// <summary>
        /// The number of pieces which work has been completed. This does not include cancelled pieces. After a box is validated,
        /// underpicked pieces are considered to be complete.
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? PiecesComplete { get; set; }

        /// <summary>
        /// The number of pieces for which pulling or picking needs to be performed.
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? PiecesRemaining { get; set; }


        ///// <summary>
        ///// Number of under picked pieces in verified boxes
        ///// </summary>
        //[DisplayFormat(DataFormatString = "{0:N0}")]
        //public int? UnderPickedPieces { get; set; }

        /// <summary>
        /// Sum of expected pieces in cancelled boxes plus under picked pieces in completed boxes
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? PiecesCancelled { get; set; }

        //public int PercentPiecesComplete
        //{
        //    get
        //    {
        //        //return 10;
        //        if (PiecesRemaining == null || PiecesRemaining == 0)
        //        {
        //            return 0;
        //        }

        //        return (int)Math.Round(PiecesComplete * 100.0 / (PiecesComplete + PiecesRemaining.Value));
        //    }
        //}

        //[Obsolete("Rename to PercentPiecesRemainig")]
        //public int PercentPiecesIncomplete
        //{
        //    get
        //    {
        //        //return 10;
        //        if (PiecesComplete + PiecesRemaining == 0)
        //        {
        //            return 0;
        //        }

        //        return (int)Math.Round(PiecesRemaining * 100.0 / (PiecesComplete + PiecesRemaining));
        //    }
        //}

        /// <summary>
        /// Sum of expected pieces in all created boxes. If all boxes have been created, which should be the case for unfrozen buckets, then this should be the same as
        /// pieces ordered.
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int PiecesBoxesCreated { get; private set; }
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