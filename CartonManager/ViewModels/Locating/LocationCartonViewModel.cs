using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.CartonManager.ViewModels.Locating
{
    public class LocationCartonViewModel :SoundModel
    {

        public string ScanText { get; set; }

        private string _currentLocationId;

        /// <summary>
        /// Currently active location
        /// </summary>
        [Display(Name = "Location")]
        public string CurrentLocationId
        {
            get
            {
                return _currentLocationId ?? string.Empty;
            }
            set
            {
                _currentLocationId = (value ?? string.Empty).ToUpper();
            }
        }

        /// <summary>
        /// Cartons of which Pallet will be located
        /// </summary>
        public string PalletId { get; set; }

        /// <summary>
        /// Number of cartons on the <see cref="PalletId"/>
        /// </summary>
        public int CartonsOnPallet { get; set; }

        public string DestAreaId { get; set; }

        public string AreaShortName { get; set; }

        public string DestBuildingId { get; set; }

        public int? CountCartonsAtLocation { get; set; }

        public int? LocationTravelSequence { get; set; }

        public string LastCartonId { get; set; }

        public int? MaxCartonsAtLocation { get; set; }
    }
}