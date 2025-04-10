using System.ComponentModel.DataAnnotations;

namespace UserService.Validators
{
    public class BirthDateValidation: ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return new ValidationResult("Дата рождения не может быть пустой.");
            }

            if (value is DateTime birthDate)
            {
                var currentDate = DateTime.Now;
                var tenYearsAgo = currentDate.AddYears(-10);

                if (birthDate <= tenYearsAgo)
                {
                    return ValidationResult.Success;
                }
                else
                {
                    return new ValidationResult("Дата рождения должна быть меньше текущей даты хотя бы на 10 лет.");
                }
            }
            else
            {
                return new ValidationResult("Неверный формат даты.");
            }
        }
    }
}
