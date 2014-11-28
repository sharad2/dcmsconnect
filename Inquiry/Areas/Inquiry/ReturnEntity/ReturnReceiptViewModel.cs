using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace DcmsMobile.Inquiry.Areas.Inquiry.ReturnEntity
{
    public class ReturnReceiptViewModel
    {
        [Display(Name = "Returns Authorization Number")]
        public string ReturnNumber { get; set; }

       
        [Display(Name = "Receipt Number")]
        public string ReceiptNumber { get; set; }

       
        [Display(Name = "Customer")]
        public string CustomerId { get; set; }

        public string CustomerName { get; set; }

        [Display(Name = "Received Date")]
        [DisplayFormat(DataFormatString="{0:d}")]
        public DateTime ReceivedDate { get; set; }

       
        [Display(Name = "Carrier")]
        public string CarrierId { get; set; }

        public string CarrierDescription { get; set; }

        [Display(Name = "Carrier")]
        [DisplayFormat(NullDisplayText = "None")]
        public string CarrierDisplay
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(this.CarrierId))
                {
                    return string.Format("{0}: {1}", this.CarrierId, this.CarrierDescription);
                }
                return null;
            }
        }

        [Display(Name = "DM Number")]
        public string DMNumber { get; set; }

        [Display(Name = "Inserted By")]
        public string InsertedBy { get; set; }

        [Display(Name = "Insert Date")]
        [DisplayFormat(DataFormatString = "{0:d}")]
        public DateTime InsertDate { get; set; }

        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; }

        [Display(Name = "Modified Date")]
        [DisplayFormat(DataFormatString="{0:d}")]
        public DateTime ModifiedDate { get; set; }
       
        public string VwhId { get; set; }

        [Display(Name = "Activity Id")]
        public int? ActivityId { get; set; }

       public IList<ReturnSkuModel> ReturnSku { get; set; }

        [Display(Name = "Customer Store")]
        public string CustomerStoreId { get; set; }

        [Display(Name = "Dm Date")]
        public DateTime? DmDate { get; set; }

         [Display(Name = "Reason")]
        public string ReasonCode { get; set; }

        public string ReasonDescription { get; set; }

        [Display(Name="Reason")]
        [DisplayFormat(NullDisplayText="None")]
        public string ReasonDisplay
        {
            get
            {
                if(!string.IsNullOrWhiteSpace(this.ReasonCode))
                {
                    return string.Format("{0}: {1}", this.ReasonCode, this.ReasonDescription);
                }
                return null;
            }
        }

        [DisplayFormat(DataFormatString="$ {0:N2}")]
        public decimal? TotalRetailPrice
        {
            get
            {
                return this.ReturnSku.Count > 0 ? this.ReturnSku.Sum(p => p.RetailPrice) : null;
                
            }
        }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? TotalPieces
        {
            get
            {
                return this.ReturnSku.Count > 0 ? this.ReturnSku.Sum(p => p.Pieces) : null;

            }
        }
        
        [DisplayFormat(DataFormatString="{0:N0}")]
        [Display(Name = "Expected Pieces")]
        public int? ExpectedPieces { get; set; }

    }
}