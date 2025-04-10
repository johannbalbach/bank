using System.ComponentModel.DataAnnotations;

namespace Core.DTO.DTOs.Requests.BankAccount
{
    public class DebitCardBankAccountCreateRequestDTO
    {
        [MaxLength(5, ErrorMessage = "Длина названия валюты не может превышать 5 символов")]
        [MinLength(1, ErrorMessage = "Длина названия валюты не может быть меньше 1 символа")]
        [RegularExpression("[A-Z]{1,5}", ErrorMessage = "Название валюты должно включать только латинские буквы")]
        public string CurrencyType { get; set; }

        [Required(ErrorMessage = "Название счёта обязательно")]
        [MaxLength(200, ErrorMessage = "Длина названия счёта не может превышать 200 символов")]
        public string AccountName { get; set; }
    }
}
