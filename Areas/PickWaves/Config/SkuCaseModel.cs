using System.ComponentModel.DataAnnotations;
using DcmsMobile.PickWaves.Repository.Config;

namespace DcmsMobile.PickWaves.ViewModels.Config
{
    public class SkuCaseModel
    {
        public SkuCaseModel()
        {

        }

        
       
        public SkuCaseModel(SkuCase entity)
        {
            this.CaseId = entity.CaseId;
            this.Description = entity.Description;
            this.EmptyWeight = entity.EmptyWeight;
            this.IsAvailable = entity.IsAvailable;
            this.MaxContentVolume = entity.MaxContentVolume;
            this.OuterCubeVolume = entity.OuterCubeVolume;
        }

        private string _caseId;
        [Required(ErrorMessage = "Case cannot be null.")]
        public string CaseId
        {
           get
            {
                return _caseId;
            }
            set
            {
                _caseId = value.ToUpper();
            }
        }

        public string Description { get; set; }

        [Required(ErrorMessage = "Empty weight cannot be null.")]
        public decimal? EmptyWeight { get; set; }

        [Required(ErrorMessage = "Max content volume cannot be null.")]
        public decimal? MaxContentVolume { get; set; }

        public bool IsAvailable { get; set; }

        [Required(ErrorMessage = "Outer Cube volume cannot be null.")]
        public decimal? OuterCubeVolume { get; set; }

    }
}