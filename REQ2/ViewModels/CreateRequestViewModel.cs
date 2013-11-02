using DcmsMobile.REQ2.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace DcmsMobile.REQ2.ViewModels
{
    public class EditRequestHeaderModel : RequestHeaderModel
    {
        public EditRequestHeaderModel()
        {

        }

        public EditRequestHeaderModel(Request entity)
            : base(entity)
        {

        }

        [Required(ErrorMessage = "Building is required")]
        public override string BuildingId
        {
            get { return base.BuildingId; }
            set { base.BuildingId = value; }
        }

        [Required(ErrorMessage = "VWh is required")]
        public override string VirtualWareHouseId
        {
            get { return base.VirtualWareHouseId; }
            set { base.VirtualWareHouseId = value; }
        }

        [Required(ErrorMessage = "Pull From Area is required")]
        public override string SourceAreaId
        {
            get { return base.SourceAreaId; }
            set { base.SourceAreaId = value; }
        }

        [Required(ErrorMessage = "Pull To Area is required")]
        public override string DestinationAreaId
        {
            get { return base.DestinationAreaId; }
            set { base.DestinationAreaId = value; }
        }

        [Required(ErrorMessage = "Priority is required")]
        public override int Priority
        {
            get { return base.Priority; }
            set { base.Priority = value; }
        }

    }

    public class CreateRequestViewModel
    {
        public EditRequestHeaderModel CurrentRequest { get; set; }
        
        public IEnumerable<SelectListItem> BuildingList { get; set; }

        public IEnumerable<SelectListItem> VirtualWareHouseList { get; set; }

        public IEnumerable<SelectListItem> SewingPlantCodes { get; set; }

        public IEnumerable<SelectListItem> PriceSeasonCodes { get; set; }

        public IEnumerable<SelectListItem> TargetVwhList { get; set; }

        public IEnumerable<SelectListItem> SourceQualityCodeList { get; set; }

    }
}
//$Id$