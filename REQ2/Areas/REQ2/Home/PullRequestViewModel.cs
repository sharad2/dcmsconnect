
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DcmsMobile.REQ2.Areas.REQ2.Home
{
 
    public class PullRequestSkuModel
    {
        public PullRequestSkuModel()
        {
            SourceSku = new SkuModel();
            TargetSku = new SkuModel();
        }

        internal PullRequestSkuModel(RequestSku entity)
        {
            this.SourceSku = new SkuModel
            {
                Style = entity.SourceSku.Style,
                Color = entity.SourceSku.Color,
                Dimension = entity.SourceSku.Dimension,
                SkuSize = entity.SourceSku.SkuSize,
                SkuId = entity.SourceSku.SkuId,
               
            };
            if (entity.TargetSku != null)
            {
                this.TargetSku = new SkuModel
                {
                    Style = entity.TargetSku.Style,
                    Color = entity.TargetSku.Color,
                    Dimension = entity.TargetSku.Dimension,
                    SkuSize = entity.TargetSku.SkuSize,
                   // SkuId = entity.TargetSku.SkuId,
                   
                };
            }

            //Pcs requested
            this.RequestedPieces = entity.RequestedPieces;
            this.AssignedPieces = entity.AssignedPieces;
            this.AssignedCartons = entity.AssignedCartons;

        }

        public int RequestedPieces { get; set; }
        public int AssignedPieces { get; set; }
        public int AssignedCartons { get; set; }

        [Required(ErrorMessage = "{0} is required")]
        public SkuModel SourceSku { get; set; }

        [Display(Name = "SKU")]
        public string DisplaySku
        {
            get
            {
                return string.Format(this.SourceSku.Style + "," + this.SourceSku.Color + "," + this.SourceSku.Dimension + "," + this.SourceSku.SkuSize);
            }
        }

        [Required(ErrorMessage = "{0} is required")]
        public SkuModel TargetSku { get; set; }

        [Display(Name = "Target SKU")]
        public string DisplayTargetSku
        {
            get
            {
                return this.TargetSku != null ? string.Format(this.TargetSku.Style + "," + this.TargetSku.Color + "," + this.TargetSku.Dimension + "," + this.TargetSku.SkuSize) : string.Empty;
            }
        }

    }

    public class PullRequestViewModel
    {
        public PullRequestViewModel()
        {

        }

        internal PullRequestViewModel(PullRequest entity)
        {
            this.CtnResvId = entity.CtnResvId;
            this.SourceAreaShortName = entity.SourceAreaShortName;
            this.DestinationAreaShortName = entity.DestinationAreaShortName;
            this.IsConversionRequest = entity.IsConversionRequest;
            this.Priority = entity.Priority;
            this.Remarks = entity.Remarks;
            this.RequestedBy = entity.RequestedBy;
            this.DateCreated = entity.DateCreated;
            this.QuantityRequested = entity.QuantityRequested;
            this.SourceQuality = entity.SourceQuality;
            this.TargetQuality = entity.TargetQuality;
            this.SourceVwhId = entity.SourceVwhId;
            this.TargetVwhId = entity.TargetVwhId;
            this.AssignedPieces = entity.AssignedPieces;
            this.BuildingId = entity.BuildingId;
            this.PriceSeasonCode = entity.PriceSeasonCode;
            this.SewingPlantCode = entity.SewingPlantCode;
        }


        public string CtnResvId { get; set; }

        public string SourceAreaShortName { get; set; }

        public string DestinationAreaShortName { get; set; }

        public bool IsConversionRequest { get; set; }

        public string Priority { get; set; }
    

       // public string PackagingPreferance { get; set; }
        public string Remarks { get; set; }

        public string RequestedBy { get; set; }

        public DateTime? DateCreated { get; set; }

        public int QuantityRequested { get; set; }

        public string SourceVwhId { get; set; }

        public string TargetVwhId { get; set; }

        public string SourceQuality { get; set; }

        public string TargetQuality { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int AssignedPieces { get; set; }

        public string BuildingId { get; set; }

        public string PriceSeasonCode { get; set; }

        public string SewingPlantCode { get; set; }

        public IList<PullRequestSkuModel> SkuList { get; set; }


    }
}

