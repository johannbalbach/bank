using Bank.DTO.DTOs.ServiceBusDto;
using Core.DAL;
using Core.DAL.Models.BankAccounts.Tariff;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Core.BL.MassTransit.Consumers
{
    public class TariffCreatedConsumerCore : IConsumer<CreateTariffEvent>
    {
        private readonly CoreDbContext _coreDbContext;

        public TariffCreatedConsumerCore(CoreDbContext coreDbContext)
        {
            _coreDbContext = coreDbContext;
        }

        public async Task Consume(ConsumeContext<CreateTariffEvent> context)
        {
            var createTariffEvent = context.Message;

            bool tariffExists = await _coreDbContext.CreditTariffs.AnyAsync(x => x.Id == createTariffEvent.Id);

            if (tariffExists)
            {
                return;
            }

            var tariff = new CreditTariff
            {
                Id = createTariffEvent.Id,
                Name = createTariffEvent.Name,
                InterestRate = createTariffEvent.InterestRate,
                MinimumPayment = createTariffEvent.MinimumPayment,
                PaymentType = createTariffEvent.PaymentType,
                CreditLimit = createTariffEvent.CreditLimit,
                CreateDateTime = DateTime.UtcNow
            };

            await _coreDbContext.CreditTariffs.AddAsync(tariff);

            await _coreDbContext.SaveChangesAsync();
        }
    }
}
