using System.ComponentModel.DataAnnotations;

namespace ITI_Project.Api.Attributes
{
    public class FullNameAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is string fullName)
            {
                var nameParts = fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);

                if (nameParts.Length >= 2)
                {
                    return ValidationResult.Success;
                }
            }
            return new ValidationResult("Full name must contain at least two words.");
        }
    }
}
