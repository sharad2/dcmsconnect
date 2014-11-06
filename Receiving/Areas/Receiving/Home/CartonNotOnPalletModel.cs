using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.Receiving.Areas.Receiving.Home
{
    //TODO: Think of a better name.
    public class CartonNotOnPalletModel
    {
        [Display(Name = "Carton")]
        public string CartonId { get; set; }

        [Display(Name = "Vwh")]
        public string VWHId { get; set; }

        [Display(Name = "Area")]
        public string AreaId { get; set; }
    }
}