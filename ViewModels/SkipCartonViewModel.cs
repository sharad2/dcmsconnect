using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.PieceReplenish.ViewModels
{
    public class SkipCartonViewModel : ViewModelBase
    {

        public string Choice { get; set; }

        private string _cartonId;
        /// <summary>
        /// Scanned carton ID
        /// </summary>
        [Display(Name = "Carton")]
        public string CartonId
        {
            get
            {
                return _cartonId;
            }
            set
            {
                _cartonId = value != null ? value.ToUpper() : value;
            }
        }

        [Required]
        public ContextModel Context { get; set; }

        /// <summary>
        /// on whhich pallet cartons will be put
        /// </summary>
        [Required]
        [Display(Name = "Pallet")]
        public string PalletId { get; set; }

        public string SourceLocationId { get; set; }

        [Display(Name = "Cartons on Pallet")]
        public int CountCartonsOnPallet { get; set; }
    }
}