namespace DcmsMobile.Shipping.Repository.ScanToTruck
{
    public class Appointment
    {
        public int? UnloadedPalletCount { get; set; }

        public int? LoadedPalletCount { get; set; }

        public int? BoxesNotOnPalletCount { get; set; }

        public string AppointmentBuildingId { get; set; }

        public int? LoadedBoxCount { get; set; }

        public int? TotalBoxCount { get; set; }

        public int? PalletsInSuspenseCount { get; set; }

        public string CarrierId { get; set; }

        public string DoorId { get; set; }

        public int TotalPalletCount { get; set; }

        public int UnPalletizeBoxCount { get; set; }

        public string BuildingId { get; set; }
    }
}