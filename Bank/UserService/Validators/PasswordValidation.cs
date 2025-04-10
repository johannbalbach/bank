using System.ComponentModel.DataAnnotations;

namespace UserService.Validators
{
    public class PasswordValidation : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {

            if (value == null)
            {
                return null;
            }

            var password = (string)value!;
            if (!password.Any(char.IsDigit))
            {
                return new ValidationResult("Password must contain at least 1 digit");
            }

            return ValidationResult.Success;
        }
    }
}
