using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using DcmsMobile.Repack.ViewModels;

namespace DcmsMobile.Repack.Helpers
{
    /// <summary>
    /// Adds the data-val-required additional value to model meta data if the value is really required based on UI.
    /// This works because our editor templates treat ModelMetadata.AdditionalValues as HTML attributes.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class RequiredForUiAttribute : ValidationAttribute, IMetadataAware
    {
        private readonly RepackUiStyle _uiStyles;

        public RequiredForUiAttribute(RepackUiStyle uiStyles)
        {
            _uiStyles = uiStyles;
        }

        public void OnMetadataCreated(ModelMetadata metadata)
        {
            var uiStyle = RepackViewModel.CurrentUiStyle;
            metadata.IsRequired = _uiStyles.HasFlag(uiStyle);
            if (metadata.IsRequired)
            {
                metadata.AdditionalValues.Add("data-val-required", FormatErrorMessage(ErrorMessage));
            }
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var uiStyle = RepackViewModel.CurrentUiStyle;
            if (_uiStyles.HasFlag(uiStyle) && value == null)
            {
                return new ValidationResult(FormatErrorMessage(ErrorMessage), new[] { validationContext.MemberName });
            }
            return ValidationResult.Success;
        }
    }
}