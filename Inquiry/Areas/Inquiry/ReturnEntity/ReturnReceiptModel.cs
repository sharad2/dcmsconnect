using System;
using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.Inquiry.Areas.Inquiry.ReturnEntity
{
    public class ReturnReceiptModel
    {
        public ReturnReceiptModel()
        {
           
        }

        internal ReturnReceiptModel(ReturnReceiptDetail entity)
        {
            this.ActivityId = entity.ActivityId;
            this.CustomerId = entity.CustomerId;
            this.CustomerName = entity.CustomerName;
            this.CustomerStoreId = entity.CustomerStoreId;
            this.ExpectedPieces = entity.ExpectedPieces;
            this.IsCompleteReceipt = entity.IsCompleteReceipt;
            this.NoOfCartons = entity.NoOfCartons;
            this.ReasonCode = entity.ReasonCode;
            this.ReasonDescription = entity.ReasonDescription;
            this.ReceiptNumber = entity.ReceiptNumber;
            this.ReceivedDate = entity.ReceivedDate;
            this.ReturnNumber = entity.ReturnNumber;
            this.TotalReceipts = entity.TotalReceipts;
        }
        /// <summary>
        /// This property will contain total number of receipts in an Return Authorization number
        /// </summary>
        [ScaffoldColumn(false)]
        public int TotalReceipts { get; set; }

        [ScaffoldColumn(false)]
        public string ReturnNumber { get; set; }

        [Display(ShortName = "Receipt",Order=1)]
        public string ReceiptNumber { get; set; }

        [Display(ShortName = "Is Complete Receipt", Order = 8)]
        public bool IsCompleteReceipt { get; set; }

        [ScaffoldColumn(false)]
        public string CustomerId { get; set; }

        [Display(ShortName = "Customer", Order = 3)]
        public string CustomerName { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        [Display(ShortName = "# Cartons", Order = 6)]
        public int? NoOfCartons { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        [Display(ShortName = "Pcs Expected", Order = 7)]
        public int? ExpectedPieces { get; set; }

        [DisplayFormat(DataFormatString = "{0:d}")]
        [Display(ShortName = "Received On", Order = 2)]
        public DateTimeOffset ReceivedDate { get; set; }

        [Display(ShortName="Activity",Order=9)]
        public int? ActivityId { get; set; }

        [Display(ShortName = "Store", Order = 4)]
        public string CustomerStoreId { get; set; }

        [ScaffoldColumn(false)]
        public string ReasonCode { get; set; }

        [Display(ShortName = "Reason", Order = 5)]
        [DisplayFormat(NullDisplayText="No Reason Specified")]
        public string ReasonDescription { get; set; }

    }
}
