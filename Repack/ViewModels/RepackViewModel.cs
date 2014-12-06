using DcmsMobile.Repack.Helpers;
using EclipseLibrary.Mvc.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc;

namespace DcmsMobile.Repack.ViewModels
{
    [Flags]
    public enum RepackUiStyle
    {
        Storage = 0x1,
        Conversion = 0x2,
        Receive = 0x8,
        Advanced = 0x10,
        BulkAdvanced = 0x20,
        BulkConversion = 0x40,
        All = 0xFFFFFFF
    }

    /// <summary>
    /// The UiStyle attribute controls which property is visible in which view
    /// </summary>
    /// <remarks>
    /// The UiHintForUi attribute applied to the class controls the UI which is presented for UPC code and pieces
    /// </remarks>
    [UiHintForUi(RepackUiStyle.BulkAdvanced | RepackUiStyle.BulkConversion | RepackUiStyle.Receive, "PieceScan", "PieceScan")]
    public class RepackViewModel : IValidatableObject
    {
        internal const string KEY_UISTYLE = "RepackUiStyle";

        public RepackViewModel()
        {
            this.NumberOfCartons = 1;
        }

        public RepackViewModel(RepackUiStyle uiStyle)
        {
            this.NumberOfCartons = 1;
            HttpContext.Current.Items[KEY_UISTYLE] = uiStyle;
        }

        public string PageHeading
        {
            get;
            set;
        }

        /// <summary>
        /// Static access to current UI style
        /// </summary>
        public static RepackUiStyle CurrentUiStyle
        {
            get
            {
                var uiStyle = HttpContext.Current.Items[KEY_UISTYLE] as RepackUiStyle?;
                if (uiStyle == null)
                {
                    var fieldName = ReflectionHelpers.NameFor((RepackViewModel m) => m.UiStyle);
                    var value = HttpContext.Current.Request.Form[fieldName];
                    uiStyle = (RepackUiStyle)Enum.Parse(typeof(RepackUiStyle), value);
                    HttpContext.Current.Items[KEY_UISTYLE] = uiStyle;
                }
                return uiStyle.Value;
            }
        }
        /// <summary>
        /// The Current UI Style
        /// </summary>
        /// <remarks>
        /// Because we are using HttpContext.Current, this will pose some challenges during Unit testing
        /// </remarks>
        public RepackUiStyle UiStyle
        {
            get
            {
                return CurrentUiStyle;
            }
        }


        #region Inventory Areas
        /// <summary>
        /// Where will the inventory be decremented from. Always required.
        /// </summary>
        [DisplayName("Source Area")]
        [UIHint("DropDownList")]
        [DisplayFormat(NullDisplayText = "(Select Source Area)")]
        [RequiredForUi(RepackUiStyle.Advanced | RepackUiStyle.BulkAdvanced | RepackUiStyle.BulkConversion | RepackUiStyle.Conversion | RepackUiStyle.Storage, ErrorMessage = "Source Area is required")]
        [UiHintForUi(RepackUiStyle.Advanced | RepackUiStyle.BulkAdvanced | RepackUiStyle.BulkConversion | RepackUiStyle.Conversion | RepackUiStyle.Storage, "DropDownList")]
        public string SourceArea { get; set; }

        public IDictionary<string, IEnumerable<SelectListItem>> SourceArea_List { get; set; }

        /// <summary>
        /// Where will the carton be created. Always required.
        /// </summary>
        [DisplayName("Destination Area")]
        [Required(ErrorMessage = "Destination Area is required")]
        [UIHint("DropDownList")]
        [DisplayFormat(NullDisplayText = "(Select Destination Area)")]
        public string DestinationArea { get; set; }

        public IDictionary<string, IEnumerable<SelectListItem>> DestinationArea_List { get; set; }

        /// <summary>
        /// There is no conditional required validation in this model. This is a bug. We trust that the client will validate properly.
        /// </summary>
        /// <remarks>
        /// Ideally, the palet should be required, when destination area is a pallet area. This has not yet been implemented on the server since it is non trivial.
        /// </remarks>
        private string _palletId;
        [DisplayName("Pallet ID")]
        [RegularExpression(@"[pP]\S{1,20}", ErrorMessage = "Pallet ID must begin with a P and have 20 or less characters.")]
        public string PalletId
        {
            get { return _palletId; }
            set { _palletId = value != null ? value.ToUpper() : value; }
        }

        #endregion

        #region Carton Properties
        /// <summary>
        /// Price season code is never required.
        /// </summary>
        [DisplayName("Price Season Code")]
        [UIHint("DropDownList")]
        [DisplayFormat(NullDisplayText = "(None)")]
        public string PriceSeasonCode { get; set; }

        public IDictionary<string, IEnumerable<SelectListItem>> PriceSeasonCode_List { get; set; }

        /// <summary>
        /// Always required, but not always visible. When not visible, the first value is stored in hidden field.
        /// </summary>
        [DisplayName("Quality Code")]
        [Required(ErrorMessage = "Quality Code is required")]
        [UiHintForUi(RepackUiStyle.Advanced | RepackUiStyle.BulkAdvanced, "DropDownList")]
        public string QualityCode { get; set; }


        public IDictionary<string, IEnumerable<SelectListItem>> QualityCode_List { get; set; }

