using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace DcmsMobile.Inquiry.Areas.Inquiry.SkuAreaEntity
{
   public class SKUAreaListModel
    {
        public string IaId { get; set; }

        public string ShortName { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        [Display(Name = "# Locations")]
        public int? NumberOfLocations { get; set; }
    }
}
