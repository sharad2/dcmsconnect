using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace DcmsMobile.Receiving.ViewModels.Home
{
    public class ReceivingViewModel : ReceivingProcessModel
    {
        public const string SCAN_NEWPALLET = "PNEW";

        public ReceivingViewModel()
        {
            this.Pallets = new PalletViewModel[] { };
            this.ScanModel = new ScanViewModel();

            // To prevent the Required Error
            this.ProDate = DateTime.MinValue;
            this.ProNumber = string.Empty;
            this.ExpectedCartons = 1;
        }

        [Required(AllowEmptyStrings = true)]
        public override string ProNumber
        {
            get
            {
                return base.ProNumber;
            }
            set
            {
                base.ProNumber = value;
            }
        }

        [Required]
        public override int? ProcessId
        {
            get
            {
                return this.ScanModel.ProcessId;
            }
            set
            {
                this.ScanModel.ProcessId = value;
            }
        }

        public int? cartonsOnPallet { get; set; }

        public IList<PalletViewModel> Pallets { get; set; }

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
        #region Printing 
        [Display(Name = "Printer")]
        public string PrinterId { get; set; }

        public IEnumerable<SelectListItem>PrinterList { get; set; }
        #endregion
        /// <summary>
        /// Becomes true if the logged in user does not match the user who created the process
        /// </summary>
        /// <remarks>
        /// It never becomes true if the user is not logged in
        /// </remarks>
        public bool UserMismatch { get; set; }

        public ScanViewModel ScanModel { get; set; }

        [Required(AllowEmptyStrings = true)]
        public override string CarrierId
        {
            get
            {
                return base.CarrierId;
            }
            set
            {
                base.CarrierId = value;
            }
        }

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

        public int percentReceivedCartons
        {
            get
            { 
            return this.CartonCount * 100 / (this.ExpectedCartons.HasValue && this.ExpectedCartons.Value > 0 ? this.ExpectedCartons.Value : 1);

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

        public string PalletId { get; set; }

        public string PalletDispos { get; set; }
    }
}



//$Id$