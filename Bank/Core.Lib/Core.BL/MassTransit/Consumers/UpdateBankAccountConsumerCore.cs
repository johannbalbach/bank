using Bank.BL.Consumer;
using Bank.BL.Services;
using Bank.DTO.DTOs.ServiceBusDto;
using Core.DAL;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Polly;

namespace Core.BL.MassTransit.Consumers
{
    public class UpdateBankAccountConsumerCore : BaseConsumer<UpdateBankAccountEvent>
    {
        private CoreDbContext _coreDbContext;

        public UpdateBankAccountConsumerCore(CoreDbContext coreDbContext, IIdempotencyService idempotencyService, IServiceProvider serviceProvider)
            : base(idempotencyService, serviceProvider)
        {
            _coreDbContext = coreDbContext;
        }

        protected override async Task<baseResponse> ProcessMessageAsync(UpdateBankAccountEvent message)
        {
            var updateBankAccountEvent = message;

            var creditBankAccount = await _coreDbContext.CreditBankAccounts
                                    .FirstOrDefaultAsync(x => x.Id == updateBankAccountEvent.Id)
                                    ?? throw new KeyNotFoundException($"Credit bank account with id {updateBankAccountEvent.Id} was not found");

            creditBankAccount.Balance = updateBankAccountEvent.Balance;
            creditBankAccount.Debt = updateBankAccountEvent.Debt;
            _coreDbContext.Entry(creditBankAccount).State = EntityState.Modified;

            await _coreDbContext.SaveChangesAsync();

            return new NoResponse();
        }

        protected override async Task SendResponse(ConsumeContext<UpdateBankAccountEvent> context, baseResponse response)
        {

        }
    }
}
