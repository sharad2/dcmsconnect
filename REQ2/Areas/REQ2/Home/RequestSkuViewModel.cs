using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.REQ2.Areas.REQ2.Home
{
    public class RequestSkuViewModel
    {
        public RequestSkuViewModel()
        {
            SourceSku = new SkuModel();
            TargetSku = new SkuModel();
        }

        internal RequestSkuViewModel(RequestSku entity)
        {
            this.Pieces = entity.RequestedPieces;            
            this.SourceSku = new SkuModel
            {
                Style = entity.SourceSku.Style,
                Color = entity.SourceSku.Color,
                Dimension = entity.SourceSku.Dimension,
                SkuSize = entity.SourceSku.SkuSize,
                SkuId = entity.SourceSku.SkuId,
                UpcCode = entity.SourceSku.UpcCode
            };
            if (entity.TargetSku != null)
            {
                this.TargetSku = new SkuModel
                {
                    Style = entity.TargetSku.Style,
                    Color = entity.TargetSku.Color,
                    Dimension = entity.TargetSku.Dimension,
                    SkuSize = entity.TargetSku.SkuSize,
                    SkuId = entity.TargetSku.SkuId,
                    UpcCode = entity.TargetSku.UpcCode
                };
            }
        }

        [Display(Name = "Source SKU")]
        [Required(ErrorMessage = "{0} is required")]
        public SkuModel SourceSku { get; set; }

        [Display(Name = "Target SKU")]
        [Required(ErrorMessage = "{0} is required")]
        public SkuModel TargetSku { get; set; }

        [Display(Name = "Pieces")]
        [Range(minimum: 1, maximum: int.MaxValue, ErrorMessage = "{0} must be greater then or equal to 1")]
        public int Pieces { get; set; }
    }
}
//$Id$