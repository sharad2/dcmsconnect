using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using DcmsMobile.Receiving.ViewModels;


namespace DcmsMobile.Receiving.Areas.Receiving.Home
{
    public class ReceivingViewModel
    {
        public const string SCAN_NEWPALLET = "PNEW";

        public ReceivingViewModel()
        {
        }

        public int? PalletLimit { get; set; }

        [Display(Name = "Season Code")]
        public string PriceSeasonCode { get; set; }

        [Display(Name = "Pro Date")]
        [DisplayFormat(DataFormatString = "{0:d}")]
        public DateTime? ProDate { get; set; }

        [Display(Name = "Pro #")]
        public string ProNumber { get; set; }

        [Display(Name = "Process")]
        public int ProcessId { get; set; }

        public string SpotCheckAreaId { get; set; }

        [Display(Name = "Received by")]
        [Obsolete]
        public string OperatorName { get; set; }

        public string ReceivingAreaId { get; set; }

        /// <summary>
        /// Array of pallet ids in javascript format
        /// </summary>
        public string PalletIdListJson { get; set; }

        //This property keep the list of all NonPalletizeCartons.
        private IEnumerable<CartonNotOnPalletModel> _nonPalletizeCarton;
        public IEnumerable<CartonNotOnPalletModel> NonPalletizeCartonList
        {
            get { return _nonPalletizeCarton ?? (_nonPalletizeCarton = Enumerable.Empty<CartonNotOnPalletModel>().ToList()); }
            set
            {
                _nonPalletizeCarton = value;
            }
        }

        public string CarrierId { get; set; }

        /// <summary>
        /// Url of Report 40.103: Summary of the Shipments as well as cartons received.
        /// </summary>
        public string ProcessDetailUrl
        {
            get
            {

                return System.Configuration.ConfigurationManager.AppSettings["DcmsLiveBaseUrl"] + "Reports/Category_040/R40_103.aspx";
            }
        }

        [Display(Name = "Expected Cartons")]
        public int? ExpectedCartons { get; set; }

        public int CartonCount { get; set; }

        public int PercentReceivedCartons
        {
            get
            {
                if ((this.ExpectedCartons ?? 0) <= 0)
                {
                    return 0;
                }
                return (int)Math.Round(this.CartonCount * 100.0 / this.ExpectedCartons.Value);

            }
        }

    }

    /// <summary>
    /// This information is posted back at each scan
    /// </summary>
    public class ScanViewModel
    {
        [Required(ErrorMessage = "Process ID is required")]
        public int? ProcessId { get; set; }

        [Required(ErrorMessage = "Must scan something")]
        [Display(Name = "Scan Carton or Pallet")]
        public string ScanText { get; set; }

        /// <summary>
        /// Currently active pallet
        /// </summary>
        public string PalletId { get; set; }

        /// <summary>
        /// Disposition of the currently active pallet
        /// </summary>
        public string PalletDispos { get; set; }
    }
}



//$Id$