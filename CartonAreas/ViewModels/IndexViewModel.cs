using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.CartonAreas.ViewModels
{
    public class IndexViewModel
    {
        public IList<BuildingModel> Buildings { get; set; }

        [Display(Name = "Building")]
        [Required]
        public string BuildingId { get; set; }

        [Display(Name = "Address1")]
        [Required]
        public string Address1 { get; set; }

        [Display(Name = "Address2")]
        public string Address2 { get; set; }

        [Display(Name = "Address3")]
        public string Address3 { get; set; }

        [Display(Name = "Address4")]
        public string Address4 { get; set; }

        [Display(Name = "City")]
        [Required]
        public string City { get; set; }

        [Display(Name = "State")]
        [Required]
        public string State { get; set; }

        [Display(Name = "ZipCode")]
        [Required]
        public string ZipCode { get; set; }

        [Display(Name = "Pallet Limit")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? ReceivingPalletLimit { get; set; }

        [Display(Name = "Ricther Warehouse Id")]
        [Required]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public string RictherWarehouseId { get; set; }

    }
}