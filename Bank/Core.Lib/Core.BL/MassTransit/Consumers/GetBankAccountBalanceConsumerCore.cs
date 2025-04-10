using Bank.DTO.DTOs.ServiceBusDto;
using Core.DAL;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Core.BL.MassTransit.Consumers
{
    public class GetBankAccountBalanceConsumerCore : IConsumer<GetBankAccountBalanceRequest>
    {
        public CoreDbContext _coreDbContext;

        public GetBankAccountBalanceConsumerCore(CoreDbContext coreDbContext)
        {
            _coreDbContext = coreDbContext;
        }

        public async Task Consume(ConsumeContext<GetBankAccountBalanceRequest> context)
        {
            var getBankAccountRequest = context.Message;

            var creditBankAccount = await _coreDbContext.CreditBankAccounts
                                    .Select(x => new GetBankAccountBalanceCommand
                                    {
                                        Id = x.Id,
                                        Debt = x.Debt,
                                        Balance = x.Balance
                                    })
                                    .FirstOrDefaultAsync(x => x.Id == getBankAccountRequest.BankAccountId)
                                    ?? throw new KeyNotFoundException($"Bank account with id {getBankAccountRequest.BankAccountId} was not found");

            await context.RespondAsync(creditBankAccount);
        }
    }
}
