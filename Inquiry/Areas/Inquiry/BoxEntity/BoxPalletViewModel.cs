using DcmsMobile.Inquiry.Areas.Inquiry.SharedViews;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace DcmsMobile.Inquiry.Areas.Inquiry.BoxEntity
{

    public class BoxPalletViewModel : IBoxListViewModel
    {

        //private string _palletToPrint;

        [Display(Name = "Pallet")]
        [Required(ErrorMessage = "Pallet should have Pallet ID")]
        public string PalletId { get; set; }

        public int UccPrintedBoxes { get; set; }

        public int CclPrintedBoxes { get; set; }

        /// <summary>
        /// Number of boxes on pallet
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int TotalBoxes
        {
            get
            {
                return AllBoxes.Count;
            }
        }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int PrintedBoxes
        {
            get
            {
                return Math.Max(UccPrintedBoxes, CclPrintedBoxes);
            }
        }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int UnprintedBoxes
        {
            get
            {
                return TotalBoxes - PrintedBoxes;
            }
        }

        public int UccUnprintedBoxes
        {
            get
            {
                return TotalBoxes - UccPrintedBoxes;
            }
        }

        public int CclUnprintedBoxes
        {
            get
            {
                return TotalBoxes - CclPrintedBoxes;
            }
        }

        [DisplayFormat(NullDisplayText = "Null")]
        public int? PickedBoxes { get; set; }

        [Display(Name = "Area")]
        public string Area { get; set; }

        public string ShortName { get; set; }

        /// <summary>
        /// This property is added to show building of area
        /// </summary>
        [DisplayFormat(NullDisplayText = "Not Defined")]
        public string Building { get; set; }

        //[AdditionalMetadata(ExcelAttribute.BUTTON_NAME, ExcelAttribute.BUTTON_NAME)]
        [Display(Name = "SKU on pallet", Order = 2)]
        public IList<BoxSkuModel> AllSku { get; set; }

        //[AdditionalMetadata(ExcelAttribute.BUTTON_NAME, ExcelAttribute.BUTTON_NAME)]
        [Display(Name = "Boxes on pallet", Order = 1)]
        public IList<BoxHeadlineModel> AllBoxes { get; set; }

        /// <summary>
        /// This property is added to show pallet productivity
        /// </summary>
        //[AdditionalMetadata(ExcelAttribute.BUTTON_NAME, ExcelAttribute.BUTTON_NAME)]
        [Display(Name = "Pallet History", Order = 3)]
        public IList<BoxPalletHistoryModel> PalletHistory { get; set; }

        [Display(Name = "Printer")]
        public string PrinterId { get; set; }

        public bool ShowPrintDialog { get; set; }

        public IEnumerable<SelectListItem> PrinterList { get; set; }

        public bool PrintAllBoxes { get; set; }

        [Display(Name = "UCC")]
        public bool PrintUcc { get; set; }

        [Display(Name = "CCL")]
        public bool PrintCcl { get; set; }

        [Display(Name = "Print Pallet Summary")]
        public bool IsPrintPalletSummary { get; set; }

        /// <summary>
        /// Posted user choice indicating whether individual box labels should be printed
        /// </summary>
        [Display(Name = "Print Boxes")]
        public bool IsPrintBoxes { get; set; }

        [Obsolete]
        public int? BoxLimit { get; set; }

        //public string BoxListCaption
        //{
        //    get
        //    {
        //        if (AllBoxes.Count < TotalBoxes)
        //        {
        //            return string.Format("{0:N0} of {1:N0} boxes listed", AllBoxes.Count, TotalBoxes);
        //        }
        //        else
        //        {
        //            return string.Format("{0:N0} boxes", this.AllBoxes.Count);
        //        }
        //    }
        //}



        //public string PalletLocatingLink { get; set; }

        //public string MovePalletLink { get; set; }
        private readonly IList<DcmsLinkModel> _dcmsLinks = new List<DcmsLinkModel>();
        public IList<DcmsLinkModel> DcmsLinks
        {
            get
            {
                return _dcmsLinks;
            }
        }




        public bool ShowPickslipLinks
        {
            get { return true; }
        }
    }
}