using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DcmsMobile.CartonAreas.ViewModels
{
    public class AddNewBuildingViewModel
    {
        [Display(Name = "Building")]
        [Required]
        public string BuildingId { get; set; }

        [Display(Name = "Address Line 1")]
        [Required]
        public string Address1 { get; set; }

        [Display(Name = "Address Line 2")]
        public string Address2 { get; set; }

        [Display(Name = "Address Line 3")]
        public string Address3 { get; set; }

        [Display(Name = "Address Line 4")]
        public string Address4 { get; set; }

        [Display(Name = "City")]
        [Required]
        public string City { get; set; }

        [Display(Name = "State")]        
        public string State { get; set; }

        [Display(Name = "Zip Code")]
        [Required]
        public string ZipCode { get; set; }

        [Display(Name = "Pallet Limit")]
        public int? ReceivingPalletLimit { get; set; }

        [Display(Name = "Country Code")]
        public string CountryCode { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }

    }
}