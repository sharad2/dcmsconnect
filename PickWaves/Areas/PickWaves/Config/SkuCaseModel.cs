using System.ComponentModel.DataAnnotations;


namespace DcmsMobile.PickWaves.Areas.PickWaves.Config
{
    public class SkuCaseModel
    {
        public SkuCaseModel()
        {

        }



        internal SkuCaseModel(SkuCase entity)
        {
            this.CaseId = entity.CaseId;
            this.Description = entity.Description;
            this.EmptyWeight = entity.EmptyWeight;
            this.IsAvailable = entity.IsAvailable;
            this.MaxContentVolume = entity.MaxContentVolume;
            this.OuterCubeVolume = entity.OuterCubeVolume;
        }

        private string _caseId;
        [Required(ErrorMessage = "Case is required")]
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

        [Required(ErrorMessage = "Empty weight is required")]
        public decimal? EmptyWeight { get; set; }

        [Required(ErrorMessage = "Max content volume is required")]
        public decimal? MaxContentVolume { get; set; }

        public bool IsAvailable { get; set; }

        [Required(ErrorMessage = "Outer Cube volume is required")]
        public decimal? OuterCubeVolume { get; set; }

    }
}