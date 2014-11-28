using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.Inquiry.Areas.Inquiry.CartonAreaEntity
{
    public class CartonAreaModel
    {
        public string Area { get; set; }

        public string AreaShortName { get; set; }

        public string Description { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}", NullDisplayText = "No Loc")]
        public int TotalLocations { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}", NullDisplayText = "No Loc")]
        public int UsedLocations { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? CartonCount { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? DistinctSKUs { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? Quantity { get; set; }

        [DisplayFormat(NullDisplayText = "Unknown Building")]
        public string BuildingId { get; set; }
                
        public string BuildingDescription { get; set; }

        public int PercentUsed
        {
            get
            {
                if (this.TotalLocations == 0)
                {
                    return 0;
                }
                return (int)Math.Round(this.UsedLocations * 100.0 / this.TotalLocations);
            }
        }

    }

    public class CartonAreaListViewModel
    {

        public IList<CartonAreaModel> AllAreas { get; set; }

        
    }
}