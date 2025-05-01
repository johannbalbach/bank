using Bank.BL.Redis.Messages;
using Bank.BL.Redis.Patterns;
using Bank.DAL.Enums;
using Bank.DTO.DTOs.ServiceBusDto;
using Core.BL.CQRS.Base;
using Core.BL.Services.BankAccounts.BankAccountService;
using Core.BL.Services.BankAccounts.BankAccountsValidation;
using Core.BL.Services.Firebase;
using Core.BL.SignalR;
using Core.DAL;
using Core.DAL.Models.History;
using Core.DTO.DTOs.Requests.BankAccount;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

namespace Core.BL.CQRS.Commands.PutMoneyOnBankAccount
{
    public class PutMoneyOnCardBankAccountRequestHandler : IRequestHandler<MediatorRequest<PutMoneyOnCardBankAccountRequest, PutMoneyOnCardBankAccountResponse>, PutMoneyOnCardBankAccountResponse>
    {
        private CoreDbContext _coreDbContext;
        private IRequestClient<IsUserBlockedRequest> _requestClient;
        private IBankAccountService _bankAccountService;
        private IHubContext<CoreSignalRHub, ICoreHub> _hub;
        private readonly IFirebaseService _firebaseService;
        private readonly IRedisMessagingFacade _redisMessagingFacade;

        private BankAccountValidationIsFrozen _handler;

        public PutMoneyOnCardBankAccountRequestHandler(
            CoreDbContext coreDbContext,
            IRequestClient<IsUserBlockedRequest> requestClient,
            IHubContext<CoreSignalRHub, ICoreHub> hub,
            IBankAccountService bankAccountService,
            IFirebaseService firebaseService,
            IRedisMessagingFacade redisMessagingFacade)
        {
            _coreDbContext = coreDbContext;
            _requestClient = requestClient;
            _hub = hub;
            _bankAccountService = bankAccountService;
            _firebaseService = firebaseService;
            _redisMessagingFacade = redisMessagingFacade;

            _handler = new();
            _handler.ConnectHandler(new BankAccountValidationIsClosed().ConnectHandler(new NullHandler()));
        }

        public async Task<PutMoneyOnCardBankAccountResponse> Handle(MediatorRequest<PutMoneyOnCardBankAccountRequest, PutMoneyOnCardBankAccountResponse> request, CancellationToken cancellationToken)
        {
            var cardBankAccount = await _coreDbContext.CardBankAccounts
                                    .Include(x => x.Currency)
                                    .FirstOrDefaultAsync(x => x.Id == request.Request.AccountId, cancellationToken: cancellationToken)
                                    ?? throw new KeyNotFoundException($"Bank account with id {request.Request.AccountId} was not found");

            if(cardBankAccount.OwnerId != request.MetaData!.UserId
               && request.MetaData.UserRole != UserRole.Employee && !request.Request.IsTransfer)
            {
                throw new AccessViolationException($"ERROR");
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

            decimal previousBalance = cardBankAccount.Balance;
            cardBankAccount.Balance += request.Request.RequestDTO.Money;
            cardBankAccount.ModifyDateTime = DateTime.UtcNow;
            _coreDbContext.Entry(cardBankAccount).State = EntityState.Modified;

            var operationsHistory = new BankAccountOperationsHistory
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

            await _redisMessagingFacade.ProcessRedisMessage<RedisMessage>(request.MetaData.RedisMessageId, $"Put money on card bank account with id {cardBankAccount.Id}", StatusCodes.Status200OK);

            await _firebaseService.SendFirebasePushMessage(request.MetaData, new()
            {
                UserNotification = new()
                {
                    Title = "Пополнение счёта",
                    Body = $"Пополнение дебетового счёта {cardBankAccount.AccountName} на сумму {request.Request.RequestDTO.Money} {request.Request.RequestDTO.CurrencyType}"
                },
                EmployeesNotification = new()
                {
                    Title = "Пополнение счёта",
                    Body = $"Пользователь с id {request.MetaData.UserId} пополнил дебетовый счёт с id {cardBankAccount.Id} ({cardBankAccount.AccountName}) " +
                    $"на сумму {request.Request.RequestDTO.Money} {request.Request.RequestDTO.CurrencyType}"
                }
            });

            return new PutMoneyOnCardBankAccountResponse();
        }
    }
}
