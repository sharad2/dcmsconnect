using DcmsMobile.Inquiry.Areas.Inquiry.SharedViews;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

namespace DcmsMobile.Inquiry.Areas.Inquiry.BoxEntity
{

    public class BoxVasModel
    {
        internal BoxVasModel(BoxVas entity)
        {
            VasDescription = entity.VasDescription;
            IsComplete = entity.IsComplete;
            IsRequired = entity.IsRequired;
        }
        public string VasDescription { get; set; }

        public bool IsRequired { get; set; }

        public bool IsComplete { get; set; }
    }

    public enum BoxStatus
    {
        Active,
        Cancelled,
        Transferred
    }

    /// <summary>
    /// This view model is used to post the information required to bring box labels
    /// </summary>
    public class BoxViewModel
    {

        public BoxViewModel()
        {

        }

        internal BoxViewModel(Box entity)
        {
            this.LastCclPrintedBy = entity.LastCclPrintedBy;
            this.LastUccPrintedBy = entity.LastUccPrintedBy;
            this.CustomerDc = entity.CustomerDC;
            this.CustomerStore = entity.CustomerStore;
            this.QcDate = entity.QcDate;
            this.CustomerDisplayName = string.Format("{0} : {1} ", entity.CustomerId,
                                                     entity.CustomerName);
            this.CustomerId = entity.CustomerId;
            this.VwhId = entity.VwhId;
            this.RejectionCode = entity.RejectionCode;
            this.RfidTagsRequired = entity.RfidTagsRequired == "Y";
            this.SuspenseDate = entity.SuspenseDate;
            this.ProNo = entity.ProNo;
            this.PalletId = entity.PalletId;
            if (entity.ToAddress != null)
            {
                this.ToAddressLines = entity.ToAddress.Where(p => !string.IsNullOrEmpty(p)).ToArray();
                this.City = entity.ToCity;
                this.State = entity.ToState;
            };
            this.Ucc128Id = entity.Ucc128Id;
            this.StopProcessDate = entity.StopProcessDate;
            switch (entity.StopProcessReason)
            {
                case "$XREF":
                    StopProcessReason = BoxStatus.Transferred;
                    break;

                case "$BOXCANCEL":
                    StopProcessReason = BoxStatus.Cancelled;
                    break;

                default:
                    StopProcessReason = BoxStatus.Active;
                    break;
            }
            if (entity.StopProcessReason == "$XREF")
            {

            }
            //this.StopProcessReason = entity.StopProcessReason;
            this.LastCclPrintedDate = entity.LastCclPrintedDate;
            this.LastUccPrintedDate = entity.LastUccPrintedDate;
            this.PickslipId = entity.PickslipId;
            this.ValidationDate = entity.VerificationDate;
            this.IaId = entity.IaId;
            this.CanPrintCatalog = !string.IsNullOrWhiteSpace(entity.CatalogDocumentId);
            this.AreaShortName = entity.ShortName;
            this.Building = entity.Building;
            this.IaShortDescription = entity.IaShortDescription;
            this.PitchingEndDate = entity.PitchingEndDate;
            this.CartonId = entity.CartonId;
        }

        public string CustomerId { get; set; }

        //[AdditionalMetadata(ExcelAttribute.BUTTON_NAME, ExcelAttribute.BUTTON_NAME)]
        //[Display(Name="List of SKUs in box",ShortName="SKU in Box")]
        public IList<BoxSkuModel> SkuWithEpc { get; set; }

        /// <summary>
        /// BoxPallet Displays this in its grid
        /// </summary>
        [Display(Name = "Last CCL Printed Date")]
        [DisplayFormat(DataFormatString="{0:g}", NullDisplayText="Never")]
        public DateTime? LastCclPrintedDate { get; set; }

        /// <summary>
        /// BoxPallet Displays this in its grid
        /// </summary>
        [Display(Name = "Last UCC Printed Date")]
        [DisplayFormat(DataFormatString = "{0:g}", NullDisplayText = "Never")]
        public DateTime? LastUccPrintedDate { get; set; }


        /// <summary>
        /// BoxPallet Displays this in its grid
        /// </summary>
        [Display(ShortName = "Pickslip", Name = "Pickslip")]
        public long PickslipId { get; set; }

