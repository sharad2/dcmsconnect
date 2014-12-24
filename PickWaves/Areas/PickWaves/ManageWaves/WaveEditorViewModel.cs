using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DcmsMobile.PickWaves.Areas.PickWaves.ManageWaves
{
    public class WaveEditorViewModel
    {
        public string CustomerId { get; set; }

        public string CustomerName { get; set; }

        public int BucketId { get; set; }

        public string BucketName { get; set; }

        public string PriorityId { get; set; }

        public string PullAreaId { get; set; }

        public IList<SelectListItem> PullAreaList { get; set; }

        public bool RequiredBoxExpediting { get; set; }

        public string PitchAreaId { get; set; }

        public IList<SelectListItem> PitchAreaList { get; set; }

        public bool QuickPitch { get; set; }

        public int? PitchLimit { get; set; }

        public string BucketComment { get; set; }

        public bool UnfreezeWaveAfterSave { get; set; }

        public int PiecesIncomplete { get; set; }

        public string PullAreaShortName { get; set; }

        public string PitchAreaShortName { get; set; }
    }
}