using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DcmsMobile.Shipping.Repository
{
    
    public class EdiPo
    {

        public string Po_Id { get; set; }

        public string Load_Id { get; set; }
   
        public DateTime? PickUp_Date { get; set;}

        /// <summary>
        /// Count of Load in PO
        /// </summary>
        public int? LoadCount { get; set; }

         /// <summary>
         /// Count of PickUpDate In PO
         /// </summary>
        public int? PickUpDateCount { get; set; }
    }
}