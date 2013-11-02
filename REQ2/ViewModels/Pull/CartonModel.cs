namespace DcmsMobile.REQ2.ViewModels.Pull
{
    public class CartonModel
    {
        public string CartonId { get; set; }

        public string LocationId { get; set; }

        public SkuModel SkuInCarton { get; set; }

        public string SkuDisplay
        {
            get
            {
                return string.Format("{0} {1} {2} {3}", this.SkuInCarton.Style, this.SkuInCarton.Color, this.SkuInCarton.Dimension, this.SkuInCarton.SkuSize);
            }
        }
    }
}