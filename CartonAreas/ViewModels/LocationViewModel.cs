using System.ComponentModel.DataAnnotations;
using System;

namespace DcmsMobile.CartonAreas.ViewModels
{
    public class LocationViewModel
    {
        [Required]
        [Display(Name = "Location")]
        public string LocationId { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        [Display(Name = "Total Pallets")]
        public int PalletCount { get; set; }


        [DisplayFormat(DataFormatString = "{0:N0}")]
        [Display(Name = "VWh")]
        public string AssignedVwhId { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        [Display(Name = "Assigned Cartons")]
        public int CartonCount { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        [Display(Name = "Total Pieces")]
        public int? TotalPieces { get; set; }

        [DisplayFormat(DataFormatString = "{0:#,###}")]
        [Display(Name = "Max Cartons")]
        [Range(minimum: 1, maximum: 99999, ErrorMessage = "Cartons must be in between 1 to 99999")]
        public int? MaxAssignedCartons { get; set; }

        public SkuModel AssignedSku { get; set; }

        public SkuModel CartonSku { get; set; }
        public int PercentFull
        {
            get
            {
                if (this.MaxAssignedCartons == null || this.MaxAssignedCartons <= 0)
                {
                    return 0;       // Not full at all
                }
                var pct = (int)((double)this.CartonCount * 100.0 / (double)this.MaxAssignedCartons);
                return Math.Min(pct, 100);
            }
        }

        public int CartonSkuCount { get; set; }
    }
}
//$Id$