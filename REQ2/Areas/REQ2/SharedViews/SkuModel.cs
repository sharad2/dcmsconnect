
namespace DcmsMobile.REQ2.Areas.REQ2.SharedViews
{
    public class SkuModel
    {
        public int SkuId { get; set; }

        public string Style { get; set; }

        public string Color { get; set; }

        public string Dimension { get; set; }

        public string SkuSize { get; set; }

        public string UpcCode { get; set; }

        /// <summary>
        /// Readable representation of the SKU. <see cref="ReqService.AddSkutoRequest"/> takes advantage of this.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{0},{1},{2},{3}", Style, Color, Dimension, SkuSize);
        }
    }
}

//$Id$