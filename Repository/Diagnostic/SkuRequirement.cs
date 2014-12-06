using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.PieceReplenish.Repository.Diagnostic
{
    public class SkuRequirement
    {
        [Key]
        public Sku Sku { get; set; }

        public string VwhId { get; set; }

        public string BuildingId { get; set; }

        public string PickAreaId { get; set; }

        public string ShortName { get; set; }

        public string RestockAreaId { get; set; }

        public string ReplenishAreaId { get; set; }

        public string LocationId { get; set; }

        public string RestockAisleId { get; set; }

        public int PiecesAtLocation { get; set; }

        public int? LocationCapacity { get; set; }

        public int PiecesRequiredAtLocation { get; set; }

        public string LocationType { get; set; }

        public bool IsFrozen { get; set; }
    }
}