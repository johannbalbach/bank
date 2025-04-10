using Bank.DAL.Enums;
using Bank.DTO.DTOs;

namespace Core.DTO.DTOs.Responses.BankAccounts
{
    public class BankAccountResponseDTO : BaseDTO
    {
        public DateTime? CloseDateTime { get; set; }
        public string CurrencyType { get; set; }
        public decimal Balance { get; set; }
        public BankAccountType BankAccountType { get; set; }
        public bool IsFrozen { get; set; }
        public string AccountName { get; set; }
    }
}
