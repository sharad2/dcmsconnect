using System.ComponentModel.DataAnnotations;


namespace DcmsMobile.BoxPick.ViewModels.MainContent
{
    public class BoxModel
    {
        [Display(Name = "UCC ID")]
        public string UccId { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        [Display(Name = "Pieces")]
        public int Pieces { get; set; }

        [Display(Name = "Status")]
        public string IaId { get; set; }

        [Display(Name = "VWh ID")]
        public string VwhId { get; set; }

        [Display(Name = "Quality")]
        public string QualityCode { get; set; }

        [Display(Name = "Carton")]
        public string AssociatedCartonId { get; set; }

        public string SkuInCarton { get; set; }

        [Display(Name = "Location")]
        public string CartonLocationId { get; set; }

        public string SkuInBox { get; set; }
    }
}

/*
    $Id$ 
    $Revision$
    $URL$
    $Header$
    $Author$
    $Date$
*/