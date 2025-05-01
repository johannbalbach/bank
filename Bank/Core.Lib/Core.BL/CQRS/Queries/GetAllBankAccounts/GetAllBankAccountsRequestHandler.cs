using Bank.BL.Redis.Messages;
using Bank.BL.Redis.Patterns;
using Bank.BL.Values;
using Bank.DAL;
using Core.BL.CQRS.Base;
using Core.BL.Extensions;
using Core.DAL;
using Core.DAL.Models.BankAccounts;
using Core.DTO.DTOs.Requests.BankAccount;
using Core.DTO.DTOs.Responses.BankAccounts;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using X.PagedList.Extensions;

namespace Core.BL.CQRS.Queries.GetAllBankAccounts
{
    public class GetAllBankAccountsRequestHandler : IRequestHandler<MediatorRequest<AllBankAccountsQueryDTO, BankAccountsListDTO>, BankAccountsListDTO>
    {

        private readonly CoreDbContext _coreDbContext;
        private readonly IRedisMessagingFacade _redisMessagingFacade;

        public GetAllBankAccountsRequestHandler(CoreDbContext coreDbContext, IRedisMessagingFacade redisMessagingFacade)
        {
            _coreDbContext = coreDbContext;
            _redisMessagingFacade = redisMessagingFacade;
        }

        public async Task<BankAccountsListDTO> Handle(MediatorRequest<AllBankAccountsQueryDTO, BankAccountsListDTO> request, CancellationToken cancellationToken)
        {
            var bankAccounts = _coreDbContext.BankAccounts
                               .AsNoTracking()
                               .Include(x => (x as CardBankAccount).DebitCard)
                               .Include(x => (x as CreditBankAccount).CreditCard)
                               .OrderByDescending(x => x.CreateDateTime)
                               .AsSplitQuery()
                               .AsQueryable();

            if (!string.IsNullOrEmpty(request.Request.FullName))
            {
                bankAccounts = bankAccounts.Where(x => EF.Functions.ILike(x.Owner.FullName, "%" + request.Request.FullName + "%"));
            }

            if (request.Request.BankAccountType.HasValue)
            {
                bankAccounts = bankAccounts.Where(x => x.BankAccountType == request.Request.BankAccountType.Value);
            }

            if (request.Request.IsFrozen.HasValue)
            {
                bankAccounts = bankAccounts.Where(x => x.IsFrozen == request.Request.IsFrozen);
            }

            if (request.Request.IsClosed.HasValue)
            {
                bankAccounts = bankAccounts.Where(x => request.Request.IsClosed == true ? x.CloseDateTime.HasValue : !x.CloseDateTime.HasValue);
            }

            (var bankAccountsPagedList, var metadata) = await bankAccounts.ToPagedListAsync(request.Request.page, 1000);

            var cardBankAccounts = bankAccountsPagedList.OfType<CardBankAccount>().Select(x => x.CardBankAccountToDTO());
            var creditBankAccounts = bankAccountsPagedList.OfType<CreditBankAccount>().Select(x => x.CreditBankAccountToDTOShort());

            var resultList = cardBankAccounts.Concat(creditBankAccounts).OrderByDescending(x => x.BankAccount.CreateDateTime).ToList();

            await _redisMessagingFacade.ProcessRedisMessage<RedisMessage>(request.MetaData.RedisMessageId, "Get all bank accounts", StatusCodes.Status200OK);

            return new BankAccountsListDTO
            {
                BankAccounts = resultList,
                MetaData = metadata
            };
        }
    }
}
