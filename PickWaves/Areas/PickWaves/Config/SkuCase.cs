
namespace DcmsMobile.PickWaves.Repository.Config
{
    internal class SkuCase
    {
        public string CaseId { get; set; }

        public string Description { get; set; }

        public decimal? EmptyWeight { get; set; }

        public decimal? MaxContentVolume { get; set; }

        public bool IsAvailable { get; set; }

        public decimal? OuterCubeVolume { get; set; }        

    }
}