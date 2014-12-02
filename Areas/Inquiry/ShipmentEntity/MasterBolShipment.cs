using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DcmsMobile.Inquiry.Areas.Inquiry.ShipmentEntity
{
    internal class MasterBolShipment
    {
        public string ShippingId { get; set; }

        public bool OnHold { get; set; }

        public DateTime? ArrivalDate { get; set; }

        public DateTime? ShippingDate { get; set; }

    }
}