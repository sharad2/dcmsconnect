
using DcmsMobile.Inquiry.Helpers;

namespace DcmsMobile.Inquiry.Areas.Inquiry.PickslipEntity
{
    class PickslipSku:SkuBase
    {
        public int? Pieces { get; set; }

        public string VwhId { get; set; }

        public string QualityCode { get; set; }

        public decimal? RetailPrice { get; set; }

        public int? MinPiecesPerBox { get; set; }

        public int? MaxPiecesPerBox { get; set; }

        public int? PiecesPerPackage { get; set; }
    }
}
