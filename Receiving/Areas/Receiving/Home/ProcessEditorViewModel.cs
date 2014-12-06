
using EclipseLibrary.Mvc.Html;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace DcmsMobile.Receiving.Areas.Receiving.Home
{
    public class ProcessEditorViewModel
    {
        public ProcessEditorViewModel()
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
        public string CarrierId { get; set; }

        public string CarrierDisplayName { get; set; }


        [Required(ErrorMessage = "Pro Date is required")]
        [Display(Name = "Pro Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:d}", ApplyFormatInEditMode = true)]
        public DateTime? ProDate { get; set; }

        /// <summary>
        /// The time when last carton was received.
        /// </summary>
        [Display(Name = "Receiving Stopped")]
        [DisplayFormat(NullDisplayText = "Not Received")]
        [Obsolete]
        public DateTime? ReceivingEndDate { get; set; }

        [Display(Name = "Name_Process", ResourceType = typeof(Resources.Receiving))]
        public virtual int? ProcessId { get; set; }

        /// <summary>
        /// The number of pallets created by this process
        /// </summary>
        [Display(Name = "Name_PalletCount", ResourceType = typeof(Resources.Receiving))]
        public int PalletCount { get; set; }

        /// <summary>
        /// The expected number of cartons to be received in this process.
        /// </summary>
        [Required(ErrorMessageResourceType = typeof(Resources.Receiving), ErrorMessageResourceName = "RequiredErrorMessage")]
        [Display(Name = "Name_ExpectedCartons", ResourceType = typeof(Resources.Receiving))]
        [Range(1, 99999, ErrorMessageResourceType = typeof(Resources.Receiving), ErrorMessageResourceName = "RangeMinMaxErrorMessage")]
        [DisplayFormat(DataFormatString="{0:N0}", NullDisplayText="??")]
        public int? ExpectedCartons { get; set; }

        /// <summary>
        /// The  number of cartons that can be received in a pallet in current process.
        /// </summary>
        [Display(Name = "Max Cartons Per Pallet")]
        public int? PalletLimit { get; set; }

        [Required]
        [Display(Name = "Spot Check Area")]
        public string SpotCheckAreaId { get; set; }

        public IEnumerable<GroupSelectListItem> SpotCheckAreasList { get; set; }


        [Display(Name = "Receive in")]
        [Required]
        public string ReceivingAreaId { get; set; }

        public IEnumerable<GroupSelectListItem> ReceivingAreasList { get; set; }

        [Display(Name = "Season Code")]
        public string PriceSeasonCode { get; set; }

        public IEnumerable<SelectListItem> PriceSeasonCodeList { get; set; }
    }
}



//$Id$