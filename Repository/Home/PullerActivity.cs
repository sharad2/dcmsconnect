using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.PieceReplenish.Repository.Home
{
    /// <summary>
    /// Supplies information about what pull activity the puler is involved in
    /// </summary>
    public class PullerActivity
    {
        [Key]
        public string PullerName { get; set; }

        [Key]
        public string PalletId { get; set; }
        
        /// <summary>
        /// Restock aisle being pulled
        /// </summary>
        [Key]
        public string RestockAisleId { get; set; }

        public DateTime? MinAssignDate { get; set; }


        /// <summary>
        /// SKUs being pulled by this puller
        /// </summary>
        public IEnumerable<int> ListSkuId { get; set; }

        /// <summary>
        /// For/In which building puller is pulling
        /// </summary>
        public string BuildingId { get; set; }

        /// <summary>
        /// value will be set true when puller will use the Piece Replenishment module else set to false.
        /// </summary>
        /// <remarks>
        /// Helps to track the puller who are using the old Pull module
        /// </remarks>
        public bool IsUsingReplenishmentModule { get; set; }

        public string Styles { get; set; }
        
        public int? CartonCount { get; set; }
    }
}