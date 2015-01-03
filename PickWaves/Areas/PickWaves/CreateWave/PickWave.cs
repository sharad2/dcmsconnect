using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.PickWaves.Areas.PickWaves.CreateWave
{
    /// <summary>
    /// </summary>
    /// <remarks>
    /// Represents those properties of a bucket which are useful while creating or editing a bucket.
    /// </remarks>
    internal class PickWave
    {
        [Key]
        public int BucketId { get; set; }

        public int PickslipCount { get; set; }

        public string PullAreaShortName { get; set; }

        public string PitchAreaShortName { get; set; }
    }
}