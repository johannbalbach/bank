using Core.DAL.Models.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.BL.Services.BankAccounts.BankAccountsValidation
{
    public class NullHandler : BankAccountsValidationBasePipeline
    {
        public override async Task Invoke(BaseBankAccount account)
        {
            if(_next != null)
            {
                throw new Exception($"Null hanlder cant have next handler");
            }

            return;
        }
    }
}
