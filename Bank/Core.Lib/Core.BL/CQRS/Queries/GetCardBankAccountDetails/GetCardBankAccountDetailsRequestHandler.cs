using Bank.BL.Redis.Messages;
using Bank.BL.Redis.Patterns;
using Core.BL.CQRS.Base;
using Core.BL.Extensions;
using Core.DAL;
using Core.DTO.DTOs.Responses.Aggregates;
using Core.DTO.DTOs.Responses.BankAccounts;
using Core.DTO.DTOs.Responses.Cards;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Core.BL.CQRS.Queries.GetCardBankAccountDetails
{
    public class GetCardBankAccountDetailsRequestHandler : IRequestHandler<MediatorRequest<GetCardBankAccountDetailsRequest, BankAccountCardDTO<BankAccountResponseDTO, CardResponseDTO>>,
        BankAccountCardDTO<BankAccountResponseDTO, CardResponseDTO>>
    {

        private readonly CoreDbContext _coreDbContext;
        private readonly IRedisMessagingFacade _redisMessagingFacade;

        public GetCardBankAccountDetailsRequestHandler(CoreDbContext coreDbContext, IRedisMessagingFacade redisMessagingFacade)
        {
            _coreDbContext = coreDbContext;
            _redisMessagingFacade = redisMessagingFacade;
        }

        public async Task<BankAccountCardDTO<BankAccountResponseDTO, CardResponseDTO>> Handle(MediatorRequest<GetCardBankAccountDetailsRequest, BankAccountCardDTO<BankAccountResponseDTO, CardResponseDTO>> request,
            CancellationToken cancellationToken)
        {
            var cardBankAccount = await _coreDbContext.CardBankAccounts
                                    .AsNoTracking()
                                    .Include(x => x.DebitCard)
                                    .AsSplitQuery()
                                    .FirstOrDefaultAsync(x => x.Id == request.Request.AccountId);

            if(cardBankAccount == null)
            {
                throw new KeyNotFoundException($"Card bank account with id {cardBankAccount} was not found");
            }

            if(cardBankAccount.OwnerId != request.MetaData!.UserId && request.MetaData.UserRole != Bank.DAL.Enums.UserRole.Employee)
            {
                throw new AccessViolationException($"ERROR");
            }

            await _redisMessagingFacade.ProcessRedisMessage<RedisMessage>(request.MetaData.RedisMessageId, "Details of card bank account", 200);

            return cardBankAccount.CardBankAccountToDTO();
        }
    }
}
