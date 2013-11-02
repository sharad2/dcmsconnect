using System.ComponentModel.DataAnnotations;
using DcmsMobile.REQ2.Models;
using System;

namespace DcmsMobile.REQ2.ViewModels
{
    public class RequestSkuModel
    {
        public RequestSkuModel()
        {
            SourceSku = new SkuModel();
            TargetSku = new SkuModel();
        }

        public RequestSkuModel(RequestSku entity)
        {
            this.Pieces = entity.Pieces;
            this.PulledCartons = entity.PulledCartons;
            this.AssignedPieces = entity.AssignedPieces;
            this.TotalCartons = entity.TotalCartons == 0 ? (int?)null : entity.TotalCartons;
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
            else
            {
                this.TargetSku = new SkuModel();
            }
        }

        [Display(Name = "Source SKU")]
        [Required(ErrorMessage = "{0} is required")]
        public SkuModel SourceSku { get; set; }

        private string MakeEditable(string str)
        {
            var tokens = str.Split(new[] { "&rarr;", " " }, StringSplitOptions.RemoveEmptyEntries);
            return string.Join("-", tokens);
        }
        /// <summary>
        /// Returns source and target style seperated by hyphen. Target not displayed if it is same as source
        /// </summary>
        [DisplayFormat(HtmlEncode = false)]
        public string StyleDisplay
        {
            get
            {
                if (string.IsNullOrWhiteSpace(TargetSku.Style) || (SourceSku.Style ?? "") == (TargetSku.Style ?? ""))
                {
                    return SourceSku.Style ?? "";
                }
                return string.Format("{0} &rarr; {1}", SourceSku.Style, TargetSku.Style);

            }
        }

        public string StyleEditable
        {
            get
            {
                return MakeEditable(this.StyleDisplay);
            }
        }

        [DisplayFormat(HtmlEncode = false)]
        public string ColorDisplay
        {
            get
            {
                if (string.IsNullOrWhiteSpace(TargetSku.Color) || (SourceSku.Color ?? "") == (TargetSku.Color ?? ""))
                {
                    return SourceSku.Color ?? "";
                }
                else
                {
                    return string.Format("{0} &rarr; {1}", SourceSku.Color, TargetSku.Color);
                }
            }
        }

        public string ColorEditable
        {
            get
            {
                return MakeEditable(this.ColorDisplay);
            }
        }

        [DisplayFormat(HtmlEncode = false)]
        public string DimensionDisplay
        {
            get
            {
                if (string.IsNullOrWhiteSpace(TargetSku.Dimension) || (SourceSku.Dimension ?? "") == (TargetSku.Dimension ?? ""))
                {
                    return SourceSku.Dimension ?? "";
                }
                else
                {
                    return string.Format("{0} &rarr; {1}", SourceSku.Dimension, TargetSku.Dimension);
                }
            }
        }

        public string DimensionEditable
        {
            get
            {
                return MakeEditable(this.DimensionDisplay);
            }
        }

        [DisplayFormat(HtmlEncode = false)]
        public string SkuSizeDisplay
        {
            get
            {
                if (string.IsNullOrWhiteSpace(TargetSku.SkuSize) || (SourceSku.SkuSize ?? "") == (TargetSku.SkuSize ?? ""))
                {
                    return SourceSku.SkuSize ?? "";
                }
                else
                {
                    return string.Format("{0} &rarr; {1}", SourceSku.SkuSize, TargetSku.SkuSize);
                }
            }
        }

        public string SkuSizeEditable
        {
            get
            {
                return MakeEditable(this.SkuSizeDisplay);
            }
        }

        [Display(Name = "Target SKU")]
        [Required(ErrorMessage = "{0} is required")]
        public SkuModel TargetSku { get; set; }

        [Display(Name = "Pieces")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        [Range(minimum: 1, maximum: int.MaxValue, ErrorMessage = "{0} must be greater then or equal to 1")]
        public int Pieces { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? PulledCartons { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? UnPulledCartons
        {
            get
            {
                return this.TotalCartons - this.PulledCartons;
            }
        }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? TotalCartons { get; set; }


        private int? _assignedPieces;
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? AssignedPieces
        {
            get
            {
                return _assignedPieces == 0 ? null : _assignedPieces;
            }
            set
            {
                _assignedPieces = value;
            }
        }
    }
}
//$Id$