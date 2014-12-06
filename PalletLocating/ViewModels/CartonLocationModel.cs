using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.PalletLocating.ViewModels
{
    /// <summary>
    /// Represents a destination location suggested to the user
    /// </summary>
    public class CartonLocationModel
    {
        [Display(Name = "Area")]
        public string AreaId { get; set; }


        public string AreaShortName { get; set; }

        [Display(Name = "Location")]
        public string LocationId { get; set; }

        [Display(Name = "# Cartons")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int CartonCount { get; set; }

        [Display(Name = "Max Cartons")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? MaxCartons { get; set; }

        [Display(Name = "Ctns at Locn")]
        [DisplayFormat(NullDisplayText = "Empty")]
        public string DisplayCartonCount
        {
            get
            {
                if (MaxCartons.HasValue)
                {
                    if (this.CartonCount == 0)
                    {
                        return string.Format("Max {0:#,###}", this.MaxCartons);
                    }
                    else
                    {
                        return string.Format("{0:#,###} of {1:#,###}", this.CartonCount, this.MaxCartons);
                    }
                }
                else
                {
                    return string.Format("{0:#,###}", this.CartonCount);
                }
            }
        }

        public int SkuCount { get; set; }

    }
}



/*
    $Id$ 
    $Revision$
    $URL$
    $Header$
    $Author$
    $Date$
*/