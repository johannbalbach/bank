using Core.DTO.DTOs.Responses.BankAccounts;
using Core.DTO.DTOs.Responses.Cards;

namespace Core.DTO.DTOs.Responses.Aggregates
{
    public class BankAccountCardDTO<TBankAccount, TCreditCard>
        where TBankAccount : BankAccountResponseDTO
        where TCreditCard : CardResponseDTO
    {
        public TBankAccount BankAccount { get; set; } = null!;
        public TCreditCard Card { get; set; } = null!;
    }
}
