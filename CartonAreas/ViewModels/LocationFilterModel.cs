using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System;

namespace DcmsMobile.CartonAreas.ViewModels
{
    [Obsolete]
    public class LocationFilterModel
    {
        [Required]
        public string AreaId { get; set; }

        [Display(Name = "Assigned SKU is")]
        public int? SkuId { get; set; }

        [Display(Name = "Location contents")]
        public bool? EmptyLocations { get; set; }

        [Display(Name = "SKU assignment")]
        public bool? AssignedLocationChoice { get; set; }

        //[Display(Name = "Assigned max cartons more than equal to")]
        //[Range(minimum: 1, maximum: 99999, ErrorMessage = "Assigned max cartons must be in between 1 to 99999")]
        //public int? AssignedMaxCarton { get; set; }

        /// <summary>
        /// The text entered by the user for SKU
        /// </summary>
        public string SkuEntry { get; set; }

        [Display(Name = "Location ID is")]
        public string LocationId { get; set; }

        //public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        //{
        //    if (this.AssignedLocationChoice == "I" && this.SkuId == null) {
        //        yield return new ValidationResult("Specify the SKU");
        //    }
        //}
    }
}
//$Id$
