using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.PickWaves.Helpers
{
    public enum ProgressStage
    {
        [Display(Name = "Unknown")]
        Unknown,  // Since the value of this entry is 0, it becomes the default value

        /// <summary>
        /// Frozen means Bucket.Freeze = 'Y'
        /// </summary>
        [Display(Name = "Frozen")]
        Frozen,

        /// <summary>
        /// In progress means not frozen and some non validated boxes
        /// </summary>
        [Display(Name = "In Progress")]
        InProgress,

        /// <summary>
        /// Completed means not frozen and all boxes have been validated
        /// </summary>
        [Display(Name = "Completed")]
        Completed

    }
}