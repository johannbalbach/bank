using Bank.BL.Consumer;
using Bank.BL.Services;
using Bank.DTO.DTOs.ServiceBusDto;
using Core.DAL;
using Core.DAL.Models.BankAccounts.Tariff;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Polly;

namespace Core.BL.MassTransit.Consumers
{
    public class TariffCreatedConsumerCore : BaseConsumer<CreateTariffEvent>
    {
        private readonly CoreDbContext _coreDbContext;

        public TariffCreatedConsumerCore(CoreDbContext coreDbContext, IIdempotencyService idempotencyService, IServiceProvider serviceProvider) : base(idempotencyService, serviceProvider)
        {
            _coreDbContext = coreDbContext;
        }

        protected override async Task<baseResponse> ProcessMessageAsync(CreateTariffEvent message)
        {
            var createTariffEvent = message;

            bool tariffExists = await _coreDbContext.CreditTariffs.AnyAsync(x => x.Id == createTariffEvent.Id);

            if (tariffExists)
            {
                return new NoResponse();
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

            return new NoResponse();
        }

        protected override async Task SendResponse(ConsumeContext<CreateTariffEvent> context, baseResponse response)
        {

        }
    }
}
