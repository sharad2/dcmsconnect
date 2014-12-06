
namespace DcmsMobile.CartonManager.Models
{
    public class Pallet
    {
        public string PalletId { get; set; }

        public int CartonCount { get; set; }

        public string MinShortName { get; set; }

        public int CartonNeededSomeWork { get; set; }

        /// <summary>
        /// If this is > 1, the UI should display warning
        /// </summary>
        public int CartonAreaCount { get; set; }

        public string MaxShortName { get; set; }

        public string BuildingId { get; set; }
        
    }
}