using Bank.BL.Redis;
using Bank.BL.Redis.Messages;
using Bank.BL.Redis.Patterns;
using Bank.DTO.DTOs.ServiceBusDto;
using Core.BL.CQRS.Base;
using Core.BL.Services.BankAccounts.BankAccountsValidation;
using Core.DAL;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Core.BL.CQRS.Commands.ChangeBankAccountNameCommand
{
    public class ChangeBankAccountNameRequestHandler : IRequestHandler<MediatorRequest<ChangeBankAccountNameRequest, ChangeBankAccountNameResponse>, ChangeBankAccountNameResponse>
    {
        private CoreDbContext _coreDbContext;
        private IRequestClient<IsUserBlockedRequest> _requestClient;
        private readonly IRedisMessagingFacade _redisMessagingFacade;

        private BankAccountValidationIsFrozen _handler;

        public ChangeBankAccountNameRequestHandler(CoreDbContext coreDbContext, IRequestClient<IsUserBlockedRequest> requestClient, IRedisMessagingFacade redisMessagingFacade)
        {
            _coreDbContext = coreDbContext;
            _requestClient = requestClient;
            _redisMessagingFacade = redisMessagingFacade;

            _handler = new();
            _handler.ConnectHandler(new BankAccountValidationIsClosed().ConnectHandler(new NullHandler()));
        }

        public async Task<ChangeBankAccountNameResponse> Handle(MediatorRequest<ChangeBankAccountNameRequest, ChangeBankAccountNameResponse> request, CancellationToken cancellationToken)
        {
            var bankAccount = await _coreDbContext.BankAccounts
                                .FirstOrDefaultAsync(x => x.Id == request.Request.AccountId, cancellationToken: cancellationToken) 
                                ?? throw new KeyNotFoundException($"Bank account with id {request.Request.AccountId} was not found");

            if(bankAccount.OwnerId != request.MetaData!.UserId)
            {
                throw new AccessViolationException("ERROR");
            }

            var result = await _requestClient.GetResponse<IsUserBlockedCommand>(new IsUserBlockedRequest { UserId = request.MetaData.UserId });
            if (result.Message.IsBlocked)
            {
                throw new AccessViolationException($"You are blocked");
            }

            await _handler.Invoke(bankAccount);

            bankAccount.AccountName = request.Request.AccountName;
            bankAccount.ModifyDateTime = DateTime.UtcNow;
            _coreDbContext.Entry(bankAccount).State = EntityState.Modified;

            await _coreDbContext.SaveChangesAsync(cancellationToken);

            await _redisMessagingFacade.ProcessRedisMessage<RedisMessage>(request.MetaData.RedisMessageId,
                $"Successfully changed account name ({bankAccount.AccountName}) for account with id {bankAccount.Id}", StatusCodes.Status200OK);

            return new ChangeBankAccountNameResponse();
        }
    }
}
