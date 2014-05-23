using System.ComponentModel.DataAnnotations;
namespace DcmsMobile.CartonAreas.ViewModels
{
    public class EditAddressOfBuildingViewModel
    {

        public string BuildingId { get; set; }

        [Required(ErrorMessage = "Address Line 1 is required")]
        [Display(Name = "Address Line 1")]
        public string Address1 { get; set; }

        [Display(Name = "Address Line 2")]
        public string Address2 { get; set; }

        [Display(Name = "Address Line 3")]
        public string Address3 { get; set; }

        [Display(Name = "Address Line 4")]
        public string Address4 { get; set; }

        [Required(ErrorMessage = "City is required")]
        [Display(Name = "City")]
        public string City { get; set; }

        [Display(Name = "State")]
        public string State { get; set; }

        [Required(ErrorMessage = "Zip Code is required")]
        [Display(Name = "Zip Code")]
        public string ZipCode { get; set; }

        [Display(Name = "Country Code")]
        public string CountryCode { get; set; }
    }
}
