using DcmsMobile.Inquiry.Areas.Inquiry.SharedViews;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

namespace DcmsMobile.Inquiry.Areas.Inquiry.PickslipEntity
{
    public class PickslipViewModel : IBoxListViewModel
    {

        public PickslipViewModel()
        {

        }

        internal PickslipViewModel(Pickslip pickslip)
        {
            //this.CancelDate = pickslip.CancelDate;
            this.CustomerId = pickslip.CustomerId;
            this.CustomerName = pickslip.CustomerName;
            this.DcCancelDate = pickslip.DcCancelDate;
            this.Iteration = pickslip.Iteration;
            this.PoId = pickslip.PoId;
            this.StartDate = pickslip.StartDate;
            this.CustomerDC = pickslip.CustomerDC;
            this.CustomerStore = pickslip.CustomerStore;
            this.CarrierId = pickslip.CarrierId;

            this.BucketId = pickslip.BucketId;
            //this.BoxCreatedOn = pickslip.BoxCreatedOn;
            this.BucketCreatedBy = pickslip.BucketCreatedBy;
            this.BucketCreatedOn = pickslip.BucketCreatedOn;

            this.CustomerDepartmentId = pickslip.CustomerDepartmentId;
            if (pickslip.ShipAddress != null)
            {
                this.ShipAddress = string.Join(", ", pickslip.ShipAddress
                    .Concat(new[] { 
                    pickslip.ShipCity,
                    string.Format("{0} {1}", pickslip.ShipState, pickslip.ShipZipCode),
                    pickslip.ShipCountry
                }).Where(p => !string.IsNullOrWhiteSpace(p))
                    );
            }
            //this.ShipCity = pickslip.ShipCity;
            //this.ShipState = pickslip.ShipState;
            //this.ShipCountry = pickslip.ShipCountry;
            //this.ShipZipCode = pickslip.ShipZipCode;
            this.VendorNumber = pickslip.VendorNumber;
            this.ExportFlag = pickslip.ExportFlag == "Y";
            this.ErpId = pickslip.ErpId;
            this.AsnFlag = pickslip.AsnFlag;
            this.PickslipId = pickslip.PickslipId;
            this.ImportDate = pickslip.ImportDate;
            this.PickslipCancelDate = pickslip.PickslipCancelDate;
            this.TotalQuantityOrdered = pickslip.TotalQuantityOrdered;
            this.CompletedPieces = pickslip.ExpectedPieces;
            this.PitchedPieces = pickslip.CurrentPieces;
            this.ShippingId = pickslip.ShippingId;
            this.ShipDate = pickslip.ShipDate;
            this.ShipperName = pickslip.ShipperName;
            this.ShipmentOnHold = string.IsNullOrEmpty(pickslip.ShipmentOnHold);
            //this.TotalSkuOrdered = pickslip.TotalSKUOrdered;
            this.TransferDate = pickslip.TransferDate;
        }

        /// <summary>
        /// Is this an international pickslip? Yes or no.
        /// </summary>
        [Display(Name = "Export Flag")]
        public bool ExportFlag { get; set; }

        [Display(Name = "ASN Flag")]
        public bool AsnFlag { get; set; }

        [Display(Name = "ERP Id")]
        [DisplayFormat(NullDisplayText="Unknown")]
        public string ErpId { get; set; }


        [Display(Name = "Carrier")]
        [DisplayFormat(NullDisplayText = "Unknown")]
        public string CarrierId { get; set; }


        ////public PickslipModel Pickslip { get; set; }

        [Display(Name = "Customer")]
        public string CustomerId { get; set; }

        [Display(Name = "Name")]
        public string CustomerName { get; set; }

        public string CustomerDisplayName
        {
            get
            {
                return string.Format("{0} : {1}", this.CustomerId, this.CustomerName);
            }
        }

        [Display(Name = "PO")]
        public string PoId { get; set; }


        public int? Iteration { get; set; }


        [Display(Name = "Start Date")]
        [DisplayFormat(DataFormatString = "{0:d}")]
        public DateTime? StartDate { get; set; }

        //[Display(Name = "Cancel Date")]
        //[DisplayFormat(DataFormatString = "{0:d}")]
        //public DateTime? CancelDate { get; set; }

        [Display(Name = "DC Cancel Date")]
        [DisplayFormat(DataFormatString = "{0:d}")]
        public DateTime? DcCancelDate { get; set; }

        public IList<PickslipSkuModel> AllSku { get; set; }

        public IList<BoxHeadlineModel> AllBoxes { get; set; }

        [Display(Name = "DC")]
        [DisplayFormat(NullDisplayText = "None")]
        public string CustomerDC { get; set; }

        [Display(Name = "Store")]
        [DisplayFormat(NullDisplayText = "None")]
        public string CustomerStore { get; set; }

        [Display(Name = "Department")]
        [DisplayFormat(NullDisplayText = "N/A")]
        public string CustomerDepartmentId { get; set; }


        //Bucket Id of box pallet
        [Display(Name = "Wave")]
        public int? BucketId { get; set; }

        //The following properties are added to show bucket information of pickslip on pickslip page
        [Display(Name = "Created by")]
        public string BucketCreatedBy { get; set; }

        [Display(Name = "Created")]
        [DisplayFormat(DataFormatString = "{0:g}")]
        public DateTime? BucketCreatedOn { get; set; }

