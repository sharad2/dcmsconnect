
namespace DcmsMobile.REQ2.Models
{
    public class RequestSku
    {

        public RequestSku()
        {
            this.SourceSku = new Sku();
            this.TargetSku = new Sku();
        }
        public Sku SourceSku { get; set; }
        public Sku TargetSku { get; set; }
        public int Pieces { get; set; }

        public int? PulledCartons { get; set; }

        public int? TotalCartons { get; set; }

        public int? AssignedPieces { get; set; }
    }
}

//$Id$