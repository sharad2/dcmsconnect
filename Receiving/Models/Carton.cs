using System;
using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.Receiving.Models
{

    public class CartonArea
    {
        //Building to display
        public string BuildingId { get; set; }
        public string AreaId { get; set; }
        public string ShortName { get; set; }
        public string Description { get; set; }
        public bool IsNumberedArea { get; set; }
        public bool IsReceivingArea { get; set; }
        public bool IsSpotCheckArea { get; set; }
    }

    /// <summary>
    /// Encapsulates information about a carton. Derived classes exist for Intransit and Received cartons
    /// </summary>
    public abstract class CartonBase
    {
        [Key]
        [Required]
        [Display(ShortName = "Carton", Name = "Carton")]
        public string CartonId { get; set; }

        public string UpcCode { get; set; }

        public Sku Sku { get; set; }

        [Display(ShortName = "Vwh")]
        public string VwhId { get; set; }

        [Display(ShortName = "Area")]
        public string DestinationArea
        {
            get;
            set;
        }

        [DisplayFormat(DataFormatString = "{0:dd-MMM hh:mm:ss tt}")]
        [Display(Name = "Receive Date")]
        public DateTime? ReceivedDate { get; set; }


        /// <summary>
        /// Contract :
        /// We use the format for disposition C15REC i.e first part is VWh_id and second part is Destination Area.
        /// </summary>
        public string DispositionId
        {
            get
            {
                var str = string.Format("{0}{1}",
                     this.VwhId, this.DestinationArea
                    );
                return str;
            }
        }
    }

    public class ReceivedCarton : CartonBase
    {
        public string PalletId { get; set; }
    }

    public class IntransitCarton : CartonBase
    {
        // SpotCheck is enabled ot not
        public bool IsSpotCheckEnabled {get;set;}

        public decimal? SpotCheckPercent { get; set; }


        // Tells whether the shipment of the carton has been closed. 
        public bool  IsShipmentClosed { get; set; }
    }
}



//$Id$