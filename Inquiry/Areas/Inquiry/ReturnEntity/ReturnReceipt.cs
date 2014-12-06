using System;

namespace DcmsMobile.Inquiry.Areas.Inquiry.ReturnEntity
{

    internal class ReturnReceiptBase
    {
        public string ReturnNumber { get; set; }

        public string CustomerId { get; set; }

        public string CustomerName { get; set; }

        /// <summary>
        /// This property will contain total number of receipts in an Return Authorization number
        /// </summary>
        public int TotalReceipts { get; set; }

        public int? NoOfCartons { get; set; }

        public int? ExpectedPieces { get; set; }

        public DateTime ReceivedDate { get; set; }
    }

    internal class ReturnReceiptHeadline : ReturnReceiptBase
    {

        public int? CustomerCount { get; set; }

    }

    internal class ReturnReceiptDetail : ReturnReceiptBase
    {
        

        public string ReceiptNumber { get; set; } 

        public bool IsCompleteReceipt { get; set; }

        public string CarrierId { get; set; }

        public string CarrierDescription { get; set; }

        public string DMNumber { get; set; }

        public string InsertedBy { get; set; }

        public DateTime InsertDate { get; set; }

        public string ModifiedBy { get; set; }

        public DateTime ModifiedDate { get; set; }

        public string VwhId { get; set; }

        public int? ActivityId { get; set; }

        public int? SkuId { get; set; }

        public string Upc { get; set; }

        public string Style { get; set; }

        public string Color { get; set; }

        public string Dimension { get; set; }

        public string SkuSize { get; set; }

        public string CustomerStoreId { get; set; }

        public DateTime? DmDate { get; set; }

        public string ReasonCode { get; set; }

        public string ReasonDescription { get; set; }

        public int? Quantity { get; set; }

        public decimal? RetailPrice { get; set; }

      

    }
}