using System;
using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.Inquiry.Areas.Inquiry.SkuAreaEntity
{
    
    public class SkuAreaViewModel
    {

        public string ShortName { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }

        [Display(Name = "Default Location")]
        public string DefaultLocation { get; set; }

        [Display(Name = "Building")]
        public string WhId { get; set; }

        public string PickingAreaFlag { get; set; }

        public string ShipingAreaFlag { get; set; }


        [Display(Name = "Pull Carton Limit")]
        public int? PullCartonLimit { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        [Display(Name = "# Locations")]
        public int NumberOfLocations { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        [Display(Name = "# Assigned Locations")]
        public int AssignedLocations { get; set; }

        public int PercentFull
        {
            get
            {
                if (this.NumberOfLocations == 0)
                {
                    return 0;
                }
                return (int)Math.Round(this.AssignedLocations * 100.0 / this.NumberOfLocations);
            }
        }


        //[Display(Name = "Shiping Area")]
        //[Obsolete]
        //public string ShipingArea
        //{
        //    get
        //    {
        //        return (!string.IsNullOrEmpty(this.ShipingAreaFlag)) ? "Yes" : "No";
        //    }
        //}

        //[Display(Name = "Picking Area")]
        //[Obsolete]
        //public string PickingArea
        //{
        //    get
        //    {
        //        return (!string.IsNullOrEmpty(this.PickingAreaFlag)) ? "Yes" : "No";
        //    }
        //}      
       
    }
}




//$Id$