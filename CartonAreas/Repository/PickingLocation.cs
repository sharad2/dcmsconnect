namespace DcmsMobile.CartonAreas.Repository
{
    public class PickingLocation
    {
        public string LocationId { get; set; }

        public int TotalPieces { get; set; }

        public int MaxAssignedPieces { get; set; }

        public Sku AssignedSku { get; set; }

        public string AssignedVwhId { get; set; }

        public int CountTotalLocations { get; set; }
    }
}