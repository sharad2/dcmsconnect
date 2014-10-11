using System;

namespace DcmsMobile.PieceReplenish.Repository.Home
{
    /// <summary>
    /// Keeps details about the job which is responsible for refreshing the materialized view
    /// </summary>
    public class JobRefresh
    {
        /// <summary>
        /// When view last updated
        /// </summary>
        public DateTime? LastRefreshedTime { get; set; }

        /// <summary>
        /// job status will be either RUNNING or SCHEDULED
        /// </summary>
        public string Status { get; set; }

        public DateTime? NextRunDate { get; set; }

        public bool IsRefreshingNow { get; set; }
    }
}