using Bank.BL.Consumer;
using Bank.BL.Other;
using Bank.BL.Services;
using Bank.BL.Values;
using Bank.DAL.Enums;
using Bank.DTO.DTOs.ServiceBusDto;
using Core.DAL;
using Core.DAL.Models;
using Core.DAL.Models.BankAccounts;
using Core.DAL.Models.Cards;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Polly;

namespace Core.BL.MassTransit.Consumers
{
    public class BankAccountCreatedConsumerCore : BaseConsumer<CreateBankAccountEvent>
    {
        private readonly CoreDbContext _coreDbContext;

        public BankAccountCreatedConsumerCore(CoreDbContext coreDbContext, IIdempotencyService idempotencyService, IServiceProvider serviceProvider)
            : base(idempotencyService, serviceProvider)
        {
            _coreDbContext = coreDbContext;
        }

        protected override async Task<baseResponse> ProcessMessageAsync(CreateBankAccountEvent message)
        {
            var createBankAccountEvent = message;

            bool creditBankAccountExists = await _coreDbContext.CreditBankAccounts.AnyAsync(x => x.Id == createBankAccountEvent.Id);

            if (creditBankAccountExists)
            {
                return new NoResponse();
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
                return new NoResponse();
            }

            bool tariffExists = await _coreDbContext.CreditTariffs.AnyAsync(x => x.Id == createBankAccountEvent.TariffId);

            if (!tariffExists)
            {
                return new NoResponse();
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

            return new NoResponse();
        }

        protected override async Task SendResponse(ConsumeContext<CreateBankAccountEvent> context, baseResponse response)
        {
           
        }
    }
}
