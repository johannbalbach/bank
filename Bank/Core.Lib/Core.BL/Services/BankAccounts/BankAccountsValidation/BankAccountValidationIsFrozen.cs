using Core.DAL.Models.Base;

namespace Core.BL.Services.BankAccounts.BankAccountsValidation
{
    public class BankAccountValidationIsFrozen : BankAccountsValidationBasePipeline
    {
        public override async Task Invoke(BaseBankAccount account)
        {
            if (_next == null) return;

            if (account.IsFrozen)
            {
                throw new AccessViolationException($"Bank account with id {account.Id} is frozen");
            }

            await _next.Invoke(account);
        }
    }
}
