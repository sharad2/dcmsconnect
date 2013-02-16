using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.DcmsLite.ViewModels.Validation
{
    public class IndexViewModel : ViewModelBase
    {

        public const string REGEX_UCC = @"0000\d{16}";

        [Display(Name="Scan Box")]
        [StringLength(20, MinimumLength = 20, ErrorMessage = "UCC must be exactly 20 digits.")]
        [RegularExpression(REGEX_UCC, ErrorMessage = "UCC must be 0000 + 16 digits.")]
        [Required(ErrorMessage = "Scan UCC to verify the Box.")]
        public string UccId { get; set; }

        public string LastScan { get; set; }

        public string PostVerifyArea { get; set; }

        public string BadVerifyArea { get; set; }

        /// <summary>
        /// Sound file to play on page load.
        /// </summary>
        public char Sound { get; set; }
    }
}