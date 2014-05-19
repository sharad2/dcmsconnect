using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.CartonAreas.ViewModels
{
    public class CartonAreaViewModel
    {
        [Display(Name = "Name_AreaList", ResourceType = typeof(Resources.CartonAreasResource))]
        public IEnumerable<CartonAreaModel> CartonAreaList { get; set; }

        // This property contains the information of current area, which will be updated.
        public CartonAreaModel CurrentArea { get; set; }

    }
}
//$Id$