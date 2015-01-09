using DcmsMobile.PickWaves.Repository;
using System;

namespace DcmsMobile.PickWaves.Areas.PickWaves.ManageWaves
{
    /// <summary>
    /// Represents bucket information retrieved for a specific customer
    /// </summary>
    internal class CustomerBucket:BucketBase
    {
       
        /// <summary>
        /// Total pieces ordered
        /// </summary>
        public int OrderedPieces { get; set; }

        /// <summary>
        /// Number of purchase orders in the bucket.
        /// </summary>
        public int CountPurchaseOrder { get; set; }

        /// <summary>
        /// Number of pickslips in the bucket
        /// </summary>
        internal int CountPickslips { get; set; }

        /// <summary>
        /// Earliest cancel date of the POs within the bucket
        /// </summary>
        public DateTime? MinDcCancelDate { get; set; }

        /// <summary>
        /// Latest cancel date of the POs within the bucket
        /// </summary>
        public DateTime? MaxDcCancelDate { get; set; }




        /// <summary>
        /// Pitch Area
        /// </summary>
        public string PitchAreaId { get; set; }

        public string PitchAreaShortName { get; set; }

        public string PitchAreaDescription { get; set; }

        public string PitchAreaBuildingId { get; set; }

        public string ReplenishAreaId { get; set; }


        /// <summary>
        /// Pull Area
        /// </summary>
        public string PullAreaId { get; set; }

        public string PullAreaShortName { get; set; }

        public string PullAreaDescription { get; set; }

        public string PullAreaBuildingId { get; set; }
    }
}