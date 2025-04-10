using System.ComponentModel;

namespace Bank.DAL.Enums
{
    public enum BankAccountType
    {
        [Description("Карточный счёт")]
        Card,
        [Description("Кредитный счёт")]
        Credit,
        [Description("Мастер счёт")]
        Master
    }
}
