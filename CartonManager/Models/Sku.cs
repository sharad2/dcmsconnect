
namespace DcmsMobile.CartonManager.Models
{
    public class Sku
    {
        public int SkuId { get; set; }

        public string Style { get; set; }

        public string Color { get; set; }

        public string Dimension { get; set; }

        public string SkuSize { get; set; }

        public string UpcCode { get; set; }

        public override string ToString()
        {
            return string.Format("{0},{1},{2},{3}", this.Style, this.Color, this.Dimension, this.SkuSize);
        }
    }
}




//$Id$