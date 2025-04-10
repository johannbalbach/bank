using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace UserService.Validators
{
    public class PhoneNumberValidation: ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return new ValidationResult("Номер телефона не может быть пустым.");
            }

            string phoneNumber = value.ToString();
            string pattern = @"^\+7(\d{10}|\(\d{3}\)[-\s]?\d{3}[-\s]?\d{2}[-\s]?\d{2}|\d{3}[-\s]?\d{3}[-\s]?\d{2}[-\s]?\d{2})$";

            if (Regex.IsMatch(phoneNumber, pattern))
            {
                return ValidationResult.Success;
            }
            else
            {
                return new ValidationResult("Неверный формат номера телефона. Допустимые форматы: +7 *** *** ** **, +7 (***) *** ** **, +7**********, +7(***)-***-**-**");
            }
        }
    }
}
