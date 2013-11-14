using System;
using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.PickWaves.Helpers
{
    [Flags]
    public enum BoxState
    {
        [Display(Name="All")]
        NotSet = 0x0,

        [Display(Name = "Not Started")]
        NotStarted = 0x1,

        /// <summary>
        /// Not validated
        /// </summary>
        [Display(Name = "In Progress")]
        InProgress = 0x2,

        /// <summary>
        /// Means box has been validated
        /// </summary>
        [Display(Name = "Completed")]
        Completed = 0x4,

        /// <summary>
        /// Cancelled
        /// </summary>
        [Display(Name = "Cancelled")]
        Cancelled = 0x8


    }
}