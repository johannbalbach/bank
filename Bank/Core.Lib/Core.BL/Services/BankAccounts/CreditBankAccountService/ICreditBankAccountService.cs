namespace Core.BL.Services.BankAccounts.CreditBankAccountService
{
    public interface ICreditBankAccountService
    {
        Task RecalculateTariffMinimumPayment(Guid tariffId, decimal currentDebt);
    }
}
