using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace DcmsMobile.PickWaves.Areas.PickWaves.ManageWaves
{
    public class WaveEditorViewModel: IValidatableObject
    {
        public WaveEditorViewModel()
        {
                
        }
        internal WaveEditorViewModel(BucketEditable entity)
        {
            this.BucketComment = entity.BucketComment;
            this.BucketId = entity.BucketId;
            this.BucketName = entity.BucketName;
            this.CustomerId = entity.CustomerId;
            this.PitchAreaId = entity.PitchAreaId;
            this.PitchAreaShortName = entity.PitchAreaShortName;
            this.PullAreaId = entity.PullAreaId;
            this.PullAreaShortName = entity.PullAreaShortName;
            this.PitchLimit = entity.PitchLimit;
            this.QuickPitch = entity.QuickPitch;
            this.RequiredBoxExpediting = entity.RequireBoxExpediting;

        }
      
        public string CustomerId { get; set; }

        public string CustomerName { get; set; }

        public int BucketId { get; set; }

         [Required(ErrorMessage="Bucket Name is Required.")]
        public string BucketName { get; set; }

        //public int PriorityId { get; set; }

        public string PullAreaId { get; set; }

        public IList<SelectListItem> PullAreaList { get; set; }

        public bool RequiredBoxExpediting { get; set; }

        public string PitchAreaId { get; set; }

        public IList<SelectListItem> PitchAreaList { get; set; }

        public bool QuickPitch { get; set; }

        [Required(ErrorMessage = "Pitch Limit is Required.")]
        public int? PitchLimit { get; set; }

        public string BucketComment { get; set; }

        public bool UnfreezeWaveAfterSave { get; set; }

        public string PullAreaShortName { get; set; }

        public string PitchAreaShortName { get; set; }

        public int PiecesIncomplete { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (this.UnfreezeWaveAfterSave && string.IsNullOrWhiteSpace(this.PitchAreaId) && string.IsNullOrWhiteSpace(this.PullAreaId))
            {
                yield return new ValidationResult("To unfreeze the pickwave you must specify pitch area and/or pull area");
            }
        }
    }
}