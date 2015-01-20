using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using DcmsMobile.PickWaves.Helpers;
using DcmsMobile.PickWaves.Repository;

namespace DcmsMobile.PickWaves.ViewModels
{
    [Flags]
    public enum BucketModelFlags
    {
        Default,

        ///// <summary>
        ///// By default menu to edit/freeze/unfreeze bucket is not rendered. This flag renders it.
        ///// </summary>
        //ShowEditMenu,

        /// <summary>
        /// By default a link to the bucket viewer page is displayed. This flag hides it. It is set by the viewer page itself to prefent linking to self.
        /// </summary>
        HideViewerLink
    }
    /// <summary>
    /// Contains properties of a bucket
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BucketModel
    {
        //private readonly IList<BucketActivityModel> _activities;
        public BucketModel()
        {
            Activities2 = new Dictionary<BucketActivityType, BucketActivityModel>
            {
                {BucketActivityType.Pitching, new BucketActivityModel()},
                {BucketActivityType.Pulling, new BucketActivityModel()}
            };
        }

        internal BucketModel(BucketWithActivities src, string customerName, BucketModelFlags flags)
        {
            CustomerName = customerName;
            //_activities = new List<BucketActivityModel>(3);
            BucketId = src.BucketId;
            BucketName = src.BucketName;
            BucketComment = src.BucketComment;
            IsFrozen = src.IsFrozen;
            PriorityId = src.PriorityId;
            QuickPitch = src.QuickPitch;
            PitchLimit = src.PitchLimit;
            CreatedBy = src.CreatedBy;
            CreationDate = src.CreationDate;

            //// Pitching Bucket
            //var activity = new BucketActivityModel(src.Activities[BucketActivityType.Pitching]);
            //Activities.Add(activity);

            //// Pulling Bucket
            //activity = new BucketActivityModel(src.Activities[BucketActivityType.Pulling]);
            //Activities.Add(activity);

            Activities2 = new Dictionary<BucketActivityType, BucketActivityModel>
            {
                {BucketActivityType.Pitching, new BucketActivityModel(src.Activities[BucketActivityType.Pitching])},
                {BucketActivityType.Pulling, new BucketActivityModel(src.Activities[BucketActivityType.Pulling])}
            };

            CountPickslips = src.CountPickslips;
            CountPurchaseOrder = src.CountPurchaseOrder;
            //MinPoId = src.MinPoId;
            //MaxPoId = src.MaxPoId;
            CustomerId = src.MaxCustomerId;
            //MaxCustomerName = src.MaxCustomerName;

            DcCancelDateRange = new DateRange
            {
                From = src.MinDcCancelDate,
                To = src.MaxDcCancelDate
            };

            //var pcs = src.Activities.Sum(p => p.Stats.GetPieces(PiecesKind.Expected, new[] { BoxState.Completed, BoxState.InProgress, BoxState.Cancelled })) ?? 0;
            var pcs = src.Activities.Sum(p => new[] { p.Stats[PiecesKind.Expected, BoxState.Completed],
                p.Stats[PiecesKind.Expected, BoxState.InProgress],
                p.Stats[PiecesKind.Expected, BoxState.Cancelled] }.Sum());
            if (pcs != this.OrderedPieces)
            {
                this.BoxNotCreatedPieces = this.OrderedPieces - (pcs ?? 0);
            }

            OrderedPieces = src.OrderedPieces;



            ProgressStage state;
            if (src.IsFrozen)
            {
                state = ProgressStage.Frozen;
            }
            else if (PiecesRemaining == 0 && this.BoxesTotal == this.BoxesValidated)
            {
                state = ProgressStage.Completed;
            }
            else
            {
                state = ProgressStage.InProgress;
            }

            BucketState = state;

            RequiredBoxExpediting = src.RequireBoxExpediting;

            CountAssignedSku = src.CountAssignedSku;
            CountTotalSku = src.CountTotalSku;

            Flags = flags;
        }

        /// <summary>
        /// Whether links to the bucket editor should be displayed
        /// </summary>
        public BucketModelFlags Flags { get; set; }

        #region Bucket

        [Display(Name = "Pick Wave")]
        public int BucketId { get; set; }

        [Display(Name = "Name")]
        public string BucketName
        {
            get;
            set;
        }

        /// <summary>
        /// Comment of the bucket
        /// </summary>
        [Display(Name = "Remark")]
        [DisplayFormat(NullDisplayText = "Not Specified")]
        public string BucketComment { get; set; }

        private int _priority;

        [Display(Name = "Priority")]
        public int PriorityId
        {
            get
            {
                return _priority == 0 ? 1 : _priority;
            }
            set
            {
                _priority = value;
            }
        }

        [Display(Name = "Required Box Expediting")]
        public bool RequiredBoxExpediting { get; set; }

        [Display(Name = "Quick Pitch")]
        public bool QuickPitch { get; set; }

        [Display(Name = "Freeze")]
        public bool IsFrozen { get; set; }

        public DateTime? CreationDate { get; set; }

        public string CreatedBy { get; set; }

        /// <summary>
        /// Showing piece limit on pitch cart
        /// </summary>
        [Display(Name = "Pitch Limit")]
        public int? PitchLimit { get; set; }

        #endregion

        //[Obsolete]
        //public IList<BucketActivityModel> Activities
        //{
        //    get
        //    {
        //        return _activities;
        //    }
        //}

        public IDictionary<BucketActivityType, BucketActivityModel> Activities2 { get; set; }

        [Display(Name = "#Pickslips")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int CountPickslips { get; set; }

        /// <summary>
        /// Number of purchase orders in the bucket
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int CountPurchaseOrder { get; set; }


        /// <summary>
        /// One of the customers of this bucket
        /// </summary>
        [Display(Name = "Customer")]
        public string CustomerId { get; set; }

        public string CustomerName { get; set; }

        /// <summary>
        /// Created, In Progress, Complete
        /// </summary>
        public ProgressStage BucketState { get; set; }

        public string StateDisplayName
        {
            get
            {
                return PickWaveHelpers.GetEnumMemberAttributes<ProgressStage, DisplayAttribute>()[this.BucketState].Name;
            }
        }

        [Display(Name = "DC Cancel Date")]
        [DataType(DataType.Text)]
        public DateRange DcCancelDateRange { get; set; }

        #region Boxes
        /// <summary>
        /// Number of boxes created for this bucket
        /// </summary>
        [Display(Name = "Created Boxes")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? BoxesTotal
        {
            get
            {
                return this.Activities2.Sum(p => p.Value.BoxesComplete) + this.Activities2.Sum(p => p.Value.BoxesNotStarted) + 
                    this.Activities2.Sum(p => p.Value.BoxesCancelled) + this.Activities2.Sum(p => p.Value.BoxesInprogress);
            }
        }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        [Obsolete]
        public int? BoxesCancelled
        {
            get
            {
                return this.Activities2.Sum(p => p.Value.BoxesCancelled);
            }
        }


        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? BoxesValidated
        {
            get
            {
                return this.Activities2.Sum(p => p.Value.BoxesComplete);
            }
        }




        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? BoxesRemaining
        {
            get
            {
                return this.Activities2.Sum(p => p.Value.BoxesRemaining);
            }
        }



        #endregion

        #region Pieces

        /// <summary>
        /// Pieces in cancelled boxes
        /// </summary>
        [Obsolete]
        public int? CancelledPieces
        {
            get
            {
                return this.Activities2.Sum(p => p.Value.PiecesCancelled);
            }
        }



        [Display(Name = "Ordered Pieces")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int OrderedPieces { get; set; }

        /// <summary>
        /// How many pieces for which boxes have not been created
        /// </summary>
        public int? BoxNotCreatedPieces
        {
            get;
            private set;
        }

        /// <summary>
        /// Are we overshipping? This should always be negative or 0
        /// </summary>
       
        public int? OverShippedPieces
        {
            get
            {
                if (this.PiecesComplete <= this.OrderedPieces)
                {
                    // Normal case
                    return null;
                }
                return this.PiecesComplete - this.OrderedPieces;
            }
        }

        /// <summary>
        /// Total number of pieces which are pulled and pitched, i.e. PickedPieces + PulledPieces
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? PiecesComplete
        {
            get
            {
                return this.Activities2.Sum(p => p.Value.PiecesComplete);
            }
        }

        /// <summary>
        /// Number of pieces which have not yet reached their respective box, i.e. OrderedPieces - PiecesInBox
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? PiecesRemaining
        {
            get
            {
                return this.Activities2.Sum(p => p.Value.PiecesRemaining);
            }

        }

        /// <summary>
        /// Number of pieces which have not yet reached their respective box, i.e. OrderedPieces - PiecesInBox
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:N0}")]
       [Obsolete]
        public int? PiecesToShip
        {
            get
            {
                return this.PiecesRemaining + PiecesComplete;
            }

        }

        /// <summary>
        /// % w.r.t. pieces complete + pieces incomplete
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:p0}")]
        public decimal? PercentPiecesComplete
        {
            get
            {
                if (OrderedPieces == 0 || PiecesComplete == 0)
                {
                    return 0;
                }
                return PiecesComplete / (decimal)OrderedPieces;
            }
        }

        #endregion

        #region Sku Assigned

        internal int CountAssignedSku { get; set; }

        internal int CountTotalSku { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int CountNotAssignedSku
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(this.Activities2[BucketActivityType.Pitching].AreaId))
                {
                    if (this.CountTotalSku > this.CountAssignedSku)
                    {
                        return (this.CountTotalSku - this.CountAssignedSku);
                    }
                }
                return 0;
            }
        }

        #endregion

        /// <summary>
        /// Pull area short name of the pick wave
        /// </summary>
        [DisplayFormat(NullDisplayText = "Not decided")]
        public string PullAreaShortName
        {
            get
            {
                return this.Activities2[BucketActivityType.Pulling].AreaShortName;
            }
        }

        /// <summary>
        /// Pull area short name of the pick wave
        /// </summary>
        [DisplayFormat(NullDisplayText = "Not decided")]
        public string PitchAreaShortName
        {
            get
            {
                return this.Activities2[BucketActivityType.Pitching].AreaShortName;
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
