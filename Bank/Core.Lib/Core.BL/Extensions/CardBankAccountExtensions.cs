using Core.DAL.Models.BankAccounts;
using Core.DAL.Models.Base;
using Core.DTO.DTOs.Responses.Aggregates;
using Core.DTO.DTOs.Responses.BankAccounts;
using Core.DTO.DTOs.Responses.Cards;

namespace Core.BL.Extensions
{
    public static class CardBankAccountExtensions
    {
        public static BankAccountCardDTO<BankAccountResponseDTO, CardResponseDTO> CardBankAccountToDTO(this CardBankAccount account)
        {
            return new BankAccountCardDTO<BankAccountResponseDTO, CardResponseDTO>
            {
                BankAccount = new()
                {
                    Id = account.Id,
                    CloseDateTime = account.CloseDateTime,
                    CurrencyType = account.CurrencyType,
                    Balance = account.Balance,
                    BankAccountType = account.BankAccountType,
                    IsFrozen = account.IsFrozen,
                    AccountName = account.AccountName,
                    CreateDateTime = account.CreateDateTime,
                    ModifyDateTime = account.ModifyDateTime
                },
                Card = new()
                {
                    Id = account.DebitCard.Id,
                    CardNumber = account.DebitCard.CardNumber,
                    CardType = account.DebitCard.CardType,
                    CardCategory = account.DebitCard.CardCategory,
                    CreateDateTime = account.DebitCard.CreateDateTime,
                    ModifyDateTime = account.DebitCard.ModifyDateTime
                }
            };
        }

    }
}
