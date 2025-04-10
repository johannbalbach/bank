using Core.DAL;
using Core.DAL.Models.Currency;
using Microsoft.EntityFrameworkCore;

namespace Core.BL.Services.BankAccounts.BankAccountService
{
    public class BankAccountService : IBankAccountService
    {
        private readonly CoreDbContext _context;
        private const string _mainCurrency = "RUB";

        public BankAccountService(CoreDbContext context)
        {
            _context = context;
        }

        public async Task<decimal> ConvertMoney(string requestCurrencyName, string accountCurrencyName, decimal requestCurrencyAmount)
        {
            CurrencyType? requestCurrencyFromDb = null;
            CurrencyType? accountCurrencyFromDb = null;

            decimal amount = requestCurrencyAmount;
            bool check = requestCurrencyName == accountCurrencyName;

            if (requestCurrencyName != _mainCurrency && !check)
            {
                requestCurrencyFromDb = await _context.Currencies.FirstOrDefaultAsync(x => x.VchCode == requestCurrencyName)
                                        ?? throw new KeyNotFoundException($"Currency with name {requestCurrencyName} was not found in database");

                amount *= requestCurrencyFromDb.VunitRate;
            }

            if(accountCurrencyName != _mainCurrency && !check)
            {
                accountCurrencyFromDb = await _context.Currencies.FirstOrDefaultAsync(x => x.VchCode == accountCurrencyName)
                                        ?? throw new KeyNotFoundException($"Currency with name {accountCurrencyName} was not found in database");

                amount /= accountCurrencyFromDb.VunitRate;
            }

            return amount;
        }
    }
}
