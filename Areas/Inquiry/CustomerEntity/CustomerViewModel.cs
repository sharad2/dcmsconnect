using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.Inquiry.Areas.Inquiry.CustomerEntity
{
    public class CustomerViewModel
    {
        [Display(Name = "Customer ID")]
        [Required(ErrorMessage = "Customer should have Customer ID")]
        [DataType("Alert")]
        public string CustomerId { get; set; }

        [Required(ErrorMessage = "Customer should have name")]
        [Display(Name = "Name")]
        [DataType("Alert")]
        public string CustomerName { get; set; }

        [Display(Name = "Type")]
        [DisplayFormat(NullDisplayText = "Not Defined")]
        public string Category { get; set; }

        [Display(Name = "Account Type")]
        [DisplayFormat(NullDisplayText = "Not Defined")]
        public string AccountType { get; set; }


        [Display(Name = "Default Carrier")]
        public string CarrierId { get; set; }

        [Display(Name = "Default Description")]
        [DisplayFormat(NullDisplayText = "None")]
        public string CarrierDescription { get; set; }

        [Display(Name = "Pick Mode")]
        public string DefaultPickMode { get; set; }

        [Display(Name = "Min Pieces Per Box")]
        [DisplayFormat(NullDisplayText = "1")]
        public int? MinPiecesPerBox { get; set; }


        [Display(Name = "Max Pieces Per Box")]
        [DisplayFormat(NullDisplayText = "Unlimited")]
        public int? MaxPiecesPerBox { get; set; }

        public string DisplayPiecesPerBox
        {
            get
            {
                //No constaints of pieces
                if (MinPiecesPerBox == null && MaxPiecesPerBox == null)
                {
                    return string.Format("No Constraint");
                }
                else
                {
                    //Min and max pieces are equal
                    if (MinPiecesPerBox == MaxPiecesPerBox)
                    {
                        return string.Format("{0}", MaxPiecesPerBox);
                    }
                    else
                    {
                        //only max pieces are decided
                        if (MinPiecesPerBox == null && MaxPiecesPerBox != null)
                        {
                            return string.Format("Upto {0}", MaxPiecesPerBox);
                        }
                        //only min pieces are decided
                        else if (MinPiecesPerBox != null && MaxPiecesPerBox == null)
                        {
                            return string.Format("Atleast {0}", MinPiecesPerBox);
                        }
                        else
                        {
                            //both are decided i.e range is given
                            return string.Format("{0} To {1}", MinPiecesPerBox, MaxPiecesPerBox);
                        }
                    }
                }

            }

        }








        //[Display(Name = "ASN ")]
        [Display(Name = "Send Advance Shipment Notification")]
        public bool Asn_flag { get; set; }

        [Display(Name = "Allow bulk pitching")]
        [DataType("Alert")]
        public bool AmsFlag { get; set; }


        [Display(Name = "Carton Content Label")]
        public int? NumberOfCcl { get; set; }

        public string CclShortName { get; set; }

        [Display(Name = "Master Packing Slip")]
        public int? NumberOfMps { get; set; }

        public string MpsShortName { get; set; }


        [Display(Name = "Packing Slip Per Box")]
        public int? NumberOfPspb { get; set; }

        public string PspbShortName { get; set; }


        [Display(Name = "Shiping Label")]
        public int? NumberOfShlbl { get; set; }

        public string ShlblShortName { get; set; }


        [Display(Name = "Universal Carton Content Label")]
        public int? NumberOfUcc { get; set; }

        public string UccShortName { get; set; }

        public string CustVas { get; set; }

        //[Display(Name = "Edi 753/754 Customer")]
        [Display(Name = "Automatic Routing")]
        [DataType("Alert")]
        public bool EdiFlag { get; set; }

        [Display(Name = "Scan Country of Origin")]
        [DataType("Alert")]
        public bool ScoFlag { get; set; }

        public string UrlManageCustomerPickwave { get; set; }

        public string UrlCustomerVas { get; set; }

        public string UrlRouteOrder { get; set; }

    }
}