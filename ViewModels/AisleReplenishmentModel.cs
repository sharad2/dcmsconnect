using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.PieceReplenish.ViewModels
{
    /// <summary>
    /// Statistics per priority/restock aisle
    /// </summary>
    /// <remarks>
    /// The UI displays model multiple times for different groupings. Gruping 1: priority/restock aisle; Grouping 2: SKU
    /// </remarks>
    public class AisleReplenishmentModel
    {
        [Key]
        public string RestockAisleId { get; set; }

        /// <summary>
        /// Number of cartons which will be pulled from BIR for the group
        /// </summary>
        [Display(Name = "# Cartons")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int CartonsToPull { get; set; }

        /// <summary>
        /// Number of cartons which will be restocked from RST for the group
        /// </summary>
        [Display(Name = "# Cartons")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int CartonsInRestock { get; set; }

        /// <summary>
        /// Pieces to be pulled from BIR of the SKUs in this group
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int TotalPiecesToPull { get; set; }

        /// <summary>
        /// Pieces in RST of the SKUs in this group
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? PiecesInRestock { get; set; }

        /// <summary>
        /// For the SKUs in the group, how many pieces are already at pick locations
        /// </summary>
        public int? PiecesInAisle { get; set; }

        /// <summary>
        /// Total capacity of all pick locations
        /// </summary>
        [Display(Name = "Aisle Capacity")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int Capacity { get; set; }

        public IEnumerable<SkuModel> SkuList { get; set; }


        public string Pullers { get; set; }

        public int PercentInAisle { get; set; }

        public int PercentInRestock { get; set; }

        public int PercentToPull { get; set; }
    }
}
/*
    $Id$ 
    $Revision$
    $URL$
    $Header$
    $Author$
    $Date$
*/
