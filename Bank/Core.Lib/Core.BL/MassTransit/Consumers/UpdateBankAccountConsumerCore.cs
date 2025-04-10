using Bank.DTO.DTOs.ServiceBusDto;
using Core.DAL;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Core.BL.MassTransit.Consumers
{
    public class UpdateBankAccountConsumerCore : IConsumer<UpdateBankAccountEvent>
    {
        private CoreDbContext _coreDbContext;

        public UpdateBankAccountConsumerCore(CoreDbContext coreDbContext)
        {
            _coreDbContext = coreDbContext;
        }

        public async Task Consume(ConsumeContext<UpdateBankAccountEvent> context)
        {
            var updateBankAccountEvent = context.Message;

            var creditBankAccount = await _coreDbContext.CreditBankAccounts
                                    .FirstOrDefaultAsync(x => x.Id == updateBankAccountEvent.Id)
                                    ?? throw new KeyNotFoundException($"Credit bank account with id {updateBankAccountEvent.Id} was not found");

            creditBankAccount.Balance = updateBankAccountEvent.Balance;
            creditBankAccount.Debt = updateBankAccountEvent.Debt;
            _coreDbContext.Entry(creditBankAccount).State = EntityState.Modified;

            await _coreDbContext.SaveChangesAsync();
        }
    }
}
