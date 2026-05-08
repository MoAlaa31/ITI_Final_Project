using System.ComponentModel.DataAnnotations;

namespace ITI_Project.Api.Attributes
{
    public class ValidEnumAttribute<T> : ValidationAttribute where T : Enum
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
                return ValidationResult.Success;

            // Prevent default/undefined enum value (commonly 0) from passing validation
            // when the API expects enums to start from 1.
            if (value is T enumValue && EqualityComparer<T>.Default.Equals(enumValue, default))
                return new ValidationResult($"Invalid value for {validationContext.DisplayName}.");

            if (Enum.IsDefined(typeof(T), value))
                return ValidationResult.Success;

            return new ValidationResult($"Invalid value for {validationContext.DisplayName}.");
        }
    }
}
