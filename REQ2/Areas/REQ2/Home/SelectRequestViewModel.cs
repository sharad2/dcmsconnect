using System.Collections.Generic;
using System.Web.Mvc;
using EclipseLibrary.Mvc.Html;

namespace DcmsMobile.REQ2.Areas.REQ2.Home
{
    public class SelectRequestViewModel
    {
        public RequestHeaderViewModel CurrentRequest { get; set; }

        public IEnumerable<SelectListItem> BuildingList { get; set; }

        public IEnumerable<SelectListItem> TargetQualityCodeList { get; set; }

        public IEnumerable<SelectListItem> VirtualWareHouseList { get; set; }

        public IEnumerable<GroupSelectListItem> DestinationAreas { get; set; }

        public IEnumerable<GroupSelectListItem> SourceAreas { get; set; }

        public IEnumerable<SelectListItem> TargetVwhList { get; set; }

        public IEnumerable<SelectListItem> SaleTypes { get; set; }

        public string PriorityId { get; set; }

    }
}
//$Id$