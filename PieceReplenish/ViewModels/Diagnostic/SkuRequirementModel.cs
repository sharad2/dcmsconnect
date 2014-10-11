using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.PieceReplenish.ViewModels.Diagnostic
{
    public class SkuRequirementModel
    {
        [Key]
        public string BuildingId { get; set; }

        [Key]
        public string PickAreaId { get; set; }

        public string ShortName { get; set; }

        [Key]
        public string LocationId { get; set; }

        public string VwhId { get; set; }

        public string RestockAreaId { get; set; }

        public string ReplenishAreaId { get; set; }

        public string RestockAisleId { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int PiecesAtLocation { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? LocationCapacity { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int PiecesRequiredAtLocation { get; set; }

        public string LocationType { get; set; }

        public bool IsFrozen { get; set; }
    }
}