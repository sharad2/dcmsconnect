using System;
using System.ComponentModel.DataAnnotations;
using DcmsMobile.Shipping.Repository;

namespace DcmsMobile.Shipping.ViewModels
{
    public class PoStatusModel
    {
        public PoStatusModel(PoStatus entity)
        {
            this.PoId = entity.PoId;
            this.CustomerId = entity.CustomerId;
            this.AtsDate = entity.AtsDate;
            this.DcCancelDate = entity.DCCancelDate;
            this.CountUnroutedPo = entity.CountUntoutedPO;
            this.CountRotingInProgressPo = entity.CountRoutingInprogressPo;
            this.CountRoutedPo = entity.CountRoutedPO;
            this.ShippingId = entity.ShippingId;
            this.BuildingId = entity.BuildingId;
            this.CustomerName = entity.CustomerName;
            this.DcId = entity.DcId;
        }

        [Key]
        public string PoId { get; set; }

        [Key]
        public string CustomerId { get; set; }

        public string CustomerName { get; set; }

        public string BuildingId { get; set; }

        public int? CountUnroutedPo { get; set; }

        public int? CountRotingInProgressPo { get; set; }

        public int? CountRoutedPo { get; set; }

        public string ShippingId { get; set; }

        public DateTime? AtsDate { get; set; }

        public DateTime? DcCancelDate { get; set; }

        public string CustomUrl { get; set; }

        public string DcId { get; set; }

        public string CustomerDisplay
        {

            get
            {
                return string.Format("{0}:{1}", this.CustomerId, this.CustomerName);
            }
        }

        public RoutingStatus Status
        {
            get
            {
                RoutingStatus flag;  // = RoutingStatus.Notset;
                if (this.CountUnroutedPo.HasValue && this.CountUnroutedPo.Value > 0)
                {
                    flag = RoutingStatus.Unrouted;
                }
                else if (this.CountRotingInProgressPo.HasValue && this.CountRotingInProgressPo.Value > 0)
                {
                    flag = RoutingStatus.Routing;
                }
                else if (this.CountRoutedPo.HasValue && this.CountRoutedPo.Value > 0)
                {
                    flag = RoutingStatus.Routed;
                }
                else if (!string.IsNullOrWhiteSpace(this.ShippingId))
                {
                    flag = RoutingStatus.InBol;
                }
                else
                {
                    throw new NotImplementedException("We should never get here");
                }
                return flag;
            }
        }
        /// <summary>
        /// Url of Report 110.21: Summary of the Shipments as well as cartons received.
        /// </summary>
        public string BolDetailUrl
        {
            get
            {
                return System.Configuration.ConfigurationManager.AppSettings["DcmsLiveBaseUrl"] + "Reports/Category_110/R110_21.aspx";
            }
        }
    }
}