using Bank.DTO.DTOs.ServiceBusDto;
using Core.DAL;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Core.BL.MassTransit.Consumers
{
    public class CloseBankAccountConsumerCore : IConsumer<CloseBankAccountEvent>
    {
        private CoreDbContext _coreDbContext;

        public CloseBankAccountConsumerCore(CoreDbContext coreDbContext)
        {
            _coreDbContext = coreDbContext;
        }

        public async Task Consume(ConsumeContext<CloseBankAccountEvent> context)
        {
            var closeBankAccountEvent = context.Message;

            var creditBankAccount = await _coreDbContext.CreditBankAccounts
                                    .FirstOrDefaultAsync(x => x.Id == closeBankAccountEvent.Id)
                                    ?? throw new KeyNotFoundException($"Credit bank account with id {closeBankAccountEvent.Id} was not found");

            creditBankAccount.CloseDateTime = DateTime.UtcNow;
            _coreDbContext.Entry(creditBankAccount).State = EntityState.Modified;

            await _coreDbContext.SaveChangesAsync();
        }
    }
}
