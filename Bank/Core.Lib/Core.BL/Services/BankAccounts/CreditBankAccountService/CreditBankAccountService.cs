using Bank.BL.Options;
using Core.BL.Filters;
using Core.DAL;
using Core.DAL.Models.BankAccounts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Core.BL.Services.BankAccounts.CreditBankAccountService
{
    public class CreditBankAccountService : ICreditBankAccountService
    {
        private readonly CoreDbContext _context;
        private readonly TariffOptionsModel _model;

        public CreditBankAccountService(CoreDbContext context, IOptions<TariffOptionsModel> model)
        {
            _context = context;
            _model = model.Value;
        }

        public async Task RecalculateTariffMinimumPayment(Guid tariffId, decimal currentDebt)
        {
            var tariff = await _context.CreditTariffs
                        .Where(x => !x.DeleteDateTime.HasValue)
                        .FirstOrDefaultAsync(x => x.Id == tariffId)
                        ?? throw new Exception($"Tariff with id {tariffId} was not found");

            tariff.MinimumPayment = Math.Round(currentDebt * (decimal)(_model.MinimumPaymentPercentOfDebt * 1.0 / 100.0), 2);
            _context.Entry(tariff).State = EntityState.Modified;

            await _context.SaveChangesAsync();
        }
    }
}
