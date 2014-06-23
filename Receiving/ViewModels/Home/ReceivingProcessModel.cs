using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Web.Mvc;
using EclipseLibrary.Mvc.Html;

namespace DcmsMobile.Receiving.ViewModels.Home
{
    public class ReceivingProcessModel : ViewModelBase
    {
        public ReceivingProcessModel()
        {
            this.ProNumber = string.Empty;
            this.CarrierId = string.Empty;
        }

        [Required(ErrorMessage = "{0} is required")]
        [Display(ShortName = "PRO#", Name = "Pro #")]
        [StringLength(25, ErrorMessageResourceType = typeof(Resources.Receiving), ErrorMessageResourceName = "MaxStringLengthErrorMessage")]
        // ReSharper disable MemberCanBeProtected.Global
        public virtual string ProNumber { get; set; }
        // ReSharper restore MemberCanBeProtected.Global

        [Required(ErrorMessage = "{0} is required")]
        [Display(Name = "Carrier")]
        [DisplayFormat(NullDisplayText = "Unknown Carrier")]
        // ReSharper disable MemberCanBeProtected.Global
        public virtual string CarrierId { get; set; }
        // ReSharper restore MemberCanBeProtected.Global

        [Required(ErrorMessage = "{0} is required")]
        [Display(Name = "Pro Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:d}", ApplyFormatInEditMode = true)]
        public DateTime? ProDate { get; set; }

        [ReadOnly(true)]
        [Display(Name = "Received by")]
        public string OperatorName { get; set; }

        [Display(Name = "Receiving Started")]
        [DisplayFormat(NullDisplayText = "Not Received")]
        public DateTime? ReceivingStartDate { get; set; }

        /// <summary>
        /// The time when last carton was received.
        /// </summary>
        [Display(Name = "Receiving Stopped")]
        [DisplayFormat(NullDisplayText = "Not Received")]
        public DateTime? ReceivingEndDate { get; set; }


        /// <summary>
        /// Elapsed time of Current receiving process
        /// </summary>
        [Display(Name = "Elapsed Time")]
        public string ElapsedTime
        {
            get
            {
                if (ReceivingEndDate != null && ReceivingStartDate != null)
                {
                    var interval = ReceivingEndDate.Value.Subtract(ReceivingStartDate.Value).Duration();
                    return string.Format("{0}:{1}:{2} Hours", (int)interval.TotalHours, interval.Minutes, interval.Seconds);
                }
                return new TimeSpan().ToString();
            }
        }

        public string CarrierDescription { get; set; }

        [Display(Name = "Carrier")]
        [DisplayFormat(NullDisplayText = "Unknown Carrier")]
        public string CarrierDisplayName
        {
            get
            {
                if (string.IsNullOrEmpty(this.CarrierId))
                {
                    return null;
                }
                return string.Format("{0}: {1}", this.CarrierId, this.CarrierDescription);
            }
        }

        [Display(Name = "Name_Process", ResourceType = typeof(Resources.Receiving))]
        public virtual int? ProcessId { get; set; }

        /// <summary>
        /// The number of pallets created by this process
        /// </summary>
        [Display(Name = "Name_PalletCount", ResourceType = typeof(Resources.Receiving))]
        public int PalletCount { get; set; }

        /// <summary>
        /// The number of carton received by this process
        /// </summary>
        [Display(Name = "Name_CartonCount", ResourceType = typeof(Resources.Receiving))]
        public int CartonCount { get; set; }
        /// <summary>
        /// The expected number of cartons to be received in this process.
        /// </summary>
        [Required(ErrorMessageResourceType = typeof(Resources.Receiving), ErrorMessageResourceName = "RequiredErrorMessage")]
        [Display(Name = "Name_ExpectedCartons", ResourceType = typeof(Resources.Receiving))]
        [Range(1, 99999, ErrorMessageResourceType = typeof(Resources.Receiving), ErrorMessageResourceName = "RangeMinMaxErrorMessage")]
        public int? ExpectedCartons { get; set; }

        /// <summary>
        /// The  number of cartons that can be received in a pallet in current process.
        /// </summary>
        [Display(Name = "Pallet Limit")]
        public int? PalletLimit { get; set; }

        [Display(Name = "Receive in")]
        public string DestinationCartonStorageArea { get; set; }

        [Required]
        [Display(Name = "Spot Check Area")]
        public string SpotCheckAreaId { get; set; }

        public IEnumerable<GroupSelectListItem> SpotCheckAreasList { get; set; }

        [Required]
        [Display(Name = "Receiving Area")]
        public string ReceivingAreaId { get; set; }
        public IEnumerable<GroupSelectListItem> ReceivingAreasList { get; set; }

        [Display(Name = "Season Code")]
        public string PriceSeasonCode { get; set; }

        public IEnumerable<SelectListItem> PriceSeasonCodeList { get; set; }
    }
}



//$Id$