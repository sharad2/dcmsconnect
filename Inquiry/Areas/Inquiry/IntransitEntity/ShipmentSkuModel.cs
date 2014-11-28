using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.Inquiry.Areas.Inquiry.IntransitEntity
{
    public class ShipmentSkuModel
    {
        [Required(ErrorMessage = "This SKU does not have a style")]
        [DisplayFormat(NullDisplayText = "No Style")]
        [Display(Name = "Style", Order = 1)]
        [DataType("Alert")]
        public string Style { get; set; }

        [Required(ErrorMessage = "This SKU does not have a color")]
        [DisplayFormat(NullDisplayText = "No Color")]
        [Display(Name = "Color", Order = 2)]
        [DataType("Alert")]
        public string Color { get; set; }

        [Required(ErrorMessage = "This SKU does not have a dimension")]
        [DisplayFormat(NullDisplayText = "No Dimension")]
        [Display(Name = "Dim", ShortName = "Dimension", Order = 3)]
        [DataType("Alert")]
        public string Dimension { get; set; }

        [Required(ErrorMessage = "This SKU does not have a size")]
        [DisplayFormat(NullDisplayText = "No Size")]
        [Display(Name = "Size", Order = 4)]
        [DataType("Alert")]
        public string SkuSize { get; set; }

        [ScaffoldColumn(false)]
        public string DisplaySku
        {
            get
            {
                return string.Format("{0} {1} {2} {3}", this.Style, this.Color, this.Dimension, this.SkuSize);
            }
        }

        [Display(Name = "VWH")]
        [ScaffoldColumn(false)]
        public string VwhId { get; set; }


        [Display(Name = "Pieces", ShortName = "Pcs Received", Order = 10)]
        [Range(1, int.MaxValue, ErrorMessage = "SKU Pieces are negative or 0")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        [DataType("Alert")]
        public int? ReceivedPieces { get; set; }

        [Display(Name = "Pcs Expected ", Order = 9)]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? ExpectedPieces { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        [Display(ShortName = "Pieces Underage", Order = 11)]
        public int? UnderReceviedPieces { get; set; }

        [Display(Name = "Pieces", ShortName = "Pieces Overage", Order = 12)]
        [Range(1, int.MaxValue, ErrorMessage = "SKU Pieces are negative or 0")]
        [DisplayFormat(DataFormatString = "{0:N0}")]        
        public int? OverReceviedPieces
        {
            get;
            set;
        }

        public int? PcsReceivedInOtherShipment { get; set; }

        [Display(Name = "Received Carton Count", ShortName = "Ctns Received", Order = 6)]
        [DataType("Alert")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? ReceivedCartonCount { get; set; }

        [Display(Name = "Carton Count", ShortName = "Ctns Expected", Order = 5)]
        [DataType("Alert")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? ExpectedCartonCount { get; set; }       

        //[ScaffoldColumn(false)]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        [Display(ShortName = "Ctns Underage", Order = 7)]
        public int? UnderReceviedCartonCount { get; set; }

        //[ScaffoldColumn(false)]
        [Display(ShortName = "Ctns Overage", Order = 8)]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? OverReceviedCartonCount { get; set; }

        public int? CtnsReceivedInOtherShipment { get; set; }

        [ScaffoldColumn(false)]
        public string MinMergedToBuddyShipment { get; set; }

        [ScaffoldColumn(false)]
        public string MaxMergedToBuddyShipment { get; set; }

        [ScaffoldColumn(false)]
        public int CountMergedToBuddyShipment { get; set; }

        [ScaffoldColumn(false)]
        public string MinMergedInBuddyShipment { get; set; }

        [ScaffoldColumn(false)]
        public string MaxMergedInBuddyShipment { get; set; }

        [ScaffoldColumn(false)]
        public int CountMergedInBuddyShipment { get; set; }

        public string Remark
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(this.UnderReceivedCartonMessge) && !string.IsNullOrWhiteSpace(this.OverReceivedCartonMessge))
                {
                    return this.UnderReceivedCartonMessge + " and " + this.OverReceivedCartonMessge;
                }
                else if (!string.IsNullOrWhiteSpace(this.UnderReceivedCartonMessge))
                {
                    return this.UnderReceivedCartonMessge;
                }
                else if (!string.IsNullOrWhiteSpace(this.OverReceivedCartonMessge))
                {
                    return this.OverReceivedCartonMessge;
                }
                else
                {
                    return null;
                }
            }
        }

        [ScaffoldColumn(false)]
        public string UnderReceivedCartonMessge
        {
            get
            {
                if (this.UnderReceviedCartonCount > 0)
                {
                    if(this.CountMergedToBuddyShipment > 2)
                    {
                        return string.Format("{0} piece(s) of SKU is recevied in shipment {1}, {2} and {3}", this.PcsReceivedInOtherShipment, this.MinMergedToBuddyShipment, this.MaxMergedToBuddyShipment, this.CountMergedToBuddyShipment - 2);
                    }
                    else if(this.CountMergedToBuddyShipment > 1)
                    {
                        return string.Format("{0} piece(s) of SKU is recevied in shipment {1} and {2}", this.PcsReceivedInOtherShipment, this.MinMergedToBuddyShipment, this.MaxMergedToBuddyShipment);
                    }
                    else if (this.CountMergedToBuddyShipment == 1)
                    {
                        return string.Format("{0} piece(s) of SKU is recevied in shipment {1}", this.PcsReceivedInOtherShipment, this.MinMergedToBuddyShipment);
                    }
                }
                return null;
                //return UnderReceviedCartonCount > 0 ? string.Format("{0} Carton(s) of SKU is recevied in another shipment", this.UnderReceviedCartonCount) : "";
            }
        }

        [ScaffoldColumn(false)]
        public string OverReceivedCartonMessge
        {
            get
            {
                if (this.OverReceviedCartonCount > 0)
                {
                    if (this.CountMergedInBuddyShipment > 2)
                    {
                        return string.Format("{0} piece(s) of SKU belongs to shipment {1}, {2} and {3}", this.OverReceviedPieces, this.MinMergedInBuddyShipment, this.MaxMergedInBuddyShipment, this.CountMergedInBuddyShipment - 2);
                    }
                    else if (this.CountMergedInBuddyShipment > 1)
                    {
                        return string.Format("{0} piece(s) of SKU belongs to shipment {1} and {2}", this.OverReceviedPieces, this.MinMergedInBuddyShipment, this.MaxMergedInBuddyShipment);
                    }
                    else if (this.CountMergedInBuddyShipment == 1)
                    {
                        return string.Format("{0} piece(s) of SKU belongs to another shipment {1}", this.OverReceviedPieces, this.MinMergedInBuddyShipment);
                    }
                }
                return null;
                //return OverReceviedCartonCount > 0 ? string.Format("{0} Carton(s) of SKU belongs to another shipment", this.OverReceviedCartonCount) : "";
            }
        }
    }
}