using Bank.BL.Other;
using Bank.BL.Values;
using Bank.DAL.Enums;
using Bank.DTO.DTOs.ServiceBusDto;
using Core.DAL;
using Core.DAL.Models;
using Core.DAL.Models.BankAccounts;
using Core.DAL.Models.Cards;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Core.BL.MassTransit.Consumers
{
    public class BankAccountCreatedConsumerCore : IConsumer<CreateBankAccountEvent>
    {
        private readonly CoreDbContext _coreDbContext;

        public BankAccountCreatedConsumerCore(CoreDbContext coreDbContext)
        {
            _coreDbContext = coreDbContext;
        }

        public async Task Consume(ConsumeContext<CreateBankAccountEvent> context)
        {
            var createBankAccountEvent = context.Message;

            bool creditBankAccountExists = await _coreDbContext.CreditBankAccounts.AnyAsync(x => x.Id == createBankAccountEvent.Id);

            if (creditBankAccountExists)
            {
                return;
            }

            if (createBankAccountEvent.AccountName == "MasterAccount")
            {
                User user = new User
                {
                    Id = createBankAccountEvent.OwnerId,
                    FullName = "MasterUser"
                };
                await _coreDbContext.Users.AddAsync(user);
                await _coreDbContext.SaveChangesAsync();
            }
            bool userExists = await _coreDbContext.Users.AnyAsync(x => x.Id == createBankAccountEvent.OwnerId);

            if (!userExists)
            {
                return;
            }

            bool tariffExists = await _coreDbContext.CreditTariffs.AnyAsync(x => x.Id == createBankAccountEvent.TariffId);

            if (!tariffExists)
            {
                return;
            }

            var currencyType = await _coreDbContext.Currencies.FirstOrDefaultAsync(x => x.VchCode == createBankAccountEvent.CurrencyType)
                               ?? throw new KeyNotFoundException($"Валюта {createBankAccountEvent.CurrencyType} was not found");

            var creditBankAccount = new CreditBankAccount
            {
                Id = createBankAccountEvent.Id,
                CreateDateTime = DateTime.UtcNow,
                AccountNumber = createBankAccountEvent.AccountNumber,
                Debt = createBankAccountEvent.Debt,
                Balance = createBankAccountEvent.Balance,
                TariffId = createBankAccountEvent.TariffId,
                CurrencyType = createBankAccountEvent.CurrencyType,
                BankAccountType = createBankAccountEvent.AccountType == BankAccountType.Master ? BankAccountType.Master : BankAccountType.Credit,
                OwnerId = createBankAccountEvent.OwnerId,
                AccountName = createBankAccountEvent.AccountName,
                IsFrozen = false,
                Currency = currencyType
            };
            var initCard = new CreditCard
            {
                CardCategory = CardCategory.Black,
                CardNumber = StringGenerator.GenerateRandomString(1, CoreConstants.CardNumberLength),
                CreateDateTime = DateTime.UtcNow,
                IsActive = true,
                OwnerId = createBankAccountEvent.OwnerId,
                CreditBankAccountId = creditBankAccount.Id
            };
            creditBankAccount.CreditCard = initCard;
            creditBankAccount.PayingCard = initCard;


            await _coreDbContext.AddAsync(creditBankAccount);

            await _coreDbContext.SaveChangesAsync();
        }
    }
}
