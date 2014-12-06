using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DcmsMobile.Inquiry.Areas.Inquiry.ShipmentEntity
{


    public class ParentShipmentHeadlineModel
    {

        [Display(Name = "Master BOL")]
        [DisplayFormat(DataFormatString = "Master BOL:{0}")]
        public string MBolID { get; set; }

        public string ParentShippingId { get; set; }

        [Display(Name = "Shipping Date")]
        [DisplayFormat(DataFormatString = "{0:d}")]
        public DateTime? ShippingDate { get; set; }

        public string CarrierId { get; set; }

        public string CarrierName { get; set; }

        public string CustomerID { get; set; }

        public string CustomerName { get; set; }

        //[Display(Name = "Arrival Date")]
        //[DisplayFormat(NullDisplayText = "None", DataFormatString = "{0:d}")]
        //public DateTime? ArrivalDate { get; set; }

        [Display(Name = "Number of Carton")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? BoxCount { get; set; }

        public DateTime? StatusShippedDate { get; set; }

    }
    public class ParentShipmentListViewModel
    {
        public IList<ParentShipmentHeadlineModel> ParentShipmentList { get; set; }
    }
}