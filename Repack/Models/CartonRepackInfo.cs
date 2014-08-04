namespace DcmsMobile.Repack.Models
{
    /// <summary>
    /// Contains information needed to create a carton
    /// </summary>
    public class CartonRepackInfo
    {
        public string CartonId { get; set; }

        public string SourceSkuArea { get; set; }

        public string DestinationCartonArea { get; set; }

        public string PrinterName { get; set; }

        public string PalletId { get; set; }

        public string PriceSeasonCode { get; set; }

        public string VwhId { get; set; }

        public string QualityCode { get; set; }

        public int? SkuId { get; set; }

        public string UpcCode { get; set; }

        public string SewingPlantCode { get; set; }

        public int? Pieces { get; set; }

        public int NumberOfCartons { get; set; }

        public int TartgetSkuId { get; set; }

        public string TargetVWhId { get; set; }

        public string TargetQualityCode { get; set; }

        public string ShipmentId { get; set; }
    }

}





//$Id$