using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using EclipseLibrary.Mvc.Helpers;
using EclipseLibrary.Mvc.ModelBinding;

namespace DcmsMobile.REQ2.ViewModels
{
    [ModelBinder(typeof(ManageSkuViewModelBinder))]
    public class ManageSkuViewModel : IValidatableObject
    {

        #region SourceSKu Property
        private string _newStyle;
        [Required(ErrorMessage = "Required")]
        public string NewStyle
        {
            get
            {
                return _newStyle;
            }
            set
            {
                _newStyle = value != null ? value.ToUpper() : null;
            }
        }
        private string _newColor;
        [Required(ErrorMessage = "Required")]
        public string NewColor
        {
            get
            {
                return _newColor;
            }
            set
            {
                _newColor = value != null ? value.ToUpper() : null;
            }
        }

        private string _newDimension;
        [Required(ErrorMessage = "Required")]
        public string NewDimension
        {
            get
            {
               return _newDimension ;
            }
            set
            {
                _newDimension = value != null ? value.ToUpper() : null;
            }
        }

        private string _newSkuSize;
        [Required(ErrorMessage = "Required")]
        public string NewSkuSize
        {
            get
            {
                return _newSkuSize;
            }
            set
            {
                _newSkuSize = value != null ? value.ToUpper() : null;
            }
        }

        #endregion

        #region TargetSKu Property

        private string _targetStyle;
        internal string TargetStyle
        {
            get
            {
                return this.CurrentRequest.IsConversionRequest ? _targetStyle : string.Empty;
            }
            set
            {
                _targetStyle = value;
            }
        }

        private string _targetColor;
        internal string TargetColor
        {
            get
            {
                return this.CurrentRequest.IsConversionRequest ? _targetColor : string.Empty;
            }
            set
            {
                _targetColor = value;
            }
        }

        private string _targetDimension;
        internal string TargetDimension
        {
            get
            {
                return this.CurrentRequest.IsConversionRequest ? _targetDimension : string.Empty;
            }
            set
            {
                _targetDimension = value;
            }
        }

        private string _targetSkuSize;
        internal string TargetSkuSize
        {
            get
            {
                return this.CurrentRequest.IsConversionRequest ? _targetSkuSize : string.Empty;
            }
            set
            {
                _targetSkuSize = value;
            }
        }
        #endregion

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Pieces")]
        [Range(minimum: 1, maximum: int.MaxValue, ErrorMessage = "1 or more")]
        public int? NewPieces { get; set; }

        [Display(Name = "Target")]
        public RequestModel CurrentRequest { get; set; }

        /// <summary>
        /// The SKU id which was last added or updated. This will never be posted.
        /// </summary>
        [ReadOnly(true)]
        public int? SkuIdLastAdded { get; set; }

        private IList<RequestSkuModel> _requestedSkus;

        /// <summary>
        /// We never return null.
        /// </summary>
        public IList<RequestSkuModel> RequestedSkus
        {
            get
            {
                return _requestedSkus ?? new RequestSkuModel[0];
            }
            set
            {
                _requestedSkus = value;
            }
        }

        /// <summary>
        /// Rkandari
        /// Url of Report 40.16: Shows you the cartons of a particular area. You can see cartons in conversion area from this report.
        /// </summary>
        public string CartonDetailsForStoragAreaUrl
        {
            get
            {

                return System.Configuration.ConfigurationManager.AppSettings["DcmsLiveBaseUrl"] + "Reports/Category_040/R40_16.aspx";
            }
        }

        /// <summary>
        /// Url of Report 30.06: Show SKUs which are to be pulled for CON area.
        /// </summary>
        public string SkuToBePulledUrl
        {
            get
            {
                return System.Configuration.ConfigurationManager.AppSettings["DcmsLiveBaseUrl"] + "Reports/Category_030/R30_06.aspx";
            }
        }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? TotalPulledCartons
        {
            get
            {
                return RequestedSkus.Sum(p => p.PulledCartons);
            }
        }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? TotalCartons
        {
            get
            {
                return RequestedSkus.Sum(p => p.TotalCartons);
            }
        }

