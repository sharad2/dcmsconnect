
using DcmsMobile.REQ2.Areas.REQ2.SharedViews;
namespace DcmsMobile.REQ2.Areas.REQ2.Home
{
    public class RequestSkuModel
    {

        public RequestSkuModel()
        {
            this.SourceSku = new SkuModel();
            this.TargetSku = new SkuModel();
        }
        public SkuModel SourceSku { get; set; }
        public SkuModel TargetSku { get; set; }
        public int Pieces { get; set; }
    }
}

//$Id$