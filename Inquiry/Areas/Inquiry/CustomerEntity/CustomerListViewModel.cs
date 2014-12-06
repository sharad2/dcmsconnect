using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.Inquiry.Areas.Inquiry.CustomerEntity
{
    public class CustomerHeadlineModel
    {

        public string CustomerId { get; set; }

        public string CustomerName { get; set; }


        public string CustomerTypeDescription { get; set; }

 
        [DisplayFormat(DataFormatString = "{0:d}")]
        public DateTime? PickslipImportDate { get; set; }

       
        public int? PoCount { get; set; }
    }

    public class CustomerListViewModel
    {
        public IList<CustomerHeadlineModel> CustomerList { get; set; }
    }
}