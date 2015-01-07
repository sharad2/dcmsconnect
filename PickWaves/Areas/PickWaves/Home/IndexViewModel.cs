using DcmsMobile.PickWaves.Helpers;
using DcmsMobile.PickWaves.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DcmsMobile.PickWaves.Areas.PickWaves.Home
{
    public class CustomerListModel
    {

        public string CustomerId { get; set; }

        public string CustomerName { get; set; }

        /// <summary>
        /// Is this an active customer
        /// </summary>
        public bool IsCustomerActive { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int PickslipCount { get; set; }

        [DataType(DataType.Text)]
        public DateRange ImportDateRange { get; set; }

        public bool InternationalFlag { get; set; }

    }

    public class RecentBucketModel
    {
        public int BucketId { get; set; }

        [DisplayFormat(DataFormatString = "{0:d}")]
        public DateTime CreationDate { get; set; }

        public string CreatedBy { get; set; }

    }

    public class IndexViewModel : ViewModelBase
    {
        public IList<CustomerListModel> ImportedOrders { get; set; }

        public IList<RecentBucketModel> RecentBuckets { get; set; }
    }
}