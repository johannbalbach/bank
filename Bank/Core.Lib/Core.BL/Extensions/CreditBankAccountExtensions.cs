using Core.DAL.Models.BankAccounts;
using Core.DTO.DTOs.Responses.Aggregates;
using Core.DTO.DTOs.Responses.BankAccounts;
using Core.DTO.DTOs.Responses.Cards;

namespace Core.BL.Extensions
{
    public static class CreditBankAccountExtensions
    {
        public static BankAccountCardDTO<BankAccountResponseDTO, CardResponseDTO> CreditBankAccountToDTOShort(this CreditBankAccount account)
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
                    Id = account.CreditCard.Id,
                    CardNumber = account.CreditCard.CardNumber,
                    CardType = account.CreditCard.CardType,
                    CardCategory = account.CreditCard.CardCategory,
                    CreateDateTime = account.CreditCard.CreateDateTime,
                    ModifyDateTime = account.CreditCard.ModifyDateTime
                }
            };
        }

        public static BankAccountCardDTO<BankAccountResponseDTO, CreditCardResponseDTO> CreditBankAccountToDTO(this CreditBankAccount account)
        {
            return new BankAccountCardDTO<BankAccountResponseDTO, CreditCardResponseDTO>
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
                    Id = account.CreditCard.Id,
                    CardNumber = account.CreditCard.CardNumber,
                    CardType = account.CreditCard.CardType,
                    CardCategory = account.CreditCard.CardCategory,
                    IsActive = account.CreditCard.IsActive,
                    CreateDateTime = account.CreditCard.CreateDateTime,
                    ModifyDateTime = account.CreditCard.ModifyDateTime
                }
            };
        }

        public static BankAccountCardDTO<CreditBankAccountFullResponseDTO, CreditCardResponseDTO> CreditBankAccountToDTOFull(this CreditBankAccount account)
        {
            return new BankAccountCardDTO<CreditBankAccountFullResponseDTO, CreditCardResponseDTO>
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
                    Debt = account.Debt,
                    AccountNumber = account.AccountNumber,
                    CreateDateTime = account.CreateDateTime,
                    ModifyDateTime = account.ModifyDateTime,
                    Tariff = new()
                    {
                        Id = account.Tariff.Id,
                        CreditLimit = account.Tariff.CreditLimit,
                        InterestRate = account.Tariff.InterestRate,
                        MinimumPayment = account.Tariff.MinimumPayment,
                        Name = account.Tariff.Name,
                        PaymentType = account.Tariff.PaymentType
                    }
                },
                Card = new()
                {
                    Id = account.CreditCard.Id,
                    CardNumber = account.CreditCard.CardNumber,
                    CardType = account.CreditCard.CardType,
                    IsActive = account.CreditCard.IsActive,
                    CreateDateTime = account.CreditCard.CreateDateTime,
                    ModifyDateTime = account.CreditCard.ModifyDateTime
                }
            };
        }
    }
}
