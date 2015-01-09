using DcmsMobile.PickWaves.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DcmsMobile.PickWaves.Areas.PickWaves.ManageWaves
{
        /// <summary>
    /// Contains properties of bucket which can be edited
    /// </summary>
    internal class BucketEditable:BucketBase
    {

        public string PullAreaId { get; set; }

        public string PullAreaShortName { get; set; }

        public string PitchAreaId { get; set; }

        public string PitchAreaShortName { get; set; }

        public string CustomerId { get; set; }

    }
}