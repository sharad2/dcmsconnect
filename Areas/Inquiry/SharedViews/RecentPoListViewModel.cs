using DcmsMobile.Inquiry.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace DcmsMobile.Inquiry.Areas.Inquiry.SharedViews
{
    public class RecentPoModel
    {
        public RecentPoModel()
        {

        }

        internal RecentPoModel(PoHeadline entity)
        {
            //this.BoxCount = entity.BoxCount;
            this.CustomerId = entity.CustomerId;
            this.CustomerName = entity.CustomerName;
            this.DcCancelDate = entity.DcCancelDate;
            this.ImportDate = entity.ImportDate;
            this.Iteration = entity.Iteration;
            this.PiecesInBox = entity.PiecesInBox;
            this.PiecesOrdered = entity.PiecesOrdered;
            this.PO = entity.PO;
            this.StartDate = entity.StartDate;
            this.TotalPickslip = entity.TotalPickslip;
            this.TotalPO = entity.TotalPO;
            //this.VWhId = entity.VWhId;
            //this.WhId = entity.BuildingId;
        }


        [DisplayFormat(DataFormatString = "{0:N0}")]
        [ScaffoldColumn(false)]
        public int TotalPO { get; set; }

        [Key]
        [ScaffoldColumn(false)]
        public string CustomerId { get; set; }

        [ScaffoldColumn(false)]
        public string CustomerName { get; set; }

        [Key]
        [ScaffoldColumn(false)]
        public int Iteration { get; set; }

        //[Obsolete]
        //[Display(ShortName = "Vwh", Order = 2)]
        //public string VWhId { get; set; }

        [Key]
        [Display(ShortName = "PO", Order = 3)]
        public string PO { get; set; }

        [Display(ShortName = "Import Date", Order = 4)]
        [DataType(DataType.DateTime)]
        public DateTime? ImportDate { get; set; }

        [DisplayFormat(DataFormatString = "{0:d}")]
        [Display(ShortName = "Start Date", Order = 5)]
        public DateTime? StartDate { get; set; }

        [DisplayFormat(DataFormatString = "{0:d}")]
        [Display(ShortName = "Dc Cancel Date", Order = 6)]
        public DateTime? DcCancelDate { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        [Display(ShortName = "Pcs Ordered", Order = 10)]
        public int? PiecesOrdered { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}", NullDisplayText = "0")]
        [Display(ShortName = "Pcs in Box", Order = 9)]
        public int? PiecesInBox { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        [Display(ShortName = "# Pickslip", Order = 7)]
        public int TotalPickslip { get; set; }

        //        [Obsolete]
        //[Display(ShortName="Building",Order=1)]
        //public string WhId { get; set; }

        //[Obsolete]
        //[DisplayFormat(DataFormatString = "{0:N0}",NullDisplayText="0")]
        //[Display(ShortName = "# Box", Order = 8)]
        //public int? BoxCount { get; set; } 

        public double PiecesPickedPercent
        {
            get
            {
                int? pcs = 0;
                if (this.PiecesInBox.HasValue)
                {
                    pcs = PiecesInBox;
                }
                if (this.PiecesOrdered == null || pcs == null) 
                {
                    return (double)0;
                }
                return  (double)pcs / (double)this.PiecesOrdered;
            }
        }

    }

    public class RecentPoListViewModel
    {
        public IList<RecentPoModel> PoList { get; set; }


        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int TotalPoCount
        {
            get
            {
                if (PoList == null || PoList.Count == 0)
                {
                    return 0;
                }
                return this.PoList.First().TotalPO;
            }
        }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int PoCount
        {
            get
            {
                if (PoList == null)
                {
                    return 0;
                }
                return this.PoList.Count;
            }
        }

        /// <summary>
        /// This property enables the customerId to be visible in the PO List page and hides in SKU and Customer page
        /// </summary>
        public bool ShowCustomerFlag { get; set; }

    }

}