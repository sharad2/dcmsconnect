using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.PieceReplenish.ViewModels
{
    public class PullerActivityModel
    {
        [Key]
        public string PullerName { get; set; }
        
        [Key]
        public string RestockAisleId { get; set; }

        /// <summary>
        /// Pallet ID for which puller have assigned some cartons to pull
        /// </summary>
        [Key]
        public string PalletId { get; set; }

        public DateTime? MinAssignDate { get; set; }

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

        /// <summary>
        /// Comma seprated list of Styles, which are being pulled by Puller
        /// </summary>
        public string Styles { get; set; }
        
        /// <summary>
        /// How many cartons are yet assigned to puller per pallet
        /// </summary>
        public int? CartonCount { get; set; }
    }
}
