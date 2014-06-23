using System;

namespace DcmsMobile.Receiving.Repository
{
    public class ShipmentList
    {
        public string ShipmentId { get; set; }

        public long? PoNumber { get; set; }

        public string IntransitType { get; set; }   
     
        public DateTimeOffset? MaxReceiveDate { get; set; }

        public int ExpectedQuantity { get; set; }

        public int ReceivedQuantity { get; set; }

        public string ErpType { get; set; }

        public int CartonReceived { get; set; }

        public int CartonCount { get; set; }

        public long? ProcessNumber { get; set; }

        public DateTime ShipmentDate { get; set; }
    }
}
