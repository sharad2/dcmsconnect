using DcmsMobile.CartonManager.Models;
using DcmsMobile.CartonManager.Repository;
using EclipseLibrary.Mvc.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace DcmsMobile.CartonManager.ViewModels
{
    /// <summary>
    /// Serves as an abstract base class for all UI View models.
    /// </summary>
    /// <typeparam name="TQualify">CartonModel derived class which encapsulates the qualification rules</typeparam>
    /// <typeparam name="TUpdate">CartonModel derived class which encapsulates update rules</typeparam>
    /// <remarks>
    /// <para>
    /// To create a derived ViewModel, specify the CartonModel derived classes which will supply the qualification and update rules.
    /// The derived class can also have additional UI related properties but note that these properties cannot be validated. Validation attributes are read in
    /// CartonModel derived classes only. This limitation exists because <see cref="UpdateCartonModel"/> is posted and validated, not the model you sent to the view.
    /// </para>
    /// </remarks>
    [ModelBinder(typeof(ViewModelBaseModelBinder))]
    public abstract class ViewModelBase : SoundModel, IValidatableObject
    {
        private readonly CartonModel _updatingRules;
        private readonly CartonModel _qualificationRules;

        private readonly IList<string> _statusMessages;

        public ViewModelBase()
        {
            this.ViewModelType = this.GetType().FullName;
            _updatingRules = new CartonModel();
            _qualificationRules = new CartonModel();
            _statusMessages = new List<string>(6);
        }


        /// <summary>
        /// List of status messages which will be displayed
        /// </summary>
        public IList<string> StatusMessages
        {
            get
            {
                return _statusMessages;
            }
        }


        private string _scanText;

        [Display(Name = "Carton/Pallet")]
        public string ScanText
        {
            get
            {
                return _scanText;
            }
            set
            {

                _scanText = value != null ? value.ToUpper() : null;
            }
        }

        public string ConfirmScanText { get; set; }

        /// <summary>
        /// If null, it is not updated
        /// </summary>
        [Display(Name = "Printer")]
        public string PrinterId { get; set; }

        /// <summary>
        /// This must always be posted by all views
        /// </summary>
        public string ViewModelType { get; set; }

        /// <summary>
        /// Called before the values in the model are validated
        /// </summary>
        /// <param name="service"></param>
        public abstract void OnModelUpdated();

        /// <summary>
        /// Called after all carton update queries have been executed. This is your chance to perform additional updates.
        /// </summary>
        /// <param name="service"></param>
        /// <remarks>
        /// </remarks>
        public virtual void OnCartonUpdated(CartonManagerService service)
        {
        }

        /// <summary>
        /// This is called just before the view is rendered using this model. The call comes only in postback scenario. This is not called for AJAX requests.
        /// Fill your drop down lists here.
        /// </summary>
        /// <param name="service"></param>
        public virtual void OnViewExecuting(CartonManagerService service, ControllerContext context)
        {
        }

        public virtual CartonModel UpdatingRules
        {
            get { return _updatingRules; }
        }

        public virtual CartonModel QualificationRules
        {
            get { return _qualificationRules; }
        }

        public CartonUpdateFlags UpdateFlags
        {
            get
            {
                var flags = CartonUpdateFlags.None;
                if (_updatingRules == null)
                {
                    return flags;
                }

                if (_updatingRules.Pieces.HasValue)
                {
                    flags |= CartonUpdateFlags.Pieces;
                }
                if (!string.IsNullOrWhiteSpace(_updatingRules.VwhId))
                {
                    flags |= CartonUpdateFlags.Vwh;
                }
                if (!string.IsNullOrWhiteSpace(_updatingRules.QualityCode))
                {
                    flags |= CartonUpdateFlags.Quality;
                }
                if (!string.IsNullOrWhiteSpace(_updatingRules.PalletId))
                {
                    flags |= CartonUpdateFlags.Pallet;
                }
                if (!string.IsNullOrWhiteSpace(_updatingRules.AreaId))
                {
                    flags |= CartonUpdateFlags.Area;
                }
                if (!string.IsNullOrWhiteSpace(_updatingRules.SkuBarCode))
                {
                    flags |= CartonUpdateFlags.Sku;
                }
                if (!string.IsNullOrWhiteSpace(_updatingRules.LocationID))
                {
                    flags |= CartonUpdateFlags.Location;
                }
                if (!string.IsNullOrWhiteSpace(_updatingRules.PriceSeasonCode))
                {
                    flags |= CartonUpdateFlags.PriceSeasonCode;
                }
                if (_updatingRules.Rework == ReworkStatus.DoesNotNeedRework)
                {
                    flags |= CartonUpdateFlags.AbandonRework;
                }
                if (_updatingRules.Rework == ReworkStatus.NeedsRework || _updatingRules.Rework == ReworkStatus.CompleteRework)
                {
                    flags |= CartonUpdateFlags.MarkReworkComplete;
                }
                if (_updatingRules.RemoveExistingPallet)
                {
                    flags |= CartonUpdateFlags.RemovePallet;
                }
                return flags;
            }
        }

        public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Return error if nothing has been specified
            if (this.UpdateFlags == CartonUpdateFlags.None)
            {
                yield return new ValidationResult("Please specify what to do with the carton");
            }
            if (this.UpdatingRules.Pieces != null && this.QualificationRules.Pieces != null && this.QualificationRules.Pieces.Value == this.UpdatingRules.Pieces.Value)
            {
                yield return new ValidationResult("Qualifying pieces cannot be same as pieces to update");
            }
            if (!string.IsNullOrWhiteSpace(this.UpdatingRules.QualityCode) && !string.IsNullOrWhiteSpace(this.QualificationRules.QualityCode) && this.UpdatingRules.QualityCode == this.QualificationRules.QualityCode)
            {
                yield return new ValidationResult("Qualifying quality code cannot be same as quality code to update");
            }
            if (!string.IsNullOrWhiteSpace(this.UpdatingRules.SkuBarCode) && !string.IsNullOrWhiteSpace(this.QualificationRules.SkuBarCode) && this.UpdatingRules.SkuBarCode == this.QualificationRules.SkuBarCode)
            {
                yield return new ValidationResult("Qualifying SKU cannot be same as SKU to update");
            }
            if (!string.IsNullOrWhiteSpace(this.UpdatingRules.VwhId) && !string.IsNullOrWhiteSpace(this.QualificationRules.VwhId) && this.UpdatingRules.VwhId == this.QualificationRules.VwhId)
            {
                yield return new ValidationResult("Qualifying virtual warehouse cannot be same as virtual warehouse to update");
            }
            if (!string.IsNullOrWhiteSpace(this.UpdatingRules.PalletId) && !string.IsNullOrWhiteSpace(this.ScanText) && this.UpdatingRules.PalletId == this.ScanText)
            {
                yield return new ValidationResult("Same pallet scanned to move cartons");
            }
            if (!string.IsNullOrWhiteSpace(this.UpdatingRules.PriceSeasonCode) && !string.IsNullOrWhiteSpace(this.QualificationRules.PriceSeasonCode) && this.UpdatingRules.PriceSeasonCode == this.QualificationRules.PriceSeasonCode)
            {
                yield return new ValidationResult("Qualifying price season code cannot be same as price season code to update");
            }
            foreach (var result in ValidateReasonCode(validationContext))
            {
                yield return result;
            }
        }

        /// <summary>
        /// Override this if it is OK to not specify a reason code while updating
        /// </summary>
        /// <param name="validationContext"></param>
        /// <returns></returns>
        protected virtual IEnumerable<ValidationResult> ValidateReasonCode(ValidationContext validationContext)
        {
            if (this.UpdatingRules.Pieces !=null || !string.IsNullOrWhiteSpace(this.UpdatingRules.SkuBarCode))
            {
                if (string.IsNullOrWhiteSpace(this.UpdatingRules.ReasonCode))
                {
                    yield return new ValidationResult("Please specify reason code for changing carton inventory.");
                }
            }
        }

        #region Postback Properties
        /// <summary>
        /// This will be posted by mobile views
        /// </summary>
        public string ViewName { get; set; }
        #endregion
    }

    /// <summary>
    /// Polymorphic model binder. Creates the view model of the appropriate based upon the posted model type
    /// </summary>
    /// <remarks>
    /// Deriving from <see cref="DefaultModelBinderEx"/> to make <see cref="BindUpperCaseAttribute"/> work.
    /// </remarks>
    public class ViewModelBaseModelBinder : DefaultModelBinder
    {
        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            if (bindingContext.ModelType != typeof(ViewModelBase))
            {
                // We will only do special handling if the passed model type is ViewModelBase
                return base.BindModel(controllerContext, bindingContext);
            }
            var fieldName = ReflectionHelpers.NameFor((ViewModelBase m) => m.ViewModelType);
            var typeValue = bindingContext.ValueProvider.GetValue(fieldName);
            if (typeValue == null)
            {
                throw new Exception("All views must post the hidden field for ViewModelType");
            }
            var viewTypeName = (string)typeValue.ConvertTo(typeof(string));
            var viewType = Type.GetType(viewTypeName, true);
            bindingContext = new ModelBindingContext
                                {
                                    FallbackToEmptyPrefix = bindingContext.FallbackToEmptyPrefix, // only fall back if prefix not specified
                                    ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(null, viewType),
                                    ModelState = bindingContext.ModelState,
                                    PropertyFilter = null,
                                    ValueProvider = bindingContext.ValueProvider
                                };
            return base.BindModel(controllerContext, bindingContext);
        }

        protected override void OnModelUpdated(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var model = (ViewModelBase)bindingContext.Model;
            model.OnModelUpdated();
            base.OnModelUpdated(controllerContext, bindingContext);
        }
    }

}