using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.BoxPick.Models
{
    /// <summary>
    /// A box is valid if:
    /// 1. UCCId exists and has proper pattern.
    /// 2. Must have pieces
    /// 3. Must have SKU, Vwh, Quality
    /// </summary>
    /// <remarks>
    /// This is the box to pick associated with the pallet.
    /// </remarks>
    public class Box
    {
        public const string REGEX_UCC = @"0000\d{16}";

        [Display(Name = "Box")]
        [StringLength(20, MinimumLength = 20, ErrorMessage = "Ucc must be exactly 20 digits.")]
        [RegularExpression(REGEX_UCC, ErrorMessage = "Ucc must be 0000 + 16 digits.")]
        [Required]
        [DataType(DataType.Text)]
        public string UccId { get; set; }


        [DisplayFormat(DataFormatString = "{0:N0}")]
        [Range(1, int.MaxValue, ErrorMessage="The box must not be empty.")]
        public int Pieces { get; set; }

        /// <summary>
        /// Indicates whether Box is picked
        /// </summary>
        public string IaId { get; set; }

        [Required(ErrorMessage = "The box must contain an SKU.")]
        public Sku SkuInBox
        {
            get;
            set;
        }

        /// <summary>
        /// Retrieve VwhId
        /// </summary>
        [Required(ErrorMessage = "The box must have virtual warehouse assigned.")]
        public string VwhId { get; set; }

        /// <summary>
        /// Retrieve QualityCode
        /// </summary>
        [Required(ErrorMessage="The box must have quality code assigned.")]
        public string QualityCode { get; set; }

        /// <summary>
        /// The carton assigned to this box
        /// </summary>
        public Carton AssociatedCarton
        {
            get;
            set;
        }
    }
}



//$Id$