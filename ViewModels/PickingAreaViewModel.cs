using System;
using System.Collections.Generic;

namespace DcmsMobile.CartonAreas.ViewModels
{
    public class PickingAreaViewModel
    {
        public IList<PickingAreaModel> PickingAreaList { get; set; }

        public string BuildingId { get; set; } 

        public string AreaId { get; set; }

        public bool IsPickingArea { get; set; }

        public bool IsShippingArea { get; set; }

        public bool IsRestockArea { get; set; }

        public bool LocationNumberingFlag { get; set; }

        public string Description { get; set; }
       
      
    }

    public class PickingAreaModel
    {
        public bool IsPickingArea { get; set; }

        public bool IsShippingArea { get; set; }

        public bool IsRestockArea { get; set; }

        public bool LocationNumberingFlag { get; set; }

        public string AreaId { get; set; }

        public string Description { get; set; }

        public string ShortName { get; set; }

        public int LocationCount { get; set; }
    }
}