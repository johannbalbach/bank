using System.ComponentModel;

namespace Bank.DAL.Enums
{
    public enum BankAccountOperationType
    {
        [Description("Пополнение счёта")]
        Replenishment,
        [Description("Снятие денег со счёта")]
        Withdrawal,
        [Description("Выплата кредита")]
        LoanRepayment
    }
}
