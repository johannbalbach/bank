using Bank.BL.Consumer;
using Bank.BL.Services;
using Bank.DTO.DTOs.ServiceBusDto;
using Core.DAL;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Polly;

namespace Core.BL.MassTransit.Consumers
{
    public class CloseBankAccountConsumerCore : BaseConsumer<CloseBankAccountEvent>
    {
        private CoreDbContext _coreDbContext;

        public CloseBankAccountConsumerCore(CoreDbContext coreDbContext, IIdempotencyService idempotencyService, IServiceProvider serviceProvider)
            : base(idempotencyService, serviceProvider)
        {
            _coreDbContext = coreDbContext;
        }

        protected override async Task<baseResponse> ProcessMessageAsync(CloseBankAccountEvent message)
        {
            var closeBankAccountEvent = message;

            var creditBankAccount = await _coreDbContext.CreditBankAccounts
                                    .FirstOrDefaultAsync(x => x.Id == closeBankAccountEvent.Id)
                                    ?? throw new KeyNotFoundException($"Credit bank account with id {closeBankAccountEvent.Id} was not found");

            creditBankAccount.CloseDateTime = DateTime.UtcNow;
            _coreDbContext.Entry(creditBankAccount).State = EntityState.Modified;

            await _coreDbContext.SaveChangesAsync();

            return new NoResponse();
        }

        protected override async Task SendResponse(ConsumeContext<CloseBankAccountEvent> context, baseResponse response)
        {

        }
    }
}
