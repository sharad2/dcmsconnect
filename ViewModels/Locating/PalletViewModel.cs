
using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.CartonManager.ViewModels.Locating
{
    public class PalletViewModel : SoundModel
    {
        private string _palletId;

        [Display(Name = "Pallet")]
        [RegularExpression(@"^([P|p]\S{1,7})", ErrorMessage = "Pallet must be start with P")]
        public string PalletId
        {
            get
            {
                return _palletId ?? string.Empty;
            }
            set
            {
                _palletId = (value ?? string.Empty).ToUpper();
            }
        }
    }
}