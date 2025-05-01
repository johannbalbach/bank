using Bank.BL.Redis.Messages;
using Bank.BL.Redis.Patterns;
using Core.BL.CQRS.Base;
using Core.BL.Extensions;
using Core.DAL;
using Core.DAL.Models.BankAccounts;
using Core.DTO.DTOs.Responses.Aggregates;
using Core.DTO.DTOs.Responses.BankAccounts;
using Core.DTO.DTOs.Responses.Cards;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Core.BL.CQRS.Queries.GetCreditBankAccountDetails
{
    public class GetCreditBankAccountDetailsRequestHandler : IRequestHandler<MediatorRequest<GetCreditBankAccountDetailsRequest, BankAccountCardDTO<CreditBankAccountFullResponseDTO, CreditCardResponseDTO>>,
        BankAccountCardDTO<CreditBankAccountFullResponseDTO, CreditCardResponseDTO>>
    {

        private CoreDbContext _coreDbContext { get; set; }
        private readonly IRedisMessagingFacade _redisMessagingFacade;

        public GetCreditBankAccountDetailsRequestHandler(CoreDbContext coreDbContext, IRedisMessagingFacade redisMessagingFacade)
        {
            _coreDbContext = coreDbContext;
            _redisMessagingFacade = redisMessagingFacade;
        }

        public async Task<BankAccountCardDTO<CreditBankAccountFullResponseDTO, CreditCardResponseDTO>> Handle(MediatorRequest<GetCreditBankAccountDetailsRequest, BankAccountCardDTO<CreditBankAccountFullResponseDTO, CreditCardResponseDTO>> request, CancellationToken cancellationToken)
        {
            var creditBankAccount = await _coreDbContext.CreditBankAccounts
                                    .AsNoTracking()
                                    .Include(x => x.CreditCard)
                                    .Include(x => x.Tariff)
                                    .AsSplitQuery()
                                    .FirstOrDefaultAsync(x => x.Id == request.Request.AccountId)
                                    ?? throw new KeyNotFoundException($"Credit bank account with id {request.Request.AccountId} was not found");

            if (creditBankAccount.OwnerId != request.MetaData!.UserId && request.MetaData.UserRole != Bank.DAL.Enums.UserRole.Employee)
            {
                throw new AccessViolationException($"ERROR");
            }

            await _redisMessagingFacade.ProcessRedisMessage<RedisMessage>(request.MetaData.RedisMessageId, "Details of card bank account", 200);

            return creditBankAccount.CreditBankAccountToDTOFull();
        }
    }
}
