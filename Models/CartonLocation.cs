using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.BoxPick.Models
{
    public class CartonLocation
    {
        [Required]
        [Display(Name="Location")]
        public string CartonLocationId { get; set; }

        [Required]
        public string CartonStorageArea { get; set; }

        [Required]
        public Sku SkuToPick { get; set; }

        public int CountCartonsToPick { get; set; }

        public int PiecesPerCarton { get; set; }

    }
}



//$Id$