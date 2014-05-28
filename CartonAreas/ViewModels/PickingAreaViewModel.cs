using System.Collections.Generic;

namespace DcmsMobile.CartonAreas.ViewModels
{
    public class PickingAreaViewModel
    {
        public IList<PickingAreaModel> PickingAreaList { get; set; }

        public string BuildingId { get; set; }
    }

    public class PickingAreaModel : AreaModel
    {
        public bool IsPickingArea { get; set; }

        public bool IsShippingArea { get; set; }

        public bool IsRestockArea { get; set; }

        public bool LocationNumberingFlag { get; set; }
    }
}