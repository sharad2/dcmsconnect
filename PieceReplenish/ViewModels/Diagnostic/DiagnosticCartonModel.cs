using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.PieceReplenish.ViewModels.Diagnostic
{
    public class DiagnosticCartonModel
    {
        [Display(Name = "Carton")]
        public string CartonId { get; set; }

        [Display(Name = "Location")]
        public string LocationId { get; set; }
        
        public int Quantity { get; set; }

        public string AreaId { get; set; }

        public string BuildingId { get; set; }

        public string VwhId { get; set; }

        public string QualityCode { get; set; }

        public bool IsCartonInSuspense { get; set; }

        public bool IsCartonDamage { get; set; }

        public bool IsWorkNeeded { get; set; }

        public bool IsBestQalityCarton { get; set; }

        public bool CanPullCarton
        {
            get
            {
                return !IsCartonDamage && !IsCartonInSuspense && !IsWorkNeeded && IsBestQalityCarton;
            }
        }

        public int CumPieces { get; set; }
    }
}