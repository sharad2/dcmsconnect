using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.BoxPick.Models
{
    /// <summary>
    /// Represents the properties of a carton
    /// </summary>
    public class Carton
    {
        [Display(Name="Carton")]
        public string CartonId { get; set; }

        public Sku SkuInCarton
        {
            get;
            set;
        }

        public int Pieces { get; set; }

        /// <summary>
        /// TODO: Verify pattern for location id: [|F|FFDC] followed by 6 digits
        /// </summary>
        //[RegularExpression(@"F{0,1}\d{6}|FFDC\d{6}")]
        public string LocationId { get; set; }

        public string VwhId { get; set; }

        public string QualityCode { get; set; }

        public string StorageArea { get; set; }


        /// <summary>
        /// Pallet on which carton exists.
        /// </summary>
        public string AssociatedPalletId { get; set; }
    }
}





//$Id$