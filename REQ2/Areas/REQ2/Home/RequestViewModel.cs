using System;
using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.REQ2.Areas.REQ2.Home
{

    public class RequestCartonRulesViewModel
    {
        [Display(Name = "Quality")]
        public string QualityCode { get; set; }


        [Display(Name = "Sewing Plant")]
        public string SewingPlantCode { get; set; }


        [Display(Name = "Price Season")]
        public string PriceSeasonCode { get; set; }

        [Display(Name = "Received on")]
        [DisplayFormat(DataFormatString = "{0:d}", ApplyFormatInEditMode = true)]
        public DateTime? CartonReceivedDate { get; set; }

        [Display(Name = "Building")]
        public string BuildingId { get; set; }
    }



}
//$Id$