        /// <summary>
        /// Required only for some UI styles. For other UI styles it is not rendered and is always null.
        /// </summary>
        [DisplayName("Sewing Plant")]
        [UiHintForUi(RepackUiStyle.Advanced | RepackUiStyle.BulkAdvanced | RepackUiStyle.Receive | RepackUiStyle.Storage, "DropDownList")]
        [RequiredForUi(RepackUiStyle.Storage | RepackUiStyle.Receive, ErrorMessage = "Sewing Plant is required")]
        [DisplayFormat(NullDisplayText = "(Not set)")]
        public string SewingPlantCode { get; set; }

        public IEnumerable<SelectListItem> SewingPlantCode_List { get; set; }

        /// <summary>
        /// Always required and visible.
        /// </summary>
        [DisplayName("Virtual Warehouse")]
        [Required(ErrorMessage = "Virtual Warehouse is required")]
        [UIHint("DropDownList")]
        public string VwhId { get; set; }

        public IDictionary<string, IEnumerable<SelectListItem>> VwhId_List { get; set; }

        [DisplayName("Shipment ID")]
        [AdditionalMetadata("maxlength", 11)]
        [UiHintForUi(RepackUiStyle.Advanced | RepackUiStyle.BulkAdvanced | RepackUiStyle.Receive, "TextBox")]
        public string ShipmentId { get; set; }

        #endregion

        #region Scanning
        [Required(ErrorMessage = "SKU is required")]
        public string SkuBarCode { get; set; }

        /// <summary>
        /// Always required and visible.
        /// </summary>
        [Required(ErrorMessage = "Pieces in carton is required")]
        [DisplayName("Pieces")]
        [Range(1, 999, ErrorMessage = "Pieces in carton must be between 1 and 999")]
        [UIHint("TextBox")]
        [AdditionalMetadata("size", 4)]
        [AdditionalMetadata("maxlength", 4)]
        public int? Pieces { get; set; }

        /// <summary>
        /// Required when AllowCartonScan is true.
        /// </summary>
        [DisplayName("Carton ID")]
        [UiHintForUi(RepackUiStyle.Receive | RepackUiStyle.Advanced, "TextBox")]
        [RequiredIf("AllowCartonScan", true, ErrorMessage = "Carton ID is required")]
        public string CartonId { get; set; }

        /// <summary>
        /// Always required. Default is set as 1. May not be visible.
        /// </summary>
        [DisplayName("How many cartons?")]
        [Required(ErrorMessage = "Please enter no. of Cartons")]
        [Range(1, 999, ErrorMessage = "Pieces in carton must be between 1 and 999")]
        [UiHintForUi(RepackUiStyle.BulkAdvanced | RepackUiStyle.BulkConversion, "TextBox")]
        [AdditionalMetadata("maxlength", 3)]
        [AdditionalMetadata("size", 3)]
        public int? NumberOfCartons { get; set; }
        #endregion

        #region Behavior
        [Display(Name = "Require scanning of Carton ID. This is useful if you are receiving cartons.")]
        [UiHintForUi(RepackUiStyle.Receive | RepackUiStyle.Advanced, "Checkbox")]
        public bool AllowCartonScan { get; set; }

        [DisplayName("Print carton tickets on:")]
        [UIHint("DropDownList")]
        [DisplayFormat(NullDisplayText = "(Do not print carton tickets)")]
        //[RequiredForUi(RepackUiStyle.Storage)]
        public string PrinterName { get; set; }

        public IDictionary<string, IEnumerable<SelectListItem>> PrinterName_List { get; set; }
        #endregion

        #region Conversion
        /// <summary>
        /// Whether SKU needs to be converted
        /// </summary>
        [Display(Name = "Conversion")]
        [UiHintForUi(RepackUiStyle.Advanced | RepackUiStyle.BulkAdvanced, "CheckBox")]
        public bool ConvertSku { get; set; }

        [RequiredIf("ConvertSku", true, ErrorMessage = "TargetSKU is required for conversion")]
        [Display(Name = "Target SKU")]
        public string TargetSkuBarCode { get; set; }

        /// <summary>
        /// Required only if visible
        /// </summary>
        [DisplayName("Target VWh")]
        [UiHintForUi(RepackUiStyle.Conversion | RepackUiStyle.BulkConversion | RepackUiStyle.Advanced | RepackUiStyle.BulkAdvanced, "DropDownList")]
        [DisplayFormat(NullDisplayText = "(No change)")]
        public string TargetVwhId { get; set; }

        public IDictionary<string, IEnumerable<SelectListItem>> TargetVwhId_List { get; set; }

        [DisplayName("Target Quality Code")]
        [UiHintForUi(RepackUiStyle.BulkConversion | RepackUiStyle.Advanced | RepackUiStyle.BulkAdvanced | RepackUiStyle.Conversion, "DropDownList")]
        [DisplayFormat(NullDisplayText = "(No change)")]
        public string TargetQualityCode { get; set; }

        public IDictionary<string, IEnumerable<SelectListItem>> TargetQualityCode_List { get; set; }




        #endregion


        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (this.ConvertSku)
            {
                if (this.SkuBarCode != null && this.TargetSkuBarCode != null && this.SkuBarCode == this.TargetSkuBarCode)
                {
                    yield return new ValidationResult("Conversion SKU must be different from the Source SKU",
                        new[] { this.NameFor(m => m.TargetSkuBarCode) });
                }
            }
        }
    }
}




//$Id$