using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.BoxPick.Models
{
    /// <summary>
    /// A pallet is considered to be valid if:
    /// 1. It has an ID;
    /// 2. The pattern of the ID is reasonable.
    /// 3. It has a box to pick.
    /// 4. Pickable box count is non zero.
    /// </summary>
    /// <remarks>
    /// This is a self querying model. It queries database after it is bound if <see cref="QueryTime"/> is null.
    /// To requery the pallet set <see cref="QueryTime"/> to null and rebind.
    /// 
    /// The database query is expected to return the pallet whether it is full or not.
    /// </remarks>
    [DisplayName("Pallet")]
    public class Pallet
    {
        [Required(ErrorMessage="Pallet ID is required")]
        public string PalletId
        {
            get;
            set;
        }

        /// <summary>
        /// The time at which this pallet was queried from the database
        /// </summary>
        /// <remarks>
        /// If the pallet does not exist in database, this will be null
        /// </remarks>
        [Required(ErrorMessage="Pallet does not exist in database")]
        public DateTime? QueryTime { get; set; }

        /// <summary>
        /// Number of cartons on this pallet which have already been picked
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int PickedBoxCount { get; set; }

        /// <summary>
        /// Number of cartons on this pallet which have not yet been picked
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:N0}")]
        [Range(1, int.MaxValue, ErrorMessage="Must have at least one box to pick")]
        public int PickableBoxCount { get; set; }

        /// <summary>
        /// Total number of boxes which the pallet will have after all cartons have been successfully picked
        /// </summary>
        public int TotalBoxCount
        {
            get;
            set;
        }

        /// <summary>
        /// If no pickable boxes then we are full.
        /// </summary>
        public bool IsFull
        {
            get
            {
                return TotalBoxCount == PickedBoxCount;
            }
        }

        /// <summary>
        /// The first available box to pick in pull order
        /// </summary>
        /// <remarks>
        /// This box should be ignored in <see cref="PickableBoxCount"/> is 0. This is because the query returns
        /// a picked box when there are no unpicked boxes to left.
        /// </remarks>
        [Required(ErrorMessage="Must have a box to pick")]
        public Box BoxToPick
        {
            get;
            set;
        }

        /// <summary>
        /// This should normally be same as <see cref="PickableBoxCount"/>. If a carton gets stolen, this could be less.
        /// </summary>
        public int ReservedCartonCount { get; set; }

        public string CartonSourceArea { get; set; }

        public string SourceAreaShortName { get; set; }

        public string BuildingId { get; set; }

        /// <summary>
        /// Property type is array to ensure that caching the value does not cause issues
        /// </summary>
        [ScaffoldColumn(true)]
        public CartonLocation[] CartonLocations { get; set; }

        public string DestinationArea { get; set; }

        public string DestAreaShortName { get; set; }

        public string PickModeText { get; set; }

        public int CountRequiredVAS { get; set; }
    }
}



//$Id$