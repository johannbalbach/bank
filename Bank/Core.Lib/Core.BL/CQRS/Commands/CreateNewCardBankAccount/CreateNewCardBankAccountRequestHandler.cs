using Bank.BL.Other;
using Bank.BL.Redis.Messages;
using Bank.BL.Redis.Patterns;
using Bank.BL.Values;
using Bank.DAL.Enums;
using Bank.DTO.DTOs.ServiceBusDto;
using Core.BL.CQRS.Base;
using Core.DAL;
using Core.DAL.Models.BankAccounts;
using Core.DAL.Models.Cards;
using Core.DTO.DTOs.Requests.BankAccount;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Core.BL.CQRS.Commands.CreateNewCardBankAccount
{
    public class CreateNewCardBankAccountRequestHandler : IRequestHandler<MediatorRequest<DebitCardBankAccountCreateRequestDTO, CreateNewCardBankAccountResponse>, CreateNewCardBankAccountResponse>
    {
        private CoreDbContext _coreDbContext;
        private IRequestClient<IsUserBlockedRequest> _requestClient;
        private readonly IRedisMessagingFacade _redisMessagingFacade;

        public CreateNewCardBankAccountRequestHandler(CoreDbContext coreDbContext, IRequestClient<IsUserBlockedRequest> requestClient, IRedisMessagingFacade redisMessagingFacade)
        {
            _coreDbContext = coreDbContext;
            _requestClient = requestClient;
            _redisMessagingFacade = redisMessagingFacade;
        }

        public async Task<CreateNewCardBankAccountResponse> Handle(MediatorRequest<DebitCardBankAccountCreateRequestDTO, CreateNewCardBankAccountResponse> request, CancellationToken cancellationToken)
        {
            var owner = await _coreDbContext.Users
                        .Where(x => !x.DeleteDateTime.HasValue)
                        .FirstOrDefaultAsync(x => x.Id == request.MetaData!.UserId, cancellationToken: cancellationToken)
                        ?? throw new KeyNotFoundException($"User with id {request.MetaData!.UserId} was not found in database");

            var result = await _requestClient.GetResponse<IsUserBlockedCommand>(new IsUserBlockedRequest { UserId = request.MetaData.UserId });
            if (result.Message.IsBlocked)
            {
                throw new AccessViolationException($"You are blocked");
            }

            var currencyType = await _coreDbContext.Currencies.FirstOrDefaultAsync(x => x.VchCode == request.Request.CurrencyType, cancellationToken: cancellationToken)
                               ?? throw new KeyNotFoundException($"Currency {request.Request.CurrencyType} was not found");

            var cardBankAccount = new CardBankAccount
            {
                CurrencyType = request.Request.CurrencyType,
                AccountName = request.Request.AccountName,
                BankAccountType = BankAccountType.Card,
                Owner = owner,
                Currency = currencyType,
                CreateDateTime = DateTime.UtcNow,
                DebitCard = new()
                {
                    CardNumber = StringGenerator.GenerateRandomString(1, CoreConstants.CardNumberLength),
                    CardCategory = CardCategory.Black,
                    CardType = CardType.DebitCard,
                    CreateDateTime = DateTime.UtcNow,
                    Owner = owner
                }
            };

            await _coreDbContext.CardBankAccounts.AddAsync(cardBankAccount, cancellationToken);

            await _coreDbContext.SaveChangesAsync(cancellationToken);

            await _redisMessagingFacade.ProcessRedisMessage<RedisMessage>(request.MetaData.RedisMessageId, "Card bank account was created", StatusCodes.Status200OK);

            return new CreateNewCardBankAccountResponse
            {
                AccountId = cardBankAccount.Id
            };
        }
    }
}
