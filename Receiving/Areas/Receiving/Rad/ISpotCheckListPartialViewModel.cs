
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.Receiving.Areas.Receiving.Rad
{


    public class SpotCheckConfigurationModel
    {
        public SpotCheckConfigurationModel()
        {

        }
        public SpotCheckConfigurationModel(SpotCheckConfiguration src)
        {
            Style = src.Style;
            SewingPlantId = src.SewingPlantId;
            PlantName = src.PlantName;
            SpotCheckPercent = src.SpotCheckPercent;
            Color = src.Color;
            IsSpotCheckEnabled = src.IsSpotCheckEnable.Value;
            CreatedDate = src.CreatedDate;
            CreatedBy = src.CreatedBy;
            ModifiedDate = src.ModifiedDate;
            ModifiedBy = src.ModifiedBy;
        }


        [DisplayName("Style")]
        [Display(ShortName = "Style")]
        [DisplayFormat(NullDisplayText = "All")]
        public string Style { get; set; }

        [DisplayName("Sewing Plant")]
        [Display(ShortName = "Sewing Plant")]
        [DisplayFormat(NullDisplayText="All")]
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

        [DisplayName("Color")]
        [Display(ShortName = "Color")]
        [DisplayFormat(NullDisplayText = "All")]
        public string Color { get; set; }

        [DisplayName("SpotCheck Enabled")]
        [Display(ShortName = "SpotCheck Enabled")]
        public bool IsSpotCheckEnabled { get; set; }

        /// <summary>
        /// Date on which SpotCheck configuration created
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:ddd d MMM}")]
        public DateTimeOffset? CreatedDate { get; set; }

        /// <summary>
        /// User who created configuration
        /// </summary>
        public string CreatedBy { get; set; }

        /// <summary>
        /// Date when configuration is modified
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:ddd d MMM}")]
        public DateTimeOffset? ModifiedDate { get; set; }


        /// <summary>
        /// user who modified configuration
        /// </summary>
        public string ModifiedBy { get; set; }

    }


    /// <summary>
    /// View model for list partial
    /// </summary>
    public interface ISpotCheckListPartialViewModel
    {
        bool EnableEditing { get; }

        IList<SpotCheckConfigurationModel> SpotCheckList { get; set; }
    }
}