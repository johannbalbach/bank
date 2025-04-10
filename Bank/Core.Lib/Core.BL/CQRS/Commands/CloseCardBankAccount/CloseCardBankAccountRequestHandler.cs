using Bank.DTO.DTOs.ServiceBusDto;
using Core.BL.CQRS.Base;
using Core.BL.Services.BankAccounts.BankAccountsValidation;
using Core.DAL;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Core.BL.CQRS.Commands.CloseCardBankAccount
{
    public class CloseCardBankAccountRequestHandler : IRequestHandler<MediatorRequest<CloseCardBankAccountRequest, CloseCardBankAccountResponse>, CloseCardBankAccountResponse>
    {
        private CoreDbContext _coreDbContext;
        private IRequestClient<IsUserBlockedRequest> _requestClient;

        private BankAccountValidationIsFrozen _handler;

        public CloseCardBankAccountRequestHandler(CoreDbContext coreDbContext, IRequestClient<IsUserBlockedRequest> requestClient)
        {
            _coreDbContext = coreDbContext;
            _requestClient = requestClient;

            _handler = new();
            _handler.ConnectHandler(new BankAccountValidationIsClosed().ConnectHandler(new NullHandler()));
        }

        public async Task<CloseCardBankAccountResponse> Handle(MediatorRequest<CloseCardBankAccountRequest, CloseCardBankAccountResponse> request, CancellationToken cancellationToken)
        {
            var cardBankAccount = await _coreDbContext.CardBankAccounts
                                  .FirstOrDefaultAsync(x => x.Id == request.Request.AccountId, cancellationToken: cancellationToken)
                                  ?? throw new KeyNotFoundException($"Bank account with id {request.Request.AccountId} was not found");

            if(cardBankAccount.OwnerId != request.MetaData!.UserId)
            {
                throw new AccessViolationException("ERROR");
            }

            var result = await _requestClient.GetResponse<IsUserBlockedCommand>(new IsUserBlockedRequest { UserId = request.MetaData.UserId });
            if (result.Message.IsBlocked)
            {
                throw new AccessViolationException($"You are blocked");
            }

            await _handler.Invoke(cardBankAccount);

            if(cardBankAccount.Balance > 0)
            {
                throw new InvalidDataException($"Bank account has money on it");
            }

            var now = DateTime.UtcNow;

            cardBankAccount.CloseDateTime = now;
            cardBankAccount.ModifyDateTime = now;
            _coreDbContext.Entry(cardBankAccount).State = EntityState.Modified;

            await _coreDbContext.SaveChangesAsync(cancellationToken);

            return new CloseCardBankAccountResponse();
        }
    }
}
