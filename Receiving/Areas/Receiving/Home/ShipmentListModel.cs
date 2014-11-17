using System;
using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.Receiving.Areas.Receiving.Home
{
    public class ShipmentListModel
    {
        public string ShipmentId { get; set; }

       [DisplayFormat(NullDisplayText = "NA")]
        public long? PoNumber { get; set; }

      
        public string IntransitType { get; set; }       

        [DisplayFormat(DataFormatString = "{0:g}")]
        public DateTimeOffset? MaxReceiveDate { get; set; }


        public int ExpectedQuantity { get; set; }

        public int ReceivedQuantity { get; set; }

        public string ErpType { get; set; }

        public int CartonReceived { get; set; }

        public int CartonCount { get; set; }


       [DisplayFormat(NullDisplayText = "NA")]
        public long? ProcessNumber { get; set; }

        public DateTime ShipmentDate { get; set; }
    }
}