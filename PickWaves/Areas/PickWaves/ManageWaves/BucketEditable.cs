using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DcmsMobile.PickWaves.Areas.PickWaves.ManageWaves
{
    /// <summary>
    /// Contains properties of bucket which can be edited
    /// </summary>
    internal class BucketEditable
    {
        public string PullAreaId { get; set; }

        public string PitchAreaId { get; set; }

        public string BucketName { get; set; }

        public bool RequireBoxExpediting { get; set; }

        public bool QuickPitch { get; set; }

        public int PitchLimit { get; set; }

        public string BucketComment { get; set; }

        /// <summary>
        /// This flag is not used for updating. It is used for returning the current value after the update
        /// </summary>
        public bool IsFrozen { get; set; }

    }
}