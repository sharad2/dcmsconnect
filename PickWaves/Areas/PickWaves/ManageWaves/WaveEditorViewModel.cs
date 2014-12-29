using DcmsMobile.PickWaves.Repository;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace DcmsMobile.PickWaves.Areas.PickWaves.ManageWaves
{
    public class WaveEditorViewModel
    {
        public WaveEditorViewModel()
        {
                
        }

        internal WaveEditorViewModel(Bucket entity)
        {
            this.BucketComment = entity.BucketComment;
            this.BucketId = entity.BucketId;
            this.BucketName = entity.BucketName;
            this.CustomerId = entity.MaxCustomerId;
            this.CustomerName = entity.MaxCustomerName;
            if (entity.Activities.Contains(Helpers.BucketActivityType.Pitching))
            {
                this.PitchAreaId = entity.Activities[Helpers.BucketActivityType.Pitching].Area.AreaId;
                this.PitchAreaShortName = entity.Activities[Helpers.BucketActivityType.Pitching].Area.ShortName;
            }
            if (entity.Activities.Contains(Helpers.BucketActivityType.Pulling))
            {
                this.PullAreaId = entity.Activities[Helpers.BucketActivityType.Pulling].Area.AreaId;
                this.PullAreaShortName = entity.Activities[Helpers.BucketActivityType.Pulling].Area.ShortName;
            }
            this.PitchLimit = entity.PitchLimit;
            //this.PriorityId = entity.PriorityId;
            this.QuickPitch = entity.QuickPitch;
            this.RequiredBoxExpediting = entity.RequiredBoxExpediting; 
            
        }
        public string CustomerId { get; set; }

        public string CustomerName { get; set; }

        public int BucketId { get; set; }

        public string BucketName { get; set; }

        //public int PriorityId { get; set; }

        public string PullAreaId { get; set; }

        public IList<SelectListItem> PullAreaList { get; set; }

        public bool RequiredBoxExpediting { get; set; }

        public string PitchAreaId { get; set; }

        public IList<SelectListItem> PitchAreaList { get; set; }

        public bool QuickPitch { get; set; }

        [Required]
        public int? PitchLimit { get; set; }

        public string BucketComment { get; set; }

        public bool UnfreezeWaveAfterSave { get; set; }

        public string PullAreaShortName { get; set; }

        public string PitchAreaShortName { get; set; }

        public int PiecesIncomplete { get; set; }
    }
}