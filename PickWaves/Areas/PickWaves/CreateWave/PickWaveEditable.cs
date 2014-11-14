using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.PickWaves.Repository.CreateWave
{
    /// <summary>
    /// </summary>
    /// <remarks>
    /// Represents those properties of a bucket which are useful while creating or editing a bucket.
    /// </remarks>
    public class PickWaveEditable
    {
        [Key]
        public int BucketId { get; set; }

        #region Editable
        /// <summary>
        /// Name of the bucket
        /// </summary>
        public string BucketName { get; set; }

        /// <summary>
        /// Priority Id of Bucket
        /// </summary>
        public int PriorityId { get; set; }

        /// <summary>
        /// If bucket is pull , set it as not null.
        /// Set "Y" For normal bucket
        /// Set "N" for Expedite bucket
        /// </summary>
        public string PullingBucket { get; set; }

        public bool QuickPitch { get; set; }

        public string PullAreaId { get; set; }

        public string PitchAreaId { get; set; }
        #endregion

        #region Non editable

        public int PickslipCount { get; set; }

        public string PullAreaShortName { get; set; }

        public string PitchAreaShortName { get; set; }

        #endregion
    }
}