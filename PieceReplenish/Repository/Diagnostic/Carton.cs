
namespace DcmsMobile.PieceReplenish.Repository.Diagnostic
{
    public class Carton
    {
        public string CartonId { get; set; }

        public string LocationId { get; set; }

        public Sku SkuInCarton { get; set; }

        public int Quantity { get; set; }

        public string AreaId { get; set; }

        public string BuildingId { get; set; }

        public string VwhId { get; set; }

        public string QualityCode { get; set; }

        public bool IsCartonInSuspense { get; set; }

        public bool IsCartonDamage { get; set; }

        public bool IsWorkNeeded { get; set; }

        public bool IsBestQalityCarton { get; set; }
    }
}