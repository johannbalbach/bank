using Core.BL.CQRS.Base;
using Core.BL.Extensions;
using Core.DAL;
using Core.DAL.Models;
using Core.DAL.Models.BankAccounts;
using Core.DAL.Models.Cards;
using Core.DTO.DTOs.Responses.Aggregates;
using MediatR;
using Microsoft.EntityFrameworkCore;


namespace Core.BL.CQRS.Queries.GetUserBankAccounts
{
    public class GetUserBankAccountsRequestHandler : IRequestHandler<MediatorRequest<GetUserBankAccountsRequest, UserBankAccountsResponseDTO>, UserBankAccountsResponseDTO>
    {
        private readonly CoreDbContext _coreDbContext;

        public GetUserBankAccountsRequestHandler(CoreDbContext coreDbContext)
        {
            _coreDbContext = coreDbContext;
        }

        public async Task<UserBankAccountsResponseDTO> Handle(MediatorRequest<GetUserBankAccountsRequest, UserBankAccountsResponseDTO> request, CancellationToken cancellationToken)
        {
            UserBankAccountsResponseDTO response = new();

            var user = await _coreDbContext.Users
                        .AsNoTracking()
                        .Include(x => x.BankAccounts)
                            .ThenInclude(b => (b as CardBankAccount).DebitCard)
                        .Include(x => x.BankAccounts)
                            .ThenInclude(b => (b as CreditBankAccount).CreditCard)
                        .Include(x => x.BankAccounts)
                            .ThenInclude(b => (b as CreditBankAccount).Tariff)
                        .AsSplitQuery()
                        .FirstOrDefaultAsync(x => x.Id == request.Request.UserId, cancellationToken: cancellationToken)
                        ?? throw new KeyNotFoundException($"User with id {request.Request.UserId} was not found");

            response.CardBankAccounts = user.BankAccounts.OfType<CardBankAccount>().Select(x => x.CardBankAccountToDTO()).Where(x => !request.Request.AccountsIds.Contains(x.BankAccount.Id)).ToList();
            response.CreditBankAccounts = user.BankAccounts.OfType<CreditBankAccount>().Select(x => x.CreditBankAccountToDTO()).Where(x => !request.Request.AccountsIds.Contains(x.BankAccount.Id)).ToList();

            return response;
        }
    }
}
