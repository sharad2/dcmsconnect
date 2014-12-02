using System;

namespace DcmsMobile.Inquiry.Areas.Inquiry.PickslipEntity
{
    internal class PickslipHeadline
    {
        public long PickslipId { get; set; }

        public DateTime? StartDate { get; set; }

        public string PoId { get; set; }

        public int? Iteration { get; set; }

        public DateTime? DcCancelDate { get; set; }

        public string CustomerName { get; set; }

        public string CustomerId { get; set; }

        public DateTime? CancelDate { get; set; }

        public string ShippingId { get; set; }

        public DateTime? PickslipCancelDate { get; set; }

        public DateTime? ImportDate { get; set; }

        public string ExportFlag { get; set; }

        public DateTime? TransferDate { get; set; }

        public int? TotalQuantityOrdered { get; set; }

        /// <summary>
        /// This property is added to contain validated box expected pieces in case of PO
        /// </summary>
        public int? ExpectedPieces { get; set; }

        /// <summary>
        /// This property is added to contain pitched pieces in case of PO
        /// </summary>
        public int? CurrentPieces { get; set; }

        public string ShipperName { get; set; }

        public string ShipmentOnHold { get; set; }

        public DateTime? ShipDate { get; set; }

        public DateTime? ValidationDate { get; set; }

    }

    internal class Pickslip : PickslipHeadline
    {
        public string CarrierId { get; set; }
        
        public DateTime? CreateDate { get; set; }

        public string CustomerDC { get; set; }

        public string CustomerStore { get; set; }

        public string CustomerDepartmentId { get; set; }

        public int? BucketId { get; set; }

        public string BucketCreatedBy { get; set; }

        public DateTime? BucketCreatedOn { get; set; }
        
        public string[] ShipAddress
        {
            get;
            set;
        }

        public string ShipZipCode { get; set; }

        public string ShipCity { get; set; }

        public string ShipState { get; set; }

        public string ShipCountry { get; set; }
        public string VendorNumber { get; set; }

        public bool AsnFlag { get; set; }

        public string ErpId { get; set; }
    }

}




//$Id$