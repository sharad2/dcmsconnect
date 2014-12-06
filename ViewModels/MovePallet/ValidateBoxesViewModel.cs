using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.BoxManager.ViewModels.MovePallet
{
    public class ValidateBoxesViewModel : ViewModelBase
    {
        [Display(ShortName = "Last scan")]
        [DisplayFormat(NullDisplayText = "Unknown")]
        public string ScanText { get; set; }

        [Required]
        public string SourcePalletId { get; set; }

        public bool IsConfirmScanText { get; set; }

        [Display(ShortName = "Validate boxes")]
        public int ScanBoxCount { get; set; }
    }
}