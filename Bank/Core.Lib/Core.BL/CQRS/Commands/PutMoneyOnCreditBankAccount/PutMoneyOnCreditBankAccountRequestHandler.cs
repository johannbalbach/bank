using Bank.BL.Redis.Messages;
using Bank.BL.Redis.Patterns;
using Bank.DAL.Enums;
using Bank.DTO.DTOs.ServiceBusDto;
using Core.BL.CQRS.Base;
using Core.BL.Services.BankAccounts.BankAccountService;
using Core.BL.Services.BankAccounts.BankAccountsValidation;
using Core.BL.Services.BankAccounts.CreditBankAccountService;
using Core.BL.Services.Firebase;
using Core.BL.SignalR;
using Core.DAL;
using Core.DAL.Models.BankAccounts;
using Core.DAL.Models.History;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Core.BL.CQRS.Commands.PutMoneyOnCreditBankAccount
{
    public class PutMoneyOnCreditBankAccountRequestHandler : IRequestHandler<MediatorRequest<PutMoneyOnCreditBankAccountRequest, PutMoneyOnCreditBankAccountResponse>, PutMoneyOnCreditBankAccountResponse>
    {
        private CoreDbContext _coreDbContext;
        private ICreditBankAccountService _creditBankAccountService;
        private IRequestClient<IsUserBlockedRequest> _requestClient;
        private IHubContext<CoreSignalRHub, ICoreHub> _hub;
        private IBankAccountService _bankAccountService;
        private readonly IFirebaseService _firebaseService;
        private readonly IRedisMessagingFacade _redisMessagingFacade;

        private BankAccountValidationIsClosed _handler;

        public PutMoneyOnCreditBankAccountRequestHandler(
            CoreDbContext coreDbContext,
            ICreditBankAccountService creditBankAccountService,
            IRequestClient<IsUserBlockedRequest> requestClient,
            IHubContext<CoreSignalRHub, ICoreHub> hub,
            IBankAccountService bankAccountService,
            IFirebaseService firebaseService,
            IRedisMessagingFacade redisMessagingFacade) 
        {
            _coreDbContext = coreDbContext;
            _creditBankAccountService = creditBankAccountService;
            _requestClient = requestClient;
            _hub = hub;
            _bankAccountService = bankAccountService;
            _firebaseService = firebaseService;
            _redisMessagingFacade = redisMessagingFacade;

            _handler = new();
            _handler.ConnectHandler(new NullHandler());
        }

        public async Task<PutMoneyOnCreditBankAccountResponse> Handle(MediatorRequest<PutMoneyOnCreditBankAccountRequest, PutMoneyOnCreditBankAccountResponse> request, CancellationToken cancellationToken)
        {
            var creditBankAccount = await _coreDbContext.CreditBankAccounts
                                    .FirstOrDefaultAsync(x => x.Id == request.Request.AccountId, cancellationToken: cancellationToken)
                                    ?? throw new KeyNotFoundException($"Card bank account with id {request.Request.AccountId} was not found");

            if (creditBankAccount.OwnerId != request.MetaData!.UserId
               && request.MetaData.UserRole != UserRole.Employee && !request.Request.IsTransfer)
            {
                throw new AccessViolationException($"ERROR");
            }

            var result = await _requestClient.GetResponse<IsUserBlockedCommand>(new IsUserBlockedRequest { UserId = request.MetaData.UserId });
            if (result.Message.IsBlocked)
            {
                throw new AccessViolationException($"You are blocked");
            }

            await _handler.Invoke(creditBankAccount);

            if (creditBankAccount.Debt <= 0)
            {
                throw new ArgumentException($"You cant pay your debt for this credit bank account");
            }

            request.Request.RequestDTO.Money = await _bankAccountService.ConvertMoney(
                request.Request.RequestDTO.CurrencyType,
                creditBankAccount.CurrencyType,
                request.Request.RequestDTO.Money);

            decimal previousDebt = creditBankAccount.Debt;

            creditBankAccount.Debt -= request.Request.RequestDTO.Money;
            creditBankAccount.ModifyDateTime = DateTime.UtcNow;

            if(creditBankAccount.Debt < 0)
            {
                var deltaMoney = request.Request.RequestDTO.Money - previousDebt;
                creditBankAccount.Debt = 0;
                creditBankAccount.Balance += deltaMoney;
            }

            _coreDbContext.Entry(creditBankAccount).State = EntityState.Modified;

            /*if(creditBankAccount.Debt > 0)
            {
                await _creditBankAccountService.RecalculateTariffMinimumPayment(creditBankAccount.TariffId, creditBankAccount.Debt);
            }*/

            var operationsHistory = new BankAccountOperationsHistory
            {
                BankAccountOperationType = BankAccountOperationType.LoanRepayment,
                OperatingMoney = request.Request.RequestDTO.Money,
                CurrentBalance = creditBankAccount.Debt,
                PreviousBalance = previousDebt,
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
                BankAccountOperationType = BankAccountOperationType.LoanRepayment,
                OperatingMoney = request.Request.RequestDTO.Money,
                CurrentBalance = creditBankAccount.Debt,
                PreviousBalance = previousDebt,
                OperationDateTime = DateTime.UtcNow,
                BankAccountOperationInitiator = BankAccountOperationInitiator.User,
                BankAccountOperationStatus = BankAccountOperationStatus.Success,

                BankAccountId = request.Request.AccountId,
                UserId = request.MetaData.UserId
            };

            string serialize = JsonSerializer.Serialize(response);

            await _hub.Clients.Group(UserRole.Employee.ToString()).OperationCreated(serialize);
            if (request.MetaData.UserRole != UserRole.Employee)
            {
                await _hub.Clients.Group(request.MetaData.UserId.ToString()).OperationCreated(serialize);
            }

            await _redisMessagingFacade.ProcessRedisMessage<RedisMessage>(request.MetaData.RedisMessageId, $"Payed loan of credit bank account with id {creditBankAccount.Id}", StatusCodes.Status200OK);

            await _firebaseService.SendFirebasePushMessage(request.MetaData, new()
            {
                UserNotification = new()
                {
                    Title = "Погашение кредита",
                    Body = $"Погашение кредита счёта {creditBankAccount.AccountName} на сумму {request.Request.RequestDTO.Money} {request.Request.RequestDTO.CurrencyType}"
                },
                EmployeesNotification = new()
                {
                    Title = "Погашение кредита",
                    Body = $"Пользователь с id {request.MetaData.UserId} погасил кредит счёта с id {creditBankAccount.Id} ({creditBankAccount.AccountName}) " +
                    $"на сумму {request.Request.RequestDTO.Money} {request.Request.RequestDTO.CurrencyType}"
                }
            });

            return new PutMoneyOnCreditBankAccountResponse();
        }
    }
}
