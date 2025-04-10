using System.ComponentModel.DataAnnotations;

namespace CreditService.Dtos
{
    public class CreditRequestDto
    {
        public Guid TariffId { get; set; }

        [Required(ErrorMessage = "Указание количества денег обязательно")]
        [Range(1.0, 100000, ErrorMessage = "Количество денег должно быть между 1 и 100000")]
        public decimal Amount { get; set; }

        [MaxLength(5, ErrorMessage = "Длина названия валюты не может превышать 5 символов")]
        [MinLength(1, ErrorMessage = "Длина названия валюты не может быть меньше 1 символа")]
        [RegularExpression("[A-Z]{1,5}", ErrorMessage = "Название валюты должно включать только латинские буквы")]
        public string CurrencyType { get; set; }

        [Required(ErrorMessage = "Название счёта обязательно")]
        [MaxLength(200, ErrorMessage = "Длина названия счёта не может превышать 200 символов")]
        public string AccountName { get; set; }
    }
}
