using DcmsMobile.Inquiry.Helpers;
using System;
using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.Inquiry.Areas.Inquiry.BoxEntity
{
    /// <summary>
    /// Represents a VAS along with flags indicating whether VAS is required/completed
    /// </summary>
    internal class BoxVas
    {
        public string VasDescription { get; set; }

        public bool IsRequired { get; set; }

        public bool IsComplete { get; set; }
    }
    /// <summary>
    /// BoxSku class contains the sku details in the box
    /// </summary>
    internal class BoxSku:SkuBase
    {
        public int? ExpectedPieces { get; set; }

        public decimal? ExtendedPrice { get; set; }

        public int? CurrentPieces { get; set; }

        public string MinPicker { get; set; }

        public string VwhId { get; set; }
    }

    /// <summary>
    /// ViewModel for Box view
    /// </summary>
    internal class Box
    {
        public string Ucc128Id { get; set; }

        public string ProNo { get; set; }

        #region Pickslip Properties
        public long PickslipId { get; set; }

        public int? BucketId { get; set; }

        public string CustomerDC { get; set; }

        public string CustomerStore { get; set; }

        public string CustomerId { get; set; }

        public string CustomerName { get; set; }

        #endregion

   
        public string PalletId { get; set; }

        public string IaId { get; set; }

        public string ShortName { get; set; }

        public string IaShortDescription { get; set; }

        /// <summary>
        /// This property is added to show building of area
        /// </summary>
        public string Building { get; set; }

        //The property is added because we show's carton on box pallet scan
        public string CartonId { get; set; }

        public string[] ToAddress
        {
            get;
            set;
        }

        public string ToCity { get; set; }

        public string ToState { get; set; }

        public string RfidTagsRequired { get; set; }

        public DateTime? QcDate { get; set; }

        public DateTime? SuspenseDate { get; set; }
        
        public string RejectionCode { get; set; }

        public DateTime? VerificationDate { get; set; }

        /// <summary>
        /// Not showing year in the display format
        /// </summary>
        public DateTime? PitchingEndDate { get; set; }

        public string VwhId { get; set; }

        /// <summary>
        /// The area in which the box exists
        /// </summary>
        [Display(Name = "Area")]
        public string Area { get; set; }

        public int? ExpectedPieces { get; set; }

        public int? CurrentPieces { get; set; }

        public string LastUccPrintedBy { get; set; }

        public string LastCclPrintedBy { get; set; }

        public DateTime? LastUccPrintedDate { get; set; }

        public DateTime? LastCclPrintedDate { get; set; }
        
        public string MinPickerName { get; set; }

        public DateTime? StopProcessDate { get; set; }

        public string StopProcessReason { get; set; }
        
        //[Obsolete]
        //public string ListOfIncompleteVas { get; set; }

        //[Obsolete]
        //public string ListOfCompleteVas { get; set; }

        public string CatalogDocumentId { get; set; }        
    }

    /// <summary>
    /// To hold these query results
    ///SELECT bd.SKU_ID AS SKUID, BDEPC.EPC AS EPC
    ///  FROM BOXDET BD
    ///  LEFT OUTER JOIN BOXDET_EPC BDEPC
    ///    ON BDEPC.BOXDET_ID = BD.BOXDET_ID
    /// WHERE BD.UCC128_ID = '00000146710004870813';
    /// </summary>
    public class Epc
    {
        public int SkuId { get; set; }

        public string EpcCode { get; set; }
    }
}

//$Id$
