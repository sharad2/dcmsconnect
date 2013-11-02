using DcmsMobile.REQ2.Models;

namespace DcmsMobile.REQ2.Repository.Pull
{
    public class PullableCarton
    {
        public string CartonId { get; set; }

        public string LocationId { get; set; }

        public Sku SkuInCarton { get; set; }
        
    }
}
