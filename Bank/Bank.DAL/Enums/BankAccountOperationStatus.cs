using System.ComponentModel;

namespace Bank.DAL.Enums
{
    public enum BankAccountOperationStatus
    {
        [Description("Удача")]
        Success,
        [Description("Отказ")]
        Reject
    }
}
