using System;
using System.Collections.Generic;

namespace DcmsMobile.Inquiry.Areas.Inquiry.SkuEntity
{
    internal class CountryOfOrigin
    {
        public string CountryId { get; set; }

        public int CountColors { get; set; }

        public string CountryName { get; set; }
    }

    internal class Style
    {
        public string StyleId { get; set; }

        public String Description { get; set; }

        public string LabelId { get; set; }

        public IEnumerable<CountryOfOrigin> CountryOfOrigins { get; set; }
    }
}





//$Id$