
using EclipseLibrary.Mvc.Html;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.Receiving.ViewModels.Rad
{
    public class SpotCheckViewModel
    {
        [DisplayName("Style")]
        [Display(ShortName = "Style")]
        public string Style { get; set; }

        [DisplayName("Sewing Plant")]
        [Display(ShortName = "Sewing Plant")]
        public string SewingPlantId { get; set; }

        [DisplayName("Plant Name")]
        [Display(ShortName = "Plant Name")]
        public string PlantName { get; set; }

        [Required(ErrorMessage = "Spot Check Percentage is required")]
        [DisplayName("Spot Check %")]
        [Display(ShortName = "Spot Check %")]
        [Range(0, 100, ErrorMessage = "Percentage must be upto 100")]
        [DisplayFormat(DataFormatString = "{0:N0}%")]
        public int? SpotCheckPercent { get; set; }
        public IEnumerable<GroupSelectListItem> SewingPlantList { get; set; }

        [DisplayName("Color")]
        [Display(ShortName = "Color")]
        public string Color { get; set; }

        public bool? AllStyles { get; set; }

        public bool? AllColors { get; set; }

        [DisplayName("SpotCheck Enabled")]
        [Display(ShortName = "SpotCheck Enabled")]
        public bool IsSpotCheckEnabled { get; set; }

        /// <summary>
        /// Spot check area
        /// </summary>
        public string AreaId { get; set; }

        /// <summary>
        /// Building of Spot check area
        /// </summary>
        public string BuildingId { get; set; }

   
        //Use this property to highlight newly added configuration
        public string ConfigurationKey         
        {
            get
            {
                return (string.IsNullOrEmpty(this.SewingPlantId) ?"All" : this.SewingPlantId) + (string.IsNullOrEmpty(this.Style) ? "All" : this.Style)  +  (string.IsNullOrEmpty(this.Color) ? "All" :this.Color);
            }        
        }

        /// <summary>
        /// Date on which SpotCheck configuration created
        /// </summary>
        internal DateTimeOffset? CreatedDate { get; set; }

        /// <summary>
        /// User who created configuration
        /// </summary>
        public string CreatedBy { get; set; }

        public string CreationDateDispaly
        {
             get
             {
                 if (CreatedDate.HasValue)
                 {
                     return string.Format("{0:ddd d MMM}", CreatedDate);
                 }
                 return string.Empty;
             }
            
        }

        /// <summary>
        /// Date when configuration is modified
        /// </summary>
        internal DateTimeOffset? ModifiedDate { get; set; }


        /// <summary>
        /// user who modified configuration
        /// </summary>
        public string ModifiedBy { get; set; }


        public string ModifyDateDispaly
        {
            get
            {
                if (ModifiedDate.HasValue)
                {
                    return string.Format("{0:ddd d MMM}", ModifiedDate);
                }
                return string.Empty;
            }

        }


    }
}



//$Id$