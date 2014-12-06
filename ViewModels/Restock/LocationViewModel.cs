using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.PieceReplenish.ViewModels.Restock
{
    public class LocationViewModel : ViewModelBase
    {
        private string _locationId;
        /// <summary>
        /// Location where carton will restock
        /// </summary>
        public string LocationId {
            get { return _locationId; }
            set { _locationId = value != null ? value.ToUpper() : value; }
        }

        /// <summary>
        /// Carton that needs to be restock
        /// </summary>
        public string CartonId { get; set; }

        /// <summary>
        /// Virtual warehouse of carton that needs to be restock
        /// </summary>
        public string VwhId { get; set; }

        /// <summary>
        /// Suggestions of location to restock carton
        /// </summary>
        public IEnumerable<LocationModel> SuggestedLocations { get; set; }

        /// <summary>
        /// Confirmation for location to restock
        /// </summary>
        public string LastLocationId { get; set; }

        /// <summary>
        /// Nmmber of pieces in carton
        /// </summary>
        public int PiecesInCarton { get; set; }

        public string SkuDisplayName { get; set; }

        [DisplayFormat(DataFormatString = "${0:N2}", NullDisplayText = "N/A")]
        public decimal? SkuRetailPrice { get; set; }

    }
}