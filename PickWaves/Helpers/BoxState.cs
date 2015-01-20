using System;
using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.PickWaves.Helpers
{
    /// <summary>
    /// These states represent the progrssion of boxes, so they can be used for sorting
    /// </summary>
    public enum BoxState
    {
        NotSet,
        NotStarted,

        /// <summary>
        /// Not validated
        /// </summary>
        InProgress,

        /// <summary>
        /// Means box has been validated
        /// </summary>
        Completed,

        /// <summary>
        /// Cancelled
        /// </summary>
        Cancelled
    }
}