using DcmsMobile.PieceReplenish.Repository.Restock;

namespace DcmsMobile.PieceReplenish.ViewModels.Restock
{
    public class LocationModel
    {
        public LocationModel()
        {
        }

        internal LocationModel(AssignedLocation entity)
        {
            this.LocationId = entity.LocationId;
            this.IaId = entity.IaId;
            this.PiecesAvailable = entity.PiecesAtLocation;
            this.RestockAisleId = entity.RestockAisleId;
            this.RailCapacity = entity.RailCapacity;
            this.BuildingId = entity.BuildingId;
        }
        /// <summary>
        /// Location where carton can restock
        /// </summary>
        public string LocationId { get; set; }

        /// <summary>
        /// Restock Aisle of location
        /// </summary>
        public string RestockAisleId { get; set; }

        /// <summary>
        /// Pieces available on location
        /// </summary>
        public int PiecesAvailable { get; set; }

        /// <summary>
        /// Area of location
        /// </summary>
        public string IaId { get; set; }

        /// <summary>
        /// Contain Max Capacity of particular Rail.
        /// </summary>
        public int? RailCapacity { get; set; }

        public string BuildingId { get; set; }
    }
}