using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.CartonAreas.ViewModels
{
    public class CartonAreaViewModel
    {
        [Display(Name = "Name_AreaList", ResourceType = typeof(Resources.CartonAreasResource))]
        public IList<CartonAreaModel> CartonAreaList { get; set; }

        // This property contains the information of current area, which will be updated.
        public CartonAreaModel CurrentArea { get; set; }

        public string BuildingId { get; set; }

    }
}
//$Id$