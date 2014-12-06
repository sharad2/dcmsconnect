using System;

namespace DcmsMobile.DcmsLite.Repository.Receive
{
    public class AsnSummary
    {
        public string IntransitId { get; set; }

        public string ShipmentId { get; set; }

        public string VwhId { get; set; }

        public int CartonCount { get; set; }

        public DateTime? ReceivedDate { get; set; }

        public int Pieces { get; set; }
    }
}