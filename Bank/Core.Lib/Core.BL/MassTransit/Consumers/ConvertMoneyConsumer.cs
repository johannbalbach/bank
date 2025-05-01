using Bank.BL.Consumer;
using Bank.BL.Services;
using Bank.DTO.DTOs.ServiceBusDto;
using Core.BL.Services.BankAccounts.BankAccountService;
using MassTransit;
using Polly;

namespace Core.BL.MassTransit.Consumers
{
    public class ConvertMoneyConsumer : BaseConsumer<ConvertMoneyRequest>
    {
        private readonly IBankAccountService _bankAccountService;

        public ConvertMoneyConsumer(IBankAccountService bankAccountService, IIdempotencyService idempotencyService, IServiceProvider serviceProvider)
            : base(idempotencyService, serviceProvider)
        {
            _bankAccountService = bankAccountService;
        }

        protected override async Task<baseResponse> ProcessMessageAsync(ConvertMoneyRequest message1)
        {
            ConvertMoneyRequest message = message1;

            var convertedAmount = await _bankAccountService.ConvertMoney(
                message.RequestCurrencyName,
                message.AccountCurrencyName,
                message.RequestCurrencyAmount);

            return (new ConvertMoneyCommand
            {
                Amount = convertedAmount
            });
        }

        protected override async Task SendResponse(ConsumeContext<ConvertMoneyRequest> context, baseResponse response)
        {
            await context.RespondAsync((ConvertMoneyCommand)response);
        }
    }
}
