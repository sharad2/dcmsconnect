
namespace DcmsMobile.PalletLocating.Models
{
    public class Carton
    {
        public Carton()
        {
            
        }

        public string CartonId { get; set; }

        public Sku Sku { get; set; }
        
        public string VwhId { get; set; }

        public Area Area { get; set; }

        public string PalletId { get; set; }

        public string QualityCode { get; set; }

        public int Pieces { get; set; }
    }
}