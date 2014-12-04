namespace DcmsMobile.Repack.Models
{
    /// <summary>
    /// This class is returned to the remote validator via the Action ValidateSku()
    /// </summary>
    public class Sku
    {
        public int SkuId { get; set; }

        public string Style { get; set; }

        public string Color { get; set; }

        public string Dimension { get; set; }

        public string SkuSize { get; set; }

        public string UpcCode { get; set; }

        public int? StandardSkuSize { get; set; }

    }
}





//$Id$