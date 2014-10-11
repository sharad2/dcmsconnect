using System.ComponentModel.DataAnnotations;

namespace DcmsMobile.BoxManager.Helpers
{
    /// <summary>
    /// Ensures that the value is null
    /// </summary>
    public class MustBeNullAttribute: ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null || value.ToString() == string.Empty)
            {
                return ValidationResult.Success;
            }
            return new ValidationResult(FormatErrorMessage(validationContext.DisplayName));
        }
    }
}