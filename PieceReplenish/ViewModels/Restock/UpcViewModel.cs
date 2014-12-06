using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.PieceReplenish.ViewModels.Restock
{
    public class UpcViewModel : ViewModelBase
    {
        /// <summary>
        /// Carton to restock
        /// </summary>
        public string CartonId { get; set; }

        private string _upcCode;
        /// <summary>
        /// UPC of carton
        /// </summary>
        public string UpcCode
        {
            get { return _upcCode; }
            set { _upcCode = value != null ? value.ToUpper() : value; }
        }

        private string _lastUpcCode;
        /// <summary>
        /// UPC of carton
        /// </summary>
        public string LastUpcCode
        {
            get { return _lastUpcCode; }
            set { _lastUpcCode = value != null ? value.ToUpper() : value; }
        }

        /// <summary>
        /// Virtual warehouse of carton
        /// </summary>
        public string VwhId { get; set; }

        [DisplayFormat(DataFormatString="${0:N2}", NullDisplayText="N/A")]
        public decimal? SkuRetailPrice { get; set; }

        /// <summary>
        /// Pieces in carton
        /// </summary>
        public int Pieces { get; set; }

        /// <summary>
        /// Location suggestions to restock carton.
        /// </summary>
        public IEnumerable<LocationModel> SuggestedLocations { get; set; }

        public string SkuDisplayName { get; set; }
    }
}