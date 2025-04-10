using Bank.DTO.DTOs.ServiceBusDto;
using Core.BL.Services.BankAccounts.BankAccountService;
using MassTransit;

namespace Core.BL.MassTransit.Consumers
{
    public class ConvertMoneyConsumer : IConsumer<ConvertMoneyRequest>
    {
        private readonly IBankAccountService _bankAccountService;

        public ConvertMoneyConsumer(IBankAccountService bankAccountService)
        {
            _bankAccountService = bankAccountService;
        }

        public async Task Consume(ConsumeContext<ConvertMoneyRequest> context)
        {
            ConvertMoneyRequest message = context.Message;

            var convertedAmount = await _bankAccountService.ConvertMoney(
                message.RequestCurrencyName,
                message.AccountCurrencyName,
                message.RequestCurrencyAmount);

            await context.RespondAsync(new ConvertMoneyCommand
            {
                Amount = convertedAmount
            });
        }
    }
}