        //[Display(Name = "Box Creation Date")]
        //public DateTime? BoxCreatedOn { get; set; }

        /// <summary>
        /// A descriptive title for the model. Contains information on how the data was retrieved.
        /// Suitable for displaying as page title.
        /// </summary>
        public string ModelTitle { get; set; }

        /// <summary>
        /// This value is posted while printing
        /// </summary>
        public long PickslipId { get; set; }

        [Display(Name = " Master Packing Slip")]
        public bool IsPrintMasterPack { get; set; }

        [Display(Name = " Packing Slip")]
        public bool IsPrintPackingslip { get; set; }

        [Display(Name = "Printer")]
        public string PrinterId { get; set; }

        public IEnumerable<SelectListItem> PrinterList { get; set; }

        //[Display(Name = " # Copies")]
        //public int NumberOfCopies { get; set; }

        public string BoxListCaption
        {
            get
            {
                if (AllBoxes.Count < TotalBoxes)
                {
                    return string.Format("{0:N0} of {1:N0} boxes listed", AllBoxes.Count, TotalBoxes);
                }
                else
                {
                    return string.Format("{0:N0} Boxes", this.AllBoxes.Count);
                }
            }
        }


        public string ShipAddress
        {
            get;
            set;
        }

        [Display(Name = " Vendor")]
        public string VendorNumber { get; set; }

        [Display(Name = "Imported On")]
        [DisplayFormat(DataFormatString = "{0:g}")]
        public DateTime? ImportDate { get; set; }

        /// <summary>
        /// Non null if the pickslip has been cancelled
        /// </summary>
        [Display(Name = "Cancelled On")]
        [DisplayFormat(DataFormatString = "{0:d}")]
        public DateTime? PickslipCancelDate { get; set; }


        [Display(Name = "Total Quantity Ordered")]
        [DisplayFormat(NullDisplayText = "Null")]
        [DataType("Alert")]
        public int? TotalQuantityOrdered { get; set; }

        //[Display(Name = "Total Quantity Ordered")]
        //[DisplayFormat(NullDisplayText = "Null")]        
        //public int? TotalSkuOrdered { get; set; }

        /// <summary>
        /// This property is added to contain completed pieces in case of PO
        /// </summary>
        public int? CompletedPieces { get; set; }

        [Display(Name = "Shipping ID")]
        [DisplayFormat(NullDisplayText = "Not Created")]
        public string ShippingId { get; set; }

        /// <summary>
        /// The date on which the pickslip was shipped
        /// </summary>
        [Display(Name = "Shipped On")]
        [DisplayFormat(DataFormatString = "Shipped {0:g}", NullDisplayText = "Not Shipped")]
        //[DisplayFormat(, NullDisplayText = "Not Shipped")]
        public DateTime? ShipDate { get; set; }


        public string ShipperName { get; set; }

        public bool ShipmentOnHold { get; set; }

        public bool IsComplete
        {
            get
            {
                if (this.TotalQuantityOrdered.HasValue && this.CompletedPieces.HasValue)
                {
                    return this.TotalQuantityOrdered == this.CompletedPieces;
                }
                if (this.TotalQuantityOrdered.HasValue && AllBoxes != null)
                {
                    return this.TotalQuantityOrdered == AllBoxes.Where(p => p.StopProcessDate.HasValue || p.ValidationDate.HasValue).Sum(p => p.ExpectedPieces);
                }
                else
                {
                    return false;
                }
            }
        }

        [DisplayFormat(DataFormatString = "{0:p0}")]
        public double PercentComplete
        {
            get
            {
                int pcs = 0;
                if (CompletedPieces.HasValue)
                {
                    pcs = CompletedPieces ?? 0;
                }
                else if (AllBoxes != null)
                {
                    pcs = AllBoxes.Where(p => p.StopProcessDate.HasValue || p.ValidationDate.HasValue).Sum(p => p.ExpectedPieces) ?? 0;
                }

                if (pcs == 0 || this.TotalQuantityOrdered == null || this.TotalQuantityOrdered == 0)
                {
                    return 0;
                }
                return (double)pcs / (double)this.TotalQuantityOrdered;
            }
        }

        /// <summary>
        /// This property is added to contain pitched pieces in case of PO
        /// </summary>
        public int? PitchedPieces { get; set; }



        [DisplayFormat(DataFormatString = "{0:p0} pieces picked")]
        public double PickingPercentComplete
        {
            get
            {
                int? pcs = 0;

                if (PitchedPieces.HasValue)
                {
                    pcs = PitchedPieces;
                }
                else if (AllBoxes != null)
                {
                    pcs = this.AllBoxes.Where(p => p.StopProcessReason != "$BOXCANCEL").Sum(p => p.CurrentPieces);
                }


                if (this.TotalQuantityOrdered == null || pcs == null)
                {
                    return (double)0;
                }
                return (double)pcs / (double)this.TotalQuantityOrdered;
            }
        }

        [Display(Name = "Transferred")]
        //[DisplayFormat(DataFormatString = "{0:d}")]
        public DateTime? TransferDate { get; set; }

        public int TotalBoxes { get; set; }


        public bool ShowPickslipLinks
        {
            get { return false; }
        }
    }
}