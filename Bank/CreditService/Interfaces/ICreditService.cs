using Bank.DAL.Enums;
using CreditService.Dtos;

namespace CreditService.Interfaces
{
    public interface ICreditService
    {
        Task<CreditAccountDto> RequestCredit(CreditRequestDto request, Guid UserId);
        Task<CreditBalanceLeftDto> DepositCredit(Guid CreditId, MoneyOperationRequestDTO amount, Guid UserId);
        Task<CreditBalanceLeftDto> WithdrawCredit(Guid CreditId, MoneyOperationRequestDTO amount, Guid UserId);
        Task<CreditAccountDetailsDto> GetCreditDetails(Guid CreditId, Guid UserId, UserRole role);
        Task<CreditAccountCloseDto> CloseCredit(Guid CreditId, Guid UserId);
        Task<List<CreditOperationHistoryDto>> GetOverduePayments(Guid CreditId, Guid UserId, bool addSuccessPayments);
        Task<TariffDto> CreateCreditTariff(TariffCreateDto tariffCreateDto, Guid UserId);
        Task<List<TariffDto>> GetAllTariffs(Guid UserId);
        Task<List<CreditAccountDto>> GetUserCredits(Guid UserId, Guid ThisUserId);
        Task<CreditRatingDto> GetCreditRating(Guid UserId, Guid ThisUserId, UserRole ThisUserRole);
    }
}
