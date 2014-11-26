
namespace DcmsMobile.PickWaves.Areas.PickWaves.CreateWave
{
    internal class CreateWaveArea
    {
        /// <summary>
        /// Number of ordered SKU which can be found in this area
        /// </summary>
        public int? CountSku { get; set; }

        public int? CountOrderedSku { get; set; }


        public string AreaId { get; set; }

        public string ShortName { get; set; }

        public string Description { get; set; }

        public string BuildingId { get; set; }

        public Helpers.BucketActivityType AreaType { get; set; }
    }
}