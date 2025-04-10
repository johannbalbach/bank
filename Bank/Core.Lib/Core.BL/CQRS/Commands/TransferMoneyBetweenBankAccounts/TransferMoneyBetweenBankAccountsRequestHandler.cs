using Bank.DAL.Enums;
using Core.BL.CQRS.Base;
using Core.BL.CQRS.Commands.PutMoneyOnBankAccount;
using Core.BL.CQRS.Commands.PutMoneyOnCreditBankAccount;
using Core.BL.CQRS.Commands.WithdrawMoneyFromCardBankAccount;
using Core.BL.CQRS.Commands.WithdrawMoneyFromCreditBankAccount;
using Core.DAL;
using Core.DAL.Models.Base;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Core.BL.CQRS.Commands.TransferMoneyBetweenBankAccounts
{
    public class TransferMoneyBetweenBankAccountsRequestHandler : 
        IRequestHandler<MediatorRequest<TransferMoneyBetweenBankAccountsRequest, TransferMoneyBetweenBankAccountsResponse>, TransferMoneyBetweenBankAccountsResponse>
    {
        private readonly CoreDbContext _coreDbContext;
        private readonly IMediator _mediator;

        public TransferMoneyBetweenBankAccountsRequestHandler(CoreDbContext coreDbContext, IMediator mediator)
        {
            _coreDbContext = coreDbContext;
            _mediator = mediator;
        }

        public async Task<TransferMoneyBetweenBankAccountsResponse> Handle(MediatorRequest<TransferMoneyBetweenBankAccountsRequest, TransferMoneyBetweenBankAccountsResponse> request, CancellationToken cancellationToken)
        {
            List<Guid> idsList = [request.Request.FromAccountId, request.Request.ToAccountId];
            var bankAccounts = await _coreDbContext.BankAccounts.Where(x => idsList.Contains(x.Id)).ToListAsync(cancellationToken: cancellationToken);

            if(bankAccounts.Count < 2 || bankAccounts.Any(x => x.BankAccountType == BankAccountType.Master))
            {
                throw new KeyNotFoundException("Invalid operation");
            }

            var fromBankAccount = bankAccounts.First(x => x.Id == request.Request.FromAccountId);
            var toBankAccount = bankAccounts.First(x => x.Id == request.Request.ToAccountId);

            if(fromBankAccount.OwnerId != request.MetaData!.UserId)
            {
                throw new AccessViolationException("ERROR");
            }

            await ProcessFromAccount(fromBankAccount, request);
            await ProcessToAccount(toBankAccount, request);

            return new TransferMoneyBetweenBankAccountsResponse();
        }

        private async Task ProcessFromAccount(BaseBankAccount bankAccount, MediatorRequest<TransferMoneyBetweenBankAccountsRequest, TransferMoneyBetweenBankAccountsResponse> request)
        {
            var bankAccountType = bankAccount.BankAccountType;
            if (bankAccountType == BankAccountType.Card)
            {
                await _mediator.Send(new MediatorRequest<WithdrawMoneyFromCardBankAccountRequest, WithdrawMoneyFromCardBankAccountResponse>
                {
                    MetaData = request.MetaData,
                    Request = new()
                    {
                        AccountId = bankAccount.Id,
                        RequestDTO = new()
                        {
                            CurrencyType = request.Request.CurrencyType,
                            Money = request.Request.Amount
                        },
                        IsTransfer = true
                    }
                });
            }
            else if(bankAccountType == BankAccountType.Credit)
            {
                await _mediator.Send(new MediatorRequest<WithdrawMoneyFromCreditBankAccountRequest, WithdrawMoneyFromCreditBankAccountResponse>
                {
                    MetaData = request.MetaData,
                    Request = new()
                    {
                        AccountId = bankAccount.Id,
                        RequestDTO = new()
                        {
                            CurrencyType = request.Request.CurrencyType,
                            Money = request.Request.Amount
                        },
                        IsTransfer = true
                    }
                });
            }
        }

        private async Task ProcessToAccount(BaseBankAccount bankAccount, MediatorRequest<TransferMoneyBetweenBankAccountsRequest, TransferMoneyBetweenBankAccountsResponse> request)
        {
            var bankAccountType = bankAccount.BankAccountType;
            if (bankAccountType == BankAccountType.Card)
            {
                await _mediator.Send(new MediatorRequest<PutMoneyOnCardBankAccountRequest, PutMoneyOnCardBankAccountResponse>
                {
                    MetaData = request.MetaData,
                    Request = new()
                    {
                        AccountId = bankAccount.Id,
                        RequestDTO = new()
                        {
                            CurrencyType = request.Request.CurrencyType,
                            Money = request.Request.Amount
                        },
                        IsTransfer = true
                    }
                });
            }
            else if (bankAccountType == BankAccountType.Credit)
            {
                await _mediator.Send(new MediatorRequest<PutMoneyOnCreditBankAccountRequest, PutMoneyOnCreditBankAccountResponse>
                {
                    MetaData = request.MetaData,
                    Request = new()
                    {
                        AccountId = bankAccount.Id,
                        RequestDTO = new()
                        {
                            CurrencyType = request.Request.CurrencyType,
                            Money = request.Request.Amount
                        },
                        IsTransfer = true
                    }
                });
            }
        }
    }
}
