using Core.DAL.Models.Base;

namespace Core.BL.Services.BankAccounts.BankAccountService
{
    public interface IBankAccountService
    {
        Task<decimal> ConvertMoney(string requestCurrencyName, string accountCurrencyName, decimal requestCurrencyAmount);
    }
}
