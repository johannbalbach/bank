using Core.DAL.Models.Base;

namespace Core.BL.Services.BankAccounts.BankAccountsValidation
{
    public abstract class BankAccountsValidationBasePipeline
    {
        protected BankAccountsValidationBasePipeline? _next;

        public BankAccountsValidationBasePipeline ConnectHandler(BankAccountsValidationBasePipeline? next)
        {
            _next = next;
            return this;
        }

        public abstract Task Invoke(BaseBankAccount account);
    }
}
