using System;
using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.Shipping.Repository
{
    public class CustomerOrderSummary
    {
        [Key]
        public string CustomerId { get; set; }

        public string CustomerName { get; set; }

        public int? CountRoutedPo { get; set; }        

        public int? CountUnroutedPo { get; set; }

        public int? CountRoutingPo { get; set; }

        public DateTime? StartDate { get; set; }
      
        public DateTime? MaxDcCancelDate { get; set; }

        public int? PiecesOrdered { get; set; }

        //public int? RoutedPieces { get; set; }

        public Decimal? TotalDollarsOrdered { get; set; }

        public int? CustomerPosCount { get; set; }

        /// <summary>
        /// Number of POs which exist some BOL
        /// </summary>
        public int? CountPosInBol { get; set; }

        /// <summary>
        /// Customer for which EDI is sent and received electronically.
        /// </summary>
        public string EdiCustomer { get; set; }    


        /// <summary>
        /// NO of Unshipped BOLs
        /// </summary>
        public int? TotalUnshippedBols { get; set; }
    }
}