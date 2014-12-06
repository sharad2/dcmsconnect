using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.BoxManager.ViewModels.MovePallet
{
    /// <summary>
    /// The view prompts the user for entering the pallet which he wishes to move
    /// </summary>
    public class IndexViewModel : ViewModelBase
    {
        public string PalletId { get; set; }
        
        [Required (ErrorMessage = "Pallet is required.")]
        public string ScanText { get; set; }
    }
}