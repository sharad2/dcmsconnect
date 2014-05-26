using System;
using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.CartonAreas.ViewModels
{
    public class BuildingModel
    {
        public string BuildingId { get; set; }

        public string Description { get; set; }

        public DateTime? InsertDate { get; set; }

        public string InsertedBy { get; set; }

        public string Address1 { get; set; }

        public string Address2 { get; set; }

        public string Address3 { get; set; }

        public string Address4 { get; set; }

        public string City { get; set; }

        public string State { get; set; }

        public string ZipCode { get; set; }

        public string CountryCode { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? CountCartonArea { get; set; }

        public int? CountPickingAreas { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? CountNumberedArea { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? CountLocation { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}",NullDisplayText = "Not set")]
        public int? ReceivingPalletLimit { get; set; }
    }
}