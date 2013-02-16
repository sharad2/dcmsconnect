using System;
using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.DcmsLite.ViewModels.Receive
{
    public class AsnModel
    {
        [Display(Name = "ASN")]
        public string IntransitId { get; set; }

        [Display(Name = "Shipment")]
        public string ShipmentId { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        [Display(Name = "#Pieces")]
        public int Pieces { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        [Display(Name = "#Cartons")]
        public int CartonCount { get; set; }

        [Display(Name = "Received on")]
        public DateTime? ReceivedDate { get; set; }

        public bool IsReceived
        {
            get
            {
                return this.ReceivedDate != null;
            }
        }

        public string VwhId { get; set; }
    }
}