using Bank.BL.Consumer;
using Bank.BL.Services;
using Bank.DTO.DTOs.ServiceBusDto;
using Core.DAL;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Polly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.BL.MassTransit.Consumers
{
    public class GetBankAccountHistoryConsumerCore: BaseConsumer<GetBankAccountHistoryRequest>
    {
        public CoreDbContext _coreDbContext;

        public GetBankAccountHistoryConsumerCore(CoreDbContext coreDbContext, IIdempotencyService idempotencyService, IServiceProvider serviceProvider) : base(idempotencyService, serviceProvider)
        {
            _coreDbContext = coreDbContext;
        }

        protected override async Task<baseResponse> ProcessMessageAsync(GetBankAccountHistoryRequest message)
        {
            var getBankAccountRequest = message;

            var bankAccountHistoryList = await _coreDbContext.BankAccountOperationsHistory
                .Where(x => x.BankAccountId == getBankAccountRequest.BankAccountId)
                .Select(x => new GetBankAccountHistoryCommandDto
                {
                    OperationType = x.BankAccountOperationType,
                    OperatingMoney = x.OperatingMoney,
                    CurrentBalance = x.CurrentBalance,
                    PreviousBalance = x.PreviousBalance,
                    OperationDateTime = x.OperationDateTime,
                    OperationInitiator = x.BankAccountOperationInitiator,
                    OperationStatus = x.BankAccountOperationStatus,
                    BankAccountId = x.BankAccountId,
                    UserId = x.UserId,
                })
                .ToListAsync()
                ?? throw new KeyNotFoundException($"Some unexpected error while getting bank account history list");

            GetBankAccountHistoryCommand result = new GetBankAccountHistoryCommand { getBankAccountHistoryCommandDtos = bankAccountHistoryList };

            return result;
        }

        protected override async Task SendResponse(ConsumeContext<GetBankAccountHistoryRequest> context, baseResponse response)
        {
            await context.RespondAsync((GetBankAccountHistoryCommand)response);
        }
    }
}
