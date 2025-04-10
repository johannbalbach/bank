using Core.DTO.DTOs.Responses.BankAccounts;
using Core.DTO.DTOs.Responses.Cards;

namespace Core.DTO.DTOs.Responses.Aggregates
{
    public class UserBankAccountsResponseDTO
    {
        public ICollection<BankAccountCardDTO<BankAccountResponseDTO, CardResponseDTO>> CardBankAccounts { get; set; }
        public ICollection<BankAccountCardDTO<BankAccountResponseDTO, CreditCardResponseDTO>> CreditBankAccounts { get; set; }
    }
}