        [DisplayFormat(DataFormatString = "{0:g}", NullDisplayText = "Not Validated")]
        public DateTime? ValidationDate { get; set; }

        /// <summary>
        /// Pickslip page displays this in grid
        /// </summary>
        [Display(Name = "Area")]
        public string IaId { get; set; }

        [DisplayFormat(NullDisplayText = "Non Physical")]
        public string AreaShortName { get; set; }

        public string IaShortDescription { get; set; }

        /// <summary>
        /// This property is added to show building of area
        /// </summary>
        //[DisplayFormat(NullDisplayText = "Not Defined")]
        public string Building { get; set; }


        [Display(Name = "Pallet")]
        [DisplayFormat(NullDisplayText = "Not on pallet")]
        [RegularExpression(@"P\S{1,}", ErrorMessage = "Pallet ID must begin with P")]
        [DataType("Alert")]
        public string PalletId { get; set; }

        public string[] ToAddressLines { get; set; }

        public string City { get; set; }

        public string State { get; set; }

        [Display(Name = "Tracking #")]
        [DisplayFormat(NullDisplayText = "Not Assigned")]
        public string ProNo { get; set; }

        [Display(Name = "Suspense Date")]
        [DisplayFormat(DataFormatString = "{0:g}")]
        public DateTime? SuspenseDate { get; set; }

        [Display(Name = "Rejection Code")]
        public string RejectionCode { get; set; }

        public bool RfidTagsRequired { get; set; }

        [Display(Name = "VWH")]
        [DisplayFormat(NullDisplayText = "Unknown")]
        public string VwhId { get; set; }

        [Display(ShortName = "Customer", Name = "Customer")]
        public string CustomerDisplayName { get; set; }

        [Display(Name = "Last UCC Printed By")]
        [DisplayFormat(DataFormatString = "{0}")]
        public string LastUccPrintedBy { get; set; }

        [Display(Name = "Last CCL Printed By")]
        [DisplayFormat(DataFormatString="{0}")]
        public string LastCclPrintedBy { get; set; }

        /// <summary>
        /// Pickslip page displays this in grid
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:g}", NullDisplayText = "Not Pitched")]
        [Display(Name = "Pitched On")]
        public DateTime? PitchingEndDate { get; set; }


        [Display(Name = "Customer DC", ShortName = "DC")]
        public string CustomerDc { get; set; }

        [Display(ShortName = "Store", Name = "Customer Store")]
        public string CustomerStore { get; set; }

        public IList<SelectListItem> PrinterList { get; set; }

        [DisplayFormat(DataFormatString = "{0}", NullDisplayText = "Not Performed")]
        public DateTime? QcDate { get; set; }

        #region These printing choices are posted

        public string Ucc128Id { get; set; }

        [Display(Name = "Catalog")]
        public bool PrintCatalog { get; set; }

        [Display(Name = "UCC")]
        public bool PrintUcc { get; set; }

        [Display(Name = "CCL")]
        public bool PrintCcl { get; set; }

        [Display(Name = "Printer")]
        public string PrinterId { get; set; }

        #endregion


        public BoxStatus StopProcessReason { get; set; }

        public bool ShowPrintDialog { get; set; }

        public IList<BoxAuditModel> AuditList { get; set; }

        /// <summary>
        /// List of required VAS along with a flag indicating whether the VAS has been completed
        /// </summary>
        public IList<BoxVasModel> VasStatusList { get; set; }

        /// <summary>
        /// The property ios added to determine whether we can print catalog for a particular coustomer or not.
        /// </summary>
        public bool CanPrintCatalog { get; set; }

        /// <summary>
        /// Whether the user is authorized to cancel the box
        /// </summary>
        public bool CanCancelBox { get; set; }

        /// <summary>
        /// The role required for managerial functions
        /// </summary>
        public string ManagerRoleName { get; set; }

        [DisplayFormat(NullDisplayText="Box is Active", DataFormatString="{0:g}")]
        public DateTime? StopProcessDate { get; set; }


        public string CartonId { get; set; }

        public string PickerNames { get; set; }

        public string UrlManageVas { get; set; }

        public string UrlScanToPallet { get; set; }

        public string UrlLogin { get; set; }
    }
}