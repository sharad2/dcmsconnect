using System;
using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.Receiving.Areas.Receiving.Home.Repository
{
    /// <summary>
    /// Information about a specific receiving process
    /// </summary>
    public class ReceivingProcess
    {
        [Key]
        public int ProcessId { get; set; }

        public string ProNumber { get; set; }

        public string CarrierId { get; set; }

        public string CarrierName { get; set; }

        public DateTime? ProDate { get; set; }

        public string OperatorName { get; set; }

        public DateTime? StartDate { get; set; }

        public int PalletCount { get; set; }

        public int CartonCount { get; set; }

        public int? ExpectedCartons { get; set; }

        public int? PalletLimit { get; set; }

        public string PriceSeasonCode { get; set; }

        public string SpotCheckAreaId { get; set; }

        public string ReceivingAreaId { get; set; }

        public DateTime? ReceivingEndDate { get; set; }
    }
}



//$Id$