        public int? PercentTotalPulledCartons
        {
            get
            {
                var totalCartons = RequestedSkus.Sum(p => p.TotalCartons);
                if ((this.TotalPulledCartons ?? 0) > 0 && (totalCartons ?? 0) > 0)
                {
                    return this.TotalPulledCartons.Value * 100 / totalCartons;
                }
                return null;
            }
        }
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? DisplayTotalPieces
        {
            get
            {
                return RequestedSkus.Sum(p => p.Pieces);
            }
        }
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? DisplayTotalAssignedPieces
        {
            get
            {
                return RequestedSkus.Sum(p => p.AssignedPieces);
            }
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (this.CurrentRequest.IsConversionRequest)
            {
                // Target is optional
            }
            else
            {
                // Target must not be specified
                if (!string.IsNullOrWhiteSpace(_targetStyle) || !string.IsNullOrWhiteSpace(_targetColor) || !string.IsNullOrWhiteSpace(_targetDimension) || !string.IsNullOrWhiteSpace(_targetSkuSize))
                {
                    yield return new ValidationResult("Target SKU can only be specified for conversion requests");
                }
            }
        }
    }

    internal class ManageSkuViewModelBinder : DefaultModelBinder
    {
        /// <summary>
        /// This dictionary maps source and target SKU field names
        /// </summary>
        private static IDictionary<string, PropertyInfo> __dict;

        protected override object CreateModel(ControllerContext controllerContext, ModelBindingContext bindingContext, Type modelType)
        {
            if (__dict == null)
            {
                var props = typeof(ManageSkuViewModel).GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

                __dict = new Dictionary<string, PropertyInfo>
                    {
                        {ReflectionHelpers.NameFor((ManageSkuViewModel m) => m.NewStyle), props.Where(p => p.Name == ReflectionHelpers.NameFor((ManageSkuViewModel m) => m.TargetStyle)).Single()},
                        {ReflectionHelpers.NameFor((ManageSkuViewModel m) => m.NewColor), props.Where(p => p.Name == ReflectionHelpers.NameFor((ManageSkuViewModel m) => m.TargetColor)).Single()},
                        {ReflectionHelpers.NameFor((ManageSkuViewModel m) => m.NewDimension), props.Where(p => p.Name == ReflectionHelpers.NameFor((ManageSkuViewModel m) => m.TargetDimension)).Single()},
                        {ReflectionHelpers.NameFor((ManageSkuViewModel m) => m.NewSkuSize), props.Where(p => p.Name == ReflectionHelpers.NameFor((ManageSkuViewModel m) => m.TargetSkuSize)).Single()}
                    };
            }
            return base.CreateModel(controllerContext, bindingContext, modelType);
        }

        /// <summary>
        /// Split the Style, etc by hyphen and put the post hyphen text in corresponding target.
        /// </summary>
        /// <param name="controllerContext"></param>
        /// <param name="bindingContext"></param>
        /// <param name="propertyDescriptor"></param>
        /// <param name="value"></param>
        /// <remarks>
        /// </remarks>
        protected override void SetProperty(ControllerContext controllerContext, ModelBindingContext bindingContext, PropertyDescriptor propertyDescriptor, object value)
        {
            PropertyInfo propTarget;
            if (__dict.TryGetValue(propertyDescriptor.Name, out propTarget))
            {
                if (value != null)
                {
                    var tokens = value.ToString().Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                    propertyDescriptor.SetValue(bindingContext.Model, tokens[0].Trim().ToUpper());
                    if (tokens.Length > 1)
                    {
                        propTarget.SetValue(bindingContext.Model, tokens[1].Trim().ToUpper(), null);
                    }
                }
            }
            else
            {
                base.SetProperty(controllerContext, bindingContext, propertyDescriptor, value);
            }
        }

        /// <summary>
        /// If any component of target SKU is given, replace the null components of target SKU with the source component
        /// </summary>
        /// <param name="controllerContext"></param>
        /// <param name="bindingContext"></param>
        protected override void OnModelUpdated(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            base.OnModelUpdated(controllerContext, bindingContext);
            var model = (ManageSkuViewModel)bindingContext.Model;
            if (!string.IsNullOrWhiteSpace(model.TargetStyle) || !string.IsNullOrWhiteSpace(model.TargetColor) ||
                !string.IsNullOrWhiteSpace(model.TargetDimension) || !string.IsNullOrWhiteSpace(model.TargetSkuSize))
            {
                // At least one component of target SKU is null. Make all components non null.
                if (string.IsNullOrWhiteSpace(model.TargetStyle))
                {
                    model.TargetStyle = model.NewStyle;
                }
                if (string.IsNullOrWhiteSpace(model.TargetColor))
                {
                    model.TargetColor = model.NewColor;
                }
                if (string.IsNullOrWhiteSpace(model.TargetDimension))
                {
                    model.TargetDimension = model.NewDimension;
                }
                if (string.IsNullOrWhiteSpace(model.TargetSkuSize))
                {
                    model.TargetSkuSize = model.NewSkuSize;
                }
            }
        }
    }
}
//$Id$
