using System;
using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.Inquiry.Areas.Inquiry.IntransitEntity
{
    /// <summary>
    /// This class has been annotated for Excel Output
    /// </summary>
    public class IntransitShipmentModel
    {
        public IntransitShipmentModel()
        {

        }

        internal IntransitShipmentModel(IntransitShipment entity)
        {
            ExpectedCartonCount = entity.ExpectedCartonCount;
            ExpectedPieces = entity.ExpectedPieces;
            MaxReceiveDate = entity.MaxReceiveDate;
            MinReceiveDate = entity.MinReceiveDate;
            ReceivedCartonCount = entity.ReceivedCartonCount == (int?)null ? 0 : entity.ReceivedCartonCount;
            BuddyCartonCount = entity.BuddyCartonCount;
            BuddyReceivedPieces = entity.BuddyReceivedPieces;
            ReceivedPieces = entity.ReceivedPieces == (int?)null ? 0 : entity.ReceivedPieces;
            UnReceivedPieces = entity.UnReceivedPieces == (int?)null ? 0 : entity.UnReceivedPieces;
            UnReceivedCartonCount = entity.UnReceivedCartonCount == (int?)null ? 0 : entity.UnReceivedCartonCount;
            SewingPlantCode = entity.SewingPlantCode;
            SewingPlantName = entity.SewingPlantName;
            ShipmentDate = entity.ShipmentDate;
            ShipmentCloseDate = entity.MaxUploadDate;
            ShipmentId = entity.ShipmentId;
            MinBuddyShipmentId = entity.MinBuddyShipmentId;
            MaxBuddyShipmentId = entity.MaxBuddyShipmentId;
            CountBuddyShipmentId = entity.CountBuddyShipmentId ?? 0;
            IsShipmentClose = entity.IsShipmentClosed;
            MaxOtherShipmentId = entity.MaxOtherShipmentId;
            MinOtherShipmentId = entity.MinOtherShipmentId;
            CountOtherShipmentId = entity.CountOtherShipmentId;
            CountOtherReceivedCarton = entity.CountOtherReceivedCarton;
            CountOtherReceivedPieces = entity.CountOtherReceivedPieces;
        }

        [DisplayFormat(NullDisplayText = "None")]
        [Display(ShortName = "Shipment", Order = 1)]
        public string ShipmentId { get; set; }

        /// <summary>
        /// TODO : 
        /// </summary>
        [DisplayFormat(NullDisplayText = "None")]
        [Display(ShortName = "Buddy Shipment 1")]
        [ScaffoldColumn(false)]
        public string MinBuddyShipmentId { get; set; }

        /// <summary>
        /// TODO : 
        /// </summary>
        [DisplayFormat(NullDisplayText = "None")]
        [Display(ShortName = "Buddy Shipment 2")]
        [ScaffoldColumn(false)]
        public string MaxBuddyShipmentId { get; set; }

        /// <summary>
        /// TODO:
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:N0}")]
        [ScaffoldColumn(false)]
        public int CountBuddyShipmentId { get; set; }

        [Display(ShortName = "Sewing Plant")]
        [ScaffoldColumn(false)]
        public string SewingPlantCode { get; set; }

        [Display(ShortName = "Sewing Plant")]
        [ScaffoldColumn(false)]
        public string SewingPlantName { get; set; }

        /// <summary>
        /// This shows up in Excel.
        /// </summary>
        [Display(ShortName = "Sewing Plant", Order = 80)]
        public string SewingPlant
        {
            get
            {
                return string.Format("{0} {1}", this.SewingPlantCode, this.SewingPlantName);
            }
        }

        [DataType(DataType.Date)]
        [Display(ShortName = "Shipment Date", Order = 10)]
        //[ScaffoldColumn(false)]
        public DateTimeOffset? ShipmentDate { get; set; }

        [ScaffoldColumn(false)]
        public DateTime? MinReceiveDate { get; set; }

        /// <summary>
        /// This shows up in excel as Received Date.
        /// </summary>
        [Display(ShortName = "Received Date", Order = 90)]
       
        public DateTime? MaxReceiveDate { get; set; }

        /// <summary>
        /// Display Received Date
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:g}",NullDisplayText="None")]
        [ScaffoldColumn(false)]
        public string DisplayReceivedDate
        {
            get
            {
                //Checking if date-time are not equal. 
                if (this.MinReceiveDate.ToString() != this.MaxReceiveDate.ToString())
                {
                    //Case:1 Date is same but time is diffrent
                    if (MinReceiveDate.Value.Hour == MaxReceiveDate.Value.Hour)
                    {
                        return string.Format("{0} to {1:T}", this.MinReceiveDate, this.MaxReceiveDate);
                    }
                    //Case:2 Date and time both are diffrent
                    return string.Format("{0} to {1}", this.MinReceiveDate, this.MaxReceiveDate);
                }
                //Date and time both are same
                return this.MinReceiveDate.ToString();
            }
        }



        [DisplayFormat(DataFormatString = "{0:N0}")]
        [Display(ShortName = "Ctn Expected", Order = 20)]
        public int? ExpectedCartonCount { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        [Display(ShortName = "Pcs Expected", Order = 50)]
        public int? ExpectedPieces { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        [Display(ShortName = "Pcs Received", Order = 60)]
        public int? ReceivedPieces { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        [Display(ShortName = "Ctn Received", Order = 30)]
        public int? ReceivedCartonCount { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        [ScaffoldColumn(false)]
        [Display(ShortName = "Ctn Not Received", Order = 4)]
        public int? UnReceivedCartonCount { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        [Display(ShortName = "Pieces Not Received", Order = 8)]
        [ScaffoldColumn(false)]
        public int? UnReceivedPieces { get; set; }

        /// <summary>
        /// Number of cartons received on behalf of other shipments
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:N0}")]
        [Display(ShortName = "Ctn received in excess", Order = 5)]
        [ScaffoldColumn(false)]
        public int? BuddyCartonCount { get; set; }

        /// <summary>
        /// Number of pieces received on behalf of other shipments
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:N0}")]
        [Display(ShortName = "Pieces received in excess", Order = 9)]
        [ScaffoldColumn(false)]
        public int? BuddyReceivedPieces { get; set; }


        /// <summary>
        ///  BuddyCartons + My cartons + Unrreceived . 
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:N0}")]
        [Display(ShortName = "Ctn Variance", Order = 40)]
        public int? TotalCartonVariance
        {
            get
            {
                return (this.ReceivedCartonCount ?? 0) - (this.ExpectedCartonCount ?? 0);

            }
        }


        /// <summary>
        ///   Buddy Pcs + My pcs  received by some other shipment. 
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:N0}")]
        [Display(ShortName = "Pcs Variance", Order = 70)]

        public int? TotalPiecesVariance
        {
            get
            {
                return (this.ReceivedPieces ?? 0) - (this.ExpectedPieces ?? 0);
            }
        }

        [ScaffoldColumn(false)]
        public bool IsShipmentClose { get; set; }

        //[ScaffoldColumn(false)]
        [Display(ShortName = "Sent to ERP", Order = 15)]
        [DisplayFormat(DataFormatString = "{0:d}", NullDisplayText = "None")]
        public DateTime? ShipmentCloseDate { get; set; }

         [ScaffoldColumn(false)]
        public string MinOtherShipmentId { get; set; }

         [ScaffoldColumn(false)]
        public string MaxOtherShipmentId { get; set; }

         [ScaffoldColumn(false)]
        public int? CountOtherShipmentId { get; set; }

         [ScaffoldColumn(false)]
        public int? CountOtherReceivedCarton { get; set; }

         [ScaffoldColumn(false)]
        public int? CountOtherReceivedPieces { get; set; }

        

        [Display(ShortName = "Variance Commentary")]
        [DataType(DataType.MultilineText)]
        public string VarianceCommentsExcel
        {
            get
            {
                string x = string.Empty;
                if (this.CountBuddyShipmentId > 0)
                {
                    switch (this.CountBuddyShipmentId)
                    {
                       

                        case 1:
                            x = string.Format(" Received cartons include {0:N0} cartons of Shipment {1}.", this.BuddyCartonCount, this.MaxBuddyShipmentId);
                            break;
                        case 2:
                            x = string.Format(" Received cartons include {0:N0} cartons of Shipments {1} and {2}.",
                                this.BuddyCartonCount, this.MaxBuddyShipmentId, this.MinBuddyShipmentId);
                            break;

                        default:
                            x = string.Format(" Received cartons include {0:N0} cartons of Shipments {1}, {2} and {3} others.",
                                this.BuddyCartonCount, this.MaxBuddyShipmentId, this.MinBuddyShipmentId, this.BuddyCartonCount - 2);
                            break;
                    }
                   
                }
                if (this.CountOtherShipmentId > 0)
                {
                    switch (this.CountOtherShipmentId)
                    {
                        

                        case 1:
                           x = x + string.Format(" {0:N0} cartons were received after closing against Shipment {1}.", this.CountOtherReceivedCarton, this.MaxOtherShipmentId);
                            break;
                        case 2:
                            x = x + string.Format(" {0:N0} cartons were received after closing against Shipments {1} and {2}.",
                                this.CountOtherReceivedCarton, this.MaxOtherShipmentId, this.MinOtherShipmentId);
                            break;

                        default:
                            x = x + string.Format(" {0:N0} cartons were received after closing against Shipments {1}, {2} and {3} others.",
                                this.CountOtherReceivedCarton, this.MaxOtherShipmentId, this.MinOtherShipmentId, this.CountOtherReceivedCarton - 2);
                            break;
                    }
                }
                return x;

            }
        }
    }
}