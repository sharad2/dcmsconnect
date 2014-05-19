using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DcmsMobile.CartonAreas.Repository
{
    public class Building
    {

        public string BuildingId { get; set; }

        public string Description { get; set; }

        public DateTime? InsertDate { get; set; }

        public string InsertedBy { get; set; }

        public string Address1 { get; set; }

        public string Address2 { get; set; }

        public string Address4 { get; set; }

        public string City { get; set; }

        public string State { get; set; }

        public int? ZipCode { get; set; }

        public int? CountArea { get; set; }

        public int? CountNumberedArea { get; set; }

        public int? CountLocation { get; set; }

        public string Address3 { get; set; }

        public int? ReceivingPalletLimit { get; set; }
    }
}