
namespace DcmsMobile.REQ2.Models
{
    public class AssignedCarton
    {
        public SkuModel Sku { get; set; }

        public int TotalCartons { get; set; }

        public int PulledCartons { get; set; }

        public int TotalPieces { get; set; }

        public int PulledPieces { get; set; }
    }
}

//$Id$