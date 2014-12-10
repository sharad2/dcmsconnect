using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.PickWaves.Areas.PickWaves.Config
{
    public class ConstraintModel
    {
        public ConstraintModel()
        {

        }

        /// <summary>
        /// Generate constraints based on the passed list of SPLH
        /// </summary>
        /// <param name="entity"></param>
        internal ConstraintModel(ConfigService.Constraint entity)
        {
            this.MaxBoxWeight = entity.MaxBoxWeight;
            this.MaxSkuWithinBox = entity.MaxSkuWithinBox;
            this.IsSingleStyleColor = entity.IsSingleStyleColor;
            this.RequiredMinSkuPieces = entity.RequiredMinSkuPieces;
            this.RequiredMaxSkuPieces = entity.RequiredMaxSkuPieces;
        }

        /// <summary>
        /// The maximum permissible weight of the box after SKUs have been added to it.
        /// </summary>
        [DisplayFormat(NullDisplayText = "No Limit")]
        [Display(Name = "Maximum Weight")]
        public int? MaxBoxWeight { get; set; }

        [Display(Name = "Single Style Color")]
        public bool IsSingleStyleColor { get; set; }

        [DisplayFormat(NullDisplayText = "No Limit")]
        [Display(Name = "Maximum SKUs")]
        public int? MaxSkuWithinBox { get; set; }
        /// <summary>
        /// Required Min pieces in box of a single SKU
        /// </summary>
        [DisplayFormat(NullDisplayText = "No Limit")]
        [Display(Name = "Minimum Pieces/SKU")]
        [Range(minimum: 1, maximum: 99, ErrorMessage = "Number of Pieces must be in between 1 to 99")]
        public int? RequiredMinSkuPieces { get; set; }

        /// <summary>
        /// Required Max pieces in box of a single SKU
        /// </summary>
        [DisplayFormat(NullDisplayText = "No Limit")]
        [Display(Name = "Maximum Pieces/SKU")]
        [Range(minimum: 1, maximum: 99, ErrorMessage = "Number of Pieces must be in between 1 to 99")]
        public int? RequiredMaxSkuPieces { get; set; }

        /// <summary>
        /// Returns true if at least one constraint is set
        /// </summary>
        public bool HasConstraint
        {
            get
            {
                return MaxBoxWeight.HasValue || MaxSkuWithinBox.HasValue || RequiredMinSkuPieces.HasValue ||RequiredMaxSkuPieces.HasValue || IsSingleStyleColor;
            }
        }
    }

    /// <summary>
    /// Used for editing the constraints of a particular customer
    /// </summary>
    public class CustomerConstraintEditorModel : ConstraintModel
    {
        public CustomerConstraintEditorModel()
        {

        }
        internal CustomerConstraintEditorModel(ConfigService.Constraint entity)
            : base(entity)
        {
            OrigRequiredMinSkuPieces = this.RequiredMinSkuPieces;
            OrigRequiredMaxSkuPieces = this.RequiredMaxSkuPieces;
            OrigMaxBoxWeight = this.MaxBoxWeight;
            OrigMaxSkuWithinBox = this.MaxSkuWithinBox;
            OrigIsSingleStyleColor = this.IsSingleStyleColor;
        }
      
        [Required (ErrorMessage="Customer is required")]
        public string CustomerId { get; set; }

        public string CustomerName { get; set; }

        public int? OrigRequiredMinSkuPieces { get; set; }

        public int? OrigRequiredMaxSkuPieces { get; set; }

        public decimal? OrigMaxBoxWeight { get; set; }

        public int? OrigMaxSkuWithinBox { get; set; }

        public bool OrigIsSingleStyleColor { get; set; }
    }
}