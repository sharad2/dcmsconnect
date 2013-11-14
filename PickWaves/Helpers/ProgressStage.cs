using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.PickWaves.Helpers
{
    public enum ProgressStage
    {
        [Display(Name = "Unknown")]
        Unknown,  // Since the value of this entry is 0, it becomes the default value

        [Display(Name = "Frozen")]
        Frozen,

        [Display(Name = "In Progress")]
        InProgress,

        [Display(Name = "Completed")]
        Completed

    }
}