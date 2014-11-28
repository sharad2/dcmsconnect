using System;
using System.ComponentModel;

namespace DcmsMobile.Inquiry.Areas.Inquiry.CustomerEntity
{

    internal class CustomerHeadline
    {
        public string CustomerId { get; set; }

        public string CustomerName { get; set; }


        public string CustomerTypeDescription { get; set; }

        public DateTime? PickslipImportDate { get; set; }

            public int? PoCount { get; set; }
    }

    internal class Customer
    {

        public string CustomerId { get; set; }

        public string CustomerName { get; set; }


        public string CustomerTypeDescription { get; set; }

        public string Category { get; set; }


        public string CarrierId { get; set; }

        public string CarrierDescription { get; set; }

        public bool Asn_flag { get; set; }

        public string DefaultPickMode { get; set; }

        public int? MinPiecesPerBox { get; set; }

        public int? MaxPiecesPerBox { get; set; }

        public bool AmsFlag { get; set; }

        public bool EdiFlag { get; set; }

        public bool ScoFlag { get; set; }

        public int? NumberOfMps { get; set; }

        public string MpsShortName { get; set; }
        
        public int? NumberOfPspb { get; set; }

        public string PspbShortName { get; set; }

        public int? NumberOfCcl { get; set; }

        public string CclShortName { get; set; }

        public int? NumberOfUcc { get; set; }

        public string UccShortName { get; set; }

        public int? NumberOfShlbl { get; set; }

        public string ShlblShortName { get; set; }

        public string CustVas { get; set; }
    }
}




//$Id$