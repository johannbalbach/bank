using Bank.DAL.Enums;
using Bank.DTO.DTOs.ServiceBusDto;
using Core.BL.CQRS.Base;
using Core.BL.Services.BankAccounts.BankAccountService;
using Core.BL.Services.BankAccounts.BankAccountsValidation;
using Core.BL.SignalR;
using Core.DAL;
using Core.DAL.Models.History;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Core.BL.CQRS.Commands.WithdrawMoneyFromCardBankAccount
{
    public class WithdrawMoneyFromCardBankAccountRequestHandler : IRequestHandler<MediatorRequest<WithdrawMoneyFromCardBankAccountRequest, WithdrawMoneyFromCardBankAccountResponse>,
        WithdrawMoneyFromCardBankAccountResponse>
    {
        private CoreDbContext _coreDbContext;
        private IRequestClient<IsUserBlockedRequest> _requestClient;
        private IHubContext<CoreSignalRHub, ICoreHub> _hub;
        private IBankAccountService _bankAccountService;

        private BankAccountValidationIsFrozen _handler;

        public WithdrawMoneyFromCardBankAccountRequestHandler(
            CoreDbContext coreDbContext,
            IRequestClient<IsUserBlockedRequest> requestClient,
            IHubContext<CoreSignalRHub, ICoreHub> hub,
            IBankAccountService bankAccountService)
        {
            _coreDbContext = coreDbContext;
            _requestClient = requestClient;
            _hub = hub;
            _bankAccountService = bankAccountService;

            _handler = new();
            _handler.ConnectHandler(new BankAccountValidationIsClosed().ConnectHandler(new NullHandler()));
        }

        public async Task<WithdrawMoneyFromCardBankAccountResponse> Handle(MediatorRequest<WithdrawMoneyFromCardBankAccountRequest, WithdrawMoneyFromCardBankAccountResponse> request, CancellationToken cancellationToken)
        {
            var cardBankAccount = await _coreDbContext.CardBankAccounts
                                  .FirstOrDefaultAsync(x => x.Id == request.Request.AccountId, cancellationToken: cancellationToken)
                                  ?? throw new KeyNotFoundException($"Bank account with id {request.Request.AccountId} was not found");

            if(cardBankAccount.OwnerId != request.MetaData!.UserId && !request.Request.IsTransfer)
            {
                throw new AccessViolationException($"ERROR!");
            }

            var result = await _requestClient.GetResponse<IsUserBlockedCommand>(new IsUserBlockedRequest { UserId = request.MetaData.UserId });
            if (result.Message.IsBlocked)
            {
                throw new AccessViolationException($"You are blocked");
            }

            await _handler.Invoke(cardBankAccount);

            request.Request.RequestDTO.Money = await _bankAccountService.ConvertMoney(
                request.Request.RequestDTO.CurrencyType,
                cardBankAccount.CurrencyType,
                request.Request.RequestDTO.Money);

            if (cardBankAccount.Balance - request.Request.RequestDTO.Money < 0)
            {
                throw new InvalidDataException($"Card bank account balance cant be less than 0");
            }

            decimal previousBalance = cardBankAccount.Balance;
            cardBankAccount.Balance -= request.Request.RequestDTO.Money;
            cardBankAccount.ModifyDateTime = DateTime.UtcNow;
            _coreDbContext.Entry(cardBankAccount).State = EntityState.Modified;

            var operationsHistory = new BankAccountOperationsHistory
            {
                BankAccountOperationType = BankAccountOperationType.Withdrawal,
                OperatingMoney = request.Request.RequestDTO.Money,
                CurrentBalance = cardBankAccount.Balance,
                PreviousBalance = previousBalance,
                OperationDateTime = DateTime.UtcNow,
                BankAccountOperationInitiator = BankAccountOperationInitiator.User,
                BankAccountOperationStatus = BankAccountOperationStatus.Success,

                BankAccountId = request.Request.AccountId,
                UserId = request.MetaData.UserId
            };
            await _coreDbContext.BankAccountOperationsHistory.AddAsync(operationsHistory, cancellationToken);

            await _coreDbContext.SaveChangesAsync(cancellationToken);

            var response = new
            {
                BankAccountOperationType = BankAccountOperationType.Replenishment,
                OperatingMoney = request.Request.RequestDTO.Money,
                CurrentBalance = cardBankAccount.Balance,
                PreviousBalance = previousBalance,
                OperationDateTime = DateTime.UtcNow,
                BankAccountOperationInitiator = BankAccountOperationInitiator.User,
                BankAccountOperationStatus = BankAccountOperationStatus.Success,

                UserId = request.MetaData.UserId,
                BankAccountId = request.Request.AccountId
            };

            string serialize = JsonSerializer.Serialize(response);

            await _hub.Clients.Group(UserRole.Employee.ToString()).OperationCreated(serialize);
            if (request.MetaData.UserRole != UserRole.Employee)
            {
                await _hub.Clients.Group(request.MetaData.UserId.ToString()).OperationCreated(serialize);
            }

            return new WithdrawMoneyFromCardBankAccountResponse();
        }
    }
}
