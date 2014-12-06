using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.PieceReplenish.Repository.Home
{

    /// <summary>
    /// Represents an SKU requirement of an aisle
    /// </summary>
    public class AisleSku
    {
        [Key]
        public string RestockAisleId { get; set; }

        [Key]
        public int SkuId { get; set; }

        /// <summary>
        /// The Virtual warehouse of carton containing this SKU
        /// </summary>
        [Key]
        public string VwhId { get; set; }
         
        public string Style { get; set; }

        public string Color { get; set; }

        public string Dimension { get; set; }

        public string SkuSize { get; set; }

        public string UpcCode { get; set; }

        /// <summary>
        /// Maximum priority of the location within the aisle
        /// </summary>
        public int? SkuReplenishmentPriority { get; set; }

        /// <summary>
        /// Sum of capacities of all locatins within this aisle
        /// </summary>
        public int Capacity { get; set; }

        /// <summary>
        /// Pieces at locations within this aisle
        /// </summary>
        public int? PiecesAtPickLocations { get; set; }

        /// <summary>
        /// Comma seperated list of all user names who have participated in managing priority of locations within this aisle
        /// </summary>
        public string SkuPriorityModifiedBy { get; set; }

        /// <summary>
        /// Total number of pieces which are in Pullable cartons for this aisle
        /// </summary>
        public int PiecesInPullableCarton { get; set; }

        /// <summary>
        /// How many pieces already available in RST of this SKU for this aisle
        /// </summary>
        public int? PiecesAwaitingRestock { get; set; }

        /// <summary>
        /// The best wave priority of the SKU
        /// </summary>
        public int? WavePriority { get; set; }

        /// <summary>
        /// Number of buckets for which the sku is needed
        /// </summary>
        public int? WaveCount { get; set; }

        /// <summary>
        /// Number of cartons destined for this aisle
        /// </summary>
        public int CartonsToPull { get; set; }

        public int? CartonsInRestock { get; set; }

        /// <summary>
        /// Number of pieces we have decided to pick
        /// </summary>
        public int? PiecesToPick { get; set; }
    }
}