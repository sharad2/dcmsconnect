using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.PickWaves.Areas.PickWaves.ManageWaves
{
    public class Sku
    {
        [Key]
        public int SkuId { get; set; }

        public string Style { get; set; }

        public string Color { get; set; }

        public string Dimension { get; set; }

        public string SkuSize { get; set; }

        public string UpcCode { get; set; }

        public string VwhId { get; set; }

        public decimal WeightPerDozen { get; set; }

        public decimal VolumePerDozen { get; set; }

        public bool IsAssignedSku { get; set; }
    }
}