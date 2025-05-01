using Bank.BL.Consumer;
using Bank.BL.Services;
using Bank.DTO.DTOs.ServiceBusDto;
using Core.DAL;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Polly;

namespace Core.BL.MassTransit.Consumers
{
    public class GetBankAccountBalanceConsumerCore : BaseConsumer<GetBankAccountBalanceRequest>
    {
        public CoreDbContext _coreDbContext;

        public GetBankAccountBalanceConsumerCore(CoreDbContext coreDbContext, IIdempotencyService idempotencyService, IServiceProvider serviceProvider)
            : base(idempotencyService, serviceProvider)
        {
            _coreDbContext = coreDbContext;
        }

        protected override async Task<baseResponse> ProcessMessageAsync(GetBankAccountBalanceRequest message)
        {
            var getBankAccountRequest = message;

            var creditBankAccount = await _coreDbContext.CreditBankAccounts
                                    .Select(x => new GetBankAccountBalanceCommand
                                    {
                                        Id = x.Id,
                                        Debt = x.Debt,
                                        Balance = x.Balance
                                    })
                                    .FirstOrDefaultAsync(x => x.Id == getBankAccountRequest.BankAccountId)
                                    ?? throw new KeyNotFoundException($"Bank account with id {getBankAccountRequest.BankAccountId} was not found");

            return creditBankAccount;
        }

        protected override async Task SendResponse(ConsumeContext<GetBankAccountBalanceRequest> context, baseResponse response)
        {
            await context.RespondAsync((GetBankAccountBalanceCommand)response);
        }
    }
}
