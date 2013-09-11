using System;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc;
using DcmsMobile.Shipping.Repository;

namespace DcmsMobile.Shipping.ViewModels
{
    public class CustomerSummaryModel
    {
        public CustomerSummaryModel()
        {

        }

        public CustomerSummaryModel(CustomerOrderSummary entity)
        {
            CustomerId = entity.CustomerId;
            CustomerName = entity.CustomerName;
            CountOpenPos = entity.CustomerPosCount == null || entity.CustomerPosCount == 0 ? (int?)null : entity.CustomerPosCount;
            CountPosInBol = entity.CountPosInBol == null || entity.CountPosInBol == 0 ? (int?)null : entity.CountPosInBol;
            CountRoutedPo = entity.CountRoutedPo == null || entity.CountRoutedPo == 0 ? (int?)null : entity.CountRoutedPo;
            CountRoutingInProgressPo = entity.CountRoutingPo == null || entity.CountRoutingPo == 0 ? (int?)null : entity.CountRoutingPo;
            CountUnroutedpo = entity.CountUnroutedPo == null || entity.CountUnroutedPo == 0 ? (int?)null : entity.CountUnroutedPo;
            OrderedPieces = entity.PiecesOrdered;
            MaxDcCancelDate = entity.MaxDcCancelDate;
            StartDate = entity.StartDate;
            TotalDollarsOrdered = entity.TotalDollarsOrdered;
            EdiCustomer = entity.EdiCustomer;
            TotalUnshippedBols = entity.TotalUnshippedBols;
        }

        [Key]
        public string CustomerId { get; set; }

        public string CustomerName { get; set; }

        public string CustomerDisplay
        {
            get
            {
                if (string.IsNullOrWhiteSpace(this.CustomerId))
                {
                    return null;
                }
                return string.Format("{0}: {1}", this.CustomerId, this.CustomerName);
            }
        }

        /// <summary>
        /// Number of open POs of this customer
        /// </summary>
        public int? CountOpenPos { get; set; }


        /// <summary>
        /// Number of POs in BOL
        /// </summary>
        public int? CountPosInBol { get; set; }

        /// <summary>
        /// Customer for which EDI is sent and received electronically.
        /// </summary>
        public string EdiCustomer { get; set; }

        /// <summary>
        /// Number of POs for which BOL has been created
        /// </summary>
        public string CountPosInBolDisplay
        {

            get
            {
                return string.Format("{0:N0}", this.CountPosInBol);
            }
        }

        public int PercentPosInBol
        {
            get
            {
                if (this.CountOpenPos == null || this.CountOpenPos.Value == 0 || this.CountPosInBol == null || this.CountPosInBol.Value == 0)
                {
                    return 0;
                }
                return Math.Max((int)Math.Round((decimal)this.CountPosInBol.Value * 100 / (decimal)this.CountOpenPos), 10);
            }
        }

        /// <summary>
        /// Total number of unshipped BOLs
        /// </summary>
        public int? TotalUnshippedBols { get; set; }

        /// <summary>
        /// Number of POs having Load or PickUpdate
        /// </summary>
        public int? CountRoutedPo { get; set; }

        public string CountRoutedPoDisplay
        {
            get
            {
                return string.Format("{0:N0}", this.CountRoutedPo);
            }
        }

        public int PercentRoutedPo
        {
            get
            {
                if (this.CountOpenPos == null || this.CountOpenPos.Value == 0 || this.CountRoutedPo == null || this.CountRoutedPo.Value == 0)
                {
                    return 0;
                }
                return Math.Max((int)Math.Round((decimal)this.CountRoutedPo.Value * 100 / (decimal)this.CountOpenPos), 10);
            }
        }

        /// <summary>
        /// Number of POs having ATS date
        /// </summary>
        public int? CountRoutingInProgressPo { get; set; }

        public string CountRoutingInProgressPoDisplay
        {
            get
            {
                return string.Format("{0:N0}", this.CountRoutingInProgressPo);
            }
        }

        public int PercentRoutingInProgressPo
        {
            get
            {
                if (this.CountOpenPos == null || this.CountOpenPos.Value == 0 || this.CountRoutingInProgressPo == null || this.CountRoutingInProgressPo.Value == 0)
                {
                    return 0;
                }
                return Math.Max((int)Math.Round((decimal)this.CountRoutingInProgressPo.Value * 100 / (decimal)this.CountOpenPos), 10);
            }
        }
        /// <summary>
        /// Number of Unrouted POs 
        /// </summary>
        public int? CountUnroutedpo { get; set; }

        public string CountUnroutedpoDisplay
        {
            get
            {
                return string.Format("{0:N0}", this.CountUnroutedpo);
            }
        }

        public int PercentUnroutedPo
        {
            get
            {
                if (this.CountOpenPos == null || this.CountOpenPos.Value == 0 || this.CountUnroutedpo == null || this.CountUnroutedpo.Value == 0)
                {
                    return 0;
                }
                return Math.Max((int)Math.Round((decimal)this.CountUnroutedpo.Value * 100 / (decimal)this.CountOpenPos), 10);
            }
        }

        internal int? OrderedPieces { get; set; }

        public string OrderedPiecesDisplay
        {
            get
            {
                return string.Format("{0:N0}", this.OrderedPieces);
            }
        }

        /// <summary>
        /// Shows number of days for future dates. Actual date for past dates highlighted as error.
        /// </summary>
        public IHtmlString DcCancelDateDisplay
        {
            get
            {
                if (this.MaxDcCancelDate == null)
                {
                    return MvcHtmlString.Empty;
                }
                string str;
                var span = this.MaxDcCancelDate.Value - DateTime.Today;
                if (span.TotalDays < 0)
                {
                    str = string.Format("<span class='ui-state-error' title='The cancel date was {1} days ago and {2}'>{0:d}</span>", this.MaxDcCancelDate, Math.Abs(Math.Round(span.TotalDays)), this.StartDateDisplay);
                }
                else if (span.Days == 0)
                {
                    str = string.Format("<span class='ui-state-highlight' title='{0:D}'>Today</span>", this.MaxDcCancelDate);
                }
                else
                {
                    // Normal case
                    str = string.Format("<span title='{1:D}'>{0} days</span>", span.Days, this.MaxDcCancelDate);
                }
                return MvcHtmlString.Create(str);
            }
        }

        internal DateTime? StartDate { get; set; }

        /// <summary>
        /// Shows number of days for future dates. Actual date for past dates highlighted as error.
        /// </summary>
        public IHtmlString StartDateDisplay
        {
            get
            {
                if (this.StartDate == null)
                {
                    return MvcHtmlString.Empty;
                }
                string str;
                var span = this.StartDate.Value - DateTime.Today;
                if (span.TotalDays < 0)
                {
                    str = string.Format("start date was {1} days ago (start date was:{0:d})", this.StartDate, Math.Abs(Math.Round(span.TotalDays)));
                }
                else if (span.Days == 0)
                {
                    str = string.Format("{0:D}'>Today", this.StartDate);
                }
                else
                {
                    // Normal case
                    str = string.Format("{1:D}>{0} days", span.Days, this.StartDate);
                }
                return MvcHtmlString.Create(str);
            }
        }

        internal DateTime? MaxDcCancelDate { get; set; }

        internal decimal? TotalDollarsOrdered { get; set; }

        public string TotalDollarsOrderedDisplay
        {
            get
            {
                return string.Format("${0:N0}", this.TotalDollarsOrdered);
            }
        }
    }

}