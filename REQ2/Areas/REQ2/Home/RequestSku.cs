
using DcmsMobile.REQ2.Areas.REQ2.SharedViews;
namespace DcmsMobile.REQ2.Areas.REQ2.Home
{
    internal class RequestSku
    {

        public RequestSku()
        {
            this.SourceSku = new Sku();
            this.TargetSku = new Sku();
        }
        public Sku SourceSku { get; set; }
        public Sku TargetSku { get; set; }
        public int Pieces { get; set; }
    }
}

//$Id$