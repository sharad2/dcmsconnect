using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.Inquiry.Areas.Inquiry.SkuEntity
{
    public class StyleViewModel
    {
        [Display(Name = "Style")]
        public string StyleId { get; set; }

        [Display(Name = "Description")]
        public String Description { get; set; }

        [Display(Name = "Label")]
        public string LabelId { get; set; }

        [Display(Name = "Country of Origin")]
        public IEnumerable<CountryOfOriginModel> CountryOfOrigins { get; set; }
    }
}