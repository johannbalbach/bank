using System.ComponentModel.DataAnnotations;

namespace Core.DTO.DTOs.Requests.BankAccount
{
    public class ChangeBankAccountNameRequestDTO
    {
        [Required(ErrorMessage = "Название счёта обязательно")]
        [MaxLength(200, ErrorMessage = "Длина названия счёта не может превышать 200 символов")]
        public string AccountName { get; set; }
    }
}
