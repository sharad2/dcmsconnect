using System;
using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.Inquiry.Areas.Inquiry.PickslipEntity
{

    public class PickslipHeadlineModel
    {
        public PickslipHeadlineModel()
        {

        }

        internal PickslipHeadlineModel(PickslipHeadline entity)
        {
            this.ExportFlag = entity.ExportFlag;
            this.ImportDate = entity.ImportDate;
            this.PickslipCancelDate = entity.PickslipCancelDate;
            this.PickslipId = entity.PickslipId;
            this.TotalQuantityOrdered = entity.TotalQuantityOrdered;
            this.TransferDate = entity.TransferDate;
            this.ExpectedPieces = entity.ExpectedPieces;
            this.CompletedPitchedPieces = entity.CurrentPieces;
            this.ShippingId = entity.ShippingId;
            this.ShipDate = entity.ShipDate;
            this.ShipperName = entity.ShipperName;
            this.ShipmentOnHold = !string.IsNullOrEmpty(entity.ShipmentOnHold);
            this._validationDate = entity.ValidationDate;
        }


        [Display(Name = "Transferred On", Order=5)]
        [DataType(DataType.DateTime)]   
        public DateTime? TransferDate { get; set; }

        [Display(Name = "Export Flag")]
        [DisplayFormat(NullDisplayText = "None")]
        [ScaffoldColumn(false)]
        public string ExportFlag { get; set; }

        /// <summary>
        /// Non null if the pickslip has been cancelled
        /// </summary>
        [Display(Name = "Cancelled On",Order=3)]
        [DisplayFormat(DataFormatString = "{0:d}")]
        public DateTime? PickslipCancelDate { get; set; }

        [Display(Name = "Pickslip",Order=1)]
        [Required(ErrorMessage = "Pickslip should have Pickslip ID")]
        [DisplayFormat(NullDisplayText = "No Pickslip")]
        [DataType(DataType.Text)]
        public long PickslipId { get; set; }

        [Display(Name = "Imported On",Order=2)]
        [DisplayFormat(DataFormatString = "{0:g}")]
        [DataType(DataType.DateTime)]
        public DateTime? ImportDate { get; set; }

        [Display(ShortName = "Validated On",Order=4)]
        //[DataType(DataType.DateTime)]
        public DateTime? _validationDate { get; set; }


        [Display(Name = "Total Quantity Ordered", ShortName = "Pcs Ordered", Order = 7)]
        [Required(ErrorMessage = "Total ordered quantity is missing")]
        [DisplayFormat(NullDisplayText = "Null",DataFormatString = "{0:N0}")]
        [DataType("Alert")]
        public int? TotalQuantityOrdered { get; set; }

        /// <summary>
        /// This property is added to contain completed pieces in case of PO
        /// </summary>
        [ScaffoldColumn(false)]
        public int? ExpectedPieces { get; set; }

        [ScaffoldColumn(false)]
        public bool IsComplete
        {
            get
            {
                if (this.TotalQuantityOrdered.HasValue && this.ExpectedPieces.HasValue)
                {
                    return this.TotalQuantityOrdered == this.ExpectedPieces;
                }
                //if (this.TotalQuantityOrdered.HasValue && AllBoxes != null)
                //{
                //    return this.TotalQuantityOrdered == AllBoxes.Where(p => p.StopProcessDate.HasValue || p.ValidationDate.HasValue).Sum(p => p.ExpectedPieces);
                //}
                else
                {
                    return false;
                }
            }
        }

       [DisplayFormat(DataFormatString = "{0:p0}")]
        [ScaffoldColumn(false)]
        public double PickingPercentComplete
        {
            get
            {
                int? pcs = 0;

                if (CompletedPitchedPieces.HasValue)
                {
                    pcs = CompletedPitchedPieces;
                }
                //else if (AllBoxes != null)
                //{
                //    pcs = this.AllBoxes.Where(p => p.StopProcessReason != "$BOXCANCEL").Sum(p => p.CurrentPieces);
                //}


                if (this.TotalQuantityOrdered == null || pcs == null)
                {
                    return (double)0;
                }
                return (double)pcs / (double)this.TotalQuantityOrdered;
            }
        }

        /// <summary>
        /// Non null if the pickslip is complete
        /// </summary>
        [ScaffoldColumn(false)]
        [Display(Name = "Cancel Date")]
        //[DisplayFormat(DataFormatString = "{0:d}")]
        public DateTime? ValidationDate
        {
            get
            {
                if (this.IsComplete)
                {
                    return this._validationDate;
                }
                return null;

            }
        }

        /// <summary>
        /// This property is added to contain pitched pieces in case of PO
        /// </summary>       
        [Display(ShortName = "Pcs Pitched", Order = 6)]
        [DisplayFormat(DataFormatString = "{0:N0}",NullDisplayText = "0")]

        public int? CompletedPitchedPieces { get; set; }

        [ScaffoldColumn(false)]
        public string ShipperName { get; set; }
        
        [ScaffoldColumn(false)]
        public bool ShipmentOnHold { get; set; }

        [Display(Name = "Shipping ID",Order=8,ShortName="Shipment")]
        [DisplayFormat(NullDisplayText = "None")]
        public string ShippingId { get; set; }

        [DisplayFormat(DataFormatString = "{0:p0} Complete")]
        [ScaffoldColumn(false)]
        public double PercentComplete
        {
            get
            {
                int pcs = 0;
                if (ExpectedPieces.HasValue)
                {
                    pcs = ExpectedPieces ?? 0;
                }
                //else if (AllBoxes != null)
                //{
                //    pcs = AllBoxes.Where(p => p.StopProcessDate.HasValue || p.ValidationDate.HasValue).Sum(p => p.ExpectedPieces) ?? 0;
                //}

                if (pcs == 0 || this.TotalQuantityOrdered == null || this.TotalQuantityOrdered == 0)
                {
                    return 0;
                }
                return (double)pcs / (double)this.TotalQuantityOrdered;
            }
        }

        /// <summary>
        /// The date on which the pickslip was shipped
        /// </summary>
        [Display(Name = "Shipped On")]
        [DisplayFormat(NullDisplayText = "Not Shipped")]
        //[DisplayFormat(DataFormatString = "Shipped On {0:g}", NullDisplayText = "Not Shipped")]
        [ScaffoldColumn(false)]
        public DateTime? ShipDate { get; set; }
      
    }

}