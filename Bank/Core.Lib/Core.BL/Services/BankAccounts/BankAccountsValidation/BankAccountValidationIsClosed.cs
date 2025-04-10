using Core.DAL.Models.Base;

namespace Core.BL.Services.BankAccounts.BankAccountsValidation
{
    public class BankAccountValidationIsClosed : BankAccountsValidationBasePipeline
    {
        public override async Task Invoke(BaseBankAccount account)
        {
            if (_next == null) return;

            if (account.CloseDateTime.HasValue)
            {
                throw new AccessViolationException($"Bank account with id {account.Id} is closed");
            }

            await _next.Invoke(account);
        }
    }
}
