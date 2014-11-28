using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DcmsMobile.Inquiry.Areas.Inquiry.CartonAreaEntity
{
    public class CartonAreaHeadline
    {
        public string Area { get; set; }

        public string AreaShortName { get; set; }

        public string Description { get; set; }

        public int TotalLocations { get; set; }

        public int UsedLocations { get; set; }

        public int? CartonCount { get; set; }

        public int? DistinctSKUs { get; set; }

        public int? Quantity { get; set; }

        public string Building { get; set; }

        public string BuildingDescription { get; set; }
    }
}